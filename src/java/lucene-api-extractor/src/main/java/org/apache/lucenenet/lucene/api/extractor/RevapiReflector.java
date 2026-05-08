/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

package org.apache.lucenenet.lucene.api.extractor;

import org.revapi.AnalysisContext;
import org.revapi.API;
import org.revapi.Archive;
import org.revapi.Revapi;
import org.revapi.TreeFilter;
import org.revapi.base.FileArchive;
import org.revapi.java.JavaApiAnalyzer;
import org.revapi.java.JavaArchiveAnalyzer;

import javax.lang.model.element.AnnotationMirror;
import javax.lang.model.element.ElementKind;
import javax.lang.model.element.ExecutableElement;
import javax.lang.model.element.Modifier;
import javax.lang.model.element.NestingKind;
import javax.lang.model.element.TypeElement;
import javax.lang.model.element.TypeParameterElement;
import javax.lang.model.element.VariableElement;
import javax.lang.model.type.ArrayType;
import javax.lang.model.type.DeclaredType;
import javax.lang.model.type.IntersectionType;
import javax.lang.model.type.TypeKind;
import javax.lang.model.type.TypeMirror;
import javax.lang.model.type.TypeVariable;
import javax.lang.model.type.WildcardType;
import javax.lang.model.util.Elements;
import javax.lang.model.util.Types;
import java.util.ArrayList;
import java.util.Enumeration;
import java.util.LinkedHashSet;
import java.util.List;
import java.util.Set;
import java.util.jar.JarEntry;
import java.util.jar.JarFile;

/**
 * Builds {@link TypeMetadata} for each public/protected type in a JAR by driving Revapi's
 * Java analyzer.
 * <p>
 * Type discovery walks the JAR entries directly (so package-private outer classes still
 * surface their public nested types, matching {@link JarReflector}'s output). Reflection-
 * style introspection then runs against javac's compile-time element model — unresolvable
 * transitive references become symbolic error types rather than {@code NoClassDefFoundError},
 * which is the original reason for switching off {@code java.lang.reflect}.
 */
public class RevapiReflector {

    private static final String EMPTY_CONFIG = "{}";

    /**
     * Bundles javac's {@link Elements} / {@link Types} utilities and the
     * {@code stableParameterNames} flag so they can be threaded through the reflector
     * with one argument.
     */
    record Env(Elements elements, Types types, boolean stableParameterNames) {}

    public static List<LibraryResult> reflectOverJars(ExtractContext context) throws Exception {
        var libraries = context.getLibraries();
        var dependencies = context.getDependencies();
        var results = new ArrayList<LibraryResult>(libraries.length);

        for (MavenCoordinates library : libraries) {
            // Cross-reference resolution: every other primary jar plus all user-provided
            // dependency jars become supplementary archives so javac can complete symbols.
            var supplementary = new ArrayList<Archive>();
            for (var other : libraries) {
                if (!other.equals(library)) {
                    supplementary.add(new FileArchive(other.getFullJarPath(context)));
                }
            }
            for (var dep : dependencies) {
                supplementary.add(new FileArchive(dep.getFullJarPath(context)));
            }
            var types = reflectOverJar(context, library, supplementary);
            results.add(new LibraryResult(library, types));
        }

        results.sort(LibraryResult::compareTo);
        return results;
    }

    static List<TypeMetadata> reflectOverJar(ExtractContext context,
                                             MavenCoordinates library,
                                             List<Archive> supplementary) throws Exception {
        System.err.println("Reflecting over jar: " + library.getJarName());

        var primaryFile = library.getFullJarPath(context);
        var primary = new FileArchive(primaryFile);

        var api = API.builder()
                .of(primary)
                .supportedBy(supplementary)
                .build();

        var revapi = Revapi.builder().withAnalyzers(JavaApiAnalyzer.class).build();
        var analysisContext = AnalysisContext.builder(revapi)
                .withOldAPI(api)
                .withNewAPI(api)
                .withConfigurationFromJSON(EMPTY_CONFIG)
                .build();

        try (var apiAnalyzer = new JavaApiAnalyzer()) {
            apiAnalyzer.initialize(analysisContext);
            JavaArchiveAnalyzer archiveAnalyzer = apiAnalyzer.getArchiveAnalyzer(api);
            // analyze() returns a lazy forest; traversing it (via getRoots()) drives the
            // actual javac compilation and populates the probing environment so that
            // getElementUtils() becomes available afterward.
            var forest = archiveAnalyzer.analyze(TreeFilter.matchAndDescend());
            forest.getRoots(); // force compilation
            var probing = archiveAnalyzer.getProbingEnvironment();
            var env = new Env(probing.getElementUtils(), probing.getTypeUtils(),
                    context.isStableParameterNames());

            var types = new ArrayList<TypeMetadata>();
            for (String canonicalName : enumerateTypeNames(primaryFile)) {
                TypeElement typeElement;
                try {
                    typeElement = env.elements().getTypeElement(canonicalName);
                } catch (RuntimeException e) {
                    System.err.println("Failed to resolve type " + canonicalName + ": " + e.getMessage());
                    continue;
                }
                if (typeElement == null) {
                    if (context.isStrict()) {
                        throw new IllegalStateException("Failed to resolve type: " + canonicalName);
                    }
                    System.err.println("Failed to resolve type: " + canonicalName);
                    continue;
                }
                // API-visible types only: public or protected.
                if (!typeElement.getModifiers().contains(Modifier.PUBLIC)
                        && !typeElement.getModifiers().contains(Modifier.PROTECTED)) {
                    continue;
                }
                types.add(buildTypeMetadata(typeElement, env));
            }

            types.sort(TypeMetadata::compareTo);
            return types;
        }
    }

    /**
     * Enumerates every {@code .class} entry in the jar and returns its canonical
     * (source-form) name — i.e. with {@code $} replaced by {@code .} — suitable for
     * {@link Elements#getTypeElement(CharSequence)}.
     */
    /**
     * Returns true if any segment of the binary name (split on {@code $}) is purely
     * numeric — the marker for anonymous and synthetic classes (e.g. {@code Outer$1},
     * {@code Outer$Inner$2}). Such classes have no canonical name and cannot be
     * resolved via {@link Elements#getTypeElement(CharSequence)}.
     */
    static boolean isAnonymousClassBinaryName(String binaryName) {
        for (String part : binaryName.split("\\$")) {
            if (!part.isEmpty() && part.chars().allMatch(Character::isDigit)) {
                return true;
            }
        }
        return false;
    }

    private static List<String> enumerateTypeNames(java.io.File jarPath) throws Exception {
        var names = new ArrayList<String>();
        try (JarFile jarFile = new JarFile(jarPath)) {
            Enumeration<JarEntry> entries = jarFile.entries();
            while (entries.hasMoreElements()) {
                JarEntry entry = entries.nextElement();
                String entryName = entry.getName();
                if (!entryName.endsWith(".class")) {
                    continue;
                }
                if (entryName.endsWith("package-info.class") || entryName.endsWith("module-info.class")) {
                    continue;
                }
                String binaryName = entryName.substring(0, entryName.length() - ".class".length())
                        .replace('/', '.');
                // Skip anonymous classes (e.g. Outer$1, Outer$Inner$2) — they have no
                // canonical name and cannot be retrieved via Elements.getTypeElement.
                // The reflection-based extractor filters them out implicitly because
                // they're never public.
                if (isAnonymousClassBinaryName(binaryName)) {
                    continue;
                }
                // javac's getTypeElement wants the canonical (source) name, so $ → .
                names.add(binaryName.replace('$', '.'));
            }
        }
        return names;
    }

    static TypeMetadata buildTypeMetadata(TypeElement type, Env env) {
        var enclosingTypeName = enclosingTypeName(type, env);
        var packageName = packageNameOf(type, env);
        var simpleName = type.getSimpleName().toString();
        var fullName = binaryName(type, env);
        var kind = kindOf(type);

        var modifiers = sorted(getModifiers(type.getModifiers(), type));
        var typeParameters = getTypeParameterStrings(type.getTypeParameters(), env);

        TypeMirror superclass = type.getSuperclass();
        String baseType;
        String genericBaseType;
        if (superclass == null || superclass.getKind() == TypeKind.NONE) {
            baseType = null;
            genericBaseType = null;
        } else {
            baseType = typeNameOf(superclass, /*generic*/ false, env);
            genericBaseType = typeNameOf(superclass, /*generic*/ true, env);
        }

        var interfaces = new ArrayList<String>();
        var genericInterfaces = new ArrayList<String>();
        for (var iface : type.getInterfaces()) {
            interfaces.add(typeNameOf(iface, false, env));
            genericInterfaces.add(typeNameOf(iface, true, env));
        }
        interfaces.sort(String::compareTo);
        genericInterfaces.sort(String::compareTo);

        var annotations = getAnnotations(type.getAnnotationMirrors(), env);

        return new TypeMetadata(
                packageName,
                kind,
                simpleName,
                fullName,
                enclosingTypeName,
                baseType,
                genericBaseType,
                interfaces,
                genericInterfaces,
                modifiers,
                typeParameters,
                annotations,
                extractConstructors(type, env),
                extractMethods(type, env),
                extractFields(type, env)
        );
    }

    static String kindOf(TypeElement type) {
        return switch (type.getKind()) {
            case ANNOTATION_TYPE -> "annotation";
            case ENUM -> "enum";
            case RECORD -> "record";
            case INTERFACE -> "interface";
            default -> "class";
        };
    }

    static List<ConstructorMetadata> extractConstructors(TypeElement type, Env env) {
        var result = new ArrayList<ConstructorMetadata>();
        // Non-static inner classes have a synthetic enclosing-instance parameter (`this$0`)
        // that reflection surfaces as a real first parameter on every constructor.
        // javac's element model hides it; reintroduce it so the JSON matches reflection.
        boolean injectEnclosingParam = type.getNestingKind() == NestingKind.MEMBER
                && !type.getModifiers().contains(Modifier.STATIC);
        for (var member : type.getEnclosedElements()) {
            if (member.getKind() != ElementKind.CONSTRUCTOR) {
                continue;
            }
            var ctor = (ExecutableElement) member;
            if (!isApiVisible(ctor.getModifiers())) {
                continue;
            }
            var ctorMods = sorted(getModifiers(ctor.getModifiers(), null));
            // Reflection's Modifier.toString() returns "transient" for the ACC_VARARGS bit
            // (0x0080), since that bit shares its value with ACC_TRANSIENT on fields.
            // Replicate that quirk so JSON matches the reflection-based baseline.
            if (ctor.isVarArgs()) {
                ctorMods.add("transient");
                ctorMods.sort(String::compareTo);
            }
            var params = buildParameters(ctor, env);
            if (injectEnclosingParam) {
                var encl = type.getEnclosingElement();
                String enclName = (encl instanceof TypeElement enclType)
                        ? env.elements().getBinaryName(enclType).toString()
                        : "";
                // The synthetic this$0 isn't visible to javac's element model. In stable
                // mode we name it arg0 (matching reflection's no-parameters fallback) and
                // shift downstream params to arg1, arg2, .... When using real names we
                // emit the JVM-conventional this$0 and leave the others as javac gave them.
                var injected = new ArrayList<ParameterMetadata>(params.size() + 1);
                String synthName = env.stableParameterNames() ? "arg0" : "this$0";
                injected.add(new ParameterMetadata(synthName, enclName, enclName, List.of()));
                if (env.stableParameterNames()) {
                    for (int i = 0; i < params.size(); i++) {
                        var p = params.get(i);
                        injected.add(new ParameterMetadata("arg" + (i + 1), p.type(), p.genericType(), p.annotations()));
                    }
                } else {
                    injected.addAll(params);
                }
                params = injected;
            }
            result.add(new ConstructorMetadata(
                    params,
                    ctorMods,
                    getThrowsTypes(ctor, env),
                    getAnnotations(ctor.getAnnotationMirrors(), env),
                    ctor.isVarArgs()
            ));
        }
        result.sort(ConstructorMetadata::compareTo);
        return result;
    }

    static List<MethodMetadata> extractMethods(TypeElement type, Env env) {
        var result = new ArrayList<MethodMetadata>();
        for (var member : type.getEnclosedElements()) {
            if (member.getKind() != ElementKind.METHOD) {
                continue;
            }
            var method = (ExecutableElement) member;
            if (!isApiVisible(method.getModifiers())) {
                continue;
            }
            var returnTypeMirror = method.getReturnType();
            var methodMods = sorted(getModifiers(method.getModifiers(), null));
            if (method.isVarArgs()) {
                methodMods.add("transient");
                methodMods.sort(String::compareTo);
            }
            result.add(new MethodMetadata(
                    method.getSimpleName().toString(),
                    typeNameOf(returnTypeMirror, false, env),
                    typeNameOf(returnTypeMirror, true, env),
                    buildParameters(method, env),
                    methodMods,
                    getMethodTypeParameterNames(method.getTypeParameters(), env),
                    getThrowsTypes(method, env),
                    getAnnotations(method.getAnnotationMirrors(), env),
                    method.isVarArgs()
            ));
        }
        result.sort(MethodMetadata::compareTo);
        return result;
    }

    static List<FieldMetadata> extractFields(TypeElement type, Env env) {
        var result = new ArrayList<FieldMetadata>();
        for (var member : type.getEnclosedElements()) {
            if (member.getKind() != ElementKind.FIELD && member.getKind() != ElementKind.ENUM_CONSTANT) {
                continue;
            }
            var field = (VariableElement) member;
            if (!isApiVisible(field.getModifiers())) {
                continue;
            }
            var fieldType = field.asType();
            result.add(new FieldMetadata(
                    field.getSimpleName().toString(),
                    typeNameOf(fieldType, false, env),
                    typeNameOf(fieldType, true, env),
                    sorted(getModifiers(field.getModifiers(), null)),
                    getAnnotations(field.getAnnotationMirrors(), env),
                    field.getModifiers().contains(Modifier.STATIC)
            ));
        }
        result.sort(FieldMetadata::compareTo);
        return result;
    }

    private static List<ParameterMetadata> buildParameters(ExecutableElement executable, Env env) {
        var params = executable.getParameters();
        var result = new ArrayList<ParameterMetadata>(params.size());
        for (int i = 0; i < params.size(); i++) {
            var p = params.get(i);
            // In stable mode (used by `hash`), emit arg{i} so the digest doesn't shift
            // when a jar is rebuilt with or without -parameters / debug info. Otherwise
            // use whatever name javac surfaced (real source name when available, else
            // its own arg{i} fallback).
            String name = env.stableParameterNames() ? "arg" + i : p.getSimpleName().toString();
            result.add(new ParameterMetadata(
                    name,
                    typeNameOf(p.asType(), false, env),
                    typeNameOf(p.asType(), true, env),
                    getAnnotations(p.getAnnotationMirrors(), env)
            ));
        }
        return result;
    }

    private static List<String> getThrowsTypes(ExecutableElement executable, Env env) {
        var result = new ArrayList<String>();
        for (var t : executable.getThrownTypes()) {
            result.add(typeNameOf(t, true, env));
        }
        result.sort(String::compareTo);
        return result;
    }

    private static List<String> getMethodTypeParameterNames(List<? extends TypeParameterElement> typeParameters,
                                                            Env env) {
        var result = new ArrayList<String>(typeParameters.size());
        for (var tp : typeParameters) {
            result.add(formatTypeParameter(tp, env));
        }
        return result;
    }

    private static List<String> getTypeParameterStrings(List<? extends TypeParameterElement> typeParameters,
                                                         Env env) {
        var result = new ArrayList<String>(typeParameters.size());
        for (var tp : typeParameters) {
            result.add(formatTypeParameter(tp, env));
        }
        return result;
    }

    /**
     * Renders a type parameter the same way reflection's
     * {@link java.lang.reflect.TypeVariable#getName()} + bounds rendering does:
     * just the name when the only bound is {@code java.lang.Object}, otherwise
     * {@code "Name extends Bound1 & Bound2"}.
     */
    private static String formatTypeParameter(TypeParameterElement tp, Env env) {
        var name = tp.getSimpleName().toString();
        var bounds = tp.getBounds();
        if (bounds.isEmpty()) {
            return name;
        }
        if (bounds.size() == 1
                && "java.lang.Object".equals(typeNameOf(bounds.get(0), false, env))) {
            return name;
        }
        var sb = new StringBuilder(name).append(" extends ");
        for (int i = 0; i < bounds.size(); i++) {
            if (i > 0) {
                sb.append(" & ");
            }
            sb.append(typeNameOf(bounds.get(i), true, env));
        }
        return sb.toString();
    }

    private static List<AnnotationMetadata> getAnnotations(List<? extends AnnotationMirror> annotationMirrors,
                                                            Env env) {
        var result = new ArrayList<AnnotationMetadata>(annotationMirrors.size());
        for (var am : annotationMirrors) {
            result.add(new AnnotationMetadata(typeNameOf(am.getAnnotationType(), false, env)));
        }
        result.sort(AnnotationMetadata::compareTo);
        return result;
    }

    static List<String> getModifiers(Set<Modifier> modSet, TypeElement typeContext) {
        var modifiers = new ArrayList<String>();
        if (modSet.contains(Modifier.PUBLIC)) {
            modifiers.add("public");
        }
        if (modSet.contains(Modifier.PROTECTED)) {
            modifiers.add("protected");
        }
        if (modSet.contains(Modifier.PRIVATE)) {
            modifiers.add("private");
        }
        // For types, suppress redundant "abstract" on interfaces/annotations.
        // For members, "abstract" is always meaningful.
        boolean suppressAbstract = typeContext != null
                && (typeContext.getKind() == ElementKind.INTERFACE
                || typeContext.getKind() == ElementKind.ANNOTATION_TYPE);
        if (modSet.contains(Modifier.ABSTRACT) && !suppressAbstract) {
            modifiers.add("abstract");
        }
        if (modSet.contains(Modifier.FINAL)) {
            modifiers.add("final");
        }
        if (modSet.contains(Modifier.STATIC)) {
            modifiers.add("static");
        }
        if (modSet.contains(Modifier.SYNCHRONIZED)) {
            modifiers.add("synchronized");
        }
        if (modSet.contains(Modifier.NATIVE)) {
            modifiers.add("native");
        }
        if (modSet.contains(Modifier.STRICTFP)) {
            modifiers.add("strictfp");
        }
        if (modSet.contains(Modifier.TRANSIENT)) {
            modifiers.add("transient");
        }
        if (modSet.contains(Modifier.VOLATILE)) {
            modifiers.add("volatile");
        }
        return modifiers;
    }

    private static boolean isApiVisible(Set<Modifier> modifiers) {
        return modifiers.contains(Modifier.PUBLIC) || modifiers.contains(Modifier.PROTECTED);
    }

    private static List<String> sorted(List<String> list) {
        list.sort(String::compareTo);
        return list;
    }

    private static String packageNameOf(TypeElement type, Env env) {
        var pkg = env.elements().getPackageOf(type);
        return pkg == null ? "" : pkg.getQualifiedName().toString();
    }

    private static String binaryName(TypeElement type, Env env) {
        return env.elements().getBinaryName(type).toString();
    }

    private static String enclosingTypeName(TypeElement type, Env env) {
        if (type.getNestingKind() == NestingKind.TOP_LEVEL) {
            return null;
        }
        var encl = type.getEnclosingElement();
        if (encl instanceof TypeElement enclType) {
            return env.elements().getBinaryName(enclType).toString();
        }
        return null;
    }

    /**
     * Renders a {@link TypeMirror} as a string in the same format as
     * {@link java.lang.reflect.Type#getTypeName()}.
     * <p>
     * When {@code generic} is false, type arguments are dropped and any type variable
     * is replaced by its erasure — matching {@code Class.getTypeName()} on a raw
     * {@code Class<?>} and reflection's view of erased parameter types. When
     * {@code generic} is true we keep type arguments and emit the type variable's
     * own name, matching {@code Method.getGenericReturnType().getTypeName()}.
     * <p>
     * Declared types render as their binary form ({@code Outer$Inner}) when
     * {@code env} is non-null, matching the baseline reflection-based extractor.
     */
    static String typeNameOf(TypeMirror mirror, boolean generic, Env env) {
        if (mirror == null || mirror.getKind() == TypeKind.NONE || mirror.getKind() == TypeKind.NULL) {
            return "";
        }
        // Erase type variables when emitting the non-generic form so a parameter declared
        // as `T` (bound on `Number`) renders as `java.lang.Number`, matching reflection's
        // raw Class<?> view.
        if (!generic && env != null && mirror.getKind() == TypeKind.TYPEVAR) {
            mirror = env.types().erasure(mirror);
        }
        return switch (mirror.getKind()) {
            case BOOLEAN -> "boolean";
            case BYTE -> "byte";
            case SHORT -> "short";
            case INT -> "int";
            case LONG -> "long";
            case FLOAT -> "float";
            case DOUBLE -> "double";
            case CHAR -> "char";
            case VOID -> "void";
            case ARRAY -> typeNameOf(((ArrayType) mirror).getComponentType(), generic, env) + "[]";
            case DECLARED -> declaredTypeName((DeclaredType) mirror, generic, env);
            case TYPEVAR -> ((TypeVariable) mirror).asElement().getSimpleName().toString();
            case WILDCARD -> wildcardName((WildcardType) mirror, generic, env);
            case INTERSECTION -> intersectionName((IntersectionType) mirror, generic, env);
            // EXECUTABLE / PACKAGE / MODULE / ERROR / OTHER — fall back to javac's textual form.
            default -> mirror.toString();
        };
    }

    private static String declaredTypeName(DeclaredType declared, boolean generic, Env env) {
        var element = declared.asElement();
        String base;
        if (element instanceof TypeElement te && env != null) {
            base = env.elements().getBinaryName(te).toString();
        } else if (element instanceof TypeElement te) {
            // Fallback when binary name is unavailable.
            base = te.getQualifiedName().toString();
        } else {
            base = element.getSimpleName().toString();
        }
        if (!generic) {
            return base;
        }
        var args = declared.getTypeArguments();
        if (args.isEmpty()) {
            return base;
        }
        var sb = new StringBuilder(base).append('<');
        for (int i = 0; i < args.size(); i++) {
            if (i > 0) {
                sb.append(", ");
            }
            sb.append(typeNameOf(args.get(i), true, env));
        }
        sb.append('>');
        return sb.toString();
    }

    private static String wildcardName(WildcardType wildcard, boolean generic, Env env) {
        var extendsBound = wildcard.getExtendsBound();
        var superBound = wildcard.getSuperBound();
        if (extendsBound != null) {
            return "? extends " + typeNameOf(extendsBound, generic, env);
        }
        if (superBound != null) {
            return "? super " + typeNameOf(superBound, generic, env);
        }
        return "?";
    }

    private static String intersectionName(IntersectionType intersection, boolean generic, Env env) {
        var seen = new LinkedHashSet<String>();
        for (var bound : intersection.getBounds()) {
            seen.add(typeNameOf(bound, generic, env));
        }
        return String.join(" & ", seen);
    }
}

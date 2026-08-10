/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Reflection;
using System.Security;
using Lucene;

namespace Opennlp.Tools.Util.Ext
{
    /// <summary>
    /// The {@link ExtensionLoader} is responsible to load extensions to the OpenNLP library.
    /// <p>
    /// <b>Note:</b> Do not use this class, internal use only!
    /// </summary>
    internal class ExtensionLoader
    {
        private static bool isOsgiAvailable = false;
        private ExtensionLoader()
        {
        }

        /// <summary>
        /// LUCENENET: Resolves a type name that may be a Java class name.
        /// <para/>
        /// Serialized OpenNLP models record their tool factory as a Java class name
        /// (for example <c>opennlp.tools.sentdetect.SentenceDetectorFactory</c>).
        /// <see cref="Type.GetType(string)"/> cannot resolve those, so the Java name is
        /// translated to the corresponding ported type by title-casing each segment
        /// and searching this assembly.
        /// </summary>
        internal static Type ResolveType(string className)
        {
            if (className is null)
            {
                return null;
            }

            Type type = Type.GetType(className);
            if (type != null)
            {
                return type;
            }

            // Translate a Java package/class name to the ported namespace, e.g.
            // "opennlp.tools.sentdetect.SentenceDetectorFactory" ->
            // "Opennlp.Tools.Sentdetect.SentenceDetectorFactory".
            string[] parts = className.Split('.');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length > 0 && char.IsLower(parts[i][0]))
                {
                    parts[i] = char.ToUpperInvariant(parts[i][0]) + parts[i].Substring(1);
                }
            }

            string portedName = string.Join(".", parts);

            type = typeof(ExtensionLoader).Assembly.GetType(portedName, throwOnError: false);
            if (type != null)
            {
                return type;
            }

            // Fall back to matching on the simple type name, which covers cases where
            // the Java package does not line up with the ported namespace.
            string simpleName = parts[parts.Length - 1];
            foreach (Type candidate in typeof(ExtensionLoader).Assembly.GetTypes())
            {
                if (string.Equals(candidate.Name, simpleName, StringComparison.Ordinal))
                {
                    return candidate;
                }
            }

            return null;
        }

        internal static bool IsOSGiAvailable()
        {
            return isOsgiAvailable;
        }

        internal static void SetOSGiAvailable()
        {
            isOsgiAvailable = true;
        }

        // Pass in the type (interface) of the class to load
        /// <summary>
        /// Instantiates an user provided extension to OpenNLP.
        /// <p>
        /// The extension is either loaded from the class path or if running
        /// inside an OSGi environment via an OSGi service.
        /// <p>
        /// Initially it tries using the public default
        /// constructor. If it is not found, it will check if the class follows the singleton
        /// pattern: a static field named <code>INSTANCE</code> that returns an object of the type
        /// <code>T</code>.
        /// </summary>
        /// <param name="extensionClassName"></param>
        /// <returns>the instance of the extension class</returns>
        // TODO: Throw custom exception if loading fails ...
        public static T InstantiateExtension<T>(string extensionClassName)
        {

            // First try to load extension and instantiate extension from class path
            try
            {
                var extClazz = ResolveType(extensionClassName);
                if (typeof(T).IsAssignableFrom(extClazz))
                {
                    try
                    {
                        return (T)Activator.CreateInstance(extClazz);
                    }
                    catch (TargetInvocationException e)
                    {
                        throw new ExtensionNotLoadedException(e);
                    }
                    catch (MethodAccessException e)
                    {
                        // constructor is private. Try to load using INSTANCE
                        FieldInfo instanceField;
                        try
                        {
                            instanceField = extClazz.GetField("INSTANCE", BindingFlags.DeclaredOnly);
                        }
                        // catch (NoSuchFieldException e1)
                        // {
                        //     throw new ExtensionNotLoadedException(e1);
                        // }
                        catch (SecurityException e1)
                        {
                            throw new ExtensionNotLoadedException(e1);
                        }

                        if (instanceField != null)
                        {
                            try
                            {
                                return (T)instanceField.GetValue(null);
                            }
                            catch (ArgumentException e1)
                            {
                                throw new ExtensionNotLoadedException(e1);
                            }
                            catch (FieldAccessException e1)
                            {
                                throw new ExtensionNotLoadedException(e1);
                            }
                        }

                        throw new ExtensionNotLoadedException(e);
                    }
                }
                else
                {
                    throw new ExtensionNotLoadedException("Extension class '" + extClazz.Name + "' needs to have type: " + typeof(T).Name);
                }
            }
            catch (ClassNotFoundException e)
            {
            }


            // Loading from class path failed
            // Either something is wrong with the class name or OpenNLP is
            // running in an OSGi environment. The extension classes are not
            // on our classpath in this case.
            // In OSGi we need to use services to get access to extensions.
            // Determine if OSGi class is on class path
            // Now load class which depends on OSGi API
            if (isOsgiAvailable)
            {

                // The OSGIExtensionLoader class will be loaded when the next line
                // is executed, but not prior, and that is why it is safe to directly
                // reference it here.

                // LUCENENET TODO: determine if this is needed via MEF or something...
                // OSGiExtensionLoader extLoader = OSGiExtensionLoader.GetInstance();
                // return extLoader.GetExtension(clazz, extensionClassName);
            }

            throw new ExtensionNotLoadedException("Unable to find implementation for " + typeof(T).Name + ", the class or service " + extensionClassName + " could not be located!");
        }
    }
}

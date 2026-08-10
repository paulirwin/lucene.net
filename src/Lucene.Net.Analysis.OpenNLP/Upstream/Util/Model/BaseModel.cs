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

#nullable enable
using Opennlp.Tools.Util.Ext;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Lucene;
using Lucene.Net.Analysis.OpenNlp.Upstream.Support;
using Lucene.Net.Support;
using Lucene.Net.Support.IO;

namespace Opennlp.Tools.Util.Model
{
    /// <summary>
    /// This model is a common based which can be used by the components
    /// model classes.
    ///
    /// TODO:
    /// Provide sub classes access to serializers already in constructor
    /// </summary>
    public abstract class BaseModel : ArtifactProvider
    {
        protected static readonly string MANIFEST_ENTRY = "manifest.properties";
        protected static readonly string FACTORY_NAME = "factory";
        private static readonly string MANIFEST_VERSION_PROPERTY = "Manifest-Version";
        private static readonly string COMPONENT_NAME_PROPERTY = "Component-Name";
        private static readonly string VERSION_PROPERTY = "OpenNLP-Version";
        private static readonly string TIMESTAMP_PROPERTY = "Timestamp";
        private static readonly string LANGUAGE_PROPERTY = "Language";
        public static readonly string TRAINING_CUTOFF_PROPERTY = "Training-Cutoff";
        public static readonly string TRAINING_ITERATIONS_PROPERTY = "Training-Iterations";
        public static readonly string TRAINING_EVENTHASH_PROPERTY = "Training-Eventhash";
        private static string SERIALIZER_CLASS_NAME_PREFIX = "serializer-class-";
        private Dictionary<string, ArtifactSerializer> artifactSerializers = new Dictionary<string, ArtifactSerializer>();
        protected Dictionary<string, object> artifactMap = new Dictionary<string, object>();
        internal BaseToolFactory toolFactory;
        private string componentName;
        private bool subclassSerializersInitiated = false;
        private bool finishedLoadingArtifacts = false;
        private bool isLoadedFromSerialized;
        private BaseModel(string componentName, bool isLoadedFromSerialized)
        {
            this.isLoadedFromSerialized = isLoadedFromSerialized;
            this.componentName = componentName ?? throw new ArgumentNullException(nameof(componentName), "componentName must not be null!");
        }

        /// <summary>
        /// Initializes the current instance. The sub-class constructor should call the
        /// method {@link #checkArtifactMap()} to check the artifact map is OK.
        /// <p>
        /// Sub-classes will have access to custom artifacts and serializers provided
        /// by the factory.
        /// </summary>
        /// <param name="componentName">
        ///          the component name</param>
        /// <param name="languageCode">
        ///          the language code</param>
        /// <param name="manifestInfoEntries">
        ///          additional information in the manifest</param>
        /// <param name="factory">
        ///          the factory</param>
        internal BaseModel(string componentName, string languageCode, Dictionary<string, string>? manifestInfoEntries, BaseToolFactory? factory) : this(componentName, false)
        {
            // LUCENENET: ArgumentException.ThrowIfNullOrEmpty is net7.0+.
            if (string.IsNullOrEmpty(languageCode))
            {
                throw new ArgumentException("languageCode must not be null or empty", nameof(languageCode));
            }
            CreateBaseArtifactSerializers(artifactSerializers);
            Properties manifest = new Properties();
            manifest.SetProperty(MANIFEST_VERSION_PROPERTY, "1.0");
            manifest.SetProperty(LANGUAGE_PROPERTY, languageCode);
            manifest.SetProperty(VERSION_PROPERTY, Version.CurrentVersion().ToString());
            manifest.SetProperty(TIMESTAMP_PROPERTY, (J2N.Time.NanoTime() / J2N.Time.MillisecondsPerNanosecond).ToString());
            manifest.SetProperty(COMPONENT_NAME_PROPERTY, componentName);
            if (manifestInfoEntries != null)
            {
                foreach (KeyValuePair<string, string> entry in manifestInfoEntries)
                {
                    manifest.SetProperty(entry.Key, entry.Value);
                }
            }

            artifactMap.Put(MANIFEST_ENTRY, manifest);
            finishedLoadingArtifacts = true;
            if (factory != null)
            {
                SetManifestProperty(FACTORY_NAME, factory.GetType().FullName ?? throw new InvalidOperationException("Factory class must have a full name"));
                artifactMap.PutAll(factory.CreateArtifactMap());

                // new manifest entries
                Dictionary<string, string> entries = factory.CreateManifestEntries();
                foreach (KeyValuePair<string, string> entry in entries)
                {
                    SetManifestProperty(entry.Key, entry.Value);
                }
            }

            try
            {
                InitializeFactory();
            }
            catch (InvalidFormatException e)
            {
                throw new ArgumentException("Could not initialize tool factory. ", e);
            }

            LoadArtifactSerializers();
        }

        /// <summary>
        /// Initializes the current instance. The sub-class constructor should call the
        /// method {@link #checkArtifactMap()} to check the artifact map is OK.
        /// </summary>
        /// <param name="componentName">
        ///          the component name</param>
        /// <param name="languageCode">
        ///          the language code</param>
        /// <param name="manifestInfoEntries">
        ///          additional information in the manifest</param>
        protected BaseModel(string componentName, string languageCode, Dictionary<string, string> manifestInfoEntries)
            : this(componentName, languageCode, manifestInfoEntries, null)
        {
        }

        /// <summary>
        /// Initializes the current instance.
        /// </summary>
        /// <param name="componentName">the component name</param>
        /// <param name="in">the input stream containing the model</param>
        /// <exception cref="IOException"></exception>
        protected BaseModel(string componentName, Stream @in) : this(componentName, true)
        {
            LoadModel(@in);
        }

        protected BaseModel(string componentName, FileInfo modelFile) : this(componentName, true)
        {
            using Stream @in = modelFile.OpenRead();
            LoadModel(@in);
        }
        //
        // protected BaseModel(string componentName, Uri modelURL) : this(componentName, true)
        // {
        //     using (InputStream in = new BufferedInputStream(modelURL.OpenStream()))
        //     {
        //         LoadModel(@in);
        //     }
        // }

        private void LoadModel(Stream @in)
        {
            //Objects.RequireNonNull(@in, "in must not be null");
            CreateBaseArtifactSerializers(artifactSerializers);
            // if (!@in.MarkSupported())
            // {
            //     @in = new BufferedInputStream(@in);
            // }


            // TODO: Discuss this solution, the buffering should
            // int MODEL_BUFFER_SIZE_LIMIT = int.MaxValue;
            // @in.Mark(MODEL_BUFFER_SIZE_LIMIT);
            using var zip = new ZipArchive(@in, ZipArchiveMode.Read, leaveOpen: true);

            // The model package can contain artifacts which are serialized with 3rd party
            // serializers which are configured in the manifest file. To be able to load
            // the model the manifest must be read first, and afterwards all the artifacts
            // can be de-serialized.
            // The ordering of artifacts in a zip package is not guaranteed. The stream is first
            // read until the manifest appears, reseted, and read again to load all artifacts.
            bool isSearchingForManifest = true;
            using IEnumerator<ZipArchiveEntry> entries = zip.Entries.GetEnumerator();
            while (entries.MoveNext() && entries.Current is { } entry && isSearchingForManifest)
            {
                if ("manifest.properties".Equals(entry.Name))
                {

                    // TODO: Probably better to use the serializer here directly!
                    ArtifactSerializer factory = artifactSerializers["properties"];
                    using (var entryStream = entry.Open())
                    {
                        artifactMap.Put(entry.Name, factory.Create(entryStream));
                    }
                    isSearchingForManifest = false;
                }

                //zip.CloseEntry();
            }

            InitializeFactory();
            LoadArtifactSerializers();

            // The Input Stream should always be reset-able because if markSupport returns
            // false it is wrapped before hand into an Buffered InputStream
            @in.Position = 0;
            FinishLoadingArtifacts(@in);
            CheckArtifactMap();
        }

        private void InitializeFactory()
        {
            string? factoryName = GetManifestProperty(FACTORY_NAME);
            if (factoryName == null)
            {
                // load the default factory
                var factoryClass = GetDefaultFactory();
                if (factoryClass != null)
                {
                    this.toolFactory = BaseToolFactory.Create(factoryClass, this);
                }
            }
            else
            {
                try
                {
                    this.toolFactory = BaseToolFactory.Create(factoryName, this);
                }
                catch (InvalidFormatException e)
                {
                    throw new ArgumentException("Caught format exception", e);
                }
            }
        }

        /// <summary>
        /// Sub-classes should override this method if their module has a default
        /// BaseToolFactory sub-class.
        /// </summary>
        /// <returns>the default {@link BaseToolFactory} for the module, or null if none.</returns>
        protected virtual Type? GetDefaultFactory()
        {
            return null;
        }

        /// <summary>
        /// Loads the artifact serializers.
        /// </summary>
        private void LoadArtifactSerializers()
        {
            if (!subclassSerializersInitiated)
                CreateArtifactSerializers(artifactSerializers);
            subclassSerializersInitiated = true;
        }

        /// <summary>
        /// Finish loading the artifacts now that it knows all serializers.
        /// </summary>
        private void FinishLoadingArtifacts(Stream @in)
        {
            using var zip = new ZipArchive(@in, ZipArchiveMode.Read, leaveOpen: true);
            Dictionary<string, object> artifactMap = new Dictionary<string, object>();
            foreach (var entry in zip.Entries)
            {
                // Note: The manifest.properties file will be read here again,
                // there should be no need to prevent that.
                string entryName = entry.Name;
                string extension = GetEntryExtension(entryName);
                ArtifactSerializer factory = artifactSerializers[extension];
                string? artifactSerializerClazzName = GetManifestProperty(SERIALIZER_CLASS_NAME_PREFIX + entryName);
                if (artifactSerializerClazzName != null)
                {
                    factory = ExtensionLoader.InstantiateExtension<ArtifactSerializer>(artifactSerializerClazzName);
                }

                if (factory != null)
                {
                    using var entryStream = entry.Open();
                    artifactMap.Put(entryName, factory.Create(entryStream));
                }
                else
                {
                    throw new InvalidFormatException("Unknown artifact format: " + extension);
                }

                //zip.CloseEntry();
            }

            this.artifactMap.PutAll(artifactMap);
            finishedLoadingArtifacts = true;
        }

        /// <summary>
        /// Extracts the "." extension from an entry name.
        /// </summary>
        /// <param name="entry">the entry name which contains the extension</param>
        /// <returns>the extension</returns>
        /// <exception cref="InvalidFormatException">if no extension can be extracted</exception>
        private string GetEntryExtension(string entry)
        {
            int extensionIndex = entry.LastIndexOf('.') + 1;
            if (extensionIndex == -1 || extensionIndex >= entry.Length)
                throw new InvalidFormatException("Entry name must have type extension: " + entry);
            return entry.Substring(extensionIndex);
        }

        internal virtual ArtifactSerializer GetArtifactSerializer(string resourceName)
        {
            try
            {
                return artifactSerializers[GetEntryExtension(resourceName)];
            }
            catch (InvalidFormatException e)
            {
                throw new InvalidOperationException("Caught format exception", e);
            }
        }

        internal static Dictionary<string, ArtifactSerializer> CreateArtifactSerializers()
        {
            Dictionary<string, ArtifactSerializer> serializers = new Dictionary<string, ArtifactSerializer>();
            GenericModelSerializer.Register(serializers);
            PropertiesSerializer.Register(serializers);
            DictionarySerializer.Register(serializers);
            serializers.Put("txt", new ByteArraySerializer());
            serializers.Put("html", new ByteArraySerializer());
            return serializers;
        }

        /// <summary>
        /// Registers all {@link ArtifactSerializer} for their artifact file name extensions.
        /// The registered {@link ArtifactSerializer} are used to create and serialize
        /// resources in the model package.
        ///
        /// Override this method to register custom {@link ArtifactSerializer}s.
        ///
        /// Note:
        /// Subclasses should generally invoke super.createArtifactSerializers at the beginning
        /// of this method.
        ///
        /// This method is called during construction.
        /// </summary>
        /// <param name="serializers">the key of the map is the file extension used to lookup
        ///     the {@link ArtifactSerializer}.</param>
        internal virtual void CreateArtifactSerializers(Dictionary<string, ArtifactSerializer> serializers)
        {
            if (this.toolFactory != null)
                serializers.PutAll(this.toolFactory.CreateArtifactSerializersMap());
        }

        private void CreateBaseArtifactSerializers(Dictionary<string, ArtifactSerializer> serializers)
        {
            serializers.PutAll(CreateArtifactSerializers());
        }

        /// <summary>
        /// Validates the parsed artifacts. If something is not
        /// valid subclasses should throw an {@link InvalidFormatException}.
        ///
        /// Note:
        /// Subclasses should generally invoke super.validateArtifactMap at the beginning
        /// of this method.
        /// </summary>
        /// <exception cref="InvalidFormatException"></exception>
        protected virtual void ValidateArtifactMap()
        {
            if (!(artifactMap[MANIFEST_ENTRY] is Properties))
                throw new InvalidFormatException("Missing the " + MANIFEST_ENTRY + "!");

            // First check version, everything else might change in the future
            string versionString = GetManifestProperty(VERSION_PROPERTY);
            if (versionString != null)
            {
                Version version;
                try
                {
                    version = Version.Parse(versionString);
                }
                catch (System.FormatException e)
                {
                    throw new InvalidFormatException("Unable to parse model version '" + versionString + "'!", e);
                }


                // Version check is only performed if current version is not the dev/debug version
                if (!Version.CurrentVersion().Equals(Version.DEV_VERSION))
                {

                    // Major and minor version must match, revision might be
                    // this check allows for the use of models of n minor release behind current minor release
                    if (Version.CurrentVersion().GetMajor() != version.GetMajor() || Version.CurrentVersion().GetMinor() - 4 > version.GetMinor())
                    {
                        throw new InvalidFormatException("Model version " + version + " is not supported by this (" + Version.CurrentVersion() + ") version of OpenNLP!");
                    }


                    // Reject loading a snapshot model with a non-snapshot version
                    if (!Version.CurrentVersion().IsSnapshot() && version.IsSnapshot())
                    {
                        throw new InvalidFormatException("Model version " + version + " is a snapshot - snapshot models are not supported by this non-snapshot version (" + Version.CurrentVersion() + ") of OpenNLP!");
                    }
                }
            }
            else
            {
                throw new InvalidFormatException("Missing " + VERSION_PROPERTY + " property in " + MANIFEST_ENTRY + "!");
            }

            if (GetManifestProperty(COMPONENT_NAME_PROPERTY) == null)
                throw new InvalidFormatException("Missing " + COMPONENT_NAME_PROPERTY + " property in " + MANIFEST_ENTRY + "!");
            if (!GetManifestProperty(COMPONENT_NAME_PROPERTY).Equals(componentName))
                throw new InvalidFormatException("The " + componentName + " cannot load a model for the " + GetManifestProperty(COMPONENT_NAME_PROPERTY) + "!");
            if (GetManifestProperty(LANGUAGE_PROPERTY) == null)
                throw new InvalidFormatException("Missing " + LANGUAGE_PROPERTY + " property in " + MANIFEST_ENTRY + "!");

            // Validate the factory. We try to load it using the ExtensionLoader. It
            // will return the factory, null or raise an exception
            string factoryName = GetManifestProperty(FACTORY_NAME);
            if (factoryName != null)
            {
                try
                {
                    if (ExtensionLoader.InstantiateExtension<BaseToolFactory>(factoryName) == null)
                    {
                        throw new InvalidFormatException("Could not load an user extension specified by the model: " + factoryName);
                    }
                }
                catch (Exception e)
                {
                    throw new InvalidFormatException("Could not load an user extension specified by the model: " + factoryName, e);
                }
            }


            // validate artifacts declared by the factory
            if (toolFactory != null)
            {
                toolFactory.ValidateArtifactMap();
            }
        }

        /// <summary>
        /// Checks the artifact map.
        /// <p>
        /// A subclass should call this method from a constructor which accepts the individual
        /// artifact map items, to validate that these items form a valid model.
        /// <p>
        /// If the artifacts are not valid an IllegalArgumentException will be thrown.
        /// </summary>
        protected virtual void CheckArtifactMap()
        {
            if (!finishedLoadingArtifacts)
                throw new InvalidOperationException("The method BaseModel.finishLoadingArtifacts(..) was not called by BaseModel sub-class.");
            try
            {
                ValidateArtifactMap();
            }
            catch (InvalidFormatException e)
            {
                throw new ArgumentException("Caught format exception", e);
            }
        }

        /// <summary>
        /// Retrieves the value to the given key from the manifest.properties
        /// entry.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>the value</returns>
        public string? GetManifestProperty(string key)
        {
            Properties manifest = (Properties)artifactMap[MANIFEST_ENTRY];
            return manifest.GetProperty(key);
        }

        /// <summary>
        /// Sets a given value for a given key to the manifest.properties entry.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        protected void SetManifestProperty(string key, string value)
        {
            Properties manifest = (Properties)artifactMap[MANIFEST_ENTRY];
            manifest.SetProperty(key, value);
        }

        /// <summary>
        /// Retrieves the language code of the material which
        /// was used to train the model or x-unspecified if
        /// non was set.
        /// </summary>
        /// <returns>the language code of this model</returns>
        public string GetLanguage()
        {
            return GetManifestProperty(LANGUAGE_PROPERTY);
        }

        /// <summary>
        /// Retrieves the OpenNLP version which was used
        /// to create the model.
        /// </summary>
        /// <returns>the version</returns>
        public Version GetVersion()
        {
            string version = GetManifestProperty(VERSION_PROPERTY);
            return Version.Parse(version);
        }

        // /// <summary>
        // /// Serializes the model to the given {@link OutputStream}.
        // /// </summary>
        // /// <param name="out">stream to write the model to</param>
        // /// <exception cref="IOException"></exception>
        // public void Serialize(Stream @out)
        // {
        //     if (!subclassSerializersInitiated)
        //     {
        //         throw new InvalidOperationException("The method BaseModel.loadArtifactSerializers() was not called by BaseModel subclass constructor.");
        //     }
        //
        //     foreach (KeyValuePair<string, object> entry in artifactMap)
        //     {
        //         string name = entry.Key;
        //         object artifact = entry.Value;
        //         if (artifact is SerializableArtifact)
        //         {
        //             SerializableArtifact serializableArtifact = (SerializableArtifact)artifact;
        //             string artifactSerializerName = serializableArtifact.GetArtifactSerializerClass().GetName();
        //             SetManifestProperty(SERIALIZER_CLASS_NAME_PREFIX + name, artifactSerializerName);
        //         }
        //     }
        //
        //     using var zip = new ZipArchive(@out, ZipArchiveMode.Create, leaveOpen: true);
        //     foreach (KeyValuePair<string, object> entry in artifactMap)
        //     {
        //         string name = entry.Key;
        //         var zipEntry = zip.CreateEntry(name);
        //         object artifact = entry.Value;
        //         ArtifactSerializer serializer = GetArtifactSerializer(name);
        //
        //         // If model is serialize-able always use the provided serializer
        //         if (artifact is SerializableArtifact)
        //         {
        //             SerializableArtifact serializableArtifact = (SerializableArtifact)artifact;
        //             string artifactSerializerName = serializableArtifact.GetArtifactSerializerClass().GetName();
        //             serializer = ExtensionLoader.InstantiateExtension<ArtifactSerializer>(artifactSerializerName);
        //         }
        //
        //         if (serializer == null)
        //         {
        //             throw new InvalidOperationException("Missing serializer for " + name);
        //         }
        //
        //         serializer.Serialize(artifactMap[name], zip);
        //         //zip.CloseEntry();
        //     }
        //
        //     // zip.Finish();
        //     // zip.Flush();
        //}
        //
        // public void Serialize(FileInfo model)
        // {
        //     using Stream @out = model.OpenWrite();
        //     Serialize(@out);
        // }
        //
        // public void Serialize(string model)
        // {
        //     Serialize(new FileInfo(model));
        // }

        public virtual T GetArtifact<T>(string key)
        {
            // LUCENENET: Java's Map.get() returns null for an absent key, whereas the
            // .NET indexer throws KeyNotFoundException, so TryGetValue is used here.
            if (!artifactMap.TryGetValue(key, out object artifact) || artifact is null)
                return default;
            return (T)artifact;
        }

        public virtual bool IsLoadedFromSerialized()
        {
            return isLoadedFromSerialized;
        }

        // // These methods are required to serialize/deserialize the model because
        // // many of the included objects in this model are not Serializable.
        // // An alternative to this solution is to make all included objects
        // // Serializable and remove the writeObject and readObject methods.
        // // This will allow the usage of final for fields that should not change.
        // private async Task WriteObject(Stream @out)
        // {
        //     await @out.WriteUTFAsync(componentName);
        //     this.Serialize(@out);
        // }

        private async Task ReadObject(Stream @in)
        {
            isLoadedFromSerialized = true;
            artifactSerializers = new Dictionary<string, ArtifactSerializer>();
            artifactMap = new Dictionary<string, object>();
            componentName = await @in.ReadUTFAsync();
            this.LoadModel(@in);
        }
    }
}

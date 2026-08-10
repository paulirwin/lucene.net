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
using Lucene.Net.Support;
using Opennlp.Tools.Dictionary;
using Opennlp.Tools.Ml.Model;
using Opennlp.Tools.Namefind;
using Opennlp.Tools.Util;
using Opennlp.Tools.Util.Ext;
using Opennlp.Tools.Util.Featuregen;
using Opennlp.Tools.Util.Model;
using Lucene.Net.Support.IO;
using Lucene.Net.Analysis.OpenNlp.Upstream.Support;
using J2N;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Opennlp.Tools.Postag
{
    /// <summary>
    /// The factory that provides POS Tagger default implementations and resources
    /// </summary>
    internal class POSTaggerFactory : BaseToolFactory
    {
        private static readonly string TAG_DICTIONARY_ENTRY_NAME = "tags.tagdict";
        private static readonly string NGRAM_DICTIONARY_ENTRY_NAME = "ngram.dictionary";
        protected Opennlp.Tools.Dictionary.Dictionary ngramDictionary;
        private byte[] featureGeneratorBytes;
        private Dictionary<string, object> resources;
        protected TagDictionary posDictionary;
        /// <summary>
        /// Creates a {@link POSTaggerFactory} that provides the default implementation
        /// of the resources.
        /// </summary>
        public POSTaggerFactory()
        {
        }

        /// <summary>
        /// Creates a {@link POSTaggerFactory}. Use this constructor to
        /// programmatically create a factory.
        /// </summary>
        /// <param name="ngramDictionary"></param>
        /// <param name="posDictionary"></param>
        /// <remarks>
        /// @deprecatedthis constructor is here for backward compatibility and
        ///             is not functional anymore in the training of 1.8.x series models
        /// </remarks>
        public POSTaggerFactory(Opennlp.Tools.Dictionary.Dictionary ngramDictionary, TagDictionary posDictionary)
        {
            this.Init(ngramDictionary, posDictionary); // TODO: This could be made functional by creating some default feature generation
            // which uses the dictionary ...
        }

        public POSTaggerFactory(byte[] featureGeneratorBytes, Dictionary<string, object> resources, TagDictionary posDictionary)
        {
            this.featureGeneratorBytes = featureGeneratorBytes;
            if (this.featureGeneratorBytes == null)
            {
                this.featureGeneratorBytes = LoadDefaultFeatureGeneratorBytes();
            }

            this.resources = resources;
            this.posDictionary = posDictionary;
        }

        protected virtual void Init(Opennlp.Tools.Dictionary.Dictionary ngramDictionary, TagDictionary posDictionary)
        {
            this.ngramDictionary = ngramDictionary;
            this.posDictionary = posDictionary;
        }

        protected virtual void Init(byte[] featureGeneratorBytes, Dictionary<string, object> resources, TagDictionary posDictionary)
        {
            this.featureGeneratorBytes = featureGeneratorBytes;
            this.resources = resources;
            this.posDictionary = posDictionary;
        }

        private static byte[] LoadDefaultFeatureGeneratorBytes()
        {
            ByteArrayOutputStream bytes = new ByteArrayOutputStream();
            try
            {
                using (Stream @in = typeof(TokenNameFinderFactory).FindAndGetManifestResourceStream("/opennlp/tools/postag/pos-default-features.xml"))
                {
                    if (@in == null)
                    {
                        throw new InvalidOperationException("Classpath must contain pos-default-features.xml file!");
                    }

                    byte[] buf = new byte[1024];
                    int len;
                    while ((len = @in.Read(buf)) > 0)
                    {
                        bytes.Write(buf, 0, len);
                    }
                }
            }
            catch (IOException e)
            {
                throw new InvalidOperationException("Failed reading from pos-default-features.xml file on classpath!");
            }

            return bytes.ToArray();
        }

        /// <summary>
        /// Creates the {@link AdaptiveFeatureGenerator}. Usually this
        /// is a set of generators contained in the {@link AggregatedFeatureGenerator}.
        /// 
        /// Note:
        /// The generators are created on every call to this method.
        /// </summary>
        /// <returns>the feature generator or null if there is no descriptor in the model</returns>
        public virtual AdaptiveFeatureGenerator CreateFeatureGenerators()
        {
            if (featureGeneratorBytes == null && artifactProvider != null)
            {
                featureGeneratorBytes = artifactProvider.GetArtifact<byte[]>(POSModel.GENERATOR_DESCRIPTOR_ENTRY_NAME);
            }

            if (featureGeneratorBytes == null)
            {
                featureGeneratorBytes = LoadDefaultFeatureGeneratorBytes();
            }

            Stream descriptorIn = new MemoryStream(featureGeneratorBytes);
            AdaptiveFeatureGenerator generator;
            try
            {
                generator = GeneratorFactory.Create(descriptorIn, (key) =>
                {
                    if (artifactProvider != null)
                    {
                        return artifactProvider.GetArtifact<object>(key);
                    }
                    else
                    {
                        return resources[key];
                    }
                });
            }
            catch (InvalidFormatException e)
            {

                // It is assumed that the creation of the feature generation does not
                // fail after it succeeded once during model loading.
                // But it might still be possible that such an exception is thrown,
                // in this case the caller should not be forced to handle the exception
                // and a Runtime Exception is thrown instead.
                // If the re-creation of the feature generation fails it is assumed
                // that this can only be caused by a programming mistake and therefore
                // throwing a Runtime Exception is reasonable
                throw new InvalidOperationException(); // FeatureGeneratorCreationError(e);
            }
            catch (IOException e)
            {
                throw new InvalidOperationException("Reading from mem cannot result in an I/O error", e);
            }

            return generator;
        }

        public override Dictionary<string, ArtifactSerializer> CreateArtifactSerializersMap()
        {
            Dictionary<string, ArtifactSerializer> serializers = base.CreateArtifactSerializersMap();

            // NOTE: This is only needed for old models and this if can be removed if support is dropped
            POSDictionarySerializer.Register(serializers);
            return serializers;
        }

        public override Dictionary<string, object> CreateArtifactMap()
        {
            Dictionary<string, object> artifactMap = base.CreateArtifactMap();
            if (posDictionary != null)
                artifactMap.Put(TAG_DICTIONARY_ENTRY_NAME, posDictionary);
            if (ngramDictionary != null)
                artifactMap.Put(NGRAM_DICTIONARY_ENTRY_NAME, ngramDictionary);
            return artifactMap;
        }

        public virtual TagDictionary CreateTagDictionary(FileInfo dictionary)
        {
            return CreateTagDictionary(dictionary.OpenRead());
        }

        public virtual TagDictionary CreateTagDictionary(Stream @in)
        {
            return POSDictionary.Create(@in);
        }

        public virtual void SetTagDictionary(TagDictionary dictionary)
        {
            if (artifactProvider != null)
            {
                throw new InvalidOperationException("Can not set tag dictionary while using artifact provider.");
            }

            this.posDictionary = dictionary;
        }

        protected virtual Dictionary<string, object> GetResources()
        {
            if (resources != null)
            {
                return resources;
            }

            return new Dictionary<string, object>();
        }

        protected virtual byte[] GetFeatureGenerator()
        {
            return featureGeneratorBytes;
        }

        public virtual TagDictionary GetTagDictionary()
        {
            if (this.posDictionary == null && artifactProvider != null)
                this.posDictionary = artifactProvider.GetArtifact<TagDictionary>(TAG_DICTIONARY_ENTRY_NAME);
            return this.posDictionary;
        }

        /// <summary>
        /// </summary>
        /// <remarks>@deprecatedthis will be reduced in visibility and later removed</remarks>
        public virtual Opennlp.Tools.Dictionary.Dictionary GetDictionary()
        {
            if (this.ngramDictionary == null && artifactProvider != null)
                this.ngramDictionary = artifactProvider.GetArtifact<Opennlp.Tools.Dictionary.Dictionary>(NGRAM_DICTIONARY_ENTRY_NAME);
            return this.ngramDictionary;
        }

        public virtual void SetDictionary(Opennlp.Tools.Dictionary.Dictionary ngramDict)
        {
            if (artifactProvider != null)
            {
                throw new InvalidOperationException("Can not set ngram dictionary while using artifact provider.");
            }

            this.ngramDictionary = ngramDict;
        }

        public virtual POSContextGenerator GetPOSContextGenerator()
        {
            return GetPOSContextGenerator(0);
        }

        public virtual POSContextGenerator GetPOSContextGenerator(int cacheSize)
        {
            if (artifactProvider != null)
            {
                Properties manifest = artifactProvider.GetArtifact<Properties>("manifest.properties");
                string version = manifest.GetProperty("OpenNLP-Version");
                if (Opennlp.Tools.Util.Version.Parse(version).GetMinor() < 8)
                {
                    return new DefaultPOSContextGenerator(cacheSize, GetDictionary());
                }
            }

            return new ConfigurablePOSContextGenerator(cacheSize, CreateFeatureGenerators());
        }

        public virtual SequenceValidator<string> GetSequenceValidator()
        {
            return new DefaultPOSSequenceValidator(GetTagDictionary());
        }

        // TODO: This should not be done anymore for 8 models, they can just
        // use the SerializableArtifact interface
        internal class POSDictionarySerializer : ArtifactSerializer<POSDictionary>
        {
            public virtual POSDictionary Create(Stream @in)
            {
                return POSDictionary.Create(new UncloseableInputStream(@in));
            }

            // LUCENENET: serialization is not supported; inference only.
            // public virtual void Serialize(POSDictionary artifact, Stream @out)
            // {
            //     artifact.Serialize(@out);
            // }

            internal static void Register(Dictionary<string, ArtifactSerializer> factories)
            {
                factories.Put("tagdict", new POSDictionarySerializer());
            }
            // LUCENENET: upstream relies on a default interface implementation to
            // bridge the non-generic ArtifactSerializer; DIMs are unavailable on
            // netstandard2.0/net462, so the bridge is explicit here.
            object ArtifactSerializer.Create(Stream @in) => Create(@in);
        }

        protected virtual void ValidatePOSDictionary(POSDictionary posDict, AbstractModel posModel)
        {
            HashSet<string> dictTags = new HashSet<string>();
            foreach (string word in posDict)
            {
                dictTags.UnionWith(posDict.GetTags(word));
            }

            HashSet<string> modelTags = new HashSet<string>();
            for (int i = 0; i < posModel.GetNumOutcomes(); i++)
            {
                modelTags.Add(posModel.GetOutcome(i));
            }

            if (!dictTags.IsSubsetOf(modelTags))
            {
                StringBuilder unknownTag = new StringBuilder();
                foreach (string d in dictTags)
                {
                    if (!modelTags.Contains(d))
                    {
                        unknownTag.Append(d).Append(" ");
                    }
                }

                throw new InvalidFormatException("Tag dictionary contains tags " + "which are unknown by the model! The unknown tags are: " + unknownTag.ToString());
            }
        }

        public override void ValidateArtifactMap()
        {

            // Ensure that the tag dictionary is compatible with the model
            object tagdictEntry = this.artifactProvider.GetArtifact<object>(TAG_DICTIONARY_ENTRY_NAME);
            if (tagdictEntry != null)
            {
                if (tagdictEntry is POSDictionary)
                {
                    if (!this.artifactProvider.IsLoadedFromSerialized())
                    {
                        AbstractModel posModel = this.artifactProvider.GetArtifact<AbstractModel>(POSModel.POS_MODEL_ENTRY_NAME);
                        POSDictionary posDict = (POSDictionary)tagdictEntry;
                        ValidatePOSDictionary(posDict, posModel);
                    }
                }
                else
                {
                    throw new InvalidFormatException("POSTag dictionary has wrong type!");
                }
            }

            object ngramDictEntry = this.artifactProvider.GetArtifact<object>(NGRAM_DICTIONARY_ENTRY_NAME);
            if (ngramDictEntry != null && !(ngramDictEntry is Opennlp.Tools.Dictionary.Dictionary))
            {
                throw new InvalidFormatException("NGram dictionary has wrong type!");
            }
        }

        public static POSTaggerFactory Create(string subclassName, Opennlp.Tools.Dictionary.Dictionary ngramDictionary, TagDictionary posDictionary)
        {
            if (subclassName == null)
            {

                // will create the default factory
                return new POSTaggerFactory(ngramDictionary, posDictionary);
            }

            try
            {
                POSTaggerFactory theFactory = ExtensionLoader.InstantiateExtension<POSTaggerFactory>(subclassName);
                theFactory.Init(ngramDictionary, posDictionary);
                return theFactory;
            }
            catch (Exception e)
            {
                string msg = "Could not instantiate the " + subclassName + ". The initialization throw an exception.";
                throw new InvalidFormatException(msg, e);
            }
        }

        public static POSTaggerFactory Create(string subclassName, byte[] featureGeneratorBytes, Dictionary<string, object> resources, TagDictionary posDictionary)
        {
            POSTaggerFactory theFactory;
            if (subclassName == null)
            {

                // will create the default factory
                theFactory = new POSTaggerFactory(null, posDictionary);
            }
            else
            {
                try
                {
                    theFactory = ExtensionLoader.InstantiateExtension<POSTaggerFactory>(subclassName);
                }
                catch (Exception e)
                {
                    string msg = "Could not instantiate the " + subclassName + ". The initialization throw an exception.";
                    throw new InvalidFormatException(msg, e);
                }
            }

            theFactory.Init(featureGeneratorBytes, resources, posDictionary);
            return theFactory;
        }

        public virtual TagDictionary CreateEmptyTagDictionary()
        {
            this.posDictionary = new POSDictionary(true);
            return this.posDictionary;
        }
    }
}

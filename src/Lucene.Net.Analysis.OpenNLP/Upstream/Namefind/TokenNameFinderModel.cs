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

using Opennlp.Tools.Ml;
using Opennlp.Tools.Ml.Model;
using Opennlp.Tools.Util;
using Opennlp.Tools.Util.Model;
using Opennlp.Tools.Util.Featuregen;
using System;
using System.Collections.Generic;
using System.IO;
using Lucene.Net.Analysis.OpenNlp.Upstream.Support;
using Lucene.Net.Support;

namespace Opennlp.Tools.Namefind
{
    /// <summary>
    /// The {@link TokenNameFinderModel} is the model used
    /// by a learnable {@link TokenNameFinder}.
    /// </summary>
    /// <remarks>@seeNameFinderME</remarks>
    // TODO: Fix the model validation, on loading via constructors and input streams
    public class TokenNameFinderModel : BaseModel
    {
        internal class FeatureGeneratorCreationError : Exception
        {
            internal FeatureGeneratorCreationError(Exception t) : base(null, t)
            {
            }
        }

        private static readonly string COMPONENT_NAME = "NameFinderME";
        private static readonly string MAXENT_MODEL_ENTRY_NAME = "nameFinder.model";
        internal static readonly string GENERATOR_DESCRIPTOR_ENTRY_NAME = "generator.featuregen";
        internal static readonly string SEQUENCE_CODEC_CLASS_NAME_PARAMETER = "sequenceCodecImplName";
        internal TokenNameFinderModel(string languageCode, SequenceClassificationModel<string> nameFinderModel, byte[] generatorDescriptor, Dictionary<string, object> resources, Dictionary<string, string> manifestInfoEntries, SequenceCodec<string> seqCodec, TokenNameFinderFactory factory) : base(COMPONENT_NAME, languageCode, manifestInfoEntries, factory)
        {
            Init(nameFinderModel, generatorDescriptor, resources, manifestInfoEntries, seqCodec);
            if (!seqCodec.AreOutcomesCompatible(nameFinderModel.GetOutcomes()))
            {
                throw new ArgumentException("Model not compatible with name finder!");
            }
        }

        internal TokenNameFinderModel(string languageCode, MaxentModel nameFinderModel, int beamSize, byte[] generatorDescriptor, Dictionary<string, object> resources, Dictionary<string, string> manifestInfoEntries, SequenceCodec<string> seqCodec, TokenNameFinderFactory factory) : base(COMPONENT_NAME, languageCode, manifestInfoEntries, factory)
        {
            Properties manifest = (Properties)artifactMap[MANIFEST_ENTRY];
            manifest.Put(BeamSearch.BEAM_SIZE_PARAMETER, beamSize.ToString());
            Init(nameFinderModel, generatorDescriptor, resources, manifestInfoEntries, seqCodec);
            if (!IsModelValid(nameFinderModel))
            {
                throw new ArgumentException("Model not compatible with name finder!");
            }
        }

        // TODO: Extend this one with beam size!
        internal TokenNameFinderModel(string languageCode, MaxentModel nameFinderModel, byte[] generatorDescriptor, Dictionary<string, object> resources, Dictionary<string, string> manifestInfoEntries) : this(languageCode, nameFinderModel, NameFinderME.DEFAULT_BEAM_SIZE, generatorDescriptor, resources, manifestInfoEntries, new BioCodec(), new TokenNameFinderFactory())
        {
        }

        internal TokenNameFinderModel(string languageCode, MaxentModel nameFinderModel, Dictionary<string, object> resources, Dictionary<string, string> manifestInfoEntries) : this(languageCode, nameFinderModel, null, resources, manifestInfoEntries)
        {
        }

        public TokenNameFinderModel(Stream @in) : base(COMPONENT_NAME, @in)
        {
        }

        public TokenNameFinderModel(FileInfo modelFile) : base(COMPONENT_NAME, modelFile)
        {
        }

        public TokenNameFinderModel(string modelPath) : this(new FileInfo(modelPath))
        {
        }

        // public TokenNameFinderModel(URL modelURL) : base(COMPONENT_NAME, modelURL)
        // {
        // }

        private void Init(object nameFinderModel, byte[] generatorDescriptor, Dictionary<string, object> resources, Dictionary<string, string> manifestInfoEntries, SequenceCodec<string> seqCodec)
        {
            Properties manifest = (Properties)artifactMap[MANIFEST_ENTRY];
            manifest.Put(SEQUENCE_CODEC_CLASS_NAME_PARAMETER, seqCodec.GetType().FullName);
            artifactMap.Put(MAXENT_MODEL_ENTRY_NAME, nameFinderModel);
            if (generatorDescriptor != null && generatorDescriptor.Length > 0)
                artifactMap.Put(GENERATOR_DESCRIPTOR_ENTRY_NAME, generatorDescriptor);
            if (resources != null)
            {

                // The resource map must not contain key which are already taken
                // like the name finder maxent model name
                if (resources.ContainsKey(MAXENT_MODEL_ENTRY_NAME) || resources.ContainsKey(GENERATOR_DESCRIPTOR_ENTRY_NAME))
                {
                    throw new ArgumentException();
                }


                // TODO: Add checks to not put resources where no serializer exists,
                // make that case fail here, should be done in the BaseModel
                artifactMap.PutAll(resources);
            }

            CheckArtifactMap();
        }

        internal virtual SequenceClassificationModel<string> GetNameFinderSequenceModel()
        {
            Properties manifest = (Properties)artifactMap[MANIFEST_ENTRY];
            if (artifactMap[MAXENT_MODEL_ENTRY_NAME] is MaxentModel)
            {
                string beamSizeString = manifest.GetProperty(BeamSearch.BEAM_SIZE_PARAMETER);
                int beamSize = NameFinderME.DEFAULT_BEAM_SIZE;
                if (beamSizeString != null)
                {
                    beamSize = int.Parse(beamSizeString);
                }

                return new BeamSearch<string>(beamSize, (MaxentModel)artifactMap[MAXENT_MODEL_ENTRY_NAME]);
            }
            else if (artifactMap[MAXENT_MODEL_ENTRY_NAME] is SequenceClassificationModel<string>)
            {
                return (SequenceClassificationModel<string>)artifactMap[MAXENT_MODEL_ENTRY_NAME];
            }
            else
            {
                return null;
            }
        }

        protected override Type GetDefaultFactory()
        {
            return typeof(TokenNameFinderFactory);
        }

        internal virtual SequenceCodec<string> GetSequenceCodec()
        {
            return this.GetFactory().GetSequenceCodec();
        }

        internal virtual TokenNameFinderFactory GetFactory()
        {
            return (TokenNameFinderFactory)this.toolFactory;
        }

        internal override void CreateArtifactSerializers(Dictionary<string, ArtifactSerializer> serializers)
        {
            base.CreateArtifactSerializers(serializers);
            serializers.Put("featuregen", new ByteArraySerializer());
        }

        /// <summary>
        /// Create the artifact serializers. Currently for serializers related to
        /// features that require external resources, such as {@code W2VClassesDictionary}
        /// objects, the convention is to add its element tag name as key of the serializer map.
        /// For example, the element tag name for the {@code WordClusterFeatureGenerator} which
        /// uses {@code W2VClassesDictionary} objects serialized by the {@code W2VClassesDictionarySerializer}
        /// is 'wordcluster', which is the key used to add the serializer to the map.
        /// </summary>
        /// <returns>the map containing the added serializers</returns>
        internal static Dictionary<string, ArtifactSerializer> CreateArtifactSerializers()
        {

            // TODO: Not so nice, because code cannot really be reused by the other create serializer method
            //       Has to be redesigned, we need static access to default serializers
            //       and these should be able to extend during runtime ?!
            //
            //       The XML feature generator factory should provide these mappings.
            //       Usually the feature generators should know what type of resource they expect.
            Dictionary<string, ArtifactSerializer> serializers = BaseModel.CreateArtifactSerializers();
            serializers.Put("featuregen", new ByteArraySerializer());
            serializers.Put("wordcluster", new WordClusterDictionary.WordClusterDictionarySerializer());
            serializers.Put("brownclustertoken", new BrownCluster.BrownClusterSerializer());
            serializers.Put("brownclustertokenclass", new BrownCluster.BrownClusterSerializer());
            serializers.Put("brownclusterbigram", new BrownCluster.BrownClusterSerializer());
            return serializers;
        }

        private bool IsModelValid(MaxentModel model)
        {
            string[] outcomes = new string[model.GetNumOutcomes()];
            for (int i = 0; i < model.GetNumOutcomes(); i++)
            {
                outcomes[i] = model.GetOutcome(i);
            }

            return GetFactory().CreateSequenceCodec().AreOutcomesCompatible(outcomes);
        }

        protected override void ValidateArtifactMap()
        {
            base.ValidateArtifactMap();
            if (!(artifactMap[MAXENT_MODEL_ENTRY_NAME] is MaxentModel) && !(artifactMap[MAXENT_MODEL_ENTRY_NAME] is SequenceClassificationModel<string>))
            {
                throw new InvalidFormatException("Token Name Finder model is incomplete!");
            }
        }
    }
}

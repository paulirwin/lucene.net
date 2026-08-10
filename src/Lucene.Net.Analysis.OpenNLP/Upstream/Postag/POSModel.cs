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
using Opennlp.Tools.Ml;
using Opennlp.Tools.Ml.Model;
using Opennlp.Tools.Util;
using Opennlp.Tools.Util.Model;
using Lucene.Net.Analysis.OpenNlp.Upstream.Support;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Opennlp.Tools.Postag
{
    /// <summary>
    /// The {@link POSModel} is the model used
    /// by a learnable {@link POSTagger}.
    /// </summary>
    /// <remarks>@seePOSTaggerME</remarks>
    public sealed class POSModel : BaseModel, SerializableArtifact
    {
        private static readonly string COMPONENT_NAME = "POSTaggerME";
        internal const string POS_MODEL_ENTRY_NAME = "pos.model";
        internal const string GENERATOR_DESCRIPTOR_ENTRY_NAME = "generator.featuregen";
        // LUCENENET: these constructors are only used when training a new model,
        // which is not supported; we only support inference of existing models.
        // public POSModel(string languageCode, SequenceClassificationModel<string> posModel, Dictionary<string, string> manifestInfoEntries, POSTaggerFactory posFactory) : base(COMPONENT_NAME, languageCode, manifestInfoEntries, posFactory)
        // {
        //     artifactMap.Put(POS_MODEL_ENTRY_NAME, posModel ?? throw new ArgumentNullException(nameof(posModel)));
        //     artifactMap.Put(GENERATOR_DESCRIPTOR_ENTRY_NAME, posFactory.GetFeatureGenerator());
        //     foreach (Map.Entry<String, Object> resource in posFactory.GetResources().EntrySet())
        //     {
        //         artifactMap.Put(resource.GetKey(), resource.GetValue());
        //     } // TODO: This fails probably for the sequence model ... ?!
        //     // checkArtifactMap();
        // }

        // public POSModel(string languageCode, MaxentModel posModel, Dictionary<string, string> manifestInfoEntries, POSTaggerFactory posFactory) : this(languageCode, posModel, POSTaggerME.DEFAULT_BEAM_SIZE, manifestInfoEntries, posFactory)
        // {
        // }

        // public POSModel(string languageCode, MaxentModel posModel, int beamSize, Dictionary<string, string> manifestInfoEntries, POSTaggerFactory posFactory) : base(COMPONENT_NAME, languageCode, manifestInfoEntries, posFactory)
        // {
        //     if (posModel is null)
        //     {
        //         throw new ArgumentNullException(nameof(posModel));
        //     }
// 
        //     Properties manifest = (Properties)artifactMap[MANIFEST_ENTRY];
        //     manifest.SetProperty(BeamSearch.BEAM_SIZE_PARAMETER, Convert.ToString(beamSize));
        //     artifactMap.Put(POS_MODEL_ENTRY_NAME, posModel);
        //     artifactMap.Put(GENERATOR_DESCRIPTOR_ENTRY_NAME, posFactory.GetFeatureGenerator());
        //     foreach (Map.Entry<String, Object> resource in posFactory.GetResources().EntrySet())
        //     {
        //         artifactMap.Put(resource.GetKey(), resource.GetValue());
        //     }
// 
        //     CheckArtifactMap();
        // }

        public POSModel(Stream @in) : base(COMPONENT_NAME, @in)
        {
        }

        public POSModel(FileInfo modelFile) : base(COMPONENT_NAME, modelFile)
        {
        }

        // LUCENENET: the Path and URL overloads have no .NET equivalent in BaseModel.
        // public POSModel(Path modelPath) : this(modelPath.ToFile())
        // {
        // }

        // public POSModel(URL modelURL) : base(COMPONENT_NAME, modelURL)
        // {
        // }

        protected override Type GetDefaultFactory()
        {
            return typeof(POSTaggerFactory);
        }

        protected override void ValidateArtifactMap()
        {
            base.ValidateArtifactMap();
            if (!(artifactMap[POS_MODEL_ENTRY_NAME] is MaxentModel))
            {
                throw new InvalidFormatException("POS model is incomplete!");
            }
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// @deprecateduse getPosSequenceModel instead. This method will be removed soon.
        /// Only required for Parser 1.5.x backward compatibility. Newer models don't need this anymore.
        /// </remarks>
        internal MaxentModel GetPosModel()
        {
            if (artifactMap[POS_MODEL_ENTRY_NAME] is MaxentModel)
            {
                return (MaxentModel)artifactMap[POS_MODEL_ENTRY_NAME];
            }
            else
            {
                return null;
            }
        }

        internal SequenceClassificationModel<string> GetPosSequenceModel()
        {
            Properties manifest = (Properties)artifactMap[MANIFEST_ENTRY];
            if (artifactMap[POS_MODEL_ENTRY_NAME] is MaxentModel)
            {
                string beamSizeString = manifest.GetProperty(BeamSearch.BEAM_SIZE_PARAMETER);
                int beamSize = POSTaggerME.DEFAULT_BEAM_SIZE;
                if (beamSizeString != null)
                {
                    beamSize = int.Parse(beamSizeString);
                }

                return new BeamSearch<string>(beamSize, (MaxentModel)artifactMap[POS_MODEL_ENTRY_NAME]);
            }
            else if (artifactMap[POS_MODEL_ENTRY_NAME] is SequenceClassificationModel<string>)
            {
                return (SequenceClassificationModel<string>)artifactMap[POS_MODEL_ENTRY_NAME];
            }
            else
            {
                return null;
            }
        }

        internal POSTaggerFactory GetFactory()
        {
            return (POSTaggerFactory)this.toolFactory;
        }

        internal override void CreateArtifactSerializers(Dictionary<string, ArtifactSerializer> serializers)
        {
            base.CreateArtifactSerializers(serializers);
            serializers.Put("featuregen", new ByteArraySerializer());
        }

        /// <summary>
        /// Retrieves the ngram dictionary.
        /// </summary>
        /// <returns>ngram dictionary or null if not used</returns>
        internal Opennlp.Tools.Dictionary.Dictionary GetNgramDictionary()
        {
            if (GetFactory() != null)
                return GetFactory().GetDictionary();
            return null;
        }

        public Type GetArtifactSerializerClass()
        {
            return typeof(POSModelSerializer);
        }
    }
}

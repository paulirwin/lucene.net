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
using Opennlp.Tools.Ml;
using Opennlp.Tools.Ml.Model;
using Opennlp.Tools.Util;
using Opennlp.Tools.Util.Model;
using System.Collections.Generic;
using System.IO;
using Lucene.Net.Analysis.OpenNlp.Upstream.Support;
using Lucene.Net.Support;
using InvalidFormatException = Opennlp.Tools.Util.InvalidFormatException;

namespace Opennlp.Tools.Chunker
{
    /// <summary>
    /// The {@link ChunkerModel} is the model used
    /// by a learnable {@link Chunker}.
    /// </summary>
    /// <remarks>@seeChunkerME</remarks>
    public class ChunkerModel : BaseModel
    {
        private static readonly string COMPONENT_NAME = "ChunkerME";
        private static readonly string CHUNKER_MODEL_ENTRY_NAME = "chunker.model";
        internal ChunkerModel(string languageCode, SequenceClassificationModel<string> chunkerModel, Dictionary<string, string> manifestInfoEntries, ChunkerFactory factory) : base(COMPONENT_NAME, languageCode, manifestInfoEntries, factory)
        {
            artifactMap.Put(CHUNKER_MODEL_ENTRY_NAME, chunkerModel);
            CheckArtifactMap();
        }

        internal ChunkerModel(string languageCode, MaxentModel chunkerModel, Dictionary<string, string> manifestInfoEntries, ChunkerFactory factory) : this(languageCode, chunkerModel, ChunkerME.DEFAULT_BEAM_SIZE, manifestInfoEntries, factory)
        {
        }

        internal ChunkerModel(string languageCode, MaxentModel chunkerModel, int beamSize, Dictionary<string, string> manifestInfoEntries, ChunkerFactory factory) : base(COMPONENT_NAME, languageCode, manifestInfoEntries, factory)
        {
            artifactMap.Put(CHUNKER_MODEL_ENTRY_NAME, chunkerModel);
            Properties manifest = (Properties)artifactMap[MANIFEST_ENTRY];
            manifest[BeamSearch.BEAM_SIZE_PARAMETER] = beamSize.ToString();
            CheckArtifactMap();
        }

        internal ChunkerModel(string languageCode, MaxentModel chunkerModel, ChunkerFactory factory) : this(languageCode, chunkerModel, null, factory)
        {
        }

        public ChunkerModel(Stream @in) : base(COMPONENT_NAME, @in)
        {
        }

        public ChunkerModel(FileInfo modelFile) : base(COMPONENT_NAME, modelFile)
        {
        }

        public ChunkerModel(string modelPath) : this(new FileInfo(modelPath))
        {
        }

        // public ChunkerModel(Uri modelURL) : base(COMPONENT_NAME, modelURL)
        // {
        // }

        protected override void ValidateArtifactMap()
        {
            base.ValidateArtifactMap();
            if (!(artifactMap[CHUNKER_MODEL_ENTRY_NAME] is AbstractModel))
            {
                throw new InvalidFormatException("Chunker model is incomplete!");
            }


            // Since 1.8.0 we changed the ChunkerFactory signature. This will check the if the model
            // declares a not default factory, and if yes, check if it was created before 1.8
            if ((GetManifestProperty(FACTORY_NAME) != null && !GetManifestProperty(FACTORY_NAME).Equals("opennlp.tools.chunker.ChunkerFactory")) && this.GetVersion().GetMajor() <= 1 && this.GetVersion().GetMinor() < 8)
            {
                throw new InvalidFormatException("The Chunker factory '" + GetManifestProperty(FACTORY_NAME) + "' is no longer compatible. Please update it to match the latest ChunkerFactory.");
            }
        }

        /// <summary>
        /// </summary>
        /// <remarks>@deprecateduse getChunkerSequenceModel instead. This method will be removed soon.</remarks>
        internal virtual MaxentModel GetChunkerModel()
        {
            if (artifactMap[CHUNKER_MODEL_ENTRY_NAME] is MaxentModel)
            {
                return (MaxentModel)artifactMap[CHUNKER_MODEL_ENTRY_NAME];
            }
            else
            {
                return null;
            }
        }

        internal virtual SequenceClassificationModel<TokenTag> GetChunkerSequenceModel()
        {
            Properties manifest = (Properties)artifactMap[MANIFEST_ENTRY];
            if (artifactMap[CHUNKER_MODEL_ENTRY_NAME] is MaxentModel)
            {
                string beamSizeString = manifest.GetProperty(BeamSearch.BEAM_SIZE_PARAMETER);
                int beamSize = ChunkerME.DEFAULT_BEAM_SIZE;
                if (beamSizeString != null)
                {
                    beamSize = int.Parse(beamSizeString);
                }

                return new BeamSearch<TokenTag>(beamSize, (MaxentModel)artifactMap[CHUNKER_MODEL_ENTRY_NAME]);
            }
            else if (artifactMap[CHUNKER_MODEL_ENTRY_NAME] is SequenceClassificationModel<TokenTag>)
            {
                return (SequenceClassificationModel<TokenTag>)artifactMap[CHUNKER_MODEL_ENTRY_NAME];
            }
            else
            {
                return null;
            }
        }

        protected override Type GetDefaultFactory()
        {
            return typeof(ChunkerFactory);
        }

        internal virtual ChunkerFactory GetFactory()
        {
            return (ChunkerFactory)this.toolFactory;
        }
    }
}

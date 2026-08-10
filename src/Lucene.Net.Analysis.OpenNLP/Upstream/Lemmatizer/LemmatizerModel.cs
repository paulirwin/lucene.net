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

namespace Opennlp.Tools.Lemmatizer
{
    /// <summary>
    /// The {@link LemmatizerModel} is the model used
    /// by a learnable {@link Lemmatizer}.
    /// </summary>
    /// <remarks>@seeLemmatizerME</remarks>
    public class LemmatizerModel : BaseModel
    {
        private static readonly string COMPONENT_NAME = "StatisticalLemmatizer";
        private static readonly string LEMMATIZER_MODEL_ENTRY_NAME = "lemmatizer.model";
        internal LemmatizerModel(string languageCode, SequenceClassificationModel<string> lemmatizerModel, Dictionary<string, string> manifestInfoEntries, LemmatizerFactory factory) : base(COMPONENT_NAME, languageCode, manifestInfoEntries, factory)
        {
            artifactMap.Put(LEMMATIZER_MODEL_ENTRY_NAME, lemmatizerModel);
            CheckArtifactMap();
        }

        internal LemmatizerModel(string languageCode, MaxentModel lemmatizerModel, Dictionary<string, string> manifestInfoEntries, LemmatizerFactory factory) : this(languageCode, lemmatizerModel, LemmatizerME.DEFAULT_BEAM_SIZE, manifestInfoEntries, factory)
        {
        }

        internal LemmatizerModel(string languageCode, MaxentModel lemmatizerModel, int beamSize, Dictionary<string, string> manifestInfoEntries, LemmatizerFactory factory) : base(COMPONENT_NAME, languageCode, manifestInfoEntries, factory)
        {
            artifactMap.Put(LEMMATIZER_MODEL_ENTRY_NAME, lemmatizerModel);
            Properties manifest = (Properties)artifactMap[MANIFEST_ENTRY];
            manifest.Put(BeamSearch.BEAM_SIZE_PARAMETER, beamSize.ToString());
            CheckArtifactMap();
        }

        internal LemmatizerModel(string languageCode, MaxentModel lemmatizerModel, LemmatizerFactory factory) : this(languageCode, lemmatizerModel, null, factory)
        {
        }

        public LemmatizerModel(Stream @in) : base(COMPONENT_NAME, @in)
        {
        }

        public LemmatizerModel(FileInfo modelFile) : base(COMPONENT_NAME, modelFile)
        {
        }

        public LemmatizerModel(string modelPath) : this(new FileInfo(modelPath))
        {
        }

        // public LemmatizerModel(URL modelURL) : base(COMPONENT_NAME, modelURL)
        // {
        // }

        protected override void ValidateArtifactMap()
        {
            base.ValidateArtifactMap();
            if (!(artifactMap[LEMMATIZER_MODEL_ENTRY_NAME] is AbstractModel))
            {
                throw new InvalidFormatException("Lemmatizer model is incomplete!");
            }
        }

        internal virtual SequenceClassificationModel<string> GetLemmatizerSequenceModel()
        {
            Properties manifest = (Properties)artifactMap[MANIFEST_ENTRY];
            if (artifactMap[LEMMATIZER_MODEL_ENTRY_NAME] is MaxentModel)
            {
                string beamSizeString = manifest.GetProperty(BeamSearch.BEAM_SIZE_PARAMETER);
                int beamSize = LemmatizerME.DEFAULT_BEAM_SIZE;
                if (beamSizeString != null)
                {
                    beamSize = int.Parse(beamSizeString);
                }

                return new BeamSearch<string>(beamSize, (MaxentModel)artifactMap[LEMMATIZER_MODEL_ENTRY_NAME]);
            }
            else if (artifactMap[LEMMATIZER_MODEL_ENTRY_NAME] is SequenceClassificationModel<string>)
            {
                return (SequenceClassificationModel<string>)artifactMap[LEMMATIZER_MODEL_ENTRY_NAME];
            }
            else
            {
                return null;
            }
        }

        protected override Type GetDefaultFactory()
        {
            return typeof(LemmatizerFactory);
        }

        internal virtual LemmatizerFactory GetFactory()
        {
            return (LemmatizerFactory)this.toolFactory;
        }
    }
}

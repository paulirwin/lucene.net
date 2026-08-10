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
using Opennlp.Tools.Util;
using Opennlp.Tools.Util.Model;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Opennlp.Tools.Sentdetect
{
    /// <summary>
    /// The {@link SentenceModel} is the model used
    /// by a learnable {@link SentenceDetector}.
    /// </summary>
    /// <remarks>@seeSentenceDetectorME</remarks>
    public class SentenceModel : BaseModel
    {
        private static readonly string COMPONENT_NAME = "SentenceDetectorME";
        private static readonly string MAXENT_MODEL_ENTRY_NAME = "sent.model";
        internal SentenceModel(string languageCode, MaxentModel sentModel, Dictionary<string, string> manifestInfoEntries, SentenceDetectorFactory sdFactory) : base(COMPONENT_NAME, languageCode, manifestInfoEntries, sdFactory)
        {
            artifactMap.Put(MAXENT_MODEL_ENTRY_NAME, sentModel);
            CheckArtifactMap();
        }

        /// <summary>
        /// TODO: was added in 1.5.3 -&gt; remove
        /// </summary>
        /// <remarks>
        /// @deprecatedUse
        ///             {@link #SentenceModel(String, MaxentModel, Map, SentenceDetectorFactory)}
        ///             instead and pass in a {@link SentenceDetectorFactory}
        /// </remarks>
        internal SentenceModel(string languageCode, MaxentModel sentModel, bool useTokenEnd, Opennlp.Tools.Dictionary.Dictionary abbreviations, char[] eosCharacters, Dictionary<string, string> manifestInfoEntries) : this(languageCode, sentModel, manifestInfoEntries, new SentenceDetectorFactory(languageCode, useTokenEnd, abbreviations, eosCharacters))
        {
        }

        /// <summary>
        /// TODO: was added in 1.5.3 -&gt; remove
        /// </summary>
        /// <remarks>
        /// @deprecatedUse
        ///             {@link #SentenceModel(String, MaxentModel, Map, SentenceDetectorFactory)}
        ///             instead and pass in a {@link SentenceDetectorFactory}
        /// </remarks>
        internal SentenceModel(string languageCode, MaxentModel sentModel, bool useTokenEnd, Opennlp.Tools.Dictionary.Dictionary abbreviations, char[] eosCharacters) : this(languageCode, sentModel, useTokenEnd, abbreviations, eosCharacters, null)
        {
        }

        internal SentenceModel(string languageCode, MaxentModel sentModel, bool useTokenEnd, Opennlp.Tools.Dictionary.Dictionary abbreviations, Dictionary<string, string> manifestInfoEntries) : this(languageCode, sentModel, useTokenEnd, abbreviations, null, manifestInfoEntries)
        {
        }

        internal SentenceModel(string languageCode, MaxentModel sentModel, bool useTokenEnd, Opennlp.Tools.Dictionary.Dictionary abbreviations) : this(languageCode, sentModel, useTokenEnd, abbreviations, null, null)
        {
        }

        public SentenceModel(Stream @in) : base(COMPONENT_NAME, @in)
        {
        }

        public SentenceModel(FileInfo modelFile) : base(COMPONENT_NAME, modelFile)
        {
        }

        // LUCENENET: the Path and URL overloads have no .NET equivalent in BaseModel.
        // public SentenceModel(Path modelPath) : this(modelPath.ToFile())
        // {
        // }

        // public SentenceModel(URL modelURL) : base(COMPONENT_NAME, modelURL)
        // {
        // }

        protected override void ValidateArtifactMap()
        {
            base.ValidateArtifactMap();
            if (!(artifactMap[MAXENT_MODEL_ENTRY_NAME] is MaxentModel))
            {
                throw new InvalidFormatException("Unable to find " + MAXENT_MODEL_ENTRY_NAME + " maxent model!");
            }

            if (!ModelUtil.ValidateOutcomes(GetMaxentModel(), SentenceDetectorME.SPLIT, SentenceDetectorME.NO_SPLIT))
            {
                throw new InvalidFormatException("The maxent model is not compatible " + "with the sentence detector!");
            }
        }

        internal virtual SentenceDetectorFactory GetFactory()
        {
            return (SentenceDetectorFactory)this.toolFactory;
        }

        protected override Type GetDefaultFactory()
        {
            return typeof(SentenceDetectorFactory);
        }

        internal virtual MaxentModel GetMaxentModel()
        {
            return (MaxentModel)artifactMap[MAXENT_MODEL_ENTRY_NAME];
        }

        internal virtual Opennlp.Tools.Dictionary.Dictionary GetAbbreviations()
        {
            if (GetFactory() != null)
            {
                return GetFactory().GetAbbreviationDictionary();
            }

            return null;
        }

        public virtual bool UseTokenEnd()
        {
            return GetFactory() == null || GetFactory().IsUseTokenEnd();
        }

        public virtual char[] GetEosCharacters()
        {
            if (GetFactory() != null)
            {
                return GetFactory().GetEOSCharacters();
            }

            return null;
        }
    }
}

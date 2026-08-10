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

namespace Opennlp.Tools.Tokenize
{
    /// <summary>
    /// The {@link TokenizerModel} is the model used
    /// by a learnable {@link Tokenizer}.
    /// </summary>
    /// <remarks>@seeTokenizerME</remarks>
    public sealed class TokenizerModel : BaseModel
    {
        private static readonly string COMPONENT_NAME = "TokenizerME";
        private static readonly string TOKENIZER_MODEL_ENTRY = "token.model";
        /// <summary>
        /// Initializes the current instance.
        /// </summary>
        /// <param name="tokenizerModel">the model</param>
        /// <param name="manifestInfoEntries">the manifest</param>
        /// <param name="tokenizerFactory">the factory</param>
        internal TokenizerModel(MaxentModel tokenizerModel, Dictionary<string, string> manifestInfoEntries, TokenizerFactory tokenizerFactory) : base(COMPONENT_NAME, tokenizerFactory.GetLanguageCode(), manifestInfoEntries, tokenizerFactory)
        {
            artifactMap.Put(TOKENIZER_MODEL_ENTRY, tokenizerModel);
            CheckArtifactMap();
        }

        /// <summary>
        /// Initializes the current instance.
        /// </summary>
        /// <param name="in">the Input Stream to load the model from</param>
        /// <exception cref="IOException">if reading from the stream fails in anyway</exception>
        /// <exception cref="InvalidFormatException">if the stream doesn't have the expected format</exception>
        public TokenizerModel(Stream @in) : base(COMPONENT_NAME, @in)
        {
        }

        /// <summary>
        /// Initializes the current instance.
        /// </summary>
        /// <param name="modelFile">the file containing the tokenizer model</param>
        /// <exception cref="IOException">if reading from the stream fails in anyway</exception>
        public TokenizerModel(FileInfo modelFile) : base(COMPONENT_NAME, modelFile)
        {
        }

        // LUCENENET: the Path and URL overloads have no .NET equivalent in BaseModel.
        // public TokenizerModel(Path modelPath) : this(modelPath.ToFile())
        // {
        // }

        /// <summary>
        /// Initializes the current instance.
        /// </summary>
        /// <param name="modelURL">the URL pointing to the tokenizer model</param>
        /// <exception cref="IOException">if reading from the stream fails in anyway</exception>
        // public TokenizerModel(URL modelURL) : base(COMPONENT_NAME, modelURL)
        // {
        // }

        /// <summary>
        /// Checks if the tokenizer model has the right outcomes.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private static bool IsModelCompatible(MaxentModel model)
        {
            return ModelUtil.ValidateOutcomes(model, TokenizerME.SPLIT, TokenizerME.NO_SPLIT);
        }

        protected override void ValidateArtifactMap()
        {
            base.ValidateArtifactMap();
            if (!(artifactMap[TOKENIZER_MODEL_ENTRY] is AbstractModel))
            {
                throw new InvalidFormatException("Token model is incomplete!");
            }

            if (!IsModelCompatible(GetMaxentModel()))
            {
                throw new InvalidFormatException("The maxent model is not compatible with the tokenizer!");
            }
        }

        internal TokenizerFactory GetFactory()
        {
            return (TokenizerFactory)this.toolFactory;
        }

        protected override Type GetDefaultFactory()
        {
            return typeof(TokenizerFactory);
        }

        internal MaxentModel GetMaxentModel()
        {
            return (MaxentModel)artifactMap[TOKENIZER_MODEL_ENTRY];
        }

        internal Opennlp.Tools.Dictionary.Dictionary GetAbbreviations()
        {
            if (GetFactory() != null)
            {
                return GetFactory().GetAbbreviationDictionary();
            }

            return null;
        }

        public bool UseAlphaNumericOptimization()
        {
            return GetFactory() != null && GetFactory().IsUseAlphaNumericOptmization();
        }
    }
}

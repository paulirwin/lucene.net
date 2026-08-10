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
using Opennlp.Tools.Tokenize;
using Opennlp.Tools.Util;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Opennlp.Tools.Util.Featuregen
{
    /// <summary>
    /// Partitions tokens into sub-tokens based on character classes and generates
    /// class features for each of the sub-tokens and combinations of those sub-tokens.
    /// </summary>
    internal class TokenPatternFeatureGenerator : AdaptiveFeatureGenerator
    {
        private Regex noLetters = new Regex("[^a-zA-Z]");
        private Tokenizer tokenizer;
        /// <summary>
        /// Initializes a new instance.
        /// For tokinization the {@link SimpleTokenizer} is used.
        /// </summary>
        public TokenPatternFeatureGenerator() : this(SimpleTokenizer.INSTANCE)
        {
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="supportTokenizer"></param>
        public TokenPatternFeatureGenerator(Tokenizer supportTokenizer)
        {
            tokenizer = supportTokenizer;
        }

        public virtual void CreateFeatures(IList<string> feats, string[] toks, int index, string[] preds)
        {
            string[] tokenized = tokenizer.Tokenize(toks[index]);
            if (tokenized.Length == 1)
            {
                feats.Add("st=" + StringUtil.ToLowerCase(toks[index]));
                return;
            }

            feats.Add("stn=" + tokenized.Length);
            StringBuilder pattern = new StringBuilder();
            for (int i = 0; i < tokenized.Length; i++)
            {
                if (i < tokenized.Length - 1)
                {
                    feats.Add("pt2=" + FeatureGeneratorUtil.TokenFeature(tokenized[i]) + FeatureGeneratorUtil.TokenFeature(tokenized[i + 1]));
                }

                if (i < tokenized.Length - 2)
                {
                    feats.Add("pt3=" + FeatureGeneratorUtil.TokenFeature(tokenized[i]) + FeatureGeneratorUtil.TokenFeature(tokenized[i + 1]) + FeatureGeneratorUtil.TokenFeature(tokenized[i + 2]));
                }

                pattern.Append(FeatureGeneratorUtil.TokenFeature(tokenized[i]));
                if (!noLetters.IsMatch(tokenized[i]))
                {
                    feats.Add("st=" + StringUtil.ToLowerCase(tokenized[i]));
                }
            }

            feats.Add("pta=" + pattern.ToString());
        }

        // LUCENENET: AdaptiveFeatureGenerator declares these as Java 8 default
        // methods; C# default interface implementations are unavailable on
        // netstandard2.0/net462, so the empty bodies are supplied here.
        public virtual void UpdateAdaptiveData(string[] tokens, string[] outcomes)
        {
        }

        public virtual void ClearAdaptiveData()
        {
        }
    }
}

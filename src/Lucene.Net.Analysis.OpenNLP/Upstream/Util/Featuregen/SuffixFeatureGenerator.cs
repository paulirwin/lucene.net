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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Opennlp.Tools.Util.Featuregen
{
    internal class SuffixFeatureGenerator : AdaptiveFeatureGenerator
    {
        public static readonly int DEFAULT_MAX_LENGTH = 4;
        private readonly int suffixLength;
        public SuffixFeatureGenerator()
        {
            suffixLength = DEFAULT_MAX_LENGTH;
        }

        public SuffixFeatureGenerator(int suffixLength)
        {
            this.suffixLength = suffixLength;
        }

        public virtual void CreateFeatures(IList<string> features, string[] tokens, int index, string[] previousOutcomes)
        {
            string[] suffs = GetSuffixes(tokens[index]);
            foreach (string suff in suffs)
            {
                features.Add("suf=" + suff);
            }
        }

        private String[] GetSuffixes(string lex)
        {
            int suffixes = Math.Min(suffixLength, lex.Length);
            string[] suffs = new string[suffixes];
            for (int li = 0; li < suffixes; li++)
            {
                suffs[li] = lex.Substring(Math.Max(lex.Length - li - 1, 0));
            }

            return suffs;
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

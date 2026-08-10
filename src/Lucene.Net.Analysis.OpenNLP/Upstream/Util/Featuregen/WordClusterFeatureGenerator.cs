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
using Opennlp.Tools.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Opennlp.Tools.Util.Featuregen
{
    internal class WordClusterFeatureGenerator : AdaptiveFeatureGenerator
    {
        private WordClusterDictionary tokenDictionary;
        private string resourceName;
        private bool lowerCaseDictionary;
        public WordClusterFeatureGenerator(WordClusterDictionary dict, string dictResourceKey, bool lowerCaseDictionary)
        {
            tokenDictionary = dict;
            resourceName = dictResourceKey;
            this.lowerCaseDictionary = lowerCaseDictionary;
        }

        public virtual void CreateFeatures(IList<string> features, string[] tokens, int index, string[] previousOutcomes)
        {
            string clusterId;
            if (lowerCaseDictionary)
            {
                clusterId = tokenDictionary.LookupToken(StringUtil.ToLowerCase(tokens[index]));
            }
            else
            {
                clusterId = tokenDictionary.LookupToken(tokens[index]);
            }

            if (clusterId != null)
            {
                features.Add(resourceName + clusterId);
            }
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

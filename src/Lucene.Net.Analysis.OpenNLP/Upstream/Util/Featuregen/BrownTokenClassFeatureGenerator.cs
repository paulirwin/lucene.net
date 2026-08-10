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
    /// <summary>
    /// Generates Brown cluster features for current token and token class.
    /// </summary>
    internal class BrownTokenClassFeatureGenerator : AdaptiveFeatureGenerator
    {
        private BrownCluster brownLexicon;
        public BrownTokenClassFeatureGenerator(BrownCluster dict)
        {
            this.brownLexicon = dict;
        }

        public virtual void CreateFeatures(IList<string> features, string[] tokens, int index, string[] previousOutcomes)
        {
            string wordShape = FeatureGeneratorUtil.TokenFeature(tokens[index]);
            IList<string> wordClasses = BrownTokenClasses.GetWordClasses(tokens[index], brownLexicon);
            for (int i = 0; i < wordClasses.Count; i++)
            {
                features.Add("c," + "browncluster" + "=" + wordShape + "," + wordClasses[i]);
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

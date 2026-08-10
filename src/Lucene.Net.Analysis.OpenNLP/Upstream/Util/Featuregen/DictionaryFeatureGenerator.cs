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
using Opennlp.Tools.Dictionary;
using Opennlp.Tools.Namefind;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Opennlp.Tools.Util.Featuregen
{
    /// <summary>
    /// The {@link DictionaryFeatureGenerator} uses the {@link DictionaryNameFinder}
    /// to generated features for detected names based on the {@link InSpanGenerator}.
    /// </summary>
    /// <remarks>
    /// @seeDictionary
    /// @seeDictionaryNameFinder
    /// @seeInSpanGenerator
    /// </remarks>
    internal class DictionaryFeatureGenerator : AdaptiveFeatureGenerator
    {
        private InSpanGenerator isg;
        public DictionaryFeatureGenerator(Opennlp.Tools.Dictionary.Dictionary dict) : this("", dict)
        {
        }

        public DictionaryFeatureGenerator(string prefix, Opennlp.Tools.Dictionary.Dictionary dict)
        {
            SetDictionary(prefix, dict);
        }

        public virtual void SetDictionary(Opennlp.Tools.Dictionary.Dictionary dict)
        {
            SetDictionary("", dict);
        }

        public virtual void SetDictionary(string name, Opennlp.Tools.Dictionary.Dictionary dict)
        {
            isg = new InSpanGenerator(name, new DictionaryNameFinder(dict));
        }

        public virtual void CreateFeatures(IList<string> features, string[] tokens, int index, string[] previousOutcomes)
        {
            isg.CreateFeatures(features, tokens, index, previousOutcomes);
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

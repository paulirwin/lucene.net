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
using Opennlp.Tools.Postag;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Opennlp.Tools.Util.Featuregen
{
    /// <summary>
    /// Adds the token POS Tag as feature. Requires a POS Tag model.
    /// </summary>
    internal class POSTaggerNameFeatureGenerator : AdaptiveFeatureGenerator
    {
        private POSTagger posTagger;
        private string[] cachedTokens;
        private string[] cachedTags;
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="aPosTagger">a POSTagger implementation.</param>
        public POSTaggerNameFeatureGenerator(POSTagger aPosTagger)
        {
            this.posTagger = aPosTagger;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="aPosModel">a POSTagger model.</param>
        public POSTaggerNameFeatureGenerator(POSModel aPosModel)
        {
            this.posTagger = new POSTaggerME(aPosModel);
        }

        public virtual void CreateFeatures(IList<string> feats, string[] toks, int index, string[] preds)
        {
            if (!Arrays.Equals(this.cachedTokens, toks))
            {
                this.cachedTokens = toks;
                this.cachedTags = this.posTagger.Tag(toks);
            }

            feats.Add("pos=" + this.cachedTags[index]);
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

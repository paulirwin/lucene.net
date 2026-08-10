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
using Opennlp.Tools.Util;
using Opennlp.Tools.Util.Featuregen;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Opennlp.Tools.Postag
{
    /// <summary>
    /// A context generator for the POS Tagger.
    /// </summary>
    internal class ConfigurablePOSContextGenerator : POSContextGenerator
    {
        private Cache<String, String[]> contextsCache;
        private object wordsKey;
        private readonly AdaptiveFeatureGenerator featureGenerator;
        /// <summary>
        /// Initializes the current instance.
        /// </summary>
        /// <param name="cacheSize"></param>
        public ConfigurablePOSContextGenerator(int cacheSize, AdaptiveFeatureGenerator featureGenerator)
        {
            this.featureGenerator = featureGenerator ?? throw new ArgumentNullException(nameof(featureGenerator));
            if (cacheSize > 0)
            {
                contextsCache = new Cache<string, string[]>(cacheSize);
            }
        }

        /// <summary>
        /// Initializes the current instance.
        /// </summary>
        public ConfigurablePOSContextGenerator(AdaptiveFeatureGenerator featureGenerator) : this(0, featureGenerator)
        {
        }

        /// <summary>
        /// Returns the context for making a pos tag decision at the specified token index
        /// given the specified tokens and previous tags.
        /// </summary>
        /// <param name="index">The index of the token for which the context is provided.</param>
        /// <param name="tokens">The tokens in the sentence.</param>
        /// <param name="tags">The tags assigned to the previous words in the sentence.</param>
        /// <returns>The context for making a pos tag decision at the specified token index
        ///     given the specified tokens and previous tags.</returns>
        public virtual String[] GetContext(int index, string[] tokens, string[] tags, object[] additionalContext)
        {
            string tagprev = null;
            string tagprevprev = null;
            if (index - 1 >= 0)
            {
                tagprev = tags[index - 1];
                if (index - 2 >= 0)
                {
                    tagprevprev = tags[index - 2];
                }
            }

            string cacheKey = index + tagprev + tagprevprev;
            if (contextsCache != null)
            {
                if (wordsKey == tokens)
                {
                    string[] cachedContexts = contextsCache[cacheKey];
                    if (cachedContexts != null)
                    {
                        return cachedContexts;
                    }
                }
                else
                {
                    contextsCache.Clear();
                    wordsKey = tokens;
                }
            }

            IList<string> e = new List<string>();
            featureGenerator.CreateFeatures(e, tokens, index, tags);
            string[] contexts = e.ToArray();
            if (contextsCache != null)
            {
                contextsCache.Put(cacheKey, contexts);
            }

            return contexts;
        }
    }
}

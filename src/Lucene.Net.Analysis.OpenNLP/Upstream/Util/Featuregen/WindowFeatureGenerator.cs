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

using System.Collections.Generic;

namespace Opennlp.Tools.Util.Featuregen
{
    /// <summary>
    /// Generates previous and next features for a given {@link AdaptiveFeatureGenerator}.
    /// The window size can be specified.
    ///
    /// Features:
    /// Current token is always included unchanged
    /// Previous tokens are prefixed with p distance
    /// Next tokens are prefix with n distance
    /// </summary>
    public class WindowFeatureGenerator : AdaptiveFeatureGenerator
    {
        public static readonly string PREV_PREFIX = "p";
        public static readonly string NEXT_PREFIX = "n";
        private readonly AdaptiveFeatureGenerator generator;
        private readonly int prevWindowSize;
        private readonly int nextWindowSize;
        /// <summary>
        /// Initializes the current instance with the given parameters.
        /// </summary>
        /// <param name="generator">Feature generator to apply to the window.</param>
        /// <param name="prevWindowSize">Size of the window to the left of the current token.</param>
        /// <param name="nextWindowSize">Size of the window to the right of the current token.</param>
        public WindowFeatureGenerator(AdaptiveFeatureGenerator generator, int prevWindowSize, int nextWindowSize)
        {
            this.generator = generator;
            this.prevWindowSize = prevWindowSize;
            this.nextWindowSize = nextWindowSize;
        }

        /// <summary>
        /// Initializes the current instance with the given parameters.
        /// </summary>
        /// <param name="prevWindowSize"></param>
        /// <param name="nextWindowSize"></param>
        /// <param name="generators"></param>
        public WindowFeatureGenerator(int prevWindowSize, int nextWindowSize, params AdaptiveFeatureGenerator[] generators) : this(new AggregatedFeatureGenerator(generators), prevWindowSize, nextWindowSize)
        {
        }

        /// <summary>
        /// Initializes the current instance. The previous and next window size is 5.
        /// </summary>
        /// <param name="generator">feature generator</param>
        public WindowFeatureGenerator(AdaptiveFeatureGenerator generator) : this(generator, 5, 5)
        {
        }

        /// <summary>
        /// Initializes the current instance with the given parameters.
        /// </summary>
        /// <param name="generators">array of feature generators</param>
        public WindowFeatureGenerator(params AdaptiveFeatureGenerator[] generators) : this(new AggregatedFeatureGenerator(generators), 5, 5)
        {
        }

        public virtual void CreateFeatures(IList<string> features, string[] tokens, int index, string[] preds)
        {

            // current features
            generator.CreateFeatures(features, tokens, index, preds);

            // previous features
            for (int i = 1; i < prevWindowSize + 1; i++)
            {
                if (index - i >= 0)
                {
                    IList<string> prevFeatures = new List<string>();
                    generator.CreateFeatures(prevFeatures, tokens, index - i, preds);
                    foreach (string prevFeature in prevFeatures)
                    {
                        features.Add(PREV_PREFIX + i + prevFeature);
                    }
                }
            }


            // next features
            for (int i = 1; i < nextWindowSize + 1; i++)
            {
                if (i + index < tokens.Length)
                {
                    IList<string> nextFeatures = new List<string>();
                    generator.CreateFeatures(nextFeatures, tokens, index + i, preds);
                    foreach (string nextFeature in nextFeatures)
                    {
                        features.Add(NEXT_PREFIX + i + nextFeature);
                    }
                }
            }
        }

        public virtual void UpdateAdaptiveData(string[] tokens, string[] outcomes)
        {
            generator.UpdateAdaptiveData(tokens, outcomes);
        }

        public virtual void ClearAdaptiveData()
        {
            generator.ClearAdaptiveData();
        }

        public override string ToString()
        {
            return base.ToString() + ": Prev window size: " + prevWindowSize + ", Next window size: " + nextWindowSize;
        }
    }
}

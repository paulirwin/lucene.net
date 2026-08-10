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
using J2N.Collections.Generic.Extensions;
using Lucene.Net.Util;

namespace Opennlp.Tools.Util.Featuregen
{
    /// <summary>
    /// The {@link AggregatedFeatureGenerator} aggregates a set of
    /// {@link AdaptiveFeatureGenerator}s and calls them to generate the features.
    /// </summary>
    internal class AggregatedFeatureGenerator : AdaptiveFeatureGenerator
    {
        /// <summary>
        /// Contains all aggregated {@link AdaptiveFeatureGenerator}s.
        /// </summary>
        private ICollection<AdaptiveFeatureGenerator> generators;
        /// <summary>
        /// Initializes the current instance.
        /// </summary>
        /// <param name="generators">array of generators, null values are not permitted</param>
        public AggregatedFeatureGenerator(params AdaptiveFeatureGenerator[] generators)
        {
            foreach (AdaptiveFeatureGenerator generator in generators)
            {
                // LUCENENET: ArgumentNullException.ThrowIfNull is net6.0+.
                if (generator is null)
                {
                    throw new ArgumentNullException(nameof(generator), "null values in generators are not permitted");
                }
            }

            this.generators = new List<AdaptiveFeatureGenerator>(generators);
            this.generators = this.generators.AsReadOnly();
        }

        public AggregatedFeatureGenerator(ICollection<AdaptiveFeatureGenerator> generators) : this(generators.ToArray())
        {
        }

        /// <summary>
        /// Calls the {@link AdaptiveFeatureGenerator#clearAdaptiveData()} method
        /// on all aggregated {@link AdaptiveFeatureGenerator}s.
        /// </summary>
        public virtual void ClearAdaptiveData()
        {
            foreach (AdaptiveFeatureGenerator generator in generators)
            {
                generator.ClearAdaptiveData();
            }
        }

        /// <summary>
        /// Calls the {@link AdaptiveFeatureGenerator#createFeatures(List, String[], int, String[])}
        /// method on all aggregated {@link AdaptiveFeatureGenerator}s.
        /// </summary>
        public virtual void CreateFeatures(IList<string> features, string[] tokens, int index, string[] previousOutcomes)
        {
            foreach (AdaptiveFeatureGenerator generator in generators)
            {
                generator.CreateFeatures(features, tokens, index, previousOutcomes);
            }
        }

        /// <summary>
        /// Calls the {@link AdaptiveFeatureGenerator#updateAdaptiveData(String[], String[])}
        /// method on all aggregated {@link AdaptiveFeatureGenerator}s.
        /// </summary>
        public virtual void UpdateAdaptiveData(string[] tokens, string[] outcomes)
        {
            foreach (AdaptiveFeatureGenerator generator in generators)
            {
                generator.UpdateAdaptiveData(tokens, outcomes);
            }
        }

        /// <summary>
        /// Retrieves a {@link Collections} of all aggregated
        /// {@link AdaptiveFeatureGenerator}s.
        /// </summary>
        /// <returns>all aggregated generators</returns>
        public virtual ICollection<AdaptiveFeatureGenerator> GetGenerators()
        {
            return generators;
        }
    }
}

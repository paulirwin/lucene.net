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
using System.Linq;
using System.Xml;
using Lucene.Net.Support;

namespace Opennlp.Tools.Util.Featuregen
{
    /// <summary>
    /// </summary>
    /// <remarks>@seeAggregatedFeatureGenerator</remarks>
    internal class AggregatedFeatureGeneratorFactory : GeneratorFactory.AbstractXmlFeatureGeneratorFactory, GeneratorFactory.XmlFeatureGeneratorFactory
    {
        public AggregatedFeatureGeneratorFactory() : base()
        {
        }

        public virtual AdaptiveFeatureGenerator Create(XmlElement generatorElement, FeatureGeneratorResourceProvider resourceManager)
        {
            ICollection<AdaptiveFeatureGenerator> aggregatedGenerators = new LinkedList<AdaptiveFeatureGenerator>();
            XmlNodeList childNodes = generatorElement.ChildNodes;
            for (int i = 0; i < childNodes.Count; i++)
            {
                XmlNode childNode = childNodes.Item(i);

                if (childNode is XmlElement aggregatedGeneratorElement)
                {
                    aggregatedGenerators.Add(GeneratorFactory.CreateGenerator(aggregatedGeneratorElement, resourceManager));
                }
            }

            return new AggregatedFeatureGenerator(aggregatedGenerators.ToArray());
        }

        internal static void Register(IDictionary<string, GeneratorFactory.XmlFeatureGeneratorFactory> factoryMap)
        {
            factoryMap.Put("generators", new AggregatedFeatureGeneratorFactory());
        }

        public override AdaptiveFeatureGenerator Create()
        {
            IList<AdaptiveFeatureGenerator> aggregatedGenerators = new List<AdaptiveFeatureGenerator>();
            foreach (KeyValuePair<string, object> arg in args)
            {
                if (arg.Key.StartsWith("generator#", StringComparison.Ordinal))
                {
                    aggregatedGenerators.Add((AdaptiveFeatureGenerator)arg.Value);
                }
            }

            return new AggregatedFeatureGenerator(aggregatedGenerators.ToArray());
        }
    }
}

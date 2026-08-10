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
using System;
using System.Xml;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Opennlp.Tools.Util.Featuregen
{
    /// <summary>
    /// </summary>
    /// <remarks>@seeTokenClassFeatureGenerator</remarks>
    internal class TokenClassFeatureGeneratorFactory : GeneratorFactory.AbstractXmlFeatureGeneratorFactory, GeneratorFactory.XmlFeatureGeneratorFactory
    {
        public TokenClassFeatureGeneratorFactory() : base()
        {
        }

        public virtual AdaptiveFeatureGenerator Create(XmlElement generatorElement, FeatureGeneratorResourceProvider resourceManager)
        {
            string attribute = generatorElement.GetAttribute("wordAndClass");

            // Default to true.
            bool generateWordAndClassFeature = true;
            if (!string.Equals(attribute, ""))
            {

                // Anything other than "true" sets it to false.
                if (!"true".Equals(attribute, StringComparison.OrdinalIgnoreCase))
                {
                    generateWordAndClassFeature = false;
                }
            }

            return new TokenClassFeatureGenerator(generateWordAndClassFeature);
        }

        internal static void Register(IDictionary<string, GeneratorFactory.XmlFeatureGeneratorFactory> factoryMap)
        {
            factoryMap.Put("tokenclass", new TokenClassFeatureGeneratorFactory());
        }

        public override AdaptiveFeatureGenerator Create()
        {
            return new TokenClassFeatureGenerator(GetBool("wordAndClass", true));
        }
    }
}

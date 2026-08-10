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
using System.Xml;
using Lucene;
using Lucene.Net.Support;

namespace Opennlp.Tools.Util.Featuregen
{
    /// <summary>
    /// </summary>
    /// <remarks>@seeCharacterNgramFeatureGenerator</remarks>
    internal class CharacterNgramFeatureGeneratorFactory : GeneratorFactory.AbstractXmlFeatureGeneratorFactory, GeneratorFactory.XmlFeatureGeneratorFactory
    {
        public CharacterNgramFeatureGeneratorFactory() : base()
        {
        }

        public virtual AdaptiveFeatureGenerator Create(XmlElement generatorElement, FeatureGeneratorResourceProvider resourceManager)
        {
            string minString = generatorElement.GetAttribute("min");
            int min;
            try
            {
                min = int.Parse(minString);
            }
            catch (System.FormatException e)
            {
                throw new InvalidFormatException("min attribute '" + minString + "' is not a number!", e);
            }

            string maxString = generatorElement.GetAttribute("max");
            int max;
            try
            {
                max = int.Parse(maxString);
            }
            catch (System.FormatException e)
            {
                throw new InvalidFormatException("max attribute '" + maxString + "' is not a number!", e);
            }

            return new CharacterNgramFeatureGenerator(min, max);
        }

        internal static void Register(IDictionary<string, GeneratorFactory.XmlFeatureGeneratorFactory> factoryMap)
        {
            factoryMap.Put("charngram", new CharacterNgramFeatureGeneratorFactory());
        }

        public override AdaptiveFeatureGenerator Create()
        {
            return new CharacterNgramFeatureGenerator(GetInt("min"), GetInt("max"));
        }
    }
}

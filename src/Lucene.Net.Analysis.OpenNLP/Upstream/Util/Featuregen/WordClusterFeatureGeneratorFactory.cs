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
using Opennlp.Tools.Util.Model;
using JCG = J2N.Collections.Generic;
using System;
using System.Xml;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Opennlp.Tools.Util.Featuregen
{
    /// <summary>
    /// Defines a word cluster generator factory; it reads an element containing
    /// 'w2vwordcluster' as a tag name; these clusters are typically produced by
    /// word2vec or clark pos induction systems.
    /// </summary>
    internal class WordClusterFeatureGeneratorFactory : GeneratorFactory.AbstractXmlFeatureGeneratorFactory, GeneratorFactory.XmlFeatureGeneratorFactory
    {
        public WordClusterFeatureGeneratorFactory() : base()
        {
        }

        public virtual AdaptiveFeatureGenerator Create(XmlElement generatorElement, FeatureGeneratorResourceProvider resourceManager)
        {
            string dictResourceKey = generatorElement.GetAttribute("dict");
            bool lowerCaseDictionary = "true".Equals(generatorElement.GetAttribute("lowerCase"));
            object dictResource = resourceManager(dictResourceKey);
            if (!(dictResource is WordClusterDictionary))
            {
                throw new InvalidFormatException("Not a WordClusterDictionary resource for key: " + dictResourceKey);
            }

            return new WordClusterFeatureGenerator((WordClusterDictionary)dictResource, dictResourceKey, lowerCaseDictionary);
        }

        internal static void Register(IDictionary<string, GeneratorFactory.XmlFeatureGeneratorFactory> factoryMap)
        {
            factoryMap.Put("wordcluster", new WordClusterFeatureGeneratorFactory());
        }

        public override AdaptiveFeatureGenerator Create()
        {

            // if resourceManager is null, we don't instantiate
            if (resourceManager == null)
                return null;
            string dictResourceKey = GetStr("dict");
            bool lowerCaseDictionary = GetBool("lowerCase");
            object dictResource = resourceManager(dictResourceKey);
            if (!(dictResource is WordClusterDictionary))
            {
                throw new InvalidFormatException("Not a WordClusterDictionary resource for key: " + dictResourceKey);
            }

            return new WordClusterFeatureGenerator((WordClusterDictionary)dictResource, dictResourceKey, lowerCaseDictionary);
        }

        public override JCG.Dictionary<string, ArtifactSerializer> GetArtifactSerializerMapping()
        {
            JCG.Dictionary<string, ArtifactSerializer> mapping = new JCG.Dictionary<string, ArtifactSerializer>();
            mapping.Put(GetStr("dict"), new WordClusterDictionary.WordClusterDictionarySerializer());
            return mapping;
        }
    }
}

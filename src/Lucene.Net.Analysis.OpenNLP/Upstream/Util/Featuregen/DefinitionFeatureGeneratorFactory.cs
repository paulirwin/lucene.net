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
    /// <remarks>@seeDefinitionFeatureGeneratorFactory</remarks>
    internal class DefinitionFeatureGeneratorFactory : GeneratorFactory.AbstractXmlFeatureGeneratorFactory, GeneratorFactory.XmlFeatureGeneratorFactory
    {
        private static readonly string ELEMENT_NAME = "definition";
        public DefinitionFeatureGeneratorFactory() : base()
        {
        }

        public virtual AdaptiveFeatureGenerator Create(XmlElement generatorElement, FeatureGeneratorResourceProvider resourceManager)
        {
            return new OutcomePriorFeatureGenerator();
        }

        internal static void Register(IDictionary<string, GeneratorFactory.XmlFeatureGeneratorFactory> factoryMap)
        {
            factoryMap.Put(ELEMENT_NAME, new DefinitionFeatureGeneratorFactory());
        }

        public override AdaptiveFeatureGenerator Create()
        {
            return new OutcomePriorFeatureGenerator();
        }
    }
}

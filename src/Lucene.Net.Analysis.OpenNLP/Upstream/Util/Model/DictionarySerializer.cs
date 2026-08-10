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

using System.Collections;
using System.Collections.Generic;
using System.IO;
using Lucene.Net.Support;

namespace Opennlp.Tools.Util.Model
{
    internal class DictionarySerializer : ArtifactSerializer<Opennlp.Tools.Dictionary.Dictionary>
    {
        // LUCENENET: upstream serializes opennlp.tools.dictionary.Dictionary, not a
        // java.util.Dictionary; the converter had mapped this to IDictionary/Hashtable.
        public virtual Opennlp.Tools.Dictionary.Dictionary Create(Stream @in)
        {
            return new Opennlp.Tools.Dictionary.Dictionary(@in);
        }

        // public virtual void Serialize(Opennlp.Tools.Dictionary.Dictionary dictionary, Stream @out)
        // {
        //     dictionary.Serialize(@out);
        // }

        internal static void Register(Dictionary<string, ArtifactSerializer> factories)
        {
            factories.Put("dictionary", new DictionarySerializer());
        }

        // LUCENENET: upstream relies on a default interface implementation to
        // bridge the non-generic ArtifactSerializer; DIMs are unavailable on
        // netstandard2.0/net462, so the bridge is explicit here.
        object ArtifactSerializer.Create(Stream @in) => Create(@in);
    }
}

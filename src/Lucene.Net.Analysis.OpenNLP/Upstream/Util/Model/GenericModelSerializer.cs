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

using Opennlp.Tools.Ml.Model;
using System.Collections.Generic;
using System.IO;

namespace Opennlp.Tools.Util.Model
{
    internal class GenericModelSerializer : ArtifactSerializer<AbstractModel>
    {
        public virtual AbstractModel Create(Stream @in)
        {
            return new GenericModelReader(new BinaryFileDataReader(@in)).GetModel();
        }

        // public virtual void Serialize(AbstractModel artifact, Stream @out)
        // {
        //     ModelUtil.WriteModel(artifact, @out);
        // }

        public static void Register(Dictionary<string, ArtifactSerializer> factories)
        {
            factories["model"] = new GenericModelSerializer();
        }

        // LUCENENET: upstream relies on a default interface implementation to
        // bridge the non-generic ArtifactSerializer; DIMs are unavailable on
        // netstandard2.0/net462, so the bridge is explicit here.
        object ArtifactSerializer.Create(Stream @in) => Create(@in);
    }
}

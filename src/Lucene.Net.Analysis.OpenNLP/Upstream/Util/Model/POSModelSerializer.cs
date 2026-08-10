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
using Opennlp.Tools.Ml;
using Opennlp.Tools.Postag;
using Opennlp.Tools.Util;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Opennlp.Tools.Util.Model
{
    internal class POSModelSerializer : ArtifactSerializer<POSModel>
    {
        public virtual POSModel Create(Stream @in)
        {
            POSModel posModel = new POSModel(new UncloseableInputStream(@in));

            // The 1.6.x models write the non-default beam size into the model itself.
            // In 1.5.x the parser configured the beam size when the model was loaded,
            // this is not possible anymore with the new APIs
            Version version = posModel.GetVersion();
            if (version.GetMajor() == 1 && version.GetMinor() == 5)
            {
                if (posModel.GetManifestProperty(BeamSearch.BEAM_SIZE_PARAMETER) == null)
                {
                    // LUCENENET: this 1.5.x back-compat path rebuilds the model via a
                    // training constructor, which is not supported (inference only).
                    // Dictionary<string, string> manifestInfoEntries = new Dictionary<string, string>();
                    //
                    // // The version in the model must be correct or otherwise version
                    // // dependent code branches in other places fail
                    // manifestInfoEntries.Put("OpenNLP-Version", "1.5.0");
                    // posModel = new POSModel(posModel.GetLanguage(), posModel.GetPosModel(), 10, manifestInfoEntries, posModel.GetFactory());
                }
            }

            return posModel;
        }

        // LUCENENET: serialization is not supported; inference only.
        // public virtual void Serialize(POSModel artifact, Stream @out)
        // {
        //     artifact.Serialize(@out);
        // }

        // LUCENENET: upstream relies on a default interface implementation to
        // bridge the non-generic ArtifactSerializer; DIMs are unavailable on
        // netstandard2.0/net462, so the bridge is explicit here.
        object ArtifactSerializer.Create(Stream @in) => Create(@in);
    }
}

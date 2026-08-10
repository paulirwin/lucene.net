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
using System.Text;

namespace Opennlp.Tools.Util.Featuregen
{
    internal class PosTaggerFeatureGenerator : AdaptiveFeatureGenerator
    {
        private readonly string SB = "S=begin";
        public virtual void CreateFeatures(IList<string> features, string[] tokens, int index, string[] tags)
        {
            string prev, prevprev = null;
            string tagprev, tagprevprev;
            tagprev = tagprevprev = null;
            if (index - 1 >= 0)
            {
                prev = tokens[index - 1];
                tagprev = tags[index - 1];
                if (index - 2 >= 0)
                {
                    prevprev = tokens[index - 2];
                    tagprevprev = tags[index - 2];
                }
                else
                {
                    prevprev = SB;
                }
            }
            else
            {
                prev = SB;
            }


            // add the words and pos's of the surrounding context
            if (prev != null)
            {
                if (tagprev != null)
                {
                    features.Add("t=" + tagprev);
                }

                if (prevprev != null)
                {
                    if (tagprevprev != null)
                    {
                        features.Add("t2=" + tagprevprev + "," + tagprev);
                    }
                }
            }
        }

        // LUCENENET: AdaptiveFeatureGenerator declares these as Java 8 default
        // methods; C# default interface implementations are unavailable on
        // netstandard2.0/net462, so the empty bodies are supplied here.
        public virtual void UpdateAdaptiveData(string[] tokens, string[] outcomes)
        {
        }

        public virtual void ClearAdaptiveData()
        {
        }
    }
}

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
using Opennlp.Tools.Util.Model;
using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Opennlp.Tools.Util.Featuregen
{
    /// <summary>
    /// 
    /// Class to load a Brown cluster document: word\tword_class\tprob
    /// http://metaoptimize.com/projects/wordreprs/
    /// 
    /// The file containing the clustering lexicon has to be passed as the
    /// value of the dict attribute of each BrownCluster feature generator.
    /// </summary>
    internal class BrownCluster : SerializableArtifact
    {
        private static readonly Regex tabPattern = new Regex("\t");
        internal class BrownClusterSerializer : ArtifactSerializer<BrownCluster>
        {
            public virtual BrownCluster Create(Stream @in)
            {
                return new BrownCluster(@in);
            }

            // LUCENENET: serialization is not supported; inference only.
            // public virtual void Serialize(BrownCluster artifact, Stream @out)
            // {
            //     artifact.Serialize(@out);
            // }
            // LUCENENET: upstream relies on a default interface implementation to
            // bridge the non-generic ArtifactSerializer; DIMs are unavailable on
            // netstandard2.0/net462, so the bridge is explicit here.
            object ArtifactSerializer.Create(Stream @in) => Create(@in);
        }

        private Dictionary<string, string> tokenToClusterMap = new Dictionary<string, string>();
        /// <summary>
        /// Generates the token to cluster map from Brown cluster input file.
        /// NOTE: we only add those tokens with frequency bigger than 5.
        /// </summary>
        /// <param name="in">the inputstream</param>
        /// <exception cref="IOException">the io exception</exception>
        public BrownCluster(Stream @in)
        {
            var breader = new StreamReader(@in, Encoding.UTF8);
            string line;
            while ((line = breader.ReadLine()) != null)
            {
                string[] lineArray = tabPattern.Split(line);
                if (lineArray.Length == 3)
                {
                    int freq = int.Parse(lineArray[2]);
                    if (freq > 5)
                    {
                        tokenToClusterMap.Put(lineArray[1], lineArray[0]);
                    }
                }
                else if (lineArray.Length == 2)
                {
                    tokenToClusterMap.Put(lineArray[0], lineArray[1]);
                }
            }
        }

        /// <summary>
        /// Check if a token is in the Brown:paths, token map.
        /// </summary>
        /// <param name="string">the token to look-up</param>
        /// <returns>the brown class if such token is in the brown cluster map</returns>
        public virtual string LookupToken(string @string)
        {
            return tokenToClusterMap.TryGetValue(@string, out string value) ? value : null;
        }

        // LUCENENET: serialization is not supported; inference only.
        // public virtual void Serialize(Stream @out)
        // {
        //     Writer writer = new BufferedWriter(new OutputStreamWriter(@out));
        //     foreach (Map.Entry<String, String> entry in tokenToClusterMap.EntrySet())
        //     {
        //         writer.Write(entry.GetKey() + "\t" + entry.GetValue() + "\n");
        //     }
        //
        //     writer.Flush();
        // }

        public virtual Type GetArtifactSerializerClass()
        {
            return typeof(BrownClusterSerializer);
        }
    }
}

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
    /// <summary>
    /// Obtain the paths listed in the pathLengths array from the Brown class.
    /// This class is not to be instantiated.
    /// </summary>
    internal class BrownTokenClasses
    {
        public static readonly int[] pathLengths = new[]
        {
            4,
            6,
            10,
            20
        };
        /// <summary>
        /// It provides a list containing the pathLengths for a token if found
        /// in the Map:token,BrownClass.
        /// </summary>
        /// <param name="token">the token to be looked up in the brown clustering map</param>
        /// <param name="brownLexicon">the Brown clustering map</param>
        /// <returns>the list of the paths for a token</returns>
        public static IList<string> GetWordClasses(string token, BrownCluster brownLexicon)
        {
            if (brownLexicon.LookupToken(token) == null)
            {
                return new List<string>(0);
            }
            else
            {
                string brownClass = brownLexicon.LookupToken(token);
                IList<string> pathLengthsList = new List<string>();
                pathLengthsList.Add(brownClass.Substring(0, Math.Min(brownClass.Length, pathLengths[0])));
                for (int i = 1; i < pathLengths.Length; i++)
                {
                    if (pathLengths[i - 1] < brownClass.Length)
                    {
                        pathLengthsList.Add(brownClass.Substring(0, Math.Min(brownClass.Length, pathLengths[i])));
                    }
                }

                return pathLengthsList;
            }
        }
    }
}

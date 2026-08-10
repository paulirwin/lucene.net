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
using Opennlp.Tools.Dictionary;
using Opennlp.Tools.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Opennlp.Tools.Namefind
{
    /// <summary>
    /// This is a dictionary based name finder, it scans text
    /// for names inside a dictionary.
    /// </summary>
    internal class DictionaryNameFinder : TokenNameFinder
    {
        private static readonly string DEFAULT_TYPE = "default";
        private Opennlp.Tools.Dictionary.Dictionary mDictionary;
        private readonly string type;
        /// <summary>
        /// Initialized the current instance with he provided dictionary
        /// and a type.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="type">the name type used for the produced spans</param>
        public DictionaryNameFinder(Opennlp.Tools.Dictionary.Dictionary dictionary, string type)
        {
            mDictionary = dictionary;
            this.type = type;
        }

        /// <summary>
        /// Initializes the current instance with the provided dictionary.
        /// </summary>
        /// <param name="dictionary"></param>
        public DictionaryNameFinder(Opennlp.Tools.Dictionary.Dictionary dictionary) : this(dictionary, DEFAULT_TYPE)
        {
        }

        public virtual Span[] Find(string[] textTokenized)
        {
            IList<Span> namesFound = new List<Span>();
            for (int offsetFrom = 0; offsetFrom < textTokenized.Length; offsetFrom++)
            {
                Span nameFound = null;
                string[] tokensSearching;
                for (int offsetTo = offsetFrom; offsetTo < textTokenized.Length; offsetTo++)
                {
                    int lengthSearching = offsetTo - offsetFrom + 1;
                    if (lengthSearching > mDictionary.GetMaxTokenCount())
                    {
                        break;
                    }
                    else
                    {
                        tokensSearching = new string[lengthSearching];
                        Array.Copy(textTokenized, offsetFrom, tokensSearching, 0, lengthSearching);
                        StringList entryForSearch = new StringList(tokensSearching);
                        if (mDictionary.Contains(entryForSearch))
                        {
                            nameFound = new Span(offsetFrom, offsetTo + 1, type);
                        }
                    }
                }

                if (nameFound != null)
                {
                    namesFound.Add(nameFound);

                    // skip over the found tokens for the next search
                    offsetFrom += nameFound.Length() - 1;
                }
            }

            return namesFound.ToArray();
        }

        public virtual void ClearAdaptiveData()
        {
        }
    }
}

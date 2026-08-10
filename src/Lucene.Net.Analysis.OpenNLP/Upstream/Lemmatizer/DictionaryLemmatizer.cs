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
using System.IO;
using System.Linq;
using Lucene.Net.Support;
using Lucene.Net.Util;

namespace Opennlp.Tools.Lemmatizer
{
    /// <summary>
    /// Lemmatize by simple dictionary lookup into a hashmap built from a file
    /// containing, for each line, word\tabpostag\tablemma.
    /// </summary>
    /// <remarks>@version2014-07-08</remarks>
    internal class DictionaryLemmatizer : Lemmatizer
    {
        /// <summary>
        /// The hashmap containing the dictionary.
        /// </summary>
        // LUCENENET: Java's HashMap keys on List.equals/hashCode (value equality), while
        // a .NET Dictionary would use reference equality for IList<string> keys and never
        // find a match. J2N's ListEqualityComparer restores the Java semantics.
        private readonly Dictionary<IList<string>, IList<string>> dictMap =
            new Dictionary<IList<string>, IList<string>>(J2N.Collections.Generic.ListEqualityComparer<string>.Default);
        /// <summary>
        /// Construct a hashmap from the input tab separated dictionary.
        ///
        /// The input file should have, for each line, word\tabpostag\tablemma.
        /// Alternatively, if multiple lemmas are possible for each word,postag pair,
        /// then the format should be word\tab\postag\tablemma01#lemma02#lemma03
        /// </summary>
        /// <param name="dictionary">
        ///          the input dictionary via inputstream</param>
        public DictionaryLemmatizer(Stream dictionary)
        {
            Init(dictionary);
        }

        public DictionaryLemmatizer(FileInfo dictionaryFile)
        {
            using (var @in = dictionaryFile.OpenRead())
            {
                Init(@in);
            }
        }

        public DictionaryLemmatizer(string dictionaryFile) : this(new FileInfo(dictionaryFile))
        {
        }

        private void Init(Stream dictionary)
        {
            using var breader = new StreamReader(dictionary);
            while (breader.ReadLine() is { } line)
            {
                string[] elems = line.Split('\t');
                string[] lemmas = elems[2].Split('#');
                this.dictMap.Put(new List<string> { elems[0], elems[1] }, lemmas);
            }
        }

        /// <summary>
        /// Get the Map containing the dictionary.
        /// </summary>
        /// <returns>dictMap the Map</returns>
        public virtual Dictionary<IList<string>, IList<string>> GetDictMap()
        {
            return this.dictMap;
        }

        /// <summary>
        /// Get the dictionary keys (word and postag).
        /// </summary>
        /// <param name="word">
        ///          the surface form word</param>
        /// <param name="postag">
        ///          the assigned postag</param>
        /// <returns>returns the dictionary keys</returns>
        private IList<string> GetDictKeys(string word, string postag)
        {
            IList<string> keys = new List<string>();
            keys.AddRange(new List<string>() { word.ToLower(), postag });
            return keys;
        }

        public virtual string[] Lemmatize(string[] tokens, string[] postags)
        {
            IList<string> lemmas = new List<string>();
            for (int i = 0; i < tokens.Length; i++)
            {
                lemmas.Add(this.Lemmatize(tokens[i], postags[i]));
            }

            return lemmas.ToArray();
        }

        public virtual IList<IList<string>> Lemmatize(IList<string> tokens, IList<string> posTags)
        {
            IList<IList<string>> allLemmas = new List<IList<string>>();
            for (int i = 0; i < tokens.Count; i++)
            {
                allLemmas.Add(this.GetAllLemmas(tokens[i], posTags[i]));
            }

            return allLemmas;
        }

        /// <summary>
        /// Lookup lemma in a dictionary. Outputs "O" if not found.
        /// </summary>
        /// <param name="word">
        ///          the token</param>
        /// <param name="postag">
        ///          the postag</param>
        /// <returns>the lemma</returns>
        private string Lemmatize(string word, string postag)
        {
            string lemma;
            IList<string> keys = this.GetDictKeys(word, postag);

            // lookup lemma as value of the map
            this.dictMap.TryGetValue(keys, out IList<string> keyValues);
            if (keyValues != null && keyValues.Count > 0)
            {
                lemma = keyValues[0];
            }
            else
            {
                lemma = "O";
            }

            return lemma;
        }

        /// <summary>
        /// Lookup every lemma for a word,pos tag in a dictionary. Outputs "O" if not
        /// found.
        /// </summary>
        /// <param name="word">
        ///          the token</param>
        /// <param name="postag">
        ///          the postag</param>
        /// <returns>every lemma</returns>
        private IList<string> GetAllLemmas(string word, string postag)
        {
            IList<string> lemmasList = new List<string>();
            IList<string> keys = this.GetDictKeys(word, postag);

            // lookup lemma as value of the map
            this.dictMap.TryGetValue(keys, out IList<string> keyValues);
            if (keyValues != null && keyValues.Count > 0)
            {
                lemmasList.AddRange(keyValues);
            }
            else
            {
                lemmasList.Add("O");
            }

            return lemmasList;
        }
    }
}

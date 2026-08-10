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
using Opennlp.Tools.Dictionary;
using Opennlp.Tools.Util;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Opennlp.Tools.Postag
{
    /// <summary>
    /// A context generator for the POS Tagger.
    /// </summary>
    internal class DefaultPOSContextGenerator : POSContextGenerator
    {
        protected readonly string SE = "*SE*";
        protected readonly string SB = "*SB*";
        private static readonly int PREFIX_LENGTH = 4;
        private static readonly int SUFFIX_LENGTH = 4;
        private static Regex hasCap = new Regex("[A-Z]");
        private static Regex hasNum = new Regex("[0-9]");
        private Cache<String, String[]> contextsCache;
        private object wordsKey;
        private Opennlp.Tools.Dictionary.Dictionary dict;
        /// <summary>
        /// Initializes the current instance.
        /// </summary>
        /// <param name="dict"></param>
        public DefaultPOSContextGenerator(Opennlp.Tools.Dictionary.Dictionary dict) : this(0, dict)
        {
        }

        /// <summary>
        /// Initializes the current instance.
        /// </summary>
        /// <param name="cacheSize"></param>
        /// <param name="dict"></param>
        public DefaultPOSContextGenerator(int cacheSize, Opennlp.Tools.Dictionary.Dictionary dict)
        {
            this.dict = dict;
            if (cacheSize > 0)
            {
                contextsCache = new Cache<string, string[]>(cacheSize);
            }
        }

        protected static String[] GetPrefixes(string lex)
        {
            string[] prefs = new string[PREFIX_LENGTH];
            for (int li = 0; li < PREFIX_LENGTH; li++)
            {
                prefs[li] = lex.Substring(0, Math.Min(li + 1, lex.Length));
            }

            return prefs;
        }

        protected static String[] GetSuffixes(string lex)
        {
            string[] suffs = new string[SUFFIX_LENGTH];
            for (int li = 0; li < SUFFIX_LENGTH; li++)
            {
                suffs[li] = lex.Substring(Math.Max(lex.Length - li - 1, 0));
            }

            return suffs;
        }

        public virtual String[] GetContext(int index, string[] sequence, string[] priorDecisions, object[] additionalContext)
        {
            return GetContext(index, sequence, priorDecisions);
        }

        /// <summary>
        /// Returns the context for making a pos tag decision at the specified token index
        /// given the specified tokens and previous tags.
        /// </summary>
        /// <param name="index">The index of the token for which the context is provided.</param>
        /// <param name="tokens">The tokens in the sentence.</param>
        /// <param name="tags">The tags assigned to the previous words in the sentence.</param>
        /// <returns>The context for making a pos tag decision at the specified token index
        ///     given the specified tokens and previous tags.</returns>
        public virtual String[] GetContext(int index, object[] tokens, string[] tags)
        {
            string next, nextnext = null, lex, prev, prevprev = null;
            string tagprev, tagprevprev;
            tagprev = tagprevprev = null;
            lex = tokens[index].ToString();
            if (tokens.Length > index + 1)
            {
                next = tokens[index + 1].ToString();
                if (tokens.Length > index + 2)
                    nextnext = tokens[index + 2].ToString();
                else
                    nextnext = SE; // Sentence End
            }
            else
            {
                next = SE; // Sentence End
            }

            if (index - 1 >= 0)
            {
                prev = tokens[index - 1].ToString();
                tagprev = tags[index - 1];
                if (index - 2 >= 0)
                {
                    prevprev = tokens[index - 2].ToString();
                    tagprevprev = tags[index - 2];
                }
                else
                {
                    prevprev = SB; // Sentence Beginning
                }
            }
            else
            {
                prev = SB; // Sentence Beginning
            }

            string cacheKey = index + tagprev + tagprevprev;
            if (contextsCache != null)
            {
                if (wordsKey == tokens)
                {
                    string[] cachedContexts = contextsCache[cacheKey];
                    if (cachedContexts != null)
                    {
                        return cachedContexts;
                    }
                }
                else
                {
                    contextsCache.Clear();
                    wordsKey = tokens;
                }
            }

            IList<string> e = new List<string>();
            e.Add("default");

            // add the word itself
            e.Add("w=" + lex);
            if (dict == null || !dict.Contains(new StringList(lex)))
            {

                // do some basic suffix analysis
                string[] suffs = GetSuffixes(lex);
                for (int i = 0; i < suffs.Length; i++)
                {
                    e.Add("suf=" + suffs[i]);
                }

                string[] prefs = GetPrefixes(lex);
                for (int i = 0; i < prefs.Length; i++)
                {
                    e.Add("pre=" + prefs[i]);
                }


                // see if the word has any special characters
                if (lex.IndexOf('-') != -1)
                {
                    e.Add("h");
                }

                if (hasCap.IsMatch(lex))
                {
                    e.Add("c");
                }

                if (hasNum.IsMatch(lex))
                {
                    e.Add("d");
                }
            }


            // add the words and pos's of the surrounding context
            if (prev != null)
            {
                e.Add("p=" + prev);
                if (tagprev != null)
                {
                    e.Add("t=" + tagprev);
                }

                if (prevprev != null)
                {
                    e.Add("pp=" + prevprev);
                    if (tagprevprev != null)
                    {
                        e.Add("t2=" + tagprevprev + "," + tagprev);
                    }
                }
            }

            if (next != null)
            {
                e.Add("n=" + next);
                if (nextnext != null)
                {
                    e.Add("nn=" + nextnext);
                }
            }

            string[] contexts = e.ToArray();
            if (contextsCache != null)
            {
                contextsCache.Put(cacheKey, contexts);
            }

            return contexts;
        }
    }
}

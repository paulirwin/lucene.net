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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Opennlp.Tools.Tokenize
{
    /// <summary>
    /// Generate events for maxent decisions for tokenization.
    /// </summary>
    internal class DefaultTokenContextGenerator : TokenContextGenerator
    {
        protected ISet<string> inducedAbbreviations;
        /// <summary>
        /// Creates a default context generator for tokenizer.
        /// </summary>
        public DefaultTokenContextGenerator() : this(new HashSet<string>())
        {
        }

        /// <summary>
        /// Creates a default context generator for tokenizer.
        /// </summary>
        /// <param name="inducedAbbreviations">the induced abbreviations</param>
        public DefaultTokenContextGenerator(ISet<string> inducedAbbreviations)
        {
            this.inducedAbbreviations = inducedAbbreviations;
        }

        /* (non-Javadoc)
         * @see opennlp.tools.tokenize.TokenContextGenerator#getContext(java.lang.String, int)
         */
        public virtual String[] GetContext(string sentence, int index)
        {
            IList<string> preds = CreateContext(sentence, index);
            return preds.ToArray();
        }

        /// <summary>
        /// Returns an {@link ArrayList} of features for the specified sentence string
        /// at the specified index. Extensions of this class can override this method
        /// to create a customized {@link TokenContextGenerator}
        /// </summary>
        /// <param name="sentence">
        ///          the token been analyzed</param>
        /// <param name="index">
        ///          the index of the character been analyzed</param>
        /// <returns>an {@link ArrayList} of features for the specified sentence string
        ///         at the specified index.</returns>
        protected virtual IList<string> CreateContext(string sentence, int index)
        {
            IList<string> preds = new List<string>();
            string prefix = sentence.Substring(0, index);
            string suffix = sentence.Substring(index);
            preds.Add("p=" + prefix);
            preds.Add("s=" + suffix);
            if (index > 0)
            {
                AddCharPreds("p1", sentence[index - 1], preds);
                if (index > 1)
                {
                    AddCharPreds("p2", sentence[index - 2], preds);
                    preds.Add("p21=" + sentence[index - 2] + sentence[index - 1]);
                }
                else
                {
                    preds.Add("p2=bok");
                }

                preds.Add("p1f1=" + sentence[index - 1] + sentence[index]);
            }
            else
            {
                preds.Add("p1=bok");
            }

            AddCharPreds("f1", sentence[index], preds);
            if (index + 1 < sentence.Length)
            {
                AddCharPreds("f2", sentence[index + 1], preds);
                preds.Add("f12=" + sentence[index] + sentence[index + 1]);
            }
            else
            {
                preds.Add("f2=bok");
            }

            if (sentence[0] == '&' && sentence[sentence.Length - 1] == ';')
            {
                preds.Add("cc"); //character code
            }

            if (index == sentence.Length - 1 && inducedAbbreviations.Contains(sentence))
            {
                preds.Add("pabb");
            }

            return preds;
        }

        /// <summary>
        /// Helper function for getContext.
        /// </summary>
        protected virtual void AddCharPreds(string key, char c, IList<string> preds)
        {
            preds.Add(key + "=" + c);
            if (char.IsLetter(c))
            {
                preds.Add(key + "_alpha");
                if (char.IsUpper(c))
                {
                    preds.Add(key + "_caps");
                }
            }
            else if (char.IsDigit(c))
            {
                preds.Add(key + "_num");
            }
            else if (StringUtil.IsWhitespace(c))
            {
                preds.Add(key + "_ws");
            }
            else
            {
                if (c == '.' || c == '?' || c == '!')
                {
                    preds.Add(key + "_eos");
                }
                else if (c == '`' || c == '"' || c == '\'')
                {
                    preds.Add(key + "_quote");
                }
                else if (c == '[' || c == '{' || c == '(')
                {
                    preds.Add(key + "_lp");
                }
                else if (c == ']' || c == '}' || c == ')')
                {
                    preds.Add(key + "_rp");
                }
            }
        }
    }
}

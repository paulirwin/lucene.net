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

namespace Opennlp.Tools.Sentdetect
{
    /// <summary>
    /// Default implementation of the {@link EndOfSentenceScanner}.
    /// It uses an character array with possible end of sentence chars
    /// to identify potential sentence endings.
    /// </summary>
    internal class DefaultEndOfSentenceScanner : EndOfSentenceScanner
    {
        private HashSet<char> eosCharacters;
        private char[] eosChars;
        /// <summary>
        /// Initializes the current instance.
        /// </summary>
        /// <param name="eosCharacters"></param>
        public DefaultEndOfSentenceScanner(char[] eosCharacters)
        {
            this.eosCharacters = new HashSet<char>();
            foreach (char eosChar in eosCharacters)
            {
                this.eosCharacters.Add(eosChar);
            }

            this.eosChars = eosCharacters;
        }

        public virtual IList<int> GetPositions(string s)
        {
            return GetPositions(s.ToCharArray());
        }

        public virtual IList<int> GetPositions(StringBuilder buf)
        {
            return GetPositions(buf.ToString().ToCharArray());
        }

        public virtual IList<int> GetPositions(char[] cbuf)
        {
            IList<int> l = new List<int>();
            for (int i = 0; i < cbuf.Length; i++)
            {
                if (eosCharacters.Contains(cbuf[i]))
                {
                    l.Add(i);
                }
            }

            return l;
        }

        public virtual char[] GetEndOfSentenceCharacters()
        {
            return eosChars;
        }

        public virtual HashSet<char> GetEOSCharacters()
        {
            return eosCharacters;
        }
    }
}

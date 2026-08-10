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
using Opennlp.Tools.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Opennlp.Tools.Tokenize
{
    /// <summary>
    /// Performs tokenization using character classes.
    /// </summary>
    internal class SimpleTokenizer : AbstractTokenizer
    {
        private class CharacterEnum
        {
            internal static readonly CharacterEnum WHITESPACE = new CharacterEnum("whitespace");
            internal static readonly CharacterEnum ALPHABETIC = new CharacterEnum("alphabetic");
            internal static readonly CharacterEnum NUMERIC = new CharacterEnum("numeric");
            internal static readonly CharacterEnum OTHER = new CharacterEnum("other");
            private string name;
            internal CharacterEnum(string name)
            {
                this.name = name;
            }

            public override string ToString()
            {
                return name;
            }
        }

        public static readonly SimpleTokenizer INSTANCE;
        static SimpleTokenizer()
        {
            INSTANCE = new SimpleTokenizer();
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// @deprecatedUse INSTANCE field instead to obtain an instance, constructor
        ///     will be made private in the future.
        /// </remarks>
        public SimpleTokenizer()
        {
        }

        public override Span[] TokenizePos(string s)
        {
            CharacterEnum charType = CharacterEnum.WHITESPACE;
            CharacterEnum state = charType;
            IList<Span> tokens = new List<Span>();
            int sl = s.Length;
            int start = -1;
            char pc = (char)0;
            for (int ci = 0; ci < sl; ci++)
            {
                char c = s[ci];
                if (StringUtil.IsWhitespace(c))
                {
                    charType = CharacterEnum.WHITESPACE;
                }
                else if (char.IsLetter(c))
                {
                    charType = CharacterEnum.ALPHABETIC;
                }
                else if (char.IsDigit(c))
                {
                    charType = CharacterEnum.NUMERIC;
                }
                else
                {
                    charType = CharacterEnum.OTHER;
                }

                if (state == CharacterEnum.WHITESPACE)
                {
                    if (charType != CharacterEnum.WHITESPACE)
                    {
                        start = ci;
                    }
                }
                else
                {
                    if (charType != state || charType == CharacterEnum.OTHER && c != pc)
                    {
                        tokens.Add(new Span(start, ci));
                        start = ci;
                    }
                }

                state = charType;
                pc = c;
            }

            if (charType != CharacterEnum.WHITESPACE)
            {
                tokens.Add(new Span(start, sl));
            }

            return tokens.ToArray();
        }
    }
}

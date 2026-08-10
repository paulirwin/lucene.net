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

using System.Globalization;
using System.Text;
using ICU4N.Globalization;
using J2N;

namespace Opennlp.Tools.Util.Featuregen
{
    /// <summary>
    /// Recognizes predefined patterns in strings.
    /// </summary>
    internal class StringPattern
    {
        private static readonly int INITAL_CAPITAL_LETTER = 0x1;
        private static readonly int ALL_CAPITAL_LETTER = 0x1 << 1;
        private static readonly int ALL_LOWERCASE_LETTER = 0x1 << 2;
        private static readonly int ALL_LETTERS = 0x1 << 3;
        private static readonly int ALL_DIGIT = 0x1 << 4;
        private static readonly int ALL_HIRAGANA = 0x1 << 5;
        private static readonly int ALL_KATAKANA = 0x1 << 6;
        private static readonly int CONTAINS_PERIOD = 0x1 << 7;
        private static readonly int CONTAINS_COMMA = 0x1 << 8;
        private static readonly int CONTAINS_SLASH = 0x1 << 9;
        private static readonly int CONTAINS_DIGIT = 0x1 << 10;
        private static readonly int CONTAINS_HYPHEN = 0x1 << 11;
        private static readonly int CONTAINS_LETTERS = 0x1 << 12;
        private static readonly int CONTAINS_UPPERCASE = 0x1 << 13;
        private readonly int pattern;
        private readonly int digits;
        private StringPattern(int pattern, int digits)
        {
            this.pattern = pattern;
            this.digits = digits;
        }

        public static StringPattern Recognize(string token)
        {
            int pattern = ALL_CAPITAL_LETTER | ALL_LOWERCASE_LETTER | ALL_DIGIT | ALL_LETTERS | ALL_HIRAGANA | ALL_KATAKANA;
            int digits = 0;
            for (int i = 0; i < token.Length; i++)
            {
                char ch = token[i];
                UnicodeCategory letterType = Character.GetType(ch);
                bool isLetter = letterType == UnicodeCategory.UppercaseLetter || letterType == UnicodeCategory.LowercaseLetter || letterType == UnicodeCategory.TitlecaseLetter || letterType == UnicodeCategory.ModifierLetter || letterType == UnicodeCategory.OtherLetter;
                if (isLetter)
                {
                    pattern |= CONTAINS_LETTERS;
                    pattern &= ~ALL_DIGIT;
                    if (letterType == UnicodeCategory.UppercaseLetter)
                    {
                        if (i == 0)
                        {
                            pattern |= INITAL_CAPITAL_LETTER;
                        }

                        pattern |= CONTAINS_UPPERCASE;
                        pattern &= ~ALL_LOWERCASE_LETTER;
                    }
                    else
                    {
                        pattern &= ~ALL_CAPITAL_LETTER;
                    }
                }
                else
                {

                    // contains chars other than letter, this means
                    // it can not be one of these:
                    pattern &= ~ALL_LETTERS;
                    pattern &= ~ALL_CAPITAL_LETTER;
                    pattern &= ~ALL_LOWERCASE_LETTER;
                    if (letterType == UnicodeCategory.DecimalDigitNumber)
                    {
                        pattern |= CONTAINS_DIGIT;
                        pattern &= ~(ALL_HIRAGANA | ALL_KATAKANA);
                        digits++;
                    }
                    else
                    {
                        pattern &= ~ALL_DIGIT;
                    }

                    switch (ch)
                    {
                        case ',':
                            pattern |= CONTAINS_COMMA;
                            break;
                        case '.':
                            pattern |= CONTAINS_PERIOD;
                            break;
                        case '/':
                            pattern |= CONTAINS_SLASH;
                            break;
                        case '-':
                            pattern |= CONTAINS_HYPHEN;
                            break;
                        default:
                            break;
                    }
                }


                // for Japanese...
                int codePoint = token.CodePointAt(i);
                var us = UScript.GetScript(codePoint);
                if (us != UScript.Common)
                {
                    if (us == UScript.Latin)
                    {
                        pattern &= ~(ALL_HIRAGANA | ALL_KATAKANA);
                    }
                    else if (us == UScript.Han)
                    {
                        pattern &= ~(ALL_HIRAGANA | ALL_KATAKANA | ALL_LOWERCASE_LETTER);
                    }
                    else if (us == UScript.Hiragana)
                    {
                        pattern &= ~(ALL_KATAKANA | ALL_LOWERCASE_LETTER);
                    }
                    else if (us == UScript.Katakana)
                    {
                        pattern &= ~(ALL_HIRAGANA | ALL_LOWERCASE_LETTER);
                    }
                }
                else
                {
                    if (ch != '・' && ch != 'ー' && ch != '〜')
                        pattern &= ~(ALL_HIRAGANA | ALL_KATAKANA);
                }
            }

            return new StringPattern(pattern, digits);
        }

        /// <summary>
        /// </summary>
        /// <returns>true if all characters are letters.</returns>
        public virtual bool IsAllLetter()
        {
            return (pattern & ALL_LETTERS) > 0;
        }

        /// <summary>
        /// </summary>
        /// <returns>true if first letter is capital.</returns>
        public virtual bool IsInitialCapitalLetter()
        {
            return (pattern & INITAL_CAPITAL_LETTER) > 0;
        }

        /// <summary>
        /// </summary>
        /// <returns>true if all letters are capital.</returns>
        public virtual bool IsAllCapitalLetter()
        {
            return (pattern & ALL_CAPITAL_LETTER) > 0;
        }

        /// <summary>
        /// </summary>
        /// <returns>true if all letters are lower case.</returns>
        public virtual bool IsAllLowerCaseLetter()
        {
            return (pattern & ALL_LOWERCASE_LETTER) > 0;
        }

        /// <summary>
        /// </summary>
        /// <returns>true if all chars are digits.</returns>
        public virtual bool IsAllDigit()
        {
            return (pattern & ALL_DIGIT) > 0;
        }

        /// <summary>
        /// </summary>
        /// <returns>true if all chars are hiragana.</returns>
        public virtual bool IsAllHiragana()
        {
            return (pattern & ALL_HIRAGANA) > 0;
        }

        /// <summary>
        /// </summary>
        /// <returns>true if all chars are katakana.</returns>
        public virtual bool IsAllKatakana()
        {
            return (pattern & ALL_KATAKANA) > 0;
        }

        /// <summary>
        /// Retrieves the number of digits.
        /// </summary>
        public virtual int Digits()
        {
            return digits;
        }

        public virtual bool ContainsPeriod()
        {
            return (pattern & CONTAINS_PERIOD) > 0;
        }

        public virtual bool ContainsComma()
        {
            return (pattern & CONTAINS_COMMA) > 0;
        }

        public virtual bool ContainsSlash()
        {
            return (pattern & CONTAINS_SLASH) > 0;
        }

        public virtual bool ContainsDigit()
        {
            return (pattern & CONTAINS_DIGIT) > 0;
        }

        public virtual bool ContainsHyphen()
        {
            return (pattern & CONTAINS_HYPHEN) > 0;
        }

        public virtual bool ContainsLetters()
        {
            return (pattern & CONTAINS_LETTERS) > 0;
        }
    }
}

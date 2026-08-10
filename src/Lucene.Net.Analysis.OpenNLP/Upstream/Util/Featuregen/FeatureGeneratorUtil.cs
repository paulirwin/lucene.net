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

using System.Text.RegularExpressions;

namespace Opennlp.Tools.Util.Featuregen
{
    /// <summary>
    /// This class provide common utilities for feature generation.
    /// </summary>
    internal class FeatureGeneratorUtil
    {
        private static readonly string TOKEN_CLASS_PREFIX = "wc";
        private static readonly string TOKEN_AND_CLASS_PREFIX = "w&c";
        private static readonly Regex capPeriod = new Regex("^[A-Z]\\.$", RegexOptions.Compiled);
        /// <summary>
        /// Generates a class name for the specified token.
        /// The classes are as follows where the first matching class is used:
        /// <ul>
        /// <li>jah - Japanese Hiragana</li>
        /// <li>jak - Japanese Katakana</li>
        /// <li>lc - lowercase alphabetic</li>
        /// <li>2d - two digits </li>
        /// <li>4d - four digits </li>
        /// <li>an - alpha-numeric </li>
        /// <li>dd - digits and dashes </li>
        /// <li>ds - digits and slashes </li>
        /// <li>dc - digits and commas </li>
        /// <li>dp - digits and periods </li>
        /// <li>num - digits </li>
        /// <li>sc - single capital letter </li>
        /// <li>ac - all capital letters </li>
        /// <li>ic - initial capital letter </li>
        /// <li>other - other </li>
        /// </ul>
        /// </summary>
        /// <param name="token">A token or word.</param>
        /// <returns>The class name that the specified token belongs in.</returns>
        public static string TokenFeature(string token)
        {
            StringPattern pattern = StringPattern.Recognize(token);
            string feat;
            if (pattern.IsAllHiragana())
            {
                feat = "jah";
            }
            else if (pattern.IsAllKatakana())
            {
                feat = "jak";
            }
            else if (pattern.IsAllLowerCaseLetter())
            {
                feat = "lc";
            }
            else if (pattern.Digits() == 2)
            {
                feat = "2d";
            }
            else if (pattern.Digits() == 4)
            {
                feat = "4d";
            }
            else if (pattern.ContainsDigit())
            {
                if (pattern.ContainsLetters())
                {
                    feat = "an";
                }
                else if (pattern.ContainsHyphen())
                {
                    feat = "dd";
                }
                else if (pattern.ContainsSlash())
                {
                    feat = "ds";
                }
                else if (pattern.ContainsComma())
                {
                    feat = "dc";
                }
                else if (pattern.ContainsPeriod())
                {
                    feat = "dp";
                }
                else
                {
                    feat = "num";
                }
            }
            else if (pattern.IsAllCapitalLetter())
            {
                if (token.Length == 1)
                {
                    feat = "sc";
                }
                else
                {
                    feat = "ac";
                }
            }
            else if (capPeriod.IsMatch(token))
            {
                feat = "cp";
            }
            else if (pattern.IsInitialCapitalLetter())
            {
                feat = "ic";
            }
            else
            {
                feat = "other";
            }

            return (feat);
        }
    }
}

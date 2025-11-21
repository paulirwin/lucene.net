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
using J2N;
using J2N.Text;

namespace Opennlp.Tools.Util
{
    public class StringUtil
    {
        /// <summary>
        /// Determines if the specified character is a whitespace.
        ///
        /// A character is considered a whitespace when one
        /// of the following conditions is meet:
        ///
        /// <ul>
        /// <li>Its a {@link Character#isWhitespace(int)} whitespace.</li>
        /// <li>Its a part of the Unicode Zs category ({@link Character#SPACE_SEPARATOR}).</li>
        /// </ul>
        ///
        /// <code>Character.isWhitespace(int)</code> does not include no-break spaces.
        /// In OpenNLP no-break spaces are also considered as white spaces.
        /// </summary>
        /// <param name="charCode"></param>
        /// <returns>true if white space otherwise false</returns>
        public static bool IsWhitespace(char charCode)
        {
            return Character.IsWhiteSpace(charCode) || Character.GetType(charCode) == UnicodeCategory.SpaceSeparator;
        }

        /// <summary>
        /// Determines if the specified character is a whitespace.
        ///
        /// A character is considered a whitespace when one
        /// of the following conditions is meet:
        ///
        /// <ul>
        /// <li>Its a {@link Character#isWhitespace(int)} whitespace.</li>
        /// <li>Its a part of the Unicode Zs category ({@link Character#SPACE_SEPARATOR}).</li>
        /// </ul>
        ///
        /// <code>Character.isWhitespace(int)</code> does not include no-break spaces.
        /// In OpenNLP no-break spaces are also considered as white spaces.
        /// </summary>
        /// <param name="charCode"></param>
        /// <returns>true if white space otherwise false</returns>
        public static bool IsWhitespace(int charCode)
        {
            return Character.IsWhiteSpace(charCode) || Character.GetType(charCode) == UnicodeCategory.SpaceSeparator;
        }

        /// <summary>
        /// Converts to lower case independent of the current locale via
        /// {@link Character#toLowerCase(char)} which uses mapping information
        /// from the UnicodeData file.
        /// </summary>
        /// <param name="string"></param>
        /// <returns>lower cased String</returns>
        public static string ToLowerCase(string @string)
        {
            char[] lowerCaseChars = new char[@string.Length];
            for (int i = 0; i < @string.Length; i++)
            {
                lowerCaseChars[i] = char.ToLowerInvariant(@string[i]);
            }

            return new string (lowerCaseChars);
        }

        /// <summary>
        /// Converts to upper case independent of the current locale via
        /// {@link Character#toUpperCase(char)} which uses mapping information
        /// from the UnicodeData file.
        /// </summary>
        /// <param name="string"></param>
        /// <returns>upper cased String</returns>
        public static string ToUpperCase(string @string)
        {
            char[] upperCaseChars = new char[@string.Length];
            for (int i = 0; i < @string.Length; i++)
            {
                upperCaseChars[i] = char.ToUpperInvariant(@string[i]);
            }

            return new string (upperCaseChars);
        }

        /// <summary>
        /// Returns {@code true} if {@link CharSequence#length()} is
        /// {@code 0} or {@code null}.
        /// </summary>
        /// <returns>{@code true} if {@link CharSequence#length()} is {@code 0}, otherwise
        ///         {@code false}</returns>
        /// <remarks>@since1.5.1</remarks>
        public static bool IsEmpty(string theString)
        {
            return theString.Length == 0;
        }

        /// <summary>
        /// Get mininum of three values.
        /// </summary>
        /// <param name="a">number a</param>
        /// <param name="b">number b</param>
        /// <param name="c">number c</param>
        /// <returns>the minimum</returns>
        private static int Minimum(int a, int b, int c)
        {
            int minValue;
            minValue = a;
            if (b < minValue)
            {
                minValue = b;
            }

            if (c < minValue)
            {
                minValue = c;
            }

            return minValue;
        }

        /// <summary>
        /// Computes the Levenshtein distance of two strings in a matrix.
        /// Based on pseudo-code provided here:
        /// https://en.wikipedia.org/wiki/Levenshtein_distance#Computing_Levenshtein_distance
        /// which in turn is based on the paper Wagner, Robert A.; Fischer, Michael J. (1974),
        /// "The String-to-String Correction Problem", Journal of the ACM 21 (1): 168-173
        /// </summary>
        /// <param name="wordForm">the form</param>
        /// <param name="lemma">the lemma</param>
        /// <returns>the distance</returns>
        public static int[,] LevenshteinDistance(string wordForm, string lemma)
        {
            int wordLength = wordForm.Length;
            int lemmaLength = lemma.Length;
            int cost;
            int[,] distance = new int[wordLength + 1, lemmaLength + 1];
            if (wordLength == 0)
            {
                return distance;
            }

            if (lemmaLength == 0)
            {
                return distance;
            }


            //fill in the rows of column 0
            for (int i = 0; i <= wordLength; i++)
            {
                distance[i, 0] = i;
            }


            //fill in the columns of row 0
            for (int j = 0; j <= lemmaLength; j++)
            {
                distance[0, j] = j;
            }


            //fill in the rest of the matrix calculating the minimum distance
            for (int i = 1; i <= wordLength; i++)
            {
                int s_i = wordForm[i - 1];
                for (int j = 1; j <= lemmaLength; j++)
                {
                    if (s_i == lemma[j - 1])
                    {
                        cost = 0;
                    }
                    else
                    {
                        cost = 1;
                    }


                    //obtain minimum distance from calculating deletion, insertion, substitution
                    distance[i,j] = Minimum(distance[i - 1,j] + 1, distance[i,j - 1] + 1, distance[i - 1,j - 1] + cost);
                }
            }

            return distance;
        }

        /// <summary>
        /// Computes the Shortest Edit Script (SES) to convert a word into its lemma.
        /// This is based on Chrupala's PhD thesis (2008).
        /// </summary>
        /// <param name="wordForm">the token</param>
        /// <param name="lemma">the target lemma</param>
        /// <param name="distance">the levenshtein distance</param>
        /// <param name="permutations">the number of permutations</param>
        public static void ComputeShortestEditScript(string wordForm, string lemma, int[,] distance, StringBuffer permutations)
        {
            int n = distance.GetLength(0);
            int m = distance.GetLength(1);
            int wordFormLength = n - 1;
            int lemmaLength = m - 1;
            while (true)
            {
                if (distance[wordFormLength, lemmaLength] == 0)
                {
                    break;
                }

                if ((lemmaLength > 0 && wordFormLength > 0) && (distance[wordFormLength - 1, lemmaLength - 1] < distance[wordFormLength, lemmaLength]))
                {
                    permutations.Append('R').Append(wordFormLength - 1).Append(wordForm[wordFormLength - 1]).Append(lemma[lemmaLength - 1]);
                    lemmaLength--;
                    wordFormLength--;
                    continue;
                }

                if (lemmaLength > 0 && (distance[wordFormLength, lemmaLength - 1] < distance[wordFormLength, lemmaLength]))
                {
                    permutations.Append('I').Append(wordFormLength).Append(lemma[lemmaLength - 1]);
                    lemmaLength--;
                    continue;
                }

                if (wordFormLength > 0 && (distance[wordFormLength - 1, lemmaLength] < distance[wordFormLength, lemmaLength]))
                {
                    permutations.Append('D').Append(wordFormLength - 1).Append(wordForm[wordFormLength - 1]);
                    wordFormLength--;
                    continue;
                }

                if ((wordFormLength > 0 && lemmaLength > 0) && (distance[wordFormLength - 1, lemmaLength - 1] == distance[wordFormLength, lemmaLength]))
                {
                    wordFormLength--;
                    lemmaLength--;
                    continue;
                }

                if (wordFormLength > 0 && (distance[wordFormLength - 1, lemmaLength] == distance[wordFormLength, lemmaLength]))
                {
                    wordFormLength--;
                    continue;
                }

                if (lemmaLength > 0 && (distance[wordFormLength, lemmaLength - 1] == distance[wordFormLength, lemmaLength]))
                {
                    lemmaLength--;
                }
            }
        }

        /// <summary>
        /// Read predicted SES by the lemmatizer model and apply the
        /// permutations to obtain the lemma from the wordForm.
        /// </summary>
        /// <param name="wordForm">the wordForm</param>
        /// <param name="permutations">the permutations predicted by the lemmatizer model</param>
        /// <returns>the lemma</returns>
        public static string DecodeShortestEditScript(string wordForm, string permutations)
        {
            StringBuffer lemma = new StringBuffer(wordForm).Reverse();
            int permIndex = 0;
            while (true)
            {
                if (permutations.Length <= permIndex)
                {
                    break;
                }


                //read first letter of permutation string
                char nextOperation = permutations[permIndex];

                //System.err.println("-> NextOP: " + nextOperation);
                //go to the next permutation letter
                permIndex++;
                if (nextOperation == 'R')
                {
                    string charAtPerm = permutations[permIndex].ToString();
                    int charIndex = int.Parse(charAtPerm);

                    // go to the next character in the permutation buffer
                    // which is the replacement character
                    permIndex++;
                    char replace = permutations[permIndex];

                    //go to the next char in the permutation buffer
                    // which is the candidate character
                    permIndex++;
                    char with = permutations[permIndex];
                    if (lemma.Length <= charIndex)
                    {
                        return wordForm;
                    }

                    if (lemma[charIndex] == replace)
                    {
                        lemma[charIndex] = with;
                    }


                    //System.err.println("-> ROP: " + lemma.toString());
                    //go to next permutation
                    permIndex++;
                }
                else if (nextOperation == 'I')
                {
                    string charAtPerm = permutations[permIndex].ToString();
                    int charIndex = int.Parse(charAtPerm);
                    permIndex++;

                    //character to be inserted
                    char @in = permutations[permIndex];
                    if (lemma.Length < charIndex)
                    {
                        return wordForm;
                    }

                    lemma.Insert(charIndex, @in);

                    //System.err.println("-> IOP " + lemma.toString());
                    //go to next permutation
                    permIndex++;
                }
                else if (nextOperation == 'D')
                {
                    string charAtPerm = permutations[permIndex].ToString();
                    int charIndex = int.Parse(charAtPerm);
                    if (lemma.Length <= charIndex)
                    {
                        return wordForm;
                    }

                    lemma.Delete(charIndex, 1);
                    permIndex++;

                    // go to next permutation
                    permIndex++;
                }
            }

            return lemma.Reverse().ToString();
        }

        /// <summary>
        /// Get the SES required to go from a word to a lemma.
        /// </summary>
        /// <param name="wordForm">the word</param>
        /// <param name="lemma">the lemma</param>
        /// <returns>the shortest edit script</returns>
        public static string GetShortestEditScript(string wordForm, string lemma)
        {
            string reversedWF = new StringBuffer(wordForm.ToLowerInvariant()).Reverse().ToString();
            string reversedLemma = new StringBuffer(lemma.ToLowerInvariant()).Reverse().ToString();
            StringBuffer permutations = new StringBuffer();
            string ses;
            if (!reversedWF.Equals(reversedLemma))
            {
                int[, ] levenDistance = StringUtil.LevenshteinDistance(reversedWF, reversedLemma);
                StringUtil.ComputeShortestEditScript(reversedWF, reversedLemma, levenDistance, permutations);
                ses = permutations.ToString();
            }
            else
            {
                ses = "O";
            }

            return ses;
        }
    }
}

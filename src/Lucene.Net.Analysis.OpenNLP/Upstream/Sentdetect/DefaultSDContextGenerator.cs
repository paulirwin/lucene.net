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

namespace Opennlp.Tools.Sentdetect
{
    /// <summary>
    /// Generate event contexts for maxent decisions for sentence detection.
    /// </summary>
    internal class DefaultSDContextGenerator : SDContextGenerator
    {
        /// <summary>
        /// String buffer for generating features.
        /// </summary>
        protected StringBuilder buf;
        /// <summary>
        /// List for holding features as they are generated.
        /// </summary>
        protected IList<string> collectFeats;
        private ISet<string> inducedAbbreviations;
        private HashSet<char> eosCharacters;
        /// <summary>
        /// Creates a new <code>SDContextGenerator</code> instance with
        /// no induced abbreviations.
        /// </summary>
        /// <param name="eosCharacters"></param>
        public DefaultSDContextGenerator(char[] eosCharacters) : this(new HashSet<string>(), eosCharacters)
        {
        }

        /// <summary>
        /// Creates a new <code>SDContextGenerator</code> instance which uses
        /// the set of induced abbreviations.
        /// </summary>
        /// <param name="inducedAbbreviations">a <code>Set</code> of Strings
        ///     representing induced abbreviations in the training data.
        ///     Example: &quot;Mr.&quot;</param>
        /// <param name="eosCharacters"></param>
        public DefaultSDContextGenerator(ISet<string> inducedAbbreviations, char[] eosCharacters)
        {
            this.inducedAbbreviations = inducedAbbreviations;
            this.eosCharacters = new HashSet<char>();
            foreach (char eosChar in eosCharacters)
            {
                this.eosCharacters.Add(eosChar);
            }

            buf = new StringBuilder();
            collectFeats = new List<string>();
        }

        private static string EscapeChar(char c)
        {
            if (c == '\n')
            {
                return "<LF>";
            }

            if (c == '\r')
            {
                return "<CR>";
            }

            return new string (new char[] { c });
        }

        /* (non-Javadoc)
         * @see opennlp.tools.sentdetect.SDContextGenerator#getContext(java.lang.StringBuffer, int)
         */
        public virtual String[] GetContext(string sb, int position)
        {
            /*
             * String preceding the eos character in the eos token.
             */
            string prefix;
            /*
             * Space delimited token preceding token containing eos character.
             */
            string previous;
            /*
             * String following the eos character in the eos token.
             */
            string suffix;
            /*
             * Space delimited token following token containing eos character.
             */
            string next;
            int lastIndex = sb.Length - 1;
            {

                // compute space previous and space next features.
                if (position > 0 && StringUtil.IsWhitespace(sb[position - 1]))
                    collectFeats.Add("sp");
                if (position < lastIndex && StringUtil.IsWhitespace(sb[position + 1]))
                    collectFeats.Add("sn");
                collectFeats.Add("eos=" + EscapeChar(sb[position]));
            }

            int prefixStart = PreviousSpaceIndex(sb, position);
            int c = position;
            {

                ///assign prefix, stop if you run into a period though otherwise stop at space
                while (--c > prefixStart)
                {
                    if (eosCharacters.Contains(sb[c]))
                    {
                        prefixStart = c;
                        c++; // this gets us out of while loop.
                    }
                }

                prefix = sb.Substring(prefixStart, (position) - (prefixStart)).Trim();
            }

            int prevStart = PreviousSpaceIndex(sb, prefixStart);
            previous = sb.Substring(prevStart, (prefixStart) - (prevStart)).Trim();
            int suffixEnd = NextSpaceIndex(sb, position, lastIndex);
            {
                c = position;
                while (++c < suffixEnd)
                {
                    if (eosCharacters.Contains(sb[c]))
                    {
                        suffixEnd = c;
                        c--; // this gets us out of while loop.
                    }
                }
            }

            int nextEnd = NextSpaceIndex(sb, suffixEnd + 1, lastIndex + 1);
            if (position == lastIndex)
            {
                suffix = "";
                next = "";
            }
            else
            {
                suffix = sb.Substring(position + 1, (suffixEnd) - (position + 1)).Trim();
                next = sb.Substring(suffixEnd + 1, (nextEnd) - (suffixEnd + 1)).Trim();
            }

            CollectFeatures(prefix, suffix, previous, next, sb[position]);
            string[] context = new string[collectFeats.Count];
            context = collectFeats.ToArray();
            collectFeats.Clear();
            return context;
        }

        /// <summary>
        /// Determines some of the features for the sentence detector and adds them to list features.
        /// </summary>
        /// <param name="prefix">String preceding the eos character in the eos token.</param>
        /// <param name="suffix">String following the eos character in the eos token.</param>
        /// <param name="previous">Space delimited token preceding token containing eos character.</param>
        /// <param name="next">Space delimited token following token containing eos character.</param>
        /// <remarks>@deprecateduse {@link #collectFeatures(String, String, String, String, char)} instead.</remarks>
        protected virtual void CollectFeatures(string prefix, string suffix, string previous, string next)
        {
            CollectFeatures(prefix, suffix, previous, next, (char?)null);
        }

        /// <summary>
        /// Determines some of the features for the sentence detector and adds them to list features.
        /// </summary>
        /// <param name="prefix">String preceding the eos character in the eos token.</param>
        /// <param name="suffix">String following the eos character in the eos token.</param>
        /// <param name="previous">Space delimited token preceding token containing eos character.</param>
        /// <param name="next">Space delimited token following token containing eos character.</param>
        /// <param name="eosChar">the EOS character been analyzed</param>
        protected virtual void CollectFeatures(string prefix, string suffix, string previous, string next, char? eosChar)
        {
            buf.Append("x=");
            buf.Append(prefix);
            collectFeats.Add(buf.ToString());
            buf.Clear();
            if (!prefix.Equals(""))
            {
                collectFeats.Add(Convert.ToString(prefix.Length));
                if (IsFirstUpper(prefix))
                {
                    collectFeats.Add("xcap");
                }

                if (eosChar != null && inducedAbbreviations.Contains(prefix + eosChar))
                {
                    collectFeats.Add("xabbrev");
                }
            }

            buf.Append("v=");
            buf.Append(previous);
            collectFeats.Add(buf.ToString());
            buf.Clear();
            if (!previous.Equals(""))
            {
                if (IsFirstUpper(previous))
                {
                    collectFeats.Add("vcap");
                }

                if (inducedAbbreviations.Contains(previous))
                {
                    collectFeats.Add("vabbrev");
                }
            }

            buf.Append("s=");
            buf.Append(suffix);
            collectFeats.Add(buf.ToString());
            buf.Clear();
            if (!suffix.Equals(""))
            {
                if (IsFirstUpper(suffix))
                {
                    collectFeats.Add("scap");
                }

                if (inducedAbbreviations.Contains(suffix))
                {
                    collectFeats.Add("sabbrev");
                }
            }

            buf.Append("n=");
            buf.Append(next);
            collectFeats.Add(buf.ToString());
            buf.Clear();
            if (!next.Equals(""))
            {
                if (IsFirstUpper(next))
                {
                    collectFeats.Add("ncap");
                }

                if (inducedAbbreviations.Contains(next))
                {
                    collectFeats.Add("nabbrev");
                }
            }
        }

        private static bool IsFirstUpper(string s)
        {
            return char.IsUpper(s[0]);
        }

        /// <summary>
        /// Finds the index of the nearest space before a specified index which is not itself preceded by a space.
        /// </summary>
        /// <param name="sb">The string buffer which contains the text being examined.</param>
        /// <param name="seek">The index to begin searching from.</param>
        /// <returns>The index which contains the nearest space.</returns>
        private static int PreviousSpaceIndex(string sb, int seek)
        {
            seek--;
            while (seek > 0 && !StringUtil.IsWhitespace(sb[seek]))
            {
                seek--;
            }

            if (seek > 0 && StringUtil.IsWhitespace(sb[seek]))
            {
                while (seek > 0 && StringUtil.IsWhitespace(sb[seek - 1]))
                    seek--;
                return seek;
            }

            return 0;
        }

        /// <summary>
        /// Finds the index of the nearest space after a specified index.
        /// </summary>
        /// <param name="sb">The string buffer which contains the text being examined.</param>
        /// <param name="seek">The index to begin searching from.</param>
        /// <param name="lastIndex">The highest index of the StringBuilder sb.</param>
        /// <returns>The index which contains the nearest space.</returns>
        private static int NextSpaceIndex(string sb, int seek, int lastIndex)
        {
            seek++;
            char c;
            while (seek < lastIndex)
            {
                c = sb[seek];
                if (StringUtil.IsWhitespace(c))
                {
                    while (sb.Length > seek + 1 && StringUtil.IsWhitespace(sb[seek + 1]))
                        seek++;
                    return seek;
                }

                seek++;
            }

            return lastIndex;
        }
    }
}

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
using System.Linq;
using System.Text;
using Lucene.Net.Support;

namespace Opennlp.Tools.Chunker
{
    /// <summary>
    /// Class for holding chunks for a single unit of text.
    /// </summary>
    public class ChunkSample
    {
        private readonly IList<string> sentence;
        private readonly IList<string> tags;
        private readonly IList<string> preds;
        /// <summary>
        /// Initializes the current instance.
        /// </summary>
        /// <param name="sentence">
        ///          training sentence</param>
        /// <param name="tags">
        ///          POS Tags for the sentence</param>
        /// <param name="preds">
        ///          Chunk tags in B-* I-* notation</param>
        public ChunkSample(string[] sentence, string[] tags, string[] preds)
        {
            ValidateArguments(sentence.Length, tags.Length, preds.Length);
            this.sentence = sentence;
            this.tags = tags;
            this.preds = preds;
        }

        /// <summary>
        /// Initializes the current instance.
        /// </summary>
        /// <param name="sentence">
        ///          training sentence</param>
        /// <param name="tags">
        ///          POS Tags for the sentence</param>
        /// <param name="preds">
        ///          Chunk tags in B-* I-* notation</param>
        public ChunkSample(IList<string> sentence, IList<string> tags, IList<string> preds)
        {
            ValidateArguments(sentence.Count, tags.Count, preds.Count);
            this.sentence = sentence;
            this.tags = tags;
            this.preds = preds;
        }

        /// <summary>
        /// Gets the training sentence
        /// </summary>
        public virtual string[] GetSentence()
        {
            return sentence.ToArray();
        }

        /// <summary>
        /// Gets the POS Tags for the sentence
        /// </summary>
        public virtual string[] GetTags()
        {
            return tags.ToArray();
        }

        /// <summary>
        /// Gets the Chunk tags in B-* I-* notation
        /// </summary>
        public virtual string[] GetPreds()
        {
            return preds.ToArray();
        }

        /// <summary>
        /// Gets the phrases as an array of spans
        /// </summary>
        public virtual Span[] GetPhrasesAsSpanList()
        {
            return PhrasesAsSpanList(GetSentence(), GetTags(), GetPreds());
        }

        /// <summary>
        /// Static method to create arrays of spans of phrases
        /// </summary>
        /// <param name="aSentence">
        ///          training sentence</param>
        /// <param name="aTags">
        ///          POS Tags for the sentence</param>
        /// <param name="aPreds">
        ///          Chunk tags in B-* I-* notation</param>
        /// <returns>the phrases as an array of spans</returns>
        public static Span[] PhrasesAsSpanList(string[] aSentence, string[] aTags, string[] aPreds)
        {
            ValidateArguments(aSentence.Length, aTags.Length, aPreds.Length);

            // initialize with the list maximum size
            IList<Span> phrases = new List<Span>(aSentence.Length);
            string startTag = "";
            int startIndex = 0;
            bool foundPhrase = false;
            for (int ci = 0, cn = aPreds.Length; ci < cn; ci++)
            {
                string pred = aPreds[ci];
                if (pred.StartsWith("B-") || !pred.Equals("I-" + startTag) && !pred.Equals("O"))
                {

                    // start
                    if (foundPhrase)
                    {

                        // handle the last
                        phrases.Add(new Span(startIndex, ci, startTag));
                    }

                    startIndex = ci;
                    startTag = pred.Substring(2);
                    foundPhrase = true;
                } // middle
                else if (pred.Equals("I-" + startTag))
                {
                } // end
                else if (foundPhrase)
                {

                    // end
                    phrases.Add(new Span(startIndex, ci, startTag));
                    foundPhrase = false;
                    startTag = "";
                }
            }

            if (foundPhrase)
            {

                // leftover
                phrases.Add(new Span(startIndex, aPreds.Length, startTag));
            }

            return phrases.ToArray();
        }

        private static void ValidateArguments(int sentenceSize, int tagsSize, int predsSize)
        {
            if (sentenceSize != tagsSize || tagsSize != predsSize)
                throw new ArgumentException("All arrays must have the same length: " + "sentenceSize: " + sentenceSize + ", tagsSize: " + tagsSize + ", predsSize: " + predsSize + "!");
        }

        /// <summary>
        /// Creates a nice to read string for the phrases formatted as following: <br>
        /// <code>
        /// [NP Rockwell_NNP ] [VP said_VBD ] [NP the_DT agreement_NN ] [VP calls_VBZ ] [SBAR for_IN ]
        /// [NP it_PRP ] [VP to_TO supply_VB ] [NP 200_CD additional_JJ so-called_JJ shipsets_NNS ]
        /// [PP for_IN ] [NP the_DT planes_NNS ] ._.
        /// </code>
        /// </summary>
        /// <returns>a nice to read string representation of the chunk phases</returns>
        public virtual string NicePrint()
        {
            Span[] spans = GetPhrasesAsSpanList();
            StringBuilder result = new StringBuilder(" ");
            for (int tokenIndex = 0; tokenIndex < sentence.Count; tokenIndex++)
            {
                foreach (Span span in spans)
                {
                    if (span.GetStart() == tokenIndex)
                    {
                        result.Append("[").Append(span.GetType()).Append(" ");
                    }

                    if (span.GetEnd() == tokenIndex)
                    {
                        result.Append("]").Append(' ');
                    }
                }

                result.Append(sentence[tokenIndex]).Append("_").Append(tags[tokenIndex]).Append(' ');
            }

            if (sentence.Count > 1)
                result.Length = result.Length - 1;
            foreach (Span span in spans)
            {
                if (span.GetEnd() == sentence.Count)
                {
                    result.Append(']');
                }
            }

            return result.ToString();
        }

        public virtual string ToString()
        {
            StringBuilder chunkString = new StringBuilder();
            for (int ci = 0; ci < preds.Count; ci++)
            {
                chunkString.Append(sentence[ci]).Append(" ").Append(tags[ci]).Append(" ").Append(preds[ci]).Append("\n");
            }

            return chunkString.ToString();
        }

        public virtual int GetHashCode()
        {
            return HashCode.Combine(Arrays.GetHashCode(GetSentence()), Arrays.GetHashCode(GetTags()), Arrays.GetHashCode(GetPreds()));
        }

        public virtual bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (obj is ChunkSample sample)
            {
                return Arrays.Equals(GetSentence(), sample.GetSentence()) && Arrays.Equals(GetTags(), sample.GetTags()) && Arrays.Equals(GetPreds(), sample.GetPreds());
            }

            return false;
        }
    }
}

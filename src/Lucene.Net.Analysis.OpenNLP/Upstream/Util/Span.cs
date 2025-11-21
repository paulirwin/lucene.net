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
using J2N.Text;

namespace Opennlp.Tools.Util
{
    /// <summary>
    /// Class for storing start and end integer offsets.
    /// </summary>
    public class Span : IComparable<Span>
    {
        private readonly int start;
        private readonly int end;
        private readonly double prob; //default is 0
        private readonly string type;
        /// <summary>
        /// Initializes a new Span Object. Sets the prob to 0 as default.
        /// </summary>
        /// <param name="s">start of span.</param>
        /// <param name="e">end of span, which is +1 more than the last element in the span.</param>
        /// <param name="type">the type of the span</param>
        public Span(int s, int e, string type) : this(s, e, type, 0)
        {
        }

        /// <summary>
        /// Initializes a new Span Object.
        /// </summary>
        /// <param name="s">start of span.</param>
        /// <param name="e">end of span, which is +1 more than the last element in the span.</param>
        /// <param name="type">the type of the span</param>
        /// <param name="prob">probability of span.</param>
        public Span(int s, int e, string type, double prob)
        {
            if (s < 0)
            {
                throw new ArgumentException("start index must be zero or greater: " + s);
            }

            if (e < 0)
            {
                throw new ArgumentException("end index must be zero or greater: " + e);
            }

            if (s > e)
            {
                throw new ArgumentException("start index must not be larger than end index: " + "start=" + s + ", end=" + e);
            }

            start = s;
            end = e;
            this.prob = prob;
            this.type = type;
        }

        /// <summary>
        /// Initializes a new Span Object. Sets the prob to 0 as default
        /// </summary>
        /// <param name="s">start of span.</param>
        /// <param name="e">end of span.</param>
        public Span(int s, int e) : this(s, e, null, 0)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="s">the start of the span (the token index, not the char index)</param>
        /// <param name="e">the end of the span (the token index, not the char index)</param>
        /// <param name="prob"></param>
        public Span(int s, int e, double prob) : this(s, e, null, prob)
        {
        }

        /// <summary>
        /// Initializes a new Span object with an existing Span which is shifted by an
        /// offset.
        /// </summary>
        /// <param name="span"></param>
        /// <param name="offset"></param>
        public Span(Span span, int offset) : this(span.start + offset, span.end + offset, span.GetType(), span.GetProb())
        {
        }

        /// <summary>
        /// Creates a new immutable span based on an existing span, where the existing span did not include the prob
        /// </summary>
        /// <param name="span">the span that has no prob or the prob is incorrect and a new Span must be generated</param>
        /// <param name="prob">the probability of the span</param>
        public Span(Span span, double prob) : this(span.start, span.end, span.GetType(), prob)
        {
        }

        /// <summary>
        /// Return the start of a span.
        /// </summary>
        /// <returns>the start of a span.</returns>
        public virtual int GetStart()
        {
            return start;
        }

        /// <summary>
        /// Return the end of a span.
        ///
        /// Note: that the returned index is one past the actual end of the span in the
        /// text, or the first element past the end of the span.
        /// </summary>
        /// <returns>the end of a span.</returns>
        public virtual int GetEnd()
        {
            return end;
        }

        /// <summary>
        /// Retrieves the type of the span.
        /// </summary>
        /// <returns>the type or null if not set</returns>
        public virtual string GetType()
        {
            return type;
        }

        /// <summary>
        /// Returns the length of this span.
        /// </summary>
        /// <returns>the length of the span.</returns>
        public virtual int Length()
        {
            return end - start;
        }

        /// <summary>
        /// Returns true if the specified span is contained by this span. Identical
        /// spans are considered to contain each other.
        /// </summary>
        /// <param name="s">The span to compare with this span.</param>
        /// <returns>true is the specified span is contained by this span; false otherwise.</returns>
        public virtual bool Contains(Span s)
        {
            return start <= s.GetStart() && s.GetEnd() <= end;
        }

        /// <summary>
        /// Returns true if the specified index is contained inside this span. An index
        /// with the value of end is considered outside the span.
        /// </summary>
        /// <param name="index">the index to test with this span.</param>
        /// <returns>true if the span contains this specified index; false otherwise.</returns>
        public virtual bool Contains(int index)
        {
            return start <= index && index < end;
        }

        /// <summary>
        /// Returns true if the specified span is the begin of this span and the
        /// specified span is contained in this span.
        /// </summary>
        /// <param name="s">The span to compare with this span.</param>
        /// <returns>true if the specified span starts with this span and is contained
        ///     in this span; false otherwise</returns>
        public virtual bool StartsWith(Span s)
        {
            return GetStart() == s.GetStart() && Contains(s);
        }

        /// <summary>
        /// Returns true if the specified span intersects with this span.
        /// </summary>
        /// <param name="s">The span to compare with this span.</param>
        /// <returns>true is the spans overlap; false otherwise.</returns>
        public virtual bool Intersects(Span s)
        {
            int sstart = s.GetStart();

            //either s's start is in this or this' start is in s
            return this.Contains(s) || s.Contains(this) || GetStart() <= sstart && sstart < GetEnd() || sstart <= GetStart() && GetStart() < s.GetEnd();
        }

        /// <summary>
        /// Returns true is the specified span crosses this span.
        /// </summary>
        /// <param name="s">The span to compare with this span.</param>
        /// <returns>true is the specified span overlaps this span and contains a
        ///     non-overlapping section; false otherwise.</returns>
        public virtual bool Crosses(Span s)
        {
            int sstart = s.GetStart();

            //either s's start is in this or this' start is in s
            return !this.Contains(s) && !s.Contains(this) && (GetStart() <= sstart && sstart < GetEnd() || sstart <= GetStart() && GetStart() < s.GetEnd());
        }

        /// <summary>
        /// Retrieves the string covered by the current span of the specified text.
        /// </summary>
        /// <param name="text"></param>
        /// <returns>the substring covered by the current span</returns>
        public virtual ICharSequence GetCoveredText(ICharSequence text)
        {
            if (GetEnd() > text.Length)
            {
                throw new ArgumentException("The span " + ToString() + " is outside the given text which has length " + text.Length + "!");
            }

            return text.Subsequence(GetStart(), GetEnd());
        }

        /// <summary>
        /// Return a copy of this span with leading and trailing white spaces removed.
        /// </summary>
        /// <param name="text"></param>
        /// <returns>the trimmed span or the same object if already trimmed</returns>
        public virtual Span Trim(ICharSequence text)
        {
            int newStartOffset = GetStart();
            for (int i = GetStart(); i < GetEnd() && char.IsWhiteSpace(text[i]); i++)
            {
                newStartOffset++;
            }

            int newEndOffset = GetEnd();
            for (int i = GetEnd(); i > GetStart() && char.IsWhiteSpace(text[i - 1]); i--)
            {
                newEndOffset--;
            }

            if (newStartOffset == GetStart() && newEndOffset == GetEnd())
            {
                return this;
            }
            else if (newStartOffset > newEndOffset)
            {
                return new Span(GetStart(), GetStart(), GetType());
            }
            else
            {
                return new Span(newStartOffset, newEndOffset, GetType());
            }
        }

        /// <summary>
        /// Compares the specified span to the current span.
        /// </summary>
        public virtual int CompareTo(Span s)
        {
            if (GetStart() < s.GetStart())
            {
                return -1;
            }
            else if (GetStart() == s.GetStart())
            {
                if (GetEnd() > s.GetEnd())
                {
                    return -1;
                }
                else if (GetEnd() < s.GetEnd())
                {
                    return 1;
                }
                else
                {

                    // compare the type
                    if (GetType() == null && s.GetType() == null)
                    {
                        return 0;
                    }
                    else if (GetType() != null && s.GetType() != null)
                    {

                        // use type lexicography order
                        return GetType().CompareTo(s.GetType());
                    }
                    else if (GetType() != null)
                    {
                        return -1;
                    }

                    return 1;
                }
            }
            else
            {
                return 1;
            }
        }

        /// <summary>
        /// Generates a hash code of the current span.
        /// </summary>
        public virtual int GetHashCode()
        {
            return HashCode.Combine(GetStart(), GetEnd(), GetType());
        }

        /// <summary>
        /// Checks if the specified span is equal to the current span.
        /// </summary>
        public virtual bool Equals(object o)
        {
            if (o == this)
            {
                return true;
            }

            if (o is Span)
            {
                Span s = (Span)o;
                return GetStart() == s.GetStart() && GetEnd() == s.GetEnd() && Equals(GetType(), s.GetType());
            }

            return false;
        }

        /// <summary>
        /// Generates a human readable string.
        /// </summary>
        public virtual string ToString()
        {
            StringBuilder toStringBuffer = new StringBuilder(15);
            toStringBuffer.Append("[");
            toStringBuffer.Append(GetStart());
            toStringBuffer.Append("..");
            toStringBuffer.Append(GetEnd());
            toStringBuffer.Append(")");
            if (GetType() != null)
            {
                toStringBuffer.Append(" ");
                toStringBuffer.Append(GetType());
            }

            return toStringBuffer.ToString();
        }

        /// <summary>
        /// Converts an array of {@link Span}s to an array of {@link String}s.
        /// </summary>
        /// <param name="spans"></param>
        /// <param name="s"></param>
        /// <returns>the strings</returns>
        public static string[] SpansToStrings(Span[] spans, ICharSequence s)
        {
            string[] tokens = new string[spans.Length];
            for (int si = 0, sl = spans.Length; si < sl; si++)
            {
                tokens[si] = spans[si].GetCoveredText(s).ToString();
            }

            return tokens;
        }

        public static string[] SpansToStrings(Span[] spans, string[] tokens)
        {
            string[] chunks = new string[spans.Length];
            StringBuilder cb = new StringBuilder();
            for (int si = 0, sl = spans.Length; si < sl; si++)
            {
                cb.Length = 0;
                for (int ti = spans[si].GetStart(); ti < spans[si].GetEnd(); ti++)
                {
                    cb.Append(tokens[ti]).Append(" ");
                }

                chunks[si] = cb.Subsequence(0, cb.Length - 1).ToString();
            }

            return chunks;
        }

        public virtual double GetProb()
        {
            return prob;
        }
    }
}

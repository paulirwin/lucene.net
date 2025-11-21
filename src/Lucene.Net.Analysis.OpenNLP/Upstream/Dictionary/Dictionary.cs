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
using Opennlp.Tools.Dictionary.Serializer;
using Opennlp.Tools.Util;
using Opennlp.Tools.Util.Model;
using System;
using System.Collections.Generic;
using System.IO;

namespace Opennlp.Tools.Dictionary
{
    /// <summary>
    /// This class is a dictionary.
    /// </summary>
    public class Dictionary : IEnumerable<StringList> //, SerializableArtifact
    {
        private class StringListWrapper
        {
            private readonly StringList stringList;
            private StringListWrapper(StringList stringList)
            {
                this.stringList = stringList;
            }

            private StringList GetStringList()
            {
                return stringList;
            }

            public virtual bool Equals(object obj)
            {
                bool result;
                if (obj == this)
                {
                    result = true;
                }
                else if (obj is StringListWrapper)
                {
                    StringListWrapper other = (StringListWrapper)obj;
                    if (isCaseSensitive)
                    {
                        result = this.stringList.Equals(other.GetStringList());
                    }
                    else
                    {
                        result = this.stringList.CompareToIgnoreCase(other.GetStringList());
                    }
                }
                else
                {
                    result = false;
                }

                return result;
            }

            public virtual int GetHashCode()
            {

                // if lookup is too slow optimize this
                return StringUtil.ToLowerCase(this.stringList.ToString()).GetHashCode();
            }

            public virtual string ToString()
            {
                return this.stringList.ToString();
            }
        }

        private HashSet<StringListWrapper> entrySet = new HashSet();
        private readonly bool isCaseSensitive;
        private int minTokenCount = 99999;
        private int maxTokenCount = 0;
        /// <summary>
        /// Initializes an empty {@link Dictionary}.
        /// </summary>
        public Dictionary() : this(false)
        {
        }

        public Dictionary(bool caseSensitive)
        {
            isCaseSensitive = caseSensitive;
        }

        /// <summary>
        /// Initializes the {@link Dictionary} from an existing dictionary resource.
        /// </summary>
        /// <param name="in">{@link InputStream}</param>
        public Dictionary(Stream @in)
        {
            isCaseSensitive = DictionaryEntryPersistor.Create(@in, (entry) => Put(entry.GetTokens()));
        }

        /// <summary>
        /// Adds the tokens to the dictionary as one new entry.
        /// </summary>
        /// <param name="tokens">the new entry</param>
        public virtual void Put(StringList tokens)
        {
            entrySet.Add(new StringListWrapper(tokens));
            minTokenCount = Math.Min(minTokenCount, tokens.Count);
            maxTokenCount = Math.Max(maxTokenCount, tokens.Count);
        }

        /// <summary>
        /// </summary>
        /// <returns>minimum token count in the dictionary</returns>
        public virtual int GetMinTokenCount()
        {
            return minTokenCount;
        }

        /// <summary>
        /// </summary>
        /// <returns>maximum token count in the dictionary</returns>
        public virtual int GetMaxTokenCount()
        {
            return maxTokenCount;
        }

        /// <summary>
        /// Checks if this dictionary has the given entry.
        /// </summary>
        /// <param name="tokens">query</param>
        /// <returns>true if it contains the entry otherwise false</returns>
        public virtual bool Contains(StringList tokens)
        {
            return entrySet.Contains(new StringListWrapper(tokens));
        }

        /// <summary>
        /// Removes the given tokens form the current instance.
        /// </summary>
        /// <param name="tokens">filter tokens</param>
        public virtual void Remove(StringList tokens)
        {
            entrySet.Remove(new StringListWrapper(tokens));
        }

        /// <summary>
        /// Retrieves an Iterator over all tokens.
        /// </summary>
        /// <returns>token-{@link Iterator}</returns>
        public virtual IEnumerator<StringList> Iterator()
        {
            IEnumerator<StringListWrapper> entries = entrySet.GetEnumerator();
            return new AnonymousIEnumerator(this);
        }

        private sealed class AnonymousIEnumerator : IEnumerator
        {
            public AnonymousIEnumerator(StringListWrapper parent)
            {
                this.parent = parent;
            }

            private readonly StringListWrapper parent;
            public bool HasNext()
            {
                return entries.HasNext();
            }

            public StringList Next()
            {
                return entries.Next().GetStringList();
            }

            public void Remove()
            {
                entries.Remove();
            }
        }

        /// <summary>
        /// Retrieves the number of tokens in the current instance.
        /// </summary>
        /// <returns>number of tokens</returns>
        public virtual int Size()
        {
            return entrySet.Count;
        }

        /// <summary>
        /// Writes the current instance to the given {@link OutputStream}.
        /// </summary>
        /// <param name="out">{@link OutputStream}</param>
        /// <exception cref="IOException"></exception>
        //public virtual void Serialize(OutputStream @out)
        //{
        //    IEnumerator<Entry> entryIterator = new AnonymousIEnumerator1(this);
        //    DictionaryEntryPersistor.Serialize(@out, entryIterator, isCaseSensitive);
        //}

        //private sealed class AnonymousIEnumerator1 : IEnumerator
        //{
        //    public AnonymousIEnumerator1(StringListWrapper parent)
        //    {
        //        this.parent = parent;
        //    }

        //    private readonly StringListWrapper parent;
        //    private IEnumerator<StringList> dictionaryIterator = this.Iterator();
        //    public bool HasNext()
        //    {
        //        return dictionaryIterator.HasNext();
        //    }

        //    public Entry Next()
        //    {
        //        StringList tokens = dictionaryIterator.Next();
        //        return new Entry(tokens, new Attributes());
        //    }

        //    public void Remove()
        //    {
        //        throw new NotSupportedException();
        //    }
        //}

        public virtual bool Equals(object obj)
        {
            bool result;
            if (obj == this)
            {
                result = true;
            }
            else if (obj is Dictionary)
            {
                Dictionary dictionary = (Dictionary)obj;
                result = entrySet.Equals(dictionary.entrySet);
            }
            else
            {
                result = false;
            }

            return result;
        }

        public virtual int GetHashCode()
        {
            return entrySet.GetHashCode();
        }

        public virtual string ToString()
        {
            return entrySet.ToString();
        }

        /// <summary>
        /// Reads a dictionary which has one entry per line. The tokens inside an
        /// entry are whitespace delimited.
        /// </summary>
        /// <param name="in">{@link Reader}</param>
        /// <returns>the parsed dictionary</returns>
        /// <exception cref="IOException"></exception>
        public static Dictionary ParseOneEntryPerLine(Reader @in)
        {
            BufferedReader lineReader = new BufferedReader(@in);
            Dictionary dictionary = new Dictionary();
            string line;
            while ((line = lineReader.ReadLine()) != null)
            {
                StringTokenizer whiteSpaceTokenizer = new StringTokenizer(line, " ");
                string[] tokens = new string[whiteSpaceTokenizer.CountTokens()];
                if (tokens.Length > 0)
                {
                    int tokenIndex = 0;
                    while (whiteSpaceTokenizer.HasMoreTokens())
                    {
                        tokens[tokenIndex++] = whiteSpaceTokenizer.NextToken();
                    }

                    dictionary.Put(new StringList(tokens));
                }
            }

            return dictionary;
        }

        /// <summary>
        /// Gets this dictionary as a {@code Set<String>}. Only {@code iterator()},
        /// {@code size()} and {@code contains(Object)} methods are implemented.
        ///
        /// If this dictionary entries are multi tokens only the first token of the
        /// entry will be part of the Set.
        /// </summary>
        /// <returns>a Set containing the entries of this dictionary</returns>
        public virtual HashSet<string> AsStringSet()
        {
            return new AnonymousAbstractSet(this);
        }

        private sealed class AnonymousIEnumerator2 : IEnumerator
        {
            public AnonymousIEnumerator2(StringListWrapper parent)
            {
                this.parent = parent;
            }

            private readonly StringListWrapper parent;
            public bool HasNext()
            {
                return entries.HasNext();
            }

            public string Next()
            {
                return entries.Next().GetStringList().GetToken(0);
            }

            public void Remove()
            {
                throw new NotSupportedException();
            }
        }

        private sealed class AnonymousAbstractSet : AbstractSet
        {
            public AnonymousAbstractSet(StringListWrapper parent)
            {
                this.parent = parent;
            }

            private readonly StringListWrapper parent;
            public IEnumerator<string> Iterator()
            {
                IEnumerator<StringListWrapper> entries = entrySet.Iterator();
                return new AnonymousIEnumerator2(this);
            }

            public int Size()
            {
                return entrySet.Count;
            }

            public bool Contains(object obj)
            {
                bool result = false;
                if (obj is string)
                {
                    string str = (string)obj;
                    result = entrySet.Contains(new StringListWrapper(new StringList(str)));
                }

                return result;
            }
        }

        /// <summary>
        /// Gets the Serializer Class for {@link Dictionary}
        /// </summary>
        /// <returns>{@link DictionarySerializer}</returns>
        //public virtual Class<TWildcardTodo> GetArtifactSerializerClass()
        //{
        //    return typeof(DictionarySerializer);
        //}
    }
}

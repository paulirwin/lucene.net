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
using System.Text;

namespace Opennlp.Tools.Util
{
    /// <summary>
    /// The {@link StringList} is an immutable list of {@link String}s.
    /// </summary>
    public class StringList : IEnumerable<string>
    {
        private string[] tokens;
        /// <summary>
        /// Initializes the current instance.
        ///
        /// Note: <br>
        /// Token String will be replaced by identical internal String object.
        /// </summary>
        /// <param name="singleToken">one single token</param>
        public StringList(string singleToken)
        {
            tokens = new string[]
            {
                string.Intern(singleToken)
            };
        }

        /// <summary>
        /// Initializes the current instance.
        ///
        /// Note: <br>
        /// Token Strings will be replaced by identical internal String object.
        /// </summary>
        /// <param name="tokens">the string parts of the new {@link StringList}, an empty
        ///     tokens array or null is not permitted.</param>
        public StringList(params string[] tokens)
        {
            if (tokens == null)
            {
                throw new ArgumentNullException(nameof(tokens), "tokens must not be null");
            }
            if (tokens.Length == 0)
            {
                throw new ArgumentException("tokens must not be empty");
            }

            this.tokens = new string[tokens.Length];
            for (int i = 0; i < tokens.Length; i++)
            {
                this.tokens[i] = string.Intern(tokens[i]);
            }
        }

        /// <summary>
        /// Retrieves a token from the given index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>token at the given index</returns>
        public virtual string GetToken(int index)
        {
            return tokens[index];
        }

        /// <summary>
        /// Retrieves the number of tokens inside this list.
        /// </summary>
        /// <returns>number of tokens</returns>
        public virtual int Size()
        {
            return tokens.Length;
        }

        /// <summary>
        /// Gets the number of tokens inside this list.
        /// </summary>
        public int Count => tokens.Length;

        /// <summary>
        /// Retrieves an {@link Iterator} over all tokens.
        /// </summary>
        /// <returns>iterator over tokens</returns>
        public virtual IEnumerator<string> Iterator()
        {
            return new AnonymousIEnumerator(this);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return Iterator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private sealed class AnonymousIEnumerator : IEnumerator<string>
        {
            public AnonymousIEnumerator(StringList parent)
            {
                this.parent = parent;
            }

            private readonly StringList parent;
            private int index = -1;

            public string Current
            {
                get
                {
                    if (index < 0 || index >= parent.tokens.Length)
                    {
                        throw new InvalidOperationException();
                    }
                    return parent.GetToken(index);
                }
            }

            object System.Collections.IEnumerator.Current => Current;

            public bool MoveNext()
            {
                return ++index < parent.tokens.Length;
            }

            public void Reset()
            {
                index = -1;
            }

            public void Dispose()
            {
            }
        }

        /// <summary>
        /// Compares to tokens list and ignores the case of the tokens.
        ///
        /// Note: This can cause problems with some locals.
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns>true if identically with ignore the case otherwise false</returns>
        public virtual bool CompareToIgnoreCase(StringList tokens)
        {
            if (Size() == tokens.Size())
            {
                for (int i = 0; i < Size(); i++)
                {
                    if (string.Compare(GetToken(i), tokens.GetToken(i), StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            foreach (string token in tokens)
            {
                hash.Add(token);
            }
            return hash.ToHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is StringList tokenList)
            {
                if (tokens.Length != tokenList.tokens.Length)
                {
                    return false;
                }
                for (int i = 0; i < tokens.Length; i++)
                {
                    if (!string.Equals(tokens[i], tokenList.tokens[i]))
                    {
                        return false;
                    }
                }
                return true;
            }

            return false;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append('[');
            for (int i = 0; i < Size(); i++)
            {
                sb.Append(GetToken(i));
                if (i < Size() - 1)
                {
                    sb.Append(',');
                }
            }

            sb.Append(']');
            return sb.ToString();
        }
    }
}

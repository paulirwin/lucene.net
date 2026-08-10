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
using Lucene.Net.Support;

namespace Opennlp.Tools.Util
{
    internal class TokenTag
    {
        private readonly string token;
        private readonly string tag;
        private readonly string[] addtionalData;
        public TokenTag(string token, string tag, String[] addtionalData)
        {
            this.token = token;
            this.tag = tag;
            if (addtionalData != null)
            {
                this.addtionalData = Arrays.CopyOf(addtionalData, addtionalData.Length);
            }
            else
            {
                this.addtionalData = null;
            }
        }

        public virtual string GetToken()
        {
            return token;
        }

        public virtual string GetTag()
        {
            return tag;
        }

        public virtual String[] GetAddtionalData()
        {
            return addtionalData;
        }

        public static String[] ExtractTokens(TokenTag[] tuples)
        {
            string[] tokens = new string[tuples.Length];
            for (int i = 0; i < tuples.Length; i++)
            {
                tokens[i] = tuples[i].GetToken();
            }

            return tokens;
        }

        public static String[] ExtractTags(TokenTag[] tuples)
        {
            string[] tags = new string[tuples.Length];
            for (int i = 0; i < tuples.Length; i++)
            {
                tags[i] = tuples[i].GetTag();
            }

            return tags;
        }

        public static TokenTag[] Create(string[] toks, string[] tags)
        {
            TokenTag[] tuples = new TokenTag[toks.Length];
            for (int i = 0; i < toks.Length; i++)
            {
                tuples[i] = new TokenTag(toks[i], tags[i], null);
            }

            return tuples;
        }

        public virtual bool Equals(object o)
        {
            if (this == o)
            {
                return true;
            }
            else if (o is TokenTag)
            {
                return Equals(this.token, ((TokenTag)o).token) && Equals(this.tag, ((TokenTag)o).tag) && Arrays.Equals(this.addtionalData, ((TokenTag)o).addtionalData);
            }

            return false;
        }

        public virtual int GetHashCode()
        {
            return HashCode.Combine(token, tag, addtionalData);
        }

        public virtual string ToString()
        {
            return token + "_" + tag;
        }
    }
}

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

namespace Opennlp.Tools.Chunker
{
    /// <summary>
    /// Features based on chunking model described in Fei Sha and Fernando Pereira. Shallow
    ///  parsing with conditional random fields. In Proceedings of HLT-NAACL 2003. Association
    ///  for Computational Linguistics, 2003.
    /// </summary>
    internal class DefaultChunkerContextGenerator : ChunkerContextGenerator
    {
        /// <summary>
        /// Creates the default context generator a chunker.
        /// </summary>
        public DefaultChunkerContextGenerator()
        {
        }

        public virtual string[] GetContext(int index, string[] tokens, string[] postags, string[] priorDecisions, object[] additionalContext)
        {
            return GetContext(index, tokens, postags, priorDecisions);
        }

        public virtual string[] GetContext(int i, string[] toks, string[] tags, string[] preds)
        {

            // Words in a 5-word window
            string w_2, w_1, w0, w1, w2;

            // Tags in a 5-word window
            string t_2, t_1, t0, t1, t2;

            // Previous predictions
            string p_2, p_1;
            if (i < 2)
            {
                w_2 = "w_2=bos";
                t_2 = "t_2=bos";
                p_2 = "p_2=bos";
            }
            else
            {
                w_2 = "w_2=" + toks[i - 2];
                t_2 = "t_2=" + tags[i - 2];
                p_2 = "p_2" + preds[i - 2];
            }

            if (i < 1)
            {
                w_1 = "w_1=bos";
                t_1 = "t_1=bos";
                p_1 = "p_1=bos";
            }
            else
            {
                w_1 = "w_1=" + toks[i - 1];
                t_1 = "t_1=" + tags[i - 1];
                p_1 = "p_1=" + preds[i - 1];
            }

            w0 = "w0=" + toks[i];
            t0 = "t0=" + tags[i];
            if (i + 1 >= toks.Length)
            {
                w1 = "w1=eos";
                t1 = "t1=eos";
            }
            else
            {
                w1 = "w1=" + toks[i + 1];
                t1 = "t1=" + tags[i + 1];
            }

            if (i + 2 >= toks.Length)
            {
                w2 = "w2=eos";
                t2 = "t2=eos";
            }
            else
            {
                w2 = "w2=" + toks[i + 2];
                t2 = "t2=" + tags[i + 2];
            }

            string[] features = new string[]
            {
                w_2,
                w_1,
                w0,
                w1,
                w2,
                w_1 + w0,
                w0 + w1,
                t_2,
                t_1,
                t0,
                t1,
                t2,
                t_2 + t_1,
                t_1 + t0,
                t0 + t1,
                t1 + t2,
                t_2 + t_1 + t0,
                t_1 + t0 + t1,
                t0 + t1 + t2,
                p_2,
                p_1,
                p_2 + p_1,
                p_1 + t_2,
                p_1 + t_1,
                p_1 + t0,
                p_1 + t1,
                p_1 + t2,
                p_1 + t_2 + t_1,
                p_1 + t_1 + t0,
                p_1 + t0 + t1,
                p_1 + t1 + t2,
                p_1 + t_2 + t_1 + t0,
                p_1 + t_1 + t0 + t1,
                p_1 + t0 + t1 + t2,
                p_1 + w_2,
                p_1 + w_1,
                p_1 + w0,
                p_1 + w1,
                p_1 + w2,
                p_1 + w_1 + w0,
                p_1 + w0 + w1
            };
            return features;
        }

        public virtual string[] GetContext(int index, TokenTag[] sequence, string[] priorDecisions, object[] additionalContext)
        {
            string[] token = TokenTag.ExtractTokens(sequence);
            string[] tags = TokenTag.ExtractTags(sequence);
            return GetContext(index, token, tags, priorDecisions, additionalContext);
        }
    }
}

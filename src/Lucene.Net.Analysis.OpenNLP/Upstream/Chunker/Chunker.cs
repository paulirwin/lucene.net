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

namespace Opennlp.Tools.Chunker
{
    /// <summary>
    /// The interface for chunkers which provide chunk tags for a sequence of tokens.
    /// </summary>
    public interface Chunker
    {
        /// <summary>
        /// Generates chunk tags for the given sequence returning the result in an array.
        /// </summary>
        /// <param name="toks">an array of the tokens or words of the sequence.</param>
        /// <param name="tags">an array of the pos tags of the sequence.</param>
        /// <returns>an array of chunk tags for each token in the sequence.</returns>
        string[] Chunk(string[] toks, string[] tags);
        /// <summary>
        /// Generates tagged chunk spans for the given sequence returning the result in a span array.
        /// </summary>
        /// <param name="toks">an array of the tokens or words of the sequence.</param>
        /// <param name="tags">an array of the pos tags of the sequence.</param>
        /// <returns>an array of spans with chunk tags for each chunk in the sequence.</returns>
        Span[] ChunkAsSpans(string[] toks, string[] tags);
        /// <summary>
        /// Returns the top k chunk sequences for the specified sentence with the specified pos-tags
        /// </summary>
        /// <param name="sentence">The tokens of the sentence.</param>
        /// <param name="tags">The pos-tags for the specified sentence.</param>
        /// <returns>the top k chunk sequences for the specified sentence.</returns>
        Sequence[] TopKSequences(string[] sentence, string[] tags);
        /// <summary>
        /// Returns the top k chunk sequences for the specified sentence with the specified pos-tags
        /// </summary>
        /// <param name="sentence">The tokens of the sentence.</param>
        /// <param name="tags">The pos-tags for the specified sentence.</param>
        /// <param name="minSequenceScore">A lower bound on the score of a returned sequence.</param>
        /// <returns>the top k chunk sequences for the specified sentence.</returns>
        Sequence[] TopKSequences(string[] sentence, string[] tags, double minSequenceScore);
    }
}

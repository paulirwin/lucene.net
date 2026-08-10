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

using System.Collections.Generic;

namespace Opennlp.Tools.Lemmatizer
{
    /// <summary>
    /// The interface for lemmatizers.
    /// </summary>
    internal interface Lemmatizer
    {
        /// <summary>
        /// Generates lemmas for the word and postag returning the result in an array.
        /// </summary>
        /// <param name="toks">an array of the tokens</param>
        /// <param name="tags">an array of the pos tags</param>
        /// <returns>an array of possible lemmas for each token in the sequence.</returns>
        string[] Lemmatize(string[] toks, string[] tags);
        /// <summary>
        /// Generates a lemma tags for the word and postag returning the result in a list
        /// of every possible lemma for each token and postag.
        /// </summary>
        /// <param name="toks">an array of the tokens</param>
        /// <param name="tags">an array of the pos tags</param>
        /// <returns>a list of every possible lemma for each token in the sequence.</returns>
        IList<IList<string>> Lemmatize(IList<string> toks, IList<string> tags);
    }
}

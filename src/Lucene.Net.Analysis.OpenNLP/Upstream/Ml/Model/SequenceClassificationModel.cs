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

namespace Opennlp.Tools.Ml.Model
{
    /// <summary>
    /// A classification model that can label an input sequence.
    /// </summary>
    /// <param name="<T>"></param>
    public interface SequenceClassificationModel<T>
    {
        /// <summary>
        /// Finds the sequence with the highest probability.
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="additionalContext"></param>
        /// <param name="cg"></param>
        /// <param name="validator"></param>
        /// <returns></returns>
        Sequence BestSequence(T[] sequence, object[] additionalContext, BeamSearchContextGenerator<T> cg, SequenceValidator<T> validator);
        /// <summary>
        /// Finds the n most probable sequences.
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="additionalContext"></param>
        /// <param name="cg"></param>
        /// <param name="validator"></param>
        /// <returns></returns>
        Sequence[] BestSequences(int numSequences, T[] sequence, object[] additionalContext, double minSequenceScore, BeamSearchContextGenerator<T> cg, SequenceValidator<T> validator);
        /// <summary>
        /// Finds the n most probable sequences.
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="additionalContext"></param>
        /// <param name="cg"></param>
        /// <param name="validator"></param>
        /// <returns></returns>
        Sequence[] BestSequences(int numSequences, T[] sequence, object[] additionalContext, BeamSearchContextGenerator<T> cg, SequenceValidator<T> validator);
        /// <summary>
        /// Returns all possible outcomes.
        /// </summary>
        /// <returns></returns>
        string[] GetOutcomes();
    }
}

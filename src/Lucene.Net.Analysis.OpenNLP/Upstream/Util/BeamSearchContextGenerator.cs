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

namespace Opennlp.Tools.Util
{
    /// <summary>
    /// Interface for context generators used with a sequence beam search.
    /// </summary>
    internal interface BeamSearchContextGenerator<T>
    {
        /// <summary>
        /// Returns the context for the specified position in the specified sequence (list).
        /// </summary>
        /// <param name="index">The index of the sequence.</param>
        /// <param name="sequence">The sequence of items over which the beam search is performed.</param>
        /// <param name="priorDecisions">The sequence of decisions made prior to the context for
        ///     which this decision is being made.</param>
        /// <param name="additionalContext">Any addition context specific to a class implementing this interface.</param>
        /// <returns>the context for the specified position in the specified sequence.</returns>
        string[] GetContext(int index, T[] sequence, string[] priorDecisions, object[] additionalContext);
    }
}

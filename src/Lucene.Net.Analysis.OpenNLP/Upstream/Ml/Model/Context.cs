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

namespace Opennlp.Tools.Ml.Model
{
    /// <summary>
    /// Class which associates a real valued parameter or expected value with a particular contextual
    /// predicate or feature.  This is used to store maxent model parameters as well as model and empirical
    /// expected values.
    /// </summary>
    internal class Context
    {
        /// <summary>
        /// The real valued parameters or expected values for this context.
        /// </summary>
        protected double[] parameters;
        /// <summary>
        /// The outcomes which occur with this context.
        /// </summary>
        protected int[] outcomes;
        /// <summary>
        /// Creates a new parameters object with the specified parameters associated with the specified
        /// outcome pattern.
        /// </summary>
        /// <param name="outcomePattern">Array of outcomes for which parameters exists for this context.</param>
        /// <param name="parameters">Parameters for the outcomes specified.</param>
        public Context(int[] outcomePattern, double[] parameters)
        {
            this.outcomes = outcomePattern;
            this.parameters = parameters;
        }

        /// <summary>
        /// Returns the outcomes for which parameters exists for this context.
        /// </summary>
        /// <returns>Array of outcomes for which parameters exists for this context.</returns>
        public virtual int[] GetOutcomes()
        {
            return outcomes;
        }

        /// <summary>
        /// Returns the parameters or expected values for the outcomes which occur with this context.
        /// </summary>
        /// <returns>Array of parameters for the outcomes of this context.</returns>
        public virtual double[] GetParameters()
        {
            return parameters;
        }

        public virtual int GetHashCode()
        {
            return HashCode.Combine(Arrays.GetHashCode(parameters), Arrays.GetHashCode(outcomes));
        }

        public virtual bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            if (obj is Context context)
            {
                return Arrays.Equals(parameters, context.parameters) && Arrays.Equals(outcomes, context.outcomes);
            }

            return false;
        }
    }
}

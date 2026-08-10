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
    /// This class encapsulates the varibales used in producing probabilities from a model
    /// and facilitaes passing these variables to the eval method.
    /// </summary>
    internal class EvalParameters
    {
        /// <summary>
        /// Mapping between outcomes and parameter values for each context.
        /// The integer representation of the context can be found using <code>pmap</code>.
        /// </summary>
        private Context[] @params;
        /// <summary>
        /// The number of outcomes being predicted.
        /// </summary>
        private readonly int numOutcomes;
        /// <summary>
        /// The maximum number of features fired in an event. Usually referred to as C.
        /// This is used to normalize the number of features which occur in an event.
        /// </summary>
        private double correctionConstant;

        public EvalParameters(Context[] @params, int numOutcomes)
        {
            this.@params = @params;
            this.numOutcomes = numOutcomes;
        }

        public virtual Context[] GetParams()
        {
            return @params;
        }

        public virtual int GetNumOutcomes()
        {
            return numOutcomes;
        }

        public virtual int GetHashCode()
        {
            return HashCode.Combine(Arrays.GetHashCode(@params), numOutcomes, correctionConstant);
        }

        public virtual bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            if (obj is EvalParameters evalParameters)
            {
                return Arrays.Equals(@params, evalParameters.@params) && numOutcomes == evalParameters.numOutcomes && correctionConstant == evalParameters.correctionConstant;
            }

            return false;
        }
    }
}

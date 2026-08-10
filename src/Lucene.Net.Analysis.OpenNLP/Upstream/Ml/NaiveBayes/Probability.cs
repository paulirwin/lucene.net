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

namespace Opennlp.Tools.Ml.Naivebayes
{
    // LUCENENET: non-generic base class
    internal abstract class Probability
    {
        public abstract double Get();
    }

    /// <summary>
    /// Class implementing the probability for a label.
    /// </summary>
    /// <param name="<T>">the label (category) class</param>
    internal class Probability<T> : Probability
    {
        protected T label;
        protected double probability = 1;

        public Probability(T label)
        {
            this.label = label;
        }

        /// <summary>
        /// Assigns a probability to a label, discarding any previously assigned probability.
        /// </summary>
        /// <param name="probability">the probability to assign</param>
        public virtual void Set(double probability)
        {
            this.probability = probability;
        }

        /// <summary>
        /// Assigns a probability to a label, discarding any previously assigned probability.
        /// </summary>
        /// <param name="probability">the probability to assign</param>
        public virtual void Set(Probability probability)
        {
            this.probability = probability.Get();
        }

        /// <summary>
        /// Assigns a probability to a label, discarding any previously assigned probability,
        /// if the new probability is greater than the old one.
        /// </summary>
        /// <param name="probability">the probability to assign</param>
        public virtual void SetIfLarger(double probability)
        {
            if (this.probability < probability)
            {
                this.probability = probability;
            }
        }

        /// <summary>
        /// Assigns a probability to a label, discarding any previously assigned probability,
        /// if the new probability is greater than the old one.
        /// </summary>
        /// <param name="probability">the probability to assign</param>
        public virtual void SetIfLarger(Probability probability)
        {
            if (this.probability < probability.Get())
            {
                this.probability = probability.Get();
            }
        }

        /// <summary>
        /// Checks if a probability is greater than the old one.
        /// </summary>
        /// <param name="probability">the probability to assign</param>
        public virtual bool IsLarger(Probability probability)
        {
            return this.probability < probability.Get();
        }

        /// <summary>
        /// Assigns a log probability to a label, discarding any previously assigned probability.
        /// </summary>
        /// <param name="probability">the log probability to assign</param>
        public virtual void SetLog(double probability)
        {
            Set(Math.Exp(probability));
        }

        /// <summary>
        /// Compounds the existing probability mass on the label with the new probability passed in to the method.
        /// </summary>
        /// <param name="probability">the probability weight to add</param>
        public virtual void AddIn(double probability)
        {
            Set(this.probability * probability);
        }

        /// <summary>
        /// Returns the probability associated with a label
        /// </summary>
        /// <returns>the probability associated with the label</returns>
        public override double Get()
        {
            return probability;
        }

        /// <summary>
        /// Returns the log probability associated with a label
        /// </summary>
        /// <returns>the log probability associated with the label</returns>
        public virtual double GetLog()
        {
            return Math.Log(Get());
        }

        /// <summary>
        /// Returns the probabilities associated with all labels
        /// </summary>
        /// <returns>the HashMap of labels and their probabilities</returns>
        public virtual T GetLabel()
        {
            return label;
        }

        public virtual string ToString()
        {
            return label == null ? "" + probability : label.ToString() + ":" + probability;
        }
    }
}

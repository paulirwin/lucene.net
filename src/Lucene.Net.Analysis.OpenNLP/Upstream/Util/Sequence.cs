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
using Lucene.Net.Support;
using Lucene.Net.Util;

namespace Opennlp.Tools.Util
{
    /// <summary>
    /// Represents a weighted sequence of outcomes.
    /// </summary>
    internal class Sequence : IComparable<Sequence>
    {
        private double score;
        private IList<string> outcomes;
        private IList<double> probs;
        private static readonly double ONE = 1;
        /// <summary>
        /// Creates a new sequence of outcomes.
        /// </summary>
        public Sequence()
        {
            outcomes = new List<string>(1);
            probs = new List<double>(1);
            score = 0;
        }

        public Sequence(Sequence s)
        {
            outcomes = new List<string>(s.outcomes.Count + 1);
            outcomes.AddRange(s.outcomes);
            probs = new List<double>(s.probs.Count + 1);
            probs.AddRange(s.probs);
            score = s.score;
        }

        public Sequence(Sequence s, string outcome, double p)
        {
            outcomes = new List<string>(s.outcomes.Count + 1);
            outcomes.AddRange(s.outcomes);
            outcomes.Add(outcome);
            probs = new List<double>(s.probs.Count + 1);
            probs.AddRange(s.probs);
            probs.Add(p);
            score = s.score + Math.Log(p);
        }

        public Sequence(IList<string> outcomes)
        {
            this.outcomes = outcomes;
            this.probs = Collections.NCopies(outcomes.Count, ONE);
        }

        public virtual int CompareTo(Sequence s)
        {
            return s.score.CompareTo(score);
        }

        public virtual int GetHashCode()
        {
            return HashCode.Combine(outcomes, probs, score);
        }

        public virtual bool Equals(object obj)
        {
            if (this == obj)
                return true;
            if (obj is Sequence)
            {
                Sequence other = (Sequence)obj;
                double epsilon = 1E-07;
                return Equals(outcomes, other.outcomes) && Equals(probs, other.probs) && Math.Abs(score - other.score) < epsilon;
            }

            return false;
        }

        /// <summary>
        /// Adds an outcome and probability to this sequence.
        /// </summary>
        /// <param name="outcome">the outcome to be added.</param>
        /// <param name="p">the probability associated with this outcome.</param>
        public virtual void Add(string outcome, double p)
        {
            outcomes.Add(outcome);
            probs.Add(p);
            score += Math.Log(p);
        }

        /// <summary>
        /// Returns a list of outcomes for this sequence.
        /// </summary>
        /// <returns>a list of outcomes.</returns>
        public virtual IList<string> GetOutcomes()
        {
            return outcomes;
        }

        /// <summary>
        /// Returns an array of probabilities associated with the outcomes of this sequence.
        /// </summary>
        /// <returns>an array of probabilities.</returns>
        public virtual double[] GetProbs()
        {
            double[] ps = new double[probs.Count];
            GetProbs(ps);
            return ps;
        }

        /// <summary>
        /// Returns the score of this sequence.
        /// </summary>
        /// <returns>The score of this sequence.</returns>
        public virtual double GetScore()
        {
            return score;
        }

        /// <summary>
        /// Populates  an array with the probabilities associated with the outcomes of this sequence.
        /// </summary>
        /// <param name="ps">a pre-allocated array to use to hold the values of the
        ///           probabilities of the outcomes for this sequence.</param>
        public virtual void GetProbs(double[] ps)
        {
            for (int pi = 0, pl = probs.Count; pi < pl; pi++)
            {
                ps[pi] = probs[pi];
            }
        }

        public virtual string ToString()
        {
            return score + " " + outcomes;
        }
    }
}

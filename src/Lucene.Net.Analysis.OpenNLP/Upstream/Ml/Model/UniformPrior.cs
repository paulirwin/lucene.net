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

namespace Opennlp.Tools.Ml.Model
{
    /// <summary>
    /// Provide a maximum entropy model with a uniform prior.
    /// </summary>
    internal class UniformPrior : Prior
    {
        private int numOutcomes;
        private double r;
        public virtual void LogPrior(double[] dist, int[] context, float[] values)
        {
            for (int oi = 0; oi < numOutcomes; oi++)
            {
                dist[oi] = r;
            }
        }

        public virtual void LogPrior(double[] dist, Context[] context, float[] values)
        {
            LogPrior(dist, (int[])null, values);
        }

        public virtual void LogPrior(double[] dist, int[] context)
        {
            LogPrior(dist, context, null);
        }

        public virtual void SetLabels(string[] outcomeLabels, string[] contextLabels)
        {
            this.numOutcomes = outcomeLabels.Length;
            // LUCENENET: 1.0 (not 1) is required here; integer division would yield 0,
            // making r negative infinity and every probability NaN.
            r = Math.Log(1.0 / numOutcomes);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(numOutcomes, r);
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            if (obj is UniformPrior)
            {
                UniformPrior prior = (UniformPrior)obj;
                return numOutcomes == prior.numOutcomes && r == prior.r;
            }

            return false;
        }
    }
}

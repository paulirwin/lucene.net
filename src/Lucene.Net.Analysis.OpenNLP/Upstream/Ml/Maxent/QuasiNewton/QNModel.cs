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

using Opennlp.Tools.Ml.Model;
using System;

namespace Opennlp.Tools.Ml.Maxent.Quasinewton
{
    internal class QNModel : AbstractModel
    {
        public QNModel(Context[] @params, String[] predLabels, String[] outcomeNames) : base(@params, predLabels, outcomeNames)
        {
            this.modelType = ModelType.MaxentQn;
        }

        public virtual int GetNumOutcomes()
        {
            return this.outcomeNames.Length;
        }

        private Context GetPredIndex(string predicate)
        {
            // LUCENENET: Java's Map.get() returns null for an absent key; the .NET indexer throws.
            pmap.TryGetValue(predicate, out Context value);
            return value;
        }

        public override double[] Eval(string[] context)
        {
            return Eval(context, new double[evalParams.GetNumOutcomes()]);
        }

        public override double[] Eval(string[] context, double[] probs)
        {
            return Eval(context, null, probs);
        }

        public override double[] Eval(string[] context, float[] values)
        {
            return Eval(context, values, new double[evalParams.GetNumOutcomes()]);
        }

        /// <summary>
        /// Model evaluation which should be used during inference.
        /// </summary>
        /// <param name="context">
        ///          The predicates which have been observed at the present
        ///          decision point.</param>
        /// <param name="values">
        ///          Weights of the predicates which have been observed at
        ///          the present decision point.</param>
        /// <param name="probs">
        ///          Probability for outcomes.</param>
        /// <returns>Normalized probabilities for the outcomes given the context.</returns>
        private double[] Eval(string[] context, float[] values, double[] probs)
        {
            for (int ci = 0; ci < context.Length; ci++)
            {
                Context pred = GetPredIndex(context[ci]);
                if (pred != null)
                {
                    double predValue = 1;
                    if (values != null)
                        predValue = values[ci];
                    double[] parameters = pred.GetParameters();
                    int[] outcomes = pred.GetOutcomes();
                    for (int i = 0; i < outcomes.Length; i++)
                    {
                        int oi = outcomes[i];
                        probs[oi] += predValue * parameters[i];
                    }
                }
            }

            double logSumExp = ArrayMath.LogSumOfExps(probs);
            for (int oi = 0; oi < outcomeNames.Length; oi++)
            {
                probs[oi] = Math.Exp(probs[oi] - logSumExp);
            }

            return probs;
        }

        /// <summary>
        /// Model evaluation which should be used during training to report model accuracy.
        /// </summary>
        /// <param name="context">
        ///          Indices of the predicates which have been observed at the present
        ///          decision point.</param>
        /// <param name="values">
        ///          Weights of the predicates which have been observed at
        ///          the present decision point.</param>
        /// <param name="probs">
        ///          Probability for outcomes</param>
        /// <param name="nOutcomes">
        ///          Number of outcomes</param>
        /// <param name="nPredLabels">
        ///          Number of unique predicates</param>
        /// <param name="parameters">
        ///          Model parameters</param>
        /// <returns>Normalized probabilities for the outcomes given the context.</returns>
        static double[] Eval(int[] context, float[] values, double[] probs, int nOutcomes, int nPredLabels, double[] parameters)
        {
            for (int i = 0; i < context.Length; i++)
            {
                int predIdx = context[i];
                double predValue = values != null ? values[i] : 1;
                for (int oi = 0; oi < nOutcomes; oi++)
                {
                    probs[oi] += predValue * parameters[oi * nPredLabels + predIdx];
                }
            }

            double logSumExp = ArrayMath.LogSumOfExps(probs);
            for (int oi = 0; oi < nOutcomes; oi++)
            {
                probs[oi] = Math.Exp(probs[oi] - logSumExp);
            }

            return probs;
        }
    }
}

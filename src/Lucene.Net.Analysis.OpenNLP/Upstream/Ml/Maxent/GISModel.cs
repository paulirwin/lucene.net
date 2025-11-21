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

namespace Opennlp.Tools.Ml.Maxent
{
    /// <summary>
    /// A maximum entropy model which has been trained using the Generalized
    /// Iterative Scaling procedure (implemented in GIS.java).
    /// </summary>
    public sealed class GISModel : AbstractModel
    {
        /// <summary>
        /// Creates a new model with the specified parameters, outcome names, and
        /// predicate/feature labels.
        /// </summary>
        /// <param name="params">
        ///          The parameters of the model.</param>
        /// <param name="predLabels">
        ///          The names of the predicates used in this model.</param>
        /// <param name="outcomeNames">
        ///          The names of the outcomes this model predicts.</param>
        public GISModel(Context[] @params, String[] predLabels, String[] outcomeNames) : this(@params, predLabels, outcomeNames, new UniformPrior())
        {
        }

        /// <summary>
        /// Creates a new model with the specified parameters, outcome names, and
        /// predicate/feature labels.
        /// </summary>
        /// <param name="params">
        ///          The parameters of the model.</param>
        /// <param name="predLabels">
        ///          The names of the predicates used in this model.</param>
        /// <param name="outcomeNames">
        ///          The names of the outcomes this model predicts.</param>
        /// <param name="prior">
        ///          The prior to be used with this model.</param>
        public GISModel(Context[] @params, String[] predLabels, String[] outcomeNames, Prior prior) : base(@params, predLabels, outcomeNames)
        {
            this.prior = prior;
            prior.SetLabels(outcomeNames, predLabels);
            modelType = ModelType.Maxent;
        }

        /// <summary>
        /// Use this model to evaluate a context and return an array of the likelihood
        /// of each outcome given that context.
        /// </summary>
        /// <param name="context">
        ///          The names of the predicates which have been observed at the
        ///          present decision point.</param>
        /// <returns>The normalized probabilities for the outcomes given the context.
        ///         The indexes of the double[] are the outcome ids, and the actual
        ///         string representation of the outcomes can be obtained from the
        ///         method getOutcome(int i).</returns>
        public override double[] Eval(string[] context)
        {
            return (Eval(context, new double[evalParams.GetNumOutcomes()]));
        }

        public override double[] Eval(string[] context, float[] values)
        {
            return (Eval(context, values, new double[evalParams.GetNumOutcomes()]));
        }

        public override double[] Eval(string[] context, double[] outsums)
        {
            return Eval(context, null, outsums);
        }

        /// <summary>
        /// Use this model to evaluate a context and return an array of the likelihood
        /// of each outcome given that context.
        /// </summary>
        /// <param name="context">
        ///          The names of the predicates which have been observed at the
        ///          present decision point.</param>
        /// <param name="outsums">
        ///          This is where the distribution is stored.</param>
        /// <returns>The normalized probabilities for the outcomes given the context.
        ///         The indexes of the double[] are the outcome ids, and the actual
        ///         string representation of the outcomes can be obtained from the
        ///         method getOutcome(int i).</returns>
        public double[] Eval(string[] context, float[] values, double[] outsums)
        {
            Context[] scontexts = new Context[context.Length];
            for (int i = 0; i < context.Length; i++)
            {
                scontexts[i] = pmap[context[i]];
            }

            prior.LogPrior(outsums, scontexts, values);
            return GISModel.Eval(scontexts, values, outsums, evalParams);
        }

        /// <summary>
        /// Use this model to evaluate a context and return an array of the likelihood
        /// of each outcome given the specified context and the specified parameters.
        /// </summary>
        /// <param name="context">
        ///          The integer values of the predicates which have been observed at
        ///          the present decision point.</param>
        /// <param name="prior">
        ///          The prior distribution for the specified context.</param>
        /// <param name="model">
        ///          The set of parametes used in this computation.</param>
        /// <returns>The normalized probabilities for the outcomes given the context.
        ///         The indexes of the double[] are the outcome ids, and the actual
        ///         string representation of the outcomes can be obtained from the
        ///         method getOutcome(int i).</returns>
        public static double[] Eval(int[] context, double[] prior, EvalParameters model)
        {
            return Eval(context, null, prior, model);
        }

        /// <summary>
        /// Use this model to evaluate a context and return an array of the likelihood
        /// of each outcome given the specified context and the specified parameters.
        /// </summary>
        /// <param name="context">
        ///          The integer values of the predicates which have been observed at
        ///          the present decision point.</param>
        /// <param name="values">
        ///          The values for each of the parameters.</param>
        /// <param name="prior">
        ///          The prior distribution for the specified context.</param>
        /// <param name="model">
        ///          The set of parametes used in this computation.</param>
        /// <returns>The normalized probabilities for the outcomes given the context.
        ///         The indexes of the double[] are the outcome ids, and the actual
        ///         string representation of the outcomes can be obtained from the
        ///         method getOutcome(int i).</returns>
        static double[] Eval(int[] context, float[] values, double[] prior, EvalParameters model)
        {
            Context[] scontexts = new Context[context.Length];
            for (int i = 0; i < context.Length; i++)
            {
                scontexts[i] = model.GetParams()[context[i]];
            }

            return GISModel.Eval(scontexts, values, prior, model);
        }

        /// <summary>
        /// Use this model to evaluate a context and return an array of the likelihood
        /// of each outcome given the specified context and the specified parameters.
        /// </summary>
        /// <param name="context">
        ///          The integer values of the predicates which have been observed at
        ///          the present decision point.</param>
        /// <param name="values">
        ///          The values for each of the parameters.</param>
        /// <param name="prior">
        ///          The prior distribution for the specified context.</param>
        /// <param name="model">
        ///          The set of parametes used in this computation.</param>
        /// <returns>The normalized probabilities for the outcomes given the context.
        ///         The indexes of the double[] are the outcome ids, and the actual
        ///         string representation of the outcomes can be obtained from the
        ///         method getOutcome(int i).</returns>
        static double[] Eval(Context[] context, float[] values, double[] prior, EvalParameters model)
        {
            ArrayMath.SumFeatures(context, values, prior);
            double normal = 0;
            for (int oid = 0; oid < model.GetNumOutcomes(); oid++)
            {
                prior[oid] = Math.Exp(prior[oid]);
                normal += prior[oid];
            }

            for (int oid = 0; oid < model.GetNumOutcomes(); oid++)
            {
                prior[oid] /= normal;
            }

            return prior;
        }
    }
}

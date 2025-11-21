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
using System.Collections.Generic;
using Lucene.Net.Support;

namespace Opennlp.Tools.Ml.Naivebayes
{
    /// <summary>
    /// Class implementing the multinomial Naive Bayes classifier model.
    /// </summary>
    public class NaiveBayesModel : AbstractModel
    {
        protected double[] outcomeTotals;
        protected long vocabulary;
        NaiveBayesModel(Context[] @params, String[] predLabels, Dictionary<string, Context> pmap, String[] outcomeNames) : base(@params, predLabels, pmap, outcomeNames)
        {
            outcomeTotals = InitOutcomeTotals(outcomeNames, @params);
            this.evalParams = new NaiveBayesEvalParameters(@params, outcomeNames.Length, outcomeTotals, predLabels.Length);
            modelType = ModelType.NaiveBayes;
        }

        public NaiveBayesModel(Context[] @params, String[] predLabels, String[] outcomeNames) : base(@params, predLabels, outcomeNames)
        {
            outcomeTotals = InitOutcomeTotals(outcomeNames, @params);
            this.evalParams = new NaiveBayesEvalParameters(@params, outcomeNames.Length, outcomeTotals, predLabels.Length);
            modelType = ModelType.NaiveBayes;
        }

        protected virtual double[] InitOutcomeTotals(string[] outcomeNames, Context[] @params)
        {
            double[] outcomeTotals = new double[outcomeNames.Length];
            for (int i = 0; i < @params.Length; ++i)
            {
                Context context = @params[i];
                for (int j = 0; j < context.GetOutcomes().Length; ++j)
                {
                    int outcome = context.GetOutcomes()[j];
                    double count = context.GetParameters()[j];
                    outcomeTotals[outcome] += count;
                }
            }

            return outcomeTotals;
        }

        public override double[] Eval(string[] context)
        {
            return Eval(context, new double[evalParams.GetNumOutcomes()]);
        }

        public override double[] Eval(string[] context, float[] values)
        {
            return Eval(context, values, new double[evalParams.GetNumOutcomes()]);
        }

        public override double[] Eval(string[] context, double[] probs)
        {
            return Eval(context, null, probs);
        }

        public virtual double[] Eval(string[] context, float[] values, double[] outsums)
        {
            Context[] scontexts = new Context[context.Length];
            Arrays.Fill(outsums, 0);
            for (int i = 0; i < context.Length; i++)
            {
                scontexts[i] = pmap[context[i]];
            }

            return Eval(scontexts, values, outsums, evalParams, true);
        }

        public static double[] Eval(int[] context, double[] prior, EvalParameters model)
        {
            return Eval(context, null, prior, model, true);
        }

        static double[] Eval(Context[] context, float[] values, double[] prior, EvalParameters model, bool normalize)
        {
            Probabilities<int> probabilities = new LogProbabilities<int>();
            double[] outcomeTotals = model is NaiveBayesEvalParameters ? ((NaiveBayesEvalParameters)model).GetOutcomeTotals() : new double[prior.Length];
            long vocabulary = model is NaiveBayesEvalParameters ? ((NaiveBayesEvalParameters)model).GetVocabulary() : 0;
            double[] activeParameters;
            int[] activeOutcomes;
            double value = 1;
            for (int ci = 0; ci < context.Length; ci++)
            {
                if (context[ci] != null)
                {
                    Context predParams = context[ci];
                    activeOutcomes = predParams.GetOutcomes();
                    activeParameters = predParams.GetParameters();
                    if (values != null)
                    {
                        value = values[ci];
                    }

                    int ai = 0;
                    for (int i = 0; i < outcomeTotals.Length && ai < activeOutcomes.Length; ++i)
                    {
                        int oid = activeOutcomes[ai];
                        double numerator = oid == i ? activeParameters[ai++] * value : 0;
                        double denominator = outcomeTotals[i];
                        probabilities.AddIn(i, GetProbability(numerator, denominator, vocabulary, true), 1);
                    }
                }
            }

            double total = 0;
            for (int i = 0; i < outcomeTotals.Length; ++i)
            {
                total += outcomeTotals[i];
            }

            for (int i = 0; i < outcomeTotals.Length; ++i)
            {
                double numerator = outcomeTotals[i];
                probabilities.AddIn(i, numerator / total, 1);
            }

            for (int i = 0; i < outcomeTotals.Length; ++i)
            {
                prior[i] = probabilities.Get(i).GetValueOrDefault(); // LUCENENET TODO: confirm GetValueOrDefault() is appropriate, it seems upstream would just throw
            }

            return prior;
        }

        static double[] Eval(int[] context, float[] values, double[] prior, EvalParameters model, bool normalize)
        {
            Context[] scontexts = new Context[context.Length];
            for (int i = 0; i < context.Length; i++)
            {
                scontexts[i] = model.GetParams()[context[i]];
            }

            return Eval(scontexts, values, prior, model, normalize);
        }

        private static double GetProbability(double numerator, double denominator, double vocabulary, bool isSmoothed)
        {
            if (isSmoothed)
                return GetSmoothedProbability(numerator, denominator, vocabulary);
            else if (denominator == 0 || denominator < Double.MinValue)
                return 0;
            else
                return 1 * numerator / denominator;
        }

        private static double GetSmoothedProbability(double numerator, double denominator, double vocabulary)
        {
            double delta = 0.05; // Lidstone smoothing
            return 1 * (numerator + delta) / (denominator + delta * vocabulary);
        }
    }
}

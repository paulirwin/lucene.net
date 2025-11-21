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
using Opennlp.Tools.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using J2N.Collections.Generic;

namespace Opennlp.Tools.Ml
{
    // LUCENENET specific non-generic base class for constants
    public abstract class BeamSearch
    {
        public static readonly string BEAM_SIZE_PARAMETER = "BeamSize";
    }

    /// <summary>
    /// Performs k-best search over sequence.  This is based on the description in
    /// Ratnaparkhi (1998), PhD diss, Univ. of Pennsylvania.
    /// </summary>
    /// <remarks>
    /// @seeSequence
    /// @seeSequenceValidator
    /// @seeBeamSearchContextGenerator
    /// </remarks>
    public class BeamSearch<T> : BeamSearch, SequenceClassificationModel<T>
    {
        private static readonly object[] EMPTY_ADDITIONAL_CONTEXT = new object[0];
        protected int size;
        protected MaxentModel model;
        private double[] probs;
        private Cache<string[], double[]> contextsCache;
        private static readonly int zeroLog = -100000;

        /// <summary>
        /// Creates new search object.
        /// </summary>
        /// <param name="size">The size of the beam (k).</param>
        /// <param name="model">the model for assigning probabilities to the sequence outcomes.</param>
        public BeamSearch(int size, MaxentModel model) : this(size, model, 0)
        {
        }

        public BeamSearch(int size, MaxentModel model, int cacheSize)
        {
            this.size = size;
            this.model = model;
            if (cacheSize > 0)
            {
                contextsCache = new Cache<string[], double[]>(cacheSize);
            }

            this.probs = new double[model.GetNumOutcomes()];
        }

        /// <summary>
        /// Returns the best sequence of outcomes based on model for this object.
        /// </summary>
        /// <param name="sequence">The input sequence.</param>
        /// <param name="additionalContext">An Object[] of additional context.
        ///     This is passed to the context generator blindly with the
        ///     assumption that the context are appropiate.</param>
        /// <returns>The top ranked sequence of outcomes or null if no sequence could be found</returns>
        public virtual Sequence[] BestSequences(int numSequences, T[] sequence, object[] additionalContext,
            double minSequenceScore, BeamSearchContextGenerator<T> cg, SequenceValidator<T> validator)
        {
            var prev = new PriorityQueue<Sequence>(size);
            var next = new PriorityQueue<Sequence>(size);
            PriorityQueue<Sequence> tmp;
            prev.Add(new Sequence());
            if (additionalContext == null)
            {
                additionalContext = EMPTY_ADDITIONAL_CONTEXT;
            }

            for (int i = 0; i < sequence.Length; i++)
            {
                int sz = Math.Min(size, prev.Count);
                for (int sc = 0; prev.Count > 0 && sc < sz; sc++)
                {
                    Sequence top = prev.Dequeue();
                    IList<string> tmpOutcomes = top.GetOutcomes();
                    string[] outcomes = tmpOutcomes.ToArray();
                    string[] contexts = cg.GetContext(i, sequence, outcomes, additionalContext);
                    double[] scores;
                    if (contextsCache != null)
                    {
                        scores = contextsCache.ComputeIfAbsent(contexts, (c) => model.Eval(c, probs));
                    }
                    else
                    {
                        scores = model.Eval(contexts, probs);
                    }

                    double[] temp_scores = new double[scores.Length];
                    Array.Copy(scores, 0, temp_scores, 0, scores.Length);
                    Array.Sort(temp_scores);
                    double min = temp_scores[Math.Max(0, scores.Length - size)];
                    for (int p = 0; p < scores.Length; p++)
                    {
                        if (scores[p] >= min)
                        {
                            string @out = model.GetOutcome(p);
                            if (validator.ValidSequence(i, sequence, outcomes, @out))
                            {
                                Sequence ns = new Sequence(top, @out, scores[p]);
                                if (ns.GetScore() > minSequenceScore)
                                {
                                    next.Add(ns);
                                }
                            }
                        }
                    }

                    if (next.Count == 0)
                    {
                        //if no advanced sequences, advance all valid
                        for (int p = 0; p < scores.Length; p++)
                        {
                            string @out = model.GetOutcome(p);
                            if (validator.ValidSequence(i, sequence, outcomes, @out))
                            {
                                Sequence ns = new Sequence(top, @out, scores[p]);
                                if (ns.GetScore() > minSequenceScore)
                                {
                                    next.Add(ns);
                                }
                            }
                        }
                    }
                }


                //    make prev = next; and re-init next (we reuse existing prev set once we clear it)
                prev.Clear();
                tmp = prev;
                prev = next;
                next = tmp;
            }

            int numSeq = Math.Min(numSequences, prev.Count);
            Sequence[] topSequences = new Sequence[numSeq];
            for (int seqIndex = 0; seqIndex < numSeq; seqIndex++)
            {
                topSequences[seqIndex] = prev.Dequeue();
            }

            return topSequences;
        }

        public virtual Sequence[] BestSequences(int numSequences, T[] sequence, object[] additionalContext,
            BeamSearchContextGenerator<T> cg, SequenceValidator<T> validator)
        {
            return BestSequences(numSequences, sequence, additionalContext, zeroLog, cg, validator);
        }

        public virtual Sequence BestSequence(T[] sequence, object[] additionalContext, BeamSearchContextGenerator<T> cg,
            SequenceValidator<T> validator)
        {
            Sequence[] sequences = BestSequences(1, sequence, additionalContext, cg, validator);
            if (sequences.Length > 0)
                return sequences[0];
            else
                return null;
        }

        public virtual string[] GetOutcomes()
        {
            string[] outcomes = new string[model.GetNumOutcomes()];
            for (int i = 0; i < model.GetNumOutcomes(); i++)
            {
                outcomes[i] = model.GetOutcome(i);
            }

            return outcomes;
        }
    }
}

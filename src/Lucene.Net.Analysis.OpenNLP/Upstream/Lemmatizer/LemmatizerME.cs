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

using Opennlp.Tools.Ml;
using Opennlp.Tools.Ml.Model;
using Opennlp.Tools.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Opennlp.Tools.Lemmatizer
{
    /// <summary>
    /// A probabilistic lemmatizer.  Tries to predict the induced permutation class
    /// for each word depending on its surrounding context. Based on
    /// Grzegorz Chrupała. 2008. Towards a Machine-Learning Architecture
    /// for Lexical Functional Grammar Parsing. PhD dissertation, Dublin City University.
    /// http://grzegorz.chrupala.me/papers/phd-single.pdf
    /// </summary>
    internal class LemmatizerME : Lemmatizer
    {
        public static readonly int LEMMA_NUMBER = 29;
        public static readonly int DEFAULT_BEAM_SIZE = 3;
        protected int beamSize;
        private Sequence bestSequence;
        private SequenceClassificationModel<string> model;
        private LemmatizerContextGenerator contextGenerator;
        private SequenceValidator<string> sequenceValidator;
        /// <summary>
        /// Initializes the current instance with the provided model
        /// and the default beam size of 3.
        /// </summary>
        /// <param name="model">the model</param>
        public LemmatizerME(LemmatizerModel model)
        {
            LemmatizerFactory factory = model.GetFactory();
            int defaultBeamSize = LemmatizerME.DEFAULT_BEAM_SIZE;
            string beamSizeString = model.GetManifestProperty(BeamSearch.BEAM_SIZE_PARAMETER);
            if (beamSizeString != null)
            {
                defaultBeamSize = int.Parse(beamSizeString);
            }

            contextGenerator = factory.GetContextGenerator();
            beamSize = defaultBeamSize;
            sequenceValidator = factory.GetSequenceValidator();
            if (model.GetLemmatizerSequenceModel() != null)
            {
                this.model = model.GetLemmatizerSequenceModel();
            }
            else
            {
                this.model = new BeamSearch<string>(beamSize, (MaxentModel)model.GetLemmatizerSequenceModel(), 0);
            }
        }

        public virtual string[] Lemmatize(string[] toks, string[] tags)
        {
            string[] ses = PredictSES(toks, tags);
            string[] lemmas = DecodeLemmas(toks, ses);
            return lemmas;
        }

        public virtual IList<IList<string>> Lemmatize(IList<string> toks, IList<string> tags)
        {
            string[] tokens = toks.ToArray();
            string[] posTags = tags.ToArray();
            string[][] allLemmas = PredictLemmas(LEMMA_NUMBER, tokens, posTags);
            IList<IList<string>> predictedLemmas = new List<IList<string>>();
            for (int i = 0; i < allLemmas.Length; i++)
            {
                predictedLemmas.Add(allLemmas[i]);
            }

            return predictedLemmas;
        }

        /// <summary>
        /// Predict Short Edit Script (automatically induced lemma class).
        /// </summary>
        /// <param name="toks">the array of tokens</param>
        /// <param name="tags">the array of pos tags</param>
        /// <returns>an array containing the lemma classes</returns>
        public virtual String[] PredictSES(string[] toks, string[] tags)
        {
            bestSequence = model.BestSequence(toks, new object[] { tags }, contextGenerator, sequenceValidator);
            IList<string> ses = bestSequence.GetOutcomes();
            return ses.ToArray();
        }

        /// <summary>
        /// Predict all possible lemmas (using a default upper bound).
        /// </summary>
        /// <param name="numLemmas">the default number of lemmas</param>
        /// <param name="toks">the tokens</param>
        /// <param name="tags">the postags</param>
        /// <returns>a double array containing all posible lemmas for each token and postag pair</returns>
        public virtual string[][] PredictLemmas(int numLemmas, string[] toks, string[] tags)
        {
            Sequence[] bestSequences = model.BestSequences(numLemmas, toks, new object[] { tags }, contextGenerator, sequenceValidator);
            string[][] allLemmas = new string[bestSequences.Length][];
            for (int i = 0; i < allLemmas.Length; i++)
            {
                IList<string> ses = bestSequences[i].GetOutcomes();
                string[] sesArray = ses.ToArray();
                allLemmas[i] = DecodeLemmas(toks, sesArray);
            }

            return allLemmas;
        }

        /// <summary>
        /// Decodes the lemma from the word and the induced lemma class.
        /// </summary>
        /// <param name="toks">the array of tokens</param>
        /// <param name="preds">the predicted lemma classes</param>
        /// <returns>the array of decoded lemmas</returns>
        public static string[] DecodeLemmas(string[] toks, string[] preds)
        {
            IList<string> lemmas = new List<string>();
            for (int i = 0; i < toks.Length; i++)
            {
                string lemma = StringUtil.DecodeShortestEditScript(toks[i].ToLower(), preds[i]);
                if (lemma.Length == 0)
                {
                    lemma = "_";
                }

                lemmas.Add(lemma);
            }

            return lemmas.ToArray();
        }

        public static String[] EncodeLemmas(string[] toks, string[] lemmas)
        {
            IList<string> sesList = new List<string>();
            for (int i = 0; i < toks.Length; i++)
            {
                string ses = StringUtil.GetShortestEditScript(toks[i], lemmas[i]);
                if (ses.Length == 0)
                {
                    ses = "_";
                }

                sesList.Add(ses);
            }

            return sesList.ToArray();
        }

        public virtual Sequence[] TopKSequences(string[] sentence, string[] tags)
        {
            return model.BestSequences(DEFAULT_BEAM_SIZE, sentence, new object[] { tags }, contextGenerator, sequenceValidator);
        }

        public virtual Sequence[] TopKSequences(string[] sentence, string[] tags, double minSequenceScore)
        {
            return model.BestSequences(DEFAULT_BEAM_SIZE, sentence, new object[] { tags }, minSequenceScore, contextGenerator, sequenceValidator);
        }

        /// <summary>
        /// Populates the specified array with the probabilities of the last decoded sequence.  The
        /// sequence was determined based on the previous call to <code>lemmatize</code>.  The
        /// specified array should be at least as large as the number of tokens in the
        /// previous call to <code>lemmatize</code>.
        /// </summary>
        /// <param name="probs">An array used to hold the probabilities of the last decoded sequence.</param>
        public virtual void Probs(double[] probs)
        {
            bestSequence.GetProbs(probs);
        }

        /// <summary>
        /// Returns an array with the probabilities of the last decoded sequence.  The
        /// sequence was determined based on the previous call to <code>chunk</code>.
        /// </summary>
        /// <returns>An array with the same number of probabilities as tokens were sent to <code>chunk</code>
        ///     when it was last called.</returns>
        public virtual double[] Probs()
        {
            return bestSequence.GetProbs();
        }

        // public static LemmatizerModel Train(string languageCode, ObjectStream<LemmaSample> samples, TrainingParameters trainParams, LemmatizerFactory posFactory)
        // {
        //     int beamSize = trainParams.GetIntParameter(BeamSearch.BEAM_SIZE_PARAMETER, LemmatizerME.DEFAULT_BEAM_SIZE);
        //     LemmatizerContextGenerator contextGenerator = posFactory.GetContextGenerator();
        //     Dictionary<string, string> manifestInfoEntries = new HashMap();
        //     TrainerType trainerType = TrainerFactory.GetTrainerType(trainParams);
        //     MaxentModel lemmatizerModel = null;
        //     SequenceClassificationModel<string> seqLemmatizerModel = null;
        //     if (TrainerType.EVENT_MODEL_TRAINER.Equals(trainerType))
        //     {
        //         ObjectStream<Event> es = new LemmaSampleEventStream(samples, contextGenerator);
        //         EventTrainer trainer = TrainerFactory.GetEventTrainer(trainParams, manifestInfoEntries);
        //         lemmatizerModel = trainer.Train(es);
        //     }
        //     else if (TrainerType.EVENT_MODEL_SEQUENCE_TRAINER.Equals(trainerType))
        //     {
        //         LemmaSampleSequenceStream ss = new LemmaSampleSequenceStream(samples, contextGenerator);
        //         EventModelSequenceTrainer trainer = TrainerFactory.GetEventModelSequenceTrainer(trainParams, manifestInfoEntries);
        //         lemmatizerModel = trainer.Train(ss);
        //     }
        //     else if (TrainerType.SEQUENCE_TRAINER.Equals(trainerType))
        //     {
        //         SequenceTrainer trainer = TrainerFactory.GetSequenceModelTrainer(trainParams, manifestInfoEntries);
        //
        //         // TODO: This will probably cause issue, since the feature generator uses the outcomes array
        //         LemmaSampleSequenceStream ss = new LemmaSampleSequenceStream(samples, contextGenerator);
        //         seqLemmatizerModel = trainer.Train(ss);
        //     }
        //     else
        //     {
        //         throw new ArgumentException("Trainer type is not supported: " + trainerType);
        //     }
        //
        //     if (lemmatizerModel != null)
        //     {
        //         return new LemmatizerModel(languageCode, lemmatizerModel, beamSize, manifestInfoEntries, posFactory);
        //     }
        //     else
        //     {
        //         return new LemmatizerModel(languageCode, seqLemmatizerModel, manifestInfoEntries, posFactory);
        //     }
        // }

        public virtual Sequence[] TopKLemmaClasses(string[] sentence, string[] tags)
        {
            return model.BestSequences(DEFAULT_BEAM_SIZE, sentence, new object[] { tags }, contextGenerator, sequenceValidator);
        }

        public virtual Sequence[] TopKLemmaClasses(string[] sentence, string[] tags, double minSequenceScore)
        {
            return model.BestSequences(DEFAULT_BEAM_SIZE, sentence, new object[] { tags }, minSequenceScore, contextGenerator, sequenceValidator);
        }
    }
}

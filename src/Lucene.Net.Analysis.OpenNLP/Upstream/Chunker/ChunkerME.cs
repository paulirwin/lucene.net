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
using System.Collections.Generic;
using System.Linq;

namespace Opennlp.Tools.Chunker
{
    /// <summary>
    /// The class represents a maximum-entropy-based chunker.  Such a chunker can be used to
    /// find flat structures based on sequence inputs such as noun phrases or named entities.
    /// </summary>
    internal class ChunkerME : Chunker
    {
        public static readonly int DEFAULT_BEAM_SIZE = 10;
        private Sequence bestSequence;
        /// <summary>
        /// The model used to assign chunk tags to a sequence of tokens.
        /// </summary>
        protected SequenceClassificationModel<TokenTag> model;
        private ChunkerContextGenerator contextGenerator;
        private SequenceValidator<TokenTag> sequenceValidator;
        /// <summary>
        /// Initializes the current instance with the specified model and
        /// the specified beam size.
        /// </summary>
        /// <param name="model">The model for this chunker.</param>
        /// <param name="beamSize">The size of the beam that should be used when decoding sequences.</param>
        /// <param name="sequenceValidator">The {@link SequenceValidator} to determines whether the outcome
        ///        is valid for the preceding sequence. This can be used to implement constraints
        ///        on what sequences are valid.</param>
        /// <remarks>
        /// @deprecatedUse {@link #ChunkerME(ChunkerModel, int)} instead and use the {@link ChunkerFactory}
        ///     to configure the {@link SequenceValidator} and {@link ChunkerContextGenerator}.
        /// </remarks>
        private ChunkerME(ChunkerModel model, int beamSize, SequenceValidator<TokenTag> sequenceValidator, ChunkerContextGenerator contextGenerator)
        {
            this.sequenceValidator = sequenceValidator;
            this.contextGenerator = contextGenerator;
            if (model.GetChunkerSequenceModel() != null)
            {
                this.model = model.GetChunkerSequenceModel();
            }
            else
            {
                this.model = new BeamSearch<TokenTag>(beamSize, model.GetChunkerModel(), 0);
            }
        }

        /// <summary>
        /// Initializes the current instance with the specified model and
        /// the specified beam size.
        /// </summary>
        /// <param name="model">The model for this chunker.</param>
        /// <param name="beamSize">The size of the beam that should be used when decoding sequences.</param>
        /// <remarks>@deprecatedbeam size is now stored inside the model</remarks>
        private ChunkerME(ChunkerModel model, int beamSize)
        {
            contextGenerator = model.GetFactory().GetContextGenerator();
            sequenceValidator = model.GetFactory().GetSequenceValidator();
            if (model.GetChunkerSequenceModel() != null)
            {
                this.model = model.GetChunkerSequenceModel();
            }
            else
            {
                this.model = new BeamSearch<TokenTag>(beamSize, model.GetChunkerModel(), 0);
            }
        }

        /// <summary>
        /// Initializes the current instance with the specified model.
        /// The default beam size is used.
        /// </summary>
        /// <param name="model"></param>
        public ChunkerME(ChunkerModel model) : this(model, DEFAULT_BEAM_SIZE)
        {
        }

        public virtual string[] Chunk(string[] toks, string[] tags)
        {
            TokenTag[] tuples = TokenTag.Create(toks, tags);
            bestSequence = model.BestSequence(tuples, new object[] { }, contextGenerator, sequenceValidator);
            IList<string> c = bestSequence.GetOutcomes();
            return c.ToArray();
        }

        public virtual Span[] ChunkAsSpans(string[] toks, string[] tags)
        {
            string[] preds = Chunk(toks, tags);
            return ChunkSample.PhrasesAsSpanList(toks, tags, preds);
        }

        public virtual Sequence[] TopKSequences(string[] sentence, string[] tags)
        {
            TokenTag[] tuples = TokenTag.Create(sentence, tags);
            return model.BestSequences(DEFAULT_BEAM_SIZE, tuples, new object[] { }, contextGenerator, sequenceValidator);
        }

        public virtual Sequence[] TopKSequences(string[] sentence, string[] tags, double minSequenceScore)
        {
            TokenTag[] tuples = TokenTag.Create(sentence, tags);
            return model.BestSequences(DEFAULT_BEAM_SIZE, tuples, new object[] { }, minSequenceScore, contextGenerator, sequenceValidator);
        }

        /// <summary>
        /// Populates the specified array with the probabilities of the last decoded sequence.  The
        /// sequence was determined based on the previous call to <code>chunk</code>.  The
        /// specified array should be at least as large as the numbe of tokens in the previous
        /// call to <code>chunk</code>.
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

        // public static ChunkerModel Train(string lang, ObjectStream<ChunkSample> @in, TrainingParameters mlParams, ChunkerFactory factory)
        // {
        //     int beamSize = mlParams.GetIntParameter(BeamSearch.BEAM_SIZE_PARAMETER, ChunkerME.DEFAULT_BEAM_SIZE);
        //     Dictionary<string, string> manifestInfoEntries = new Dictionary<string, string>();
        //     TrainerType trainerType = TrainerFactory.GetTrainerType(mlParams);
        //     MaxentModel chunkerModel = null;
        //     SequenceClassificationModel<string> seqChunkerModel = null;
        //     if (TrainerType.EVENT_MODEL_TRAINER.Equals(trainerType))
        //     {
        //         ObjectStream<Event> es = new ChunkerEventStream(@in, factory.GetContextGenerator());
        //         EventTrainer trainer = TrainerFactory.GetEventTrainer(mlParams, manifestInfoEntries);
        //         chunkerModel = trainer.Train(es);
        //     }
        //     else if (TrainerType.SEQUENCE_TRAINER.Equals(trainerType))
        //     {
        //         SequenceTrainer trainer = TrainerFactory.GetSequenceModelTrainer(mlParams, manifestInfoEntries);
        //
        //         // TODO: This will probably cause issue, since the feature generator uses the outcomes array
        //         ChunkSampleSequenceStream ss = new ChunkSampleSequenceStream(@in, factory.GetContextGenerator());
        //         seqChunkerModel = trainer.Train(ss);
        //     }
        //     else
        //     {
        //         throw new ArgumentException("Trainer type is not supported: " + trainerType);
        //     }
        //
        //     if (chunkerModel != null)
        //     {
        //         return new ChunkerModel(lang, chunkerModel, beamSize, manifestInfoEntries, factory);
        //     }
        //     else
        //     {
        //         return new ChunkerModel(lang, seqChunkerModel, manifestInfoEntries, factory);
        //     }
        // }
    }
}

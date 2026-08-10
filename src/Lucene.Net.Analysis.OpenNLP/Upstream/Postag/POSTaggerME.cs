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
using Lucene.Net.Support;
using Opennlp.Tools.Dictionary;
using Opennlp.Tools.Ml;
using Opennlp.Tools.Ml.Model;
using Opennlp.Tools.Ngram;
using Opennlp.Tools.Util;
using Opennlp.Tools.Util.Featuregen;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Opennlp.Tools.Postag
{
    /// <summary>
    /// A part-of-speech tagger that uses maximum entropy.  Tries to predict whether
    /// words are nouns, verbs, or any of 70 other POS tags depending on their
    /// surrounding context.
    /// </summary>
    internal class POSTaggerME : POSTagger
    {
        public static readonly int DEFAULT_BEAM_SIZE = 3;
        private POSModel modelPackage;
        /// <summary>
        /// The feature context generator.
        /// </summary>
        protected POSContextGenerator contextGen;
        /// <summary>
        /// Tag dictionary used for restricting words to a fixed set of tags.
        /// </summary>
        protected TagDictionary tagDictionary;
        protected Opennlp.Tools.Dictionary.Dictionary ngramDictionary;
        /// <summary>
        /// Says whether a filter should be used to check whether a tag assignment
        /// is to a word outside of a closed class.
        /// </summary>
        protected bool useClosedClassTagsFilter = false;
        /// <summary>
        /// The size of the beam to be used in determining the best sequence of pos tags.
        /// </summary>
        protected int size;
        private Sequence bestSequence;
        private SequenceClassificationModel<string> model;
        private SequenceValidator<string> sequenceValidator;
        /// <summary>
        /// Initializes the current instance with the provided model.
        /// </summary>
        /// <param name="model"></param>
        public POSTaggerME(POSModel model)
        {
            POSTaggerFactory factory = model.GetFactory();
            int beamSize = POSTaggerME.DEFAULT_BEAM_SIZE;
            string beamSizeString = model.GetManifestProperty(BeamSearch.BEAM_SIZE_PARAMETER);
            if (beamSizeString != null)
            {
                beamSize = int.Parse(beamSizeString);
            }

            modelPackage = model;
            contextGen = factory.GetPOSContextGenerator(beamSize);
            tagDictionary = factory.GetTagDictionary();
            size = beamSize;
            sequenceValidator = factory.GetSequenceValidator();
            if (model.GetPosSequenceModel() != null)
            {
                this.model = model.GetPosSequenceModel();
            }
            else
            {
                this.model = new BeamSearch<string>(beamSize, model.GetPosModel(), 0);
            }
        }

        /// <summary>
        /// Retrieves an array of all possible part-of-speech tags from the
        /// tagger.
        /// </summary>
        /// <returns>String[]</returns>
        public virtual String[] GetAllPosTags()
        {
            return model.GetOutcomes();
        }

        public virtual String[] Tag(string[] sentence)
        {
            return this.Tag(sentence, null);
        }

        public virtual String[] Tag(string[] sentence, object[] additionaContext)
        {
            bestSequence = model.BestSequence(sentence, additionaContext, contextGen, sequenceValidator);
            IList<string> t = bestSequence.GetOutcomes();
            return t.ToArray();
        }

        /// <summary>
        /// Returns at most the specified number of taggings for the specified sentence.
        /// </summary>
        /// <param name="numTaggings">The number of tagging to be returned.</param>
        /// <param name="sentence">An array of tokens which make up a sentence.</param>
        /// <returns>At most the specified number of taggings for the specified sentence.</returns>
        public virtual String[][] Tag(int numTaggings, string[] sentence)
        {
            Sequence[] bestSequences = model.BestSequences(numTaggings, sentence, null, contextGen, sequenceValidator);
            string[][] tags = new string[bestSequences.Length][];
            for (int si = 0; si < tags.Length; si++)
            {
                IList<string> t = bestSequences[si].GetOutcomes();
                tags[si] = t.ToArray();
            }

            return tags;
        }

        public virtual Sequence[] TopKSequences(string[] sentence)
        {
            return this.TopKSequences(sentence, null);
        }

        public virtual Sequence[] TopKSequences(string[] sentence, object[] additionaContext)
        {
            return model.BestSequences(size, sentence, additionaContext, contextGen, sequenceValidator);
        }

        /// <summary>
        /// Populates the specified array with the probabilities for each tag of the last tagged sentence.
        /// </summary>
        /// <param name="probs">An array to put the probabilities into.</param>
        public virtual void Probs(double[] probs)
        {
            bestSequence.GetProbs(probs);
        }

        /// <summary>
        /// Returns an array with the probabilities for each tag of the last tagged sentence.
        /// </summary>
        /// <returns>an array with the probabilities for each tag of the last tagged sentence.</returns>
        public virtual double[] Probs()
        {
            return bestSequence.GetProbs();
        }

        public virtual String[] GetOrderedTags(IList<string> words, IList<string> tags, int index)
        {
            return GetOrderedTags(words, tags, index, null);
        }

        public virtual String[] GetOrderedTags(IList<string> words, IList<string> tags, int index, double[] tprobs)
        {
            if (modelPackage.GetPosModel() != null)
            {
                MaxentModel posModel = modelPackage.GetPosModel();
                double[] probs = posModel.Eval(contextGen.GetContext(index, words.ToArray(), tags.ToArray(), null));
                string[] orderedTags = new string[probs.Length];
                for (int i = 0; i < probs.Length; i++)
                {
                    int max = 0;
                    for (int ti = 1; ti < probs.Length; ti++)
                    {
                        if (probs[ti] > probs[max])
                        {
                            max = ti;
                        }
                    }

                    orderedTags[i] = posModel.GetOutcome(max);
                    if (tprobs != null)
                    {
                        tprobs[i] = probs[max];
                    }

                    probs[max] = 0;
                }

                return orderedTags;
            }
            else
            {
                throw new NotSupportedException("This method can only be called if the " + "classifcation model is an event model!");
            }
        }

        // public static POSModel Train(string languageCode, ObjectStream<POSSample> samples, TrainingParameters trainParams, POSTaggerFactory posFactory)
        // {
        //     int beamSize = trainParams.GetIntParameter(BeamSearch.BEAM_SIZE_PARAMETER, POSTaggerME.DEFAULT_BEAM_SIZE);
        //     POSContextGenerator contextGenerator = posFactory.GetPOSContextGenerator();
        //     Dictionary<string, string> manifestInfoEntries = new Dictionary<string, string>();
        //     TrainerType trainerType = TrainerFactory.GetTrainerType(trainParams);
        //     MaxentModel posModel = null;
        //     SequenceClassificationModel<string> seqPosModel = null;
        //     if (TrainerType.EVENT_MODEL_TRAINER.Equals(trainerType))
        //     {
        //         ObjectStream<Event> es = new POSSampleEventStream(samples, contextGenerator);
        //         EventTrainer trainer = TrainerFactory.GetEventTrainer(trainParams, manifestInfoEntries);
        //         posModel = trainer.Train(es);
        //     }
        //     else if (TrainerType.EVENT_MODEL_SEQUENCE_TRAINER.Equals(trainerType))
        //     {
        //         POSSampleSequenceStream ss = new POSSampleSequenceStream(samples, contextGenerator);
        //         EventModelSequenceTrainer trainer = TrainerFactory.GetEventModelSequenceTrainer(trainParams, manifestInfoEntries);
        //         posModel = trainer.Train(ss);
        //     }
        //     else if (TrainerType.SEQUENCE_TRAINER.Equals(trainerType))
        //     {
        //         SequenceTrainer trainer = TrainerFactory.GetSequenceModelTrainer(trainParams, manifestInfoEntries);
// 
        //         // TODO: This will probably cause issue, since the feature generator uses the outcomes array
        //         POSSampleSequenceStream ss = new POSSampleSequenceStream(samples, contextGenerator);
        //         seqPosModel = trainer.Train(ss);
        //     }
        //     else
        //     {
        //         throw new ArgumentException("Trainer type is not supported: " + trainerType);
        //     }
// 
        //     if (posModel != null)
        //     {
        //         return new POSModel(languageCode, posModel, beamSize, manifestInfoEntries, posFactory);
        //     }
        //     else
        //     {
        //         return new POSModel(languageCode, seqPosModel, manifestInfoEntries, posFactory);
        //     }
        // }

        // public static Opennlp.Tools.Dictionary.Dictionary BuildNGramDictionary(ObjectStream<POSSample> samples, int cutoff)
        // {
        //     NGramModel ngramModel = new NGramModel();
        //     POSSample sample;
        //     while ((sample = samples.Read()) != null)
        //     {
        //         string[] words = sample.GetSentence();
        //         if (words.Length > 0)
        //             ngramModel.Add(new StringList(words), 1, 1);
        //     }
// 
        //     ngramModel.Cutoff(cutoff, int.MaxValue);
        //     return ngramModel.ToDictionary(true);
        // }

        // public static void PopulatePOSDictionary(ObjectStream<POSSample> samples, MutableTagDictionary dict, int cutoff)
        // {
        //     System.@out.Println("Expanding POS Opennlp.Tools.Dictionary.Dictionary ...");
        //     long start = System.NanoTime();
// 
        //     // the data structure will store the word, the tag, and the number of
        //     // occurrences
        //     Dictionary<string, Dictionary<string, AtomicInteger>> newEntries = new Dictionary<string, string>();
        //     POSSample sample;
        //     while ((sample = samples.Read()) != null)
        //     {
        //         string[] words = sample.GetSentence();
        //         string[] tags = sample.GetTags();
        //         for (int i = 0; i < words.Length; i++)
        //         {
// 
        //             // only store words
        //             if (!StringPattern.Recognize(words[i]).ContainsDigit())
        //             {
        //                 string word;
        //                 if (dict.IsCaseSensitive())
        //                 {
        //                     word = words[i];
        //                 }
        //                 else
        //                 {
        //                     word = StringUtil.ToLowerCase(words[i]);
        //                 }
// 
        //                 if (!newEntries.ContainsKey(word))
        //                 {
        //                     newEntries.Put(word, new Dictionary<string, string>());
        //                 }
// 
        //                 string[] dictTags = dict.GetTags(word);
        //                 if (dictTags != null)
        //                 {
        //                     foreach (string tag in dictTags)
        //                     {
// 
        //                         // for this tags we start with the cutoff
        //                         Dictionary<string, AtomicInteger> value = newEntries[word];
        //                         if (!value.ContainsKey(tag))
        //                         {
        //                             value.Put(tag, new AtomicInteger(cutoff));
        //                         }
        //                     }
        //                 }
// 
        //                 if (!newEntries[word].ContainsKey(tags[i]))
        //                 {
        //                     newEntries[word].Put(tags[i], new AtomicInteger(1));
        //                 }
        //                 else
        //                 {
        //                     newEntries[word][tags[i]].IncrementAndGet();
        //                 }
        //             }
        //         }
        //     }
// 
// 
        //     // now we check if the word + tag pairs have enough occurrences, if yes we
        //     // add it to the dictionary
        //     foreach (Entry<string, Dictionary<string, AtomicInteger>> wordEntry in newEntries.EntrySet())
        //     {
        //         IList<string> tagsForWord = new List<string>();
        //         foreach (Entry<string, AtomicInteger> entry in wordEntry.GetValue().EntrySet())
        //         {
        //             if (entry.GetValue().Get() >= cutoff)
        //             {
        //                 tagsForWord.Add(entry.GetKey());
        //             }
        //         }
// 
        //         if (tagsForWord.Count > 0)
        //         {
        //             dict.Put(wordEntry.GetKey(), tagsForWord.ToArray());
        //         }
        //     }
// 
        //     System.@out.Println("... finished expanding POS Dictionary. [" + (System.NanoTime() - start) / 1000000 + "ms]");
        // }
    }
}

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
using Opennlp.Tools.Sentdetect.Lang;
using Opennlp.Tools.Util;
using Opennlp.Tools.Util.Model;
using J2N.Text;
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Opennlp.Tools.Sentdetect
{
    /// <summary>
    /// A sentence detector for splitting up raw text into sentences.
    /// <p>
    /// A maximum entropy model is used to evaluate end-of-sentence characters in a
    /// string to determine if they signify the end of a sentence.
    /// </summary>
    internal class SentenceDetectorME : SentenceDetector
    {
        /// <summary>
        /// Constant indicates a sentence split.
        /// </summary>
        public static readonly string SPLIT = "s";
        /// <summary>
        /// Constant indicates no sentence split.
        /// </summary>
        public static readonly string NO_SPLIT = "n";
        /// <summary>
        /// The maximum entropy model to use to evaluate contexts.
        /// </summary>
        private MaxentModel model;
        /// <summary>
        /// The feature context generator.
        /// </summary>
        private readonly SDContextGenerator cgen;
        /// <summary>
        /// The {@link EndOfSentenceScanner} to use when scanning for end of sentence offsets.
        /// </summary>
        private readonly EndOfSentenceScanner scanner;
        /// <summary>
        /// The list of probabilities associated with each decision.
        /// </summary>
        private IList<double> sentProbs = new List<double>();
        protected bool useTokenEnd;
        /// <summary>
        /// Initializes the current instance.
        /// </summary>
        /// <param name="model">the {@link SentenceModel}</param>
        public SentenceDetectorME(SentenceModel model)
        {
            SentenceDetectorFactory sdFactory = model.GetFactory();
            this.model = model.GetMaxentModel();
            cgen = sdFactory.GetSDContextGenerator();
            scanner = sdFactory.GetEndOfSentenceScanner();
            useTokenEnd = sdFactory.IsUseTokenEnd();
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// @deprecatedUse a {@link SentenceDetectorFactory} to extend
        ///             SentenceDetector functionality.
        /// </remarks>
        public SentenceDetectorME(SentenceModel model, Factory factory)
        {
            this.model = model.GetMaxentModel();

            // if the model has custom EOS characters set, use this to get the context
            // generator and the EOS scanner; otherwise use language-specific defaults
            char[] customEOSCharacters = model.GetEosCharacters();
            if (customEOSCharacters == null)
            {
                cgen = factory.CreateSentenceContextGenerator(model.GetLanguage(), GetAbbreviations(model.GetAbbreviations()));
                scanner = factory.CreateEndOfSentenceScanner(model.GetLanguage());
            }
            else
            {
                cgen = factory.CreateSentenceContextGenerator(GetAbbreviations(model.GetAbbreviations()), customEOSCharacters);
                scanner = factory.CreateEndOfSentenceScanner(customEOSCharacters);
            }

            useTokenEnd = model.UseTokenEnd();
        }

        private static HashSet<string> GetAbbreviations(Opennlp.Tools.Dictionary.Dictionary abbreviations)
        {
            if (abbreviations == null)
            {
                return new HashSet<string>();
            }

            return new HashSet<string>(abbreviations.AsStringSet());
        }

        /// <summary>
        /// Detect sentences in a String.
        /// </summary>
        /// <param name="s">The string to be processed.</param>
        /// <returns>  A string array containing individual sentences as elements.</returns>
        public virtual String[] SentDetect(string s)
        {
            Span[] spans = SentPosDetect(s);
            string[] sentences;
            if (spans.Length != 0)
            {
                sentences = new string[spans.Length];
                for (int si = 0; si < spans.Length; si++)
                {
                    sentences[si] = spans[si].GetCoveredText(s.AsCharSequence()).ToString();
                }
            }
            else
            {
                sentences = new string[]
                {
                };
            }

            return sentences;
        }

        private int GetFirstWS(string s, int pos)
        {
            while (pos < s.Length && !StringUtil.IsWhitespace(s[pos]))
                pos++;
            return pos;
        }

        private int GetFirstNonWS(string s, int pos)
        {
            while (pos < s.Length && StringUtil.IsWhitespace(s[pos]))
                pos++;
            return pos;
        }

        /// <summary>
        /// Detect the position of the first words of sentences in a String.
        /// </summary>
        /// <param name="s">The string to be processed.</param>
        /// <returns>  A integer array containing the positions of the end index of
        ///          every sentence</returns>
        public virtual Span[] SentPosDetect(string s)
        {
            sentProbs.Clear();
            StringBuilder sb = new StringBuilder(s);
            IList<int> enders = scanner.GetPositions(s);
            IList<int> positions = new List<int>(enders.Count);
            for (int i = 0, end = enders.Count, index = 0; i < end; i++)
            {
                int cint = enders[i];

                // skip over the leading parts of non-token final delimiters
                int fws = GetFirstWS(s, cint + 1);
                if (i + 1 < end && enders[i + 1] < fws)
                {
                    continue;
                }

                if (positions.Count > 0 && cint < positions[positions.Count - 1])
                    continue;
                double[] probs = model.Eval(cgen.GetContext(sb.ToString(), cint));
                string bestOutcome = model.GetBestOutcome(probs);
                if (bestOutcome.Equals(SPLIT) && IsAcceptableBreak(s, index, cint))
                {
                    if (index != cint)
                    {
                        if (useTokenEnd)
                        {
                            positions.Add(GetFirstNonWS(s, GetFirstWS(s, cint + 1)));
                        }
                        else
                        {
                            positions.Add(GetFirstNonWS(s, cint + 1));
                        }

                        sentProbs.Add(probs[model.GetIndex(bestOutcome)]);
                    }

                    index = cint + 1;
                }
            }

            int[] starts = new int[positions.Count];
            for (int i = 0; i < starts.Length; i++)
            {
                starts[i] = positions[i];
            }


            // string does not contain sentence end positions
            if (starts.Length == 0)
            {

                // remove leading and trailing whitespace
                int start = 0;
                int end = s.Length;
                while (start < s.Length && StringUtil.IsWhitespace(s[start]))
                    start++;
                while (end > 0 && StringUtil.IsWhitespace(s[end - 1]))
                    end--;
                if (end - start > 0)
                {
                    sentProbs.Add(1);
                    return new Span[]
                    {
                        new Span(start, end)
                    };
                }
                else
                    return new Span[0];
            }


            // Convert the sentence end indexes to spans
            bool leftover = starts[starts.Length - 1] != s.Length;
            Span[] spans = new Span[leftover ? starts.Length + 1 : starts.Length];
            for (int si = 0; si < starts.Length; si++)
            {
                int start;
                if (si == 0)
                {
                    start = 0;
                }
                else
                {
                    start = starts[si - 1];
                }


                // A span might contain only white spaces, in this case the length of
                // the span will be zero after trimming and should be ignored.
                Span span = new Span(start, starts[si]).Trim(s.AsCharSequence());
                if (span.Length() > 0)
                {
                    spans[si] = span;
                }
                else
                {
                    sentProbs.Remove(si);
                }
            }

            if (leftover)
            {
                Span span = new Span(starts[starts.Length - 1], s.Length).Trim(s.AsCharSequence());
                if (span.Length() > 0)
                {
                    spans[spans.Length - 1] = span;
                    sentProbs.Add(1);
                }
            }

            /*
             * set the prob for each span
             */
            for (int i = 0; i < spans.Length; i++)
            {
                double prob = sentProbs[i];
                spans[i] = new Span(spans[i], prob);
            }

            return spans;
        }

        /// <summary>
        /// Returns the probabilities associated with the most recent
        /// calls to sentDetect().
        /// </summary>
        /// <returns>probability for each sentence returned for the most recent
        ///     call to sentDetect.  If not applicable an empty array is returned.</returns>
        public virtual double[] GetSentenceProbabilities()
        {
            double[] sentProbArray = new double[sentProbs.Count];
            for (int i = 0; i < sentProbArray.Length; i++)
            {
                sentProbArray[i] = sentProbs[i];
            }

            return sentProbArray;
        }

        /// <summary>
        /// Allows subclasses to check an overzealous (read: poorly
        /// trained) model from flagging obvious non-breaks as breaks based
        /// on some boolean determination of a break's acceptability.
        /// 
        /// <p>The implementation here always returns true, which means
        /// that the MaxentModel's outcome is taken as is.</p>
        /// </summary>
        /// <param name="s">the string in which the break occurred.</param>
        /// <param name="fromIndex">the start of the segment currently being evaluated</param>
        /// <param name="candidateIndex">the index of the candidate sentence ending</param>
        /// <returns>true if the break is acceptable</returns>
        protected virtual bool IsAcceptableBreak(string s, int fromIndex, int candidateIndex)
        {
            return true;
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// @deprecatedUse
        ///             {@link #train(String, ObjectStream, SentenceDetectorFactory, TrainingParameters)}
        ///             and pass in af {@link SentenceDetectorFactory}.
        /// </remarks>
        // public static SentenceModel Train(string languageCode, ObjectStream<SentenceSample> samples, bool useTokenEnd, Opennlp.Tools.Dictionary.Dictionary abbreviations, TrainingParameters mlParams)
        // {
        //     SentenceDetectorFactory sdFactory = new SentenceDetectorFactory(languageCode, useTokenEnd, abbreviations, null);
        //     return Train(languageCode, samples, sdFactory, mlParams);
        // }

        // public static SentenceModel Train(string languageCode, ObjectStream<SentenceSample> samples, SentenceDetectorFactory sdFactory, TrainingParameters mlParams)
        // {
        //     Dictionary<string, string> manifestInfoEntries = new Dictionary<string, string>();
// 
        //     // TODO: Fix the EventStream to throw exceptions when training goes wrong
        //     ObjectStream<Event> eventStream = new SDEventStream(samples, sdFactory.GetSDContextGenerator(), sdFactory.GetEndOfSentenceScanner());
        //     EventTrainer trainer = TrainerFactory.GetEventTrainer(mlParams, manifestInfoEntries);
        //     MaxentModel sentModel = trainer.Train(eventStream);
        //     return new SentenceModel(languageCode, sentModel, manifestInfoEntries, sdFactory);
        // }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// @deprecatedUse
        ///             {@link #train(String, ObjectStream, SentenceDetectorFactory, TrainingParameters)}
        ///             and pass in af {@link SentenceDetectorFactory}.
        /// </remarks>
        // public static SentenceModel Train(string languageCode, ObjectStream<SentenceSample> samples, bool useTokenEnd, Opennlp.Tools.Dictionary.Dictionary abbreviations)
        // {
        //     return Train(languageCode, samples, useTokenEnd, abbreviations, ModelUtil.CreateDefaultTrainingParameters());
        // }
    }
}

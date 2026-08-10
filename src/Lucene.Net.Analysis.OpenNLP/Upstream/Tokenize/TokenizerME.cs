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
using Opennlp.Tools.Tokenize.Lang;
using Opennlp.Tools.Util;
using System;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Opennlp.Tools.Tokenize
{
    /// <summary>
    /// A Tokenizer for converting raw text into separated tokens.  It uses
    /// Maximum Entropy to make its decisions.  The features are loosely
    /// based off of Jeff Reynar's UPenn thesis "Topic Segmentation:
    /// Algorithms and Applications.", which is available from his
    /// homepage: <a href="http://www.cis.upenn.edu/~jcreynar">http://www.cis.upenn.edu/~jcreynar</a>.
    /// <p>
    /// This tokenizer needs a statistical model to tokenize a text which reproduces
    /// the tokenization observed in the training data used to create the model.
    /// The {@link TokenizerModel} class encapsulates the model and provides
    /// methods to create it from the binary representation.
    /// <p>
    /// A tokenizer instance is not thread safe. For each thread one tokenizer
    /// must be instantiated which can share one <code>TokenizerModel</code> instance
    /// to safe memory.
    /// <p>
    /// To train a new model {{@link #train(ObjectStream, TokenizerFactory, TrainingParameters)} method
    /// can be used.
    /// <p>
    /// Sample usage:
    /// <p>
    /// <code>
    /// Stream modelIn;<br>
    /// <br>
    /// ...<br>
    /// <br>
    /// TokenizerModel model = TokenizerModel(modelIn);<br>
    /// <br>
    /// Tokenizer tokenizer = new TokenizerME(model);<br>
    /// <br>
    /// String tokens[] = tokenizer.tokenize("A sentence to be tokenized.");
    /// </code>
    /// </summary>
    /// <remarks>
    /// @seeTokenizer
    /// @seeTokenizerModel
    /// @seeTokenSample
    /// </remarks>
    internal class TokenizerME : AbstractTokenizer
    {
        /// <summary>
        /// Constant indicates a token split.
        /// </summary>
        public static readonly string SPLIT = "T";
        /// <summary>
        /// Constant indicates no token split.
        /// </summary>
        public static readonly string NO_SPLIT = "F";
        /// <summary>
        /// Alpha-Numeric Regex
        /// </summary>
        /// <remarks>@deprecatedAs of release 1.5.2, replaced by {@link Factory#getAlphanumeric(String)}</remarks>
        public static readonly Regex alphaNumeric = new Regex(Factory.DEFAULT_ALPHANUMERIC);
        private readonly Regex alphanumeric;
        /// <summary>
        /// The maximum entropy model to use to evaluate contexts.
        /// </summary>
        private MaxentModel model;
        /// <summary>
        /// The context generator.
        /// </summary>
        private readonly TokenContextGenerator cg;
        /// <summary>
        /// Optimization flag to skip alpha numeric tokens for further
        /// tokenization
        /// </summary>
        private bool useAlphaNumericOptimization;
        /// <summary>
        /// List of probabilities for each token returned from a call to
        /// <code>tokenize</code> or <code>tokenizePos</code>.
        /// </summary>
        private IList<Double> tokProbs;
        private IList<Span> newTokens;
        public TokenizerME(TokenizerModel model)
        {
            TokenizerFactory factory = model.GetFactory();
            this.alphanumeric = factory.GetAlphaNumericPattern();
            this.cg = factory.GetContextGenerator();
            this.model = model.GetMaxentModel();
            this.useAlphaNumericOptimization = factory.IsUseAlphaNumericOptmization();
            newTokens = new List<Span>();
            tokProbs = new List<double>(50);
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// @deprecateduse {@link TokenizerFactory} to extend the Tokenizer
        ///             functionality
        /// </remarks>
        public TokenizerME(TokenizerModel model, Factory factory)
        {
            string languageCode = model.GetLanguage();
            this.alphanumeric = factory.GetAlphanumeric(languageCode);
            this.cg = factory.CreateTokenContextGenerator(languageCode, GetAbbreviations(model.GetAbbreviations()));
            this.model = model.GetMaxentModel();
            useAlphaNumericOptimization = model.UseAlphaNumericOptimization();
            newTokens = new List<Span>();
            tokProbs = new List<double>(50);
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
        /// Returns the probabilities associated with the most recent
        /// calls to {@link TokenizerME#tokenize(String)} or {@link TokenizerME#tokenizePos(String)}.
        /// </summary>
        /// <returns>probability for each token returned for the most recent
        ///     call to tokenize.  If not applicable an empty array is returned.</returns>
        public virtual double[] GetTokenProbabilities()
        {
            double[] tokProbArray = new double[tokProbs.Count];
            for (int i = 0; i < tokProbArray.Length; i++)
            {
                tokProbArray[i] = tokProbs[i];
            }

            return tokProbArray;
        }

        /// <summary>
        /// Tokenizes the string.
        /// </summary>
        /// <param name="d">The string to be tokenized.</param>
        /// <returns>  A span array containing individual tokens as elements.</returns>
        public override Span[] TokenizePos(string d)
        {
            Span[] tokens = WhitespaceTokenizer.INSTANCE.TokenizePos(d);
            newTokens.Clear();
            tokProbs.Clear();
            foreach (Span s in tokens)
            {
                // LUCENENET: Java substring(begin, end) takes an end index; .NET takes a length.
                string tok = d.Substring(s.GetStart(), s.GetEnd() - s.GetStart());

                // Can't tokenize single characters
                if (tok.Length < 2)
                {
                    newTokens.Add(s);
                    tokProbs.Add(1);
                }
                else if (UseAlphaNumericOptimization() && alphanumeric.IsMatch(tok))
                {
                    newTokens.Add(s);
                    tokProbs.Add(1);
                }
                else
                {
                    int start = s.GetStart();
                    int end = s.GetEnd();
                    int origStart = s.GetStart();
                    double tokenProb = 1;
                    for (int j = origStart + 1; j < end; j++)
                    {
                        double[] probs = model.Eval(cg.GetContext(tok, j - origStart));
                        string best = model.GetBestOutcome(probs);
                        tokenProb *= probs[model.GetIndex(best)];
                        if (best.Equals(TokenizerME.SPLIT))
                        {
                            newTokens.Add(new Span(start, j));
                            tokProbs.Add(tokenProb);
                            start = j;
                            tokenProb = 1;
                        }
                    }

                    newTokens.Add(new Span(start, end));
                    tokProbs.Add(tokenProb);
                }
            }

            return newTokens.ToArray();
        }

        /// <summary>
        /// Trains a model for the {@link TokenizerME}.
        /// </summary>
        /// <param name="samples">
        ///          the samples used for the training.</param>
        /// <param name="factory">
        ///          a {@link TokenizerFactory} to get resources from</param>
        /// <param name="mlParams">
        ///          the machine learning train parameters</param>
        /// <returns>the trained {@link TokenizerModel}</returns>
        /// <exception cref="IOException">
        ///           it throws an {@link IOException} if an {@link IOException} is
        ///           thrown during IO operations on a temp file which is created
        ///           during training. Or if reading from the {@link ObjectStream}
        ///           fails.</exception>
        // public static TokenizerModel Train(ObjectStream<TokenSample> samples, TokenizerFactory factory, TrainingParameters mlParams)
        // {
        //     Dictionary<string, string> manifestInfoEntries = new Dictionary<string, string>();
        //     ObjectStream<Event> eventStream = new TokSpanEventStream(samples, factory.IsUseAlphaNumericOptmization(), factory.GetAlphaNumericPattern(), factory.GetContextGenerator());
        //     EventTrainer trainer = TrainerFactory.GetEventTrainer(mlParams, manifestInfoEntries);
        //     MaxentModel maxentModel = trainer.Train(eventStream);
        //     return new TokenizerModel(maxentModel, manifestInfoEntries, factory);
        // }

        /// <summary>
        /// Returns the value of the alpha-numeric optimization flag.
        /// </summary>
        /// <returns>true if the tokenizer should use alpha-numeric optimization, false otherwise.</returns>
        public virtual bool UseAlphaNumericOptimization()
        {
            return useAlphaNumericOptimization;
        }
    }
}

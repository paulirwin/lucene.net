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
using Opennlp.Tools.Tokenize.Lang;
using Opennlp.Tools.Util;
using Opennlp.Tools.Util.Ext;
using J2N.Text;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Opennlp.Tools.Tokenize
{
    /// <summary>
    /// The factory that provides {@link Tokenizer} default implementations and
    /// resources. Users can extend this class if their application requires
    /// overriding the {@link TokenContextGenerator}, {@link Opennlp.Tools.Dictionary.Dictionary} etc.
    /// </summary>
    internal class TokenizerFactory : BaseToolFactory
    {
        private string languageCode;
        private Opennlp.Tools.Dictionary.Dictionary abbreviationDictionary;
        private bool useAlphaNumericOptimization = false;
        private Regex alphaNumericPattern;
        private static readonly string ABBREVIATIONS_ENTRY_NAME = "abbreviations.dictionary";
        private static readonly string USE_ALPHA_NUMERIC_OPTIMIZATION = "useAlphaNumericOptimization";
        private static readonly string ALPHA_NUMERIC_PATTERN = "alphaNumericPattern";
        /// <summary>
        /// Creates a {@link TokenizerFactory} that provides the default implementation
        /// of the resources.
        /// </summary>
        public TokenizerFactory()
        {
        }

        /// <summary>
        /// Creates a {@link TokenizerFactory}. Use this constructor to
        /// programmatically create a factory.
        /// </summary>
        /// <param name="languageCode">
        ///          the language of the natural text</param>
        /// <param name="abbreviationDictionary">
        ///          an abbreviations dictionary</param>
        /// <param name="useAlphaNumericOptimization">
        ///          if true alpha numerics are skipped</param>
        /// <param name="alphaNumericPattern">
        ///          null or a custom alphanumeric pattern (default is:
        ///          "^[A-Za-z0-9]+$", provided by {@link Factory#DEFAULT_ALPHANUMERIC}</param>
        public TokenizerFactory(string languageCode, Opennlp.Tools.Dictionary.Dictionary abbreviationDictionary, bool useAlphaNumericOptimization, Regex alphaNumericPattern)
        {
            this.Init(languageCode, abbreviationDictionary, useAlphaNumericOptimization, alphaNumericPattern);
        }

        protected virtual void Init(string languageCode, Opennlp.Tools.Dictionary.Dictionary abbreviationDictionary, bool useAlphaNumericOptimization, Regex alphaNumericPattern)
        {
            this.languageCode = languageCode;
            this.useAlphaNumericOptimization = useAlphaNumericOptimization;
            this.alphaNumericPattern = alphaNumericPattern;
            this.abbreviationDictionary = abbreviationDictionary;
        }

        public override void ValidateArtifactMap()
        {
            if (this.artifactProvider.GetManifestProperty(USE_ALPHA_NUMERIC_OPTIMIZATION) == null)
                throw new InvalidFormatException(USE_ALPHA_NUMERIC_OPTIMIZATION + " is a mandatory property!");
            object abbreviationsEntry = this.artifactProvider.GetArtifact<Opennlp.Tools.Dictionary.Dictionary>(ABBREVIATIONS_ENTRY_NAME);
            if (abbreviationsEntry != null && !(abbreviationsEntry is Opennlp.Tools.Dictionary.Dictionary))
            {
                throw new InvalidFormatException("Abbreviations dictionary '" + abbreviationsEntry + "' has wrong type, needs to be of type Opennlp.Tools.Dictionary.Dictionary!");
            }
        }

        public override Dictionary<string, object> CreateArtifactMap()
        {
            Dictionary<string, object> artifactMap = base.CreateArtifactMap();

            // Abbreviations are optional
            if (abbreviationDictionary != null)
            {
                artifactMap.Put(ABBREVIATIONS_ENTRY_NAME, abbreviationDictionary);
            }

            return artifactMap;
        }

        public override Dictionary<string, string> CreateManifestEntries()
        {
            Dictionary<string, string> manifestEntries = base.CreateManifestEntries();
            manifestEntries.Put(USE_ALPHA_NUMERIC_OPTIMIZATION, (IsUseAlphaNumericOptmization().ToString()));

            // alphanumeric pattern is optional
            if (GetAlphaNumericPattern() != null)
            {
                manifestEntries.Put(ALPHA_NUMERIC_PATTERN, GetAlphaNumericPattern().ToString());
            }

            return manifestEntries;
        }

        /// <summary>
        /// Factory method the framework uses create a new {@link TokenizerFactory}.
        /// </summary>
        /// <param name="subclassName">the name of the class implementing the {@link TokenizerFactory}</param>
        /// <param name="languageCode">the language code the tokenizer should use</param>
        /// <param name="abbreviationDictionary">an optional dictionary containing abbreviations, or null if not present</param>
        /// <param name="useAlphaNumericOptimization">indicate if the alpha numeric optimization
        ///     should be enabled or disabled</param>
        /// <param name="alphaNumericPattern">the pattern the alpha numeric optimization should use</param>
        /// <returns>the instance of the Tokenizer Factory</returns>
        /// <exception cref="InvalidFormatException">if once of the input parameters doesn't comply if the expected format</exception>
        public static TokenizerFactory Create(string subclassName, string languageCode, Opennlp.Tools.Dictionary.Dictionary abbreviationDictionary, bool useAlphaNumericOptimization, Regex alphaNumericPattern)
        {
            if (subclassName == null)
            {

                // will create the default factory
                return new TokenizerFactory(languageCode, abbreviationDictionary, useAlphaNumericOptimization, alphaNumericPattern);
            }

            try
            {
                TokenizerFactory theFactory = ExtensionLoader.InstantiateExtension<TokenizerFactory>(subclassName);
                theFactory.Init(languageCode, abbreviationDictionary, useAlphaNumericOptimization, alphaNumericPattern);
                return theFactory;
            }
            catch (Exception e)
            {
                string msg = "Could not instantiate the " + subclassName + ". The initialization throw an exception.";
                // LUCENENET: upstream writes to stderr here.
                // Console.Error.WriteLine(msg);
                e.ToString();
                throw new InvalidFormatException(msg, e);
            }
        }

        /// <summary>
        /// Gets the alpha numeric pattern.
        /// </summary>
        /// <returns>the user specified alpha numeric pattern or a default.</returns>
        public virtual Regex GetAlphaNumericPattern()
        {
            if (this.alphaNumericPattern == null)
            {
                if (this.artifactProvider != null)
                {
                    string prop = this.artifactProvider.GetManifestProperty(ALPHA_NUMERIC_PATTERN);
                    if (prop != null)
                    {
                        this.alphaNumericPattern = new Regex(prop);
                    }
                }


                // could not load from manifest, will get from language dependent factory
                if (this.alphaNumericPattern == null)
                {
                    Factory f = new Factory();
                    this.alphaNumericPattern = f.GetAlphanumeric(languageCode);
                }
            }

            return this.alphaNumericPattern;
        }

        /// <summary>
        /// Gets whether to use alphanumeric optimization.
        /// </summary>
        /// <returns>true if the alpha numeric optimization is enabled, otherwise false</returns>
        public virtual bool IsUseAlphaNumericOptmization()
        {
            if (artifactProvider != null)
            {
                this.useAlphaNumericOptimization = bool.Parse(this.artifactProvider.GetManifestProperty(USE_ALPHA_NUMERIC_OPTIMIZATION));
            }

            return this.useAlphaNumericOptimization;
        }

        /// <summary>
        /// Gets the abbreviation dictionary
        /// </summary>
        /// <returns>null or the abbreviation dictionary</returns>
        public virtual Opennlp.Tools.Dictionary.Dictionary GetAbbreviationDictionary()
        {
            if (this.abbreviationDictionary == null && artifactProvider != null)
            {
                this.abbreviationDictionary = this.artifactProvider.GetArtifact<Opennlp.Tools.Dictionary.Dictionary>(ABBREVIATIONS_ENTRY_NAME);
            }

            return this.abbreviationDictionary;
        }

        /// <summary>
        /// Retrieves the language code.
        /// </summary>
        /// <returns>the language code</returns>
        public virtual string GetLanguageCode()
        {
            if (this.languageCode == null && this.artifactProvider != null)
            {
                this.languageCode = this.artifactProvider.GetLanguage();
            }

            return this.languageCode;
        }

        /// <summary>
        /// Gets the context generator
        /// </summary>
        /// <returns>a new instance of the context generator</returns>
        public virtual TokenContextGenerator GetContextGenerator()
        {
            Factory f = new Factory();
            ISet<string> abbs;
            Opennlp.Tools.Dictionary.Dictionary abbDict = GetAbbreviationDictionary();
            if (abbDict != null)
            {
                abbs = abbDict.AsStringSet();
            }
            else
            {
                abbs = new HashSet<string>();
            }

            return f.CreateTokenContextGenerator(GetLanguageCode(), abbs);
        }
    }
}

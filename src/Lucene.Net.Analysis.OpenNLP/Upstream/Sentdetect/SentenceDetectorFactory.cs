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
using Opennlp.Tools.Sentdetect.Lang;
using Opennlp.Tools.Util;
using Opennlp.Tools.Util.Ext;
using J2N.Text;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Opennlp.Tools.Sentdetect
{
    /// <summary>
    /// The factory that provides SentenceDetecor default implementations and
    /// resources
    /// </summary>
    internal class SentenceDetectorFactory : BaseToolFactory
    {
        private string languageCode;
        private char[] eosCharacters;
        private Opennlp.Tools.Dictionary.Dictionary abbreviationDictionary;
        private bool? useTokenEnd = null;
        private static readonly string ABBREVIATIONS_ENTRY_NAME = "abbreviations.dictionary";
        private static readonly string EOS_CHARACTERS_PROPERTY = "eosCharacters";
        private static readonly string TOKEN_END_PROPERTY = "useTokenEnd";
        /// <summary>
        /// Creates a {@link SentenceDetectorFactory} that provides the default
        /// implementation of the resources.
        /// </summary>
        public SentenceDetectorFactory()
        {
        }

        /// <summary>
        /// Creates a {@link SentenceDetectorFactory}. Use this constructor to
        /// programmatically create a factory.
        /// </summary>
        /// <param name="languageCode"></param>
        /// <param name="abbreviationDictionary"></param>
        /// <param name="eosCharacters"></param>
        public SentenceDetectorFactory(string languageCode, bool useTokenEnd, Opennlp.Tools.Dictionary.Dictionary abbreviationDictionary, char[] eosCharacters)
        {
            this.Init(languageCode, useTokenEnd, abbreviationDictionary, eosCharacters);
        }

        protected virtual void Init(string languageCode, bool useTokenEnd, Opennlp.Tools.Dictionary.Dictionary abbreviationDictionary, char[] eosCharacters)
        {
            this.languageCode = languageCode;
            this.useTokenEnd = useTokenEnd;
            this.eosCharacters = eosCharacters;
            this.abbreviationDictionary = abbreviationDictionary;
        }

        public override void ValidateArtifactMap()
        {
            if (this.artifactProvider.GetManifestProperty(TOKEN_END_PROPERTY) == null)
                throw new InvalidFormatException(TOKEN_END_PROPERTY + " is a mandatory property!");
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
                artifactMap.Put(ABBREVIATIONS_ENTRY_NAME, abbreviationDictionary);
            return artifactMap;
        }

        public override Dictionary<string, string> CreateManifestEntries()
        {
            Dictionary<string, string> manifestEntries = base.CreateManifestEntries();
            manifestEntries.Put(TOKEN_END_PROPERTY, (IsUseTokenEnd().ToString()));

            // EOS characters are optional
            if (GetEOSCharacters() != null)
                manifestEntries.Put(EOS_CHARACTERS_PROPERTY, EosCharArrayToString(GetEOSCharacters()));
            return manifestEntries;
        }

        public static SentenceDetectorFactory Create(string subclassName, string languageCode, bool useTokenEnd, Opennlp.Tools.Dictionary.Dictionary abbreviationDictionary, char[] eosCharacters)
        {
            if (subclassName == null)
            {

                // will create the default factory
                return new SentenceDetectorFactory(languageCode, useTokenEnd, abbreviationDictionary, eosCharacters);
            }

            try
            {
                SentenceDetectorFactory theFactory = ExtensionLoader.InstantiateExtension<SentenceDetectorFactory>(subclassName);
                theFactory.Init(languageCode, useTokenEnd, abbreviationDictionary, eosCharacters);
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

        public virtual char[] GetEOSCharacters()
        {
            if (this.eosCharacters == null)
            {
                if (artifactProvider != null)
                {
                    string prop = this.artifactProvider.GetManifestProperty(EOS_CHARACTERS_PROPERTY);
                    if (prop != null)
                    {
                        this.eosCharacters = EosStringToCharArray(prop);
                    }
                }
                else
                {

                    // get from language dependent factory
                    Factory f = new Factory();
                    this.eosCharacters = f.GetEOSCharacters(languageCode);
                }
            }

            return this.eosCharacters;
        }

        public virtual bool IsUseTokenEnd()
        {
            if (this.useTokenEnd == null && artifactProvider != null)
            {
                this.useTokenEnd = bool.Parse(artifactProvider.GetManifestProperty(TOKEN_END_PROPERTY));
            }

            return this.useTokenEnd ?? true;
        }

        public virtual Opennlp.Tools.Dictionary.Dictionary GetAbbreviationDictionary()
        {
            if (this.abbreviationDictionary == null && artifactProvider != null)
            {
                this.abbreviationDictionary = artifactProvider.GetArtifact<Opennlp.Tools.Dictionary.Dictionary>(ABBREVIATIONS_ENTRY_NAME);
            }

            return this.abbreviationDictionary;
        }

        public virtual string GetLanguageCode()
        {
            if (this.languageCode == null && artifactProvider != null)
            {
                this.languageCode = this.artifactProvider.GetLanguage();
            }

            return this.languageCode;
        }

        public virtual EndOfSentenceScanner GetEndOfSentenceScanner()
        {
            Factory f = new Factory();
            char[] eosChars = GetEOSCharacters();
            if (eosChars != null && eosChars.Length > 0)
            {
                return f.CreateEndOfSentenceScanner(eosChars);
            }
            else
            {
                return f.CreateEndOfSentenceScanner(this.languageCode);
            }
        }

        public virtual SDContextGenerator GetSDContextGenerator()
        {
            Factory f = new Factory();
            char[] eosChars = GetEOSCharacters();
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

            if (eosChars != null && eosChars.Length > 0)
            {
                return f.CreateSentenceContextGenerator(abbs, eosChars);
            }
            else
            {
                return f.CreateSentenceContextGenerator(this.languageCode, abbs);
            }
        }

        private string EosCharArrayToString(char[] eosCharacters)
        {
            return Convert.ToString(eosCharacters);
        }

        private char[] EosStringToCharArray(string eosCharacters)
        {
            return eosCharacters.ToCharArray();
        }
    }
}

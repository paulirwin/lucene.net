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
using Opennlp.Tools.Sentdetect;
using Opennlp.Tools.Sentdetect.Lang.Th;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Opennlp.Tools.Sentdetect.Lang
{
    internal class Factory
    {
        public static readonly char[] ptEosCharacters = new char[]
        {
            '.',
            '?',
            '!',
            ';',
            ':',
            '(',
            ')',
            '«',
            '»',
            '\'',
            '"'
        };
        public static readonly char[] defaultEosCharacters = new char[]
        {
            '.',
            '!',
            '?'
        };
        public static readonly char[] thEosCharacters = new char[]
        {
            ' ',
            '\n'
        };
        public static readonly char[] jpnEosCharacters = new char[]
        {
            '。',
            '！',
            '？'
        };
        public virtual EndOfSentenceScanner CreateEndOfSentenceScanner(string languageCode)
        {
            return new DefaultEndOfSentenceScanner(GetEOSCharacters(languageCode));
        }

        public virtual EndOfSentenceScanner CreateEndOfSentenceScanner(char[] customEOSCharacters)
        {
            return new DefaultEndOfSentenceScanner(customEOSCharacters);
        }

        public virtual SDContextGenerator CreateSentenceContextGenerator(string languageCode, ISet<string> abbreviations)
        {
            if ("th".Equals(languageCode) || "tha".Equals(languageCode))
            {
                return new SentenceContextGenerator();
            }
            else if ("pt".Equals(languageCode) || "por".Equals(languageCode))
            {
                return new DefaultSDContextGenerator(abbreviations, ptEosCharacters);
            }

            return new DefaultSDContextGenerator(abbreviations, defaultEosCharacters);
        }

        public virtual SDContextGenerator CreateSentenceContextGenerator(ISet<string> abbreviations, char[] customEOSCharacters)
        {
            return new DefaultSDContextGenerator(abbreviations, customEOSCharacters);
        }

        public virtual SDContextGenerator CreateSentenceContextGenerator(string languageCode)
        {
            return CreateSentenceContextGenerator(languageCode, new HashSet<string>());
        }

        public virtual char[] GetEOSCharacters(string languageCode)
        {
            if ("th".Equals(languageCode) || "tha".Equals(languageCode))
            {
                return thEosCharacters;
            }
            else if ("pt".Equals(languageCode) || "por".Equals(languageCode))
            {
                return ptEosCharacters;
            }
            else if ("jpn".Equals(languageCode))
            {
                return jpnEosCharacters;
            }

            return defaultEosCharacters;
        }
    }
}

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

using Opennlp.Tools.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Opennlp.Tools.Namefind
{
    public class BioCodec : SequenceCodec<string>
    {
        public static readonly string START = "start";
        public static readonly string CONTINUE = "cont";
        public static readonly string OTHER = "other";
        private static readonly Regex typedOutcomePattern = new Regex("(.+)-\\w+", RegexOptions.Compiled);
        static string ExtractNameType(string outcome)
        {
            var matcher = typedOutcomePattern.Match(outcome);
            if (matcher.Success)
            {
                return matcher.Groups[1].Value;
            }

            return null;
        }

        public virtual Span[] Decode(IList<string> c)
        {
            int start = -1;
            int end = -1;
            IList<Span> spans = new List<Span>(c.Count);
            for (int li = 0; li < c.Count; li++)
            {
                string chunkTag = c[li];
                if (chunkTag.EndsWith(BioCodec.START))
                {
                    if (start != -1)
                    {
                        spans.Add(new Span(start, end, ExtractNameType(c[li - 1])));
                    }

                    start = li;
                    end = li + 1;
                }
                else if (chunkTag.EndsWith(BioCodec.CONTINUE))
                {
                    end = li + 1;
                }
                else if (chunkTag.EndsWith(BioCodec.OTHER))
                {
                    if (start != -1)
                    {
                        spans.Add(new Span(start, end, ExtractNameType(c[li - 1])));
                        start = -1;
                        end = -1;
                    }
                }
            }

            if (start != -1)
            {
                spans.Add(new Span(start, end, ExtractNameType(c[c.Count - 1])));
            }

            return spans.ToArray();
        }

        public virtual String[] Encode(Span[] names, int length)
        {
            string[] outcomes = new string[length];
            for (int i = 0; i < outcomes.Length; i++)
            {
                outcomes[i] = BioCodec.OTHER;
            }

            foreach (Span name in names)
            {
                if (name.GetType() == null)
                {
                    outcomes[name.GetStart()] = "default" + "-" + BioCodec.START;
                }
                else
                {
                    outcomes[name.GetStart()] = name.GetType() + "-" + BioCodec.START;
                }


                // now iterate from begin + 1 till end
                for (int i = name.GetStart() + 1; i < name.GetEnd(); i++)
                {
                    if (name.GetType() == null)
                    {
                        outcomes[i] = "default" + "-" + BioCodec.CONTINUE;
                    }
                    else
                    {
                        outcomes[i] = name.GetType() + "-" + BioCodec.CONTINUE;
                    }
                }
            }

            return outcomes;
        }

        public virtual SequenceValidator<string> CreateSequenceValidator()
        {
            return new NameFinderSequenceValidator();
        }

        public virtual bool AreOutcomesCompatible(string[] outcomes)
        {

            // We should have *optionally* one outcome named "other", some named xyz-start and sometimes
            // they have a pair xyz-cont. We should not have any other outcome
            // To validate the model we check if we have one outcome named "other", at least
            // one outcome with suffix start. After that we check if all outcomes that ends with
            // "cont" have a pair that ends with "start".
            IList<string> start = new List<string>();
            IList<string> cont = new List<string>();
            for (int i = 0; i < outcomes.Length; i++)
            {
                string outcome = outcomes[i];
                if (outcome.EndsWith(BioCodec.START))
                {
                    start.Add(outcome.Substring(0, outcome.Length - BioCodec.START.Length));
                }
                else if (outcome.EndsWith(BioCodec.CONTINUE))
                {
                    cont.Add(outcome.Substring(0, outcome.Length - BioCodec.CONTINUE.Length));
                }
                else if (!outcome.Equals(BioCodec.OTHER))
                {

                    // got unexpected outcome
                    return false;
                }
            }

            if (start.Count == 0)
            {
                return false;
            }
            else
            {
                foreach (string contPreffix in cont)
                {
                    if (!start.Contains(contPreffix))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}

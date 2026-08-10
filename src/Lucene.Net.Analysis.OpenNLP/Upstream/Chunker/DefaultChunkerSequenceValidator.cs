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
using System;
using Opennlp.Tools.Util;

namespace Opennlp.Tools.Chunker
{
    internal class DefaultChunkerSequenceValidator : SequenceValidator<TokenTag>
    {
        private bool ValidOutcome(string outcome, string prevOutcome)
        {
            if (outcome.StartsWith("I-", StringComparison.Ordinal))
            {
                if (prevOutcome == null)
                {
                    return false;
                }
                else
                {
                    if (prevOutcome.Equals("O"))
                    {
                        return false;
                    }

                    if (!prevOutcome.Substring(2).Equals(outcome.Substring(2)))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        protected virtual bool ValidOutcome(string outcome, string[] sequence)
        {
            string prevOutcome = null;
            if (sequence.Length > 0)
            {
                prevOutcome = sequence[sequence.Length - 1];
            }

            return ValidOutcome(outcome, prevOutcome);
        }

        public virtual bool ValidSequence(int i, TokenTag[] sequence, string[] s, string outcome)
        {
            return ValidOutcome(outcome, s);
        }
    }
}

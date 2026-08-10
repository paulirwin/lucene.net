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

namespace Opennlp.Tools.Namefind
{
    /// <summary>
    /// This class is created by the {@link BioCodec}.
    /// </summary>
    internal class NameFinderSequenceValidator : SequenceValidator<string>
    {
        public virtual bool ValidSequence(int i, string[] inputSequence, string[] outcomesSequence, string outcome)
        {

            // outcome is formatted like "cont" or "sometype-cont", so we
            // can check if it ends with "cont".
            if (outcome.EndsWith(BioCodec.CONTINUE, StringComparison.Ordinal))
            {
                int li = outcomesSequence.Length - 1;
                if (li == -1)
                {
                    return false;
                }
                else if (outcomesSequence[li].EndsWith(BioCodec.OTHER, StringComparison.Ordinal))
                {
                    return false;
                }
                else if (outcomesSequence[li].EndsWith(BioCodec.CONTINUE, StringComparison.Ordinal) || outcomesSequence[li].EndsWith(BioCodec.START, StringComparison.Ordinal))
                {

                    // if it is continue or start, we have to check if previous match was of the same type
                    string previousNameType = NameFinderME.ExtractNameType(outcomesSequence[li]);
                    string nameType = NameFinderME.ExtractNameType(outcome);
                    if (previousNameType != null || nameType != null)
                    {
                        if (nameType != null)
                        {
                            if (nameType.Equals(previousNameType))
                            {
                                return true;
                            }
                        }

                        return false; // outcomes types are not equal
                    }
                }
            }

            return true;
        }
    }
}

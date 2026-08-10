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
using Opennlp.Tools.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Opennlp.Tools.Postag
{
    internal class DefaultPOSSequenceValidator : SequenceValidator<string>
    {
        private TagDictionary tagDictionary;
        public DefaultPOSSequenceValidator(TagDictionary tagDictionary)
        {
            this.tagDictionary = tagDictionary;
        }

        public virtual bool ValidSequence(int i, string[] inputSequence, string[] outcomesSequence, string outcome)
        {
            if (tagDictionary == null)
            {
                return true;
            }
            else
            {
                string[] tags = tagDictionary.GetTags(inputSequence[i]);
                return tags == null || new List<string>(tags).Contains(outcome);
            }
        }
    }
}

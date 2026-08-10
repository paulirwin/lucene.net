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
using Opennlp.Tools.Sentdetect;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Opennlp.Tools.Sentdetect.Lang.Th
{
    /// <summary>
    /// Creates contexts/features for end-of-sentence detection in Thai text.
    /// </summary>
    internal class SentenceContextGenerator : DefaultSDContextGenerator
    {
        public static readonly char[] eosCharacters = new[]
        {
            ' ',
            '\n'
        };
        public SentenceContextGenerator() : base(eosCharacters)
        {
        }

        protected override void CollectFeatures(string prefix, string suffix, string previous, string next)
        {
            buf.Append("p=");
            buf.Append(prefix);
            collectFeats.Add(buf.ToString());
            buf.Clear();
            buf.Append("s=");
            buf.Append(suffix);
            collectFeats.Add(buf.ToString());
            buf.Clear();
            collectFeats.Add("p1=" + prefix.Substring(Math.Max(prefix.Length - 1, 0)));
            collectFeats.Add("p2=" + prefix.Substring(Math.Max(prefix.Length - 2, 0)));
            collectFeats.Add("p3=" + prefix.Substring(Math.Max(prefix.Length - 3, 0)));
            collectFeats.Add("p4=" + prefix.Substring(Math.Max(prefix.Length - 4, 0)));
            collectFeats.Add("p5=" + prefix.Substring(Math.Max(prefix.Length - 5, 0)));
            collectFeats.Add("p6=" + prefix.Substring(Math.Max(prefix.Length - 6, 0)));
            collectFeats.Add("p7=" + prefix.Substring(Math.Max(prefix.Length - 7, 0)));
            collectFeats.Add("n1=" + suffix.Substring(0, Math.Min(1, suffix.Length)));
            collectFeats.Add("n2=" + suffix.Substring(0, Math.Min(2, suffix.Length)));
            collectFeats.Add("n3=" + suffix.Substring(0, Math.Min(3, suffix.Length)));
            collectFeats.Add("n4=" + suffix.Substring(0, Math.Min(4, suffix.Length)));
            collectFeats.Add("n5=" + suffix.Substring(0, Math.Min(5, suffix.Length)));
            collectFeats.Add("n6=" + suffix.Substring(0, Math.Min(6, suffix.Length)));
            collectFeats.Add("n7=" + suffix.Substring(0, Math.Min(7, suffix.Length)));
        }
    }
}

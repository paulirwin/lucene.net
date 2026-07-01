using Lucene.Net.Attributes;
using Lucene.Net.NUnit.TestUtilities;
using Lucene.Net.TestData.Lifecycle;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;

namespace Lucene.Net.Util
{
    /*
     * Licensed to the Apache Software Foundation (ASF) under one or more
     * contributor license agreements.  See the NOTICE file distributed with
     * this work for additional information regarding copyright ownership.
     * The ASF licenses this file to You under the Apache License, Version 2.0
     * (the "License"); you may not use this file except in compliance with
     * the License.  You may obtain a copy of the License at
     *
     *     http://www.apache.org/licenses/LICENSE-2.0
     *
     * Unless required by applicable law or agreed to in writing, software
     * distributed under the License is distributed on an "AS IS" BASIS,
     * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
     * See the License for the specific language governing permissions and
     * limitations under the License.
     */

    // LUCENENET SPIKE (issue #1087): Throwaway test that prints the actual firing order of NUnit 4
    // Execution Hooks relative to subclass lifecycle methods. Not for merge.
    [TestFixture, LuceneNetSpecific]
    public class SpikeHookOrderingTests
    {
        private static void Dump(string label, Type fixtureType)
        {
            SpikeHookRecordingFixture.Events.Clear();
            ITestResult result = TestBuilder.RunTestFixture(fixtureType);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== " + label + "  ResultState=" + result.ResultState + " ===");
            foreach (var e in SpikeHookRecordingFixture.Events)
                sb.AppendLine("  " + e);
            sb.AppendLine("HOOK_FIRED=" + SpikeHookRecordingFixture.Events.Exists(e => e.StartsWith("HOOK.", StringComparison.Ordinal)));

            System.IO.File.AppendAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "spike1087.txt"), sb.ToString());
        }

        [Test]
        public void Spike_HookOrdering_WithBase() => Dump("WITH BASE", typeof(SpikeHookRecordingFixture));

        [Test]
        public void Spike_HookOrdering_NoBase() => Dump("NO BASE", typeof(SpikeHookNoBaseFixture));
    }
}

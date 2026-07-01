using Lucene.Net.Util;
using NUnit.Framework;
using NUnit.Framework.Internal.ExecutionHooks;
using System;
using System.Collections.Generic;

namespace Lucene.Net.TestData.Lifecycle
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

    // LUCENENET SPIKE (issue #1087): Throwaway code to observe whether NUnit 4 Execution Hooks
    // wrap around subclass OneTimeSetUp/SetUp/TearDown/OneTimeTearDown, and whether they still
    // fire when a subclass overrides those methods WITHOUT calling base. Not for merge.

    /// <summary>
    /// Records hook firings into <see cref="SpikeHookRecordingFixture.Events"/>. A <c>scope</c>
    /// of one-time vs per-test is inferred from <c>HookData.Context.Test.IsSuite</c>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class SpikeRecordingHookAttribute : ExecutionHookAttribute
    {
        private static string Scope(HookData d)
            => d.Context?.Test?.IsSuite == true ? "OneTime" : "Test";

        public override void BeforeEverySetUpHook(HookData d)
            => SpikeHookRecordingFixture.Events.Add($"HOOK.BeforeEverySetUp[{Scope(d)}]");

        public override void AfterEverySetUpHook(HookData d)
            => SpikeHookRecordingFixture.Events.Add($"HOOK.AfterEverySetUp[{Scope(d)}]");

        public override void BeforeTestHook(HookData d)
            => SpikeHookRecordingFixture.Events.Add("HOOK.BeforeTest");

        public override void AfterTestHook(HookData d)
            => SpikeHookRecordingFixture.Events.Add("HOOK.AfterTest");

        public override void BeforeEveryTearDownHook(HookData d)
            => SpikeHookRecordingFixture.Events.Add($"HOOK.BeforeEveryTearDown[{Scope(d)}]");

        public override void AfterEveryTearDownHook(HookData d)
            => SpikeHookRecordingFixture.Events.Add($"HOOK.AfterEveryTearDown[{Scope(d)}]");
    }

    /// <summary>
    /// Subclasses <see cref="LuceneTestCase"/> and records its own lifecycle calls interleaved with
    /// the hook firings, so the spike test can read the actual ordering.
    /// </summary>
    [SpikeRecordingHook]
    public class SpikeHookRecordingFixture : LuceneTestCase
    {
        public static readonly List<string> Events = new List<string>();

        public override void OneTimeSetUp()
        {
            base.OneTimeSetUp();
            Events.Add("SUB.OneTimeSetUp");
        }

        public override void OneTimeTearDown()
        {
            Events.Add("SUB.OneTimeTearDown");
            base.OneTimeTearDown();
        }

        public override void SetUp()
        {
            base.SetUp();
            Events.Add("SUB.SetUp");
        }

        public override void TearDown()
        {
            Events.Add("SUB.TearDown");
            base.TearDown();
        }

        [Test]
        public void TestA() => Events.Add("SUB.TestA");
    }

    /// <summary>
    /// Like <see cref="SpikeHookRecordingFixture"/> but the overrides deliberately DO NOT call base.
    /// This tests the core #1087 question: does the framework-owned hook still fire (so framework
    /// setup/teardown is unskippable) even when a user forgets <c>base</c>?
    /// </summary>
    [SpikeRecordingHook]
    public class SpikeHookNoBaseFixture : LuceneTestCase
    {
        public override void OneTimeSetUp() => SpikeHookRecordingFixture.Events.Add("NOBASE.OneTimeSetUp");
        public override void OneTimeTearDown() => SpikeHookRecordingFixture.Events.Add("NOBASE.OneTimeTearDown");
        public override void SetUp() => SpikeHookRecordingFixture.Events.Add("NOBASE.SetUp");
        public override void TearDown() => SpikeHookRecordingFixture.Events.Add("NOBASE.TearDown");

        [Test]
        public void TestA() => SpikeHookRecordingFixture.Events.Add("NOBASE.TestA");
    }
}

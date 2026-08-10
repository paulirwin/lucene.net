// OpenNLP version compatibility level 1.9.1
using NUnit.Framework;
using Opennlp.Tools.Util;
using Assert = Lucene.Net.TestFramework.Assert;

namespace Lucene.Net.Analysis.OpenNlp.Upstream.Util
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

    /// <summary>
    /// Tests for the <see cref="Version"/> class.
    /// </summary>
    public class TestVersion : Lucene.Net.Util.LuceneTestCase
    {
        [Test]
        public void TestParse()
        {
            Version referenceVersion = Version.CurrentVersion();
            Assert.AreEqual(referenceVersion, Version.Parse(referenceVersion.ToString()));

            Assert.AreEqual(new Version(1, 5, 2, false), Version.Parse("1.5.2-incubating"));
            Assert.AreEqual(new Version(1, 5, 2, false), Version.Parse("1.5.2"));
        }

        [Test]
        public void TestParseSnapshot()
        {
            Assert.AreEqual(new Version(1, 5, 2, true), Version.Parse("1.5.2-incubating-SNAPSHOT"));
            Assert.AreEqual(new Version(1, 5, 2, true), Version.Parse("1.5.2-SNAPSHOT"));
        }

        [Test]
        public void TestParseInvalidVersion()
        {
            Assert.Throws<System.FormatException>(() => Version.Parse("1.5."));
        }

        [Test]
        public void TestParseInvalidVersion2()
        {
            Assert.Throws<System.FormatException>(() => Version.Parse("1.5"));
        }
    }
}

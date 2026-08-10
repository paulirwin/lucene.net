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
    /// Tests for the <see cref="StringUtil"/> class.
    /// </summary>
    public class TestStringUtil : Lucene.Net.Util.LuceneTestCase
    {
        [Test]
        public void TestNoBreakSpace()
        {
            Assert.IsTrue(StringUtil.IsWhitespace(0x00A0));
            Assert.IsTrue(StringUtil.IsWhitespace(0x2007));
            Assert.IsTrue(StringUtil.IsWhitespace(0x202F));

            Assert.IsTrue(StringUtil.IsWhitespace((char)0x00A0));
            Assert.IsTrue(StringUtil.IsWhitespace((char)0x2007));
            Assert.IsTrue(StringUtil.IsWhitespace((char)0x202F));
        }

        [Test]
        public void TestToLowerCase()
        {
            Assert.AreEqual("test", StringUtil.ToLowerCase("TEST"));
            Assert.AreEqual("simple", StringUtil.ToLowerCase("SIMPLE"));
        }

        [Test]
        public void TestToUpperCase()
        {
            Assert.AreEqual("TEST", StringUtil.ToUpperCase("test"));
            Assert.AreEqual("SIMPLE", StringUtil.ToUpperCase("simple"));
        }

        [Test]
        public void TestIsEmpty()
        {
            Assert.IsTrue(StringUtil.IsEmpty(""));
            Assert.IsTrue(!StringUtil.IsEmpty("a"));
        }

        [Test]
        public void TestIsEmptyWithNullString()
        {
            // should raise a NullReferenceException
            Assert.Throws<System.NullReferenceException>(() => StringUtil.IsEmpty(null));
        }
    }
}

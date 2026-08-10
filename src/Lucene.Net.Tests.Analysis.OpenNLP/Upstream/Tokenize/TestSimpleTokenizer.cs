// OpenNLP version compatibility level 1.9.1
using NUnit.Framework;
using Opennlp.Tools.Tokenize;
using Assert = Lucene.Net.TestFramework.Assert;

namespace Lucene.Net.Analysis.OpenNlp.Upstream.Tokenize
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
    /// Tests for the <see cref="SimpleTokenizer"/> class.
    /// </summary>
    public class TestSimpleTokenizer : Lucene.Net.Util.LuceneTestCase
    {
        // The SimpleTokenizer is thread safe
        private readonly SimpleTokenizer mTokenizer = SimpleTokenizer.INSTANCE;

        /// <summary>
        /// Tests if it can tokenize whitespace separated tokens.
        /// </summary>
        [Test]
        public void TestWhitespaceTokenization()
        {
            string text = "a b c  d     e                f    ";

            string[] tokenizedText = mTokenizer.Tokenize(text);

            Assert.IsTrue("a".Equals(tokenizedText[0]));
            Assert.IsTrue("b".Equals(tokenizedText[1]));
            Assert.IsTrue("c".Equals(tokenizedText[2]));
            Assert.IsTrue("d".Equals(tokenizedText[3]));
            Assert.IsTrue("e".Equals(tokenizedText[4]));
            Assert.IsTrue("f".Equals(tokenizedText[5]));

            Assert.IsTrue(tokenizedText.Length == 6);
        }

        /// <summary>
        /// Tests if it can tokenize a word and a dot.
        /// </summary>
        [Test]
        public void TestWordDotTokenization()
        {
            string text = "a.";

            string[] tokenizedText = mTokenizer.Tokenize(text);

            Assert.IsTrue("a".Equals(tokenizedText[0]));
            Assert.IsTrue(".".Equals(tokenizedText[1]));
            Assert.IsTrue(tokenizedText.Length == 2);
        }

        /// <summary>
        /// Tests if it can tokenize a word and numeric.
        /// </summary>
        [Test]
        public void TestWordNumericTokeniztation()
        {
            string text = "305KW";

            string[] tokenizedText = mTokenizer.Tokenize(text);

            Assert.IsTrue("305".Equals(tokenizedText[0]));
            Assert.IsTrue("KW".Equals(tokenizedText[1]));
            Assert.IsTrue(tokenizedText.Length == 2);
        }

        [Test]
        public void TestWordWithOtherTokenization()
        {
            string text = "rebecca.sleep()";

            string[] tokenizedText = mTokenizer.Tokenize(text);

            Assert.IsTrue("rebecca".Equals(tokenizedText[0]));
            Assert.IsTrue(".".Equals(tokenizedText[1]));
            Assert.IsTrue("sleep".Equals(tokenizedText[2]));
            Assert.IsTrue("(".Equals(tokenizedText[3]));
            Assert.IsTrue(")".Equals(tokenizedText[4]));
            Assert.IsTrue(tokenizedText.Length == 5);
        }
    }
}

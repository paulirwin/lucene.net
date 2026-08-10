// OpenNLP version compatibility level 1.9.1
using NUnit.Framework;
using Opennlp.Tools.Sentdetect;
using System.Collections.Generic;
using Assert = Lucene.Net.TestFramework.Assert;

namespace Lucene.Net.Analysis.OpenNlp.Upstream.Sentdetect
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
    /// Tests for the <see cref="DefaultEndOfSentenceScanner"/> class.
    /// </summary>
    public class TestDefaultEndOfSentenceScanner : Lucene.Net.Util.LuceneTestCase
    {
        [Test]
        public void TestScanning()
        {
            EndOfSentenceScanner scanner = new DefaultEndOfSentenceScanner(
                new char[] { '.', '!', '?' });

            IList<int> eosPositions =
                scanner.GetPositions("... um die Wertmarken zu auswählen !?");

            Assert.AreEqual(0, eosPositions[0]);
            Assert.AreEqual(1, eosPositions[1]);
            Assert.AreEqual(2, eosPositions[2]);

            Assert.AreEqual(35, eosPositions[3]);
            Assert.AreEqual(36, eosPositions[4]);
        }
    }
}

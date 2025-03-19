using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Util;
using NUnit.Framework;
using System;

namespace Lucene.Net.Reflection
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

    [TestFixture]
    public class TestReflectionTypeExtensions : LuceneTestCase
    {
        [Test]
        [TestCase(typeof(IndexWriter), "org.apache.lucene.index")]
        [TestCase(typeof(Field), "org.apache.lucene.document")] // test package mapping attribute
        public void TestGetLuceneTypeInfo_PackageNames(Type luceneNetType, string expectedPackageName)
        {
            LuceneTypeInfo info = luceneNetType.GetLuceneTypeInfo();
            Assert.AreEqual(expectedPackageName, info.PackageName);
        }

        [Test]
        [TestCase(typeof(IndexWriter), "IndexWriter")]
        [TestCase(typeof(Field), "Field")] // test package mapping attribute
        [TestCase(typeof(ReferenceContext<>), "ReferenceContext")] // this type doesn't actually exist in Lucene, but it tests the generic type erasure
        [TestCase(typeof(FieldCache.CacheEntry), "FieldCache.CacheEntry")] // test nested type
        [TestCase(typeof(IAttribute), "Attribute")] // test interface
        public void TestGetInferredLuceneTypeName(Type luceneNetType, string expectedName)
        {
            string name = luceneNetType.GetInferredLuceneTypeName();
            Assert.AreEqual(expectedName, name);
        }
    }
}

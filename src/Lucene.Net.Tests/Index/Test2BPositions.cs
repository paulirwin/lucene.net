﻿using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Documents;
using Lucene.Net.Index.Extensions;
using Lucene.Net.Store;
using NUnit.Framework;
using System;

namespace Lucene.Net.Index
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

    using BaseDirectoryWrapper = Lucene.Net.Store.BaseDirectoryWrapper;
    using Document = Documents.Document;
    using Field = Field;
    using FieldType = FieldType;
    using LuceneTestCase = Lucene.Net.Util.LuceneTestCase;
    using MockAnalyzer = Lucene.Net.Analysis.MockAnalyzer;
    using MockDirectoryWrapper = Lucene.Net.Store.MockDirectoryWrapper;
    using TextField = TextField;
    using TokenStream = Lucene.Net.Analysis.TokenStream;

    /// <summary>
    /// Test indexes ~82M docs with 52 positions each, so you get > Integer.MAX_VALUE positions
    /// @lucene.experimental
    /// </summary>
    [SuppressCodecs("SimpleText", "Memory", "Direct")]
    [TestFixture]
    public class Test2BPositions : LuceneTestCase
    {
        // uses lots of space and takes a few minutes
        [Ignore("Very slow. Enable manually by removing Ignore.")]
        [Test]
        public virtual void Test()
        {
            BaseDirectoryWrapper dir = NewFSDirectory(CreateTempDir("2BPositions"));
            if (dir is MockDirectoryWrapper mockDirectoryWrapper)
            {
                mockDirectoryWrapper.Throttling = Throttling.NEVER;
            }

            IndexWriter w = new IndexWriter(dir,
                new IndexWriterConfig(TEST_VERSION_CURRENT, new MockAnalyzer(Random))
                .SetMaxBufferedDocs(IndexWriterConfig.DISABLE_AUTO_FLUSH)
                .SetRAMBufferSizeMB(256.0)
                .SetMergeScheduler(new ConcurrentMergeScheduler())
                .SetMergePolicy(NewLogMergePolicy(false, 10))
                .SetOpenMode(OpenMode.CREATE));

            MergePolicy mp = w.Config.MergePolicy;
            if (mp is LogByteSizeMergePolicy logByteSizeMergePolicy)
            {
                // 1 petabyte:
                logByteSizeMergePolicy.MaxMergeMB = 1024 * 1024 * 1024;
            }

            Document doc = new Document();
            FieldType ft = new FieldType(TextField.TYPE_NOT_STORED);
            ft.OmitNorms = true;
            Field field = new Field("field", new MyTokenStream(), ft);
            doc.Add(field);

            const int numDocs = (int.MaxValue / 26) + 1;
            for (int i = 0; i < numDocs; i++)
            {
                w.AddDocument(doc);
                if (Verbose && i % 100000 == 0)
                {
                    Console.WriteLine(i + " of " + numDocs + "...");
                }
            }
            w.ForceMerge(1);
            w.Dispose();
            dir.Dispose();
        }

        public sealed class MyTokenStream : TokenStream
        {
            private readonly ICharTermAttribute termAtt;
            private readonly IPositionIncrementAttribute posIncAtt;
            internal int index;

            // LUCENENET-specific: must call AddAttribute from ctor in .NET
            public MyTokenStream()
            {
                termAtt = AddAttribute<ICharTermAttribute>();
                posIncAtt = AddAttribute<IPositionIncrementAttribute>();
            }

            public override bool IncrementToken()
            {
                if (index < 52)
                {
                    ClearAttributes();
                    termAtt.Length = 1;
                    termAtt.Buffer[0] = 'a';
                    posIncAtt.PositionIncrement = 1 + index;
                    index++;
                    return true;
                }
                return false;
            }

            public override void Reset()
            {
                index = 0;
            }
        }
    }
}

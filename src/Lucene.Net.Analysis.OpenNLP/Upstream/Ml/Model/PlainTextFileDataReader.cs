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

using System;
using System.IO;
using System.IO.Compression;

namespace Opennlp.Tools.Ml.Model
{
    internal class PlainTextFileDataReader : DataReader
    {
        private StreamReader input;
        public PlainTextFileDataReader(FileInfo f)
        {
            if (f.Name.EndsWith(".gz", StringComparison.Ordinal))
            {
                input = new StreamReader(new GZipStream(f.OpenRead(), CompressionMode.Decompress));
            }
            else
            {
                input = new StreamReader(f.OpenRead());
            }
        }

        public PlainTextFileDataReader(Stream @in)
        {
            input = new StreamReader(@in);
        }

        public PlainTextFileDataReader(StreamReader @in)
        {
            input = @in;
        }

        public virtual double ReadDouble()
        {
            return Double.Parse(input.ReadLine());
        }

        public virtual int ReadInt()
        {
            return int.Parse(input.ReadLine());
        }

        public virtual string ReadUTF()
        {
            return input.ReadLine();
        }
    }
}

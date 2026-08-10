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

namespace Opennlp.Tools.Util.Model
{
    /// <summary>
    /// A <see cref="Stream"/> which cannot be closed.
    /// <para/>
    /// LUCENENET: upstream extends java.io.FilterInputStream, which has no direct
    /// .NET equivalent, so this delegates to the wrapped <see cref="Stream"/>.
    /// </summary>
    internal class UncloseableInputStream : Stream
    {
        private readonly Stream @in;

        public UncloseableInputStream(Stream @in)
        {
            this.@in = @in ?? throw new ArgumentNullException(nameof(@in));
        }

        public override bool CanRead => @in.CanRead;

        public override bool CanSeek => @in.CanSeek;

        public override bool CanWrite => false;

        public override long Length => @in.Length;

        public override long Position
        {
            get => @in.Position;
            set => @in.Position = value;
        }

        public override void Flush() => @in.Flush();

        public override int Read(byte[] buffer, int offset, int count) => @in.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => @in.Seek(offset, origin);

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        /// <summary>
        /// This method does not have any effect; the <see cref="Stream"/>
        /// cannot be closed.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            // LUCENENET: intentionally does not dispose the wrapped stream.
        }
    }
}

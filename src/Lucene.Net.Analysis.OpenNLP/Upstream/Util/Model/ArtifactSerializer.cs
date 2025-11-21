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

#nullable enable
using System;
using System.IO;

namespace Opennlp.Tools.Util.Model
{
    /// <summary>
    /// Responsible to create an artifact from an {@link InputStream}.
    /// </summary>
    public interface ArtifactSerializer<T> : ArtifactSerializer
    {
        /// <summary>
        /// Creates the artifact from the provided {@link InputStream}.
        ///
        /// The {@link InputStream} remains open.
        /// </summary>
        /// <returns>the artifact</returns>
        T Create(Stream @in);

        // /// <summary>
        // /// Serializes the artifact to the provided {@link OutputStream}.
        // ///
        // /// The {@link OutputStream} remains open.
        // /// </summary>
        // void Serialize(T artifact, Stream @out);

        object? ArtifactSerializer.Create(Stream @in) => Create(@in);

        // void ArtifactSerializer.Serialize(object artifact, Stream @out)
        // {
        //     if (artifact is T typedArtifact)
        //     {
        //         Serialize(typedArtifact, @out);
        //     }
        //     else
        //     {
        //         throw new InvalidCastException($"Expected artifact of type {typeof(T)}, but got {artifact?.GetType()}");
        //     }
        // }
    }

    public interface ArtifactSerializer
    {
        object? Create(Stream @in);
        //void Serialize(object artifact, Stream @out);
    }
}

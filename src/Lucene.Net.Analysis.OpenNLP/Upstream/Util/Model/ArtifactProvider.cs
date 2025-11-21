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

namespace Opennlp.Tools.Util.Model
{
    /// <summary>
    /// Provides access to model persisted artifacts.
    /// </summary>
    public interface ArtifactProvider
    {
        /// <summary>
        /// Gets an artifact by name
        /// </summary>
        T GetArtifact<T>(string key);
        /// <summary>
        /// Retrieves the value to the given key from the manifest.properties
        /// entry.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>the value</returns>
        string GetManifestProperty(string key);
        /// <summary>
        /// Retrieves the language code of the material which was used to train the
        /// model or x-unspecified if non was set.
        /// </summary>
        /// <returns>the language code of this model</returns>
        string GetLanguage();
        /// <summary>
        /// Indicates if this provider was loaded from serialized. It is useful, for
        /// example, while validating artifacts: you can skip the time consuming ones
        /// if they where already validated during the serialization.
        /// </summary>
        /// <returns>true if this model was loaded from serialized</returns>
        bool IsLoadedFromSerialized();
    }
}

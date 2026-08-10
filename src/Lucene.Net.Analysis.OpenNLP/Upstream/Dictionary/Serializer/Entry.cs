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
using Opennlp.Tools.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Opennlp.Tools.Dictionary.Serializer
{
    /// <summary>
    /// An {@link Entry} is a {@link StringList} which can
    /// optionally be mapped to attributes.
    /// 
    /// {@link Entry}s is a read and written by the {@link DictionaryEntryPersistor}.
    /// </summary>
    /// <remarks>
    /// @seeDictionaryEntryPersistor
    /// @seeAttributes
    /// </remarks>
    internal class Entry
    {
        private StringList tokens;
        private Attributes attributes;
        /// <summary>
        /// Initializes the current instance.
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="attributes"></param>
        public Entry(StringList tokens, Attributes attributes)
        {
            this.tokens = tokens;
            this.attributes = attributes;
        }

        /// <summary>
        /// Retrieves the tokens.
        /// </summary>
        /// <returns>the tokens</returns>
        public virtual StringList GetTokens()
        {
            return tokens;
        }

        /// <summary>
        /// Retrieves the {@link Attributes}.
        /// </summary>
        /// <returns>the {@link Attributes}</returns>
        public virtual Attributes GetAttributes()
        {
            return attributes;
        }
    }
}

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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Opennlp.Tools.Sentdetect
{
    /// <summary>
    /// Scans Strings, StringBuffers, and char[] arrays for the offsets of
    /// sentence ending characters.
    /// 
    /// <p>Implementations of this interface can use regular expressions,
    /// hand-coded DFAs, and other scanning techniques to locate end of
    /// sentence offsets.</p>
    /// </summary>
    internal interface EndOfSentenceScanner
    {
        /// <summary>
        /// Returns an array of character which can indicate the end of a sentence.
        /// </summary>
        /// <returns>an array of character which can indicate the end of a sentence.</returns>
        char[] GetEndOfSentenceCharacters();
        /// <summary>
        /// Returns a set of character which can indicate the end of a sentence.
        /// </summary>
        /// <returns>a set of character which can indicate the end of a sentence.</returns>
        HashSet<char> GetEOSCharacters();
        /// <summary>
        /// The receiver scans the specified string for sentence ending characters and
        /// returns their offsets.
        /// </summary>
        /// <param name="s">a <code>String</code> value</param>
        /// <returns>a <code>List</code> of Integer objects.</returns>
        IList<int> GetPositions(string s);
        /// <summary>
        /// The receiver scans `buf' for sentence ending characters and
        /// returns their offsets.
        /// </summary>
        /// <param name="buf">a <code>StringBuilder</code> value</param>
        /// <returns>a <code>List</code> of Integer objects.</returns>
        IList<int> GetPositions(StringBuilder buf);
        /// <summary>
        /// The receiver scans `cbuf' for sentence ending characters and
        /// returns their offsets.
        /// </summary>
        /// <param name="cbuf">a <code>char[]</code> value</param>
        /// <returns>a <code>List</code> of Integer objects.</returns>
        IList<int> GetPositions(char[] cbuf);
    }
}

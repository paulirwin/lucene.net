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
using System.IO;
using System.Xml;

namespace Opennlp.Tools.Dictionary.Serializer
{
    /// <summary>
    /// This class is used by for reading and writing dictionaries of all kinds.
    /// </summary>
    internal class DictionaryEntryPersistor
    {
        private const string CHARSET = "UTF-8";

        private const string DICTIONARY_ELEMENT = "dictionary";
        private const string ENTRY_ELEMENT = "entry";
        private const string TOKEN_ELEMENT = "token";
        private const string ATTRIBUTE_CASE_SENSITIVE = "case_sensitive";

        /// <summary>
        /// Creates <see cref="Entry"/>s from the given <see cref="Stream"/> and
        /// forwards these <see cref="Entry"/>s to the <see cref="EntryInserter"/>.
        /// <para/>
        /// After creation is finished the provided <see cref="Stream"/> is closed.
        /// </summary>
        /// <param name="in">stream to read entries from</param>
        /// <param name="inserter">inserter to forward entries to</param>
        /// <returns>isCaseSensitive attribute for Dictionary</returns>
        /// <exception cref="IOException"/>
        /// <exception cref="InvalidFormatException"/>
        public static bool Create(Stream @in, EntryInserter inserter)
        {
            // LUCENENET: the upstream SAX ContentHandler is replaced with an
            // XmlReader pull parser, which is the idiomatic .NET equivalent.
            // The element/attribute handling below mirrors DictionaryContenthandler.
            bool isCaseSensitiveDictionary = true;
            var tokenList = new List<string>();
            Attributes attributes = null;

            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                IgnoreWhitespace = false,
                CloseInput = false,
            };

            try
            {
                using XmlReader reader = XmlReader.Create(@in, settings);

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        string localName = reader.LocalName;
                        bool isEmpty = reader.IsEmptyElement;

                        if (DICTIONARY_ELEMENT.Equals(localName, StringComparison.Ordinal))
                        {
                            attributes = ReadAttributes(reader);

                            /* get the attribute here ... */
                            string caseSensitive = attributes.GetValue(ATTRIBUTE_CASE_SENSITIVE);
                            if (caseSensitive != null)
                            {
                                isCaseSensitiveDictionary = bool.TryParse(caseSensitive, out bool parsed) && parsed;
                            }

                            attributes = null;
                        }
                        else if (ENTRY_ELEMENT.Equals(localName, StringComparison.Ordinal))
                        {
                            attributes = ReadAttributes(reader);

                            if (isEmpty)
                            {
                                InsertEntry(inserter, tokenList, attributes);
                                attributes = null;
                            }
                        }
                        else if (TOKEN_ELEMENT.Equals(localName, StringComparison.Ordinal))
                        {
                            // ReadElementContentAsString advances past the end element,
                            // so the token text is captured here rather than in a
                            // separate characters()/endElement() pair.
                            tokenList.Add(isEmpty ? string.Empty : reader.ReadElementContentAsString().Trim());
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement
                        && ENTRY_ELEMENT.Equals(reader.LocalName, StringComparison.Ordinal))
                    {
                        InsertEntry(inserter, tokenList, attributes);
                        attributes = null;
                    }
                }
            }
            catch (XmlException e)
            {
                throw new InvalidFormatException("The profile data stream has an invalid format!", e);
            }

            return isCaseSensitiveDictionary;
        }

        private static Attributes ReadAttributes(XmlReader reader)
        {
            var attributes = new Attributes();

            if (reader.HasAttributes)
            {
                for (int i = 0; i < reader.AttributeCount; i++)
                {
                    reader.MoveToAttribute(i);
                    attributes.SetValue(reader.LocalName, reader.Value);
                }

                reader.MoveToElement();
            }

            return attributes;
        }

        private static void InsertEntry(EntryInserter inserter, List<string> tokenList, Attributes attributes)
        {
            string[] tokens = tokenList.ToArray();

            Entry entry = new Entry(new StringList(tokens), attributes);

            inserter(entry);

            tokenList.Clear();
        }

        // LUCENENET: serialization is not supported; we only support inference
        // of existing models. The upstream serialize/serializeEntry methods
        // (which use javax.xml.transform SAXTransformerFactory) are omitted.
        //
        // public static void Serialize(Stream @out, IEnumerator<Entry> entries)
        // {
        //     DictionaryEntryPersistor.Serialize(@out, entries, true);
        // }
        //
        // public static void Serialize(Stream @out, IEnumerator<Entry> entries, bool casesensitive)
        // {
        //     ...
        // }
        //
        // private static void SerializeEntry(TransformerHandler hd, Entry entry)
        // {
        //     ...
        // }
    }
}

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
using J2N.Text;

namespace Opennlp.Tools.Ml.Model
{
    public abstract class AbstractModelReader
    {
        /// <summary>
        /// The number of predicates contained in the model.
        /// </summary>
        protected int NUM_PREDS;
        protected DataReader dataReader;
        public AbstractModelReader(FileInfo f)
        {
            string filename = f.Name;
            Stream input;

            // handle the zipped/not zipped distinction
            if (filename.EndsWith(".gz"))
            {
                input = new GZipStream(f.OpenRead(), CompressionMode.Decompress);
                filename = filename.Substring(0, filename.Length - 3);
            }
            else
            {
                input = f.OpenRead();
            }


            // handle the different formats
            if (filename.EndsWith(".bin"))
            {
                this.dataReader = new BinaryFileDataReader(input);
            }
            else
            {

                // filename ends with ".txt"
                this.dataReader = new PlainTextFileDataReader(input);
            }
        }

        public AbstractModelReader(DataReader dataReader) : base()
        {
            this.dataReader = dataReader;
        }

        /// <summary>
        /// Implement as needed for the format the model is stored in.
        /// </summary>
        public virtual int ReadInt()
        {
            return dataReader.ReadInt();
        }

        /// <summary>
        /// Implement as needed for the format the model is stored in.
        /// </summary>
        public virtual double ReadDouble()
        {
            return dataReader.ReadDouble();
        }

        /// <summary>
        /// Implement as needed for the format the model is stored in.
        /// </summary>
        public virtual string ReadUTF()
        {
            return dataReader.ReadUTF();
        }

        public virtual AbstractModel GetModel()
        {
            CheckModelType();
            return ConstructModel();
        }

        public abstract void CheckModelType();
        public abstract AbstractModel ConstructModel();
        protected virtual String[] GetOutcomes()
        {
            int numOutcomes = ReadInt();
            string[] outcomeLabels = new string[numOutcomes];
            for (int i = 0; i < numOutcomes; i++)
                outcomeLabels[i] = ReadUTF();
            return outcomeLabels;
        }

        protected virtual int[][] GetOutcomePatterns()
        {
            int numOCTypes = ReadInt();
            int[][] outcomePatterns = new int[][numOCTypes];
            for (int i = 0; i < numOCTypes; i++)
            {
                StringTokenizer tok = new StringTokenizer(ReadUTF(), " ");
                int[] infoInts = new int[tok.RemainingTokens];
                int j = 0;
                while (tok.MoveNext())
                {
                    infoInts[j] = int.Parse(tok.Current);
                    j++;
                }

                outcomePatterns[i] = infoInts;
            }

            return outcomePatterns;
        }

        protected virtual string[] GetPredicates()
        {
            NUM_PREDS = ReadInt();
            string[] predLabels = new string[NUM_PREDS];
            for (int i = 0; i < NUM_PREDS; i++)
                predLabels[i] = ReadUTF();
            return predLabels;
        }

        /// <summary>
        /// Reads the parameters from a file and populates an array of context objects.
        /// </summary>
        /// <param name="outcomePatterns">The outcomes patterns for the model.  The first index refers to which
        ///     outcome pattern (a set of outcomes that occurs with a context) is being specified.  The
        ///     second index specifies the number of contexts which use this pattern at index 0, and the
        ///     index of each outcomes which make up this pattern in indicies 1-n.</param>
        /// <returns>An array of context objects.</returns>
        /// <exception cref="IOException">when the model file does not match the outcome patterns or can not be read.</exception>
        protected virtual Context[] GetParameters(int[][] outcomePatterns)
        {
            Context[] @params = new Context[NUM_PREDS];
            int pid = 0;
            for (int i = 0; i < outcomePatterns.Length; i++)
            {
                //construct outcome pattern
                int[] outcomePattern = new int[outcomePatterns[i].Length - 1];
                Array.Copy(outcomePatterns[i], 1, outcomePattern, 0, outcomePatterns[i].Length - 1);

                //populate parameters for each context which uses this outcome pattern.
                for (int j = 0; j < outcomePatterns[i][0]; j++)
                {
                    double[] contextParameters = new double[outcomePatterns[i].Length - 1];
                    for (int k = 1; k < outcomePatterns[i].Length; k++)
                    {
                        contextParameters[k - 1] = ReadDouble();
                    }

                    @params[pid] = new Context(outcomePattern, contextParameters);
                    pid++;
                }
            }

            return @params;
        }
    }
}

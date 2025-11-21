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

using Opennlp.Tools.Ml.Model;
using System;
using System.IO;

namespace Opennlp.Tools.Ml.Naivebayes
{
    /// <summary>
    /// Abstract parent class for readers of NaiveBayes.
    /// </summary>
    public class NaiveBayesModelReader : AbstractModelReader
    {
        public NaiveBayesModelReader(FileInfo file) : base(file)
        {
        }

        public NaiveBayesModelReader(DataReader dataReader) : base(dataReader)
        {
        }

        /// <summary>
        /// Retrieve a model from disk. It assumes that models are saved in the
        /// following sequence:
        ///
        /// <br>NaiveBayes (model type identifier)
        /// <br>1. # of parameters (int)
        /// <br>2. # of outcomes (int)
        /// <br>  * list of outcome names (String)
        /// <br>3. # of different types of outcome patterns (int)
        /// <br>   * list of (int int[])
        /// <br>   [# of predicates for which outcome pattern is true] [outcome pattern]
        /// <br>4. # of predicates (int)
        /// <br>   * list of predicate names (String)
        ///
        /// <p>If you are creating a reader for a format which won't work with this
        /// (perhaps a database or xml file), override this method and ignore the
        /// other methods provided in this abstract class.
        /// </summary>
        /// <returns>The NaiveBayesModel stored in the format and location specified to
        ///     this NaiveBayesModelReader (usually via its the constructor).</returns>
        public override AbstractModel ConstructModel()
        {
            string[] outcomeLabels = GetOutcomes();
            int[][] outcomePatterns = GetOutcomePatterns();
            string[] predLabels = GetPredicates();
            Context[] @params = GetParameters(outcomePatterns);
            return new NaiveBayesModel(@params, predLabels, outcomeLabels);
        }

        public override void CheckModelType()
        {
            string modelType = ReadUTF();
            if (!modelType.Equals("NaiveBayes"))
                Console.WriteLine("Error: attempting to load a " + modelType + " model as a NaiveBayes model." + " You should expect problems.");
        }
    }
}

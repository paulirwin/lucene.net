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
using Opennlp.Tools.Ml.Model;
using System.Collections.Generic;
using System.IO;
using J2N.IO;
using Lucene.Net.Support;
using Lucene.Net.Support.IO;

namespace Opennlp.Tools.Util.Model
{
    /// <summary>
    /// Utility class for handling of {@link MaxentModel}s.
    /// </summary>
    public sealed class ModelUtil
    {
        private ModelUtil()
        {
        }

        // /// <summary>
        // /// Writes the given model to the given {@link OutputStream}.
        // ///
        // /// This methods does not closes the provided stream.
        // /// </summary>
        // /// <param name="model">the model to be written</param>
        // /// <param name="out">the stream the model should be written to</param>
        // public static void WriteModel(MaxentModel model, Stream @out)
        // {
        //     ArgumentNullException.ThrowIfNull(model);
        //     ArgumentNullException.ThrowIfNull(@out);
        //     GenericModelWriter modelWriter = new GenericModelWriter((AbstractModel)model, new DataOutputStream(new AnonymousOutputStream(this)));
        //     modelWriter.Persist();
        // }
        //
        // private sealed class AnonymousOutputStream : Stream
        // {
        //     public AnonymousOutputStream(ModelUtil parent)
        //     {
        //         this.parent = parent;
        //     }
        //
        //     private readonly ModelUtil parent;
        //     public void Write(int b)
        //     {
        //         @out.Write(b);
        //     }
        // }

        /// <summary>
        /// Checks if the expected outcomes are all contained as outcomes in the given model.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="expectedOutcomes"></param>
        /// <returns>true if all expected outcomes are the only outcomes of the model.</returns>
        public static bool ValidateOutcomes(MaxentModel model, params string[] expectedOutcomes)
        {
            bool result = true;
            if (expectedOutcomes.Length == model.GetNumOutcomes())
            {
                var expectedOutcomesSet = new HashSet<string>();
                expectedOutcomesSet.UnionWith(expectedOutcomes);
                for (int i = 0; i < model.GetNumOutcomes(); i++)
                {
                    if (!expectedOutcomesSet.Contains(model.GetOutcome(i)))
                    {
                        result = false;
                        break;
                    }
                }
            }
            else
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Writes the provided {@link InputStream} into a byte array
        /// which is returned
        /// </summary>
        /// <param name="in">stream to read data for the byte array from</param>
        /// <returns>byte array with the contents of the stream</returns>
        /// <exception cref="IOException">if an exception is thrown while reading
        ///     from the provided {@link InputStream}</exception>
        public static byte[] Read(Stream @in)
        {
            ByteArrayOutputStream byteArrayOut = new ByteArrayOutputStream();
            int length;
            byte[] buffer = new byte[1024];
            while ((length = @in.Read(buffer)) > 0)
            {
                byteArrayOut.Write(buffer, 0, length);
            }

            byteArrayOut.Dispose();
            return byteArrayOut.ToArray();
        }

        public static void AddCutoffAndIterations(Dictionary<string, string> manifestInfoEntries, int cutoff, int iterations)
        {
            manifestInfoEntries.Put(BaseModel.TRAINING_CUTOFF_PROPERTY, cutoff.ToString());
            manifestInfoEntries.Put(BaseModel.TRAINING_ITERATIONS_PROPERTY, iterations.ToString());
        }

        // /// <summary>
        // /// Creates the default training parameters in case they are not provided.
        // ///
        // /// Note: Do not use this method, internal use only!
        // /// </summary>
        // /// <returns>training parameters instance</returns>
        // public static TrainingParameters CreateDefaultTrainingParameters()
        // {
        //     TrainingParameters mlParams = new TrainingParameters();
        //     mlParams.Put(TrainingParameters.ALGORITHM_PARAM, GISTrainer.MAXENT_VALUE);
        //     mlParams.Put(TrainingParameters.ITERATIONS_PARAM, 100);
        //     mlParams.Put(TrainingParameters.CUTOFF_PARAM, 5);
        //     return mlParams;
        // }
    }
}

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
using System.Collections.Generic;

namespace Opennlp.Tools.Ml
{
    /// <summary>
    /// Utility class for simple vector arithmetic.
    /// </summary>
    public class ArrayMath
    {
        public static double InnerProduct(double[] vecA, double[] vecB)
        {
            if (vecA == null || vecB == null || vecA.Length != vecB.Length)
                return Double.NaN;
            double product = 0;
            for (int i = 0; i < vecA.Length; i++)
            {
                product += vecA[i] * vecB[i];
            }

            return product;
        }

        /// <summary>
        /// L1-norm
        /// </summary>
        public static double L1norm(double[] v)
        {
            double norm = 0;
            for (int i = 0; i < v.Length; i++)
                norm += Math.Abs(v[i]);
            return norm;
        }

        /// <summary>
        /// L2-norm
        /// </summary>
        public static double L2norm(double[] v)
        {
            return Math.Sqrt(InnerProduct(v, v));
        }

        /// <summary>
        /// Inverse L2-norm
        /// </summary>
        public static double InvL2norm(double[] v)
        {
            return 1 / L2norm(v);
        }

        /// <summary>
        /// Computes \log(\sum_{i=1}^n e^{x_i}) using a maximum-element trick
        /// to avoid arithmetic overflow.
        /// </summary>
        /// <param name="x">input vector</param>
        /// <returns>log-sum of exponentials of vector elements</returns>
        public static double LogSumOfExps(double[] x)
        {
            double max = Max(x);
            double sum = 0;
            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != Double.NegativeInfinity)
                    sum += Math.Exp(x[i] - max);
            }

            return max + Math.Log(sum);
        }

        public static double Max(double[] x)
        {
            int maxIdx = Argmax(x);
            return x[maxIdx];
        }

        /// <summary>
        /// Find index of maximum element in the vector x
        /// </summary>
        /// <param name="x">input vector</param>
        /// <returns>index of the maximum element. Index of the first
        ///     maximum element is returned if multiple maximums are found.</returns>
        public static int Argmax(double[] x)
        {
            if (x == null || x.Length == 0)
            {
                throw new ArgumentException("Vector x is null or empty");
            }

            int maxIdx = 0;
            for (int i = 1; i < x.Length; i++)
            {
                if (x[maxIdx] < x[i])
                    maxIdx = i;
            }

            return maxIdx;
        }

        public static void SumFeatures(Context[] context, float[] values, double[] prior)
        {
            for (int ci = 0; ci < context.Length; ci++)
            {
                if (context[ci] != null)
                {
                    Context predParams = context[ci];
                    int[] activeOutcomes = predParams.GetOutcomes();
                    double[] activeParameters = predParams.GetParameters();
                    double value = 1;
                    if (values != null)
                    {
                        value = values[ci];
                    }

                    for (int ai = 0; ai < activeOutcomes.Length; ai++)
                    {
                        int oid = activeOutcomes[ai];
                        prior[oid] += activeParameters[ai] * value;
                    }
                }
            }
        }

        // === Not really related to math ===
        /// <summary>
        /// Convert a list of Double objects into an array of primitive doubles
        /// </summary>
        public static double[] ToDoubleArray(IList<Double> list)
        {
            double[] arr = new double[list.Count];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = list[i];
            }

            return arr;
        }

        /// <summary>
        ///  Convert a list of Integer objects into an array of primitive integers
        /// </summary>
        public static int[] ToIntArray(IList<int> list)
        {
            int[] arr = new int[list.Count];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = list[i];
            }

            return arr;
        }
    }
}

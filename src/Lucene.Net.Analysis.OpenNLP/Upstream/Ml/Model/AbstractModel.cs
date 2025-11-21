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
using System.Text;
using Lucene.Net.Support;

namespace Opennlp.Tools.Ml.Model
{
    public abstract class AbstractModel : MaxentModel
    {
        /// <summary>
        /// Mapping between predicates/contexts and an integer representing them.
        /// </summary>
        protected Dictionary<string, Context> pmap;
        /// <summary>
        /// The names of the outcomes.
        /// </summary>
        protected string[] outcomeNames;
        /// <summary>
        /// Parameters for the model.
        /// </summary>
        protected EvalParameters evalParams;
        /// <summary>
        /// Prior distribution for this model.
        /// </summary>
        protected Prior prior;
        public enum ModelType
        {
            Maxent,
            Perceptron,
            MaxentQn,
            NaiveBayes
        }

        /// <summary>
        /// The type of the model.
        /// </summary>
        protected ModelType modelType;
        protected AbstractModel(Context[] @params, string[] predLabels, Dictionary<string, Context> pmap, string[] outcomeNames)
        {
            this.pmap = pmap;
            this.outcomeNames = outcomeNames;
            this.evalParams = new EvalParameters(@params, outcomeNames.Length);
        }

        public AbstractModel(Context[] @params, string[] predLabels, string[] outcomeNames)
        {
            Init(predLabels, @params, outcomeNames);
            this.evalParams = new EvalParameters(@params, outcomeNames.Length);
        }

        private void Init(string[] predLabels, Context[] @params, string[] outcomeNames)
        {
            this.pmap = new Dictionary<string, Context>(predLabels.Length);
            for (int i = 0; i < predLabels.Length; i++)
            {
                pmap.Put(predLabels[i], @params[i]);
            }

            this.outcomeNames = outcomeNames;
        }

        // LUCENENET: from MaxentModel interface
        public abstract double[] Eval(string[] context);
        public abstract double[] Eval(string[] context, double[] probs);
        public abstract double[] Eval(string[] context, float[] values);

        /// <summary>
        /// Return the name of the outcome corresponding to the highest likelihood
        /// in the parameter ocs.
        /// </summary>
        /// <param name="ocs">A double[] as returned by the eval(String[] context)
        ///            method.</param>
        /// <returns>   The name of the most likely outcome.</returns>
        public string GetBestOutcome(double[] ocs)
        {
            return outcomeNames[ArrayMath.Argmax(ocs)];
        }

        public virtual ModelType GetModelType()
        {
            return modelType;
        }

        /// <summary>
        /// Return a string matching all the outcome names with all the
        /// probabilities produced by the <code>eval(String[] context)</code>
        /// method.
        /// </summary>
        /// <param name="ocs">A <code>double[]</code> as returned by the
        ///            <code>eval(String[] context)</code>
        ///            method.</param>
        /// <returns>   String containing outcome names paired with the normalized
        ///            probability (contained in the <code>double[] ocs</code>)
        ///            for each one.</returns>
        public string GetAllOutcomes(double[] ocs)
        {
            if (ocs.Length != outcomeNames.Length)
            {
                return "The double array sent as a parameter to GISModel.getAllOutcomes() " + "must not have been produced by this model.";
            }
            else
            {
                //DecimalFormat df = new DecimalFormat("0.0000");
                StringBuilder sb = new StringBuilder(ocs.Length * 2);
                sb.Append(outcomeNames[0]).Append("[").Append(ocs[0].ToString("0.0000")).Append("]");
                for (int i = 1; i < ocs.Length; i++)
                {
                    sb.Append("  ").Append(outcomeNames[i]).Append("[").Append(ocs[i].ToString("0.0000")).Append("]");
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Return the name of an outcome corresponding to an int id.
        /// </summary>
        /// <param name="i">An outcome id.</param>
        /// <returns> The name of the outcome associated with that id.</returns>
        public string GetOutcome(int i)
        {
            return outcomeNames[i];
        }

        /// <summary>
        /// Gets the index associated with the String name of the given outcome.
        /// </summary>
        /// <param name="outcome">the String name of the outcome for which the
        ///          index is desired</param>
        /// <returns>the index if the given outcome label exists for this
        ///     model, -1 if it does not.</returns>
        public virtual int GetIndex(string outcome)
        {
            for (int i = 0; i < outcomeNames.Length; i++)
            {
                if (outcomeNames[i].Equals(outcome))
                    return i;
            }

            return -1;
        }

        public virtual int GetNumOutcomes()
        {
            return evalParams.GetNumOutcomes();
        }

        /// <summary>
        /// Provides the fundamental data structures which encode the maxent model
        /// information.  This method will usually only be needed by
        /// GISModelWriters.  The following values are held in the Object array
        /// which is returned by this method:
        /// <ul>
        /// <li>index 0: opennlp.tools.ml.maxent.Context[] containing the model
        ///            parameters
        /// <li>index 1: java.util.Map containing the mapping of model predicates
        ///            to unique integers
        /// <li>index 2: java.lang.String[] containing the names of the outcomes,
        ///            stored in the index of the array which represents their
        ///            unique ids in the model.
        /// </ul>
        /// </summary>
        /// <returns>An Object[] with the values as described above.</returns>
        public object[] GetDataStructures()
        {
            object[] data = new object[3];
            data[0] = evalParams.GetParams();
            data[1] = pmap;
            data[2] = outcomeNames;
            return data;
        }

        public virtual int GetHashCode()
        {
            return HashCode.Combine(pmap, Arrays.GetHashCode(outcomeNames), evalParams, prior);
        }

        public virtual bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            if (obj is AbstractModel model)
            {
                return pmap.Equals(model.pmap) && Arrays.Equals(outcomeNames, model.outcomeNames) && Equals(prior, model.prior); // LUCENENET: using Arrays.Equals since this is just a string array
            }

            return false;
        }
    }
}

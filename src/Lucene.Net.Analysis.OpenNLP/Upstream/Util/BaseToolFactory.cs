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
using Opennlp.Tools.Util.Ext;
using Opennlp.Tools.Util.Model;
using System;
using System.Collections.Generic;

namespace Opennlp.Tools.Util
{
    /// <summary>
    /// Base class for all tool factories.
    ///
    /// Extensions of this class should:
    /// <ul>
    ///  <li>implement an empty constructor (TODO is it necessary?)
    ///  <li>implement a constructor that takes the {@link ArtifactProvider} and
    ///      calls {@code BaseToolFactory(Map)}
    ///  <li>override {@link #createArtifactMap()} and
    ///      {@link #createArtifactSerializersMap()} methods if necessary.
    /// </ul>
    /// </summary>
    internal abstract class BaseToolFactory
    {
        protected ArtifactProvider artifactProvider;
        /// <summary>
        /// All sub-classes should have an empty constructor
        /// </summary>
        public BaseToolFactory()
        {
        }

        /// <summary>
        /// Initializes the ToolFactory with an artifact provider.
        /// </summary>
        protected virtual void Init(ArtifactProvider artifactProvider)
        {
            this.artifactProvider = artifactProvider;
        }

        /// <summary>
        /// Creates a {@link Map} with pairs of keys and {@link ArtifactSerializer}.
        /// The models implementation should call this method from
        /// {@code BaseModel#createArtifactSerializersMap}
        /// <p>
        /// The base implementation will return a {@link HashMap} that should be
        /// populated by sub-classes.
        /// </summary>
        public virtual Dictionary<string, ArtifactSerializer> CreateArtifactSerializersMap()
        {
            return new Dictionary<string, ArtifactSerializer>();
        }

        /// <summary>
        /// Creates a {@link Map} with pairs of keys and objects. The models
        /// implementation should call this constructor that creates a model
        /// programmatically.
        /// <p>
        /// The base implementation will return a {@link HashMap} that should be
        /// populated by sub-classes.
        /// </summary>
        public virtual Dictionary<string, object> CreateArtifactMap()
        {
            return new Dictionary<string, object>();
        }

        /// <summary>
        /// Creates the manifest entries that will be added to the model manifest
        /// </summary>
        /// <returns>the manifest entries to added to the model manifest</returns>
        public virtual Dictionary<string, string> CreateManifestEntries()
        {
            return new Dictionary<string, string>();
        }

        /// <summary>
        /// Validates the parsed artifacts. If something is not
        /// valid subclasses should throw an {@link InvalidFormatException}.
        ///
        /// Note:
        /// Subclasses should generally invoke super.validateArtifactMap at the beginning
        /// of this method.
        /// </summary>
        /// <exception cref="InvalidFormatException"></exception>
        public abstract void ValidateArtifactMap();
        public static BaseToolFactory Create(string subclassName, ArtifactProvider artifactProvider)
        {
            BaseToolFactory theFactory;
            try
            {

                // load the ToolFactory using the default constructor
                theFactory = ExtensionLoader.InstantiateExtension<BaseToolFactory>(subclassName);
                if (theFactory != null)
                {
                    theFactory.Init(artifactProvider);
                }
            }
            catch (Exception e)
            {
                string msg = "Could not instantiate the " + subclassName + ". The initialization throw an exception.";
                throw new InvalidFormatException(msg, e);
            }

            return theFactory;
        }

        public static BaseToolFactory Create(Type factoryClass, ArtifactProvider artifactProvider)
        {
            BaseToolFactory theFactory = null;
            if (factoryClass != null)
            {
                try
                {
                    theFactory = (BaseToolFactory)Activator.CreateInstance(factoryClass);
                    theFactory.Init(artifactProvider);
                }
                catch (Exception e)
                {
                    string msg = "Could not instantiate the " + factoryClass.FullName + ". The initialization throw an exception.";
                    throw new InvalidFormatException(msg, e);
                }
            }

            return theFactory;
        }
    }
}

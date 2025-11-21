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
using Opennlp.Tools.Util.Ext;
using Opennlp.Tools.Util.Featuregen;
using System;
using System.Collections.Generic;
using System.IO;
using J2N;
using Lucene;
using Lucene.Net.Support.IO;

namespace Opennlp.Tools.Namefind
{
    // Idea of this factory is that most resources/impls used by the name finder
    // can be modified through this class!
    // That only works if that's the central class used for training/runtime
    public class TokenNameFinderFactory : BaseToolFactory
    {
        private byte[] featureGeneratorBytes;
        private Dictionary<string, object> resources;
        private SequenceCodec<string> seqCodec;
        /// <summary>
        /// Creates a {@link TokenNameFinderFactory} that provides the default implementation
        /// of the resources.
        /// </summary>
        public TokenNameFinderFactory()
        {
            this.seqCodec = new BioCodec();
        }

        public TokenNameFinderFactory(byte[] featureGeneratorBytes, Dictionary<string, object> resources, SequenceCodec<string> seqCodec)
        {
            Init(featureGeneratorBytes, resources, seqCodec);
        }

        public virtual void Init(byte[] featureGeneratorBytes, Dictionary<string, object> resources, SequenceCodec<string> seqCodec)
        {
            this.featureGeneratorBytes = featureGeneratorBytes;
            this.resources = resources;
            this.seqCodec = seqCodec;
        }

        private static byte[] LoadDefaultFeatureGeneratorBytes()
        {
            ByteArrayOutputStream bytes = new ByteArrayOutputStream();
            try
            {
                using (Stream @in = typeof(TokenNameFinderFactory).FindAndGetManifestResourceStream("/opennlp/tools/namefind/ner-default-features.xml"))
                {
                    if (@in == null)
                    {
                        throw new InvalidOperationException("Classpath must contain ner-default-features.xml file!");
                    }

                    byte[] buf = new byte[1024];
                    int len;
                    while ((len = @in.Read(buf)) > 0)
                    {
                        bytes.Write(buf, 0, len);
                    }
                }
            }
            catch (IOException e)
            {
                throw new InvalidOperationException("Failed reading from ner-default-features.xml file on classpath!");
            }

            return bytes.ToArray();
        }

        protected virtual SequenceCodec<string> GetSequenceCodec()
        {
            return seqCodec;
        }

        protected virtual Dictionary<string, object> GetResources()
        {
            return resources;
        }

        protected virtual byte[] GetFeatureGenerator()
        {
            return featureGeneratorBytes;
        }

        public static TokenNameFinderFactory Create(string subclassName, byte[] featureGeneratorBytes, Dictionary<string, object> resources, SequenceCodec<string> seqCodec)
        {
            TokenNameFinderFactory theFactory;
            if (subclassName == null)
            {

                // will create the default factory
                theFactory = new TokenNameFinderFactory();
            }
            else
            {
                try
                {
                    theFactory = ExtensionLoader.InstantiateExtension<TokenNameFinderFactory>(subclassName);
                }
                catch (Exception e)
                {
                    string msg = "Could not instantiate the " + subclassName + ". The initialization throw an exception.";
                    Console.Error.WriteLine(msg);
                    e.PrintStackTrace();
                    throw new InvalidFormatException(msg, e);
                }
            }

            theFactory.Init(featureGeneratorBytes, resources, seqCodec);
            return theFactory;
        }

        public override void ValidateArtifactMap()
        {
        }

        public virtual SequenceCodec<string> CreateSequenceCodec()
        {
            if (artifactProvider != null)
            {
                string sequeceCodecImplName = artifactProvider.GetManifestProperty(TokenNameFinderModel.SEQUENCE_CODEC_CLASS_NAME_PARAMETER);
                return InstantiateSequenceCodec(sequeceCodecImplName);
            }
            else
            {
                return seqCodec;
            }
        }

        public virtual NameContextGenerator CreateContextGenerator()
        {
            AdaptiveFeatureGenerator featureGenerator = CreateFeatureGenerators();
            if (featureGenerator == null)
            {
                featureGenerator = new CachedFeatureGenerator(new WindowFeatureGenerator(new TokenFeatureGenerator(), 2, 2), new WindowFeatureGenerator(new TokenClassFeatureGenerator(true), 2, 2), new OutcomePriorFeatureGenerator(), new PreviousMapFeatureGenerator(), new BigramNameFeatureGenerator(), new SentenceFeatureGenerator(true, false));
            }

            return new DefaultNameContextGenerator(featureGenerator);
        }

        /// <summary>
        /// Creates the {@link AdaptiveFeatureGenerator}. Usually this
        /// is a set of generators contained in the {@link AggregatedFeatureGenerator}.
        ///
        /// Note:
        /// The generators are created on every call to this method.
        /// </summary>
        /// <returns>the feature generator or null if there is no descriptor in the model</returns>
        public virtual AdaptiveFeatureGenerator CreateFeatureGenerators()
        {
            if (featureGeneratorBytes == null && artifactProvider != null)
            {
                featureGeneratorBytes = artifactProvider.GetArtifact<byte[]>(TokenNameFinderModel.GENERATOR_DESCRIPTOR_ENTRY_NAME);
            }

            if (featureGeneratorBytes == null)
            {
                featureGeneratorBytes = LoadDefaultFeatureGeneratorBytes();
            }

            var descriptorIn = new MemoryStream(featureGeneratorBytes);
            AdaptiveFeatureGenerator generator;
            try
            {
                generator = GeneratorFactory.Create(descriptorIn, (key) =>
                {
                    if (artifactProvider != null)
                    {
                        return artifactProvider.GetArtifact<AdaptiveFeatureGenerator>(key);
                    }
                    else
                    {
                        return resources[key];
                    }
                });
            }
            catch (InvalidFormatException e)
            {

                // It is assumed that the creation of the feature generation does not
                // fail after it succeeded once during model loading.
                // But it might still be possible that such an exception is thrown,
                // in this case the caller should not be forced to handle the exception
                // and a Runtime Exception is thrown instead.
                // If the re-creation of the feature generation fails it is assumed
                // that this can only be caused by a programming mistake and therefore
                // throwing a Runtime Exception is reasonable
                throw new TokenNameFinderModel.FeatureGeneratorCreationError(e);
            }
            catch (IOException e)
            {
                throw new InvalidOperationException("Reading from mem cannot result in an I/O error", e);
            }

            return generator;
        }

        public static SequenceCodec<string> InstantiateSequenceCodec(string sequenceCodecImplName)
        {
            if (sequenceCodecImplName != null)
            {
                return ExtensionLoader.InstantiateExtension<SequenceCodec<string>>(sequenceCodecImplName);
            }
            else
            {

                // If nothing is specified return old default!
                return new BioCodec();
            }
        }
    }
}

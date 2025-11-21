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
using J2N;
using Lucene.Net.Analysis.OpenNlp.Upstream.Support;

namespace Opennlp.Tools.Util
{
    /// <summary>
    /// The {@link Version} class represents the OpenNlp Tools library version.
    /// <p>
    /// The version has three parts:
    /// <ul>
    /// <li>Major: OpenNlp Tools libraries with a different major version are not interchangeable.</li>
    /// <li>Minor: OpenNlp Tools libraries with an identical major version, but different
    ///     minor version may be interchangeable. See release notes for further details.</li>
    /// <li>Revision: OpenNlp Tools libraries with same major and minor version, but a different
    ///     revision, are fully interchangeable.</li>
    /// </ul>
    /// </summary>
    public class Version
    {
        private static readonly string DEV_VERSION_STRING = "0.0.0-SNAPSHOT";
        public static readonly Version DEV_VERSION = Version.Parse(DEV_VERSION_STRING);
        private static readonly string SNAPSHOT_MARKER = "-SNAPSHOT";
        private readonly int major;
        private readonly int minor;
        private readonly int revision;
        private readonly bool snapshot;
        /// <summary>
        /// Initializes the current instance with the provided
        /// versions.
        /// </summary>
        /// <param name="major"></param>
        /// <param name="minor"></param>
        /// <param name="revision"></param>
        /// <param name="snapshot"></param>
        public Version(int major, int minor, int revision, bool snapshot)
        {
            this.major = major;
            this.minor = minor;
            this.revision = revision;
            this.snapshot = snapshot;
        }

        /// <summary>
        /// Initializes the current instance with the provided
        /// versions. The version will not be a snapshot version.
        /// </summary>
        /// <param name="major"></param>
        /// <param name="minor"></param>
        /// <param name="revision"></param>
        public Version(int major, int minor, int revision) : this(major, minor, revision, false)
        {
        }

        /// <summary>
        /// Retrieves the major version.
        /// </summary>
        /// <returns>major version</returns>
        public virtual int GetMajor()
        {
            return major;
        }

        /// <summary>
        /// Retrieves the minor version.
        /// </summary>
        /// <returns>minor version</returns>
        public virtual int GetMinor()
        {
            return minor;
        }

        /// <summary>
        /// Retrieves the revision version.
        /// </summary>
        /// <returns>revision version</returns>
        public virtual int GetRevision()
        {
            return revision;
        }

        public virtual bool IsSnapshot()
        {
            return snapshot;
        }

        /// <summary>
        /// Retrieves the version string.
        ///
        /// The {@link #parse(String)} method can create an instance
        /// of {@link Version} with the returned version value string.
        /// </summary>
        /// <returns>the version value string</returns>
        public virtual string ToString()
        {
            return GetMajor() + "." + GetMinor() + "." + GetRevision() + (IsSnapshot() ? SNAPSHOT_MARKER : "");
        }

        public virtual int GetHashCode()
        {
            return HashCode.Combine(GetMajor(), GetMinor(), GetRevision(), IsSnapshot());
        }

        public virtual bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            if (obj is Version)
            {
                Version version = (Version)obj;
                return GetMajor() == version.GetMajor() && GetMinor() == version.GetMinor() && GetRevision() == version.GetRevision() && IsSnapshot() == version.IsSnapshot();
            }

            return false;
        }

        /// <summary>
        /// Return a new {@link Version} initialized to the value
        /// represented by the specified {@link String}
        /// </summary>
        /// <param name="version">the string to be parsed</param>
        /// <returns>the version represented by the string value</returns>
        /// <exception cref="FormatException">if the string does
        ///     not contain a valid version</exception>
        public static Version Parse(string version)
        {
            int indexFirstDot = version.IndexOf('.');
            int indexSecondDot = version.IndexOf('.', indexFirstDot + 1);
            if (indexFirstDot == -1 || indexSecondDot == -1)
            {
                throw new FormatException("Invalid version format '" + version + "', expected two dots!");
            }

            int indexFirstDash = version.IndexOf('-');
            int versionEnd;
            if (indexFirstDash == -1)
            {
                versionEnd = version.Length;
            }
            else
            {
                versionEnd = indexFirstDash;
            }

            bool snapshot = version.EndsWith(SNAPSHOT_MARKER);
            return new Version(int.Parse(version.Substring(0, indexFirstDot)), int.Parse(version.Substring(indexFirstDot + 1, indexSecondDot)), int.Parse(version.Substring(indexSecondDot + 1, versionEnd)), snapshot);
        }

        /// <summary>
        /// Retrieves the current version of the OpenNlp Tools library.
        /// </summary>
        /// <returns>the current version</returns>
        public static Version CurrentVersion()
        {
            Properties manifest = new Properties();

            // Try to read the version from the version file if it is available,
            // otherwise set the version to the development version
            try
            {
                using var versionIn = typeof(Version).FindAndGetManifestResourceStream("opennlp.version");
                if (versionIn != null)
                {
                    manifest.Load(versionIn);
                }
            }
            catch (Exception e)
            {
                // Ignore
            }

            string versionString = manifest.GetProperty("OpenNLP-Version", DEV_VERSION_STRING);
            if (versionString.Equals("${pom.version}"))
                versionString = DEV_VERSION_STRING;
            return Version.Parse(versionString);
        }
    }
}

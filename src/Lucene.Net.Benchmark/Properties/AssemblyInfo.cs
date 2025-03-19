﻿/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Lucene.Net.Reflection;
using System;
using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyDefaultAlias("Lucene.Net.Benchmark")]
[assembly: AssemblyCulture("")]

[assembly: CLSCompliant(true)]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("edc77cb4-597f-4818-8c83-3c006d12c384")]

[assembly: LuceneMavenMapping("org.apache.lucene", "lucene-benchmark", "4.8.1")]
[assembly: LucenePackageMapping("Lucene.Net.Benchmarks", "org.apache.lucene.benchmark")]
[assembly: LucenePackageMapping("Lucene.Net.Benchmarks.ByTask", "org.apache.lucene.benchmark.byTask", Justification = "Casing difference from expected package")]
[assembly: LucenePackageMapping("Lucene.Net.Benchmarks.ByTask.Feeds", "org.apache.lucene.benchmark.byTask.feeds", Justification = "Casing difference from expected package")]
[assembly: LucenePackageMapping("Lucene.Net.Benchmarks.ByTask.Stats", "org.apache.lucene.benchmark.byTask.stats", Justification = "Casing difference from expected package")]
[assembly: LucenePackageMapping("Lucene.Net.Benchmarks.ByTask.Tasks", "org.apache.lucene.benchmark.byTask.tasks", Justification = "Casing difference from expected package")]
[assembly: LucenePackageMapping("Lucene.Net.Benchmarks.ByTask.Utils", "org.apache.lucene.benchmark.byTask.utils", Justification = "Casing difference from expected package")]

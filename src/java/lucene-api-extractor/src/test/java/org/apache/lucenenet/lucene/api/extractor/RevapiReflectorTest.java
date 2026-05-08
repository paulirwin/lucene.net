/*
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

package org.apache.lucenenet.lucene.api.extractor;

import org.junit.jupiter.api.Nested;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;

/**
 * Unit tests for the Revapi-based reflector. The full extraction path requires a JDK
 * (not just a JRE) at runtime because Revapi drives {@code javac}; that path is exercised
 * by the integration tests in {@link ExtractRunnerTest}. This file covers helpers that
 * don't need a live javac environment.
 */
class RevapiReflectorTest {

    @Nested
    class IsAnonymousClassBinaryName {
        @Test
        void plainTopLevelIsNotAnonymous() {
            assertFalse(RevapiReflector.isAnonymousClassBinaryName("com.example.Foo"));
        }

        @Test
        void namedNestedIsNotAnonymous() {
            assertFalse(RevapiReflector.isAnonymousClassBinaryName("com.example.Foo$Bar"));
            assertFalse(RevapiReflector.isAnonymousClassBinaryName("com.example.Foo$Bar$Baz"));
        }

        @Test
        void numericSegmentMarksAnonymous() {
            assertTrue(RevapiReflector.isAnonymousClassBinaryName("com.example.Foo$1"));
            assertTrue(RevapiReflector.isAnonymousClassBinaryName("com.example.Foo$2"));
        }

        @Test
        void numericSegmentInsideNestedChainMarksAnonymous() {
            assertTrue(RevapiReflector.isAnonymousClassBinaryName("com.example.Foo$Bar$1"));
            assertTrue(RevapiReflector.isAnonymousClassBinaryName("com.example.Foo$1$Inner"));
        }

        @Test
        void multiDigitNumericSegmentIsAnonymous() {
            assertTrue(RevapiReflector.isAnonymousClassBinaryName("com.example.Foo$42"));
        }

        @Test
        void packageOnlyIsNotAnonymous() {
            // No $ in the name — definitely not anonymous.
            assertFalse(RevapiReflector.isAnonymousClassBinaryName("com.example.foo.Bar"));
        }
    }
}

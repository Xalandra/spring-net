#region License

/*
 * Copyright � 2002-2005 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

#endregion

namespace Spring.Objects.Factory.Support
{
    /// <summary>
    /// The possible object scope values.
    /// </summary>
    /// <author>Aleksandar Seovic</author>
    public enum ObjectScope
    {
        /// <summary>
        /// Application scope.
        /// </summary>
        Application = 0,

        /// <summary>
        /// Session scope.
        /// </summary>
        Session = 1,

        /// <summary>
        /// Request scope.
        /// </summary>
        Request = 2,

        /// <summary>
        /// Default scope (currently
        /// <see cref="Spring.Objects.Factory.Support.ObjectScope.Application"/>).
        /// </summary>
        /// <seealso cref="Spring.Objects.Factory.Support.ObjectScope.Application"/>
        Default = Application
    }
}
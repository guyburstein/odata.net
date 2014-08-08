//   OData .NET Libraries ver. 5.6.2
//   Copyright (c) Microsoft Corporation. All rights reserved.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

namespace Microsoft.Data.OData
{
    #region Namespaces
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    #endregion Namespaces

    /// <summary>
    /// Class representing an annotation group in the JSON Light format.
    /// </summary>
    [DebuggerDisplay("{Name}")]
    internal sealed class ODataJsonLightAnnotationGroup
    {
        /// <summary>The name of the annotation group.</summary>
        private string name;

        /// <summary>The (instance and property) annotations included in this annotation group.</summary>
        private IDictionary<string, object> annotations;

        /// <summary>
        /// The name of the annotation group.
        /// </summary>
        /// <remarks>The name has to be unique across the whole JSON Light payload.</remarks>
        public string Name
        {
            get
            {
                DebugUtils.CheckNoExternalCallers();
                return this.name;
            }

            set
            {
                DebugUtils.CheckNoExternalCallers();
                this.name = value;
            }
        }

        /// <summary>
        /// The (instance and property) annotations included in this annotation group.
        /// </summary>
        /// <remarks>The keys in the dictionary are the names of the annotations, the values are their values.</remarks>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "We allow setting of all properties on public ODataLib OM classes.")]
        public IDictionary<string, object> Annotations
        {
            get 
            {
                DebugUtils.CheckNoExternalCallers();
                return this.annotations; 
            }

            set 
            {
                DebugUtils.CheckNoExternalCallers();
                this.annotations = value; 
            }
        }
    }
}
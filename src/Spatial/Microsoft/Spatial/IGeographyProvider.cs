//   OData .NET Libraries
//   Copyright (c) Microsoft Corporation. All rights reserved.  
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

namespace Microsoft.Spatial
{
    using System;

    /// <summary>Provides access to the geography objects that this object constructs.</summary>
    public interface IGeographyProvider
    {
        /// <summary>Fires when the provider constructs a geography object.</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly", Justification = "Not following the event-handler pattern")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "Not following the event-handler pattern")]
        event Action<Geography> ProduceGeography;

        /// <summary>Gets the geography object that was constructed most recently.</summary>
        /// <returns>The geography object that was constructed.</returns>
        Geography ConstructedGeography { get; }
    }
}

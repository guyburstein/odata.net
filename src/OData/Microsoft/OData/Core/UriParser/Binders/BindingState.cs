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

namespace Microsoft.OData.Core.UriParser.Parsers
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.OData.Edm;
    using Microsoft.OData.Core.UriParser.Semantic;
    using Microsoft.OData.Core.UriParser.Syntactic;
    using ODataErrorStrings = Microsoft.OData.Core.Strings;

    /// <summary>
    /// Encapsulates the state of metadata binding.
    /// TODO: finish moving fields from MetadataBinder here and see if anything can be removed.
    /// </summary>
    internal sealed class BindingState
    {
        /// <summary>
        /// The configuration used for binding.
        /// </summary>
        private readonly ODataUriParserConfiguration configuration;

        /// <summary>
        /// The dictionary used to store mappings between Any visitor and corresponding segment paths
        /// </summary>
        private readonly Stack<RangeVariable> rangeVariables = new Stack<RangeVariable>();

        /// <summary>
        /// If there is a  $filter or $orderby, then this member holds the reference to the parameter node for the 
        /// implicit parameter ($it) for all expressions.
        /// </summary>
        private RangeVariable implicitRangeVariable;

        /// <summary>
        /// The current recursion depth of binding.
        /// </summary>
        private int BindingRecursionDepth;

        /// <summary>
        /// Collection of query option tokens associated with the currect query being processed.
        /// If a given query option is bound it should be removed from this collection.
        /// </summary>
        private List<CustomQueryOptionToken> queryOptions;

        /// <summary>
        /// Constructs a <see cref="BindingState"/> with the given <paramref name="configuration"/>.
        /// </summary>
        /// <param name="configuration">The configuration used for binding.</param>
        internal BindingState(ODataUriParserConfiguration configuration)
        {
            ExceptionUtils.CheckArgumentNotNull(configuration, "configuration");
            this.configuration = configuration;
            this.BindingRecursionDepth = 0;
        }

        /// <summary>
        /// The model used for binding.
        /// </summary>
        internal IEdmModel Model
        {
            get
            {
                return this.configuration.Model;
            }
        }

        /// <summary>
        /// The configuration used for binding.
        /// </summary>
        internal ODataUriParserConfiguration Configuration
        {
            get
            {
                return this.configuration;
            }
        }

        /// <summary>
        /// If there is a  $filter or $orderby, then this member holds the reference to the parameter node for the 
        /// implicit parameter ($it) for all expressions.
        /// </summary>
        internal RangeVariable ImplicitRangeVariable
        {
            get
            {
                return this.implicitRangeVariable;
            }

            set
            {
                Debug.Assert(this.implicitRangeVariable == null || value == null, "This should only get set once when first starting to bind a tree.");
                this.implicitRangeVariable = value;
            }
        }

        /// <summary>
        /// The dictionary used to store mappings between Any visitor and corresponding segment paths
        /// </summary>
        internal Stack<RangeVariable> RangeVariables
        {
            get
            {
                return this.rangeVariables;
            }
        }

        /// <summary>
        /// Collection of query option tokens associated with the currect query being processed.
        /// If a given query option is bound it should be removed from this collection.
        /// </summary>
        internal List<CustomQueryOptionToken> QueryOptions
        {
            get
            {
                return this.queryOptions;
            }

            set
            {
                this.queryOptions = value;
            }
        }

        /// <summary>
        /// Marks the fact that a recursive method was entered, and checks that the depth is allowed.
        /// </summary>
        internal void RecurseEnter()
        {
            this.BindingRecursionDepth++;

            // TODO: add BindingLimit, use uniform error message
            if (this.BindingRecursionDepth > this.configuration.Settings.FilterLimit)
            {
                throw new ODataException(ODataErrorStrings.UriQueryExpressionParser_TooDeep);
            }
        }

        /// <summary>
        /// Marks the fact that a recursive method is leaving.
        /// </summary>
        internal void RecurseLeave()
        {
            Debug.Assert(this.BindingRecursionDepth > 0, "Decreasing recursion depth below zero, imbalanced recursion calls.");

            this.BindingRecursionDepth--;
        }
    }
}

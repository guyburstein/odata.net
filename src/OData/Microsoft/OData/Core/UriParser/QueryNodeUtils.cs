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

namespace Microsoft.OData.Core.UriParser
{
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.OData.Core.UriParser.TreeNodeKinds;
    using Microsoft.OData.Edm;
    using Microsoft.OData.Edm.Library;
    using Microsoft.OData.Core.Metadata;
    using Microsoft.OData.Core.UriParser.Semantic;
    using ODataErrorStrings = Microsoft.OData.Core.Strings;
    #endregion Namespaces

    /// <summary>
    /// Helper methods for working with query nodes.
    /// </summary>
    internal static class QueryNodeUtils
    {
        /// <summary>
        /// Pre-look map for BinaryOperator kind resolver
        /// </summary>
        private static Dictionary<Tuple<BinaryOperatorKind, EdmPrimitiveTypeKind, EdmPrimitiveTypeKind>, EdmPrimitiveTypeKind> additionalMap =
            new Dictionary<Tuple<BinaryOperatorKind, EdmPrimitiveTypeKind, EdmPrimitiveTypeKind>, EdmPrimitiveTypeKind>(EqualityComparer<Tuple<BinaryOperatorKind, EdmPrimitiveTypeKind, EdmPrimitiveTypeKind>>.Default)
                {
                    {
                        new Tuple<BinaryOperatorKind, EdmPrimitiveTypeKind, EdmPrimitiveTypeKind>(
                            BinaryOperatorKind.Add,
                            EdmPrimitiveTypeKind.DateTimeOffset,
                            EdmPrimitiveTypeKind.Duration),
                        EdmPrimitiveTypeKind.DateTimeOffset
                    },
                    {
                        new Tuple<BinaryOperatorKind, EdmPrimitiveTypeKind, EdmPrimitiveTypeKind>(
                            BinaryOperatorKind.Add,
                            EdmPrimitiveTypeKind.Duration,
                            EdmPrimitiveTypeKind.DateTimeOffset),
                        EdmPrimitiveTypeKind.DateTimeOffset
                    },
                    {
                        new Tuple<BinaryOperatorKind, EdmPrimitiveTypeKind, EdmPrimitiveTypeKind>(
                            BinaryOperatorKind.Subtract,
                            EdmPrimitiveTypeKind.DateTimeOffset,
                            EdmPrimitiveTypeKind.Duration),
                        EdmPrimitiveTypeKind.DateTimeOffset
                    },
                     {
                        new Tuple<BinaryOperatorKind, EdmPrimitiveTypeKind, EdmPrimitiveTypeKind>(
                            BinaryOperatorKind.Subtract,
                            EdmPrimitiveTypeKind.DateTimeOffset,
                            EdmPrimitiveTypeKind.DateTimeOffset),
                        EdmPrimitiveTypeKind.Duration
                    },
                };

        /// <summary>
        /// Compute the result type of a binary operator based on the type of its operands and the operator kind.
        /// </summary>
        /// <param name="left">The type reference on the left hand.</param>
        /// <param name="right">The type reference on the right hand.</param>
        /// <param name="operatorKind">The kind of operator.</param>
        /// <returns>The result type reference of the binary operator.</returns>
        internal static IEdmPrimitiveTypeReference GetBinaryOperatorResultType(IEdmPrimitiveTypeReference left, IEdmPrimitiveTypeReference right, BinaryOperatorKind operatorKind)
        {
            Debug.Assert(left != null, "type != null");
            Debug.Assert(right != null, "type != null");

            EdmPrimitiveTypeKind kind;
            if (additionalMap.TryGetValue(new Tuple<BinaryOperatorKind, EdmPrimitiveTypeKind, EdmPrimitiveTypeKind>(operatorKind, left.PrimitiveKind(), right.PrimitiveKind()), out kind))
            {
                return EdmCoreModel.Instance.GetPrimitive(kind, left.IsNullable);
            }

            switch (operatorKind)
            {
                case BinaryOperatorKind.Or:                 // fall through
                case BinaryOperatorKind.And:                // fall through
                case BinaryOperatorKind.Equal:              // fall through
                case BinaryOperatorKind.NotEqual:           // fall through
                case BinaryOperatorKind.GreaterThan:        // fall through
                case BinaryOperatorKind.GreaterThanOrEqual: // fall through
                case BinaryOperatorKind.LessThan:           // fall through
                case BinaryOperatorKind.LessThanOrEqual:
                case BinaryOperatorKind.Has:
                    return EdmCoreModel.Instance.GetBoolean(left.IsNullable);

                case BinaryOperatorKind.Add:        // fall through
                case BinaryOperatorKind.Subtract:   // fall through
                case BinaryOperatorKind.Multiply:   // fall through
                case BinaryOperatorKind.Divide:     // fall through
                case BinaryOperatorKind.Modulo:
                    return left;

                default:
                    throw new ODataException(ODataErrorStrings.General_InternalError(InternalErrorCodes.QueryNodeUtils_BinaryOperatorResultType_UnreachableCodepath));
            }
        }

#if ORCAS
        /// <summary>
        /// The tuple for .NET35
        /// </summary>
        /// <typeparam name="T1">First component type.</typeparam>
        /// <typeparam name="T2">Second component type.</typeparam>
        /// <typeparam name="T3">Third component type.</typeparam>
        private class Tuple<T1, T2, T3>
        {
            /// <summary>
            ///  Initializes a new instance of this class.
            /// </summary>
            /// <param name="first">The value of the tuple's first component.</param>
            /// <param name="second">The value of the tuple's second component.</param>
            /// <param name="third">The value of the tuple's third component.</param>
            internal Tuple(T1 first, T2 second, T3 third)
            {
                First = first;
                Second = second;
                Third = third;
            }

            /// <summary>
            /// The value of the tuple's first component.
            /// </summary>
            public T1 First { get; private set; }

            /// <summary>
            /// The value of the tuple's second component.
            /// </summary>
            public T2 Second { get; private set; }

            /// <summary>
            /// The value of the tuple's third component.
            /// </summary>
            public T3 Third { get; private set; }
        }
#endif
    }
}

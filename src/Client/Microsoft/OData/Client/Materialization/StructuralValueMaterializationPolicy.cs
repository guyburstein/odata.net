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

namespace Microsoft.OData.Client.Materialization
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Microsoft.OData.Client;
    using Microsoft.OData.Client.Metadata;
    using Microsoft.OData.Core;
    using Microsoft.OData.Edm;
    using DSClient = Microsoft.OData.Client;

    /// <summary>
    /// Contains logic on how to materialize properties into an instance
    /// </summary>
    internal abstract class StructuralValueMaterializationPolicy : MaterializationPolicy
    {
        /// <summary> The collection value materialization policy. </summary>
        private CollectionValueMaterializationPolicy collectionValueMaterializationPolicy;

        /// <summary> The primitive property converter. </summary>
        private DSClient.SimpleLazy<PrimitivePropertyConverter> lazyPrimitivePropertyConverter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexValueMaterializationPolicy" /> class.
        /// </summary>
        /// <param name="materializerContext">The materializer context.</param>
        /// <param name="lazyPrimitivePropertyConverter">The lazy primitive property converter.</param>
        protected StructuralValueMaterializationPolicy(IODataMaterializerContext materializerContext, DSClient.SimpleLazy<PrimitivePropertyConverter> lazyPrimitivePropertyConverter)
        {
            Debug.Assert(materializerContext != null, "materializer!=null");
            this.MaterializerContext = materializerContext;
            this.lazyPrimitivePropertyConverter = lazyPrimitivePropertyConverter;
        }

        /// <summary>
        /// Gets the collection value materialization policy.
        /// </summary>
        /// <value>
        /// The collection value materialization policy.
        /// </value>
        protected internal CollectionValueMaterializationPolicy CollectionValueMaterializationPolicy
        {
            get
            {
                Debug.Assert(this.collectionValueMaterializationPolicy != null, "collectionValueMaterializationPolicy != null");
                return this.collectionValueMaterializationPolicy;
            }

            set
            {
                this.collectionValueMaterializationPolicy = value;
            }
        }

        /// <summary>
        /// Gets the primitive property converter.
        /// </summary>
        /// <value>
        /// The primitive property converter.
        /// </value>
        protected PrimitivePropertyConverter PrimitivePropertyConverter
        {
            get
            {
                Debug.Assert(this.lazyPrimitivePropertyConverter != null, "lazyPrimitivePropertyConverter != null");
                return this.lazyPrimitivePropertyConverter.Value;
            }
        }

        /// <summary>
        /// Gets the materializer context.
        /// </summary>
        /// <value>
        /// The materializer context.
        /// </value>
        protected IODataMaterializerContext MaterializerContext { get; private set; }

        /// <summary>Materializes a primitive value. No op for non-primitive values.</summary>
        /// <param name="type">Type of value to set.</param>
        /// <param name="property">Property holding value.</param>
        internal void MaterializePrimitiveDataValue(Type type, ODataProperty property)
        {
            Debug.Assert(type != null, "type != null");
            Debug.Assert(property != null, "atomProperty != null");

            if (!property.HasMaterializedValue())
            {
                object value = property.Value;
                object materializedValue = this.PrimitivePropertyConverter.ConvertPrimitiveValue(value, type);
                property.SetMaterializedValue(materializedValue);
            }
        }

        /// <summary>
        /// Applies the values of the specified <paramref name="properties"/> to a given <paramref name="instance"/>.
        /// </summary>
        /// <param name="type">Type to which properties will be applied.</param>
        /// <param name="properties">Properties to assign to the specified <paramref name="instance"/>.</param>
        /// <param name="instance">Instance on which values will be applied.</param>
        internal void ApplyDataValues(ClientTypeAnnotation type, IEnumerable<ODataProperty> properties, object instance)
        {
            Debug.Assert(type != null, "type != null");
            Debug.Assert(properties != null, "properties != null");
            Debug.Assert(instance != null, "instance != context");

            foreach (var p in properties)
            {
                this.ApplyDataValue(type, p, instance);
            }
        }

        /// <summary>Applies a data value to the specified <paramref name="instance"/>.</summary>
        /// <param name="type">Type to which a property value will be applied.</param>
        /// <param name="property">Property with value to apply.</param>
        /// <param name="instance">Instance on which value will be applied.</param>
        internal void ApplyDataValue(ClientTypeAnnotation type, ODataProperty property, object instance)
        {
            Debug.Assert(type != null, "type != null");
            Debug.Assert(property != null, "property != null");
            Debug.Assert(instance != null, "instance != null");

            var prop = type.GetProperty(property.Name, this.MaterializerContext.IgnoreMissingProperties);
            if (prop == null)
            {
                return;
            }

            // Is it a collection? (note: property.Properties will be null if the Collection is empty (contains no elements))
            Type enumTypeTmp = null;
            if (prop.IsPrimitiveOrEnumOrComplexCollection)
            {
                // Collections must not be null
                if (property.Value == null)
                {
                    throw DSClient.Error.InvalidOperation(DSClient.Strings.Collection_NullCollectionNotSupported(property.Name));
                }

                // This happens if the payload contain just primitive value for a Collection property
                if (property.Value is string)
                {
                    throw DSClient.Error.InvalidOperation(DSClient.Strings.Deserialize_MixedTextWithComment);
                }

                if (property.Value is ODataComplexValue)
                {
                    throw DSClient.Error.InvalidOperation(DSClient.Strings.AtomMaterializer_InvalidCollectionItem(property.Name));
                }

                // ODataLib already parsed the data and materialized all the primitive types. There is nothing more to materialize
                // anymore. Only complex type instance and collection instances need to be materialized, but those will be
                // materialized later on.
                // We need to materialize items before we change collectionInstance since this may throw. If we tried materializing
                // after the Collection is wiped or created we would leave the object in half constructed state.
                object collectionInstance = prop.GetValue(instance);
                if (collectionInstance == null)
                {
                    collectionInstance = this.CollectionValueMaterializationPolicy.CreateCollectionPropertyInstance(property, prop.PropertyType);

                    // allowAdd is false - we need to assign instance as the new property value
                    prop.SetValue(instance, collectionInstance, property.Name, false /* allowAdd? */);
                }
                else
                {
                    // Clear existing Collection
                    prop.ClearBackingICollectionInstance(collectionInstance);
                }

                bool isElementNullable = prop.EdmProperty.Type.AsCollection().ElementType().IsNullable;
                this.CollectionValueMaterializationPolicy.ApplyCollectionDataValues(
                    property,
                    collectionInstance,
                    prop.PrimitiveOrComplexCollectionItemType,
                    prop.AddValueToBackingICollectionInstance,
                    isElementNullable);
            }
            else if ((enumTypeTmp = Nullable.GetUnderlyingType(prop.NullablePropertyType) ?? prop.NullablePropertyType) != null
                && enumTypeTmp.IsEnum())
            {
                ODataEnumValue enumValue = property.Value as ODataEnumValue;
                object tmpValue = EnumValueMaterializationPolicy.MaterializeODataEnumValue(enumTypeTmp, enumValue);

                // TODO: 1. use EnumValueMaterializationPolicy 2. handle nullable enum property
                prop.SetValue(instance, tmpValue, property.Name, false /* allowAdd? */);
            }
            else
            {
                object propertyValue = property.Value;
                ODataComplexValue complexValue = propertyValue as ODataComplexValue;
                if (propertyValue != null && complexValue != null)
                {
                    if (!prop.EdmProperty.Type.IsComplex())
                    {
                        // The error message is a bit odd, but it's compatible with V1.
                        throw DSClient.Error.InvalidOperation(DSClient.Strings.Deserialize_ExpectingSimpleValue);
                    }

                    // Complex type.
                    bool needToSet = false;

                    ClientEdmModel edmModel = this.MaterializerContext.Model;
                    ClientTypeAnnotation complexType = edmModel.GetClientTypeAnnotation(edmModel.GetOrCreateEdmType(prop.PropertyType));
                    object complexInstance = prop.GetValue(instance);

                    // Validating property inheritance in complexvalue and instance
                    if (prop.PropertyType.Name != property.Name)
                    {
                        complexType = this.MaterializerContext.ResolveTypeForMaterialization(prop.PropertyType, complexValue.TypeName);

                        // recreate complexInstance with derived type
                        complexInstance = null;
                    }

                    if (complexValue.Properties.Any() || complexInstance == null)
                    {
                        complexInstance = this.CreateNewInstance(complexType.EdmTypeReference, complexType.ElementType);
                        needToSet = true;
                    }

                    this.MaterializeDataValues(complexType, complexValue.Properties, this.MaterializerContext.IgnoreMissingProperties);
                    this.ApplyDataValues(complexType, complexValue.Properties, complexInstance);

                    if (needToSet)
                    {
                        prop.SetValue(instance, complexInstance, property.Name, true /* allowAdd? */);
                    }
                }
                else
                {
                    this.MaterializePrimitiveDataValue(prop.NullablePropertyType, property);
                    prop.SetValue(instance, property.GetMaterializedValue(), property.Name, true /* allowAdd? */);
                }
            }
        }

        /// <summary>
        /// Materializes the primitive data values in the given list of <paramref name="values"/>.
        /// </summary>
        /// <param name="actualType">Actual type for properties being materialized.</param>
        /// <param name="values">List of values to materialize.</param>
        /// <param name="ignoreMissingProperties">
        /// Whether properties missing from the client types should be ignored.
        /// </param>
        /// <remarks>
        /// Values are materialized in-place withi each <see cref="ODataProperty"/>
        /// instance.
        /// </remarks>
        internal void MaterializeDataValues(ClientTypeAnnotation actualType, IEnumerable<ODataProperty> values, bool ignoreMissingProperties)
        {
            Debug.Assert(actualType != null, "actualType != null");
            Debug.Assert(values != null, "values != null");

            foreach (var odataProperty in values)
            {
                if (odataProperty.Value is ODataStreamReferenceValue)
                {
                    continue;
                }

                string propertyName = odataProperty.Name;

                var property = actualType.GetProperty(propertyName, ignoreMissingProperties); // may throw
                if (property == null)
                {
                    continue;
                }

                // we should throw if the property type on the client does not match with the property type in the server
                if (ClientTypeUtil.TypeOrElementTypeIsEntity(property.PropertyType))
                {
                    throw DSClient.Error.InvalidOperation(DSClient.Strings.AtomMaterializer_InvalidEntityType(property.EntityCollectionItemType ?? property.PropertyType));
                }

                if (property.IsKnownType)
                {
                    // Note: MaterializeDataValue materializes only properties of primitive types. Materialization specific 
                    // to complex types and collections is done later. 
                    this.MaterializePrimitiveDataValue(property.NullablePropertyType, odataProperty);
                }
            }
        }
    }
}

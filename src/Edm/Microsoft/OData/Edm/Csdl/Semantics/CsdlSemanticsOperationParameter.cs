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

using System;
using System.Collections.Generic;
using Microsoft.OData.Edm.Annotations;
using Microsoft.OData.Edm.Csdl.Parsing.Ast;

namespace Microsoft.OData.Edm.Csdl.CsdlSemantics
{
    /// <summary>
    /// Provides semantics for a CsdlOperationParameter.
    /// </summary>
    internal class CsdlSemanticsOperationParameter : CsdlSemanticsElement, IEdmOperationParameter
    {
        private readonly CsdlSemanticsOperation declaringOperation;
        private readonly CsdlOperationParameter parameter;

        private readonly Cache<CsdlSemanticsOperationParameter, IEdmTypeReference> typeCache = new Cache<CsdlSemanticsOperationParameter, IEdmTypeReference>();
        private static readonly Func<CsdlSemanticsOperationParameter, IEdmTypeReference> ComputeTypeFunc = (me) => me.ComputeType();

        public CsdlSemanticsOperationParameter(CsdlSemanticsOperation declaringOperation, CsdlOperationParameter parameter)
            : base(parameter)
        {
            this.parameter = parameter;
            this.declaringOperation = declaringOperation;
        }

        public override CsdlSemanticsModel Model
        {
            get { return this.declaringOperation.Model; }
        }

        public override CsdlElement Element
        {
            get { return this.parameter; }
        }

        public IEdmTypeReference Type
        {
            get { return this.typeCache.GetValue(this, ComputeTypeFunc, null); }
        }
  
        public string Name
        {
            get { return this.parameter.Name; }
        }

        public IEdmOperation DeclaringOperation
        {
            get { return this.declaringOperation; }
        }

        protected override IEnumerable<IEdmVocabularyAnnotation> ComputeInlineVocabularyAnnotations()
        {
            return this.Model.WrapInlineVocabularyAnnotations(this, this.declaringOperation.Context);
        }

        private IEdmTypeReference ComputeType()
        {
            return CsdlSemanticsModel.WrapTypeReference(this.declaringOperation.Context, this.parameter.Type);
        }
    }
}

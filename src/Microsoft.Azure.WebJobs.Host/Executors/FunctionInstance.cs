﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Microsoft.Azure.WebJobs.Host.Protocols;

namespace Microsoft.Azure.WebJobs.Host.Executors
{
    internal class FunctionInstance : IFunctionInstance
    {
        private readonly Guid _id;
        private readonly Guid? _parentId;
        private readonly ExecutionReason _reason;
        private readonly IBindingSource _bindingSource;
        private readonly IFunctionInvoker _invoker;
        private readonly FunctionDescriptor _functionDescriptor;

        public FunctionInstance(Guid id, Guid? parentId, ExecutionReason reason, IBindingSource bindingSource,
            IFunctionInvoker invoker, FunctionDescriptor functionDescriptor)
        {
            _id = id;
            _parentId = parentId;
            _reason = reason;
            _bindingSource = bindingSource;
            _invoker = invoker;
            _functionDescriptor = functionDescriptor;
        }

        public Guid Id
        {
            get { return _id; }
        }

        public Guid? ParentId
        {
            get { return _parentId; }
        }

        public ExecutionReason Reason
        {
            get { return _reason; }
        }

        public IBindingSource BindingSource
        {
            get { return _bindingSource; }
        }

        public IFunctionInvoker Invoker
        {
            get { return _invoker; }
        }

        public FunctionDescriptor FunctionDescriptor
        {
            get { return _functionDescriptor; }
        }
    }
}

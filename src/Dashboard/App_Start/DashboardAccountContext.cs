﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using Dashboard.Data;
using Microsoft.WindowsAzure.Storage;

namespace Dashboard
{
    public class DashboardAccountContext
    {
        public const string ConnectionStringName = ConnectionStringNames.Dashboard;
        public static readonly string PrefixedConnectionStringName = 
            ConnectionStringProvider.GetPrefixedConnectionStringName(ConnectionStringName);

        public DashboardAccountContext()
        {
            ConnectionStringState = ConnectionStringState.Unknown;
            StorageAccount = null;
        }

        public ConnectionStringState ConnectionStringState { get; internal set; }
        
        public bool HasSetupError
        {
            get { return ConnectionStringState != ConnectionStringState.Valid; }
        }

        public string SdkStorageAccountName { get; internal set; }
        
        public CloudStorageAccount StorageAccount { get; internal set; }

        // True for hosts that don't support invoke from the dashboard. 
        public bool DisableInvoke { get; set; }
    }
}
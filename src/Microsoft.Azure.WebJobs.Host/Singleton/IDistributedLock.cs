// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Host
{
    /// <summary>
    /// Handle for a lock returned by <see cref="IDistributedLockManager"/>
    /// The SDK will call call <see cref="IDistributedLockManager.RenewAsync(IDistributedLock, CancellationToken)"/> to renew the release.
    /// </summary>
    public interface IDistributedLock
    {
        /// <summary>
        /// The Lock identity.  
        /// </summary>
        string LockId { get; }
    }
}
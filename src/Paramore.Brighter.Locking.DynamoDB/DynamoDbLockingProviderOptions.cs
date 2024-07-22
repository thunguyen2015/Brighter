﻿#region Licence
/* The MIT License (MIT)
Copyright © 2024 Dominic Hickie <dominichickie@gmail.com>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the “Software”), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE. */
#endregion
namespace Paramore.Brighter.Locking.DynamoDb
{
    public class DynamoDbLockingProviderOptions(string lockTableName, string leaseholderGroupId)
    {
        /// <summary>
        /// The name of the dynamo DB table containing locks
        /// </summary>
        public string LockTableName { get; init; } = lockTableName;

        /// <summary>
        /// The ID of the group of potential leaseholders that share the lock
        /// </summary>
        public string LeaseholderGroupId { get; init; } = leaseholderGroupId;

        /// <summary>
        /// The amount of time before the lease automatically expires
        /// </summary>
        public TimeSpan LeaseValidity { get; set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Whether the lock provider should manually release the lock on completion or simply wait for expiry
        /// </summary>
        public bool ManuallyReleaseLock { get; set; } = false;
    }
}

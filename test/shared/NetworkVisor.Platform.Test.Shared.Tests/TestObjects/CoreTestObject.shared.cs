// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// // ***********************************************************************
// <copyright file="CoreTestObject.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using NetworkVisor.Core.CoreObject;
using NetworkVisor.Core.Logging.Interfaces;

namespace NetworkVisor.Platform.Test.TestObjects
{
    /// <summary>
    /// Class ObjectTest.
    /// </summary>
    public class CoreTestObject : CoreObjectBase, ICoreTestObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTestObject"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public CoreTestObject(ICoreLogger? logger)
        : base(logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTestObject"/> class.
        /// </summary>
        public CoreTestObject()
            : this(null)
        {
        }

        protected override void SetObjectId(Guid newObjectID, bool updateObjectVersion = true)
        {
            base.SetObjectId(newObjectID, false);
        }

        protected override void SetCreatedTimestamp(DateTimeOffset newCreatedTimestamp, bool updateObjectVersion = true)
        {
            base.SetCreatedTimestamp(newCreatedTimestamp, false);
        }

        protected override void SetModifiedTimestamp(DateTimeOffset newModifiedTimestamp, bool updateObjectVersion = true)
        {
            base.SetModifiedTimestamp(newModifiedTimestamp, false);
        }

        protected override void SetObjectVersion(ulong newObjectVersion, bool updateObjectVersion = true)
        {
            base.SetObjectVersion(newObjectVersion, false);
        }
    }
}

// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// ***********************************************************************
// <copyright file="CoreConnectionEntityIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ***********************************************************************
// <summary>
// </summary>

using System.Net;
using FluentAssertions;
using NetworkVisor.Core.Entities;
using NetworkVisor.Core.Entities.Base;
using NetworkVisor.Core.Entities.Connections;
using NetworkVisor.Core.Entities.Constants;
using NetworkVisor.Core.Entities.Extensions;
using NetworkVisor.Core.Networking.NetworkInterface;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Messaging.Shared.IntegrationTests.Entities
{
    /// <summary>
    /// Class CoreConnectionEntityIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreConnectionEntityIntegrationTests))]
    public class CoreConnectionEntityIntegrationTests : CoreEntityTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreConnectionEntityIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public CoreConnectionEntityIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public async Task ConnectionEntityIntegration_InsertConnectionEntityAsync()
        {
            var connectionEntity = new CoreConnectionEntity();
            this.ValidateAndOutputEntity<CoreConnectionEntity>(connectionEntity, CoreEntityType.ConnectionV1, CoreEntityConstants.DefaultConnectionWeight, CoreConnectionEntity.CreateLookupKey(CoreConnectionEntityType.UnknownConnection, Guid.Empty, Guid.Empty));
            (await this.TestEntityDatabase.InsertConnectionEntityAsync(connectionEntity)).Should().Be(1);
            (await this.TestEntityDatabase.SQLiteAsyncConnection.Table<CoreConnectionEntity>().CountAsync()).Should().Be(1);
        }
    }
}

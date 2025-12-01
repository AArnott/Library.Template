// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// ***********************************************************************
// <copyright file="CoreDeviceEntityUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ***********************************************************************
// <summary>
// </summary>

using FluentAssertions;
using NetworkVisor.Core.Entities;
using NetworkVisor.Core.Entities.Base;
using NetworkVisor.Core.Entities.Devices;
using NetworkVisor.Core.Entities.Devices.Base;
using NetworkVisor.Core.Entities.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Messaging.Shared.UnitTests.Entities
{
    /// <summary>
    /// Class CoreDeviceEntityUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreDeviceEntityUnitTests))]
    public class CoreDeviceEntityUnitTests : CoreEntityTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreDeviceEntityUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public CoreDeviceEntityUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreDeviceEntityUnitTests_Default()
        {
            var deviceEntity = new CoreDeviceEntity();
            this.ValidateAndOutputEntity<CoreDeviceEntity>(deviceEntity, CoreEntityType.DeviceV1, CoreDeviceEntityConstants.DefaultDeviceScore);

            // Device specific validations
            _ = deviceEntity.DeviceEntityType.Should().Be(CoreDeviceEntityType.GenericDevice);
        }

        [Fact]
        public void CoreDeviceEntityUnitTests_Default_Same()
        {
            var deviceEntity = new CoreDeviceEntity();
            this.ValidateAndOutputEntity<CoreDeviceEntity>(deviceEntity, CoreEntityType.DeviceV1, CoreDeviceEntityConstants.DefaultDeviceScore);
            var deviceEntity2 = new CoreDeviceEntity(deviceEntity, deviceEntity.DeviceEntityType);
            _ = deviceEntity.EntityID.Should().Be(deviceEntity2.EntityID);
            _ = deviceEntity.DisplayName.Should().Be(deviceEntity2.DisplayName);
            _ = deviceEntity.EntityType.Should().Be(deviceEntity2.EntityType);
            _ = deviceEntity.EntityOwnerID.Should().Be(deviceEntity2.EntityOwnerID);

            _ = deviceEntity.TimeToLive.Should().Be(deviceEntity2.TimeToLive);
            _ = deviceEntity.Score.Should().Be(deviceEntity2.Score);

            // Set CreatedUtc and ModifiedUtc to the same value
            // Records should generally not be modified after creation
            deviceEntity2.CreatedUtc = deviceEntity.CreatedUtc;
            deviceEntity2.ModifiedUtc = deviceEntity.ModifiedUtc;
            _ = deviceEntity.Should().BeEquivalentTo(deviceEntity2);

            _ = deviceEntity.Entity.Should().Be(deviceEntity2.Entity);
        }

        [Fact]
        public void CoreDeviceEntityUnitTests_ChangeDeviceEntityType()
        {
            var deviceEntity = new CoreDeviceEntity();
            var deviceEntity2 = new CoreDeviceEntity(deviceEntity, CoreDeviceEntityType.NetworkDevice);
            this.ValidateAndOutputEntity<CoreDeviceEntity>(deviceEntity2, CoreEntityType.DeviceV1, CoreDeviceEntityConstants.DefaultDeviceScore);

            _ = deviceEntity.EntityID.Should().Be(deviceEntity2.EntityID);
            _ = deviceEntity.DisplayName.Should().Be(deviceEntity2.DisplayName);
            _ = deviceEntity.EntityType.Should().Be(deviceEntity2.EntityType);
            _ = deviceEntity.EntityOwnerID.Should().Be(deviceEntity2.EntityOwnerID);

            _ = deviceEntity.TimeToLive.Should().Be(deviceEntity2.TimeToLive);
            _ = deviceEntity.Score.Should().Be(deviceEntity2.Score);

            deviceEntity2.DeviceEntityType.Should().Be(CoreDeviceEntityType.NetworkDevice);
        }
    }
}

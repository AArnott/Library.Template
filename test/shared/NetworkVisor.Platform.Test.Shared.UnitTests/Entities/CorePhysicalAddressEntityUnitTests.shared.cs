// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// ***********************************************************************
// <copyright file="CorePhysicalAddressEntityUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ***********************************************************************
// <summary>
// </summary>

using System.Net.NetworkInformation;
using FluentAssertions;
using NetworkVisor.Core.CoreObject;
using NetworkVisor.Core.Entities;
using NetworkVisor.Core.Entities.Networks.Addresses;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Serialization;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;
using Xunit.Sdk;

[assembly: RegisterXunitSerializer(typeof(CoreXunitSerializer), typeof(CoreIPEndPoint), typeof(PhysicalAddress), typeof(CoreIPAddressSubnet), typeof(CoreObjectItem))]

namespace NetworkVisor.Platform.Test.Messaging.Shared.UnitTests.Entities
{
    /// <summary>
    /// Class CorePhysicalAddressEntityUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CorePhysicalAddressEntityUnitTests))]
    public class CorePhysicalAddressEntityUnitTests : CoreEntityTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CorePhysicalAddressEntityUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public CorePhysicalAddressEntityUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void PhysicalAddressEntityUnit_Default()
        {
            var physicalAddressEntity = new CorePhysicalAddressEntity();
            this.ValidateAndOutputEntity<CorePhysicalAddressEntity>(physicalAddressEntity, CoreEntityType.PhysicalAddressV1, CorePhysicalAddressConstants.NonePhysicalAddressScore, CorePhysicalAddressEntity.CreateLookupKey(physicalAddressEntity.PhysicalAddress));

            // PhysicalAddress specific validations
            _ = physicalAddressEntity.PhysicalAddress.Should().Be(PhysicalAddress.None);
        }

        [Fact]
        public void PhysicalAddressEntityUnit_Default_Same()
        {
            var physicalAddressEntity = new CorePhysicalAddressEntity(PhysicalAddressExtensions.TestPhysicalAddress);
            this.ValidateAndOutputEntity<CorePhysicalAddressEntity>(physicalAddressEntity, CoreEntityType.PhysicalAddressV1, CorePhysicalAddressConstants.TestPhysicalAddressScore, CorePhysicalAddressEntity.CreateLookupKey(physicalAddressEntity.PhysicalAddress));
            var physicalAddressEntity2 = new CorePhysicalAddressEntity(PhysicalAddressExtensions.TestPhysicalAddress);
            physicalAddressEntity.EntityID.Should().Be(physicalAddressEntity2.EntityID);
            physicalAddressEntity.DisplayName.Should().Be(physicalAddressEntity2.DisplayName);
            physicalAddressEntity.EntityType.Should().Be(physicalAddressEntity2.EntityType);
            physicalAddressEntity.EntityOwnerID.Should().Be(physicalAddressEntity2.EntityOwnerID);

            physicalAddressEntity.TimeToLive.Should().Be(physicalAddressEntity2.TimeToLive);
            physicalAddressEntity.Score.Should().Be(physicalAddressEntity2.Score);

            // Set CreatedUtc and ModifiedUtc to the same value
            // Records should generally not be modified after creation
            physicalAddressEntity2.CreatedUtc = physicalAddressEntity.CreatedUtc;
            physicalAddressEntity2.ModifiedUtc = physicalAddressEntity.ModifiedUtc;
            physicalAddressEntity.Should().BeEquivalentTo(physicalAddressEntity2);

            physicalAddressEntity.Entity.Should().Be(physicalAddressEntity2.Entity);
        }

        [Theory]
        [InlineData(null, "96dd196e-7708-6256-b6b6-42bbde6dddea", "00:00:00:00:00:00", CorePhysicalAddressConstants.NonePhysicalAddressScore)]
        [InlineData("00-00-00-00-00-00", "96dd196e-7708-6256-b6b6-42bbde6dddea", "00:00:00:00:00:00", CorePhysicalAddressConstants.NonePhysicalAddressScore)]
        [InlineData("02-00-00-00-00-00", "861b67e5-6eb7-ae5d-8803-42b629ffa423", "02:00:00:00:00:00", CorePhysicalAddressConstants.RestrictedPhysicalAddressScore)]
        [InlineData("FF-FF-FF-FF-FF-FF", "f3c17bb0-0648-e753-bc18-aeeef4202a26", "FF:FF:FF:FF:FF:FF", CorePhysicalAddressConstants.BroadcastPhysicalAddressScore)]
        [InlineData("50-0F-F5-26-B7-28", "2954b5a8-85d8-005a-8d60-f0acc9695e4f", "50:0F:F5:26:B7:28", 5769099194315309056)]
        public void PhysicalAddressEntityUnit_Parsing(string? physicalAddressString, string entityIDString, string lookupKey, ulong score)
        {
            PhysicalAddress physicalAddress = PhysicalAddressExtensions.NormalizedParse(physicalAddressString);
            var physicalAddressEntity = new CorePhysicalAddressEntity(physicalAddress);

            this.ValidateAndOutputEntity<CorePhysicalAddressEntity>(physicalAddressEntity, CoreEntityType.PhysicalAddressV1, score, lookupKey);

            physicalAddressEntity.PhysicalAddress.Should().Be(physicalAddress);
            physicalAddressEntity.EntityID.Should().Be(Guid.Parse(entityIDString));

            if (physicalAddress.IsNone())
            {
                physicalAddressEntity.IsBroadcast.Should().BeFalse();
                physicalAddressEntity.IsRestricted.Should().BeFalse();
                physicalAddressEntity.IsNone.Should().BeTrue();
                physicalAddressEntity.IsUnicast.Should().BeFalse();
                physicalAddressEntity.IsMulticast.Should().BeTrue();
                physicalAddressEntity.IsUniversal.Should().BeFalse();
                physicalAddressEntity.OUI.Should().BeEmpty();

                physicalAddressEntity.Score.Should().Be(CorePhysicalAddressConstants.NonePhysicalAddressScore);
                physicalAddressEntity.LookupKey.Should().Be(CorePhysicalAddressEntity.CreateLookupKey(PhysicalAddress.None));
                physicalAddressEntity.EntityID.Should().Be(CorePhysicalAddressConstants.NonePhysicalAddressEntityID);
            }

            if (physicalAddress.IsRestricted())
            {
                physicalAddressEntity.IsBroadcast.Should().BeFalse();
                physicalAddressEntity.IsRestricted.Should().BeTrue();
                physicalAddressEntity.IsNone.Should().BeFalse();
                physicalAddressEntity.IsUnicast.Should().BeTrue();
                physicalAddressEntity.IsMulticast.Should().BeFalse();
                physicalAddressEntity.IsUniversal.Should().BeFalse();
                physicalAddressEntity.OUI.Should().Be("02-00-00");

                physicalAddressEntity.Score.Should().Be(CorePhysicalAddressConstants.RestrictedPhysicalAddressScore);
                physicalAddressEntity.LookupKey.Should().Be(CorePhysicalAddressEntity.CreateLookupKey(PhysicalAddressExtensions.RestrictedPhysicalAddress));
                physicalAddressEntity.EntityID.Should().Be(CorePhysicalAddressConstants.RestrictedPhysicalAddressEntityID);
            }

            if (physicalAddress.IsBroadcastAddress())
            {
                physicalAddressEntity.IsBroadcast.Should().BeTrue();
                physicalAddressEntity.IsRestricted.Should().BeFalse();
                physicalAddressEntity.IsNone.Should().BeFalse();
                physicalAddressEntity.IsUnicast.Should().BeFalse();
                physicalAddressEntity.IsMulticast.Should().BeTrue();
                physicalAddressEntity.IsUniversal.Should().BeFalse();
                physicalAddressEntity.OUI.Should().Be("FF-FF-FF");

                physicalAddressEntity.Score.Should().Be(CorePhysicalAddressConstants.BroadcastPhysicalAddressScore);
                physicalAddressEntity.LookupKey.Should().Be(CorePhysicalAddressEntity.CreateLookupKey(PhysicalAddressExtensions.Broadcast));
                physicalAddressEntity.EntityID.Should().Be(CorePhysicalAddressConstants.BroadcastPhysicalAddressEntityID);
            }

            if (physicalAddress.Equals(PhysicalAddressExtensions.TestPhysicalAddress))
            {
                physicalAddressEntity.IsBroadcast.Should().BeFalse();
                physicalAddressEntity.IsRestricted.Should().BeFalse();
                physicalAddressEntity.IsNone.Should().BeFalse();
                physicalAddressEntity.IsUnicast.Should().BeTrue();
                physicalAddressEntity.IsMulticast.Should().BeFalse();
                physicalAddressEntity.IsUniversal.Should().BeTrue();
                physicalAddressEntity.OUI.Should().Be("50-0F-F5");

                physicalAddressEntity.Score.Should().Be(CorePhysicalAddressConstants.TestPhysicalAddressScore);
                physicalAddressEntity.LookupKey.Should().Be(CorePhysicalAddressEntity.CreateLookupKey(PhysicalAddressExtensions.TestPhysicalAddress));
                physicalAddressEntity.EntityID.Should().Be(CorePhysicalAddressConstants.TestPhysicalAddressEntityID);
            }
        }
    }
}

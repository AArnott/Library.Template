// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// ***********************************************************************
// <copyright file="CoreNetworkDeviceUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using FluentAssertions;
using NetworkVisor.Core.Device;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using NetworkVisor.Platform.Test.TestDevices;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Devices
{
    /// <summary>
    /// Class CoreNetworkDeviceUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreNetworkDeviceUnitTests))]

    public class CoreNetworkDeviceUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkDeviceUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNetworkDeviceUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void NetworkDeviceUnit_DeviceId_SameAs_ObjectId()
        {
            var testNetworkDevice = new CoreTestNetworkDevice<CoreNetworkDeviceUnitTests>(this.TestNetworkServices, this.TestNetworkServices.PreferredLocalNetworkAddress, CoreDeviceType.TestLocalNetworkDevice);
            testNetworkDevice.DeviceID.Should().Be(testNetworkDevice.ObjectId);
        }
    }
}

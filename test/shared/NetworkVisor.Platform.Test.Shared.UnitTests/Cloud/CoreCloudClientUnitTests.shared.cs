// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// ***********************************************************************
// <copyright file="CoreCloudClientUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************
using FluentAssertions;
using NetworkVisor.Core.Cloud.Client;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.Cloud.Client;
using NetworkVisor.Core.Networking.DeviceInfo;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Cloud
{
    /// <summary>
    /// Class CoreCloudClientUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreCloudClientUnitTests))]

    public class CoreCloudClientUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreCloudClientUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreCloudClientUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreCloudClientUnitTests_Ctor()
        {
            this.TestOutputHelper.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestOutputHelper>();
            this.TestNetworkingSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkingSystem>();
            this.TestNetworkingSystem.FileSystem.Should().BeSameAs(this.TestFileSystem);
            this.TestNetworkingSystem.Logger.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreLogger>();

            using var cloudClient = new CoreCloudClient(this.TestCaseServiceProvider, this.TestCaseLogger);

            cloudClient.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreCloudClient>();
            cloudClient.NetworkingSystem.Should().BeSameAs(this.TestNetworkingSystem);
            cloudClient.CloudDeviceInfo.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreCloudDeviceInfo>();
        }
    }
}

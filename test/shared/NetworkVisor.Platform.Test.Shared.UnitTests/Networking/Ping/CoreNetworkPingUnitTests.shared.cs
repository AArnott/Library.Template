// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-25-2020
// ***********************************************************************
// <copyright file="CoreNetworkPingUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Net;
using System.Net.NetworkInformation;
using FluentAssertions;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.Ping;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Networking.Ping
{
    /// <summary>
    /// Class CoreNetworkPingUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreNetworkPingUnitTests))]

    public class CoreNetworkPingUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkPingUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNetworkPingUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Tests defaults.
        /// </summary>
        [Fact]
        public void NetworkPingUnitTests_PingResult_Defaults()
        {
            CorePingResult pingResult = new CorePingResult();
            pingResult.Address.Should().Be(IPAddress.None);
            pingResult.ErrorMessage.Should().Be(string.Empty);
            pingResult.Options.Ttl.Should().Be(128);
            pingResult.Options.DontFragment.Should().BeFalse();
            pingResult.Options.Should().BeEquivalentTo(new PingOptions());
        }

        /// <summary>
        /// Tests null ping reply.
        /// </summary>
        [Fact]
        public void NetworkPingUnitTests_PingResult_Null()
        {
            Func<CorePingResult> fx = () => new CorePingResult(null!);
            fx.Should().Throw<ArgumentNullException>().WithParameterName("pingReply");
        }

        /// <summary>
        /// Test error message property.
        /// </summary>
        [Fact]
        public void NetworkPingUnitTests_PingResult_ErrorMessage()
        {
            CorePingResult pingResult = new CorePingResult();
            pingResult.ErrorMessage.Should().Be(string.Empty);
            pingResult.ErrorMessage = "Ping Error";
            pingResult.ErrorMessage.Should().Be("Ping Error");
        }

        /// <summary>
        /// Test error message property.
        /// </summary>
        [Fact]
        public void NetworkPingUnitTests_PingResult_IPAddress_IPStatus()
        {
            var ipAddress = IPAddress.Parse(CoreIPAddressExtensions.StringNonRoutable);
            CorePingResult pingResult = new CorePingResult(ipAddress, IPStatus.BadRoute);

            pingResult.Address.Should().Be(ipAddress);
            pingResult.Status.Should().Be(IPStatus.BadRoute);
            pingResult.ErrorMessage.Should().Be(string.Empty);
            pingResult.Options.Ttl.Should().Be(128);
            pingResult.Options.DontFragment.Should().BeFalse();
            pingResult.Options.Should().BeEquivalentTo(new PingOptions());
        }
    }
}

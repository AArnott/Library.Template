// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-25-2020
// ***********************************************************************
// <copyright file="CoreLocalNetworkCapabilitiesIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Net;
using FluentAssertions;
using NetworkVisor.Core.Networking.Services.LocalNetwork;
using NetworkVisor.Core.Networking.Sockets.Listeners;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Networking.NetworkingSystem;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Networking.LocalNetworkServices
{
    /// <summary>
    /// Class CoreLocalNetworkCapabilitiesIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreLocalNetworkCapabilitiesIntegrationTests))]

    public class CoreLocalNetworkCapabilitiesIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreLocalNetworkCapabilitiesIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreLocalNetworkCapabilitiesIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method LocalNetworkCapabilitiesIntegration_Output.
        /// </summary>
        [Fact]
        public void LocalNetworkCapabilitiesIntegration_Output()
        {
            this.TestOutputHelper.WriteLine($"Local Network Services: {this.TestNetworkingSystem.LocalNetworkCapabilities}");

            foreach (object? enumItem in Enum.GetValues(typeof(CoreLocalNetworkCapabilities))!)
            {
                if (enumItem is not CoreLocalNetworkCapabilities service)
                {
                    continue;
                }

                if (this.TestNetworkingSystem.LocalNetworkCapabilities.HasFlag(service!))
                {
                    this.TestOutputHelper.WriteLine($"    {service}");
                }

                this.TestNetworkingSystem.IsLocalNetworkCapabilitySupported(service).Should().Be(this.TestNetworkingSystem.LocalNetworkCapabilities.HasFlag(service));
            }
        }
    }
}

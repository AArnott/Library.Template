// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests.Networking
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// ***********************************************************************
// <copyright file="CoreNetworkUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Net;
using FluentAssertions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Networking
{
    /// <summary>
    /// Class NetworkUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreNetworkUnitTests))]

    public class CoreNetworkUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNetworkUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void Ctor()
        {
            this.TestOutputHelper.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestOutputHelper>();
        }

        /// <summary>
        /// Defines the test method NetworkEnumerator_Ctor.
        /// </summary>
        [Fact]
        public void NetworkEnumerator_Ctor()
        {
            var networkEnumerator = new CoreNetworkCollection(IPAddress.Parse("192.168.1.0").ToIPAddressSubnet(CoreIPAddressExtensions.SubnetClassC));
            int count = 0;

            foreach (IPAddress ipAddress in networkEnumerator)
            {
                this.TestOutputHelper.WriteLine($"IPAddress({count}):\t{ipAddress}");
                count++;
            }

            count.Should().Be(256);
        }
    }
}

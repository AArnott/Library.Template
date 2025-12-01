// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests.Networking
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// ***********************************************************************
// <copyright file="CoreHostEnvironmentUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************
using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.Hosting;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Networking.Extensions
{
    /// <summary>
    /// Class NetworkUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreHostEnvironmentUnitTests))]

    public class CoreHostEnvironmentUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreHostEnvironmentUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreHostEnvironmentUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Theory]
        [InlineData(CoreHostEnvironment.Default, "Unknown")]
        [InlineData(CoreHostEnvironment.Development, "Development")]
        [InlineData(CoreHostEnvironment.Testing, "Testing")]
        [InlineData(CoreHostEnvironment.Staging, "Staging")]
        [InlineData(CoreHostEnvironment.Production, "Production")]
        public void CoreHostEnvironment_ToCoreHostEnvironment(CoreHostEnvironment hostEnvironment, string expectedString)
        {
            expectedString.ToCoreHostEnvironment().Should().Be(hostEnvironment);

            var hostingEnvironment = new HostingEnvironment() { EnvironmentName = expectedString, };
            hostingEnvironment.ToCoreHostEnvironment().Should().Be(hostEnvironment);
        }

        [Fact]
        public void CoreHostEnvironment_Null()
        {
            Func<CoreHostEnvironment> fx = () => ((IHostEnvironment)null!).ToCoreHostEnvironment();
            fx.Should().Throw<ArgumentNullException>();
        }
    }
}

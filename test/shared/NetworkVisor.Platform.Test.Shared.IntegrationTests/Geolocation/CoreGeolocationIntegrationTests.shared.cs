// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreGeolocationIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************
using FluentAssertions;
using NetworkVisor.Core.Geolocation;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Geolocation
{
    /// <summary>
    /// Class CoreGeolocationIntegrationTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreGeolocationIntegrationTests))]
    public class CoreGeolocationIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreGeolocationIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreGeolocationIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void GeolocationIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);
        }

        [Fact]
        public void GeolocationIntegration_OperatingSystem()
        {
            this.TestOperatingSystem?.GeolocationService.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreGeolocationService>();
            this.TestOperatingSystem?.GeolocationService.OperatingSystem.Should().BeSameAs(this.TestOperatingSystem);
        }

        [Fact]
        public void GeolocationIntegration_IsGeolocationEnabled_Output()
        {
            this.TestOperatingSystem?.GeolocationService.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreGeolocationService>();
            this.TestOutputHelper.WriteLine($"IsGeolocationEnabled: {this.TestOperatingSystem?.GeolocationService.IsGeolocationEnabled}");
        }

        [Fact]
        public async Task GeolocationIntegration_IsGeolocationPermissionGrantedAsync_Output()
        {
            this.TestOperatingSystem?.GeolocationService.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreGeolocationService>();
            bool isGeolocationPermissionGranted = await this.TestOperatingSystem!.GeolocationService.IsGeolocationPermissionGrantedAsync();

            this.TestOutputHelper.WriteLine($"IsGeolocationPermissionGranted: {isGeolocationPermissionGranted}");
        }
    }
}

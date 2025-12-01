// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreGuidExtensionsUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using FluentAssertions;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Extensions
{
    /// <summary>
    /// Class CoreGuidExtensionsUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreGuidExtensionsUnitTests))]
    public class CoreGuidExtensionsUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreGuidExtensionsUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreGuidExtensionsUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreGuidExtensionsUnit_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Unit);
        }

        /// <summary>
        /// Defines the test method GuidExtensions_Empty.
        /// </summary>
        [Fact]
        public void GuidExtensions_Empty()
        {
            Guid guid = Guid.Empty;

            string guidNoDashes = guid.ToStringNoDashes();

            guidNoDashes.Should().Be("00000000000000000000000000000000");
        }

        /// <summary>
        /// Defines the test method GuidExtensions_LowerCase.
        /// </summary>
        [Fact]
        public void GuidExtensions_LowerCase()
        {
            var guid = Guid.NewGuid();
            string guidNoDashes = guid.ToStringNoDashes();

            guidNoDashes.Should().Be(guid.ToString().ToLowerInvariant().Replace("-", string.Empty));
        }
    }
}

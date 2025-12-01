// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-20-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-20-2020
// ***********************************************************************
// <copyright file="CoreTestClassFixtureUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Test
{
    /// <summary>
    /// Class CoreTestClassFixtureUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreTestClassFixtureUnitTests))]

    public class CoreTestClassFixtureUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTestClassFixtureUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreTestClassFixtureUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void TestClassFixture_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Unit);
        }

        [Fact]
        public void TestClassFixture_DisposeTwice()
        {
            using var testAssemblyFixture = CoreTestAssemblyFixture.Create();
            var testClassFixture = new CoreTestClassFixture(testAssemblyFixture);
            testClassFixture.Dispose();
            testClassFixture.Dispose();
        }
    }
}

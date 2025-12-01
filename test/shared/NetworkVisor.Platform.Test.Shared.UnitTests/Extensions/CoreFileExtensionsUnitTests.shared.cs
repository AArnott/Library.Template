// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreFileExtensionsUnitTests.shared.cs" company="Network Visor">
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
    /// Class CoreFileExtensionsUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreFileExtensionsUnitTests))]

    public class CoreFileExtensionsUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreFileExtensionsUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreFileExtensionsUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreFileExtensionsUnit_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Unit);
        }

        /// <summary>
        /// Defines the test method FileExtensions_PrefixExt.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <param name="ext">The ext.</param>
        [Theory]
        [InlineData("prefix", ".ext")]
        public void FileExtensions_PrefixExt(string prefix, string ext)
        {
            string? randomFile = FileExtensions.GetRandomFileName(prefix, ext);

            this.TestOutputHelper.WriteLine(randomFile);
            randomFile.Should().EndWith(ext);
            randomFile.Should().StartWith(prefix);

            string? randomFile2 = FileExtensions.GetRandomFileName(prefix, ext);
            randomFile2.Should().NotBe(randomFile);
            this.TestOutputHelper.WriteLine(randomFile2);
        }

        /// <summary>
        /// Defines the test method FileExtensions_NullExt.
        /// </summary>
        [Fact]
        public void FileExtensions_NullExt()
        {
            string? prefix = "prefix";

            string? randomFile = FileExtensions.GetRandomFileName(prefix, null);
            this.TestOutputHelper.WriteLine(randomFile);

            randomFile.Should().StartWith(prefix);

            string? randomFile2 = FileExtensions.GetRandomFileName(prefix, null);
            this.TestOutputHelper.WriteLine(randomFile2);

            randomFile2.Should().NotBe(randomFile);
        }

        /// <summary>
        /// Defines the test method FileExtensions_NullPrefix.
        /// </summary>
        [Fact]
        public void FileExtensions_NullPrefix()
        {
            string? ext = ".ext";

            string? randomFile = FileExtensions.GetRandomFileName(null, ext);

            this.TestOutputHelper.WriteLine(randomFile);
            randomFile.Should().EndWith(ext);

            string? randomFile2 = FileExtensions.GetRandomFileName(null, ext);
            this.TestOutputHelper.WriteLine(randomFile2);
            randomFile2.Should().NotBe(randomFile);
        }
    }
}

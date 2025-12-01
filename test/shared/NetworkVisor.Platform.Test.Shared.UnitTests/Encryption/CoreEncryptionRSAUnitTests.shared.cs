// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// // ***********************************************************************
// <copyright file="CoreEncryptionRSAUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.Encryption.Asymmetry.DSA;
using NetworkVisor.Core.Encryption.Asymmetry.RSA;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Encryption
{
    /// <summary>
    /// Class CoreEncryptionRSAUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreEncryptionRSAUnitTests))]

    public class CoreEncryptionRSAUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreEncryptionRSAUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreEncryptionRSAUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreEncryptionRSA_CoreDSAKeyGenerator_Output()
        {
            CoreKeyParameter keyParameter = CoreDSAKeyGenerator.Generator();

            keyParameter.Should().NotBeNull().And.Subject.Should().BeAssignableTo<CoreKeyParameter>();
            this.TestOutputHelper.WriteLine(keyParameter.ToStringWithParentsPropNameMultiLine());
        }

        [Fact]
        public void CoreEncryptionRSA_CoreDSAKeyGenerator_InvalidSize()
        {
            // Size should not be less than 0
            Action act = () => CoreDSAKeyGenerator.Generator(-1);
            act.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("size");

            // Size should not be 0
            act = () => CoreDSAKeyGenerator.Generator(0);
            act.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("size");

            // Size should not be greater than 1024
            act = () => CoreDSAKeyGenerator.Generator(1025);
            act.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("size");

            // Size must be a multiple of 64
            act = () => CoreDSAKeyGenerator.Generator(1023);
            act.Should().Throw<ArgumentOutOfRangeException>().And.ParamName.Should().Be("size");
        }
    }
}

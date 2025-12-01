// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// ***********************************************************************
// <copyright file="CoreEmailExtensionsUnitTests.shared.cs" company="Network Visor">
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
    /// Class CoreNetworkIdentityUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreEmailExtensionsUnitTests))]

    public class CoreEmailExtensionsUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreEmailExtensionsUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreEmailExtensionsUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Theory]
        [InlineData("", false)]
        [InlineData(null, false)]
        [InlineData("stevebu@networkvisor.com", true)]
        [InlineData("email@example.com", true)]
        [InlineData("firstname.lastname@example.com", true)]
        [InlineData("email@subdomain.example.com", true)]
        [InlineData("firstname+lastname@example.com", true)]

        [InlineData("\"email\"@example.com", true)]
        [InlineData("1234567890@example.com", true)]
        [InlineData("email@example-one.com", true)]
        [InlineData("_______@example.com", true)]
        [InlineData("email@example.name", true)]
        [InlineData("email@example.museum", true)]
        [InlineData("email@example.co.jp", true)]
        [InlineData("firstname-lastname@example.com", true)]

        [InlineData("email@example.web", true)]

        [InlineData("plainaddress", false)]
        [InlineData("#@%^%#$@#$@#.com", false)]
        [InlineData("@example.com", false)]
        [InlineData("Joe Smith <email@example.com>", false)]
        [InlineData("email.example.com", false)]
        [InlineData("email@example@example.com", false)]
        [InlineData(".email@example.com", false)]
        [InlineData("email.@example.com", false)]
        [InlineData("email..email@example.com", false)]
        [InlineData("email@example.com (Joe Smith)", false)]
        [InlineData("email@example", false)]
        [InlineData("email@-example.com", false)]
        [InlineData("email@111.222.333.44444", false)]
        [InlineData("email@example..com", false)]
        [InlineData("Abc..123@example.com", false)]

        [InlineData("あいうえお@example.com", true)]
        [InlineData("email@123.123.123.123", false)]
        [InlineData("email@[123.123.123.123]", false)]
        public void EmailExtensions_ValidateEmailAddress(string? emailAddress, bool isValid)
        {
            emailAddress.IsValidEmailAddress().Should().Be(isValid);
        }
    }
}

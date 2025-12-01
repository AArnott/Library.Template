// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="CoreDatabaseUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database
{
    /// <summary>
    /// Class CoreDatabaseUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreDatabaseUnitTests))]

    public class CoreDatabaseUnitTests : CoreTestCaseBase
    {
        private readonly string userSecureStorageFilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreDatabaseUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreDatabaseUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.userSecureStorageFilePath = this.TestFileSystem.GetLocalUserAppDatabaseTempFilePath("dbtest", ".db");
        }

        [Fact]
        public void DatabaseUnitTest_Test()
        {
        }
    }
}

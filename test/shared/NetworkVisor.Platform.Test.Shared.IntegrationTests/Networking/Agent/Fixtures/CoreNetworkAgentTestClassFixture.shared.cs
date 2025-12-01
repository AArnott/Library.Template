// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 10-05-2024
// // ***********************************************************************
// <copyright file="CoreNetworkAgentTestClassFixture.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary>
//      Network Agent Test class fixture.  Instantiated once per test class.
// </summary>

using NetworkVisor.Core.Test.Fixtures;
using Xunit;

namespace NetworkVisor.Platform.Test.Fixtures
{
    /// <summary>
    /// Provides a fixture for test class, instantiated once per test class.
    /// </summary>
    public class CoreNetworkAgentTestClassFixture : CoreTestClassFixture
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkAgentTestClassFixture"/> class.
        /// </summary>
        /// <param name="testAssemblyFixture">
        /// The <see cref="ICoreTestAssemblyFixture"/> instance that provides shared resources for the test run.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="testAssemblyFixture"/> is <c><see langword="null"/></c>.
        /// </exception>
        public CoreNetworkAgentTestClassFixture(CoreTestAssemblyFixture testAssemblyFixture)
        : base(testAssemblyFixture)
        {
        }
    }
}

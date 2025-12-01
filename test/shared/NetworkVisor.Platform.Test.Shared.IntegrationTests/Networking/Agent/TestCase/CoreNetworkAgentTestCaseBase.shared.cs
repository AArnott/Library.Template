// Assembly         : NetworkVisor.Platform.Test.Shared
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 10-05-2024
// // ***********************************************************************
// <copyright file="CoreNetworkAgentTestCaseBase.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using NetworkVisor.Core.Test.Fixtures;
using NetworkVisor.Platform.Test.Fixtures;
using Xunit;

namespace NetworkVisor.Platform.Test.TestCase
{
    /// <summary>
    /// Represents the base class for network agent core test cases in the NetworkVisor platform.
    /// </summary>
    /// <remarks>
    /// This abstract class provides a foundational implementation for test cases, integrating
    /// with xUnit and offering various utilities and services for testing within the NetworkVisor platform.
    /// </remarks>
    public abstract class CoreNetworkAgentTestCaseBase : CoreTestClassBase, IClassFixture<CoreNetworkAgentTestClassFixture>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkAgentTestCaseBase"/> class.
        /// </summary>
        /// <param name="testClassFixture">The Network Agent test class fixture.</param>
        /// <remarks>
        ///     TestClassFixture is shared across test cases within the same test class.
        /// </remarks>
        protected CoreNetworkAgentTestCaseBase(ICoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }
    }
}

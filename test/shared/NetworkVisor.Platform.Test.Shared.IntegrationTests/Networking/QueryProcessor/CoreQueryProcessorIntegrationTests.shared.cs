// Assembly         : NetworkVisor.Platform.Test.Shared.Messaging.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="CoreQueryProcessorIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.Messaging.Services.CommandProcessor;
using NetworkVisor.Core.Messaging.Services.QueryProcessor;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Paramore.Darker;
using Xunit;

namespace NetworkVisor.Platform.Test.Messaging.Shared.IntegrationTests.QueryProcessor
{
    /// <summary>
    /// Class CoreQueryProcessorIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreQueryProcessorIntegrationTests))]

    public class CoreQueryProcessorIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreQueryProcessorIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreQueryProcessorIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CommandDispatchService_IsRunning()
        {
            _ = this.TestCommandDispatchService.Should().NotBeNull();
            _ = this.TestCommandDispatchService!.IsRunning.Should().BeTrue();
        }

        [Fact]
        public void QueryProcessor_Ctor()
        {
            _ = this.TestQueryProcessor!.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestQueryProcessor>();
            _ = this.TestQueryProcessor!.Should().BeAssignableTo<ICoreQueryProcessor>();
            _ = this.TestQueryProcessor!.Should().BeAssignableTo<IQueryProcessor>();
        }
    }
}

// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-20-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-20-2020
// ***********************************************************************
// <copyright file="CoreTestContextIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.TestCase;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Test
{
    /// <summary>
    /// Class CoreTestContextIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreTestContextIntegrationTests))]

    public class CoreTestContextIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTestContextIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreTestContextIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void TestContextIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);
        }

        [Fact]
        public void TestContextIntegration_ActiveTestContext()
        {
            this.XunitTestContext.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ITestContext>();
            ActiveTestContext.Should().BeSameAs(this.XunitTestContext);
            this.XunitTestContext.Should().BeSameAs(Xunit.TestContext.Current);
        }

        [Fact]
        public void TestContextIntegration_ActiveXunitTestAssembly()
        {
            this.XunitTestContext.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ITestContext>();
            this.XunitTestContext.TestAssembly.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ITestAssembly>();
            this.XunitTestContext.TestAssembly.Should().NotBeNull().And.Subject.Should().BeAssignableTo<IXunitTestAssembly>();
            CoreTestAssemblyFixtureBase.ActiveXunitTestAssembly.Should().BeSameAs(this.XunitTestContext.TestAssembly);
            this.XunitTestContext.TestAssembly.Should().BeSameAs(Xunit.TestContext.Current.TestAssembly);
            this.XunitTestAssembly.Should().BeSameAs(Xunit.TestContext.Current.TestAssembly);
        }

        [Fact]
        public void TestContextIntegration_ActiveXunitTest()
        {
            this.XunitTestContext.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ITestContext>();
            this.XunitTestContext.Test.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ITest>();
            this.XunitTestContext.Test.Should().NotBeNull().And.Subject.Should().BeAssignableTo<IXunitTest>();
            ActiveXunitTest.Should().BeSameAs(this.XunitTest);
            this.XunitTestContext.Test.Should().BeSameAs(Xunit.TestContext.Current.Test);
            this.XunitTest.Should().BeSameAs(Xunit.TestContext.Current.Test);
        }

        [Fact]
        public void TestContextIntegration_ActiveTestCase()
        {
            ActiveTestCase.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestCase>();
            ActiveTestCase.Should().BeSameAs(this);
        }

        [Fact]
        public void TestContextIntegration_ActiveXunitTestCase()
        {
            this.XunitTestContext.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ITestContext>();
            this.XunitTestContext.TestCase.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ITestCase>();
            this.XunitTestContext.TestCase.Should().NotBeNull().And.Subject.Should().BeAssignableTo<IXunitTestCase>();
            ActiveXunitTestCase.Should().BeSameAs(this.XunitTestCase);
            this.XunitTestContext.TestCase.Should().BeSameAs(Xunit.TestContext.Current.TestCase);
            this.XunitTestCase.Should().BeSameAs(Xunit.TestContext.Current.TestCase);
        }

        [Fact]
        public void TestContextIntegration_ActiveTestOutputHelper()
        {
            this.XunitTestContext.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ITestContext>();
            ActiveTestOutputHelper.Should().BeSameAs(this.TestOutputHelper);
            this.XunitTestContext.TestOutputHelper.Should().BeSameAs(this.TestOutputHelper.WrappedTestOutputHelper);
            ActiveTestOutputHelper!.WriteLine("Test Output from custom TestOutputHelper");
        }

        [Fact]
        public void TestContextIntegration_ActiveTestCaseLogger()
        {
            ActiveTestCaseLogger.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestCaseLogger>();
            ActiveTestCaseLogger.Should().BeSameAs(this.TestCaseLogger);
            ActiveTestCaseLogger!.LogDebug("Test logging from custom ActiveTestCaseLogger");
        }

        [Fact]
        public void TestContextIntegration_ActiveTestCaseLogger_BeginTestCaseScope()
        {
            ActiveTestCaseLogger.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestCaseLogger>();
            ActiveTestCaseLogger.Should().BeSameAs(this.TestCaseLogger);
            using IDisposable? testCaseScope = ActiveTestCaseLogger!.BeginTestCaseScope(this);
            ActiveTestCaseLogger!.LogDebug("Test logging from custom ActiveTestCaseLogger with test case scope");
        }

        [Fact]
        public void TestContextIntegration_ActiveTestDisplayName()
        {
            this.XunitTestContext.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ITestContext>();
            this.XunitTestContext.TestMethod?.MethodName.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"Test Method Name: {this.XunitTestContext.TestMethod?.MethodName}");
            ActiveTestDisplayName.Should().BeSameAs(this.TestDisplayName);
            this.XunitTestContext.TestMethod?.MethodName.Should().Be("TestContextIntegration_ActiveTestDisplayName");
        }
    }
}

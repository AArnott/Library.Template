// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 05-12-2020
//
// Last Modified By : SteveBu
// Last Modified On : 05-12-2020
// ***********************************************************************
// <copyright file="CoreNullLoggerFactoryUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Logging.Factory;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Logging.Logger
{
    /// <summary>
    /// Class CoreNullLoggerFactoryUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreNullLoggerFactoryUnitTests))]

    public class CoreNullLoggerFactoryUnitTests : CoreTestCaseBase
    {
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNullLoggerFactoryUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNullLoggerFactoryUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.TestNullLoggerFactory = new NullCoreLoggerFactory(this.TestAssembly);
            AssertionConfiguration.Current.Formatting.MaxLines = 2000;
        }

        private ICoreLoggerFactory TestNullLoggerFactory { get; }

        [Fact]
        public void CoreNullLoggerFactoryUnit_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Unit);
        }

        /// <summary>
        /// Defines the test method NullCoreLoggerFactory_Instance.
        /// </summary>
        [Fact]
        public void NullCoreLoggerFactory_Instance()
        {
            this.TestNullLoggerFactory.Should().NotBeNull();
        }

        /// <summary>
        /// Defines the test method NullCoreLoggerFactory_Methods.
        /// </summary>
        [Fact]
        public void NullCoreLoggerFactory_Methods()
        {
            this.TestNullLoggerFactory.Should().NotBeNull();
            this.TestNullLoggerFactory.WrappedLoggerFactory.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ILoggerFactory>();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            this.TestNullLoggerFactory.AddProvider(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        /// <summary>
        /// Defines the test method NullCoreLoggerFactory_Construct.
        /// </summary>
        [Fact]
        public void NullCoreLoggerFactory_Construct()
        {
            using var nullLoggerFactory = new NullCoreLoggerFactory(this.TestAssembly);

            nullLoggerFactory.Should().NotBeNull();
            nullLoggerFactory.WrappedLoggerFactory.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ILoggerFactory>();
        }

        /// <summary>
        /// Defines the test method NullCoreLoggerFactory_Dispose_Twice.
        /// </summary>
        [Fact]
        public void NullCoreLoggerFactory_Dispose_Twice()
        {
            using var nullLoggerFactory = new NullCoreLoggerFactory(this.TestAssembly);

            nullLoggerFactory.Should().NotBeNull();
            nullLoggerFactory!.Dispose();
        }

        [Fact]
        public void NullCoreLoggerFactory_ServiceProvider()
        {
            using var nullLoggerFactory = new NullCoreLoggerFactory(this.TestAssembly);

            nullLoggerFactory.ServiceProvider.Should().NotBeNull().And.BeAssignableTo<IServiceProvider>();
            ICoreFileSystem fileSystem = nullLoggerFactory.ServiceProvider.GetRequiredService<ICoreFileSystem>();
            fileSystem.Should().NotBeNull().And.BeAssignableTo<ICoreFileSystem>();
        }

        /// <summary>
        /// Defines the test method NullCoreLoggerFactory_CreateLoggers.
        /// </summary>
        [Fact]
        public void NullCoreLoggerFactory_CreateLoggers()
        {
            using var nullLoggerFactory = new NullCoreLoggerFactory(this.TestAssembly);

            nullLoggerFactory.Should().NotBeNull();
            nullLoggerFactory.CreateLogger("test").Should().NotBeNull().And.Subject.Should().BeAssignableTo<ILogger>().And.Subject.Should().BeAssignableTo<ICoreLogger>();
            nullLoggerFactory.CreateLogger<CoreNullLoggerFactoryUnitTests>().Should().NotBeNull().And.Subject.Should().BeAssignableTo<ILogger>().And.Subject.Should().BeAssignableTo<ICoreLogger<CoreNullLoggerFactoryUnitTests>>();
            nullLoggerFactory.CreateCoreLogger("test").Should().NotBeNull().And.Subject.Should().BeAssignableTo<ILogger>().And.Subject.Should().BeAssignableTo<ICoreLogger>();
            nullLoggerFactory.CreateCoreLogger<CoreNullLoggerFactoryUnitTests>().Should().NotBeNull().And.Subject.Should().BeAssignableTo<ILogger>().And.Subject.Should().BeAssignableTo<ICoreLogger<CoreNullLoggerFactoryUnitTests>>();
            nullLoggerFactory.CreateCoreLogger(typeof(CoreNullLoggerFactoryUnitTests).GetLoggerCategoryName()).Should().NotBeNull().And.Subject.Should().BeAssignableTo<ILogger>().And.Subject.Should().BeAssignableTo<ICoreLogger>();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!this.disposedValue)
                {
                    this.TestNullLoggerFactory?.Dispose();
                    this.disposedValue = true;
                }
            }

            base.Dispose(disposing);
        }
    }
}

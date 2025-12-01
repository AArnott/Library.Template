// Assembly         : NetworkVisor.Platform.Test.Shared
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 10-05-2024
// // ***********************************************************************
// <copyright file="CoreTestAssemblyFixture.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary>
//      Test assembly fixture.  Instantiated once per test assembly.
//      Must be placed in the same assembly as the test classes.
// </summary>

using System.Net;
using System.Net.NetworkInformation;
using Microsoft.Extensions.DependencyInjection;
using NetworkVisor.Core.CoreObject;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Test.TestStartup;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Serialization;
using NetworkVisor.Platform.Test.TestStartup;
using Xunit;
using Xunit.Sdk;

[assembly: AssemblyFixture(typeof(CoreTestAssemblyFixture))]

namespace NetworkVisor.Platform.Test.Fixtures
{
    /// <summary>
    /// Provides a fixture for a test run, instantiated once per test run.
    /// </summary>
    public class CoreTestAssemblyFixture : CoreTestAssemblyFixtureBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTestAssemblyFixture"/> class.
        /// </summary>
        public CoreTestAssemblyFixture()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkVisor.Platform.Test.Fixtures.CoreTestAssemblyFixture"/> class
        /// with the specified test startup services, application update settings, initial services,
        /// cleanup behavior, and test case scope configuration.
        /// </summary>
        /// <param name="testStartupServices">
        /// An optional implementation of <see cref="NetworkVisor.Core.Test.TestStartup.ICoreTestStartupServices"/>
        /// to configure test-specific startup services. If not provided, a default instance is used.
        /// </param>
        /// <param name="updateStaticApplication">
        /// A boolean indicating whether to enable static application updates. Defaults to <see langword="true"/>.
        /// </param>
        /// <param name="initialServices">
        /// An optional <see cref="Microsoft.Extensions.DependencyInjection.ServiceCollection"/>
        /// to initialize additional services for the test environment.
        /// </param>
        /// <param name="cleanUpAppSessionOnDispose">
        /// An optional boolean specifying whether to clean up the application session when the fixture is disposed.
        /// </param>
        /// <param name="addTestCaseScope">
        /// An optional boolean indicating whether to add a test case scope to the services.
        /// </param>
        /// <remarks>
        /// This constructor is designed to provide flexibility in configuring the test assembly fixture,
        /// allowing customization of startup services, application behavior, and resource management.
        /// </remarks>
        protected CoreTestAssemblyFixture(ICoreTestStartupServices? testStartupServices = null, bool updateStaticApplication = true, ServiceCollection? initialServices = null, bool? cleanUpAppSessionOnDispose = null, bool? addTestCaseScope = null)
            : base(testStartupServices ?? new CoreTestStartupServices(updateStaticApplication && !IsMobileTestApp), updateStaticApplication, initialServices, cleanUpAppSessionOnDispose, addTestCaseScope)
        {
        }

#if NV_PLAT_MOBILE
        /// <summary>
        /// Gets a value indicating whether the current test application is a mobile test application.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the test application is a mobile test application; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// This property is used to determine the type of test application being executed,
        /// which can influence the configuration and behavior of the test environment.
        /// </remarks>
        public static bool IsMobileTestApp => true;
#else
        /// <summary>
        /// Gets a value indicating whether the current test application is a mobile test application.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the test application is a mobile test application; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// This property is used to determine the type of test application being executed,
        /// which can influence the configuration and behavior of the test environment.
        /// </remarks>
        public static bool IsMobileTestApp => false;
#endif

        /// <summary>
        /// Creates a new instance of the <see cref="CoreTestAssemblyFixture"/> class with the specified configuration options.
        /// </summary>
        /// <param name="testStartupServices">
        /// An optional instance of <see cref="ICoreTestStartupServices"/> to initialize startup services. Defaults to <see langword="null"/>.
        /// </param>
        /// <param name="updateStaticApplication">
        /// A boolean value indicating whether to update the static application instance. Defaults to <see langword="false"/>.
        /// </param>
        /// <param name="initialServices">
        /// An optional <see cref="ServiceCollection"/> to provide initial services. Defaults to <see langword="null"/>.
        /// </param>
        /// <param name="cleanUpAppSessionOnDispose">
        /// A boolean value indicating whether to clean up the application session on disposal. Defaults to <see langword="null"/>.
        /// </param>
        /// <param name="addTestCaseScope">
        /// A boolean value indicating whether to add a test case scope. Defaults to <see langword="null"/>.
        /// </param>
        /// <returns>
        /// A new instance of the <see cref="CoreTestAssemblyFixture"/> class configured with the specified options.
        /// </returns>
        public static CoreTestAssemblyFixture Create(ICoreTestStartupServices? testStartupServices = null, bool updateStaticApplication = false, ServiceCollection? initialServices = null, bool? cleanUpAppSessionOnDispose = null, bool? addTestCaseScope = null)
            => new(testStartupServices ?? new CoreTestStartupServices(updateStaticApplication), updateStaticApplication, initialServices, cleanUpAppSessionOnDispose, addTestCaseScope);
    }
}

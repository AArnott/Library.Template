// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.Tests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreTestStartupServices.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Methods and properties to access the file system.</summary>
// ***********************************************************************

using Microsoft.Extensions.Configuration;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Networking.Hosting;
using NetworkVisor.Core.Startup;
using NetworkVisor.Core.Test.TestStartup;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared;

namespace NetworkVisor.Platform.Test.TestStartup
{
    /// <summary>
    /// Represents the core test startup services used in the NetworkVisor platform's testing infrastructure.
    /// </summary>
    /// <remarks>
    /// This class extends <see cref="NetworkVisor.Core.Startup.CoreStartupServices"/> and implements
    /// <see cref="ICoreTestStartupServices"/> to provide additional functionality
    /// specific to test scenarios. It is designed to facilitate the initialization and configuration of
    /// test-specific services and environments.
    /// </remarks>
    public class CoreTestStartupServices : CoreStartupServices, ICoreTestStartupServices
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTestStartupServices"/> class.
        /// </summary>
        /// <param name="updateStaticInstance">
        /// A value indicating whether to update the static instance of the startup services. Defaults to <see langword="true"/>.
        /// </param>
        /// <param name="configurationManager">
        /// An optional <see cref="IConfigurationManager"/> instance for managing application configuration.
        /// </param>
        /// <param name="appFolderName">
        /// An optional name of the application folder, used for organizing application-specific data.
        /// </param>
        /// <param name="appSessionID">
        /// An optional identifier for the application session, useful for tracking and managing session-specific data.
        /// </param>
        /// <param name="appHostEnvironment">
        /// The hosting environment in which the application is running. Defaults to <see cref="CoreHostEnvironment.Default"/>.
        /// </param>
        /// <param name="appSettingsLoadOptions">
        /// Optional settings that specify how application settings should be loaded. Defaults to <see langword="null"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <see cref="CoreTestAssemblyFixtureBase.ActiveTestAssembly"/> is <see langword="null"/>.
        /// </exception>
        public CoreTestStartupServices(bool updateStaticInstance = true, IConfigurationManager? configurationManager = null, string? appFolderName = null, string? appSessionID = null, CoreHostEnvironment appHostEnvironment = CoreHostEnvironment.Default, CoreAppSettingsLoadOptions? appSettingsLoadOptions = null)
        : base(CoreTestAssemblyFixtureBase.ActiveTestAssembly ?? throw new ArgumentNullException(nameof(CoreTestAssemblyFixtureBase.ActiveTestAssembly)), updateStaticInstance, configurationManager, appFolderName, appSessionID, appHostEnvironment, appSettingsLoadOptions)
        {
        }
    }
}

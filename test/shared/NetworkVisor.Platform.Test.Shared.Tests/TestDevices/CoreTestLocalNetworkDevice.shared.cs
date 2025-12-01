// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// // ***********************************************************************
// <copyright file="CoreTestLocalNetworkDevice.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.Device;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.Services;
using NetworkVisor.Core.Startup;
using NetworkVisor.Platform.Networking.Devices;

namespace NetworkVisor.Platform.Test.TestDevices
{
    public class CoreTestLocalNetworkDevice<TTestClass> : CoreLocalNetworkDevice, ICoreTestLocalNetworkDevice
    {
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTestLocalNetworkDevice{TTestClass}"/> class.
        /// </summary>
        /// <param name="networkServices">Network Services interface.</param>
        /// <param name="cacheTimestamp">Timestamp of cache to use.</param>
        /// <param name="cacheVersion">Version of the cache to use.</param>
        /// <param name="logger">Optional logger.</param>
        /// <param name="deviceType">Type of device.</param>
        public CoreTestLocalNetworkDevice(ICoreNetworkServices networkServices, DateTimeOffset cacheTimestamp, ulong cacheVersion, ICoreLogger? logger = null, CoreDeviceType deviceType = CoreDeviceType.LocalComputer)
            : base(networkServices, cacheTimestamp, cacheVersion, logger, deviceType)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTestLocalNetworkDevice{TTestClass}"/> class.
        /// </summary>
        /// <param name="networkServices">Network Services interface.</param>
        /// <param name="cacheTimestamp">Timestamp of cache to use.</param>
        /// <param name="logger">Optional logger.</param>
        public CoreTestLocalNetworkDevice(ICoreNetworkServices networkServices, DateTimeOffset cacheTimestamp, ICoreLogger? logger = null)
            : base(networkServices, cacheTimestamp, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTestLocalNetworkDevice{TTestClass}"/> class.
        /// </summary>
        /// <param name="networkServices">Network Services interface.</param>
        /// <param name="cacheVersion">Version of the cache to use.</param>
        /// <param name="logger">Optional logger.</param>
        public CoreTestLocalNetworkDevice(ICoreNetworkServices networkServices, ulong cacheVersion, ICoreLogger? logger = null)
            : base(networkServices, cacheVersion, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTestLocalNetworkDevice{TTestClass}"/> class.
        /// </summary>
        /// <param name="networkServices">Network Services interface.</param>
        /// <param name="logger">Optional logger.</param>
        public CoreTestLocalNetworkDevice(ICoreNetworkServices networkServices, ICoreLogger? logger = null)
            : base(networkServices, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTestLocalNetworkDevice{TTestClass}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        /// <param name="logger">Optional logger instance for logging purposes.</param>
        public CoreTestLocalNetworkDevice(IServiceProvider serviceProvider, ICoreLogger? logger = null)
            : base(serviceProvider, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTestLocalNetworkDevice{TTestClass}"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        public CoreTestLocalNetworkDevice(IServiceProvider serviceProvider)
            : base(serviceProvider, serviceProvider.GetRequiredService<ICoreLogger>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTestLocalNetworkDevice{TTestClass}"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor is specifically designed for test scenarios. It creates a test network services instance
        /// using the type of the test class <typeparamref name="TTestClass"/> and sets up the device accordingly.
        /// </remarks>
        public CoreTestLocalNetworkDevice()
            : base(CoreStartupServices.ServiceProvider)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                try
                {
                    if (disposing)
                    {
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Failed to dispose TestCoreLocalNetworkDevice.");
                }
                finally
                {
                    this.disposedValue = true;
                }
            }

            base.Dispose(disposing);
        }
    }
}

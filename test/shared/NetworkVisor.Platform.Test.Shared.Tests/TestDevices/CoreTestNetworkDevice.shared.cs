// ***********************************************************************
// Assembly         : NetworkVisor.Core.Test
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 10-05-2024
// ***********************************************************************
// <copyright file="CoreTestNetworkDevice.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using NetworkVisor.Core.CoreObject;
using NetworkVisor.Core.Device;
using NetworkVisor.Core.Logging.LogProperty;
using NetworkVisor.Core.Networking.Devices;
using NetworkVisor.Core.Networking.Preferred;
using NetworkVisor.Core.Networking.Services;
using NetworkVisor.Core.Startup;
using NetworkVisor.Core.Test.TestCase;
using NetworkVisor.Platform.Test.TestCase;

namespace NetworkVisor.Platform.Test.TestDevices
{
    /// <summary>
    /// Represents a test implementation of a core network device, providing platform-specific
    /// overrides and additional functionality for testing purposes.
    /// </summary>
    /// <typeparam name="TTestClass">
    /// The type of the test class associated with this network device.
    /// </typeparam>
    /// <remarks>
    /// This class extends <see cref="CoreNetworkDeviceBase{T}"/> to provide a specialized
    /// implementation for testing network devices. It includes platform-specific overrides
    /// for retrieving network-related properties and supports test-specific configurations.
    /// </remarks>
    public class CoreTestNetworkDevice<TTestClass> : CoreNetworkDeviceBase<CoreTestNetworkDevice<TTestClass>>, ICoreTestNetworkDevice
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTestNetworkDevice{TTestClass}"/> class.
        /// </summary>
        /// <param name="networkServices">
        /// An instance of <see cref="ICoreNetworkServices"/> representing the network services interface.
        /// </param>
        /// <param name="preferredNetworkAddress">
        /// An optional instance of <see cref="ICorePreferredNetworkAddress"/> representing the preferred network address.
        /// </param>
        /// <param name="deviceType">
        /// The type of the device, with a default value of <see cref="CoreDeviceType.NetworkDevice"/>.
        /// </param>
        public CoreTestNetworkDevice(ICoreNetworkServices networkServices, ICorePreferredNetworkAddress? preferredNetworkAddress, CoreDeviceType deviceType = CoreDeviceType.NetworkDevice)
            : base(networkServices, Guid.NewGuid(), preferredNetworkAddress?.IPAddress, preferredNetworkAddress?.SubnetMask, preferredNetworkAddress?.PhysicalAddress, deviceType)
        {
            this.TestPreferredNetworkAddress = preferredNetworkAddress;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTestNetworkDevice{TTestClass}"/> class.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider used to resolve required network services.
        /// </param>
        /// <remarks>
        /// This constructor configures the test network device by initializing it with the preferred local network address
        /// and setting up essential properties such as IP address, subnet mask, and physical address.
        /// It ensures that the network services are properly managed and disposed of when no longer required.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the preferred local network address is <see langword="null"/>.
        /// </exception>
        public CoreTestNetworkDevice(IServiceProvider serviceProvider)
            : base(serviceProvider.GetRequiredService<ICoreNetworkServices>(), Guid.NewGuid(), null, CoreDeviceType.TestLocalNetworkDevice)
        {
            this.TestPreferredNetworkAddress = this.NetworkingSystem.PreferredLocalNetworkAddress ?? throw new ArgumentNullException(nameof(this.NetworkingSystem.PreferredLocalNetworkAddress));
            _ = this.SetBagWithAction(CorePropID.NetworkDeviceIPAddress, this.NetworkingSystem.PreferredLocalNetworkAddress.IPAddress, CoreObjectItemProvider.NetworkDevice, CoreLogPropertyType.IPAddress, true);
            _ = this.SetBagWithAction(CorePropID.NetworkDeviceSubnetMask, this.NetworkingSystem.PreferredLocalNetworkAddress.SubnetMask, CoreObjectItemProvider.NetworkDevice, CoreLogPropertyType.IPAddress, true);
            _ = this.SetBagWithAction(CorePropID.NetworkDevicePhysicalAddress, this.NetworkingSystem.PreferredLocalNetworkAddress.PhysicalAddress, CoreObjectItemProvider.NetworkDevice, CoreLogPropertyType.PhysicalAddress, true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTestNetworkDevice{TTestClass}"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor sets up the test network device with the preferred local network address
        /// and configures the necessary properties such as IP address, subnet mask, and physical address.
        /// It also ensures that the network services are properly disposed of when no longer needed.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the preferred local network address is <see langword="null"/>.
        /// </exception>
        public CoreTestNetworkDevice()
            : this(CoreStartupServices.ServiceProvider)
        {
        }

        /// <inheritdoc />
        [JsonIgnore]
        public ICoreTestCase? ActiveTestCase => CoreTestClassBase.ActiveTestCase;

        /// <summary>
        /// Gets the test PreferredNetworkAddress.
        /// </summary>
        private ICorePreferredNetworkAddress? TestPreferredNetworkAddress { get; }

        /// <inheritdoc />
        protected override ICorePreferredNetworkAddress? CalculatePreferredNetworkAddress()
        {
            return this.TestPreferredNetworkAddress;
        }

        /// <inheritdoc />
        protected override IPAddress? GetPlatformIPAddress()
        {
            return this.TestPreferredNetworkAddress?.IPAddress;
        }

        /// <inheritdoc />
        protected override IPAddress? GetPlatformSubnetMask()
        {
            return this.TestPreferredNetworkAddress?.SubnetMask;
        }

        /// <inheritdoc />
        protected override PhysicalAddress? GetPlatformPhysicalAddress()
        {
            return this.TestPreferredNetworkAddress?.PhysicalAddress;
        }

        /// <inheritdoc />
        protected override string? GetPlatformModel()
        {
            return null;
        }

        /// <inheritdoc />
        protected override string? GetPlatformManufacturer()
        {
            return null;
        }

        /// <inheritdoc />
        protected override string? GetPlatformDeviceName()
        {
            return null;
        }

        /// <inheritdoc />
        protected override CoreDeviceIdiom? GetPlatformDeviceIdiom()
        {
            return null;
        }

        /// <inheritdoc />
        protected override CoreDeviceHostType? GetPlatformDeviceHostType()
        {
            return null;
        }

        /// <inheritdoc />
        protected override bool GetPlatformIsTestDevice() => true;

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            base.Dispose(disposing);
        }
    }
}

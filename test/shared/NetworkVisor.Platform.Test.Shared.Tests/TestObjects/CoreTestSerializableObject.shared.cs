// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// // ***********************************************************************
// <copyright file="CoreTestSerializableObject.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Networking.Services;
using NetworkVisor.Core.Serialization;
using NetworkVisor.Core.Startup;
using NetworkVisor.Platform.Test.TestDevices;

namespace NetworkVisor.Platform.Test.TestObjects
{
    public class CoreTestSerializableObject<TTestClass> : CoreSerializableObject, IEqualityComparer<CoreTestSerializableObject<TTestClass>>, IEquatable<CoreTestSerializableObject<TTestClass>>, IDisposable, ICoreTestSerializableObject
    {
        private bool disposedValue;

        public CoreTestSerializableObject(IServiceProvider serviceProvider)
            : this(serviceProvider.GetRequiredService<ICoreNetworkServices>())
        {
        }

        public CoreTestSerializableObject()
            : this(CoreStartupServices.ServiceProvider.GetRequiredService<ICoreNetworkServices>())
        {
        }

        protected CoreTestSerializableObject(ICoreNetworkServices testNetworkServices, ICoreTestCaseLogger? testCaseLogger = null)
            : base(testCaseLogger ?? testNetworkServices?.Logger as ICoreGlobalLogger)
        {
            this.TestNetworkServices = testNetworkServices ?? throw new ArgumentNullException(nameof(testNetworkServices));
            this.TestLocalNetworkDevice = new CoreTestLocalNetworkDevice<TTestClass>(testNetworkServices);
            this.TestNetworkDevice = new CoreTestNetworkDevice<TTestClass>(testNetworkServices, testNetworkServices.PreferredLocalNetworkAddress);
        }

        /// <inheritdoc/>
        public ICoreTestNetworkDevice TestNetworkDevice { get; set; }

        /// <inheritdoc/>
        public ICoreTestLocalNetworkDevice TestLocalNetworkDevice { get; set; }

        public ICoreFileSystem FileSystem => this.TestNetworkServices.FileSystem;

        public ICoreOperatingSystem OperatingSystem => this.FileSystem.OperatingSystem;

        public ICoreFrameworkInfo FrameworkInfo => this.OperatingSystem.FrameworkInfo;

        protected ICoreNetworkingSystem NetworkingSystem => this.TestNetworkServices.NetworkingSystem;

        protected ICoreNetworkServices TestNetworkServices { get; }

        /// <summary>
        /// Create a serializable object from a Json string.
        /// </summary>
        /// <typeparam name="T">Type of serializable object.</typeparam>
        /// <param name="jsonString">Json string to serialize form.</param>
        /// <param name="options">Optional JsonSerializerOptions or null to use defaults.</param>
        /// <param name="logger">Optional logger.</param>
        /// <returns>Serialized Object or exception causing failure.</returns>
        public static (T? SerializedObject, Exception? Exception) TestCreateFromJson<T>(string? jsonString, JsonSerializerOptions options, ICoreLogger? logger = null)
        {
            return CreateFromJson<T>(jsonString, options, logger);
        }

        /// <summary>
        /// Creates a serializable object from a JSON string using the specified serialization format.
        /// </summary>
        /// <typeparam name="T">The type of the serializable object to create. Must have a parameterless constructor.</typeparam>
        /// <param name="jsonString">The JSON string to deserialize from.</param>
        /// <param name="serializationFormat">The format of the serialization. Defaults to <see cref="CoreSerializationFormatFlags.JsonCompact"/>.</param>
        /// <param name="serviceProvider">An optional service provider for resolving dependencies during deserialization.</param>
        /// <param name="logger">An optional logger for logging deserialization events or errors.</param>
        /// <returns>
        /// A tuple containing the deserialized object of type <typeparamref name="T"/> (or <see langword="null"/> if deserialization fails)
        /// and an <see cref="Exception"/> instance representing the error that occurred (or <see langword="null"/> if deserialization succeeds).
        /// </returns>
        public static (T? SerializedObject, Exception? Exception) TestCreateFromJson<T>(string? jsonString, CoreSerializationFormatFlags serializationFormat = CoreSerializationFormatFlags.JsonCompact, IServiceProvider? serviceProvider = null, ICoreLogger? logger = null)
        {
            return TestCreateFromJson<T>(jsonString, CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(serializationFormat, serviceProvider), logger);
        }

        /// <inheritdoc/>
        public override bool Equals(object? other) => this.Equals(this, other as CoreTestSerializableObject<TTestClass>);

        /// <inheritdoc/>
        public bool Equals(CoreTestSerializableObject<TTestClass>? other) => this.Equals(this, other);

        /// <inheritdoc />
        public bool Equals(CoreTestSerializableObject<TTestClass>? x, CoreTestSerializableObject<TTestClass>? y)
        {
            return ReferenceEquals(x, y)
                || (x is not null
                && y is not null
                && x.GetType() == y.GetType()
                && x.FileSystem.Equals(y.FileSystem)
                && x.TestNetworkDevice.Equals(y.TestNetworkDevice));

            // && x.LocalNetworkDevice.Equals(y.LocalNetworkDevice)
        }

        /// <inheritdoc />
        public int GetHashCode(CoreTestSerializableObject<TTestClass>? obj)
        {
            var hashCode = default(HashCode);

            if (obj is not null)
            {
                hashCode.Add(obj.FileSystem);
                hashCode.Add(obj.TestNetworkServices);

                // hashCode.Add(obj.LocalNetworkDevice);
            }

            return hashCode.ToHashCode();
        }

        /// <inheritdoc />
        public override int GetHashCode() => this.GetHashCode(this);

        public void SynchronizeVersionInfo(ICoreTestSerializableObject serializableObject)
        {
            this.TestLocalNetworkDevice.SynchronizeObjectVersionInfo(serializableObject.TestLocalNetworkDevice);
            this.TestNetworkDevice.SynchronizeObjectVersionInfo(serializableObject.TestNetworkDevice);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!this.disposedValue)
            {
                this.TestNetworkDevice.Dispose();
                this.TestLocalNetworkDevice.Dispose();
            }

            this.disposedValue = true;
        }
    }
}

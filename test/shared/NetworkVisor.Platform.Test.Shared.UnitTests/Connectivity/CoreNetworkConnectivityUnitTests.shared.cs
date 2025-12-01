// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// ***********************************************************************
// <copyright file="CoreNetworkConnectivityUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using FluentAssertions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Connectivity
{
    /// <summary>
    /// Class CoreNetworkConnectivityUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreNetworkConnectivityUnitTests))]

    public class CoreNetworkConnectivityUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkConnectivityUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNetworkConnectivityUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void Ctor()
        {
            this.TestOutputHelper.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestOutputHelper>();
            this.TestNetworkingSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkingSystem>();
            this.TestNetworkingSystem.FileSystem.Should().BeSameAs(this.TestFileSystem);
        }

#if FIX_CONNECTIVITY_TESTS

        /// <summary>
        /// Defines the test method NetworkConnectivity_NetworkAccess.
        /// </summary>
        /// <param name="networkAccessPrevious">Previous network access.</param>
        /// <param name="networkAccess">Current network access.</param>
        /// <param name="systemNetworkEvent">Network Event to test.</param>
        /// <param name="isNetworkAvailable">True if network is available.</param>
        /// <param name="hasNetworkAccessChanged">True if network access has changed.</param>
        [Theory]
        [InlineData(CoreNetworkAccess.Unknown, CoreNetworkAccess.Unknown, CoreSystemNetworkEvent.Unknown, false, false)]
        [InlineData(CoreNetworkAccess.Unknown, CoreNetworkAccess.None, CoreSystemNetworkEvent.Unknown, false, true)]
        [InlineData(CoreNetworkAccess.Unknown, CoreNetworkAccess.Local, CoreSystemNetworkEvent.Unknown, true, true)]
        [InlineData(CoreNetworkAccess.Unknown, CoreNetworkAccess.ConstrainedInternet, CoreSystemNetworkEvent.AddressChange, true, true)]
        [InlineData(CoreNetworkAccess.Unknown, CoreNetworkAccess.Internet, CoreSystemNetworkEvent.NetworkAvailabilityChange, true, true)]

        [InlineData(CoreNetworkAccess.None, CoreNetworkAccess.Unknown, CoreSystemNetworkEvent.Unknown, false, true)]
        [InlineData(CoreNetworkAccess.None, CoreNetworkAccess.None, CoreSystemNetworkEvent.Unknown, false, false)]
        [InlineData(CoreNetworkAccess.None, CoreNetworkAccess.Local, CoreSystemNetworkEvent.Unknown, true, true)]
        [InlineData(CoreNetworkAccess.None, CoreNetworkAccess.ConstrainedInternet, CoreSystemNetworkEvent.AddressChange, true, true)]
        [InlineData(CoreNetworkAccess.None, CoreNetworkAccess.Internet, CoreSystemNetworkEvent.NetworkAvailabilityChange, true, true)]

        [InlineData(CoreNetworkAccess.Local, CoreNetworkAccess.Unknown, CoreSystemNetworkEvent.Unknown, false, true)]
        [InlineData(CoreNetworkAccess.Local, CoreNetworkAccess.None, CoreSystemNetworkEvent.Unknown, false, true)]
        [InlineData(CoreNetworkAccess.Local, CoreNetworkAccess.Local, CoreSystemNetworkEvent.Unknown, true, false)]
        [InlineData(CoreNetworkAccess.Local, CoreNetworkAccess.ConstrainedInternet, CoreSystemNetworkEvent.AddressChange, true, true)]
        [InlineData(CoreNetworkAccess.Local, CoreNetworkAccess.Internet, CoreSystemNetworkEvent.NetworkAvailabilityChange, true, true)]

        [InlineData(CoreNetworkAccess.ConstrainedInternet, CoreNetworkAccess.Unknown, CoreSystemNetworkEvent.Unknown, false, true)]
        [InlineData(CoreNetworkAccess.ConstrainedInternet, CoreNetworkAccess.None, CoreSystemNetworkEvent.Unknown, false, true)]
        [InlineData(CoreNetworkAccess.ConstrainedInternet, CoreNetworkAccess.Local, CoreSystemNetworkEvent.Unknown, true, true)]
        [InlineData(CoreNetworkAccess.ConstrainedInternet, CoreNetworkAccess.ConstrainedInternet, CoreSystemNetworkEvent.AddressChange, true, false)]
        [InlineData(CoreNetworkAccess.ConstrainedInternet, CoreNetworkAccess.Internet, CoreSystemNetworkEvent.NetworkAvailabilityChange, true, true)]

        [InlineData(CoreNetworkAccess.Internet, CoreNetworkAccess.Unknown, CoreSystemNetworkEvent.Unknown, false, true)]
        [InlineData(CoreNetworkAccess.Internet, CoreNetworkAccess.None, CoreSystemNetworkEvent.Unknown, false, true)]
        [InlineData(CoreNetworkAccess.Internet, CoreNetworkAccess.Local, CoreSystemNetworkEvent.Unknown, true, true)]
        [InlineData(CoreNetworkAccess.Internet, CoreNetworkAccess.ConstrainedInternet, CoreSystemNetworkEvent.AddressChange, true, true)]
        [InlineData(CoreNetworkAccess.Internet, CoreNetworkAccess.Internet, CoreSystemNetworkEvent.NetworkAvailabilityChange, true, false)]

        public void NetworkConnectivity_CoreNetworkConnectivityEventArgs_ToString(CoreNetworkAccess networkAccessPrevious, CoreNetworkAccess networkAccess, CoreSystemNetworkEvent systemNetworkEvent, bool isNetworkAvailable, bool hasNetworkAccessChanged)
        {
            var networkId = Guid.NewGuid();

            // TODO: Figure out correct timeout.
            CancellationTokenSource cts = new CancellationTokenSource();

            var networkConnectivityEventArgs = new TestCoreNetworkConnectivityEventArgs(this.TestNetworkingSystem, systemNetworkEvent, networkAccessPrevious, networkAccess, isNetworkAvailable, cts.Token, networkId);

            string? testOutput = networkConnectivityEventArgs.ToStringWithParentsPropName();
            this.TestOutputHelper.WriteLine(testOutput);

            networkConnectivityEventArgs.NetworkEventID.Should().Be(networkId);
            networkConnectivityEventArgs.SystemNetworkEvent.Should().Be(systemNetworkEvent);
            networkConnectivityEventArgs.NetworkAccessPrevious.Should().Be(networkAccessPrevious);
            networkConnectivityEventArgs.NetworkAccess.Should().Be(networkAccess);
            hasNetworkAccessChanged.Should().Be(networkConnectivityEventArgs.NetworkAccessPrevious != networkConnectivityEventArgs.NetworkAccess);
            testOutput.Should().Be($"NetworkEventID={networkId},NetworkAccessPrevious={networkConnectivityEventArgs.NetworkAccessPrevious},NetworkAccess={networkConnectivityEventArgs.NetworkAccess},NetworkChangeEvent={networkConnectivityEventArgs.NetworkChangeEvent.ToFormattedString()},NetworkChangeEventPrevious=[None],IsNetworkChangeEventSameAsPrevious=False,NetworkChangeEventChangesFromPrevious={networkConnectivityEventArgs.NetworkChangeEvent.ToFormattedString()},SystemNetworkEvent={systemNetworkEvent},CreatedTimestamp={networkConnectivityEventArgs.CreatedTimestamp.ToUniversalTimeMilliseconds()},HasNetworkConnectivityChanged=False,HasNetworkAvailabilityChanged=False,HasPreferredNetworkInterfaceChanged=False");
        }

        /// <summary>
        /// Defines the test method NetworkConnectivity_NetworkAccess.
        /// </summary>
        [Fact]
        public void NetworkConnectivity_CoreNetworkConnectivityEventArgs_ToString_ConnectionProfiles()
        {
            var networkId = Guid.NewGuid();

            // TODO: Figure out correct timeout.
            CancellationTokenSource cts = new CancellationTokenSource();

            var networkConnectivityEventArgs = new TestCoreNetworkConnectivityEventArgs(this.TestNetworkingSystem, CoreSystemNetworkEvent.Unknown, CoreNetworkAccess.Unknown, CoreNetworkAccess.Local, true, cts.Token, networkId);

            string? testOutput = networkConnectivityEventArgs.ToStringWithParentsPropName();
            this.TestOutputHelper.WriteLine(testOutput);

            networkConnectivityEventArgs.NetworkEventID.Should().Be(networkId);
            networkConnectivityEventArgs.SystemNetworkEvent.Should().Be(CoreSystemNetworkEvent.Unknown);
            networkConnectivityEventArgs.NetworkAccessPrevious.Should().Be(CoreNetworkAccess.Unknown);
            networkConnectivityEventArgs.NetworkAccess.Should().Be(CoreNetworkAccess.Local);
            testOutput.Should().Be($"NetworkEventID={networkId},NetworkAccessPrevious=Unknown,NetworkAccess=Local,NetworkChangeEvent=[NetworkAvailability,LocalNetworkAvailable,InternetAvailable],NetworkChangeEventPrevious=[None],IsNetworkChangeEventSameAsPrevious=False,NetworkChangeEventChangesFromPrevious=[NetworkAvailability,LocalNetworkAvailable,InternetAvailable],SystemNetworkEvent=Unknown,CreatedTimestamp={networkConnectivityEventArgs.CreatedTimestamp.ToUniversalTimeMilliseconds()},HasNetworkConnectivityChanged=False,HasNetworkAvailabilityChanged=False,HasPreferredNetworkInterfaceChanged=False");
        }

        private class TestCoreNetworkConnectivityEventArgs : CoreNetworkConnectivityEventArgs
        {
            private readonly CoreNetworkAccess networkAccessNew;

            public TestCoreNetworkConnectivityEventArgs(ICoreNetworkingSystem networkingSystem, CoreSystemNetworkEvent systemNetworkEvent, CoreNetworkAccess networkAccessPrevious, CoreNetworkAccess networkAccess, bool isNetworkAvailable, CancellationToken ctx = default, Guid? networkEventId = null)
                : base(networkingSystem, systemNetworkEvent, isNetworkAvailable ? CoreNetworkChangeEvent.InternetAvailable : CoreNetworkChangeEvent.NoNetworkAvailable, CoreNetworkChangeEvent.None, networkAccessPrevious, ctx, networkEventId)
            {
                this.networkAccessNew = networkAccess;
            }

            protected override CoreNetworkAccess GetPlatformNetworkAccess() => this.networkAccessNew;
        }
#endif
    }
}

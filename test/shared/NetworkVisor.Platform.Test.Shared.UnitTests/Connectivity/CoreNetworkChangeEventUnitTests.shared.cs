// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// // ***********************************************************************
// <copyright file="CoreNetworkChangeEventUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.Networking.Connectivity;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Networking.NetworkingSystem;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Connectivity
{
    /// <summary>
    /// Class CoreNetworkChangeEventUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreNetworkChangeEventUnitTests))]

    public class CoreNetworkChangeEventUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkChangeEventUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNetworkChangeEventUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method NetworkConnectivity_NetworkAccess.
        /// </summary>
        /// <param name="networkChangeEventInitial">Initial Network Change event.</param>
        /// <param name="networkAccessPrevious">Previous network access.</param>
        /// <param name="networkAccess">Current network access.</param>
        /// <param name="networkChangeEventExpected">Expected network change event.</param>
        [Theory]
        [InlineData(CoreNetworkChangeEvent.None, CoreNetworkAccess.Unknown, CoreNetworkAccess.Unknown, CoreNetworkChangeEvent.None)]
        [InlineData(CoreNetworkChangeEvent.None, CoreNetworkAccess.Unknown, CoreNetworkAccess.None, CoreNetworkChangeEvent.NoNetworkAvailableEvent | CoreNetworkChangeEvent.ChangedNetworkAvailabilityEvent)]
        [InlineData(CoreNetworkChangeEvent.None, CoreNetworkAccess.Unknown, CoreNetworkAccess.Local, CoreNetworkChangeEvent.LocalNetworkAvailableEvent | CoreNetworkChangeEvent.ChangedNetworkAvailabilityEvent)]
        [InlineData(CoreNetworkChangeEvent.None, CoreNetworkAccess.Unknown, CoreNetworkAccess.ConstrainedInternet, CoreNetworkChangeEvent.ConstrainedInternetAvailableEvent | CoreNetworkChangeEvent.ChangedNetworkAvailabilityEvent)]
        [InlineData(CoreNetworkChangeEvent.None, CoreNetworkAccess.Unknown, CoreNetworkAccess.Internet, CoreNetworkChangeEvent.InternetAvailableEvent | CoreNetworkChangeEvent.ChangedNetworkAvailabilityEvent)]

        [InlineData(CoreNetworkChangeEvent.None, CoreNetworkAccess.None, CoreNetworkAccess.Unknown, CoreNetworkChangeEvent.UnknownNetworkAvailabilityEvent | CoreNetworkChangeEvent.ChangedNetworkAvailabilityEvent)]
        [InlineData(CoreNetworkChangeEvent.None, CoreNetworkAccess.None, CoreNetworkAccess.None, CoreNetworkChangeEvent.None)]
        [InlineData(CoreNetworkChangeEvent.None, CoreNetworkAccess.None, CoreNetworkAccess.Local, CoreNetworkChangeEvent.LocalNetworkAvailableEvent | CoreNetworkChangeEvent.ChangedNetworkAvailabilityEvent)]
        [InlineData(CoreNetworkChangeEvent.None, CoreNetworkAccess.None, CoreNetworkAccess.ConstrainedInternet, CoreNetworkChangeEvent.ConstrainedInternetAvailableEvent | CoreNetworkChangeEvent.ChangedNetworkAvailabilityEvent)]
        [InlineData(CoreNetworkChangeEvent.None, CoreNetworkAccess.None, CoreNetworkAccess.Internet, CoreNetworkChangeEvent.InternetAvailableEvent | CoreNetworkChangeEvent.ChangedNetworkAvailabilityEvent)]

        [InlineData(CoreNetworkChangeEvent.None, CoreNetworkAccess.Local, CoreNetworkAccess.Unknown, CoreNetworkChangeEvent.UnknownNetworkAvailabilityEvent | CoreNetworkChangeEvent.ChangedNetworkAvailabilityEvent)]
        [InlineData(CoreNetworkChangeEvent.None, CoreNetworkAccess.Local, CoreNetworkAccess.None, CoreNetworkChangeEvent.NoNetworkAvailableEvent | CoreNetworkChangeEvent.ChangedNetworkAvailabilityEvent)]
        [InlineData(CoreNetworkChangeEvent.None, CoreNetworkAccess.Local, CoreNetworkAccess.Local, CoreNetworkChangeEvent.None)]
        [InlineData(CoreNetworkChangeEvent.None, CoreNetworkAccess.Local, CoreNetworkAccess.ConstrainedInternet, CoreNetworkChangeEvent.ConstrainedInternetAvailableEvent | CoreNetworkChangeEvent.ChangedNetworkAvailabilityEvent)]
        [InlineData(CoreNetworkChangeEvent.None, CoreNetworkAccess.Local, CoreNetworkAccess.Internet, CoreNetworkChangeEvent.InternetAvailableEvent | CoreNetworkChangeEvent.ChangedNetworkAvailabilityEvent)]

        [InlineData(CoreNetworkChangeEvent.None, CoreNetworkAccess.ConstrainedInternet, CoreNetworkAccess.Unknown, CoreNetworkChangeEvent.UnknownNetworkAvailabilityEvent | CoreNetworkChangeEvent.ChangedNetworkAvailabilityEvent)]
        [InlineData(CoreNetworkChangeEvent.None, CoreNetworkAccess.ConstrainedInternet, CoreNetworkAccess.None, CoreNetworkChangeEvent.NoNetworkAvailableEvent | CoreNetworkChangeEvent.ChangedNetworkAvailabilityEvent)]
        [InlineData(CoreNetworkChangeEvent.None, CoreNetworkAccess.ConstrainedInternet, CoreNetworkAccess.Local, CoreNetworkChangeEvent.LocalNetworkAvailableEvent | CoreNetworkChangeEvent.ChangedNetworkAvailabilityEvent)]
        [InlineData(CoreNetworkChangeEvent.None, CoreNetworkAccess.ConstrainedInternet, CoreNetworkAccess.ConstrainedInternet, CoreNetworkChangeEvent.None)]
        [InlineData(CoreNetworkChangeEvent.None, CoreNetworkAccess.ConstrainedInternet, CoreNetworkAccess.Internet, CoreNetworkChangeEvent.InternetAvailableEvent | CoreNetworkChangeEvent.ChangedNetworkAvailabilityEvent)]

        [InlineData(CoreNetworkChangeEvent.None, CoreNetworkAccess.Internet, CoreNetworkAccess.Unknown, CoreNetworkChangeEvent.UnknownNetworkAvailabilityEvent | CoreNetworkChangeEvent.ChangedNetworkAvailabilityEvent)]
        [InlineData(CoreNetworkChangeEvent.None, CoreNetworkAccess.Internet, CoreNetworkAccess.None, CoreNetworkChangeEvent.NoNetworkAvailableEvent | CoreNetworkChangeEvent.ChangedNetworkAvailabilityEvent)]
        [InlineData(CoreNetworkChangeEvent.None, CoreNetworkAccess.Internet, CoreNetworkAccess.Local, CoreNetworkChangeEvent.LocalNetworkAvailableEvent | CoreNetworkChangeEvent.ChangedNetworkAvailabilityEvent)]
        [InlineData(CoreNetworkChangeEvent.None, CoreNetworkAccess.Internet, CoreNetworkAccess.ConstrainedInternet, CoreNetworkChangeEvent.ConstrainedInternetAvailableEvent | CoreNetworkChangeEvent.ChangedNetworkAvailabilityEvent)]
        [InlineData(CoreNetworkChangeEvent.None, CoreNetworkAccess.Internet, CoreNetworkAccess.Internet, CoreNetworkChangeEvent.None)]

        [InlineData(CoreNetworkChangeEvent.NetworkingSystemInitialize, CoreNetworkAccess.Unknown, CoreNetworkAccess.Unknown, CoreNetworkChangeEvent.NetworkingSystemInitialize | CoreNetworkChangeEvent.UnknownNetworkAvailabilityEvent)]
        [InlineData(CoreNetworkChangeEvent.NetworkingSystemInitialize, CoreNetworkAccess.Unknown, CoreNetworkAccess.None, CoreNetworkChangeEvent.NetworkingSystemInitialize | CoreNetworkChangeEvent.NoNetworkAvailableEvent)]
        [InlineData(CoreNetworkChangeEvent.NetworkingSystemInitialize, CoreNetworkAccess.Unknown, CoreNetworkAccess.Local, CoreNetworkChangeEvent.NetworkingSystemInitialize | CoreNetworkChangeEvent.LocalNetworkAvailableEvent)]
        [InlineData(CoreNetworkChangeEvent.NetworkingSystemInitialize, CoreNetworkAccess.Unknown, CoreNetworkAccess.ConstrainedInternet, CoreNetworkChangeEvent.NetworkingSystemInitialize | CoreNetworkChangeEvent.ConstrainedInternetAvailableEvent)]
        [InlineData(CoreNetworkChangeEvent.NetworkingSystemInitialize, CoreNetworkAccess.Unknown, CoreNetworkAccess.Internet, CoreNetworkChangeEvent.NetworkingSystemInitialize | CoreNetworkChangeEvent.InternetAvailableEvent)]

        [InlineData(CoreNetworkChangeEvent.NetworkingSystemInitialize, CoreNetworkAccess.None, CoreNetworkAccess.Unknown, CoreNetworkChangeEvent.NetworkingSystemInitialize | CoreNetworkChangeEvent.UnknownNetworkAvailabilityEvent)]
        [InlineData(CoreNetworkChangeEvent.NetworkingSystemInitialize, CoreNetworkAccess.None, CoreNetworkAccess.None, CoreNetworkChangeEvent.NetworkingSystemInitialize | CoreNetworkChangeEvent.NoNetworkAvailableEvent)]
        [InlineData(CoreNetworkChangeEvent.NetworkingSystemInitialize, CoreNetworkAccess.None, CoreNetworkAccess.Local, CoreNetworkChangeEvent.NetworkingSystemInitialize | CoreNetworkChangeEvent.LocalNetworkAvailableEvent)]
        [InlineData(CoreNetworkChangeEvent.NetworkingSystemInitialize, CoreNetworkAccess.None, CoreNetworkAccess.ConstrainedInternet, CoreNetworkChangeEvent.NetworkingSystemInitialize | CoreNetworkChangeEvent.ConstrainedInternetAvailableEvent)]
        [InlineData(CoreNetworkChangeEvent.NetworkingSystemInitialize, CoreNetworkAccess.None, CoreNetworkAccess.Internet, CoreNetworkChangeEvent.NetworkingSystemInitialize | CoreNetworkChangeEvent.InternetAvailableEvent)]

        [InlineData(CoreNetworkChangeEvent.NetworkingSystemInitialize, CoreNetworkAccess.Local, CoreNetworkAccess.Unknown, CoreNetworkChangeEvent.NetworkingSystemInitialize | CoreNetworkChangeEvent.UnknownNetworkAvailabilityEvent)]
        [InlineData(CoreNetworkChangeEvent.NetworkingSystemInitialize, CoreNetworkAccess.Local, CoreNetworkAccess.None, CoreNetworkChangeEvent.NetworkingSystemInitialize | CoreNetworkChangeEvent.NoNetworkAvailableEvent)]
        [InlineData(CoreNetworkChangeEvent.NetworkingSystemInitialize, CoreNetworkAccess.Local, CoreNetworkAccess.Local, CoreNetworkChangeEvent.NetworkingSystemInitialize | CoreNetworkChangeEvent.LocalNetworkAvailableEvent)]
        [InlineData(CoreNetworkChangeEvent.NetworkingSystemInitialize, CoreNetworkAccess.Local, CoreNetworkAccess.ConstrainedInternet, CoreNetworkChangeEvent.NetworkingSystemInitialize | CoreNetworkChangeEvent.ConstrainedInternetAvailableEvent)]
        [InlineData(CoreNetworkChangeEvent.NetworkingSystemInitialize, CoreNetworkAccess.Local, CoreNetworkAccess.Internet, CoreNetworkChangeEvent.NetworkingSystemInitialize | CoreNetworkChangeEvent.InternetAvailableEvent)]

        [InlineData(CoreNetworkChangeEvent.NetworkingSystemInitialize, CoreNetworkAccess.ConstrainedInternet, CoreNetworkAccess.Unknown, CoreNetworkChangeEvent.NetworkingSystemInitialize | CoreNetworkChangeEvent.UnknownNetworkAvailabilityEvent)]
        [InlineData(CoreNetworkChangeEvent.NetworkingSystemInitialize, CoreNetworkAccess.ConstrainedInternet, CoreNetworkAccess.None, CoreNetworkChangeEvent.NetworkingSystemInitialize | CoreNetworkChangeEvent.NoNetworkAvailableEvent)]
        [InlineData(CoreNetworkChangeEvent.NetworkingSystemInitialize, CoreNetworkAccess.ConstrainedInternet, CoreNetworkAccess.Local, CoreNetworkChangeEvent.NetworkingSystemInitialize | CoreNetworkChangeEvent.LocalNetworkAvailableEvent)]
        [InlineData(CoreNetworkChangeEvent.NetworkingSystemInitialize, CoreNetworkAccess.ConstrainedInternet, CoreNetworkAccess.ConstrainedInternet, CoreNetworkChangeEvent.NetworkingSystemInitialize | CoreNetworkChangeEvent.ConstrainedInternetAvailableEvent)]
        [InlineData(CoreNetworkChangeEvent.NetworkingSystemInitialize, CoreNetworkAccess.ConstrainedInternet, CoreNetworkAccess.Internet, CoreNetworkChangeEvent.NetworkingSystemInitialize | CoreNetworkChangeEvent.InternetAvailableEvent)]

        [InlineData(CoreNetworkChangeEvent.NetworkingSystemInitialize, CoreNetworkAccess.Internet, CoreNetworkAccess.Unknown, CoreNetworkChangeEvent.NetworkingSystemInitialize | CoreNetworkChangeEvent.UnknownNetworkAvailabilityEvent)]
        [InlineData(CoreNetworkChangeEvent.NetworkingSystemInitialize, CoreNetworkAccess.Internet, CoreNetworkAccess.None, CoreNetworkChangeEvent.NetworkingSystemInitialize | CoreNetworkChangeEvent.NoNetworkAvailableEvent)]
        [InlineData(CoreNetworkChangeEvent.NetworkingSystemInitialize, CoreNetworkAccess.Internet, CoreNetworkAccess.Local, CoreNetworkChangeEvent.NetworkingSystemInitialize | CoreNetworkChangeEvent.LocalNetworkAvailableEvent)]
        [InlineData(CoreNetworkChangeEvent.NetworkingSystemInitialize, CoreNetworkAccess.Internet, CoreNetworkAccess.ConstrainedInternet, CoreNetworkChangeEvent.NetworkingSystemInitialize | CoreNetworkChangeEvent.ConstrainedInternetAvailableEvent)]
        [InlineData(CoreNetworkChangeEvent.NetworkingSystemInitialize, CoreNetworkAccess.Internet, CoreNetworkAccess.Internet, CoreNetworkChangeEvent.NetworkingSystemInitialize | CoreNetworkChangeEvent.InternetAvailableEvent)]
        public void NetworkChangeEvent_Validation(CoreNetworkChangeEvent networkChangeEventInitial, CoreNetworkAccess networkAccessPrevious, CoreNetworkAccess networkAccess, CoreNetworkChangeEvent networkChangeEventExpected)
        {
            CoreNetworkChangeEvent networkChangeEvent = CoreNetworkingSystemBase.NetworkChangeEventFromNetworkAccessChange(networkChangeEventInitial, networkAccess, networkAccessPrevious);

            networkChangeEvent.IsNetworkingSystemInitializeEvent().Should().Be(networkChangeEvent.HasFlag(CoreNetworkChangeEvent.NetworkingSystemInitialize));
            networkChangeEvent.IsNetworkAvailable().Should().Be(!networkChangeEvent.HasFlag(CoreNetworkChangeEvent.NoNetworkAvailable));
            networkChangeEvent.IsLocalNetworkAvailable().Should().Be(networkChangeEvent.HasFlag(CoreNetworkChangeEvent.LocalNetworkAvailable));
            networkChangeEvent.IsConstrainedInternetAvailable().Should().Be(networkChangeEvent.HasFlag(CoreNetworkChangeEvent.ConstrainedInternetAvailable));
            networkChangeEvent.IsInternetAvailable().Should().Be(networkChangeEvent.HasFlag(CoreNetworkChangeEvent.InternetAvailable));
            networkChangeEvent.IsNone().Should().Be(networkChangeEvent == CoreNetworkChangeEvent.None);
            networkChangeEvent.HasConnectionProfileChanged().Should().Be(networkChangeEvent.HasFlag(CoreNetworkChangeEvent.ConnectionProfileChanged));
            networkChangeEvent.HasNetworkAvailabilityChanged().Should().Be((networkChangeEvent & (CoreNetworkChangeEvent.ChangedNetworkAvailability | CoreNetworkChangeEvent.PreferredNetworkInterfaceChange)) != 0);
            networkChangeEvent.HasNetworkInterfaceChanged().Should().Be((networkChangeEvent & (CoreNetworkChangeEvent.NetworkInterfaceAddressInfoChanged | CoreNetworkChangeEvent.NetworkInterfaceOperationalChange)) != 0);
            networkChangeEvent.HasPreferredInterfaceOrNetworkChanged().Should().Be((networkChangeEvent & (CoreNetworkChangeEvent.PreferredNetworkInterfaceChange | CoreNetworkChangeEvent.SwitchedNetworks)) != 0);

            this.TestOutputHelper.WriteLine($"NetworkChangeEvent: [{networkChangeEvent.ToFormattedString()}]");
            networkChangeEvent.Should().Be(networkChangeEventExpected);
        }

        [Theory]
        [InlineData(CoreNetworkChangeEvent.None, CoreNetworkChangeEvent.None, CoreNetworkChangeEvent.None)]
        [InlineData(CoreNetworkChangeEvent.None, CoreNetworkChangeEvent.NetworkingSystemInitialize, CoreNetworkChangeEvent.NetworkingSystemInitialize)]

        [InlineData(CoreNetworkChangeEvent.NetworkingSystemInitialize, CoreNetworkChangeEvent.NetworkingSystemInitialize, CoreNetworkChangeEvent.None)]
        [InlineData(CoreNetworkChangeEvent.ChangedNetworkAvailabilityEvent, CoreNetworkChangeEvent.ChangedNetworkAvailabilityEvent, CoreNetworkChangeEvent.None)]
        [InlineData(CoreNetworkChangeEvent.UnknownNetworkAvailabilityEvent, CoreNetworkChangeEvent.UnknownNetworkAvailabilityEvent, CoreNetworkChangeEvent.None)]
        [InlineData(CoreNetworkChangeEvent.NoNetworkAvailableEvent, CoreNetworkChangeEvent.NoNetworkAvailableEvent, CoreNetworkChangeEvent.None)]
        [InlineData(CoreNetworkChangeEvent.LocalNetworkAvailableEvent, CoreNetworkChangeEvent.LocalNetworkAvailableEvent, CoreNetworkChangeEvent.None)]
        [InlineData(CoreNetworkChangeEvent.ConstrainedInternetAvailableEvent, CoreNetworkChangeEvent.ConstrainedInternetAvailableEvent, CoreNetworkChangeEvent.None)]
        [InlineData(CoreNetworkChangeEvent.InternetAvailableEvent, CoreNetworkChangeEvent.InternetAvailableEvent, CoreNetworkChangeEvent.None)]
        [InlineData(CoreNetworkChangeEvent.SwitchedNetworksEvent, CoreNetworkChangeEvent.SwitchedNetworksEvent, CoreNetworkChangeEvent.None)]
        [InlineData(CoreNetworkChangeEvent.ConnectionProfileChangedEvent, CoreNetworkChangeEvent.ConnectionProfileChangedEvent, CoreNetworkChangeEvent.None)]
        [InlineData(CoreNetworkChangeEvent.SwitchedAccessPointsEvent, CoreNetworkChangeEvent.SwitchedAccessPointsEvent, CoreNetworkChangeEvent.None)]
        [InlineData(CoreNetworkChangeEvent.PreferredNetworkInterfaceChangeEvent, CoreNetworkChangeEvent.PreferredNetworkInterfaceChangeEvent, CoreNetworkChangeEvent.None)]
        [InlineData(CoreNetworkChangeEvent.NetworkInterfaceAddressInfoChangedEvent, CoreNetworkChangeEvent.NetworkInterfaceAddressInfoChangedEvent, CoreNetworkChangeEvent.None)]
        [InlineData(CoreNetworkChangeEvent.NetworkInterfaceAddedEvent, CoreNetworkChangeEvent.NetworkInterfaceAddedEvent, CoreNetworkChangeEvent.None)]
        [InlineData(CoreNetworkChangeEvent.NetworkInterfaceRemovedEvent, CoreNetworkChangeEvent.NetworkInterfaceRemovedEvent, CoreNetworkChangeEvent.None)]
        [InlineData(CoreNetworkChangeEvent.NetworkInterfaceOperationalChangedToUpEvent, CoreNetworkChangeEvent.NetworkInterfaceOperationalChangedToUpEvent, CoreNetworkChangeEvent.None)]
        [InlineData(CoreNetworkChangeEvent.NetworkInterfaceOperationalChangedToDownEvent, CoreNetworkChangeEvent.NetworkInterfaceOperationalChangedToDownEvent, CoreNetworkChangeEvent.None)]
        [InlineData(CoreNetworkChangeEvent.NetworkSignalStrengthChangeEvent, CoreNetworkChangeEvent.NetworkSignalStrengthChangeEvent, CoreNetworkChangeEvent.None)]
        [InlineData(CoreNetworkChangeEvent.NetworkBandwidthChangeEvent, CoreNetworkChangeEvent.NetworkBandwidthChangeEvent, CoreNetworkChangeEvent.None)]
        [InlineData(CoreNetworkChangeEvent.NetworkSpeedChangeEvent, CoreNetworkChangeEvent.NetworkSpeedChangeEvent, CoreNetworkChangeEvent.None)]
        [InlineData(CoreNetworkChangeEvent.ChangedNetworkAvailability, CoreNetworkChangeEvent.ChangedNetworkAvailabilityEvent, CoreNetworkChangeEvent.NetworkAvailability)]
        [InlineData(CoreNetworkChangeEvent.UnknownNetworkAvailability, CoreNetworkChangeEvent.UnknownNetworkAvailabilityEvent, CoreNetworkChangeEvent.NetworkAvailability)]
        [InlineData(CoreNetworkChangeEvent.NoNetworkAvailable, CoreNetworkChangeEvent.NoNetworkAvailableEvent, CoreNetworkChangeEvent.NetworkAvailability)]
        [InlineData(CoreNetworkChangeEvent.LocalNetworkAvailable, CoreNetworkChangeEvent.LocalNetworkAvailableEvent, CoreNetworkChangeEvent.NetworkAvailability)]
        [InlineData(CoreNetworkChangeEvent.ConstrainedInternetAvailable, CoreNetworkChangeEvent.ConstrainedInternetAvailableEvent, CoreNetworkChangeEvent.LocalNetworkAvailableEvent)]
        [InlineData(CoreNetworkChangeEvent.InternetAvailable, CoreNetworkChangeEvent.InternetAvailableEvent, CoreNetworkChangeEvent.LocalNetworkAvailableEvent)]
        [InlineData(CoreNetworkChangeEvent.SwitchedNetworks, CoreNetworkChangeEvent.SwitchedNetworksEvent, CoreNetworkChangeEvent.NetworkAddressChanged | CoreNetworkChangeEvent.ChangedNetworkAvailabilityEvent)]
        [InlineData(CoreNetworkChangeEvent.ConnectionProfileChanged, CoreNetworkChangeEvent.ConnectionProfileChangedEvent, CoreNetworkChangeEvent.NetworkAddressChanged | CoreNetworkChangeEvent.ChangedNetworkAvailabilityEvent)]
        [InlineData(CoreNetworkChangeEvent.SwitchedAccessPoints, CoreNetworkChangeEvent.SwitchedAccessPointsEvent, CoreNetworkChangeEvent.NetworkAddressChanged)]
        [InlineData(CoreNetworkChangeEvent.PreferredNetworkInterfaceChange, CoreNetworkChangeEvent.PreferredNetworkInterfaceChangeEvent, CoreNetworkChangeEvent.NetworkAddressChanged)]
        [InlineData(CoreNetworkChangeEvent.NetworkInterfaceAddressInfoChanged, CoreNetworkChangeEvent.NetworkInterfaceAddressInfoChangedEvent, CoreNetworkChangeEvent.NetworkAddressChanged | CoreNetworkChangeEvent.ConnectionProfileChanged)]
        [InlineData(CoreNetworkChangeEvent.NetworkInterfaceAdded, CoreNetworkChangeEvent.NetworkInterfaceAddedEvent, CoreNetworkChangeEvent.ConnectionProfileChanged)]
        [InlineData(CoreNetworkChangeEvent.NetworkInterfaceRemoved, CoreNetworkChangeEvent.NetworkInterfaceRemovedEvent, CoreNetworkChangeEvent.ConnectionProfileChanged)]
        [InlineData(CoreNetworkChangeEvent.NetworkInterfaceOperationalChangedToUp, CoreNetworkChangeEvent.NetworkInterfaceOperationalChangedToUpEvent, CoreNetworkChangeEvent.NetworkInterfaceOperationalChange | CoreNetworkChangeEvent.NetworkInterfaceAddressInfoChangedEvent)]
        [InlineData(CoreNetworkChangeEvent.NetworkInterfaceOperationalChangedToDown, CoreNetworkChangeEvent.NetworkInterfaceOperationalChangedToDownEvent, CoreNetworkChangeEvent.NetworkInterfaceOperationalChange | CoreNetworkChangeEvent.NetworkInterfaceAddressInfoChangedEvent)]
        [InlineData(CoreNetworkChangeEvent.NetworkSignalStrengthChange, CoreNetworkChangeEvent.NetworkSignalStrengthChangeEvent, CoreNetworkChangeEvent.NetworkQualityChanged)]
        [InlineData(CoreNetworkChangeEvent.NetworkBandwidthChange, CoreNetworkChangeEvent.NetworkBandwidthChangeEvent, CoreNetworkChangeEvent.NetworkQualityChanged)]
        [InlineData(CoreNetworkChangeEvent.NetworkSpeedChange, CoreNetworkChangeEvent.NetworkSpeedChangeEvent, CoreNetworkChangeEvent.NetworkQualityChanged)]

        [InlineData(CoreNetworkChangeEvent.NoNetworkAvailableEvent, CoreNetworkChangeEvent.InternetAvailableEvent, CoreNetworkChangeEvent.LocalNetworkAvailable | CoreNetworkChangeEvent.InternetAvailable)]
        [InlineData(CoreNetworkChangeEvent.LocalNetworkAvailableEvent, CoreNetworkChangeEvent.InternetAvailableEvent, CoreNetworkChangeEvent.InternetAvailable)]
        [InlineData(CoreNetworkChangeEvent.InternetAvailableEvent, CoreNetworkChangeEvent.LocalNetworkAvailableEvent, CoreNetworkChangeEvent.None)]

        public void NetworkChangeEvent_CalculateNetworkChangeEventUpdates(CoreNetworkChangeEvent networkChangeEventPrevious, CoreNetworkChangeEvent networkChangeEventCurrent, CoreNetworkChangeEvent networkChangeEventExpected)
        {
            CoreNetworkConnectivityEventArgs.CalculateNetworkChangeEventUpdates(networkChangeEventCurrent, networkChangeEventPrevious).Should().Be(networkChangeEventExpected);
        }

        [Theory]
        [InlineData(CoreConnectivityEventSource.Unknown, CoreNetworkChangeEvent.None, CoreNetworkChangeEvent.None, false)]
        [InlineData(CoreConnectivityEventSource.Unknown, CoreNetworkChangeEvent.None, CoreNetworkChangeEvent.NetworkingSystemInitialize, false)]

        [InlineData(CoreConnectivityEventSource.InitializePreferredNetworkInterface, CoreNetworkChangeEvent.None, CoreNetworkChangeEvent.None, false)]
        [InlineData(CoreConnectivityEventSource.InitializePreferredNetworkInterface, CoreNetworkChangeEvent.None, CoreNetworkChangeEvent.NetworkingSystemInitialize, false)]

        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.NetworkingSystemInitialize, CoreNetworkChangeEvent.NetworkingSystemInitialize, false)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.ChangedNetworkAvailabilityEvent, CoreNetworkChangeEvent.ChangedNetworkAvailabilityEvent, false)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.UnknownNetworkAvailabilityEvent, CoreNetworkChangeEvent.UnknownNetworkAvailabilityEvent, false)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.NoNetworkAvailableEvent, CoreNetworkChangeEvent.NoNetworkAvailableEvent, false)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.LocalNetworkAvailableEvent, CoreNetworkChangeEvent.LocalNetworkAvailableEvent, false)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.ConstrainedInternetAvailableEvent, CoreNetworkChangeEvent.ConstrainedInternetAvailableEvent, false)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.InternetAvailableEvent, CoreNetworkChangeEvent.InternetAvailableEvent, false)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.SwitchedNetworksEvent, CoreNetworkChangeEvent.SwitchedNetworksEvent, true)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.ConnectionProfileChangedEvent, CoreNetworkChangeEvent.ConnectionProfileChangedEvent, false)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.SwitchedAccessPointsEvent, CoreNetworkChangeEvent.SwitchedAccessPointsEvent, false)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.PreferredNetworkInterfaceChangeEvent, CoreNetworkChangeEvent.PreferredNetworkInterfaceChangeEvent, true)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.NetworkInterfaceAddressInfoChangedEvent, CoreNetworkChangeEvent.NetworkInterfaceAddressInfoChangedEvent, false)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.NetworkInterfaceAddedEvent, CoreNetworkChangeEvent.NetworkInterfaceAddedEvent, false)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.NetworkInterfaceRemovedEvent, CoreNetworkChangeEvent.NetworkInterfaceRemovedEvent, false)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.NetworkInterfaceOperationalChangedToUpEvent, CoreNetworkChangeEvent.NetworkInterfaceOperationalChangedToUpEvent, false)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.NetworkInterfaceOperationalChangedToDownEvent, CoreNetworkChangeEvent.NetworkInterfaceOperationalChangedToDownEvent, false)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.NetworkSignalStrengthChangeEvent, CoreNetworkChangeEvent.NetworkSignalStrengthChangeEvent, false)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.NetworkBandwidthChangeEvent, CoreNetworkChangeEvent.NetworkBandwidthChangeEvent, false)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.NetworkSpeedChangeEvent, CoreNetworkChangeEvent.NetworkSpeedChangeEvent, false)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.ChangedNetworkAvailability, CoreNetworkChangeEvent.ChangedNetworkAvailabilityEvent, true)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.UnknownNetworkAvailability, CoreNetworkChangeEvent.UnknownNetworkAvailabilityEvent, true)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.NoNetworkAvailable, CoreNetworkChangeEvent.NoNetworkAvailableEvent, true)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.LocalNetworkAvailable, CoreNetworkChangeEvent.LocalNetworkAvailableEvent, true)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.ConstrainedInternetAvailable, CoreNetworkChangeEvent.ConstrainedInternetAvailableEvent, true)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.InternetAvailable, CoreNetworkChangeEvent.InternetAvailableEvent, true)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.SwitchedNetworks, CoreNetworkChangeEvent.SwitchedNetworksEvent, true)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.ConnectionProfileChanged, CoreNetworkChangeEvent.ConnectionProfileChangedEvent, true)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.SwitchedAccessPoints, CoreNetworkChangeEvent.SwitchedAccessPointsEvent, true)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.PreferredNetworkInterfaceChange, CoreNetworkChangeEvent.PreferredNetworkInterfaceChangeEvent, true)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.NetworkInterfaceAddressInfoChanged, CoreNetworkChangeEvent.NetworkInterfaceAddressInfoChangedEvent, true)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.NetworkInterfaceAdded, CoreNetworkChangeEvent.NetworkInterfaceAddedEvent, true)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.NetworkInterfaceRemoved, CoreNetworkChangeEvent.NetworkInterfaceRemovedEvent, true)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.NetworkInterfaceOperationalChangedToUp, CoreNetworkChangeEvent.NetworkInterfaceOperationalChangedToUpEvent, true)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.NetworkInterfaceOperationalChangedToDown, CoreNetworkChangeEvent.NetworkInterfaceOperationalChangedToDownEvent, true)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.NetworkSignalStrengthChange, CoreNetworkChangeEvent.NetworkSignalStrengthChangeEvent, true)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.NetworkBandwidthChange, CoreNetworkChangeEvent.NetworkBandwidthChangeEvent, true)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.NetworkSpeedChange, CoreNetworkChangeEvent.NetworkSpeedChangeEvent, true)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.NoNetworkAvailableEvent, CoreNetworkChangeEvent.InternetAvailableEvent, true)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.LocalNetworkAvailableEvent, CoreNetworkChangeEvent.InternetAvailableEvent, true)]
        [InlineData(CoreConnectivityEventSource.NetworkAddressChangeEvent, CoreNetworkChangeEvent.InternetAvailableEvent, CoreNetworkChangeEvent.LocalNetworkAvailableEvent, false)]

        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.ChangedNetworkAvailabilityEvent, CoreNetworkChangeEvent.ChangedNetworkAvailabilityEvent, false)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.UnknownNetworkAvailabilityEvent, CoreNetworkChangeEvent.UnknownNetworkAvailabilityEvent, false)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.NoNetworkAvailableEvent, CoreNetworkChangeEvent.NoNetworkAvailableEvent, false)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.LocalNetworkAvailableEvent, CoreNetworkChangeEvent.LocalNetworkAvailableEvent, false)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.ConstrainedInternetAvailableEvent, CoreNetworkChangeEvent.ConstrainedInternetAvailableEvent, false)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.InternetAvailableEvent, CoreNetworkChangeEvent.InternetAvailableEvent, false)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.SwitchedNetworksEvent, CoreNetworkChangeEvent.SwitchedNetworksEvent, true)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.ConnectionProfileChangedEvent, CoreNetworkChangeEvent.ConnectionProfileChangedEvent, false)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.SwitchedAccessPointsEvent, CoreNetworkChangeEvent.SwitchedAccessPointsEvent, false)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.PreferredNetworkInterfaceChangeEvent, CoreNetworkChangeEvent.PreferredNetworkInterfaceChangeEvent, true)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.NetworkInterfaceAddressInfoChangedEvent, CoreNetworkChangeEvent.NetworkInterfaceAddressInfoChangedEvent, true)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.NetworkInterfaceAddedEvent, CoreNetworkChangeEvent.NetworkInterfaceAddedEvent, false)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.NetworkInterfaceRemovedEvent, CoreNetworkChangeEvent.NetworkInterfaceRemovedEvent, false)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.NetworkInterfaceOperationalChangedToUpEvent, CoreNetworkChangeEvent.NetworkInterfaceOperationalChangedToUpEvent, true)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.NetworkInterfaceOperationalChangedToDownEvent, CoreNetworkChangeEvent.NetworkInterfaceOperationalChangedToDownEvent, true)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.NetworkSignalStrengthChangeEvent, CoreNetworkChangeEvent.NetworkSignalStrengthChangeEvent, false)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.NetworkBandwidthChangeEvent, CoreNetworkChangeEvent.NetworkBandwidthChangeEvent, false)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.NetworkSpeedChangeEvent, CoreNetworkChangeEvent.NetworkSpeedChangeEvent, false)]

        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.NetworkingSystemInitialize, CoreNetworkChangeEvent.NetworkingSystemInitialize, false)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.ChangedNetworkAvailability, CoreNetworkChangeEvent.ChangedNetworkAvailabilityEvent, false)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.UnknownNetworkAvailability, CoreNetworkChangeEvent.UnknownNetworkAvailabilityEvent, false)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.NoNetworkAvailable, CoreNetworkChangeEvent.NoNetworkAvailableEvent, false)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.LocalNetworkAvailable, CoreNetworkChangeEvent.LocalNetworkAvailableEvent, false)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.ConstrainedInternetAvailable, CoreNetworkChangeEvent.ConstrainedInternetAvailableEvent, false)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.InternetAvailable, CoreNetworkChangeEvent.InternetAvailableEvent, false)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.SwitchedNetworks, CoreNetworkChangeEvent.SwitchedNetworksEvent, true)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.ConnectionProfileChanged, CoreNetworkChangeEvent.ConnectionProfileChangedEvent, false)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.SwitchedAccessPoints, CoreNetworkChangeEvent.SwitchedAccessPointsEvent, false)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.PreferredNetworkInterfaceChange, CoreNetworkChangeEvent.PreferredNetworkInterfaceChangeEvent, true)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.NetworkInterfaceAddressInfoChanged, CoreNetworkChangeEvent.NetworkInterfaceAddressInfoChangedEvent, true)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.NetworkInterfaceAdded, CoreNetworkChangeEvent.NetworkInterfaceAddedEvent, false)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.NetworkInterfaceRemoved, CoreNetworkChangeEvent.NetworkInterfaceRemovedEvent, false)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.NetworkInterfaceOperationalChangedToUp, CoreNetworkChangeEvent.NetworkInterfaceOperationalChangedToUpEvent, true)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.NetworkInterfaceOperationalChangedToDown, CoreNetworkChangeEvent.NetworkInterfaceOperationalChangedToDownEvent, true)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.NetworkSignalStrengthChange, CoreNetworkChangeEvent.NetworkSignalStrengthChangeEvent, false)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.NetworkBandwidthChange, CoreNetworkChangeEvent.NetworkBandwidthChangeEvent, false)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.NetworkSpeedChange, CoreNetworkChangeEvent.NetworkSpeedChangeEvent, false)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.NoNetworkAvailableEvent, CoreNetworkChangeEvent.InternetAvailableEvent, false)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.LocalNetworkAvailableEvent, CoreNetworkChangeEvent.InternetAvailableEvent, false)]
        [InlineData(CoreConnectivityEventSource.GetAllNetworkInterfaces, CoreNetworkChangeEvent.InternetAvailableEvent, CoreNetworkChangeEvent.LocalNetworkAvailableEvent, false)]
        public void NetworkChangeEvent_CalculateShouldDispatchNetworkChangeEvent(CoreConnectivityEventSource connectivityEventSource, CoreNetworkChangeEvent networkChangeEventPrevious, CoreNetworkChangeEvent networkChangeEventCurrent, bool expected)
        {
            CoreNetworkChangeEvent networkChangeEventChanges = CoreNetworkConnectivityEventArgs.CalculateNetworkChangeEventChanges(networkChangeEventCurrent, networkChangeEventPrevious);
            CoreNetworkChangeEvent networkChangeEventUpdates = CoreNetworkConnectivityEventArgs.CalculateNetworkChangeEventUpdates(networkChangeEventCurrent, networkChangeEventPrevious);
            CoreNetworkConnectivityEventArgs.CalculateShouldDispatchNetworkChangeEvent(connectivityEventSource, networkChangeEventCurrent, networkChangeEventPrevious, networkChangeEventChanges, networkChangeEventUpdates).Should().Be(expected);

            // Always dispatch a preferred network interface change or switch network event.
            if (CoreNetworkConnectivityEventArgs.CalculateHasPreferredInterfaceOrNetworkChanged(networkChangeEventCurrent))
            {
                expected.Should().BeTrue();
            }
            else
            {
                switch (connectivityEventSource)
                {
                    case CoreConnectivityEventSource.Unknown:
                        {
                            expected.Should().BeFalse();
                            break;
                        }

                    case CoreConnectivityEventSource.NetworkAddressChangeEvent:
                        {
                            (!networkChangeEventUpdates.IsNone()).Should().Be(expected);
                            break;
                        }

                    case CoreConnectivityEventSource.NetworkAvailabilityChangeEvent:
                        {
                            (!networkChangeEventUpdates.IsNone()).Should().Be(expected);
                            break;
                        }

                    case CoreConnectivityEventSource.InitializePreferredNetworkInterface:
                        {
                            expected.Should().BeFalse();
                            break;
                        }

                    case CoreConnectivityEventSource.GetAllNetworkInterfaces:
                        {
                            networkChangeEventCurrent.HasNetworkInterfaceChanged().Should().Be(expected);
                            break;
                        }

                    default:
                        throw new ArgumentOutOfRangeException(nameof(connectivityEventSource), connectivityEventSource, null);
                }
            }
        }
    }
}

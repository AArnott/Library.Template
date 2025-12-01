// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// // ***********************************************************************
// <copyright file="CorePreferencesIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Networking.Services;
using NetworkVisor.Core.Preferences;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Preferences
{
    /// <summary>
    /// Class CorePreferencesIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CorePreferencesIntegrationTests))]

    public class CorePreferencesIntegrationTests : CoreTestCaseBase
    {
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CorePreferencesIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CorePreferencesIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.Preferences = new TestCorePreferences(this.TestNetworkServices);
        }

        internal TestCorePreferences Preferences { get; }

        [Fact]
        public void Preferences_Ctor()
        {
            this.Preferences.Should().NotBeNull();
        }

        protected override void Dispose(bool disposing)
        {
            if (!this._isDisposed)
            {
                try
                {
                    if (disposing)
                    {
                        this.Preferences.DeleteTestStorageAsync().GetAwaiter().GetResult();
                        this.Preferences.Dispose();
                    }
                }
                finally
                {
                    this._isDisposed = true;
                }
            }

            base.Dispose(disposing);
        }

        internal class TestCorePreferences : CorePreferences
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TestCorePreferences"/> class.
            /// </summary>
            /// <param name="networkServices"></param>
            public TestCorePreferences(ICoreNetworkServices networkServices)
                : base(networkServices)
            {
            }
        }
    }
}

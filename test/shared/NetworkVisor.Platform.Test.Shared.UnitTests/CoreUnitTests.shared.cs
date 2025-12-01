// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-20-2020
//
// Last Modified By : SteveBu
// Last Modified On : 10-05-2024
// ***********************************************************************
// <copyright file="CoreUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************
// #define NV_TEST_FORCE_FAILURE

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests
{
    /// <summary>
    /// Class CoreUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreUnitTests))]

    public class CoreUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">Class test fixture common across all test cases.</param>
        public CoreUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        public static bool ForceTestFailure => false;

        [Fact(SkipUnless = nameof(ForceTestFailure), Skip = "Used to force a test failure")]
        public void CoreUnit_ForceFailure()
        {
            if (!CoreAppConstants.IsRunningInCI)
            {
                false.Should().BeTrue();
            }
        }

        [Fact]
        public void CoreUnit_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Unit);
        }

        [Fact]
        public void HasPlatformTraitAttribute()
        {
            this.TestClassType.HasPlatformTraitAttribute().Should().BeTrue();
        }

        [Fact]
        public void Validate_IsRunningInCI()
        {
#if NV_BUILD_CI
            CoreAppConstants.IsRunningInCI.Should().BeTrue();
#else
            CoreAppConstants.IsRunningInCI.Should().BeFalse();
#endif
        }

        [Fact]
        [ExcludeFromCodeCoverage]
        public void GetTraitOperatingSystem()
        {
            // Only do OperatingSystem match in CI and not Desktop
            if (!CoreAppConstants.IsRunningInCI || this.TestClassType.GetTraitOperatingSystem() == TraitOperatingSystem.NetCore)
            {
                return;
            }

            ICoreOperatingSystem operatingSystem = this.TestOperatingSystem;

            if (operatingSystem.IsAndroid)
            {
                this.TestClassType.GetTraitOperatingSystem().Should().Be(TraitOperatingSystem.Android);
            }
            else if (operatingSystem.IsIOS)
            {
                this.TestClassType.GetTraitOperatingSystem().Should().Be(TraitOperatingSystem.IOS);
            }
            else if (operatingSystem.IsMacCatalyst)
            {
                this.TestClassType.GetTraitOperatingSystem().Should().Be(TraitOperatingSystem.MacCatalyst);
            }
            else if (operatingSystem.IsLinux)
            {
                this.TestClassType.GetTraitOperatingSystem().Should().Be(TraitOperatingSystem.Linux);
            }
            else if (operatingSystem.IsMacOS)
            {
                this.TestClassType.GetTraitOperatingSystem().Should().Be(TraitOperatingSystem.MacOS);
            }
            else if (operatingSystem.IsWindows)
            {
#if NV_PLAT_LINUX
                // Linux is now built on a Windows Host
                this.TestClassType.GetTraitOperatingSystem().Should().Be(TraitOperatingSystem.Linux);
#elif NV_PLAT_WPF
                this.TestClassType.GetTraitOperatingSystem().Should().Be(TraitOperatingSystem.WPF);
#else
                this.TestClassType.GetTraitOperatingSystem().Should().Be(TraitOperatingSystem.Windows);
#endif
            }
            else if (operatingSystem.IsWinUI)
            {
                this.TestClassType.GetTraitOperatingSystem().Should().Be(TraitOperatingSystem.WinUI);
            }
            else
            {
                Assert.Fail("Unknown OperatingSystemType");
            }
        }

        [Fact]
        public void GetTraitTestType()
        {
            this.TestClassType.GetTraitTestType().Should().Be(TraitTestType.Unit);
        }

        [Fact]
        public void OutputTraits()
        {
            this.TestOutputHelper.WriteLine($"TestType: {this.TestClassType.GetTraitTestType()}");
            this.TestOutputHelper.WriteLine($"OS Type: {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
        }
    }
}

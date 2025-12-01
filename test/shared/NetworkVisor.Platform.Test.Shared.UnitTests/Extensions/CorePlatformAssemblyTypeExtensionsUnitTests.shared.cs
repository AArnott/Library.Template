// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// // ***********************************************************************
// <copyright file="CorePlatformAssemblyTypeExtensionsUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary>Platform Assembly Type Extensions Unit Tests.</summary>

using FluentAssertions;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Extensions
{
    /// <summary>
    /// Class CorePlatformAssemblyTypeExtensionsUnitTests. Platform Assembly Type Extensions Unit Tests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CorePlatformAssemblyTypeExtensionsUnitTests))]

    public class CorePlatformAssemblyTypeExtensionsUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CorePlatformAssemblyTypeExtensionsUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CorePlatformAssemblyTypeExtensionsUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CorePlatformAssemblyTypeExtensions_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Unit);
        }

        [Fact]
        public void CorePlatformAssemblyTypeExtensions_GetPlatformAssemblyType()
        {
#if NV_PLAT_ANDROID
            typeof(CorePlatformAssemblyTypeExtensionsUnitTests).GetPlatformAssemblyType().Should().Be(CorePlatformAssemblyType.Android);
#elif NV_PLAT_IOS
            typeof(CorePlatformAssemblyTypeExtensionsUnitTests).GetPlatformAssemblyType().Should().Be(CorePlatformAssemblyType.IOS);
#elif NV_PLAT_LINUX
            typeof(CorePlatformAssemblyTypeExtensionsUnitTests).GetPlatformAssemblyType().Should().Be(CorePlatformAssemblyType.Linux);
#elif NV_PLAT_MACCATALYST
            typeof(CorePlatformAssemblyTypeExtensionsUnitTests).GetPlatformAssemblyType().Should().Be(CorePlatformAssemblyType.MacCatalyst);
#elif NV_PLAT_MACOS
            typeof(CorePlatformAssemblyTypeExtensionsUnitTests).GetPlatformAssemblyType().Should().Be(CorePlatformAssemblyType.MacOS);
#elif NV_PLAT_WPF
            typeof(CorePlatformAssemblyTypeExtensionsUnitTests).GetPlatformAssemblyType().Should().Be(CorePlatformAssemblyType.WPF);
#elif NV_PLAT_WINUI
            typeof(CorePlatformAssemblyTypeExtensionsUnitTests).GetPlatformAssemblyType().Should().Be(CorePlatformAssemblyType.WinUI);
#elif NV_PLAT_WINDOWS || NV_PLAT_WPF
            typeof(CorePlatformAssemblyTypeExtensionsUnitTests).GetPlatformAssemblyType().Should().Be(CorePlatformAssemblyType.Windows);
#elif NV_PLAT_NETCORE
            typeof(CorePlatformAssemblyTypeExtensionsUnitTests).GetPlatformAssemblyType().Should().Be(CorePlatformAssemblyType.NetCore);
#else
#error NV_PLAT_XXXX is undefined
#endif

        }

        [Theory]
        [InlineData(null, CorePlatformAssemblyType.Unknown)]
        [InlineData("Foobar", CorePlatformAssemblyType.Unknown)]
        [InlineData("NetworkVisor.Platform.Linux", CorePlatformAssemblyType.Linux)]
        [InlineData("NetworkVisor.Core.CoreSystem.CoreFrameworkInfo NetworkVisor.Platform.Linux", CorePlatformAssemblyType.Linux)]
        public void CorePlatformAssemblyTypeExtensions_ToPlatformAssemblyType(string? platformAssemblyNamespace, CorePlatformAssemblyType platformAssemblyTypeExpected)
        {
            platformAssemblyNamespace.ToPlatformAssemblyType().Should().Be(platformAssemblyTypeExpected);
        }

        [Theory]
        [InlineData("Unknown", CorePlatformAssemblyType.Unknown)]
        [InlineData("NetworkVisor.Platform.Windows", CorePlatformAssemblyType.Windows)]
        [InlineData("NetworkVisor.Platform.MacOS", CorePlatformAssemblyType.MacOS)]
        [InlineData("NetworkVisor.Platform.Android", CorePlatformAssemblyType.Android)]
        [InlineData("NetworkVisor.Platform.IOS", CorePlatformAssemblyType.IOS)]
        [InlineData("NetworkVisor.Platform.MacCatalyst", CorePlatformAssemblyType.MacCatalyst)]
        [InlineData("NetworkVisor.Platform.Linux", CorePlatformAssemblyType.Linux)]
        [InlineData("NetworkVisor.Platform.WinUI", CorePlatformAssemblyType.WinUI)]
        [InlineData("NetworkVisor.Platform.WPF", CorePlatformAssemblyType.WPF)]
        [InlineData("NetworkVisor.Platform.NetCore", CorePlatformAssemblyType.NetCore)]
        public void CorePlatformAssemblyTypeExtensions_ToPlatformAssemblyTypeName(string? platformAssemblyNamespace, CorePlatformAssemblyType platformAssemblyTypeExpected)
        {
            platformAssemblyTypeExpected.ToPlatformAssemblyTypeName().Should().Be(platformAssemblyNamespace);
            platformAssemblyTypeExpected.GetDescription().Should().Be(platformAssemblyNamespace);
        }
    }
}

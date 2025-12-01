// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-20-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-20-2020
// ***********************************************************************
// <copyright file="CoreProductInfoIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Reflection;
using FluentAssertions;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.CoreSystem
{
    /// <summary>
    /// Class CoreProductInfoIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreProductInfoIntegrationTests))]

    public class CoreProductInfoIntegrationTests : CoreTestCaseBase
    {
        private readonly ICoreProductInfo productInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreProductInfoIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreProductInfoIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.productInfo = new CoreProductInfo(this.TestAssembly);
        }

        [Fact]
        public void ProductInfoIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);
        }

        [Fact]
        public void ProductInfoIntegration_OperatingSystem_ProductInfo()
        {
            this.TestOperatingSystem.Should().NotBeNull().And.BeOfType<CoreOperatingSystem>();
            this.TestOperatingSystem.ProductInfo.Should().NotBeNull().And.BeAssignableTo<ICoreProductInfo>();
        }

        [Fact]
        public void ProductInfoIntegration_OperatingSystem_ProductInfo_ProductAssemblyType()
        {
            this.TestOperatingSystem.ProductInfo.Should().NotBeNull().And.BeAssignableTo<ICoreProductInfo>();
            this.TestOperatingSystem.ProductInfo.ProductAssembly.Should().NotBeNull();
            this.TestOperatingSystem.ProductInfo.ProductAssembly.Should().BeSameAs(this.TestAssembly);
        }

        [Fact]
        public void ProductInfoIntegration_OperatingSystem_ProductInfo_Output()
        {
            this.TestOperatingSystem.ProductInfo.Should().NotBeNull().And.BeAssignableTo<ICoreProductInfo>();
            this.TestOutputHelper.WriteLine($"ProductInfo:\n{this.TestOperatingSystem.ProductInfo.ToStringWithPropNameMultiLine()}");
        }

        [Fact]
        public void ProductInfoIntegration_OperatingSystem_ProductInfo_ProductAssembly_GetDisplayFullName()
        {
            Assembly productAssembly = this.TestOperatingSystem.ProductInfo.ProductAssembly;
            productAssembly.Should().NotBeNull();
            this.TestOutputHelper.WriteLine($"ProductInfo.ProductAssembly.GetNamespace: {productAssembly.GetNamespace()}");
        }
    }
}

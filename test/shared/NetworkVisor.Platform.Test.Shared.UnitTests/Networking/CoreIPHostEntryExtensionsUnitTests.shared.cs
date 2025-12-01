// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreIPHostEntryExtensionsUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Logging.Factory;
using NetworkVisor.Core.Logging.Loggable;
using NetworkVisor.Core.Logging.LogProperty;
using NetworkVisor.Core.Logging.Types;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Networking
{
    /// <summary>
    /// Class CoreIPHostEntryExtensionsUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreIPHostEntryExtensionsUnitTests))]

    public class CoreIPHostEntryExtensionsUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreIPHostEntryExtensionsUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreIPHostEntryExtensionsUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method IPHostEntryExtensions_Ctor.
        /// </summary>
        [Fact]
        public void IPHostEntryExtensions_Ctor()
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            ((IPHostEntry)null).ToString(CoreLoggableFormatFlags.ToLogWithParents).Should().BeEmpty();
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }

        /// <summary>
        /// Defines the test method IPHostEntryExtensions_FormatString_StringBuilder.
        /// </summary>
        [Fact]
        public void IPHostEntryExtensions_FormatString_StringBuilder()
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            ((IPHostEntry)null).FormatLogString(null, new CoreLogPropertyListFormatter(CoreLoggableFormatFlags.ToLogWithParents, CoreAppConstants.GetMinimumLogLevel())).ToString().Should().BeEmpty();
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }

        /// <summary>
        /// Defines the test method IPHostEntryExtensions_GetLogPropertyListLevel_Null.
        /// </summary>
        [Fact]
        public void IPHostEntryExtensions_GetLogPropertyListLevel_Null()
        {
            Func<IEnumerable<ICoreLogPropertyLevel>> fx = () => new IPHostEntry().GetLogPropertyListLevel(null!);

            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logPropertyListFormatter");
        }

        /// <summary>
        /// Defines the test method IPHostEntryExtensions_FormatLogString_Null.
        /// </summary>
        [Fact]
        public void IPHostEntryExtensions_FormatLogString_Null()
        {
            Func<StringBuilder> fx = () => new IPHostEntry().FormatLogString(null, null!);

            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logPropertyListFormatter");
        }

        /// <summary>
        /// Defines the test method IPHostEntryExtensions_Empty_ToString.
        /// </summary>
        [Fact]
        public void IPHostEntryExtensions_Empty_ToString()
        {
            new IPHostEntry().ToString(new CoreLogPropertyListFormatter(CoreLoggableFormatFlags.ToString)).Should().Be($"{StringExtensions.NullString},{StringExtensions.NullString},{StringExtensions.NullString}");
        }

        /// <summary>
        /// Defines the test method IPHostEntryExtensions_Empty_ToStringWithPropName.
        /// </summary>
        [Fact]
        public void IPHostEntryExtensions_Empty_ToStringWithPropName()
        {
            new IPHostEntry().ToString(new CoreLogPropertyListFormatter(CoreLoggableFormatFlags.ToStringWithPropName)).Should().Be($"HostName={StringExtensions.NullString},Aliases={StringExtensions.NullString},AddressList={StringExtensions.NullString}");
        }

        /// <summary>
        /// Defines the test method IPHostEntryExtensions_ToString.
        /// </summary>
        [Fact]
        public void IPHostEntryExtensions_ToString()
        {
            new IPHostEntry()
            {
                AddressList = [],
                Aliases = [],
                HostName = "www.foo.com",
            }.ToString(new CoreLogPropertyListFormatter(CoreLoggableFormatFlags.ToString)).Should().Be("www.foo.com,[],[]");
        }

        /// <summary>
        /// Defines the test method IPHostEntryExtensions_ToStringWithPropName.
        /// </summary>
        [Fact]
        public void IPHostEntryExtensions_ToStringWithPropName()
        {
            new IPHostEntry()
            {
                AddressList = [],
                Aliases = [],
                HostName = "www.foo.com",
            }.ToString(new CoreLogPropertyListFormatter(CoreLoggableFormatFlags.ToStringWithPropName)).Should().Be("HostName=www.foo.com,Aliases=[],AddressList=[]");
        }

        /// <summary>
        /// Defines the test method IPHostEntryExtensions_ToString_Null.
        /// </summary>
        [Fact]
        public void IPHostEntryExtensions_ToString_Null()
        {
            Func<string> fx = () => new IPHostEntry().ToString(null!);

            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logPropertyListFormatter");
        }
    }
}

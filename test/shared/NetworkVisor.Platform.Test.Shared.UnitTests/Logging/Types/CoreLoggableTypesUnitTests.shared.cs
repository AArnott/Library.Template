// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreLoggableTypesUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.Logging.Loggable;
using NetworkVisor.Core.Logging.LogProperty;
using NetworkVisor.Core.Logging.Types;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Logging.Types
{
    /// <summary>
    /// Class CoreLoggableTypesUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreLoggableTypesUnitTests))]

    public class CoreLoggableTypesUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreLoggableTypesUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreLoggableTypesUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method LoggableFormatUtils_IsWithParents.
        /// </summary>
        [Fact]
        public void LoggableFormatUtils_IsWithParents()
        {
            CoreLoggableFormatFlags.ToLogWithParents.IsWithParents().Should().BeTrue();
            CoreLoggableFormatFlags.ToLog.IsWithParents().Should().BeFalse();
            CoreLoggableFormatFlags.ToLogWithParentsMultiLine.IsWithParents().Should().BeTrue();
            CoreLoggableFormatFlags.ToLogWithMultiLine.IsWithParents().Should().BeFalse();

            CoreLoggableFormatFlags.ToStringWithParents.IsWithParents().Should().BeTrue();
            CoreLoggableFormatFlags.ToString.IsWithParents().Should().BeFalse();
            CoreLoggableFormatFlags.ToStringWithParentsMultiLine.IsWithParents().Should().BeTrue();
            CoreLoggableFormatFlags.ToStringWithMultiLine.IsWithParents().Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method LoggableFormatUtils_IsWithMultiLine.
        /// </summary>
        [Fact]
        public void LoggableFormatUtils_IsWithMultiLine()
        {
            CoreLoggableFormatFlags.ToLogWithParentsMultiLine.IsWithMultiLine().Should().BeTrue();
            CoreLoggableFormatFlags.ToLogWithParents.IsWithMultiLine().Should().BeFalse();
            CoreLoggableFormatFlags.ToLogWithMultiLine.IsWithMultiLine().Should().BeTrue();
            CoreLoggableFormatFlags.ToLog.IsWithMultiLine().Should().BeFalse();

            CoreLoggableFormatFlags.ToStringWithParentsMultiLine.IsWithMultiLine().Should().BeTrue();
            CoreLoggableFormatFlags.ToStringWithParents.IsWithMultiLine().Should().BeFalse();
            CoreLoggableFormatFlags.ToStringWithMultiLine.IsWithMultiLine().Should().BeTrue();
            CoreLoggableFormatFlags.ToString.IsWithMultiLine().Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method LoggableFormatUtils_IsWithPropName.
        /// </summary>
        [Fact]
        public void LoggableFormatUtils_IsWithPropName()
        {
            CoreLoggableFormatFlags.ToLog.IsWithPropName().Should().BeFalse();
            CoreLoggableFormatFlags.ToLogWithParents.IsWithPropName().Should().BeTrue();

            CoreLoggableFormatFlags.ToStringWithParentsMultiLine.IsWithPropName().Should().BeFalse();
            CoreLoggableFormatFlags.ToStringWithMultiLine.IsWithPropName().Should().BeFalse();

            CoreLoggableFormatFlags.ToStringWithParents.IsWithPropName().Should().BeFalse();
            CoreLoggableFormatFlags.ToStringWithParentsPropName.IsWithPropName().Should().BeTrue();

            CoreLoggableFormatFlags.ToStringWithPropName.IsWithPropName().Should().BeTrue();
            CoreLoggableFormatFlags.ToString.IsWithPropName().Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method LoggableFormatUtils_ToLog.
        /// </summary>
        [Fact]
        public void LoggableFormatUtils_ToLog()
        {
            CoreLoggableFormatFlags.ToLogWithParents.IsToLog().Should().BeTrue();
            CoreLoggableFormatFlags.WithParents.IsToLog().Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method LoggableFormatUtils_ToString.
        /// </summary>
        [Fact]
        public void LoggableFormatUtils_ToString()
        {
            CoreLoggableFormatFlags.ToStringWithParents.IsToString().Should().BeTrue();
            CoreLoggableFormatFlags.ToLogWithParents.IsToString().Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method LogPropertyListFormatter_Properties.
        /// </summary>
        [Fact]
        public void LogPropertyListFormatter_Properties()
        {
            var logPropertyListFormatter = new CoreLogPropertyListFormatter(CoreLoggableFormatFlags.ToStringWithParentsPropNameMultiLine, LogLevel.Critical);

            logPropertyListFormatter.CurrentScopeLevel.Should().Be(0);
            logPropertyListFormatter.IsToLog.Should().BeFalse();
            logPropertyListFormatter.IsToString.Should().BeTrue();
            logPropertyListFormatter.IsWithMultiLine.Should().BeTrue();
            logPropertyListFormatter.IsWithParents.Should().BeTrue();
            logPropertyListFormatter.LoggableFormat.Should()
                .Be(CoreLoggableFormatFlags.ToStringWithParentsPropNameMultiLine);
            logPropertyListFormatter.IsWithPropName.Should().BeTrue();
            logPropertyListFormatter.MinimumLogLevel.Should().Be(LogLevel.Critical);
            logPropertyListFormatter.PropertyKeyMaxLength.Should().Be(0);
        }

        /// <summary>
        /// Defines the test method LogPropertyListFormatter_Clone.
        /// </summary>
        [Fact]
        public void LogPropertyListFormatter_Clone()
        {
            var logPropertyListFormatter = new CoreLogPropertyListFormatter(CoreLoggableFormatFlags.ToStringWithParentsPropNameMultiLine, LogLevel.Critical);
            ICoreLogPropertyListFormatter logPropertyListFormatterClone = logPropertyListFormatter.Clone();

            logPropertyListFormatter.CurrentScopeLevel.Should().Be(logPropertyListFormatterClone.CurrentScopeLevel);
            logPropertyListFormatter.IsToLog.Should().Be(logPropertyListFormatterClone.IsToLog);
            logPropertyListFormatter.IsToString.Should().Be(logPropertyListFormatterClone.IsToString);
            logPropertyListFormatter.IsWithMultiLine.Should().Be(logPropertyListFormatterClone.IsWithMultiLine);
            logPropertyListFormatter.IsWithParents.Should().Be(logPropertyListFormatterClone.IsWithParents);
            logPropertyListFormatter.LoggableFormat.Should()
                .Be(logPropertyListFormatterClone.LoggableFormat);
            logPropertyListFormatter.IsWithPropName.Should().Be(logPropertyListFormatterClone.IsWithPropName);
            logPropertyListFormatter.MinimumLogLevel.Should().Be(logPropertyListFormatterClone.MinimumLogLevel);
            logPropertyListFormatter.PropertyKeyMaxLength.Should().Be(logPropertyListFormatterClone.PropertyKeyMaxLength);
        }
    }
}

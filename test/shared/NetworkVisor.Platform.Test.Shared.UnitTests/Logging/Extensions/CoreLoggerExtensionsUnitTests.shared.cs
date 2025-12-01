// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-13-2020
// ***********************************************************************
// <copyright file="CoreLoggerExtensionsUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Diagnostics.CodeAnalysis;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Logging.Loggable;
using NetworkVisor.Core.Logging.LogProperty;
using NetworkVisor.Core.Logging.Types;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Logging.Extensions
{
    /// <summary>
    /// Class CoreLoggerExtensionsUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreLoggerExtensionsUnitTests))]

    public class CoreLoggerExtensionsUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// EmptyString.
        /// </summary>
        private const string EmptyLogString = "";

        /// <summary>
        /// The test log MSG.
        /// </summary>
        private const string TestLogMsg = "Test Log";

        /// <summary>
        /// The test property.
        /// </summary>
        private const string TestProperty = "StringValue";

        /// <summary>
        /// The test property parent.
        /// </summary>
        private const string TestPropertyParent = "ParentStringValue";

        /// <summary>
        /// The test value.
        /// </summary>
        private const string TestValue = "TestString";

        /// <summary>
        /// The parent test value.
        /// </summary>
        private const string ParentTestValue = "ParentTestString";

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreLoggerExtensionsUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreLoggerExtensionsUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method LoggerExtensions_BeginPropertyScope_Int.
        /// </summary>
        [Fact]
        public void LoggerExtensions_BeginPropertyScope_Int()
        {
            using (this.TestCaseLogger.BeginPropertyScope("PropertyInt", 14))
            {
                this.TestCaseLogger.LogDebug("Log Int Property Scope");
            }
        }

        /// <summary>
        /// Defines the test method LoggerExtensions_BeginPropertyScope_Enum.
        /// </summary>
        [Fact]
        public void LoggerExtensions_BeginPropertyScope_Enum()
        {
            using (this.TestCaseLogger.BeginPropertyScope<Enum>("PropertyInt", CoreLoggableFormatFlags.None))
            {
                this.TestCaseLogger.LogDebug("Log LoggableFormat.None Property Scope");
            }
        }

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

        /// <summary>
        /// Defines the test method LoggerExtensions_FormatPropertyList_LogPropertyEnumerable_Null.
        /// </summary>
        [Fact]
        public void LoggerExtensions_FormatPropertyList_LogPropertyEnumerable_Null()
        {
            var logPropertyListFormatter = new CoreLogPropertyListFormatter(CoreLoggableFormatFlags.None);

            Action act = () => logPropertyListFormatter.FormatPropertyList(new StringBuilder(), null!);
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logPropertyEnumerable");
        }

        /// <summary>
        /// Defines the test method LoggerExtensions_FormatPropertyDictionary_loggableDictionary_Null.
        /// </summary>
        [Fact]
        public void LoggerExtensions_FormatPropertyDictionary_loggableDictionary_Null()
        {
            var logPropertyListFormatter = new CoreLogPropertyListFormatter(CoreLoggableFormatFlags.None);

            Action act = () => logPropertyListFormatter.FormatPropertyDictionary(new StringBuilder(), null!);
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("loggableDictionary");
        }

        /// <summary>
        /// Defines the test method LoggerExtensions_FormatPropertyLevelItem_LogPropertyLevelItem_Null.
        /// </summary>
        [Fact]
        public void LoggerExtensions_FormatPropertyLevelItem_LogPropertyLevelItem_Null()
        {
            var logPropertyListFormatter = new CoreLogPropertyListFormatter(CoreLoggableFormatFlags.None);

            Func<StringBuilder> fx = () => logPropertyListFormatter.FormatPropertyLevelItem(new StringBuilder(), null!, 0);
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logPropertyLevelItem");
        }

        /// <summary>
        /// Defines the test method LoggerExtensions_FormatPropertyLevelItem_LogPropertyLevelItem_InvalidType.
        /// </summary>
        /// <param name="logPropertyType">Type of log property.</param>
        /// <param name="objectToTest">Object to test.</param>
        /// <param name="expectedValue">Expected value of test object.</param>
        [Theory]
        [InlineData(CoreLogPropertyType.StringValue, "value", "property=value")]
        [InlineData(CoreLogPropertyType.LoggableObject, "value", "property=value")]
        [InlineData(CoreLogPropertyType.Dictionary, "value", "property=value")]
        [InlineData(CoreLogPropertyType.EnumDescription, "value", "property=value")]
        [InlineData(CoreLogPropertyType.Array, "value", "property=[v,a,l,u,e]")]
        [InlineData(CoreLogPropertyType.Array, 100, "property={100}")]
        [InlineData(CoreLogPropertyType.Bool, "false", "property=false")]
        [InlineData(CoreLogPropertyType.IPAddress, "10.1.10.1", "property=10.1.10.1")]
        [InlineData(CoreLogPropertyType.IPAddressSubnet, "10.1.10.1, 255.255.255.0", "property=10.1.10.1, 255.255.255.0")]
        [InlineData(CoreLogPropertyType.GuidValue, "2982B44D-DC00-4D8B-94A0-1CFCD7A71ED5", "property=2982B44D-DC00-4D8B-94A0-1CFCD7A71ED5")]
        [InlineData(CoreLogPropertyType.Number, "100", "property=100")]
        [InlineData(CoreLogPropertyType.NumberWithSeparator, "1000000", "property=1,000,000")]
        [InlineData(CoreLogPropertyType.DateTimeOffset, "value", "property=value")]
        [InlineData(CoreLogPropertyType.DateTimeOffsetUtc, "value", "property=value")]
        [InlineData(CoreLogPropertyType.PhysicalAddress, "B4-2E-99-A6-08-2D", "property=B4-2E-99-A6-08-2D")]
        [InlineData(CoreLogPropertyType.WmiProperty, "value", "property=value")]
        [InlineData(CoreLogPropertyType.Exception, default(Exception), "property=null")]
        [InlineData(CoreLogPropertyType.Unknown, "value", "property=value")]
        public void LoggerExtensions_FormatPropertyLevelItem_LogPropertyLevelItem_InvalidType(CoreLogPropertyType logPropertyType, object? objectToTest, string? expectedValue)
        {
            var logPropertyListFormatter = new CoreLogPropertyListFormatter(CoreLoggableFormatFlags.WithPropName);

            logPropertyListFormatter.FormatPropertyLevelItem(
                new StringBuilder(),
                new CoreLogPropertyLevel("property", objectToTest, LogLevel.Debug, logPropertyType),
                0).ToString().Should().Be(expectedValue);
        }

        [Fact]
        public void LoggerExtensions_FormatPropertyLevelItem_LogPropertyLevelItem_Exception()
        {
            var logPropertyListFormatter = new CoreLogPropertyListFormatter(CoreLoggableFormatFlags.WithPropName);

            logPropertyListFormatter.FormatPropertyLevelItem(
                new StringBuilder(),
                new CoreLogPropertyLevel("property", new InvalidOperationException("Test"), LogLevel.Debug, CoreLogPropertyType.Exception),
                0).ToString().Should().Be("property=System.InvalidOperationException: Test");
        }

        [Fact]
        public void LoggerExtensions_FormatPropertyLevelItem_LogPropertyLevelItem_NotException()
        {
            var logPropertyListFormatter = new CoreLogPropertyListFormatter(CoreLoggableFormatFlags.WithPropName);

            logPropertyListFormatter.FormatPropertyLevelItem(
                new StringBuilder(),
                new CoreLogPropertyLevel("property", "Test", LogLevel.Debug, CoreLogPropertyType.Exception),
                0).ToString().Should().Be("property=Test");
        }

        /// <summary>
        /// Defines the test method LoggerExtensions_FormatLogPropertyDictionary_loggableDictionary_Null.
        /// </summary>
        [Fact]
        public void LoggerExtensions_FormatLogPropertyDictionary_loggableDictionary_Null()
        {
            var logPropertyListFormatter = new CoreLogPropertyListFormatter();

            Action act = () => logPropertyListFormatter.FormatLogPropertyDictionary(new StringBuilder(), null!);
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("loggableDictionary");
        }

        /// <summary>
        /// Defines the test method LoggerExtensions_FormatLogPropertyDictionary_loggableDictionary_Empty.
        /// </summary>
        [Fact]
        public void LoggerExtensions_FormatLogPropertyDictionary_loggableDictionary_Empty()
        {
            new CoreLogPropertyListFormatter(CoreLoggableFormatFlags.None).FormatLogPropertyDictionary(new StringBuilder(), new CoreLogPropertyDictionary()).ToString().Should().Be("{}");
        }

        /// <summary>
        /// Defines the test method LoggerExtensions_FormatPropertyArray_PropertyArray_Null.
        /// </summary>
        [Fact]
        public void LoggerExtensions_FormatPropertyArray_PropertyArray_Null()
        {
            var logPropertyListFormatter = new CoreLogPropertyListFormatter();

            Action act = () => logPropertyListFormatter.FormatPropertyArray(new StringBuilder(), null!);
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("propertyArray");
        }

        /// <summary>
        /// Defines the test method LoggerExtensions_FormatLogPropertyDictionary_LoggableDictionary_Null.
        /// </summary>
        [Fact]
        public void LoggerExtensions_FormatLogPropertyDictionary_LoggableDictionary_Null()
        {
            var logPropertyListFormatter = new CoreLogPropertyListFormatter();

            Action act = () => logPropertyListFormatter.FormatLogPropertyDictionary(new StringBuilder(), null!);
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("loggableDictionary");
        }

        /// <summary>
        /// Defines the test method LoggerExtensions_FormatPropertyObjectValue_PropertyArray_Null.
        /// </summary>
        [Fact]
        public void LoggerExtensions_FormatPropertyObjectValue_PropertyArray_Null()
        {
            new CoreLogPropertyListFormatter()
                .FormatPropertyObjectValue(new StringBuilder(), null!).ToString().Should().Be(StringExtensions.NullString);
        }

        /// <summary>
        /// Defines the test method LoggerExtensions_FormatLogPropertyDictionary_FormatPropertyObjectValue_LoggableDictionary_Empty.
        /// </summary>
        [Fact]
        public void LoggerExtensions_FormatLogPropertyDictionary_FormatPropertyObjectValue_LoggableDictionary_Empty()
        {
            new CoreLogPropertyListFormatter().FormatPropertyObjectValue(new StringBuilder(), new CoreLogPropertyDictionary()).ToString().Should().Be("{}");
        }

        /// <summary>
        /// Defines the test method LoggerExtensions_FormatPropertyList_ToProperties.
        /// </summary>
        [Fact]
        public void LoggerExtensions_FormatPropertyList_ToProperties()
        {
            new CoreLogPropertyListFormatter(CoreLoggableFormatFlags.ToLog).IsToLog.Should().BeTrue();
            new CoreLogPropertyListFormatter(CoreLoggableFormatFlags.ToString).IsToLog.Should().BeFalse();
            new CoreLogPropertyListFormatter(CoreLoggableFormatFlags.ToString).IsToString.Should().BeTrue();
            new CoreLogPropertyListFormatter(CoreLoggableFormatFlags.ToLog).IsToString.Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method LoggerExtensions_FormatPropertyList_StringBuilder_Null.
        /// </summary>
        [Fact]
        public void LoggerExtensions_FormatPropertyList_StringBuilder_Null()
        {
            var logPropertyListFormatter = new CoreLogPropertyListFormatter(CoreLoggableFormatFlags.None);

            logPropertyListFormatter.FormatPropertyList(null, []).ToString().Should().BeEmpty();
        }

        /// <summary>
        /// Defines the test method LoggerExtensions_CoreLogProperty_LoggableObject.
        /// </summary>
        [Fact]
        public void LoggerExtensions_CoreLogProperty_LoggableObject()
        {
            TestLoggable testLoggable = CreateTestLoggable(this.TestCaseLogger);

            var coreLogProperty = new CoreLogProperty("property", testLoggable, CoreLogPropertyType.LoggableObject);
            coreLogProperty.Should().NotBeNull();
            coreLogProperty.Key.Should().Be("property");
            coreLogProperty.Value.Should().BeSameAs(testLoggable).And.Subject.Should().BeAssignableTo<ICoreLoggable>();
            coreLogProperty.ToString().Should().Be("{TestString}");
            this.TestOutputHelper.WriteLine(coreLogProperty.ToString());
        }

        /// <summary>
        /// Defines the test method LoggerExtensions_CoreLogProperty_Dictionary_ToString.
        /// </summary>
        [Fact]
        public void LoggerExtensions_CoreLogProperty_Dictionary_ToString()
        {
            TestLoggable testLoggable = CreateTestLoggable(this.TestCaseLogger);
            Dictionary<string, object> loggableDictionary = CreateLoggableDictionary(testLoggable);

            var coreLogProperty = new CoreLogProperty("property", loggableDictionary, CoreLogPropertyType.Dictionary);
            coreLogProperty.Should().NotBeNull();
            coreLogProperty.Key.Should().Be("property");
            coreLogProperty.Value.Should().BeSameAs(loggableDictionary).And.Subject.Should().BeAssignableTo<IDictionary<string, object>>();
            this.TestOutputHelper.WriteLine(coreLogProperty.ToString());
            coreLogProperty.ToString().Should().Be("{item1={value1},item2={TestString},item3=[value2a,value2b],item4=[],item5=[{TestString}]}");
        }

        /// <summary>
        /// Defines the test method LoggerExtensions_CoreLogProperty_Dictionary_ToStringWithPropName.
        /// </summary>
        [Fact]
        public void LoggerExtensions_CoreLogProperty_Dictionary_ToStringWithPropName()
        {
            TestLoggable testLoggable = CreateTestLoggable(this.TestCaseLogger);
            Dictionary<string, object> loggableDictionary = CreateLoggableDictionary(testLoggable);

            var coreLogProperty = new CoreLogProperty("property", loggableDictionary, CoreLogPropertyType.Dictionary);
            coreLogProperty.Should().NotBeNull();
            coreLogProperty.Key.Should().Be("property");
            coreLogProperty.Value.Should().BeSameAs(loggableDictionary).And.Subject.Should().BeAssignableTo<IDictionary<string, object>>();
            this.TestOutputHelper.WriteLine(coreLogProperty.ToStringWithPropName());
            coreLogProperty.ToStringWithPropName().Should().Be("property={item1={prop1=value1},item2={StringValue=TestString},item3=[value2a,value2b],item4=[],item5=[{StringValue=TestString}]}");
        }

        /// <summary>
        /// Defines the test method LoggerExtensions_CoreLogProperty_Dictionary_ToStringMultiLine.
        /// </summary>
        [Fact]
        public void LoggerExtensions_CoreLogProperty_Dictionary_ToStringMultiLine()
        {
            TestLoggable testLoggable = CreateTestLoggable(this.TestCaseLogger);
            Dictionary<string, object> loggableDictionary = CreateLoggableDictionary(testLoggable);

            var coreLogProperty = new CoreLogProperty("property", loggableDictionary, CoreLogPropertyType.Dictionary);
            coreLogProperty.Should().NotBeNull();
            coreLogProperty.Key.Should().Be("property");
            coreLogProperty.Value.Should().BeSameAs(loggableDictionary).And.Subject.Should().BeAssignableTo<IDictionary<string, object>>();
            this.TestOutputHelper.WriteLine(coreLogProperty.ToString());
            coreLogProperty.ToStringWithMultiLine().Should().Be("{item1={value1},item2={TestString},item3=[value2a,value2b],item4=[],item5=[{TestString}]}");
        }

        /// <summary>
        /// Defines the test method LoggerExtensions_CoreLogProperty_Dictionary_ToStringWithPropName.
        /// </summary>
        [Fact]
        public void LoggerExtensions_CoreLogProperty_Dictionary_ToStringWithPropNameMultiLine()
        {
            TestLoggable testLoggable = CreateTestLoggable(this.TestCaseLogger);
            Dictionary<string, object> loggableDictionary = CreateLoggableDictionary(testLoggable);

            var coreLogProperty = new CoreLogProperty("property", loggableDictionary, CoreLogPropertyType.Dictionary);
            coreLogProperty.Should().NotBeNull();
            coreLogProperty.Key.Should().Be("property");
            coreLogProperty.Value.Should().BeSameAs(loggableDictionary).And.Subject.Should().BeAssignableTo<IDictionary<string, object>>();
            this.TestOutputHelper.WriteLine(coreLogProperty.ToStringWithPropName());
            coreLogProperty.ToStringWithPropNameMultiLine().Should().Be("property={item1={prop1=value1},item2={StringValue=TestString},item3=[value2a,value2b],item4=[],item5=[{StringValue=TestString}]}");
        }

        /// <summary>
        /// Defines the test method LoggerExtensions_CoreLogProperty_Dictionary_Empty.
        /// </summary>
        [Fact]
        public void LoggerExtensions_CoreLogProperty_Dictionary_Empty()
        {
            TestLoggable testLoggable = CreateTestLoggable(this.TestCaseLogger);
            var loggableDictionary = new Dictionary<string, object>();

            var coreLogProperty = new CoreLogProperty("property", loggableDictionary, CoreLogPropertyType.Dictionary);
            coreLogProperty.Should().NotBeNull();
            coreLogProperty.Key.Should().Be("property");
            coreLogProperty.Value.Should().BeSameAs(loggableDictionary).And.Subject.Should()
                .BeAssignableTo<IDictionary<string, object>>();
            coreLogProperty.ToString().Should().Be("{}");
        }

        /// <summary>
        /// Defines the test method LoggerExtensions_CoreLogProperty_Array_ToString.
        /// </summary>
        [Fact]
        public void LoggerExtensions_CoreLogProperty_Array_ToString()
        {
            TestLoggable testLoggable = CreateTestLoggable(this.TestCaseLogger);
            CoreLogProperty coreLogProperty = CreateTestCoreLogProperty(testLoggable);

            this.TestOutputHelper.WriteLine(coreLogProperty.ToString());

            // CoreLogProperty.ToString().Should().Be(@"[1,2,[3,4],TestString,[item1:value1,item2:TestString,item3:[value2a,value2b],item4:[],item5:[TestString]]]");
        }

        /// <summary>
        /// Defines the test method LoggerExtensions_CoreLogProperty_Array_ToStringWithParents.
        /// </summary>
        [Fact]
        public void LoggerExtensions_CoreLogProperty_Array_ToStringWithParents()
        {
            TestLoggable testLoggable = CreateTestLoggable(this.TestCaseLogger);
            CoreLogProperty coreLogProperty = CreateTestCoreLogProperty(testLoggable);

            this.TestOutputHelper.WriteLine(coreLogProperty.ToStringWithParents());

            // CoreLogProperty.ToStringWithParents().Should().Be(@"[1,2,[3,4],NetworkVisor.Platform.Shared.UnitTests.Logging.Extensions.CoreLoggerExtensionsUnitTests.TestLoggableParentTestStringTestString,[item1:value1,item2:NetworkVisor.Platform.Shared.UnitTests.Logging.Extensions.LoggerExtensionsUnitTests.TestLoggableParentTestStringTestString,item3:[value2a,value2b],item4:[],item5:[NetworkVisor.Core.Shared.UnitTests.Logging.Extensions.LoggerExtensionsUnitTests.TestLoggableParentTestStringTestString]]]");
        }

        /// <summary>
        /// Defines the test method LoggerExtensions_CoreLogProperty_Array_ToStringWithPropName.
        /// </summary>
        [Fact]
        public void LoggerExtensions_CoreLogProperty_Array_ToStringWithPropName()
        {
            TestLoggable testLoggable = CreateTestLoggable(this.TestCaseLogger);
            CoreLogProperty coreLogProperty = CreateTestCoreLogProperty(testLoggable);

            this.TestOutputHelper.WriteLine(coreLogProperty.ToStringWithPropName());

            // CoreLogProperty.ToStringWithPropName().Should().Be(@"[1,2,[3,4],TestString,[item1:value1,item2:TestString,item3:[value2a,value2b],item4:[],item5:[TestString]]]");
        }

        /// <summary>
        /// Defines the test method LoggerExtensions_CoreLogProperty_Array_ToStringWithParentsPropName.
        /// </summary>
        [Fact]
        public void LoggerExtensions_CoreLogProperty_Array_ToStringWithParentsPropName()
        {
            TestLoggable testLoggable = CreateTestLoggable(this.TestCaseLogger);
            CoreLogProperty coreLogProperty = CreateTestCoreLogProperty(testLoggable);

            this.TestOutputHelper.WriteLine(coreLogProperty.ToStringWithParentsPropName());

            // CoreLogProperty.ToStringWithParentsPropName().Should().Be(@"[1,2,[3,4],TestString,[item1:value1,item2:TestString,item3:[value2a,value2b],item4:[],item5:[TestString]]]");
        }

        /// <summary>
        /// Defines the test method LoggerExtensions_CoreLogProperty_Array_ToStringWithPropNameMultiLine.
        /// </summary>
        [Fact]
        public void LoggerExtensions_CoreLogProperty_Array_ToStringWithPropNameMultiLine()
        {
            TestLoggable testLoggable = CreateTestLoggable(this.TestCaseLogger);
            CoreLogProperty coreLogProperty = CreateTestCoreLogProperty(testLoggable);

            this.TestOutputHelper.WriteLine(coreLogProperty.ToStringWithPropNameMultiLine());

            // CoreLogProperty.ToStringWithPropNameMultiLine().Should().Be(@"[1,2,[3,4],TestString,[item1:value1,item2:TestString,item3:[value2a,value2b],item4:[],item5:[TestString]]]");
        }

        /// <summary>
        /// Defines the test method LoggerExtensions_CoreLogProperty_Array_ToStringWithParentsPropNameMultiLine.
        /// </summary>
        [Fact]
        public void LoggerExtensions_CoreLogProperty_Array_ToStringWithParentsPropNameMultiLine()
        {
            TestLoggable testLoggable = CreateTestLoggable(this.TestCaseLogger);
            CoreLogProperty coreLogProperty = CreateTestCoreLogProperty(testLoggable);

            this.TestOutputHelper.WriteLine(coreLogProperty.ToStringWithParentsPropNameMultiLine());

            // CoreLogProperty.ToStringWithParentsPropNameMultiLine().Should().Be(@"[1,2,[3,4],TestString,[item1:value1,item2:TestString,item3:[value2a,value2b],item4:[],item5:[TestString]]]");
        }

        /// <summary>
        /// Defines the test method CoreLoggable_Properties.
        /// </summary>
        [Fact]
        public void CoreLoggable_Properties()
        {
            TestLoggable testLoggable = CreateTestLoggable(this.TestCaseLogger);
            testLoggable.Logger.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreLogger>();
        }

        /// <summary>
        /// Defines the test method CoreLoggable_ToString.
        /// </summary>
        [Fact]
        public void CoreLoggable_ToString()
        {
            TestLoggable testLoggable = CreateTestLoggable(this.TestCaseLogger);
            this.TestOutputHelper.WriteLine(testLoggable.ToString());
            testLoggable.ToString().Should().Be($"TestString");
        }

        /// <summary>
        /// Defines the test method CoreLoggable_ToStringPropName.
        /// </summary>
        [Fact]
        public void CoreLoggable_ToStringPropName()
        {
            TestLoggable testLoggable = CreateTestLoggable(this.TestCaseLogger);
            this.TestOutputHelper.WriteLine(testLoggable.ToStringWithPropName());
            testLoggable.ToStringWithPropName().Should().Be($"StringValue=TestString");
        }

        /// <summary>
        /// Defines the test method CoreLoggable_ToStringParents.
        /// </summary>
        [Fact]
        public void CoreLoggable_ToStringParents()
        {
            TestLoggable testLoggable = CreateTestLoggable(this.TestCaseLogger);
            this.TestOutputHelper.WriteLine(testLoggable.ToStringWithParents());
            testLoggable.ToStringWithParents().Should().Be($"NetworkVisor.Platform.Test.Shared.UnitTests.Logging.Extensions.CoreLoggerExtensionsUnitTests.TestLoggable,ParentTestString,TestString");
        }

        /// <summary>
        /// Defines the test method CoreLoggable_ToStringParentsPropName.
        /// </summary>
        [Fact]
        public void CoreLoggable_ToStringParentsPropName()
        {
            TestLoggable testLoggable = CreateTestLoggable(this.TestCaseLogger);
            this.TestOutputHelper.WriteLine(testLoggable.ToStringWithParentsPropName());
            testLoggable.ToStringWithParentsPropName().Should().Be("ObjectType=NetworkVisor.Platform.Test.Shared.UnitTests.Logging.Extensions.CoreLoggerExtensionsUnitTests.TestLoggable,ParentStringValue=ParentTestString,StringValue=TestString");
        }

        /// <summary>
        /// Defines the test method CoreLoggable_ToStringMultiLine.
        /// </summary>
        [Fact]
        public void CoreLoggable_ToStringMultiLine()
        {
            TestLoggable testLoggable = CreateTestLoggable(this.TestCaseLogger);
            this.TestOutputHelper.WriteLine(testLoggable.ToStringWithMultiLine());
            testLoggable.ToStringWithMultiLine().Should().Be($"TestString");
        }

        /// <summary>
        /// Defines the test method CoreLoggable_ToStringPropNameMultiLine.
        /// </summary>
        [Fact]
        public void CoreLoggable_ToStringPropNameMultiLine()
        {
            TestLoggable testLoggable = CreateTestLoggable(this.TestCaseLogger);
            this.TestOutputHelper.WriteLine(testLoggable.ToStringWithPropNameMultiLine());
            testLoggable.ToStringWithPropNameMultiLine().Should().Be($"StringValue=TestString");
        }

        /// <summary>
        /// Defines the test method CoreLoggable_ToStringParentsMultiLine.
        /// </summary>
        [Fact]
        public void CoreLoggable_ToStringParentsMultiLine()
        {
            TestLoggable testLoggable = CreateTestLoggable(this.TestCaseLogger);
            this.TestOutputHelper.WriteLine(testLoggable.ToStringWithParentsMultiLine());
            testLoggable.ToStringWithParentsMultiLine().Should().Be($"NetworkVisor.Platform.Test.Shared.UnitTests.Logging.Extensions.CoreLoggerExtensionsUnitTests.TestLoggable{Environment.NewLine}ParentTestString{Environment.NewLine}TestString");
        }

        /// <summary>
        /// Defines the test method CoreLoggable_ToStringParentsPropNameMultiLine.
        /// </summary>
        [Fact]
        public void CoreLoggable_ToStringParentsPropNameMultiLine()
        {
            TestLoggable testLoggable = CreateTestLoggable(this.TestCaseLogger);
            this.TestOutputHelper.WriteLine(testLoggable.ToStringWithParentsPropNameMultiLine());
            testLoggable.ToStringWithParentsPropNameMultiLine().Should().Be($"ObjectType=NetworkVisor.Platform.Test.Shared.UnitTests.Logging.Extensions.CoreLoggerExtensionsUnitTests.TestLoggable{Environment.NewLine}ParentStringValue=ParentTestString{Environment.NewLine}StringValue=TestString");
        }

        /// <summary>
        /// Defines the test method CoreLoggable_GetLogPropertyListLevel_Null.
        /// </summary>
        [Fact]
        [ExcludeFromCodeCoverage]
        public void CoreLoggable_GetLogPropertyListLevel_Null()
        {
            Func<IEnumerable<ICoreLogPropertyLevel>> fx = () =>
            {
                var testLoggableEmpty = new TestLoggableEmpty(this.TestCaseLogger);

                return testLoggableEmpty.GetLogPropertyListLevel(null!);
            };

            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logPropertyListFormatter");
        }

        /// <summary>
        /// Defines the test method CoreLoggable_FormatLogString_Null.
        /// </summary>
        [Fact]
        [ExcludeFromCodeCoverage]
        public void CoreLoggable_FormatLogString_Null()
        {
            Func<StringBuilder> fx = () =>
            {
                TestLoggable testLoggable = CreateTestLoggable(this.TestCaseLogger);

                return testLoggable.FormatLogString(null, null!);
            };

            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logPropertyListFormatter");
        }

        /// <summary>
        /// Defines the test method CoreLoggable_ToString_LogPropertyListFormatter.
        /// </summary>
        [Fact]
        public void CoreLoggable_ToString_LogPropertyListFormatter()
        {
            TestLoggable testLoggable = CreateTestLoggable(this.TestCaseLogger);
            testLoggable.ToString(new CoreLogPropertyListFormatter()).Should().Be("TestString");
        }

        /// <summary>
        /// Defines the test method CoreLogger_BeginPropertyScope_Property_Object_Null.
        /// </summary>
        [Fact]
        public void CoreLogger_BeginPropertyScope_Property_Object_Null()
        {
            Func<IDisposable?> fx = () => ((ICoreTestCaseLogger)null).BeginPropertyScope(TestProperty, TestValue);

            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");

            fx = () => this.TestCaseLogger!.BeginPropertyScope(null, TestValue);

            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("propertyName");

            fx = () => this.TestCaseLogger!.BeginPropertyScope(TestProperty, (string)null);

            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("value");
        }

        /// <summary>
        /// Creates the test property list.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="loggableFormat">The loggable format.</param>
        /// <returns>IEnumerable&lt;KeyValuePair&lt;System.String, System.Object&gt;&gt;.</returns>
        private static IEnumerable<KeyValuePair<string, object>> CreateTestPropertyList(LogLevel logLevel, CoreLoggableFormatFlags loggableFormat)
        {
            var testPropertyList = new List<KeyValuePair<string, object>>();

            if (logLevel <= LogLevel.Information)
            {
                testPropertyList.Add(new KeyValuePair<string, object>(TestProperty, TestValue));
            }

            if (loggableFormat.IsWithParents() && logLevel <= LogLevel.Debug)
            {
                testPropertyList.Add(new KeyValuePair<string, object>(TestPropertyParent, ParentTestValue));
            }

            return testPropertyList;
        }

        /// <summary>
        /// Creates a Loggable Dictionary for testing.
        /// </summary>
        /// <param name="testLoggable">Object to test.</param>
        /// <returns>Dictionary.</returns>
        private static Dictionary<string, object> CreateLoggableDictionary(TestLoggable testLoggable)
        {
            return new Dictionary<string, object>
            {
                { "item1", new CoreLogProperty("prop1", "value1", CoreLogPropertyType.StringValue) },
                { "item2", testLoggable },
                { "item3", new[] { "value2a", "value2b" } },
                { "item4", Array.Empty<string>() },
                { "item5", new[] { testLoggable } },
            };
        }

        /// <summary>
        /// Creates a CoreLogProperty for testing.
        /// </summary>
        /// <param name="testLoggable">Object to test.</param>
        /// <returns>Dictionary.</returns>
        private static CoreLogProperty CreateTestCoreLogProperty(TestLoggable testLoggable)
        {
            object[] loggableArray = new object[]
            {
                1,
                "2",
                new string[] { "3", "4" },
                testLoggable,
                new Dictionary<string, object>
                {
                    { "item1", new CoreLogProperty("prop1", "value1", CoreLogPropertyType.StringValue) },
                    { "item2", testLoggable },
                    { "item3", new[] { "value2a", "value2b" } },
                    { "item4", Array.Empty<string>() },
                    { "item5", new[] { testLoggable } },
                },
            };

            var coreLogProperty = new CoreLogProperty("property", loggableArray, CoreLogPropertyType.Array);
            coreLogProperty.Should().NotBeNull();
            coreLogProperty.Key.Should().Be("property");
            coreLogProperty.Value.Should().BeSameAs(loggableArray).And.Subject.Should().BeAssignableTo<object[]>();

            return coreLogProperty;
        }

        /// <summary>
        /// Creates the test loggable.
        /// </summary>
        /// <param name="testLogger">test logger.</param>
        /// <param name="testString">The test string.</param>
        /// <returns>TestLoggable.</returns>
        private static TestLoggable CreateTestLoggable(ICoreLogger testLogger, string testString = TestValue)
        {
            return new TestLoggable(testString, testLogger);
        }

        /// <summary>
        /// Class TestCoreLoggable.
        /// Implements the <see cref="ICoreLoggable" />.
        /// </summary>
        /// <seealso cref="ICoreLoggable" />
        [ExcludeFromCodeCoverage]
        private class TestCoreLoggable : ICoreLoggable
        {
            /// <inheritdoc />
            public ICoreLogger Logger { get; set; } = null;

            /// <inheritdoc />
            public string ToString(ICoreLogPropertyListFormatter logPropertyListFormatter)
            {
                return string.Empty;
            }

            /// <inheritdoc />
            public string ToString(CoreLoggableFormatFlags loggableFormat, LogLevel? logLevel = null)
            {
                return string.Empty;
            }

            /// <inheritdoc />
            public StringBuilder FormatLogString(StringBuilder? sb, ICoreLogPropertyListFormatter logPropertyListFormatter)
            {
                return sb ?? new StringBuilder();
            }

            /// <inheritdoc />
            public IEnumerable<ICoreLogPropertyLevel> GetLogPropertyListLevel(ICoreLogPropertyListFormatter logPropertyListFormatter)
            {
                return [];
            }
        }

        /// <summary>
        /// Class TestLoggableParent.
        /// Implements the <see cref="CoreLoggableBase" />.
        /// </summary>
        /// <seealso cref="CoreLoggableBase" />
        [ExcludeFromCodeCoverage]
        private class TestLoggableParent : CoreLoggableBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TestLoggableParent"/> class.
            /// </summary>
            /// <param name="parentStringValue">The parent string value.</param>
            /// <param name="logger">The logger.</param>
            public TestLoggableParent(string parentStringValue, ICoreLogger? logger = null)
                : base(logger)
            {
                this.ParentStringValue = parentStringValue;
            }

            /// <summary>
            /// Gets the parent string value.
            /// </summary>
            /// <value>The parent string value.</value>
            public string ParentStringValue { get; }

            /// <inheritdoc />
            public override IEnumerable<ICoreLogPropertyLevel> GetLogPropertyListLevel(ICoreLogPropertyListFormatter logPropertyListFormatter)
            {
                using ICoreLogPropertyScope logPropertyScope = logPropertyListFormatter.AddLogPropertyScope();

                List<ICoreLogPropertyLevel> logPropertyLevels = logPropertyListFormatter.IsWithParents
                    ? base.GetLogPropertyListLevel(logPropertyListFormatter).ToList()
                    : [];

                if (logPropertyListFormatter.MinimumLogLevel <= LogLevel.Debug)
                {
                    logPropertyLevels.Add(new CoreLogPropertyLevel(nameof(this.ParentStringValue), this.ParentStringValue, LogLevel.Information, CoreLogPropertyType.StringValue, logPropertyScope.CurrentScopeLevel));
                }

                return logPropertyLevels.Where(p => p.MinimumLogLevel.CompareTo(logPropertyListFormatter.MinimumLogLevel) >= 0);
            }
        }

        /// <summary>
        /// Class TestLoggable.
        /// Implements the <see cref="TestLoggableParent" />.
        /// </summary>
        /// <seealso cref="TestLoggableParent" />
        [ExcludeFromCodeCoverage]
        private class TestLoggable : TestLoggableParent
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TestLoggable"/> class.
            /// </summary>
            /// <param name="stringValue">The string value.</param>
            /// <param name="logger">The logger.</param>
            public TestLoggable(string stringValue, ICoreLogger logger)
                : base(ParentTestValue, logger)
            {
                this.StringValue = stringValue;
            }

            /// <summary>
            /// Gets or sets the string value.
            /// </summary>
            /// <value>The string value.</value>
            public string StringValue { get; set; }

            /// <inheritdoc />
            public override IEnumerable<ICoreLogPropertyLevel> GetLogPropertyListLevel(ICoreLogPropertyListFormatter logPropertyListFormatter)
            {
                using ICoreLogPropertyScope logPropertyScope = logPropertyListFormatter.AddLogPropertyScope();

                List<ICoreLogPropertyLevel> logPropertyLevels = logPropertyListFormatter.IsWithParents
                    ? base.GetLogPropertyListLevel(logPropertyListFormatter).ToList()
                    : [];

                if (logPropertyListFormatter.MinimumLogLevel <= LogLevel.Information)
                {
                    logPropertyLevels.Add(new CoreLogPropertyLevel(nameof(this.StringValue), this.StringValue, LogLevel.Information, CoreLogPropertyType.StringValue, logPropertyScope.CurrentScopeLevel));
                }

                return logPropertyLevels.Where(p => p.MinimumLogLevel.CompareTo(logPropertyListFormatter.MinimumLogLevel) >= 0);
            }
        }

        /// <summary>
        /// Class TestLoggable.
        /// Implements the <see cref="TestLoggableEmpty" />.
        /// </summary>
        /// <seealso cref="TestLoggableEmpty" />
        [ExcludeFromCodeCoverage]
        private class TestLoggableEmpty : CoreLoggableBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TestLoggableEmpty"/> class.
            /// </summary>
            /// <param name="logger">The logger.</param>
            public TestLoggableEmpty(ICoreLogger? logger)
                : base(logger)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="TestLoggableEmpty"/> class.
            /// </summary>
            public TestLoggableEmpty()
                : this(null)
            {
            }
        }
    }
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
}

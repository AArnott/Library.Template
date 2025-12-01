// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreTypeExtensionsUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using FluentAssertions;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Core.Utilities;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using NetworkVisor.Platform.Test.TestDevices;
using NetworkVisor.Platform.Test.TestObjects;
using Xunit;
using TypeExtensions = NetworkVisor.Core.Extensions.TypeExtensions;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Extensions
{
    /// <summary>
    /// Class CoreTypeExtensionsUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreTypeExtensionsUnitTests))]

    public class CoreTypeExtensionsUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTypeExtensionsUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreTypeExtensionsUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreTypeExtensionsUnit_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Unit);
        }

        /// <summary>
        /// Defines the test method GetDisplayFullName_BaseTypes.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="result">The result.</param>
        [Theory]
        [InlineData(typeof(bool), "bool")]
        [InlineData(typeof(byte), "byte")]
        [InlineData(typeof(char), "char")]
        [InlineData(typeof(decimal), "decimal")]
        [InlineData(typeof(double), "double")]
        [InlineData(typeof(float), "float")]
        [InlineData(typeof(int), "int")]
        [InlineData(typeof(long), "long")]
        [InlineData(typeof(object), "object")]
        [InlineData(typeof(sbyte), "sbyte")]
        [InlineData(typeof(short), "short")]
        [InlineData(typeof(string), "string")]
        [InlineData(typeof(uint), "uint")]
        [InlineData(typeof(ulong), "ulong")]
        [InlineData(typeof(ushort), "ushort")]
        public void GetDisplayFullName_BaseTypes(Type type, string result)
        {
            type.GetDisplayFullName().Should().Be(result);
        }

        /// <summary>
        /// Defines the test method GetDisplayFullName_Null.
        /// </summary>
        [Fact]
        public void GetDisplayFullName_Null()
        {
            Func<string> fx = () => TypeExtensions.GetDisplayFullName(null);
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("type");
        }

        /// <summary>
        /// Defines the test method GetDisplayFullName_Null.
        /// </summary>
        [Fact]
        public void GetDisplayFullName_FileSystem_Null()
        {
            Func<string> fx = () => TypeExtensions.GetDisplayFullName(null, null);
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("type");

            fx = () => typeof(CoreTypeExtensionsUnitTests).GetDisplayFullName(null);
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("fileSystem");
        }

        /// <summary>
        /// Defines the test method GetDisplayFullName_Class.
        /// </summary>
        [Fact]
        public void GetDisplayFullName_Class()
        {
            this.TestOutputHelper.WriteLine($"GetDisplayFullName: {typeof(CoreTypeExtensionsUnitTests).GetDisplayFullName()}");
            typeof(CoreTypeExtensionsUnitTests).GetDisplayFullName().Should().Be("NetworkVisor.Platform.Test.Shared.UnitTests.Extensions.CoreTypeExtensionsUnitTests");
        }

        /// <summary>
        /// Defines the test method GetDisplayShortName_Class.
        /// </summary>
        [Fact]
        public void GetDisplayShortName_Class()
        {
            this.TestOutputHelper.WriteLine($"GetDisplayShortName: {typeof(CoreTypeExtensionsUnitTests).GetDisplayShortName()}");
            typeof(CoreTypeExtensionsUnitTests).GetDisplayShortName().Should().Be("CoreTypeExtensionsUnitTests");
        }

        /// <summary>
        /// Defines the test method GetDisplayFullName_Class_FileSystem.
        /// </summary>
        [Fact]
        public void GetDisplayFullName_Class_FileSystem()
        {
            typeof(CoreTypeExtensionsUnitTests).GetDisplayFullName(this.TestFileSystem).Should().Be(Path.Combine("NetworkVisor", "Platform", "Test", "Shared", "UnitTests", "Extensions", "CoreTypeExtensionsUnitTests"));
        }

        /// <summary>
        /// Defines the test method GetDisplayFullName_Struct.
        /// </summary>
        [Fact]
        public void GetDisplayFullName_Struct()
        {
            typeof(TestStruct).GetDisplayFullName().Should().Be("NetworkVisor.Platform.Test.Shared.UnitTests.Extensions.CoreTypeExtensionsUnitTests.TestStruct");
        }

        /// <summary>
        /// Defines the test method GetDisplayFullName_Generic.
        /// </summary>
        [Fact]
        public void GetDisplayFullName_Generic()
        {
            typeof(IList<string>).GetDisplayFullName().Should().Be("System.Collections.Generic.IList");
        }

        /// <summary>
        /// Defines the test method IsSharedNamespace.
        /// </summary>
        [Fact]
        public void IsSharedNamespace()
        {
            this.TestClassType.IsSharedNamespace().Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method IsSharedNamespace_Null.
        /// </summary>
        [Fact]
        public void IsSharedNamespace_Null()
        {
            Func<bool> fx = () => TypeExtensions.IsSharedNamespace(null);
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("type");
        }

        /// <summary>
        /// Defines the test method GetAssemblyNamespace_Output.
        /// </summary>
        [Fact]
        public void GetAssemblyNamespace_Output()
        {
            this.TestOutputHelper.WriteLine(typeof(CoreTypeExtensionsUnitTests).GetAssemblyNamespace());
        }

        /// <summary>
        /// Defines the test method AssemblyExtensions_GetAssemblyAndExportedTypeFromString.
        /// </summary>
        /// <param name="typeString">Type as a string.</param>
        /// <param name="expectedType">Expected parsed type.</param>
        [Theory]
        [InlineData("", null)]
        [InlineData(null, null)]

        [InlineData("NetworkVisor.Core.CoreSystem.CoreFrameworkInfo", typeof(CoreFrameworkInfo))]
        [InlineData("NetworkVisor.Core.CoreSystem.CoreFrameworkInfoBase", typeof(CoreFrameworkInfoBase))]
        [InlineData("NetworkVisor.Core.CoreSystem.CoreFrameworkInfo NetworkVisor.Platform.NetCore", typeof(CoreFrameworkInfo))]
        [InlineData("NetworkVisor.Core.CoreSystem.CoreFrameworkInfo NetworkVisor.Platform.Unknown", typeof(CoreFrameworkInfo))]

        [InlineData("NetworkVisor.Platform.Test.TestObjects.CoreTestObject", typeof(CoreTestObject))]

        [InlineData("CoreTestNetworkDevice`1", typeof(CoreTestNetworkDevice<CoreAssemblyExtensionsUnitTests>))]
        [InlineData("CoreTestSerializableObject`1", typeof(CoreTestSerializableObject<CoreAssemblyExtensionsUnitTests>))]
        [InlineData("CoreTestLocalNetworkDevice`1", typeof(CoreTestLocalNetworkDevice<CoreAssemblyExtensionsUnitTests>))]

        [InlineData("NetworkVisor.Core.CoreSystem.FooBar", null)]
        [InlineData("NetworkVisor.Core.FooBar", null)]
        [InlineData("NetworkVisor.FooBar", null)]
        [InlineData("FooBar", null)]
        public void TypeExtensions_GetPlatformTypeFromAssembly(string? typeString, Type? expectedType)
        {
            if (typeString is not null)
            {
                switch (typeString)
                {
                    case "CoreTestNetworkDevice`1":
                        {
                            typeString = typeof(CoreTestNetworkDevice<CoreAssemblyExtensionsUnitTests>).FullName;
                            break;
                        }

                    case "CoreTestSerializableObject`1":
                        {
                            typeString = typeof(CoreTestSerializableObject<CoreAssemblyExtensionsUnitTests>).FullName;
                            break;
                        }

                    case "CoreTestLocalNetworkDevice`1":
                        {
                            typeString = typeof(CoreTestLocalNetworkDevice<CoreAssemblyExtensionsUnitTests>).FullName;
                            break;
                        }
                }
            }

            (Type? Type, Exception? Exception) result = TypeExtensions.GetPlatformTypeFromAssembly(typeString);

            this.TestOutputHelper.WriteLine($"Type: {typeString}");

            result.Should().NotBeNull();

            if (string.IsNullOrEmpty(typeString))
            {
                result.Exception.Should().BeOfType<ArgumentNullException>();
            }
            else if (typeString!.Contains("FooBar"))
            {
                result.Exception.Should().BeOfType<ArgumentException>();
            }
            else
            {
                result.Exception.Should().BeNull();
                result.Type.Should().NotBeNull();

                result.Type.Should().Be(expectedType);

                if (!result.Type!.IsAbstract)
                {
                    object? instance = TypeExtensions.CreateInstanceWithServiceProvider(result.Type!, this.TestCaseServiceProvider, this.TestCaseLogger);
                    instance.Should().NotBeNull().And.Subject.Should().BeOfType(result.Type);

                    if (instance is ICoreLoggable loggable)
                    {
                        this.TestOutputHelper.WriteLine($"Instance:\n{loggable.ToStringWithParentsPropNameMultiLine()}");
                    }
                    else
                    {
                        this.TestOutputHelper.WriteLine($"Instance:\n{instance}");
                    }
                }
            }
        }

        /// <summary>
        /// Struct TestStruct.
        /// </summary>
        private struct TestStruct
        {
        }
    }
}

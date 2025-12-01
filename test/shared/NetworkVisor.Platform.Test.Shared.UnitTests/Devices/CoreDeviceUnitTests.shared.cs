// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// ***********************************************************************
// <copyright file="CoreDeviceUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.CoreObject;
using NetworkVisor.Core.Device;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Logging.Loggable;
using NetworkVisor.Core.Logging.LogProperty;
using NetworkVisor.Core.Networking.Devices;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Devices
{
    /// <summary>
    /// Class CoreDeviceUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreDeviceUnitTests))]

    public class CoreDeviceUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreDeviceUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreDeviceUnitTests(CoreTestClassFixture testClassFixture)
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
        }

        [Fact]
        public void Device_DeviceId_SameAs_ObjectId()
        {
            ICoreDevice deviceTest = this.CreateTestDevice(CoreDeviceType.UnknownDevice, this.TestCaseLogger);
            deviceTest.DeviceID.Should().Be(deviceTest.ObjectId);
        }

        /// <summary>
        /// Defines the test method Device_CompareTo_DeviceBase.
        /// </summary>
        [Fact]
        public void Device_CompareTo_DeviceBase()
        {
            ICoreDevice deviceTest = this.CreateTestDevice(CoreDeviceType.UnknownDevice, this.TestCaseLogger);
            ICoreDevice deviceTest1 = this.CreateTestDevice(CoreDeviceType.UnknownDevice, this.TestCaseLogger);

            deviceTest.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreDevice>();
            deviceTest1.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreDevice>();

            deviceTest!.CompareTo(deviceTest).Should().Be(0);
            deviceTest!.CompareTo(deviceTest1).Should().Be(0);
        }

        /// <summary>
        /// Defines the test method Device_CompareTo_ICoreObject.
        /// </summary>
        [Fact]
        public void Device_CompareTo_ICoreObject()
        {
            ICoreDevice deviceTest = this.CreateTestDevice(CoreDeviceType.UnknownDevice, this.TestCaseLogger);
            ICoreDevice deviceTest1 = this.CreateTestDevice(CoreDeviceType.UnknownDevice, this.TestCaseLogger);

            deviceTest.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreDevice>();
            deviceTest1.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreDevice>();

            deviceTest.CompareTo((ICoreObject)deviceTest).Should().Be(0);
            deviceTest.CompareTo((ICoreObject)deviceTest1).Should().Be(0);

            var coreObject = new ObjectTest(this.TestCaseLogger);

            Func<int> fx = () => deviceTest.CompareTo(coreObject!);
            fx.Should().Throw<ArgumentException>().And.Message.Should().Be("Object must be of type ICoreDevice.");
        }

        /// <summary>
        /// Defines the test method Device_CompareTo_Null.
        /// </summary>
        [Fact]
        public void Device_CompareTo_Null()
        {
            ICoreDevice deviceTest = this.CreateTestDevice(CoreDeviceType.UnknownDevice, this.TestCaseLogger);
            deviceTest.Should().NotBeNull();

            deviceTest!.CompareTo(null!).Should().Be(1);
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            deviceTest!.CompareTo((ICoreObject?)null).Should().Be(1);
#pragma warning restore IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }
#pragma warning restore IDE0079 // Remove unnecessary suppression

        /// <summary>
        /// Defines the test method Device_Equals_ICoreObject.
        /// </summary>
        [Fact]
        public void Device_Equals_ICoreObject()
        {
            ICoreDevice deviceTest = this.CreateTestDevice(CoreDeviceType.UnknownDevice, this.TestCaseLogger);
            ICoreDevice deviceTest1 = this.CreateTestDevice(CoreDeviceType.UnknownDevice, this.TestCaseLogger);

            deviceTest.Should().NotBeNull();
            deviceTest1.Should().NotBeNull();

            deviceTest!.Equals((ICoreObject)deviceTest).Should().BeTrue();
            deviceTest!.Equals((ICoreObject)deviceTest1).Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method Device_Equals_Null.
        /// </summary>
        [Fact]
        public void Device_Equals_Null()
        {
            ICoreDevice deviceTest = this.CreateTestDevice(CoreDeviceType.UnknownDevice, this.TestCaseLogger);
            deviceTest.Should().NotBeNull();

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            deviceTest!.Equals(null).Should().BeFalse();
#pragma warning restore IDE0079 // Remove unnecessary suppression
            deviceTest!.Equals((ICoreObject?)null).Should().BeFalse();
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        /// <summary>
        /// Defines the test method Device_GetLogPropertyListLevel_Null.
        /// </summary>
        [Fact]
        public void Device_GetLogPropertyListLevel_Null()
        {
            ICoreDevice deviceTest = this.CreateTestDevice(CoreDeviceType.UnknownDevice, this.TestCaseLogger);

            Func<IEnumerable<ICoreLogPropertyLevel>> fx = () => deviceTest!.GetLogPropertyListLevel(null!);

            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logPropertyListFormatter");
        }

        /// <summary>
        /// Defines the test method Device_ToLog.
        /// </summary>
        [Fact]
        public void Device_ToLog()
        {
            ICoreDevice deviceTest = this.CreateTestDevice(CoreDeviceType.UnknownDevice, this.TestCaseLogger);

            this.TestOutputHelper.WriteLine(deviceTest.ToString(CoreLoggableFormatFlags.ToLog, LogLevel.Debug));
        }

        /// <summary>
        /// Defines the test method Device_ToLogParents.
        /// </summary>
        [Fact]
        public void Device_ToLogParents()
        {
            ICoreDevice deviceTest = this.CreateTestDevice(CoreDeviceType.UnknownDevice, this.TestCaseLogger);

            this.TestOutputHelper.WriteLine(deviceTest.ToString(CoreLoggableFormatFlags.ToLogWithParents, LogLevel.Debug));
        }

        /// <summary>
        /// Defines the test method Device_ToString.
        /// </summary>
        [Fact]
        public void Device_ToString()
        {
            ICoreDevice deviceTest = this.CreateTestDevice(CoreDeviceType.UnknownDevice, this.TestCaseLogger);

            this.TestOutputHelper.WriteLine(deviceTest.ToString(CoreLoggableFormatFlags.ToString, LogLevel.Debug));
        }

        /// <summary>
        /// Defines the test method Device_ToStringParents.
        /// </summary>
        [Fact]
        public void Device_ToStringParents()
        {
            ICoreDevice deviceTest = this.CreateTestDevice(CoreDeviceType.UnknownDevice, this.TestCaseLogger);

            this.TestOutputHelper.WriteLine(deviceTest.ToString(CoreLoggableFormatFlags.ToStringWithParents, LogLevel.Debug));
        }

        /// <summary>
        /// Defines the test method Device_ToStringMultiLine.
        /// </summary>
        [Fact]
        public void Device_ToStringMultiLine()
        {
            ICoreDevice deviceTest = this.CreateTestDevice(CoreDeviceType.UnknownDevice, this.TestCaseLogger);

            this.TestOutputHelper.WriteLine(deviceTest.ToString(CoreLoggableFormatFlags.ToStringWithMultiLine, LogLevel.Debug));
        }

        /// <summary>
        /// Defines the test method Device_ToStringParentsMultiLine.
        /// </summary>
        [Fact]
        public void Device_ToStringParentsMultiLine()
        {
            ICoreDevice deviceTest = this.CreateTestDevice(CoreDeviceType.UnknownDevice, this.TestCaseLogger);

            this.TestOutputHelper.WriteLine(deviceTest.ToString(CoreLoggableFormatFlags.ToStringWithParentsMultiLine, LogLevel.Debug));
        }

        /// <summary>
        /// Defines the test method Device_ToStringParentsPropName.
        /// </summary>
        [Fact]
        public void Device_ToStringParentsPropName()
        {
            ICoreDevice deviceTest = this.CreateTestDevice(CoreDeviceType.UnknownDevice, this.TestCaseLogger);

            this.TestOutputHelper.WriteLine(deviceTest.ToString(CoreLoggableFormatFlags.ToStringWithParentsPropName, LogLevel.Debug));
        }

        /// <summary>
        /// Defines the test method Device_ToStringPropNameMultiLine.
        /// </summary>
        [Fact]
        public void Device_ToStringPropNameMultiLine()
        {
            ICoreDevice deviceTest = this.CreateTestDevice(CoreDeviceType.UnknownDevice, this.TestCaseLogger);

            this.TestOutputHelper.WriteLine(deviceTest.ToString(CoreLoggableFormatFlags.ToStringWithPropNameMultiLine, LogLevel.Debug));
        }

        /// <summary>
        /// Defines the test method Device_ToStringParentsPropNameMultiLine.
        /// </summary>
        [Fact]
        public void Device_ToStringParentsPropNameMultiLine()
        {
            ICoreDevice deviceTest = this.CreateTestDevice(CoreDeviceType.UnknownDevice, this.TestCaseLogger);

            this.TestOutputHelper.WriteLine(deviceTest.ToString(CoreLoggableFormatFlags.ToStringWithParentsPropNameMultiLine, LogLevel.Debug));
        }

        /// <summary>
        /// Creates a test device of the specified type and associates it with the provided test case logger.
        /// </summary>
        /// <param name="deviceType">The type of the device to be created.</param>
        /// <param name="testCaseLogger">The logger to be used for logging test case information.</param>
        /// <returns>An instance of <see cref="ICoreDevice"/> representing the created test device.</returns>
        private ICoreDevice CreateTestDevice(CoreDeviceType deviceType, ICoreTestCaseLogger testCaseLogger) => new DeviceTest(deviceType, this.TestCaseLogger);

        /// <summary>
        /// Class ObjectTest.
        /// Implements the <see cref="CoreObjectBase" />.
        /// </summary>
        /// <seealso cref="CoreObjectBase" />
        private class ObjectTest : CoreObjectBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ObjectTest"/> class.
            /// </summary>
            /// <param name="logger">The logger.</param>
            public ObjectTest(ICoreLogger? logger)
            : base(logger)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ObjectTest"/> class.
            /// </summary>
            public ObjectTest()
                : this(null)
            {
            }
        }

        /// <summary>
        /// Class DeviceTest.
        /// Implements the <see cref="CoreDeviceBase{T}" />.
        /// </summary>
        /// <seealso cref="CoreDeviceBase{T}" />
        private class DeviceTest : CoreDeviceBase<DeviceTest>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DeviceTest"/> class.
            /// </summary>
            /// <param name="deviceType">Type of the device.</param>
            /// <param name="logger">The logger.</param>
            public DeviceTest(CoreDeviceType deviceType, ICoreLogger logger)
                : base(Guid.NewGuid(), deviceType, logger)
            {
            }
        }
    }
}

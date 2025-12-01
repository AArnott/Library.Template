// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-13-2020
// ***********************************************************************
// <copyright file="CoreObjectBaseUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using FluentAssertions;
using NetworkVisor.Core.CoreObject;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Logging.LogProperty;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.CoreObject
{
    /// <summary>
    /// Class CoreObjectBaseUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreObjectBaseUnitTests))]

    public class CoreObjectBaseUnitTests : CoreTestCaseBase
    {
        private const string TestProp = "Test";
        private const string TestStringValue1 = "Value1";
        private const string TestStringValue2 = "Value2";
        private const string IPAddressProp = "IPAddress";
        private const string PhysicalAddressProp = "PhysicalAddress";

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreObjectBaseUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreObjectBaseUnitTests(CoreTestClassFixture testClassFixture)
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

        /// <summary>
        /// Defines the test method CoreObjectBase_Ctor.
        /// </summary>
        [Fact]
        public void CoreObjectBase_Ctor()
        {
            var objectBase1 = new CoreObjectTest(this.TestCaseLogger);
            objectBase1.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreObject>();
            objectBase1.CreatedTimestamp.Should().Be(objectBase1.ModifiedTimestamp);
            objectBase1.Logger.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreLogger>();
            objectBase1.ObjectVersion.Should().Be(1);
        }

        /// <summary>
        /// Defines the test method CoreObjectBase_Clone.
        /// </summary>
        [Fact]
        public void CoreObjectBase_Clone()
        {
            var objectBase1 = new CoreObjectTest(this.TestCaseLogger);
            ICoreObject objectBase2 = objectBase1.CloneInstance();

            // Objects version is the same but everyone else
            objectBase1.Equals(objectBase2).Should().BeFalse();
            objectBase1.IsSameObjectId(objectBase2).Should().BeFalse();
            objectBase1.IsSameObjectVersion(objectBase2).Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method CoreObjectBase_CompareTo.
        /// </summary>
        [Fact]
        public void CoreObjectBase_CompareTo()
        {
            var objectBase = new CoreObjectTest(this.TestCaseLogger);

            objectBase.CompareTo(null).Should().Be(1);
        }

        /// <summary>
        /// Defines the test method CoreObjectBase_Comparison.
        /// </summary>
        [Fact]
        public void CoreObjectBase_Comparison()
        {
            var objectBase1 = new CoreObjectTest(this.TestCaseLogger);
            var objectBase2 = new CoreObjectTest(this.TestCaseLogger);

            objectBase1.Equals(objectBase2).Should().BeFalse();
            objectBase1.IsSameObjectId(objectBase2).Should().BeFalse();
            objectBase1.IsSameObjectVersion(objectBase2).Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method CoreObjectBase_CopyInstance.
        /// </summary>
        [Fact]
        public void CoreObjectBase_CopyInstance()
        {
            var objectBase1 = new CoreObjectTest(this.TestCaseLogger);
            ICoreObject? objectBase2 = objectBase1.CopyInstance();

            // Objects version is the same but everyone else
            objectBase1.Equals(objectBase2).Should().BeTrue();
            objectBase1.IsSameObjectId(objectBase2).Should().BeTrue();
            objectBase1.IsSameObjectVersion(objectBase2).Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method CoreObjectBase_Equals_CloneInstance.
        /// </summary>
        [Fact]
        public void CoreObjectBase_Equals_CloneInstance()
        {
            var objectBase1 = new CoreObjectTest(this.TestCaseLogger);
            ICoreObject objectBase2 = objectBase1.CloneInstance();

            // ObjectId's are same but Creation and Modification dates are different
            objectBase1.Equals(objectBase2).Should().BeFalse();
            objectBase1.IsSameObjectId(objectBase2).Should().BeFalse();
            objectBase1.IsSameObjectVersion(objectBase2).Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method CoreObjectBase_Equals_CopyInstance.
        /// </summary>
        [Fact]
        public void CoreObjectBase_Equals_CopyInstance()
        {
            var objectBase1 = new CoreObjectTest(this.TestCaseLogger);
            ICoreObject? objectBase2 = objectBase1.CopyInstance();

            // ObjectId's are same but Creation and Modification dates are different
            objectBase1.Equals(objectBase2).Should().BeTrue();
            objectBase1.IsSameObjectId(objectBase2).Should().BeTrue();
            objectBase1.IsSameObjectVersion(objectBase2).Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method CoreObjectBase_GetHashCode.
        /// </summary>
        [Fact]
        public void CoreObjectBase_GetHashCode()
        {
            var objectBase = new CoreObjectTest(this.TestCaseLogger);

            objectBase.GetHashCode().Should().Be(objectBase.ObjectId.GetHashCode());
        }

        /// <summary>
        /// Defines the test method CoreObjectBase_IncrementObjectVersion.
        /// </summary>
        [Fact]
        public void CoreObjectBase_IncrementObjectVersion()
        {
            var objectBase1 = new CoreObjectTest(this.TestCaseLogger);
            ulong objectVersion = objectBase1.ObjectVersion;

            objectBase1.IncreaseObjectVersion().Should().Be(objectVersion + 1);
            objectBase1.ObjectVersion.Should().Be(objectVersion + 1);
        }

        /// <summary>
        /// Defines the test method CoreObjectBase_Null.
        /// </summary>
        [Fact]
        public void CoreObjectBase_Null()
        {
            var objectBase1 = new CoreObjectTest(this.TestCaseLogger);
            objectBase1.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreObject>();
            objectBase1.CreatedTimestamp.Should().Be(objectBase1.ModifiedTimestamp);
            objectBase1.Logger.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreLogger>();
            objectBase1.ObjectVersion.Should().Be(1);
        }

        /// <summary>
        /// Defines the test method CoreObjectBase_ToString.
        /// </summary>
        [Fact]
        public void CoreObjectBase_ToString()
        {
            var objectBase = new CoreObjectTest(this.TestCaseLogger);

            this.TestOutputHelper.WriteLine(objectBase.ToString());
        }

        /// <summary>
        /// Defines the test method CoreObjectBase_ToString_Parents.
        /// </summary>
        [Fact]
        public void CoreObjectBase_ToString_Parents()
        {
            var objectBase = new CoreObjectTest(this.TestCaseLogger);

            this.TestOutputHelper.WriteLine(objectBase.ToStringWithParentsPropName());
        }

        /// <summary>
        /// Defines the test method CoreObjectBase_ToStringMultiLine.
        /// </summary>
        [Fact]
        public void CoreObjectBase_ToStringMultiLine()
        {
            var objectBase = new CoreObjectTest(this.TestCaseLogger);

            this.TestOutputHelper.WriteLine(objectBase.ToStringWithMultiLine());
        }

        /// <summary>
        /// Defines the test method CoreObjectBase_ToString_ParentsMultiLine.
        /// </summary>
        [Fact]
        public void CoreObjectBase_ToString_ParentsMultiLine()
        {
            var objectBase = new CoreObjectTest(this.TestCaseLogger);

            this.TestOutputHelper.WriteLine(objectBase.ToStringWithParentsMultiLine());
        }

        /// <summary>
        /// Defines the test method CoreObjectBase_ToString_ParentsPropNameMultiLine.
        /// </summary>
        [Fact]
        public void CoreObjectBase_ToString_ParentsPropNameMultiLine()
        {
            var objectBase = new CoreObjectTest(this.TestCaseLogger);

            this.TestOutputHelper.WriteLine(objectBase.ToStringWithParentsPropNameMultiLine());
        }

        /// <summary>
        /// Defines the test method CoreObjectBase_UpdateInstance_CopyInstance.
        /// </summary>
        [Fact]
        public void CoreObjectBase_UpdateInstance_CopyInstance()
        {
            var objectBase = new CoreObjectTest(this.TestCaseLogger);
            ICoreObject? objectBaseClone = objectBase.CopyInstance();
            objectBaseClone.Should().NotBeNull();
            objectBase.IncreaseObjectVersion();

            objectBaseClone!.UpdateObjectVersion(objectBase)?.ObjectVersion.Should().Be(objectBaseClone.ObjectVersion);
        }

        /// <summary>
        /// Defines the test method CoreObjectBase_UpdateInstance_DifferentObject.
        /// </summary>
        [Fact]
        public void CoreObjectBase_UpdateInstance_DifferentObject()
        {
            var objectBase = new CoreObjectTest(this.TestCaseLogger);
            var objectBaseClone = new CoreObjectTest(this.TestCaseLogger);

            objectBaseClone.UpdateObjectVersion(objectBase).ObjectVersion.Should().Be(objectBaseClone.ObjectVersion);
        }

        /// <summary>
        /// Defines the test method CoreObjectBase_UpdateInstance_Null.
        /// </summary>
        [Fact]
        public void CoreObjectBase_UpdateInstance_Null()
        {
            var objectBase = new CoreObjectTest(this.TestCaseLogger);

            Func<ICoreObject> fx = () => objectBase.UpdateObjectVersion(null);
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("objectVersion");
        }

        /// <summary>
        /// Defines the test method CoreObjectBase_UpdateInstance_SameObject.
        /// </summary>
        [Fact]
        public void CoreObjectBase_UpdateInstance_SameObject()
        {
            var objectBase = new CoreObjectTest(this.TestCaseLogger);
            ulong objectVersion = objectBase.ObjectVersion;

            objectBase.UpdateObjectVersion(objectBase).ObjectVersion.Should().Be(objectVersion + 1);
        }

        /// <summary>
        /// Defines the test method CoreObjectBase_Equals.
        /// </summary>
        [Fact]
        public void CoreObjectBase_Equals()
        {
            var objectBase = new CoreObjectTest(this.TestCaseLogger);
            var objectBaseClone = new CoreObjectTest(this.TestCaseLogger);

            objectBase.Equals(objectBaseClone).Should().BeFalse();
            objectBase.Equals((object)objectBaseClone).Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method CoreObjectBase_GetLogPropertyListLevel_Null.
        /// </summary>
        [Fact]
        [ExcludeFromCodeCoverage]
        public void CoreObjectBase_GetLogPropertyListLevel_Null()
        {
            Func<IEnumerable<ICoreLogPropertyLevel>> fx = () =>
            {
                var objectBase = new CoreObjectTest(this.TestCaseLogger);

                return objectBase.GetLogPropertyListLevel(null!);
            };

            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logPropertyListFormatter");
        }

        /// <summary>
        /// Defines the test method CoreObjectBase_Equal_Operators.
        /// </summary>
        [Fact]
        public void CoreObjectBase_Equal_Operators()
        {
            var objectBase = new CoreObjectTest(this.TestCaseLogger);
            var objectBaseClone = new CoreObjectTest(this.TestCaseLogger);

            (objectBase == objectBaseClone).Should().BeFalse();
            (objectBase != objectBaseClone).Should().BeTrue();

#pragma warning disable CA1508 // Avoid dead conditional code
            (objectBase is null).Should().BeFalse();
#pragma warning disable SA1131 // Use readable conditions
            (objectBase is null).Should().BeFalse();
#pragma warning restore SA1131 // Use readable conditions
#pragma warning restore CA1508 // Avoid dead conditional code
        }

        /// <summary>
        /// Defines the test method CoreObjectBase_Comparison_Operators.
        /// </summary>
        [Fact]
        public void CoreObjectBase_Comparison_Operators()
        {
            var objectBase = new CoreObjectTest(this.TestCaseLogger);
            var objectBaseClone = new CoreObjectTest(this.TestCaseLogger);

            objectBase.IncreaseObjectVersion();

            (objectBase == objectBaseClone).Should().BeFalse();
            (objectBase >= objectBaseClone).Should().BeTrue();
            (objectBase <= objectBaseClone).Should().BeFalse();

            (objectBase > objectBaseClone).Should().BeTrue();
            (objectBase < objectBaseClone).Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method CoreObjectBase_ObjectBag_Count_Initial.
        /// </summary>
        [Fact]
        public void CoreObjectBase_ObjectBag_Count_Initial()
        {
            var objectTest = new CoreObjectTest(this.TestCaseLogger);
            objectTest.Count.Should().Be(0);
        }

        /// <summary>
        /// Defines the test method CoreObjectBase_ObjectBag_TryAdd_Empty.
        /// </summary>
        [Fact]
        public void CoreObjectBase_ObjectBag_TryAdd_Empty()
        {
            var objectTest = new CoreObjectTest(this.TestCaseLogger);
            objectTest.TryAddItem(new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, 0, CoreLogPropertyType.StringValue)).Should().BeTrue();
            objectTest.Count.Should().Be(1);
        }

        /// <summary>
        /// Class CoreObjectTest.
        /// Implements the <see cref="CoreObjectBase" />.
        /// </summary>
        /// <seealso cref="CoreObjectBase" />
        private class CoreObjectTest : CoreObjectBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CoreObjectTest"/> class.
            /// </summary>
            /// <param name="logger">The logger.</param>
            public CoreObjectTest(ICoreLogger? logger)
            : base(logger)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="CoreObjectTest"/> class.
            /// </summary>
            public CoreObjectTest()
                : this(null)
            {
            }

            /// <summary>
            /// Increases the object version.
            /// </summary>
            /// <returns>System.UInt64.</returns>
            public ulong IncreaseObjectVersion()
            {
                return this.IncrementObjectVersion();
            }

            /// <summary>
            /// Increments the object version.
            /// </summary>
            /// <param name="incAmt">The inc amt.</param>
            /// <returns>New object version.</returns>
            public ulong TestIncrementObjectVersion(ulong incAmt = ObjectVersionDefaultIncrement)
            {
                return this.IncrementObjectVersion(incAmt);
            }

            /// <summary>
            /// Updates timestamp after waiting 1 millisecond.
            /// </summary>
            public void TestUpdateTimestamp()
            {
                this.UpdateTimestamp();
            }

            /// <summary>
            /// Sets a new value on an object.
            /// </summary>
            /// <typeparam name="TItem">Type of item.</typeparam>
            /// <param name="destination">Reference to the item.</param>
            /// <param name="value">New value of the item.</param>
            /// <param name="onChanged">Action to perform on change.</param>
            /// <param name="propertyName">Name of the property.</param>
            /// <returns>True if property changed value.</returns>
            public bool TestSetWithAction<TItem>(ref TItem destination, TItem value, Action? onChanged, [CallerMemberName] string? propertyName = null)
            {
                return this.SetWithAction(ref destination, value, onChanged, propertyName);
            }

            /// <summary>
            /// Sets a new value on an object.
            /// </summary>
            /// <typeparam name="TItem">Type of item.</typeparam>
            /// <param name="propID"></param>
            /// <param name="objectItemProvider"></param>
            /// <param name="fxOnNotFound">Function to call when propID is not found.</param>
            /// <returns>True if property changed value.</returns>
            public TItem? TestGetBagWithAction<TItem>(CorePropID propID, CoreObjectItemProvider objectItemProvider, Func<TItem?>? fxOnNotFound = null)
            {
                return this.GetBagWithAction(propID, objectItemProvider, fxOnNotFound);
            }

            /// <summary>
            /// Sets a new value on an object.
            /// </summary>
            /// <typeparam name="TItem">Type of item.</typeparam>
            /// <param name="propID"></param>
            /// <param name="fxOnNotFound">Function to call when propID is not found.</param>
            /// <returns>True if property changed value.</returns>
            public TItem? TestGetBagWithAction<TItem>(CorePropID propID, Func<TItem?>? fxOnNotFound = null)
            {
                return this.GetBagWithAction(propID, fxOnNotFound);
            }

            /// <summary>
            /// Sets a new value on an object.
            /// </summary>
            /// <typeparam name="TItem">Type of item.</typeparam>
            /// <param name="propID"></param>
            /// <param name="value">New value of the item.</param>
            /// <param name="objectItemProvider"></param>
            /// <param name="propConfidenceScore"></param>
            /// <param name="logPropertyType"></param>
            /// <param name="initialUpdate"></param>
            /// <param name="onChanged">Action to perform on change.</param>
            /// <returns>True if property changed value.</returns>
            public bool TestSetBagWithAction<TItem>(CorePropID propID, TItem value, CoreObjectItemProvider objectItemProvider, CorePropConfidenceScore propConfidenceScore, CoreLogPropertyType logPropertyType, bool initialUpdate = false, Action? onChanged = null)
            {
                return this.SetBagWithAction(propID, value, objectItemProvider, propConfidenceScore, logPropertyType, initialUpdate, onChanged);
            }

            /// <summary>
            /// Sets a new value on an object.
            /// </summary>
            /// <typeparam name="TItem">Type of item.</typeparam>
            /// <param name="propID"></param>
            /// <param name="value">New value of the item.</param>
            /// <param name="objectItemProvider"></param>
            /// <param name="logPropertyType"></param>
            /// <param name="initialUpdate"></param>
            /// <param name="onChanged">Action to perform on change.</param>
            /// <returns>True if property changed value.</returns>
            public bool TestSetBagWithAction<TItem>(CorePropID propID, TItem value, CoreObjectItemProvider objectItemProvider, CoreLogPropertyType logPropertyType, bool initialUpdate = false, Action? onChanged = null)
            {
                return this.SetBagWithAction(propID, value, objectItemProvider, logPropertyType, initialUpdate, onChanged);
            }

            /// <summary>
            /// Calculate the confidence score based on inputs.
            /// </summary>
            /// <typeparam name="TItem">Type of item.</typeparam>
            /// <param name="propID"></param>
            /// <param name="value">New value of the item.</param>
            /// <param name="objectItemProvider"></param>
            /// <param name="initialUpdate"></param>
            /// <returns>Property Confidence Score.</returns>
            public CorePropConfidenceScore TestCalculateConfidenceScore<TItem>(CorePropID propID, TItem value, CoreObjectItemProvider objectItemProvider, bool initialUpdate = false)
            {
                return this.CalculateConfidenceScore(propID, value, objectItemProvider, initialUpdate);
            }

            /// <summary>
            /// Sets a new value on an object.
            /// </summary>
            /// <typeparam name="TItem">Type of item.</typeparam>
            /// <param name="destination">Reference to the item.</param>
            /// <param name="value">New value of the item.</param>
            /// <param name="propertyName">Name of the property.</param>
            /// <returns>True if property changed value.</returns>
            public bool TestSet<TItem>(ref TItem destination, TItem value, [CallerMemberName] string? propertyName = null) =>
                this.Set(ref destination, value, propertyName);
        }
    }
}

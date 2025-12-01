// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="CoreObjectBagUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Collections.Immutable;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using FluentAssertions;
using NetworkVisor.Core.CoreObject;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Logging.LogProperty;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.CoreObject
{
    /// <summary>
    /// Class CoreObjectBagUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreObjectBagUnitTests))]

    public class CoreObjectBagUnitTests : CoreTestCaseBase
    {
        private const string TestProp = "Test";
        private const string TestStringValue1 = "Value1";
        private const string TestStringValue2 = "Value2";
        private const string IPAddressProp = "IPAddress";
        private const string PhysicalAddressProp = "PhysicalAddress";

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreObjectBagUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreObjectBagUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreObjectBag_Ctor()
        {
            this.TestOutputHelper.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestOutputHelper>();
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreObjectBag_Initial()
        {
            CoreObjectBag objectBag = this.CreateTestObjectBag();

            objectBag.GetFirstItemAsIPAddress(IPAddressProp).Should().Be(CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1);
            objectBag.GetFirstItemAsPhysicalAddress(PhysicalAddressProp).Should().Be(PhysicalAddressExtensions.RestrictedPhysicalAddress);

            this.TestOutputHelper.WriteLine(objectBag.ToStringWithParentsPropNameMultiLine());
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreObjectBag_Criteria_ObjectItemProvider()
        {
            CoreObjectBag objectBag = this.CreateTestObjectBag();

            objectBag.GetFirstItemAsString(TestProp, CoreObjectItemProvider.Test).Should().Be(TestStringValue1);
            objectBag.GetFirstItemAsIPAddress(IPAddressProp, CoreObjectItemProvider.Test).Should().Be(CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1);
            objectBag.GetFirstItemAsPhysicalAddress(PhysicalAddressProp, CoreObjectItemProvider.Test).Should().Be(PhysicalAddressExtensions.RestrictedPhysicalAddress);

            objectBag.GetFirstItemAsString(TestProp, CoreObjectItemProvider.Unknown).Should().BeNull();
            objectBag.GetFirstItemAsIPAddress(IPAddressProp, CoreObjectItemProvider.Unknown).Should().BeNull();
            objectBag.GetFirstItemAsPhysicalAddress(PhysicalAddressProp, CoreObjectItemProvider.Unknown).Should().BeNull();
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreObjectBag_String()
        {
            CoreObjectBag objectBag = this.CreateTestObjectBag();

            objectBag.GetFirstItemAsString(TestProp).Should().Be(TestStringValue1);
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreObjectBag_String_Criteria()
        {
            CoreObjectBag objectBag = this.CreateTestObjectBag();

            objectBag.GetFirstItemAsString(TestProp, (item) => item.PropConfidenceScore < CorePropConfidenceScore.LowMed).Should().Be(TestStringValue2);
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreObjectBag_String_ObjectItemProvider()
        {
            CoreObjectBag objectBag = this.CreateTestObjectBag();

            objectBag.GetFirstItemAsString(TestProp, CoreObjectItemProvider.Test).Should().Be(TestStringValue1);
            objectBag.GetFirstItemAsString(TestProp, CoreObjectItemProvider.Unknown).Should().BeNull();
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreObjectBag_IPAddress_Criteria()
        {
            CoreObjectBag objectBag = this.CreateTestObjectBag();

            objectBag.GetFirstItemAsIPAddress(IPAddressProp, (item) => item.PropValue?.Equals(CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1) ?? false).Should().Be(CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1);
            objectBag.GetFirstItemAsIPAddress(IPAddressProp, (item) => item.PropValue?.Equals(IPAddress.Any) ?? false).Should().BeNull();
            objectBag.GetFirstItemAsIPAddress(IPAddressProp, (item) => item.PropValue?.Equals(PhysicalAddress.None) ?? false).Should().BeNull();
            objectBag.GetFirstItemAsIPAddress(IPAddressProp, (item) => ((IPAddress?)item.PropValue).IsNullOrNone()).Should().BeNull();
            objectBag.GetFirstItemAsIPAddress(IPAddressProp, (item) => !((IPAddress?)item.PropValue).IsNullOrNone()).Should().Be(CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1);
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreObjectBag_PhysicalAddress_Criteria()
        {
            CoreObjectBag objectBag = this.CreateTestObjectBag();

            objectBag.GetFirstItemAsPhysicalAddress(PhysicalAddressProp, (item) => item.PropValue?.Equals(PhysicalAddressExtensions.RestrictedPhysicalAddress) ?? false).Should().Be(PhysicalAddressExtensions.RestrictedPhysicalAddress);
            objectBag.GetFirstItemAsPhysicalAddress(PhysicalAddressProp, (item) => item.PropValue?.Equals(PhysicalAddress.None) ?? false).Should().BeNull();
            objectBag.GetFirstItemAsPhysicalAddress(PhysicalAddressProp, (item) => item.PropValue?.Equals(IPAddress.Any) ?? false).Should().BeNull();
            objectBag.GetFirstItemAsPhysicalAddress(PhysicalAddressProp, (item) => ((PhysicalAddress?)item.PropValue).IsNullOrNone()).Should().BeNull();
            objectBag.GetFirstItemAsPhysicalAddress(PhysicalAddressProp, (item) => !((PhysicalAddress?)item.PropValue).IsNullOrNone()).Should().Be(PhysicalAddressExtensions.RestrictedPhysicalAddress);
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreObjectBag_AddItems()
        {
            CoreObjectBag objectBag = new CoreObjectBag(this.TestCaseLogger);

            var items = new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Test, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(IPAddressProp, CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.IPAddress),
                    new CoreObjectItem(PhysicalAddressProp, PhysicalAddressExtensions.RestrictedPhysicalAddress, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.PhysicalAddress),
                };

            ISet<ICoreObjectItem> itemsAdded = objectBag.AddItems(items);

            itemsAdded.Should().BeEquivalentTo(
                new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Test, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(IPAddressProp, CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.IPAddress),
                    new CoreObjectItem(PhysicalAddressProp, PhysicalAddressExtensions.RestrictedPhysicalAddress, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.PhysicalAddress),
                }).And.Subject.Should().BeInDescendingOrder();

            this.OutputItems("Add all items", itemsAdded);

            objectBag.FindAllItems().Should().BeEquivalentTo(
                new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Test, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(IPAddressProp, CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.IPAddress),
                    new CoreObjectItem(PhysicalAddressProp, PhysicalAddressExtensions.RestrictedPhysicalAddress, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.PhysicalAddress),
                }).And.Subject.Should().BeInDescendingOrder();
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreObjectBag_AddItems_Criteria()
        {
            CoreObjectBag objectBag = new CoreObjectBag(this.TestCaseLogger);

            var items = new List<ICoreObjectItem>()
            {
                new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Test, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue),
                new CoreObjectItem(IPAddressProp, CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.IPAddress),
                new CoreObjectItem(PhysicalAddressProp, PhysicalAddressExtensions.RestrictedPhysicalAddress, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.PhysicalAddress),
            };

            ISet<ICoreObjectItem> itemsAdded = objectBag.AddItems(items, (objectItem) => objectItem.PropConfidenceScore > CorePropConfidenceScore.Low);

            this.OutputItems("Add all items with PropConfidenceScore > CorePropConfidenceScore.Low", itemsAdded);

            itemsAdded.Should().BeEquivalentTo(
                [
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(IPAddressProp, CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.IPAddress),
                    new CoreObjectItem(PhysicalAddressProp, PhysicalAddressExtensions.RestrictedPhysicalAddress, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.PhysicalAddress),
                ]).And.Subject.Should().BeInDescendingOrder();

            objectBag.FindAllItems().Should().BeEquivalentTo(
                new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(IPAddressProp, CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.IPAddress),
                    new CoreObjectItem(PhysicalAddressProp, PhysicalAddressExtensions.RestrictedPhysicalAddress, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.PhysicalAddress),
                }).And.Subject.Should().BeInDescendingOrder();
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreObjectBag_FindAllItems_PropName()
        {
            CoreObjectBag objectBag = this.CreateTestObjectBag();

            ISet<ICoreObjectItem> items = objectBag.FindAllItems(TestProp);

            this.OutputItems("Find all with TestProp", items);

            items.Should().BeEquivalentTo(
                new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Test, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue),
                }).And.Subject.Should().BeInDescendingOrder();
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreObjectBag_FindAllItems_PropName_Criteria()
        {
            CoreObjectBag objectBag = this.CreateTestObjectBag();

            ISet<ICoreObjectItem> items = objectBag.FindAllItems(TestProp, (objectItem) => objectItem.PropConfidenceScore > CorePropConfidenceScore.Low);

            this.OutputItems("Find all with TestProp and and PropConfidenceScore > CorePropConfidenceScore.Low", items);

            items.Should().BeEquivalentTo(
                new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                }).And.Subject.Should().BeInDescendingOrder();
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreObjectBag_FindAllItems_PropName_Criteria_Criteria()
        {
            CoreObjectBag objectBag = this.CreateTestObjectBag();

            ISet<ICoreObjectItem> items = objectBag.FindAllItems((propName) => propName.StartsWith("Test"), (objectItem) => objectItem.PropConfidenceScore > CorePropConfidenceScore.Low);

            this.OutputItems("Find all with propName starting with Test and PropConfidenceScore > CorePropConfidenceScore.Low", items);

            items.Should().BeEquivalentTo(
                new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                }).And.Subject.Should().BeInDescendingOrder();
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreObjectBag_FindAllItems_Criteria_PropName()
        {
            CoreObjectBag objectBag = this.CreateTestObjectBag();

            ISet<ICoreObjectItem> items = objectBag.FindAllItems((propName) => propName.StartsWith("Test"));

            this.OutputItems("Find all with propName starting with Test", items);

            items.Should().BeEquivalentTo(
                new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Test, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue),
                }).And.Subject.Should().BeInDescendingOrder();
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreObjectBag_FindAllItems()
        {
            CoreObjectBag objectBag = this.CreateTestObjectBag();

            ISet<ICoreObjectItem> items = objectBag.FindAllItems();

            this.OutputItems("Find all", items);

            items.Should().BeEquivalentTo(
                new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Test, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(IPAddressProp, CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.IPAddress),
                    new CoreObjectItem(PhysicalAddressProp, PhysicalAddressExtensions.RestrictedPhysicalAddress, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.PhysicalAddress),
                }).And.Subject.Should().BeInDescendingOrder();
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreObjectBag_FindAllItems_Criteria()
        {
            CoreObjectBag objectBag = this.CreateTestObjectBag();

            ISet<ICoreObjectItem> items = objectBag.FindAllItems((objectItem) => objectItem.PropConfidenceScore > CorePropConfidenceScore.Low);

            this.OutputItems("Find all with PropConfidenceScore > CorePropConfidenceScore.Low", items);

            items.Should().BeEquivalentTo(
                new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(IPAddressProp, CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.IPAddress),
                    new CoreObjectItem(PhysicalAddressProp, PhysicalAddressExtensions.RestrictedPhysicalAddress, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.PhysicalAddress),
                }).And.Subject.Should().BeInDescendingOrder();
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreObjectBag_FindAllItems_ObjectItemProvider()
        {
            CoreObjectBag objectBag = this.CreateTestObjectBag();

            ISet<ICoreObjectItem> items = objectBag.FindAllItems(CoreObjectItemProvider.Test);

            this.OutputItems("Find all with Test Provider", items);

            items.Should().BeEquivalentTo(
                new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Test, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(IPAddressProp, CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.IPAddress),
                    new CoreObjectItem(PhysicalAddressProp, PhysicalAddressExtensions.RestrictedPhysicalAddress, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.PhysicalAddress),
                }).And.Subject.Should().BeInDescendingOrder();

            objectBag.FindAllItems(CoreObjectItemProvider.Unknown).Should().BeEmpty();
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreObjectBag_FindAllItems_ObjectItemProvider_Criteria()
        {
            CoreObjectBag objectBag = this.CreateTestObjectBag();

            ISet<ICoreObjectItem> items = objectBag.FindAllItems(CoreObjectItemProvider.Test, (objectItem) => objectItem.PropConfidenceScore > CorePropConfidenceScore.Low);

            this.OutputItems("Find all with Test Provider and PropConfidenceScore > CorePropConfidenceScore.Low", items);

            items.Should().BeEquivalentTo(
                new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(IPAddressProp, CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.IPAddress),
                    new CoreObjectItem(PhysicalAddressProp, PhysicalAddressExtensions.RestrictedPhysicalAddress, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.PhysicalAddress),
                }).And.Subject.Should().BeInDescendingOrder();

            objectBag.FindAllItems(CoreObjectItemProvider.Test, (objectItem) => objectItem.PropConfidenceScore > CorePropConfidenceScore.MedHigh).Should().BeEmpty();
            objectBag.FindAllItems(CoreObjectItemProvider.Unknown, (objectItem) => objectItem.PropConfidenceScore > CorePropConfidenceScore.Low).Should().BeEmpty();
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreObjectBag_RemoveAllItems()
        {
            CoreObjectBag objectBag = this.CreateTestObjectBag();

            ISet<ICoreObjectItem> items = objectBag.RemoveAllItems();

            this.OutputItems("Removed all", items);

            items.Should().BeEquivalentTo(
                new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Test, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(IPAddressProp, CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.IPAddress),
                    new CoreObjectItem(PhysicalAddressProp, PhysicalAddressExtensions.RestrictedPhysicalAddress, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.PhysicalAddress),
                }).And.Subject.Should().BeInDescendingOrder();

            objectBag.FindAllItems().Should().BeEmpty();
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreObjectBag_RemoveAllItems_PropName()
        {
            CoreObjectBag objectBag = this.CreateTestObjectBag();

            ISet<ICoreObjectItem> items = objectBag.RemoveAllItems(TestProp);

            this.OutputItems("Removed all with TestProp", items);

            items.Should().BeEquivalentTo(
                new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Test, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                }).And.Subject.Should().BeInDescendingOrder();

            objectBag.FindAllItems().Should().BeEquivalentTo(
                new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(IPAddressProp, CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.IPAddress),
                    new CoreObjectItem(PhysicalAddressProp, PhysicalAddressExtensions.RestrictedPhysicalAddress, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.PhysicalAddress),
                }).And.Subject.Should().BeInDescendingOrder();
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreObjectBag_RemoveAllItems_PropName_StartsWith()
        {
            CoreObjectBag objectBag = this.CreateTestObjectBag();

            ISet<ICoreObjectItem> items = objectBag.RemoveAllItems((propName) => propName.StartsWith("Te"));

            this.OutputItems("Removed all with TestProp", items);

            items.Should().BeEquivalentTo(
                new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Test, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                }).And.Subject.Should().BeInDescendingOrder();

            objectBag.FindAllItems().Should().BeEquivalentTo(
                new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(IPAddressProp, CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.IPAddress),
                    new CoreObjectItem(PhysicalAddressProp, PhysicalAddressExtensions.RestrictedPhysicalAddress, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.PhysicalAddress),
                }).And.Subject.Should().BeInDescendingOrder();
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreObjectBag_RemoveAllItems_ObjectItemProvider_Test()
        {
            CoreObjectBag objectBag = this.CreateTestObjectBag();

            ISet<ICoreObjectItem> items = objectBag.RemoveAllItems(CoreObjectItemProvider.Test);

            this.OutputItems("Removed all with Test Provider", items);

            items.Should().BeEquivalentTo(
                new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Test, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(IPAddressProp, CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.IPAddress),
                    new CoreObjectItem(PhysicalAddressProp, PhysicalAddressExtensions.RestrictedPhysicalAddress, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.PhysicalAddress),
                }).And.Subject.Should().BeInDescendingOrder();

            objectBag.FindAllItems().Should().BeEmpty();
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreObjectBag_RemoveAllItems_ObjectItemProvider_Test_Criteria()
        {
            CoreObjectBag objectBag = this.CreateTestObjectBag();

            ISet<ICoreObjectItem> items = objectBag.RemoveAllItems(CoreObjectItemProvider.Test, (objectItem) => objectItem.PropConfidenceScore > CorePropConfidenceScore.Low);

            this.OutputItems("Removed all with Test Provider and PropConfidenceScore > CorePropConfidenceScore.Low", items);

            items.Should().BeEquivalentTo(
                new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(IPAddressProp, CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.IPAddress),
                    new CoreObjectItem(PhysicalAddressProp, PhysicalAddressExtensions.RestrictedPhysicalAddress, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.PhysicalAddress),
                }).And.Subject.Should().BeInDescendingOrder();

            objectBag.FindAllItems().Should().BeEquivalentTo(
                new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Test, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue),
                }).And.Subject.Should().BeInDescendingOrder();
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreObjectBag_RemoveAllItems_ObjectItemProvider_Unknown()
        {
            CoreObjectBag objectBag = this.CreateTestObjectBag();

            ISet<ICoreObjectItem> items = objectBag.RemoveAllItems(CoreObjectItemProvider.Unknown);

            this.OutputItems("Removed all with Unknown Provider", items);

            items.Should().BeEmpty();

            objectBag.FindAllItems().Should().BeEquivalentTo(
                new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Test, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(IPAddressProp, CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.IPAddress),
                    new CoreObjectItem(PhysicalAddressProp, PhysicalAddressExtensions.RestrictedPhysicalAddress, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.PhysicalAddress),
                }).And.Subject.Should().BeInDescendingOrder();
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreObjectBag_RemoveAllItems_Criteria()
        {
            CoreObjectBag objectBag = this.CreateTestObjectBag();

            ISet<ICoreObjectItem> items = objectBag.RemoveAllItems((objectItem) => objectItem.PropConfidenceScore > CorePropConfidenceScore.Low);

            this.OutputItems("Removed all with PropConfidenceScore > CorePropConfidenceScore.Low", items);

            items.Should().BeEquivalentTo(
                new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(IPAddressProp, CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.IPAddress),
                    new CoreObjectItem(PhysicalAddressProp, PhysicalAddressExtensions.RestrictedPhysicalAddress, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.PhysicalAddress),
                }).And.Subject.Should().BeInDescendingOrder();

            objectBag.FindAllItems().Should().BeEquivalentTo(
                new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Test, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue),
                }).And.Subject.Should().BeInDescendingOrder();
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreObjectBag_UpdateFromBag_Merge_KeepExisting()
        {
            CoreObjectBag objectBagExisting = new CoreObjectBag(this.TestCaseLogger);
            CoreObjectBag objectBagNew = new CoreObjectBag(this.TestCaseLogger);

            var itemsExisting = new List<ICoreObjectItem>()
            {
                new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Test, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue),
            };

            var itemsNew = new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(IPAddressProp, CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.IPAddress),
                    new CoreObjectItem(PhysicalAddressProp, PhysicalAddressExtensions.RestrictedPhysicalAddress, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.PhysicalAddress),
                };

            ISet<ICoreObjectItem> itemsAddedExisting = objectBagExisting.AddItems(itemsExisting);
            itemsAddedExisting.Should().BeEquivalentTo(
                new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Test, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue),
                }).And.Subject.Should().BeInDescendingOrder();

            ISet<ICoreObjectItem> itemsAddedNew = objectBagNew.AddItems(itemsNew);
            itemsAddedNew.Should().BeEquivalentTo(
                new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(IPAddressProp, CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.IPAddress),
                    new CoreObjectItem(PhysicalAddressProp, PhysicalAddressExtensions.RestrictedPhysicalAddress, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.PhysicalAddress),
                }).And.Subject.Should().BeInDescendingOrder();

            ISet<ICoreObjectItem> itemsAdded = objectBagExisting.UpdateFromBag(objectBagNew);

            this.OutputItems("Items added", itemsAdded);

            itemsAdded.Should().BeEquivalentTo(
    new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(IPAddressProp, CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.IPAddress),
                    new CoreObjectItem(PhysicalAddressProp, PhysicalAddressExtensions.RestrictedPhysicalAddress, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.PhysicalAddress),
                }).And.Subject.Should().BeInDescendingOrder();

            objectBagExisting.FindAllItems().Should().BeEquivalentTo(
                new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Test, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(IPAddressProp, CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.IPAddress),
                    new CoreObjectItem(PhysicalAddressProp, PhysicalAddressExtensions.RestrictedPhysicalAddress, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.PhysicalAddress),
                }).And.Subject.Should().BeInDescendingOrder();

            this.OutputItems("Find All Items", objectBagExisting.FindAllItems());
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreObjectBag_UpdateFromBag_Merge_Replace()
        {
            CoreObjectBag objectBagExisting = new CoreObjectBag(this.TestCaseLogger);
            CoreObjectBag objectBagNew = new CoreObjectBag(this.TestCaseLogger);

            var itemsExisting = new List<ICoreObjectItem>()
            {
                new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Test, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue),
            };

            var itemsNew = new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(IPAddressProp, CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.IPAddress),
                    new CoreObjectItem(PhysicalAddressProp, PhysicalAddressExtensions.RestrictedPhysicalAddress, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.PhysicalAddress),
                };

            ISet<ICoreObjectItem> itemsAddedExisting = objectBagExisting.AddItems(itemsExisting);
            itemsAddedExisting.Should().BeEquivalentTo(
                new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Test, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue),
                }).And.Subject.Should().BeInDescendingOrder();

            ISet<ICoreObjectItem> itemsAddedNew = objectBagNew.AddItems(itemsNew);
            itemsAddedNew.Should().BeEquivalentTo(
                new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(IPAddressProp, CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.IPAddress),
                    new CoreObjectItem(PhysicalAddressProp, PhysicalAddressExtensions.RestrictedPhysicalAddress, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.PhysicalAddress),
                }).And.Subject.Should().BeInDescendingOrder();

            ISet<ICoreObjectItem> itemsAdded = objectBagExisting.UpdateFromBag(objectBagNew);

            itemsAdded.Should().BeEquivalentTo(
                new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(IPAddressProp, CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.IPAddress),
                    new CoreObjectItem(PhysicalAddressProp, PhysicalAddressExtensions.RestrictedPhysicalAddress, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.PhysicalAddress),
                }).And.Subject.Should().BeInDescendingOrder();

            ISet<ICoreObjectItem> itemsExistingNotAdded = objectBagExisting.UpdateFromBag(objectBagNew, (item1, item2) => item1.PropConfidenceScore >= item2.PropConfidenceScore);

            itemsExistingNotAdded.Should().BeEmpty();

            itemsExistingNotAdded = objectBagExisting.UpdateFromBag(objectBagNew, (item1, item2) => false);

            itemsExistingNotAdded.Should().BeEquivalentTo(
                new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(IPAddressProp, CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.IPAddress),
                    new CoreObjectItem(PhysicalAddressProp, PhysicalAddressExtensions.RestrictedPhysicalAddress, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.PhysicalAddress),
                }).And.Subject.Should().BeInDescendingOrder();

            this.OutputItems("Union of items", objectBagExisting.FindAllItems());

            objectBagExisting.FindAllItems().Should().BeEquivalentTo(
                new List<ICoreObjectItem>()
                {
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Test, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(IPAddressProp, CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.IPAddress),
                    new CoreObjectItem(PhysicalAddressProp, PhysicalAddressExtensions.RestrictedPhysicalAddress, CoreObjectItemProvider.Unknown, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.PhysicalAddress),
                }).And.Subject.Should().BeInDescendingOrder();
        }

        [Fact]
        public async Task CoreObjectBag_SaveToStream_Empty()
        {
            CoreObjectBag objectBagEmpty = new CoreObjectBag(this.TestCaseLogger);

            using Stream streamLoaded = new MemoryStream();
            Exception? result = await objectBagEmpty.SaveToStream(streamLoaded);
            result.Should().BeNull();
            streamLoaded.Length.Should().BeGreaterThan(0);

            byte[] buffer = new byte[streamLoaded.Length];
            streamLoaded.Seek(0, SeekOrigin.Begin);
            long readBytes = await streamLoaded.ReadAsync(buffer, 0, buffer.Length);
            readBytes.Should().Be(streamLoaded.Length);

            this.TestOutputHelper.WriteLine(Encoding.UTF8.GetString(buffer));
        }

        [Fact]
        public async Task CoreObjectBag_SaveToStream_Properties()
        {
            CoreObjectBag testObjectBag = this.CreateTestObjectBag();

            using Stream streamLoaded = new MemoryStream();
            Exception? result = await testObjectBag.SaveToStream(streamLoaded);
            result.Should().BeNull();
            streamLoaded.Length.Should().BeGreaterThan(0);

            byte[] buffer = new byte[streamLoaded.Length];
            streamLoaded.Seek(0, SeekOrigin.Begin);
            long readBytes = await streamLoaded.ReadAsync(buffer, 0, buffer.Length);
            readBytes.Should().Be(streamLoaded.Length);

            this.TestOutputHelper.WriteLine(Encoding.UTF8.GetString(buffer));
        }

        private void OutputItems(string title, IEnumerable<ICoreObjectItem> items)
        {
            this.TestOutputHelper.WriteLine(new string('*', title.Length));
            this.TestOutputHelper.WriteLine(title);
            this.TestOutputHelper.WriteLine(new string('*', title.Length));
            this.TestOutputHelper.WriteLine();
            foreach (ICoreObjectItem item in items)
            {
                this.TestOutputHelper.WriteLine(item.ToStringWithParentsPropNameMultiLine());
                this.TestOutputHelper.WriteLine();
            }
        }

        private CoreObjectBag CreateTestObjectBag()
        {
            var objectBag = new CoreObjectBag(this.TestCaseLogger);

            objectBag.TryAddItem(new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Test, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue));
            objectBag.TryAddItem(new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue));
            objectBag.TryAddItem(new CoreObjectItem(IPAddressProp, CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.IPAddress));
            objectBag.TryAddItem(new CoreObjectItem(PhysicalAddressProp, PhysicalAddressExtensions.RestrictedPhysicalAddress, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.PhysicalAddress));

            return objectBag;
        }
    }
}

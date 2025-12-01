// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-27-2020
// ***********************************************************************
// <copyright file="CoreDequeUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//      Forked from https://github.com/StephenCleary/AsyncEx.
//      Original idea by Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266930.aspx.
// </summary>
// ***********************************************************************

using System.Collections;
using NetworkVisor.Core.Collections;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Async.Collections
{
    /// <summary>
    /// Class CoreDequeUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreDequeUnitTests))]

    public class CoreDequeUnitTests : CoreTestCaseBase
    {
        // Implementation detail: the default capacity.
        private const int DefaultCapacity = 8;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreDequeUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreDequeUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void Capacity_SetTo0_ActsLikeList()
        {
            var list = new List<int>
            {
                Capacity = 0,
            };
            Assert.Equal(0, list.Capacity);

            var deque = new CoreDeque<int>
            {
                Capacity = 0,
            };
            Assert.Equal(0, deque.Capacity);
        }

        [Fact]
        public void Capacity_SetNegative_ActsLikeList()
        {
            var list = new List<int>();
            Assert.Throws<ArgumentOutOfRangeException>("value", () => { list.Capacity = -1; });

            var deque = new CoreDeque<int>();
            Assert.Throws<ArgumentOutOfRangeException>("value", () => { deque.Capacity = -1; });
        }

        [Fact]
        public void Capacity_SetLarger_UsesSpecifiedCapacity()
        {
            var deque = new CoreDeque<int>(1);
            Assert.Equal(1, deque.Capacity);
            deque.Capacity = 17;
            Assert.Equal(17, deque.Capacity);
        }

        [Fact]
        public void Capacity_SetSmaller_UsesSpecifiedCapacity()
        {
            var deque = new CoreDeque<int>(13);
            Assert.Equal(13, deque.Capacity);
            deque.Capacity = 7;
            Assert.Equal(7, deque.Capacity);
        }

        [Fact]
        public void Capacity_Set_PreservesData()
        {
            var deque = new CoreDeque<int>(new int[] { 1, 2, 3 });
            Assert.Equal(3, deque.Capacity);
            deque.Capacity = 7;
            Assert.Equal(7, deque.Capacity);
            Assert.Equal(new[] { 1, 2, 3 }, deque);
        }

        [Fact]
        public void Capacity_Set_WhenSplit_PreservesData()
        {
            var deque = new CoreDeque<int>(new int[] { 1, 2, 3 });
            deque.RemoveFromFront();
            deque.AddToBack(4);
            Assert.Equal(3, deque.Capacity);
            deque.Capacity = 7;
            Assert.Equal(7, deque.Capacity);
            Assert.Equal(new[] { 2, 3, 4 }, deque);
        }

        [Fact]
        public void Capacity_Set_SmallerThanCount_ActsLikeList()
        {
            var list = new List<int>(new int[] { 1, 2, 3 });
            Assert.Equal(3, list.Capacity);
            Assert.Throws<ArgumentOutOfRangeException>("value", () => { list.Capacity = 2; });

            var deque = new CoreDeque<int>(new int[] { 1, 2, 3 });
            Assert.Equal(3, deque.Capacity);
            Assert.Throws<ArgumentOutOfRangeException>("value", () => { deque.Capacity = 2; });
        }

        [Fact]
        public void Capacity_Set_ToItself_DoesNothing()
        {
            var deque = new CoreDeque<int>(13);
            Assert.Equal(13, deque.Capacity);
            deque.Capacity = 13;
            Assert.Equal(13, deque.Capacity);
        }

        [Fact]
        public void Constructor_WithoutExplicitCapacity_UsesDefaultCapacity()
        {
            var deque = new CoreDeque<int>();
            Assert.Equal(DefaultCapacity, deque.Capacity);
        }

        [Fact]
        public void Constructor_WithoutExplicitCapacity_Null()
        {
            Assert.Throws<ArgumentNullException>(() => new CoreDeque<int>(null!));
        }

        [Fact]
        public void Constructor_CapacityOf0_ActsLikeList()
        {
            var list = new List<int>(0);
            Assert.Equal(0, list.Capacity);

            var deque = new CoreDeque<int>(0);
            Assert.Equal(0, deque.Capacity);
        }

        [Fact]
        public void Constructor_CapacityOf0_PermitsAdd()
        {
            var deque = new CoreDeque<int>(0);
            deque.AddToBack(13);
            Assert.Equal(new[] { 13 }, deque);
        }

        [Fact]
        public void Constructor_NegativeCapacity_ActsLikeList()
        {
            Assert.Throws<ArgumentOutOfRangeException>("capacity", () => new List<int>(-1));

            Assert.Throws<ArgumentOutOfRangeException>("capacity", () => new CoreDeque<int>(-1));
        }

        [Fact]
        public void Constructor_CapacityOf1_UsesSpecifiedCapacity()
        {
            var deque = new CoreDeque<int>(1);
            Assert.Equal(1, deque.Capacity);
        }

        [Fact]
        public void Constructor_FromEmptySequence_UsesDefaultCapacity()
        {
            var deque = new CoreDeque<int>([]);
            Assert.Equal(DefaultCapacity, deque.Capacity);
        }

        [Fact]
        public void Constructor_FromSequence_InitializesFromSequence()
        {
            var deque = new CoreDeque<int>(new int[] { 1, 2, 3 });
            Assert.Equal(3, deque.Capacity);
            Assert.Equal(3, deque.Count);
            Assert.Equal(new int[] { 1, 2, 3 }, deque);
        }

        [Fact]
        public void IndexOf_ItemPresent_ReturnsItemIndex()
        {
            var deque = new CoreDeque<int>(new[] { 1, 2 });
            int result = deque.IndexOf(2);
            Assert.Equal(1, result);
        }

        [Fact]
        public void IndexOf_ItemNotPresent_ReturnsNegativeOne()
        {
            var deque = new CoreDeque<int>(new[] { 1, 2 });
            int result = deque.IndexOf(3);
            Assert.Equal(-1, result);
        }

        [Fact]
        public void IndexOf_ItemPresentAndSplit_ReturnsItemIndex()
        {
            var deque = new CoreDeque<int>(new[] { 1, 2, 3 });
            deque.RemoveFromBack();
            deque.AddToFront(0);
            Assert.Equal(0, deque.IndexOf(0));
            Assert.Equal(1, deque.IndexOf(1));
            Assert.Equal(2, deque.IndexOf(2));
        }

        [Fact]
        public void Contains_ItemPresent_ReturnsTrue()
        {
            var deque = new CoreDeque<int>(new[] { 1, 2 }) as ICollection<int>;
            Assert.True(deque.Contains(2));
        }

        [Fact]
        public void Contains_ItemNotPresent_ReturnsFalse()
        {
            var deque = new CoreDeque<int>(new[] { 1, 2 }) as ICollection<int>;
            Assert.False(deque.Contains(3));
        }

        [Fact]
        public void Contains_ItemPresentAndSplit_ReturnsTrue()
        {
            var deque = new CoreDeque<int>(new[] { 1, 2, 3 });
            deque.RemoveFromBack();
            deque.AddToFront(0);
            var deq = deque as ICollection<int>;
            Assert.True(deq.Contains(0));
            Assert.True(deq.Contains(1));
            Assert.True(deq.Contains(2));
            Assert.False(deq.Contains(3));
        }

        [Fact]
        public void Add_IsAddToBack()
        {
            var deque1 = new CoreDeque<int>(new[] { 1, 2 });
            var deque2 = new CoreDeque<int>(new[] { 1, 2 });
            ((ICollection<int>)deque1).Add(3);
            deque2.AddToBack(3);
            Assert.Equal(deque1, deque2);
        }

        [Fact]
        public void NonGenericEnumerator_EnumeratesItems()
        {
            var deque = new CoreDeque<int>(new[] { 1, 2 });
            var results = new List<int>();
            IEnumerator objEnum = ((IEnumerable)deque).GetEnumerator();

            while (objEnum.MoveNext())
            {
                results.Add((int)objEnum.Current!);
            }

            Assert.Equal(results, deque);
        }

        [Fact]
        public void IsReadOnly_ReturnsFalse()
        {
            var deque = new CoreDeque<int>();
            Assert.False(((ICollection<int>)deque).IsReadOnly);
        }

        [Fact]
        public void CopyTo_CopiesItems()
        {
            var deque = new CoreDeque<int>(new[] { 1, 2, 3 });
            int[] results = new int[3];
            ((ICollection<int>)deque).CopyTo(results, 0);
        }

        [Fact]
        public void CopyTo_NullArray_ActsLikeList()
        {
            var list = new List<int>(new[] { 1, 2, 3 });
            Assert.Throws<ArgumentNullException>(() => ((ICollection<int>)list).CopyTo(null!, 0));

            var deque = new CoreDeque<int>(new[] { 1, 2, 3 });
            Assert.Throws<ArgumentNullException>(() => ((ICollection<int>)deque).CopyTo(null!, 0));
        }

        [Fact]
        public void CopyTo_NegativeOffset_ActsLikeList()
        {
            int[] destination = new int[3];
            var list = new List<int>(new[] { 1, 2, 3 });
            Assert.Throws<ArgumentOutOfRangeException>(() => ((ICollection<int>)list).CopyTo(destination, -1));

            var deque = new CoreDeque<int>(new[] { 1, 2, 3 });
            Assert.Throws<ArgumentOutOfRangeException>(() => ((ICollection<int>)deque).CopyTo(destination, -1));
        }

        [Fact]
        public void CopyTo_InsufficientSpace_ActsLikeList()
        {
            int[] destination = new int[3];
            var list = new List<int>(new[] { 1, 2, 3 });
            Assert.Throws<ArgumentException>(() => ((ICollection<int>)list).CopyTo(destination, 1));

            var deque = new CoreDeque<int>(new[] { 1, 2, 3 });
            Assert.Throws<ArgumentException>(() => ((ICollection<int>)deque).CopyTo(destination, 1));
        }

        [Fact]
        public void Clear_EmptiesAllItems()
        {
            var deque = new CoreDeque<int>(new[] { 1, 2, 3 });
            deque.Clear();
            Assert.Empty(deque);
            Assert.Equal([], deque);
        }

        [Fact]
        public void Clear_DoesNotChangeCapacity()
        {
            var deque = new CoreDeque<int>(new[] { 1, 2, 3 });
            Assert.Equal(3, deque.Capacity);
            deque.Clear();
            Assert.Equal(3, deque.Capacity);
        }

        [Fact]
        public void RemoveFromFront_Empty_ActsLikeStack()
        {
            var stack = new Stack<int>();
            Assert.Throws<InvalidOperationException>(() => stack.Pop());

            var deque = new CoreDeque<int>();
            Assert.Throws<InvalidOperationException>(() => deque.RemoveFromFront());
        }

        [Fact]
        public void RemoveFromBack_Empty_ActsLikeQueue()
        {
            var queue = new Queue<int>();
            Assert.Throws<InvalidOperationException>(() => queue.Dequeue());

            var deque = new CoreDeque<int>();
            Assert.Throws<InvalidOperationException>(() => deque.RemoveFromBack());
        }

        [Fact]
        public void Remove_ItemPresent_RemovesItemAndReturnsTrue()
        {
            var deque = new CoreDeque<int>(new[] { 1, 2, 3, 4 });
            bool result = deque.Remove(3);
            Assert.True(result);
            Assert.Equal(new[] { 1, 2, 4 }, deque);
        }

        [Fact]
        public void Remove_ItemNotPresent_KeepsItemsReturnsFalse()
        {
            var deque = new CoreDeque<int>(new[] { 1, 2, 3, 4 });
            bool result = deque.Remove(5);
            Assert.False(result);
            Assert.Equal(new[] { 1, 2, 3, 4 }, deque);
        }

        [Fact]
        public void Insert_InsertsElementAtIndex()
        {
            var deque = new CoreDeque<int>(new[] { 1, 2 });
            deque.Insert(1, 13);
            Assert.Equal(new[] { 1, 13, 2 }, deque);
        }

        [Fact]
        public void Insert_AtIndex0_IsSameAsAddToFront()
        {
            var deque1 = new CoreDeque<int>(new[] { 1, 2 });
            var deque2 = new CoreDeque<int>(new[] { 1, 2 });
            deque1.Insert(0, 0);
            deque2.AddToFront(0);
            Assert.Equal(deque1, deque2);
        }

        [Fact]
        public void Insert_AtCount_IsSameAsAddToBack()
        {
            var deque1 = new CoreDeque<int>(new[] { 1, 2 });
            var deque2 = new CoreDeque<int>(new[] { 1, 2 });
            deque1.Insert(deque1.Count, 0);
            deque2.AddToBack(0);
            Assert.Equal(deque1, deque2);
        }

        [Fact]
        public void Insert_NegativeIndex_ActsLikeList()
        {
            var list = new List<int>(new[] { 1, 2, 3 });
            Assert.Throws<ArgumentOutOfRangeException>("index", () => list.Insert(-1, 0));

            var deque = new CoreDeque<int>(new[] { 1, 2, 3 });
            Assert.Throws<ArgumentOutOfRangeException>("index", () => deque.Insert(-1, 0));
        }

        [Fact]
        public void Insert_IndexTooLarge_ActsLikeList()
        {
            var list = new List<int>(new[] { 1, 2, 3 });
            Assert.Throws<ArgumentOutOfRangeException>("index", () => list.Insert(list.Count + 1, 0));

            var deque = new CoreDeque<int>(new[] { 1, 2, 3 });
            Assert.Throws<ArgumentOutOfRangeException>("index", () => deque.Insert(deque.Count + 1, 0));
        }

        [Fact]
        public void RemoveAt_RemovesElementAtIndex()
        {
            var deque = new CoreDeque<int>(new[] { 1, 2, 3 });
            deque.RemoveFromBack();
            deque.AddToFront(0);
            deque.RemoveAt(1);
            Assert.Equal(new[] { 0, 2 }, deque);
        }

        [Fact]
        public void RemoveAt_Index0_IsSameAsRemoveFromFront()
        {
            var deque1 = new CoreDeque<int>(new[] { 1, 2 });
            var deque2 = new CoreDeque<int>(new[] { 1, 2 });
            deque1.RemoveAt(0);
            deque2.RemoveFromFront();
            Assert.Equal(deque1, deque2);
        }

        [Fact]
        public void RemoveAt_LastIndex_IsSameAsRemoveFromBack()
        {
            var deque1 = new CoreDeque<int>(new[] { 1, 2 });
            var deque2 = new CoreDeque<int>(new[] { 1, 2 });
            deque1.RemoveAt(1);
            deque2.RemoveFromBack();
            Assert.Equal(deque1, deque2);
        }

        [Fact]
        public void RemoveAt_NegativeIndex_ActsLikeList()
        {
            var list = new List<int>(new[] { 1, 2, 3 });
            Assert.Throws<ArgumentOutOfRangeException>("index", () => list.RemoveAt(-1));

            var deque = new CoreDeque<int>(new[] { 1, 2, 3 });
            Assert.Throws<ArgumentOutOfRangeException>("index", () => deque.RemoveAt(-1));
        }

        [Fact]
        public void RemoveAt_IndexTooLarge_ActsLikeList()
        {
            var list = new List<int>(new[] { 1, 2, 3 });
            Assert.Throws<ArgumentOutOfRangeException>("index", () => list.RemoveAt(list.Count));

            var deque = new CoreDeque<int>(new[] { 1, 2, 3 });
            Assert.Throws<ArgumentOutOfRangeException>("index", () => deque.RemoveAt(deque.Count));
        }

        [Fact]
        public void InsertMultiple()
        {
            InsertTest(new[] { 1, 2, 3 }, new[] { 7, 13 });
            InsertTest(new[] { 1, 2, 3, 4 }, new[] { 7, 13 });
        }

        [Fact]
        public void Insert_RangeOfZeroElements_HasNoEffect()
        {
            var deque = new CoreDeque<int>(new[] { 1, 2, 3 });
            deque.InsertRange(1, []);
            Assert.Equal(new[] { 1, 2, 3 }, deque);
        }

        [Fact]
        public void InsertMultiple_MakesRoomForNewElements()
        {
            var deque = new CoreDeque<int>(new[] { 1, 2, 3 });
            deque.InsertRange(1, new[] { 7, 13 });
            Assert.Equal(new[] { 1, 7, 13, 2, 3 }, deque);
            Assert.Equal(5, deque.Capacity);
        }

        [Fact]
        public void RemoveMultiple()
        {
            RemoveTest(new[] { 1, 2, 3 });
            RemoveTest(new[] { 1, 2, 3, 4 });
        }

        [Fact]
        public void Remove_RangeOfZeroElements_HasNoEffect()
        {
            var deque = new CoreDeque<int>(new[] { 1, 2, 3 });
            deque.RemoveRange(1, 0);
            Assert.Equal(new[] { 1, 2, 3 }, deque);
        }

        [Fact]
        public void Remove_NegativeCount_ActsLikeList()
        {
            var list = new List<int>(new[] { 1, 2, 3 });
            Assert.Throws<ArgumentOutOfRangeException>("count", () => list.RemoveRange(1, -1));

            var deque = new CoreDeque<int>(new[] { 1, 2, 3 });
            Assert.Throws<ArgumentOutOfRangeException>("count", () => deque.RemoveRange(1, -1));
        }

        [Fact]
        public void GetItem_ReadsElements()
        {
            var deque = new CoreDeque<int>(new[] { 1, 2, 3 });
            Assert.Equal(1, deque[0]);
            Assert.Equal(2, deque[1]);
            Assert.Equal(3, deque[2]);
        }

        [Fact]
        public void GetItem_Split_ReadsElements()
        {
            var deque = new CoreDeque<int>(new[] { 1, 2, 3 });
            deque.RemoveFromBack();
            deque.AddToFront(0);
            Assert.Equal(0, deque[0]);
            Assert.Equal(1, deque[1]);
            Assert.Equal(2, deque[2]);
        }

        [Fact]
        public void GetItem_IndexTooLarge_ActsLikeList()
        {
            var list = new List<int>(new[] { 1, 2, 3 });
            Assert.Throws<ArgumentOutOfRangeException>("index", () => list[3]);

            var deque = new CoreDeque<int>(new[] { 1, 2, 3 });
            Assert.Throws<ArgumentOutOfRangeException>("index", () => deque[3]);
        }

        [Fact]
        public void GetItem_NegativeIndex_ActsLikeList()
        {
            var list = new List<int>(new[] { 1, 2, 3 });
            Assert.Throws<ArgumentOutOfRangeException>("index", () => list[-1]);

            var deque = new CoreDeque<int>(new[] { 1, 2, 3 });
            Assert.Throws<ArgumentOutOfRangeException>("index", () => deque[-1]);
        }

        [Fact]
        public void SetItem_WritesElements()
        {
            var deque = new CoreDeque<int>(new[] { 1, 2, 3 });
            deque[0] = 7;
            deque[1] = 11;
            deque[2] = 13;
            Assert.Equal(new[] { 7, 11, 13 }, deque);
        }

        [Fact]
        public void SetItem_Split_WritesElements()
        {
            var deque = new CoreDeque<int>(new[] { 1, 2, 3 });
            deque.RemoveFromBack();
            deque.AddToFront(0);
            deque[0] = 7;
            deque[1] = 11;
            deque[2] = 13;
            Assert.Equal(new[] { 7, 11, 13 }, deque);
        }

        [Fact]
        public void SetItem_IndexTooLarge_ActsLikeList()
        {
            var list = new List<int>(new[] { 1, 2, 3 });
            Assert.Throws<ArgumentOutOfRangeException>("index", () => { list[3] = 13; });

            var deque = new CoreDeque<int>(new[] { 1, 2, 3 });
            Assert.Throws<ArgumentOutOfRangeException>("index", () => { deque[3] = 13; });
        }

        [Fact]
        public void SetItem_NegativeIndex_ActsLikeList()
        {
            var list = new List<int>(new[] { 1, 2, 3 });
            Assert.Throws<ArgumentOutOfRangeException>("index", () => { list[-1] = 13; });

            var deque = new CoreDeque<int>(new[] { 1, 2, 3 });
            Assert.Throws<ArgumentOutOfRangeException>("index", () => { deque[-1] = 13; });
        }

        [Fact]
        public void NongenericIndexOf_ItemPresent_ReturnsItemIndex()
        {
            var deque = new CoreDeque<int>(new[] { 1, 2 }) as IList;
            int result = deque.IndexOf(2);
            Assert.Equal(1, result);
        }

        [Fact]
        public void NongenericIndexOf_ItemNotPresent_ReturnsNegativeOne()
        {
            var deque = new CoreDeque<int>(new[] { 1, 2 }) as IList;
            int result = deque.IndexOf(3);
            Assert.Equal(-1, result);
        }

        [Fact]
        public void NongenericIndexOf_ItemPresentAndSplit_ReturnsItemIndex()
        {
            var deque_ = new CoreDeque<int>(new[] { 1, 2, 3 });
            deque_.RemoveFromBack();
            deque_.AddToFront(0);
            var deque = deque_ as IList;
            Assert.Equal(0, deque.IndexOf(0));
            Assert.Equal(1, deque.IndexOf(1));
            Assert.Equal(2, deque.IndexOf(2));
        }

        [Fact]
        public void NongenericIndexOf_WrongItemType_ReturnsNegativeOne()
        {
            var list = new List<int>(new[] { 1, 2 }) as IList;
            Assert.Equal(-1, list.IndexOf(this));

            var deque = new CoreDeque<int>(new[] { 1, 2 }) as IList;
            Assert.Equal(-1, deque.IndexOf(this));
        }

        [Fact]
        public void NongenericContains_WrongItemType_ReturnsFalse()
        {
            var list = new List<int>(new[] { 1, 2 }) as IList;
            Assert.False(list.Contains(this));

            var deque = new CoreDeque<int>(new[] { 1, 2 }) as IList;
            Assert.False(deque.Contains(this));
        }

        [Fact]
        public void NongenericContains_ItemPresent_ReturnsTrue()
        {
            var deque = new CoreDeque<int>(new[] { 1, 2 }) as IList;
            Assert.True(deque.Contains(2));
        }

        [Fact]
        public void NongenericContains_ItemNotPresent_ReturnsFalse()
        {
            var deque = new CoreDeque<int>(new[] { 1, 2 }) as IList;
            Assert.False(deque.Contains(3));
        }

        [Fact]
        public void NongenericContains_ItemPresentAndSplit_ReturnsTrue()
        {
            var deque_ = new CoreDeque<int>(new[] { 1, 2, 3 });
            deque_.RemoveFromBack();
            deque_.AddToFront(0);
            var deque = deque_ as IList;
            Assert.True(deque.Contains(0));
            Assert.True(deque.Contains(1));
            Assert.True(deque.Contains(2));
            Assert.False(deque.Contains(3));
        }

        [Fact]
        public void NongenericIsReadOnly_ReturnsFalse()
        {
            var deque = new CoreDeque<int>() as IList;
            Assert.False(deque.IsReadOnly);
        }

        [Fact]
        public void NongenericCopyTo_CopiesItems()
        {
            var deque = new CoreDeque<int>(new[] { 1, 2, 3 }) as IList;
            int[] results = new int[3];
            deque.CopyTo(results, 0);
        }

        [Fact]
        public void NongenericCopyTo_NullArray_ActsLikeList()
        {
            var list = new List<int>(new[] { 1, 2, 3 }) as IList;
            Assert.Throws<ArgumentNullException>(() => list.CopyTo(null!, 0));

            var deque = new CoreDeque<int>(new[] { 1, 2, 3 }) as IList;
            Assert.Throws<ArgumentNullException>(() => deque.CopyTo(null!, 0));
        }

        [Fact]
        public void NongenericCopyTo_NegativeOffset_ActsLikeList()
        {
            int[] destination = new int[3];
            var list = new List<int>(new[] { 1, 2, 3 }) as IList;
            Assert.Throws<ArgumentOutOfRangeException>(() => list.CopyTo(destination, -1));

            var deque = new CoreDeque<int>(new[] { 1, 2, 3 }) as IList;
            Assert.Throws<ArgumentOutOfRangeException>(() => deque.CopyTo(destination, -1));
        }

        [Fact]
        public void NongenericCopyTo_InsufficientSpace_ActsLikeList()
        {
            int[] destination = new int[3];
            var list = new List<int>(new[] { 1, 2, 3 }) as IList;
            Assert.Throws<ArgumentException>(() => list.CopyTo(destination, 1));

            var deque = new CoreDeque<int>(new[] { 1, 2, 3 }) as IList;
            Assert.Throws<ArgumentException>(() => deque.CopyTo(destination, 1));
        }

        [Fact]
        public void NongenericCopyTo_WrongType_ActsLikeList()
        {
            var destination = new IList[3];
            var list = new List<int>(new[] { 1, 2, 3 }) as IList;
            Assert.Throws<ArgumentException>(() => list.CopyTo(destination, 0));

            var deque = new CoreDeque<int>(new[] { 1, 2, 3 }) as IList;
            Assert.Throws<ArgumentException>(() => deque.CopyTo(destination, 0));
        }

        [Fact]
        public void NongenericCopyTo_MultidimensionalArray_ActsLikeList()
        {
            int[,] destination = new int[3, 3];
            var list = new List<int>(new[] { 1, 2, 3 }) as IList;
            Assert.Throws<ArgumentException>(() => list.CopyTo(destination, 0));

            var deque = new CoreDeque<int>(new[] { 1, 2, 3 }) as IList;
            Assert.Throws<ArgumentException>(() => deque.CopyTo(destination, 0));
        }

        [Fact]
        public void NongenericAdd_WrongType_ActsLikeList()
        {
            var list = new List<int>() as IList;
            Assert.Throws<ArgumentException>("value", () => list.Add(this));

            var deque = new CoreDeque<int>() as IList;
            Assert.Throws<ArgumentException>("value", () => deque.Add(this));
        }

        [Fact]
        public void NongenericNullableType_AllowsInsertingNull()
        {
            var deque = new CoreDeque<int?>();
            var list = deque as IList;
            int result = list.Add(null);
            Assert.Equal(0, result);
            Assert.Equal(new int?[] { null }, deque);
        }

        [Fact]
        public void NongenericClassType_AllowsInsertingNull()
        {
            var deque = new CoreDeque<object>();
            var list = deque as IList;
            int result = list.Add(null);
            Assert.Equal(0, result);
            Assert.Equal(new object[] { null! }, deque);
        }

        [Fact]
        public void NongenericInterfaceType_AllowsInsertingNull()
        {
            var deque = new CoreDeque<IList>();
            var list = deque as IList;
            int result = list.Add(null);
            Assert.Equal(0, result);
            Assert.Equal(new IList[] { null! }, deque);
        }

        [Fact]
        public void NongenericStruct_AddNull_ActsLikeList()
        {
            var list = new List<int>() as IList;
            Assert.Throws<ArgumentNullException>(() => list.Add(null));

            var deque = new CoreDeque<int>() as IList;
            Assert.Throws<ArgumentNullException>(() => deque.Add(null));
        }

        [Fact]
        public void NongenericGenericStruct_AddNull_ActsLikeList()
        {
            var list = new List<KeyValuePair<int, int>>() as IList;
            Assert.Throws<ArgumentNullException>(() => list.Add(null));

            var deque = new CoreDeque<KeyValuePair<int, int>>() as IList;
            Assert.Throws<ArgumentNullException>(() => deque.Add(null));
        }

        [Fact]
        public void NongenericIsFixedSize_IsFalse()
        {
            var deque = new CoreDeque<int>() as IList;
            Assert.False(deque.IsFixedSize);
        }

        [Fact]
        public void NongenericIsSynchronized_IsFalse()
        {
            var deque = new CoreDeque<int>() as IList;
            Assert.False(deque.IsSynchronized);
        }

        [Fact]
        public void NongenericSyncRoot_IsSelf()
        {
            var deque = new CoreDeque<int>() as IList;
            Assert.Same(deque, deque.SyncRoot);
        }

        [Fact]
        public void NongenericInsert_InsertsItem()
        {
            var deque = new CoreDeque<int>();
            var list = deque as IList;
            list.Insert(0, 7);
            Assert.Equal(new[] { 7 }, deque);
        }

        [Fact]
        public void NongenericInsert_WrongType_ActsLikeList()
        {
            var list = new List<int>() as IList;
            Assert.Throws<ArgumentException>("value", () => list.Insert(0, this));

            var deque = new CoreDeque<int>() as IList;
            Assert.Throws<ArgumentException>("value", () => deque.Insert(0, this));
        }

        [Fact]
        public void NongenericStruct_InsertNull_ActsMostlyLikeList()
        {
            var list = new List<int>() as IList;
            Assert.Throws<ArgumentNullException>("item", () => list.Insert(0, null)); // Should probably be "value".

            var deque = new CoreDeque<int>() as IList;
            Assert.Throws<ArgumentNullException>("value", () => deque.Insert(0, null));
        }

        [Fact]
        public void NongenericRemove_RemovesItem()
        {
            var deque = new CoreDeque<int>(new[] { 13 });
            var list = deque as IList;
            list.Remove(13);
            Assert.Equal([], deque);
        }

        [Fact]
        public void NongenericRemove_WrongType_DoesNothing()
        {
            var list = new List<int>(new[] { 13 }) as IList;
            list.Remove(this);
            list.Remove(null);
            Assert.Single(list);

            var deque = new CoreDeque<int>(new[] { 13 }) as IList;
            deque.Remove(this);
            deque.Remove(null);
            Assert.Single(deque);
        }

        [Fact]
        public void NongenericGet_GetsItem()
        {
            var deque = new CoreDeque<int>(new[] { 13 }) as IList;
            int? value = (int?)deque[0];
            Assert.Equal(13, value);
        }

        [Fact]
        public void NongenericSet_SetsItem()
        {
            var deque = new CoreDeque<int>(new[] { 13 });
            var list = deque as IList;
            list[0] = 7;
            Assert.Equal(new[] { 7 }, deque);
        }

        [Fact]
        public void NongenericSet_WrongType_ActsLikeList()
        {
            var list = new List<int>(new[] { 13 }) as IList;
            Assert.Throws<ArgumentException>("value", () => { list[0] = this; });

            var deque = new CoreDeque<int>(new[] { 13 }) as IList;
            Assert.Throws<ArgumentException>("value", () => { deque[0] = this; });
        }

        [Fact]
        public void NongenericStruct_SetNull_ActsLikeList()
        {
            var list = new List<int>(new[] { 13 }) as IList;
            Assert.Throws<ArgumentNullException>("value", () => { list[0] = null; });

            var deque = new CoreDeque<int>(new[] { 13 }) as IList;
            Assert.Throws<ArgumentNullException>("value", () => { deque[0] = null; });
        }

        [Fact]
        public void ToArray_CopiesToNewArray()
        {
            var deque = new CoreDeque<int>(new[] { 0, 1 });
            deque.AddToBack(13);
            int[] result = deque.ToArray();
            Assert.Equal(new[] { 0, 1, 13 }, result);
        }

        private static void InsertTest(IReadOnlyCollection<int> initial, IReadOnlyCollection<int> items)
        {
            int totalCapacity = initial.Count + items.Count;
            for (int rotated = 0; rotated <= totalCapacity; ++rotated)
            {
                for (int index = 0; index <= initial.Count; ++index)
                {
                    // Calculate the expected result using the slower List<int>.
                    var result = new List<int>(initial);
                    for (int i = 0; i != rotated; ++i)
                    {
                        int item = result[0];
                        result.RemoveAt(0);
                        result.Add(item);
                    }

                    result.InsertRange(index, items);

                    // First, start off the deque with the initial items.
                    var deque = new CoreDeque<int>(initial);

                    // Ensure there's enough room for the inserted items.
                    deque.Capacity += items.Count;

                    // Rotate the existing items.
                    for (int i = 0; i != rotated; ++i)
                    {
                        int item = deque[0];
                        deque.RemoveFromFront();
                        deque.AddToBack(item);
                    }

                    // Do the insert.
                    deque.InsertRange(index, items);

                    // Ensure the results are as expected.
                    Assert.Equal(result, deque);
                }
            }
        }

        private static void RemoveTest(IReadOnlyCollection<int> initial)
        {
            for (int count = 0; count <= initial.Count; ++count)
            {
                for (int rotated = 0; rotated <= initial.Count; ++rotated)
                {
                    for (int index = 0; index <= initial.Count - count; ++index)
                    {
                        // Calculated the expected result using the slower List<int>.
                        var result = new List<int>(initial);
                        for (int i = 0; i != rotated; ++i)
                        {
                            int item = result[0];
                            result.RemoveAt(0);
                            result.Add(item);
                        }

                        result.RemoveRange(index, count);

                        // First, start off the deque with the initial items.
                        var deque = new CoreDeque<int>(initial);

                        // Rotate the existing items.
                        for (int i = 0; i != rotated; ++i)
                        {
                            int item = deque[0];
                            deque.RemoveFromFront();
                            deque.AddToBack(item);
                        }

                        // Do the remove.
                        deque.RemoveRange(index, count);

                        // Ensure the results are as expected.
                        Assert.Equal(result, deque);
                    }
                }
            }
        }
    }
}

using Enjune.Misc;

namespace Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

/// <summary>
/// DeepSeek slop
/// </summary>
public class NotifyChangeListTests
{
    #region Add

    [Fact]
    public void Add_ShouldIncreaseCountAndFireOnElementAdded()
    {
        // Arrange
        var list = new NotifyChangeList<string>();
        var eventArgs = new List<string>();
        list.OnElementAdded += (item) => eventArgs.Add(item);

        // Act
        list.Add("First");

        // Assert State
        Assert.Single(list);
        Assert.Equal("First", list[0]);

        // Assert Event
        Assert.Single(eventArgs);
        Assert.Equal("First", eventArgs[0]);
    }

    [Fact]
    public void Add_ShouldNotThrowIfNoSubscribers()
    {
        // Arrange
        var list = new NotifyChangeList<int>();

        // Act & Assert (No exception should be thrown)
        list.Add(10);
        Assert.Single(list);
    }

    #endregion

    #region Remove

    [Fact]
    public void Remove_ExistingItem_ShouldReturnTrueAndFireOnElementRemoved()
    {
        // Arrange
        var list = new NotifyChangeList<string> { "A", "B" };
        var removedItem = string.Empty;
        list.OnElementRemoved += (item) => removedItem = item;

        // Act
        bool result = list.Remove("A");

        // Assert State
        Assert.True(result);
        Assert.Single(list);
        Assert.Equal("B", list[0]);

        // Assert Event
        Assert.Equal("A", removedItem);
    }

    [Fact]
    public void Remove_NonExistingItem_ShouldReturnFalseAndNotFireEvent()
    {
        // Arrange
        var list = new NotifyChangeList<string> { "A" };
        bool eventFired = false;
        list.OnElementRemoved += (_) => eventFired = true;

        // Act
        bool result = list.Remove("Z");

        // Assert
        Assert.False(result);
        Assert.Single(list);
        Assert.False(eventFired, "OnElementRemoved should not fire when item is not found.");
    }

    #endregion

    #region Insert

    [Fact]
    public void Insert_ShouldAddAtSpecifiedIndexAndFireOnElementAdded()
    {
        // Arrange
        var list = new NotifyChangeList<string> { "A", "C" };
        var eventArgs = new List<string>();
        list.OnElementAdded += (item) => eventArgs.Add(item);

        // Act
        list.Insert(1, "B");

        // Assert State
        Assert.Equal(3, list.Count);
        Assert.Equal("A", list[0]);
        Assert.Equal("B", list[1]);
        Assert.Equal("C", list[2]);

        // Assert Event
        Assert.Single(eventArgs);
        Assert.Equal("B", eventArgs[0]);
    }

    #endregion

    #region RemoveAt (THE BUG CATCHER!)

    [Fact]
    public void RemoveAt_ShouldRemoveItemAndFireOnElementRemoved_NOT_OnElementAdded()
    {
        // Arrange
        var list = new NotifyChangeList<string> { "X", "Y", "Z" };
        object? removedEventArg = null;
        bool addedEventFired = false;

        list.OnElementRemoved += (item) => removedEventArg = item;
        list.OnElementAdded += (_) => addedEventFired = true;

        // Act
        list.RemoveAt(1); // Remove "Y"

        // Assert State
        Assert.Equal(2, list.Count);
        Assert.Equal("X", list[0]);
        Assert.Equal("Z", list[1]);

        // Assert Events
        Assert.Equal("Y", removedEventArg);
        Assert.False(addedEventFired, "Bug found! RemoveAt is firing OnElementAdded, but should fire OnElementRemoved.");
    }

    [Fact]
    public void RemoveAt_InvalidIndex_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var list = new NotifyChangeList<int> { 1, 2 };

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(99));
    }

    #endregion

    #region Clear

    [Fact]
    public void Clear_WithItems_ShouldRemoveAllAndFireOnElementRemovedForEachItem()
    {
        // Arrange
        var list = new NotifyChangeList<int> { 1, 2, 3 };
        var firedItems = new List<int>();
        list.OnElementRemoved += (item) => firedItems.Add(item);

        // Act
        list.Clear();

        // Assert State
        Assert.Empty(list);

        // Assert Event
        Assert.Equal(3, firedItems.Count);
        Assert.Contains(1, firedItems);
        Assert.Contains(2, firedItems);
        Assert.Contains(3, firedItems);
    }

    [Fact]
    public void Clear_WithEmptyList_ShouldDoNothingAndNotFireEvents()
    {
        // Arrange
        var list = new NotifyChangeList<string>();
        bool eventFired = false;
        list.OnElementRemoved += (_) => eventFired = true;

        // Act
        list.Clear();

        // Assert
        Assert.Empty(list);
        Assert.False(eventFired);
    }

    #endregion

    #region Indexer (Set)

    [Fact]
    public void Indexer_Set_ShouldFireRemovedForOldAndAddedForNew()
    {
        // Arrange
        var list = new NotifyChangeList<string> { "OldValue" };
        object? removedEventArg = null;
        object? addedEventArg = null;

        list.OnElementRemoved += (item) => removedEventArg = item;
        list.OnElementAdded += (item) => addedEventArg = item;

        // Act
        list[0] = "NewValue";

        // Assert State
        Assert.Single(list);
        Assert.Equal("NewValue", list[0]);

        // Assert Events
        Assert.Equal("OldValue", removedEventArg);
        Assert.Equal("NewValue", addedEventArg);
    }

    #endregion

    #region Readonly Operations (Contains, IndexOf, CopyTo, Enumerator)

    [Fact]
    public void Contains_ShouldReturnCorrectResult()
    {
        var list = new NotifyChangeList<string> { "Apple", "Banana" };
        Assert.True(list.Contains("Apple"));
        Assert.False(list.Contains("Cherry"));
    }

    [Fact]
    public void IndexOf_ShouldReturnCorrectIndex()
    {
        var list = new NotifyChangeList<string> { "Apple", "Banana" };
        Assert.Equal(1, list.IndexOf("Banana"));
        Assert.Equal(-1, list.IndexOf("Cherry"));
    }

    [Fact]
    public void CopyTo_ShouldCopyElementsToArray()
    {
        var list = new NotifyChangeList<int> { 1, 2, 3 };
        var array = new int[3];

        list.CopyTo(array, 0);

        Assert.Equal(new[] { 1, 2, 3 }, array);
    }

    [Fact]
    public void GetEnumerator_ShouldIterateOverAllItems()
    {
        var list = new NotifyChangeList<string> { "A", "B" };
        var items = new List<string>();

        foreach (var item in list)
        {
            items.Add(item);
        }

        Assert.Equal(new[] { "A", "B" }, items);
    }

    [Fact]
    public void IsReadOnly_ShouldReturnFalse()
    {
        var list = new NotifyChangeList<int>();
        Assert.False(list.IsReadOnly);
    }

    [Fact]
    public void Count_ShouldReturnCorrectNumberOfItems()
    {
        var list = new NotifyChangeList<int> { 1, 2, 3 };
        Assert.Equal(3, list.Count);

        list.Remove(2);
        Assert.Equal(2, list.Count);
    }

    #endregion

    #region Interface Explicit Implementation (IEnumerator)

    [Fact]
    public void ExplicitNonGenericEnumerator_ShouldWorkCorrectly()
    {
        // Arrange
        var list = new NotifyChangeList<int> { 1, 2 };
        System.Collections.IEnumerable enumerable = list; // Explicit interface

        // Act
        var enumerator = enumerable.GetEnumerator();
        var items = new List<object>();
        while (enumerator.MoveNext())
        {
            items.Add(enumerator.Current);
        }

        // Assert
        Assert.Equal(new object[] { 1, 2 }, items);
    }

    #endregion

    #region Edge Cases: Structs and Null Values

    [Fact]
    public void Add_WithNullReferenceType_ShouldFireEventWithNull()
    {
        var list = new NotifyChangeList<string?>();
        object? eventArg = "Not Null";
        list.OnElementAdded += (item) => eventArg = item;

        list.Add(null);

        Assert.Null(eventArg);
        Assert.Null(list[0]);
    }

    [Fact]
    public void Add_WithValueType_ShouldFireEventWithCorrectValue()
    {
        var list = new NotifyChangeList<int>();
        int eventArg = 0;
        list.OnElementAdded += (item) => eventArg = item;

        list.Add(42);

        Assert.Equal(42, eventArg);
        Assert.Equal(42, list[0]);
    }

    #endregion
}
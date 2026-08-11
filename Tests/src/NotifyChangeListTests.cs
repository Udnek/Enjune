using System.Collections;
using Enjune.Misc;

namespace Tests;

/// <summary>
/// DeepSeek slop
/// </summary>
public class NotifyChangeListTests
{
    #region Constructors

    [Fact]
    public void Constructor_WithCapacity_ShouldCreateEmptyList()
    {
        var list = new NotifyChangeList<string>(10);
        Assert.Empty(list);
        Assert.IsType<NotifyChangeList<string>>(list);
    }

    [Fact]
    public void Constructor_WithCollection_ShouldInitializeWithItems()
    {
        var source = new[] { "Apple", "Banana", "Cherry" };
        var list = new NotifyChangeList<string>(source);

        Assert.Equal(3, list.Count);
        Assert.Equal("Apple", list[0]);
        Assert.Equal("Banana", list[1]);
        Assert.Equal("Cherry", list[2]);
    }

    [Fact]
    public void Constructor_WithEmptyCollection_ShouldCreateEmptyList()
    {
        var source = Enumerable.Empty<int>();
        var list = new NotifyChangeList<int>(source);
        Assert.Empty(list);
    }

    [Fact]
    public void Constructor_WithNullCollection_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new NotifyChangeList<string>(null!));
    }

    #endregion

    #region ForEach (NEW!)

    [Fact]
    public void ForEach_ShouldExecuteActionForEachItem()
    {
        var list = new NotifyChangeList<int> { 1, 2, 3 };
        var results = new List<int>();

        list.ForEach(x => results.Add(x * 2));

        Assert.Equal(new[] { 2, 4, 6 }, results);
    }

    [Fact]
    public void ForEach_WithEmptyList_ShouldDoNothing()
    {
        var list = new NotifyChangeList<string>();
        bool actionCalled = false;

        list.ForEach(_ => actionCalled = true);

        Assert.False(actionCalled);
    }

    [Fact]
    public void ForEach_WithNullAction_ShouldThrowArgumentNullException()
    {
        var list = new NotifyChangeList<int> { 1, 2, 3 };

        // List<T>.ForEach throws ArgumentNullException if the action is null.
        Assert.Throws<ArgumentNullException>(() => list.ForEach(null!));
    }

    [Fact]
    public void ForEach_ShouldNotFireAnyChangeEvents()
    {
        var list = new NotifyChangeList<int> { 1, 2 };
        bool addedFired = false;
        bool removedFired = false;

        list.AfterElementAdded += (_) => addedFired = true;
        list.AfterElementRemoved += (_) => removedFired = true;

        list.ForEach(x => { }); // No-op action

        Assert.False(addedFired);
        Assert.False(removedFired);
    }

    #endregion

    #region Add & AfterElementAdded

    [Fact]
    public void Add_ShouldIncreaseCountAndFireAfterElementAdded()
    {
        var list = new NotifyChangeList<string>();
        var eventArgs = new List<string>();
        list.AfterElementAdded += (item) => eventArgs.Add(item);

        list.Add("First");

        Assert.Single(list);
        Assert.Equal("First", list[0]);
        Assert.Single(eventArgs);
        Assert.Equal("First", eventArgs[0]);
    }

    [Fact]
    public void Add_AfterElementAdded_FiresAfterItemIsAdded()
    {
        // Proves the "After" semantic: the item is already in the list when the event runs.
        var list = new NotifyChangeList<int>();
        bool containsDuringEvent = false;

        list.AfterElementAdded += (item) =>
        {
            containsDuringEvent = list.Contains(item); // Should be true!
        };

        list.Add(42);

        Assert.True(containsDuringEvent, "The event fired AFTER the item was added, so Contains() should return true.");
        Assert.Contains(42, list);
    }

    [Fact]
    public void Add_ShouldNotThrowIfNoSubscribers()
    {
        var list = new NotifyChangeList<int>();
        list.Add(10);
        Assert.Single(list);
    }

    #endregion

    #region Remove & AfterElementRemoved

    [Fact]
    public void Remove_ExistingItem_ShouldReturnTrueAndFireAfterElementRemoved()
    {
        var list = new NotifyChangeList<string> { "A", "B" };
        var removedItem = string.Empty;
        list.AfterElementRemoved += (item) => removedItem = item;

        bool result = list.Remove("A");

        Assert.True(result);
        Assert.Single(list);
        Assert.Equal("B", list[0]);
        Assert.Equal("A", removedItem);
    }

    [Fact]
    public void Remove_AfterElementRemoved_FiresAfterItemIsRemoved()
    {
        var list = new NotifyChangeList<int> { 1, 2, 3 };
        bool containsDuringEvent = true;

        list.AfterElementRemoved += (item) =>
        {
            containsDuringEvent = list.Contains(item); // Should be false!
        };

        list.Remove(2);

        Assert.False(containsDuringEvent, "The event fired AFTER the item was removed, so Contains() should return false.");
        Assert.DoesNotContain(2, list);
    }

    [Fact]
    public void Remove_NonExistingItem_ShouldReturnFalseAndNotFireEvent()
    {
        var list = new NotifyChangeList<string> { "A" };
        bool eventFired = false;
        list.AfterElementRemoved += (_) => eventFired = true;

        bool result = list.Remove("Z");

        Assert.False(result);
        Assert.Single(list);
        Assert.False(eventFired);
    }

    #endregion

    #region Insert & AfterElementAdded

    [Fact]
    public void Insert_ShouldAddAtSpecifiedIndexAndFireAfterElementAdded()
    {
        var list = new NotifyChangeList<string> { "A", "C" };
        var eventArgs = new List<string>();
        list.AfterElementAdded += (item) => eventArgs.Add(item);

        list.Insert(1, "B");

        Assert.Equal(3, list.Count);
        Assert.Equal("A", list[0]);
        Assert.Equal("B", list[1]);
        Assert.Equal("C", list[2]);
        Assert.Single(eventArgs);
        Assert.Equal("B", eventArgs[0]);
    }

    [Fact]
    public void Insert_AfterElementAdded_FiresAfterItemIsInserted()
    {
        var list = new NotifyChangeList<int> { 1, 2 };
        bool containsDuringEvent = false;

        list.AfterElementAdded += (item) =>
        {
            containsDuringEvent = list.Contains(item);
        };

        list.Insert(1, 99);

        Assert.True(containsDuringEvent, "The event fired AFTER the item was inserted, so Contains() should be true.");
        Assert.Contains(99, list);
    }

    #endregion

    #region RemoveAt & AfterElementRemoved

    [Fact]
    public void RemoveAt_ShouldRemoveItemAndFireAfterElementRemoved()
    {
        var list = new NotifyChangeList<string> { "X", "Y", "Z" };
        object? removedEventArg = null;
        list.AfterElementRemoved += (item) => removedEventArg = item;

        list.RemoveAt(1);

        Assert.Equal(2, list.Count);
        Assert.Equal("X", list[0]);
        Assert.Equal("Z", list[1]);
        Assert.Equal("Y", removedEventArg);
    }

    [Fact]
    public void RemoveAt_AfterElementRemoved_FiresAfterItemIsRemoved()
    {
        var list = new NotifyChangeList<int> { 10, 20, 30 };
        bool containsDuringEvent = true;

        list.AfterElementRemoved += (item) =>
        {
            containsDuringEvent = list.Contains(item);
        };

        list.RemoveAt(0);

        Assert.False(containsDuringEvent, "The event fired AFTER the item was removed, so Contains() should be false.");
        Assert.DoesNotContain(10, list);
    }

    [Fact]
    public void RemoveAt_InvalidIndex_ShouldThrowArgumentOutOfRangeException()
    {
        var list = new NotifyChangeList<int> { 1, 2 };
        Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(99));
    }

    #endregion

    #region Clear & AfterElementRemoved

    [Fact]
    public void Clear_WithItems_ShouldRemoveAllAndFireAfterElementRemovedForEachItem()
    {
        var list = new NotifyChangeList<int> { 1, 2, 3 };
        var firedItems = new List<int>();
        list.AfterElementRemoved += (item) => firedItems.Add(item);

        list.Clear();

        Assert.Empty(list);
        Assert.Equal(3, firedItems.Count);
        Assert.Contains(1, firedItems);
        Assert.Contains(2, firedItems);
        Assert.Contains(3, firedItems);
    }

    [Fact]
    public void Clear_AfterElementRemoved_FiresAfterListIsAlreadyCleared()
    {
        var list = new NotifyChangeList<int> { 10, 20 };
        int countDuringEvent = -1;

        list.AfterElementRemoved += (_) =>
        {
            countDuringEvent = list.Count; // Should be 0 because you copied and cleared first!
        };

        list.Clear();

        Assert.Equal(0, countDuringEvent);
        Assert.Empty(list);
    }

    [Fact]
    public void Clear_WithEmptyList_ShouldDoNothingAndNotFireEvents()
    {
        var list = new NotifyChangeList<string>();
        bool eventFired = false;
        list.AfterElementRemoved += (_) => eventFired = true;

        list.Clear();

        Assert.Empty(list);
        Assert.False(eventFired);
    }

    #endregion

    #region Indexer (Set) - Both events are "After"

    [Fact]
    public void Indexer_Set_ShouldFireAfterElementRemovedForOld_ThenAfterElementAddedForNew()
    {
        var list = new NotifyChangeList<string> { "OldValue" };
        var eventLog = new List<string>();

        list.AfterElementRemoved += (item) => eventLog.Add($"Removed: {item}");
        list.AfterElementAdded += (item) => eventLog.Add($"Added: {item}");

        list[0] = "NewValue";

        Assert.Single(list);
        Assert.Equal("NewValue", list[0]);

        Assert.Equal(2, eventLog.Count);
        Assert.Equal("Removed: OldValue", eventLog[0]);
        Assert.Equal("Added: NewValue", eventLog[1]);
    }

    [Fact]
    public void Indexer_Set_AfterElementRemovedShouldSeeTheNewValueInList()
    {
        var list = new NotifyChangeList<int> { 100 };
        int? oldValueDuringEvent = null;
        int? newValueDuringEvent = null;

        list.AfterElementRemoved += (item) =>
        {
            oldValueDuringEvent = item;       // The old value passed as the event arg.
            newValueDuringEvent = list[0];     // The new value already in the list!
        };

        list[0] = 200;

        Assert.Equal(100, oldValueDuringEvent);
        Assert.Equal(200, newValueDuringEvent);
    }

    [Fact]
    public void Indexer_Set_AfterElementAddedShouldSeeTheNewValueInList()
    {
        var list = new NotifyChangeList<int> { 100 };
        int? valueDuringEvent = null;

        list.AfterElementAdded += (item) =>
        {
            valueDuringEvent = list[0]; // Should be the new value.
        };

        list[0] = 200;

        Assert.Equal(200, valueDuringEvent);
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
    public void CopyTo_ThrowsIfArrayTooSmall()
    {
        var list = new NotifyChangeList<int> { 1, 2, 3 };
        var array = new int[2];

        Assert.Throws<ArgumentException>(() => list.CopyTo(array, 0));
    }

    [Fact]
    public void GetEnumerator_ShouldIterateOverAllItems()
    {
        var list = new NotifyChangeList<string> { "A", "B" };
        var items = new List<string>();

        foreach (var item in list) items.Add(item);

        Assert.Equal(new[] { "A", "B" }, items);
    }

    [Fact]
    public void ExplicitNonGenericEnumerator_ShouldWorkCorrectly()
    {
        var list = new NotifyChangeList<int> { 1, 2 };
        IEnumerable enumerable = list;

        var enumerator = enumerable.GetEnumerator();
        var items = new List<object>();
        while (enumerator.MoveNext())
        {
            items.Add(enumerator.Current);
        }

        Assert.Equal(new object[] { 1, 2 }, items);
    }

    #endregion

    #region Interface Compliance

    [Fact]
    public void IsReadOnly_ShouldReturnFalse() => Assert.False(new NotifyChangeList<int>().IsReadOnly);

    [Fact]
    public void Count_ShouldReturnCorrectNumberOfItems()
    {
        var list = new NotifyChangeList<int> { 1, 2, 3 };
        Assert.Equal(3, list.Count);
        list.Remove(2);
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public void Implements_INotifyChangeReadonlyList()
    {
        var list = new NotifyChangeList<int>();
        Assert.IsAssignableFrom<INotifyChangeReadonlyList<int>>(list);
        Assert.IsAssignableFrom<IReadOnlyList<int>>(list);
    }

    #endregion

    #region Edge Cases: Structs and Null Values

    [Fact]
    public void Add_WithNullReferenceType_ShouldFireAfterElementAddedWithNull()
    {
        var list = new NotifyChangeList<string?>();
        object? eventArg = "Not Null";
        list.AfterElementAdded += (item) => eventArg = item;

        list.Add(null);

        Assert.Null(eventArg);
        Assert.Null(list[0]);
    }

    [Fact]
    public void Add_WithValueType_ShouldFireAfterElementAddedWithCorrectValue()
    {
        var list = new NotifyChangeList<int>();
        int eventArg = 0;
        list.AfterElementAdded += (item) => eventArg = item;

        list.Add(42);

        Assert.Equal(42, eventArg);
        Assert.Equal(42, list[0]);
    }

    [Fact]
    public void Remove_WithReferenceType_ShouldFireAfterElementRemovedWithReference()
    {
        var obj = new object();
        var list = new NotifyChangeList<object> { obj };
        object? eventArg = null;
        list.AfterElementRemoved += (item) => eventArg = item;

        list.Remove(obj);

        Assert.Same(obj, eventArg);
    }

    [Fact]
    public void Clear_WithReferenceTypes_ShouldFireEventsWithCorrectReferences()
    {
        var obj1 = new object();
        var obj2 = new object();
        var list = new NotifyChangeList<object> { obj1, obj2 };
        var firedItems = new List<object>();
        list.AfterElementRemoved += (item) => firedItems.Add(item);

        list.Clear();

        Assert.Equal(2, firedItems.Count);
        Assert.Same(obj1, firedItems[0]);
        Assert.Same(obj2, firedItems[1]);
    }

    #endregion
}
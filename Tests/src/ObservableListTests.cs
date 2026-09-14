using System.Collections;
using Enjune.Misc;

namespace Tests;

/// <summary>
/// DeepSeek slop
/// </summary>
public class ObservableListTests
{
    // Тест: добавление в конец вызывает событие AfterItemAdded с правильным индексом и элементом
    [Fact]
    public void Add_RaisesAfterItemAdded_WithCorrectIndexAndItem()
    {
        // Arrange
        var list = new ObservableList<int>();
        int capturedIndex = -1;
        int capturedItem = default;

        // Act
        using (list.AfterItemAdded((index, item) => { capturedIndex = index; capturedItem = item; }))
        {
            list.Add(42);
        }

        // Assert
        Assert.Equal(0, capturedIndex);
        Assert.Equal(42, capturedItem);
    }

    // Тест: вставка по индексу вызывает AfterItemAdded с правильными параметрами
    [Fact]
    public void Insert_RaisesAfterItemAdded_WithCorrectIndexAndItem()
    {
        // Arrange
        var list = new ObservableList<int> { 10, 20, 30 };
        int capturedIndex = -1;
        int capturedItem = default;

        // Act
        using (list.AfterItemAdded((index, item) => { capturedIndex = index; capturedItem = item; }))
        {
            list.Insert(1, 99);
        }

        // Assert
        Assert.Equal(1, capturedIndex);
        Assert.Equal(99, capturedItem);
        Assert.Equal(new[] { 10, 99, 20, 30 }, list);
    }

    // Тест: удаление элемента (Remove) вызывает AfterItemRemoved с правильным индексом и значением
    [Fact]
    public void Remove_RaisesAfterItemRemoved_WithCorrectIndexAndItem()
    {
        // Arrange
        var list = new ObservableList<int> { 5, 10, 15 };
        int capturedIndex = -1;
        int capturedItem = default;

        // Act
        using (list.AfterItemRemoved((index, item) => { capturedIndex = index; capturedItem = item; }))
        {
            bool removed = list.Remove(10);
        }

        // Assert
        Assert.Equal(1, capturedIndex);
        Assert.Equal(10, capturedItem);
        Assert.Equal(new[] { 5, 15 }, list);
    }

    // Тест: удаление по индексу (RemoveAt) вызывает AfterItemRemoved
    [Fact]
    public void RemoveAt_RaisesAfterItemRemoved_WithCorrectIndexAndItem()
    {
        // Arrange
        var list = new ObservableList<string> { "a", "b", "c" };
        int capturedIndex = -1;
        string capturedItem = null;

        // Act
        using (list.AfterItemRemoved((index, item) => { capturedIndex = index; capturedItem = item; }))
        {
            list.RemoveAt(0);
        }

        // Assert
        Assert.Equal(0, capturedIndex);
        Assert.Equal("a", capturedItem);
        Assert.Equal(new[] { "b", "c" }, list);
    }

    // Тест: очистка всего списка вызывает AfterItemRemoved для каждого элемента в обратном порядке
    [Fact]
    public void Clear_RaisesAfterItemRemoved_ForEachItemInReverseOrder()
    {
        // Arrange
        var list = new ObservableList<int> { 1, 2, 3, 4 };
        var removedItems = new List<(int index, int item)>();

        // Act
        using (list.AfterItemRemoved((index, item) => removedItems.Add((index, item))))
        {
            list.Clear();
        }

        // Assert
        Assert.Equal(4, removedItems.Count);
        // Ожидаемый порядок: удаление с последнего индекса к первому
        var expected = new[] { (3, 4), (2, 3), (1, 2), (0, 1) };
        Assert.Equal(expected, removedItems);
        Assert.Empty(list);
    }

    // Тест: замена элемента через индексатор вызывает сначала AfterItemRemoved, затем AfterItemAdded
    [Fact]
    public void IndexerSet_RaisesBothRemovedAndAdded_WithCorrectValues()
    {
        // Arrange
        var list = new ObservableList<int> { 100, 200, 300 };
        var removedCalls = new List<(int index, int item)>();
        var addedCalls = new List<(int index, int item)>();

        // Act
        using (list.AfterItemRemoved((i, v) => removedCalls.Add((i, v))))
        using (list.AfterItemAdded((i, v) => addedCalls.Add((i, v))))
        {
            list[1] = 999;
        }

        // Assert
        Assert.Single(removedCalls);
        Assert.Equal((1, 200), removedCalls[0]);

        Assert.Single(addedCalls);
        Assert.Equal((1, 999), addedCalls[0]);

        Assert.Equal(new[] { 100, 999, 300 }, list);
    }

    // Тест: Unsubscriber для AfterItemAdded действительно отписывает обработчик
    [Fact]
    public void Unsubscriber_AfterItemAdded_UnsubscribesSuccessfully()
    {
        // Arrange
        var list = new ObservableList<int>();
        int callCount = 0;

        // Act
        var unsub = list.AfterItemAdded((index, item) => callCount++);
        list.Add(1); // вызов должен сработать
        unsub.Dispose();
        list.Add(2); // после отписки вызовов быть не должно

        // Assert
        Assert.Equal(1, callCount);
    }

    // Тест: Unsubscriber для AfterItemRemoved действительно отписывает обработчик
    [Fact]
    public void Unsubscriber_AfterItemRemoved_UnsubscribesSuccessfully()
    {
        // Arrange
        var list = new ObservableList<int> { 10, 20 };
        int callCount = 0;

        // Act
        var unsub = list.AfterItemRemoved((index, item) => callCount++);
        list.RemoveAt(0); // должен вызваться
        unsub.Dispose();
        list.RemoveAt(0); // после отписки не вызовется

        // Assert
        Assert.Equal(1, callCount);
    }

    // Тест: подписка через AfterItemAddedAsOwner добавляет обработчик и событие срабатывает
    [Fact]
    public void AfterItemAddedAsOwner_SubscribesAndRaises()
    {
        // Arrange
        var list = new ObservableList<int>();
        bool called = false;

        // Act
        list.AfterItemAddedAsOwner((index, item) => called = true);
        list.Add(5);

        // Assert
        Assert.True(called);
        // Обратите внимание: нет способа отписаться, но в рамках одного теста это приемлемо
    }

    // Тест: подписка через AfterItemRemovedAsOwner добавляет обработчик и событие срабатывает
    [Fact]
    public void AfterItemRemovedAsOwner_SubscribesAndRaises()
    {
        // Arrange
        var list = new ObservableList<int> { 1, 2 };
        bool called = false;

        // Act
        list.AfterItemRemovedAsOwner((index, item) => called = true);
        list.RemoveAt(0);

        // Assert
        Assert.True(called);
    }

    // Дополнительный тест: проверка корректности методов IReadOnlyList / IList (без событий)
    [Fact]
    public void ImplementsIListInterfaceCorrectly()
    {
        var list = new ObservableList<int> { 1, 2, 3 };

        // Contains
        Assert.True(list.Contains(2));
        Assert.False(list.Contains(99));

        // IndexOf
        Assert.Equal(1, list.IndexOf(2));
        Assert.Equal(-1, list.IndexOf(99));

        // CopyTo
        var array = new int[3];
        list.CopyTo(array, 0);
        Assert.Equal(new[] { 1, 2, 3 }, array);

        // Count и IsReadOnly
        Assert.Equal(3, list.Count);
        Assert.False(list.IsReadOnly);

        // GetEnumerator (проверка через foreach)
        var enumerator = list.GetEnumerator();
        var elements = new List<int>();
        while (enumerator.MoveNext())
            elements.Add(enumerator.Current);
        Assert.Equal(new[] { 1, 2, 3 }, elements);
    }
}
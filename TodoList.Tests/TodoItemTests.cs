using System;
using Xunit;

namespace TodoList.Tests;

public class TodoItemTests
{
    [Fact]
    public void Constructor_WithText_SetsNotStartedStatusAndCurrentDate()
    {
        // Arrange
        var text = "Buy milk";
        var before = DateTime.Now;

        // Act
        var item = new TodoItem(text);

        // Assert
        Assert.Equal(text, item.Text);
        Assert.Equal(TodoStatus.NotStarted, item.Status);
        Assert.True(item.LastUpdate >= before && item.LastUpdate <= DateTime.Now);
    }

    [Fact]
    public void Constructor_WithStatusAndDate_SetsProperties()
    {
        // Arrange
        var text = "Task";
        var status = TodoStatus.Completed;
        var date = new DateTime(2023, 1, 1, 12, 0, 0);

        // Act
        var item = new TodoItem(text, status, date);

        // Assert
        Assert.Equal(text, item.Text);
        Assert.Equal(status, item.Status);
        Assert.Equal(date, item.LastUpdate);
    }

    [Fact]
    public void SetStatus_ChangesStatusAndUpdatesLastUpdate()
    {
        // Arrange
        var item = new TodoItem("Task");
        var before = DateTime.Now;
        System.Threading.Thread.Sleep(1); // ensure time changes

        // Act
        item.SetStatus(TodoStatus.InProgress);

        // Assert
        Assert.Equal(TodoStatus.InProgress, item.Status);
        Assert.True(item.LastUpdate > before);
    }

    [Fact]
    public void UpdateText_ChangesTextAndUpdatesLastUpdate()
    {
        // Arrange
        var item = new TodoItem("Old text");
        var before = DateTime.Now;
        System.Threading.Thread.Sleep(1);

        // Act
        item.UpdateText("New text");

        // Assert
        Assert.Equal("New text", item.Text);
        Assert.True(item.LastUpdate > before);
    }

    [Theory]
    [InlineData("Short text", "Short text")]
    [InlineData("This is a very long text that should be truncated to 30 characters exactly", "This is a very long text th...")]
    public void GetShortInfo_ReturnsTruncatedText(string input, string expected)
    {
        // Arrange
        var item = new TodoItem(input);

        // Act
        var shortInfo = item.GetShortInfo();

        // Assert
        Assert.Equal(expected, shortInfo);
    }

    [Fact]
    public void GetFullInfo_ReturnsMultilineStringWithDetails()
    {
        // Arrange
        var item = new TodoItem("Test\nMultiline", TodoStatus.Postponed, new DateTime(2024, 5, 10, 14, 30, 0));

        // Act
        var fullInfo = item.GetFullInfo();

        // Assert
        Assert.Contains("Текст задачи: \nTest\nMultiline", fullInfo);
        Assert.Contains("Статус: Postponed", fullInfo);
        Assert.Contains("Дата изменения: 10.05.2024 14:30:00", fullInfo);
    }
}
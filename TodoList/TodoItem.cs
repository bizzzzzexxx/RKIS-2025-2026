using System;

namespace TodoList;

public class TodoItem
{
    public string Text { get; private set; }
    public TodoStatus Status { get; private set; }
    public DateTime LastUpdate { get; private set; }

    public TodoItem(string text)
    {
        Text = text;
        Status = TodoStatus.NotStarted;
        LastUpdate = DateTime.Now;
    }

    public TodoItem(string text, TodoStatus status, DateTime lastUpdate)
    {
        Text = text;
        Status = status;
        LastUpdate = lastUpdate;
    }

    public void SetStatus(TodoStatus newStatus)
    {
        Status = newStatus;
        LastUpdate = DateTime.Now;
    }

    public void UpdateText(string newText)
    {
        Text = newText;
        LastUpdate = DateTime.Now;
    }

    public string GetShortInfo() => Text.Length <= 30 ? Text : Text.Substring(0, 27) + "...";
    public string GetFullInfo() => $"Текст задачи: \n{Text}\n" +
                                   $"Статус: {Status}\n" +
                                   $"Дата изменения: {LastUpdate:dd.MM.yyyy HH:mm:ss}";
}
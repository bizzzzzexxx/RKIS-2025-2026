using System;
using TodoList.Exceptions;

namespace TodoList;

public class StatusCommand : ICommand
{
    private readonly int _index;
    private readonly TodoStatus _newStatus;
    private TodoStatus _oldStatus;

    public StatusCommand(int index, TodoStatus newStatus)
    {
        _index = index;
        _newStatus = newStatus;
    }

    public void Execute()
    {
        if (AppInfo.CurrentProfileId == null)
            throw new AuthenticationException("Вы не авторизованы.");

        var todoList = AppInfo.CurrentTodoList;
        if (_index < 0 || _index >= todoList.Count)
            throw new TaskNotFoundException($"Задача с индексом {_index + 1} не существует.");

        _oldStatus = todoList[_index].Status;
        todoList.SetStatus(_index, _newStatus);
        Console.WriteLine($"Задача {_index + 1} получила статус {_newStatus}");

        AppInfo.UndoStack.Push(this);
        AppInfo.RedoStack.Clear();
    }

    public void Unexecute()
    {
        if (AppInfo.CurrentTodoList != null && _index >= 0 && _index < AppInfo.CurrentTodoList.Count)
        {
            AppInfo.CurrentTodoList.SetStatus(_index, _oldStatus);
            Console.WriteLine($"Отменено изменение статуса задачи {_index + 1}");
        }
    }
}
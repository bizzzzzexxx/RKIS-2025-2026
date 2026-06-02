using System;
using TodoList.Exceptions;

namespace TodoList;

public class DeleteCommand : ICommand
{
    private readonly int _index;
    private TodoItem _deletedItem;

    public DeleteCommand(int index) => _index = index;

    public void Execute()
    {
        if (AppInfo.CurrentProfileId == null)
            throw new AuthenticationException("Вы не авторизованы.");

        var todoList = AppInfo.CurrentTodoList;
        if (_index < 0 || _index >= todoList.Count)
            throw new TaskNotFoundException($"Задача с индексом {_index + 1} не существует.");

        _deletedItem = todoList[_index];
        todoList.Delete(_index);
        Console.WriteLine($"Задача {_index + 1} удалена.");

        AppInfo.UndoStack.Push(this);
        AppInfo.RedoStack.Clear();
    }

    public void Unexecute()
    {
        if (_deletedItem != null && AppInfo.CurrentTodoList != null)
        {
            AppInfo.CurrentTodoList.Add(_deletedItem, silent: true);
            Console.WriteLine($"Отменено удаление: {_deletedItem.Text}");
        }
    }
}
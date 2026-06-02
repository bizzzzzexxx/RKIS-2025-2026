using System;
using TodoList.Exceptions;

namespace TodoList;

public class ReadCommand : ICommand
{
    private readonly int _index;

    public ReadCommand(int index) => _index = index;

    public void Execute()
    {
        if (AppInfo.CurrentProfileId == null)
            throw new AuthenticationException("Вы не авторизованы.");

        var todoList = AppInfo.CurrentTodoList;
        if (_index < 0 || _index >= todoList.Count)
            throw new TaskNotFoundException($"Задача с индексом {_index + 1} не существует.");

        todoList.Read(_index);
    }

    public void Unexecute() { }
}
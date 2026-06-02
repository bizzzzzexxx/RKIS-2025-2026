using System;
using TodoList.Exceptions;

namespace TodoList;

public class RedoCommand : ICommand
{
    public void Execute()
    {
        if (AppInfo.RedoStack.Count == 0)
            throw new InvalidOperationException("Нечего повторять.");

        var command = AppInfo.RedoStack.Pop();
        command.Execute();
        Console.WriteLine("Повтор выполнена.");
    }

    public void Unexecute() { }
}
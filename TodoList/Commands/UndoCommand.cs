using System;
using TodoList.Exceptions;

namespace TodoList;

public class UndoCommand : ICommand
{
    public void Execute()
    {
        if (AppInfo.UndoStack.Count == 0)
            throw new InvalidOperationException("Нечего отменять.");

        var command = AppInfo.UndoStack.Pop();
        command.Unexecute();
        AppInfo.RedoStack.Push(command);
        Console.WriteLine("Отмена выполнена.");
    }

    public void Unexecute() { }
}
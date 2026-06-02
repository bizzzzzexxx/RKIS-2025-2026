using System;

namespace TodoList;

public class ExitCommand : ICommand
{
    public void Execute()
    {
        Console.WriteLine("Программа завершена.");
        AppInfo.IsRunning = false;
    }

    public void Unexecute()
    {
    }
}
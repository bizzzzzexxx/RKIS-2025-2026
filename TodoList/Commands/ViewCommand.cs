using System;

namespace TodoList;

public class ViewCommand : ICommand
{
    private readonly bool _showIndex;
    private readonly bool _showStatus;
    private readonly bool _showUpdateDate;

    public ViewCommand(bool showIndex, bool showStatus, bool showUpdateDate)
    {
        _showIndex = showIndex;
        _showStatus = showStatus;
        _showUpdateDate = showUpdateDate;
    }

    public void Execute()
    {
        if (AppInfo.CurrentProfileId == null)
        {
            Console.WriteLine("Нет активного профиля.");
            return;
        }
        AppInfo.CurrentTodoList?.View(_showIndex, _showStatus, _showUpdateDate);
    }

    public void Unexecute() { }
}
using System;
using System.Collections.Generic;

namespace TodoList;

public static class AppInfo
{
    public static List<Profile> Profiles { get; set; } = new();
    public static Guid? CurrentProfileId { get; set; } = null;

    public static Dictionary<Guid, TodoList> TodosDictionary { get; } = new();

    public static TodoList CurrentTodoList =>
        CurrentProfileId.HasValue && TodosDictionary.ContainsKey(CurrentProfileId.Value)
            ? TodosDictionary[CurrentProfileId.Value]
            : null;

    public static Stack<ICommand> UndoStack { get; } = new();
    public static Stack<ICommand> RedoStack { get; } = new();

    public static bool IsRunning { get; set; } = true; 

    public static void ClearStacks()
    {
        UndoStack.Clear();
        RedoStack.Clear();
    }

    public static void SetCurrentProfile(Guid profileId)
    {
        CurrentProfileId = profileId;
        ClearStacks();
        if (!TodosDictionary.ContainsKey(profileId))
            TodosDictionary[profileId] = new TodoList();
    }

    public static void Logout()
    {
        CurrentProfileId = null;
        ClearStacks();
    }
}
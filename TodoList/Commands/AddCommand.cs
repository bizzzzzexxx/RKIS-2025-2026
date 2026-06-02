using System;
using TodoList.Exceptions;

namespace TodoList;

public class AddCommand : ICommand
{
    private readonly string _text;
    private readonly bool _isMultiline;
    private int _addedIndex = -1;

    public AddCommand(string text, bool isMultiline)
    {
        _text = text;
        _isMultiline = isMultiline;
    }

    public void Execute()
    {
        if (AppInfo.CurrentProfileId == null)
            throw new AuthenticationException("Вы не авторизованы.");

        string finalText = _text;
        if (_isMultiline)
        {
            Console.WriteLine("Многострочный ввод, введите !end для завершения");
            finalText = "";
            while (true)
            {
                string line = Console.ReadLine();
                if (line == "!end") break;
                finalText += line + "\n";
            }
        }

        var todoList = AppInfo.CurrentTodoList;
        var item = new TodoItem(finalText);
        todoList.Add(item);
        _addedIndex = todoList.Count - 1;
        Console.WriteLine($"Задача добавлена: {finalText}");

        AppInfo.UndoStack.Push(this);
        AppInfo.RedoStack.Clear();
    }

    public void Unexecute()
    {
        if (_addedIndex >= 0 && AppInfo.CurrentTodoList != null && _addedIndex < AppInfo.CurrentTodoList.Count)
        {
            AppInfo.CurrentTodoList.Delete(_addedIndex);
            Console.WriteLine($"Отменено добавление: {_text}");
        }
    }
}
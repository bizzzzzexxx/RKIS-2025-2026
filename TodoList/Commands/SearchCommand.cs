using System;
using System.Collections.Generic;
using System.Linq;
using TodoList.Exceptions;

namespace TodoList;

public class SearchCommand : ICommand
{
    private readonly string _contains;
    private readonly string _startsWith;
    private readonly string _endsWith;
    private readonly DateTime? _from;
    private readonly DateTime? _to;
    private readonly TodoStatus? _status;
    private readonly string _sortBy;
    private readonly bool _descending;
    private readonly int? _top;

    public SearchCommand(string contains, string startsWith, string endsWith,
                         DateTime? from, DateTime? to, TodoStatus? status,
                         string sortBy, bool descending, int? top)
    {
        _contains = contains;
        _startsWith = startsWith;
        _endsWith = endsWith;
        _from = from;
        _to = to;
        _status = status;
        _sortBy = sortBy;
        _descending = descending;
        _top = top;
    }

    public void Execute()
    {
        if (AppInfo.CurrentProfileId == null)
            throw new AuthenticationException("Вы не авторизованы.");

        var todos = AppInfo.CurrentTodoList;
        if (todos == null || todos.Count == 0)
        {
            Console.WriteLine("Нет задач для поиска.");
            return;
        }

        var items = Enumerable.Range(0, todos.Count).Select(i => (Index: i, Item: todos[i]));
        var query = items.AsEnumerable();

        if (!string.IsNullOrEmpty(_contains))
            query = query.Where(x => x.Item.Text.Contains(_contains, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(_startsWith))
            query = query.Where(x => x.Item.Text.StartsWith(_startsWith, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(_endsWith))
            query = query.Where(x => x.Item.Text.EndsWith(_endsWith, StringComparison.OrdinalIgnoreCase));
        if (_from.HasValue)
            query = query.Where(x => x.Item.LastUpdate.Date >= _from.Value.Date);
        if (_to.HasValue)
            query = query.Where(x => x.Item.LastUpdate.Date <= _to.Value.Date);
        if (_status.HasValue)
            query = query.Where(x => x.Item.Status == _status.Value);

        if (_sortBy == "text")
            query = _descending ? query.OrderByDescending(x => x.Item.Text) : query.OrderBy(x => x.Item.Text);
        else if (_sortBy == "date")
            query = _descending ? query.OrderByDescending(x => x.Item.LastUpdate) : query.OrderBy(x => x.Item.LastUpdate);
        else
            query = _descending ? query.OrderByDescending(x => x.Index) : query.OrderBy(x => x.Index);

        if (_top.HasValue && _top.Value > 0)
            query = query.Take(_top.Value);

        var result = query.ToList();
        if (!result.Any())
        {
            Console.WriteLine("Ничего не найдено");
            return;
        }

        PrintResultTable(result);
    }

    private void PrintResultTable(List<(int Index, TodoItem Item)> items)
    {
        const int indexWidth = 8, textWidth = 36, statusWidth = 16, dateWidth = 20;
        string separator = "+-" + string.Join("-+-", new[] { indexWidth, textWidth, statusWidth, dateWidth }.Select(w => new string('-', w))) + "-+";
        string header = $"| {"Индекс",-indexWidth} | {"Текст задачи",-textWidth} | {"Статус",-statusWidth} | {"Дата обновления",-dateWidth} |";

        Console.WriteLine(separator);
        Console.WriteLine(header);
        Console.WriteLine(separator.Replace('-', '='));

        foreach (var (index, item) in items)
        {
            string text = item.GetShortInfo();
            if (text.Length > textWidth) text = text.Substring(0, textWidth - 3) + "...";
            string status = item.Status.ToString();
            string date = item.LastUpdate.ToString("yyyy-MM-dd HH:mm");

            Console.WriteLine($"| {(index + 1),-indexWidth} | {text,-textWidth} | {status,-statusWidth} | {date,-dateWidth} |");
        }
        Console.WriteLine(separator);
    }

    public void Unexecute() { }
}
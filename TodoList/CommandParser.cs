using System;
using System.Collections.Generic;
using System.Linq;
using TodoList.Exceptions;

namespace TodoList
{
    public static class CommandParser
    {
        private static readonly Dictionary<string, Func<string, ICommand>> Handlers = new();

        static CommandParser()
        {
            Handlers["help"]    = _ => new HelpCommand();
            Handlers["profile"] = args => new ProfileCommand(args.Contains("--out") || args.Contains("-o"));
            Handlers["add"]     = ParseAdd;
            Handlers["view"]    = ParseView;
            Handlers["status"]  = ParseStatus;
            Handlers["delete"]  = ParseDelete;
            Handlers["update"]  = ParseUpdate;
            Handlers["read"]    = ParseRead;
            Handlers["undo"]    = _ => new UndoCommand();
            Handlers["redo"]    = _ => new RedoCommand();
            Handlers["exit"]    = _ => new ExitCommand();
            Handlers["search"]  = ParseSearch;
            Handlers["load"]    = ParseLoad;
            Handlers["sync"]    = ParseSync;
        }

        public static ICommand? Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new InvalidCommandException("Команда не может быть пустой.");

            string[] parts = input.Split(' ', 2);
            string cmdName = parts[0].ToLower();
            string args = parts.Length > 1 ? parts[1] : "";

            if (Handlers.TryGetValue(cmdName, out var handler))
                return handler(args);

            throw new InvalidCommandException($"Неизвестная команда: '{cmdName}'.");
        }

        private static ICommand ParseAdd(string args)
        {
            var flags = ParseFlags(args);
            bool multiline = flags.Contains("--multiline") || flags.Contains("-m");
            string text = ExtractText(args);
            if (string.IsNullOrWhiteSpace(text) && !multiline)
                throw new InvalidArgumentException("Текст задачи не может быть пустым.");
            return new AddCommand(text, multiline);
        }

        private static ICommand ParseView(string args)
        {
            var flags = ParseFlags(args);
            bool showAll = flags.Contains("--all") || flags.Contains("-a");
            return new ViewCommand(
                flags.Contains("--index") || flags.Contains("-i") || showAll,
                flags.Contains("--status") || flags.Contains("-s") || showAll,
                flags.Contains("--update-date") || flags.Contains("-d") || showAll
            );
        }

        private static ICommand ParseStatus(string args)
        {
            string[] parts = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
                throw new InvalidArgumentException("Укажите индекс и статус.");
            if (!int.TryParse(parts[0], out int idx))
                throw new InvalidArgumentException("Индекс должен быть числом.");
            if (!Enum.TryParse<TodoStatus>(parts[1], true, out var status))
                throw new InvalidArgumentException($"Недопустимый статус. Доступны: {string.Join(", ", Enum.GetNames<TodoStatus>())}");
            return new StatusCommand(idx - 1, status);
        }

        private static ICommand ParseDelete(string args)
        {
            if (string.IsNullOrWhiteSpace(args))
                throw new InvalidArgumentException("Укажите индекс задачи.");
            if (!int.TryParse(args.Trim(), out int idx))
                throw new InvalidArgumentException("Индекс должен быть числом.");
            return new DeleteCommand(idx - 1);
        }

        private static ICommand ParseUpdate(string args)
        {
            string[] parts = args.Split(' ', 2);
            if (parts.Length < 2)
                throw new InvalidArgumentException("Укажите индекс и новый текст.");
            if (!int.TryParse(parts[0], out int idx))
                throw new InvalidArgumentException("Индекс должен быть числом.");
            if (string.IsNullOrWhiteSpace(parts[1]))
                throw new InvalidArgumentException("Новый текст не может быть пустым.");
            return new UpdateCommand(idx - 1, parts[1]);
        }

        private static ICommand ParseRead(string args)
        {
            if (string.IsNullOrWhiteSpace(args))
                throw new InvalidArgumentException("Укажите индекс задачи.");
            if (!int.TryParse(args.Trim(), out int idx))
                throw new InvalidArgumentException("Индекс должен быть числом.");
            return new ReadCommand(idx - 1);
        }

        private static ICommand ParseSearch(string args) => new SearchCommand(
            GetFlagValue(args, "--contains"),
            GetFlagValue(args, "--starts-with"),
            GetFlagValue(args, "--ends-with"),
            ParseDateFlag(args, "--from"),
            ParseDateFlag(args, "--to"),
            ParseStatusFlag(args),
            GetSortBy(args, out bool desc),
            desc,
            ParseTop(args)
        );

        private static ICommand ParseLoad(string args)
        {
            string[] parts = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
                throw new InvalidArgumentException("Необходимо указать количество загрузок и размер.");
            if (!int.TryParse(parts[0], out int count) || count <= 0)
                throw new InvalidArgumentException("Количество загрузок должно быть положительным целым числом.");
            if (!int.TryParse(parts[1], out int size) || size <= 0)
                throw new InvalidArgumentException("Размер загрузки должен быть положительным целым числом.");
            return new LoadCommand(count, size);
        }

        private static ICommand ParseSync(string args) => new SyncCommand(
            args.Contains("--pull"),
            args.Contains("--push")
        );

        private static HashSet<string> ParseFlags(string args) =>
            new HashSet<string>(args.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(p => p.StartsWith("-"))
                .SelectMany(p => p.StartsWith("--") ? new[] { p } : p.Skip(1).Select(c => "-" + c)));

        private static string ExtractText(string args) =>
            string.Join(" ", args.Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(p => !p.StartsWith("-")));

        private static string? GetFlagValue(string args, string flag)
        {
            int idx = args.IndexOf(flag);
            if (idx < 0) return null;
            var parts = args.Substring(idx).Split(' ', 2);
            return parts.Length > 1 ? parts[1].Trim('"') : null;
        }

        private static DateTime? ParseDateFlag(string args, string flag)
        {
            string? val = GetFlagValue(args, flag);
            if (val == null) return null;
            if (!DateTime.TryParse(val, out var date))
                throw new InvalidArgumentException($"Некорректная дата: {val}. Формат yyyy-MM-dd");
            return date;
        }

        private static TodoStatus? ParseStatusFlag(string args)
        {
            string? val = GetFlagValue(args, "--status");
            if (val == null) return null;
            if (!Enum.TryParse<TodoStatus>(val, true, out var status))
                throw new InvalidArgumentException($"Недопустимый статус: {val}");
            return status;
        }

        private static string? GetSortBy(string args, out bool descending)
        {
            descending = args.Contains("--desc");
            string? sort = GetFlagValue(args, "--sort");
            if (sort != null && sort != "text" && sort != "date")
                throw new InvalidArgumentException("--sort может быть только 'text' или 'date'");
            return sort;
        }

        private static int? ParseTop(string args)
        {
            string? val = GetFlagValue(args, "--top");
            if (val == null) return null;
            if (!int.TryParse(val, out int top) || top <= 0)
                throw new InvalidArgumentException("--top должен быть положительным числом.");
            return top;
        }
    }
}
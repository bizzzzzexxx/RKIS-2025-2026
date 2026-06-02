using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TodoList;

public class LoadCommand : ICommand
{
    private readonly int _downloadsCount;
    private readonly int _downloadSize;
    private static readonly object _consoleLock = new object();

    public LoadCommand(int downloadsCount, int downloadSize)
    {
        _downloadsCount = downloadsCount;
        _downloadSize = downloadSize;
    }

    public void Execute()
    {
        RunAsync().Wait();
    }

    private async Task RunAsync()
    {
        int requiredHeight = Console.CursorTop + _downloadsCount + 2;
        if (Console.BufferHeight < requiredHeight)
            Console.BufferHeight = requiredHeight;

        int startRow = Console.CursorTop;

        for (int i = 0; i < _downloadsCount; i++)
            Console.WriteLine();

        var tasks = new List<Task>();
        for (int i = 0; i < _downloadsCount; i++)
        {
            int index = i;
            int row = startRow + index;
            tasks.Add(DownloadAsync(index, row));
        }

        await Task.WhenAll(tasks);
        Console.WriteLine("\nВсе загрузки завершены.");
    }

    private async Task DownloadAsync(int index, int row)
    {
        var random = new Random(index);
        for (int current = 0; current <= _downloadSize; current++)
        {
            int percent = (current * 100) / _downloadSize;
            string bar = BuildProgressBar(percent);
            lock (_consoleLock)
            {
                if (row >= Console.BufferHeight)
                    Console.BufferHeight = row + 1;

                Console.SetCursorPosition(0, row);
                Console.Write(bar);
            }
            await Task.Delay(random.Next(10, 50));
        }
    }

    private string BuildProgressBar(int percent)
    {
        const int totalBars = 20;
        int filled = (percent * totalBars) / 100;
        string bar = new string('#', filled) + new string('-', totalBars - filled);
        return $"[{bar}] {percent,3}%";
    }

    public void Unexecute() { }
}
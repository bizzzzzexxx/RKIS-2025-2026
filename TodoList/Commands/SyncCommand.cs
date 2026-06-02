using System;
using System.Linq;
using TodoList.Exceptions;

namespace TodoList
{
    public class SyncCommand : ICommand
    {
        private readonly bool _pull;
        private readonly bool _push;

        public SyncCommand(bool pull, bool push)
        {
            _pull = pull;
            _push = push;
        }

        public void Execute()
        {
            if (!_pull && !_push)
            {
                Console.WriteLine("Используйте --pull или --push");
                return;
            }

            if (_pull && _push)
            {
                Console.WriteLine("Нельзя использовать --pull и --push одновременно");
                return;
            }

            if (!IsServerAvailable())
            {
                Console.WriteLine("Ошибка: сервер недоступен.");
                return;
            }

            var apiStorage = new ApiDataStorage();

            if (_pull)
            {
                try
                {
                    var remoteProfiles = apiStorage.LoadProfiles().ToList();
                    var remoteTodos = AppInfo.CurrentProfileId.HasValue
                        ? apiStorage.LoadTodos(AppInfo.CurrentProfileId.Value).ToList()
                        : new List<TodoItem>();

                    var localStorage = new FileManager(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data"));
                    localStorage.SaveProfiles(remoteProfiles);

                    if (AppInfo.CurrentProfileId.HasValue)
                    {
                        var localTodoList = new TodoList();
                        foreach (var item in remoteTodos)
                            localTodoList.Add(item, silent: true);
                        localStorage.SaveTodos(AppInfo.CurrentProfileId.Value, localTodoList.ToList());
                    }

                    AppInfo.Profiles = remoteProfiles;
                    if (AppInfo.CurrentProfileId.HasValue && AppInfo.TodosDictionary.ContainsKey(AppInfo.CurrentProfileId.Value))
                    {
                        var newList = new TodoList();
                        foreach (var item in remoteTodos)
                            newList.Add(item, silent: true);
                        AppInfo.TodosDictionary[AppInfo.CurrentProfileId.Value] = newList;
                    }

                    Console.WriteLine("Синхронизация (pull) завершена.");
                }
                catch (Exception ex)
                {
                    throw new DataStorageException($"Ошибка при pull: {ex.Message}", ex);
                }
            }
            else if (_push)
            {
                try
                {
                    var localProfiles = AppInfo.Profiles;
                    apiStorage.SaveProfiles(localProfiles);

                    if (AppInfo.CurrentProfileId.HasValue && AppInfo.CurrentTodoList != null)
                    {
                        apiStorage.SaveTodos(AppInfo.CurrentProfileId.Value, AppInfo.CurrentTodoList.ToList());
                    }

                    Console.WriteLine("Синхронизация (push) завершена.");
                }
                catch (Exception ex)
                {
                    throw new DataStorageException($"Ошибка при push: {ex.Message}", ex);
                }
            }
        }

        private static bool IsServerAvailable()
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                var response = client.GetAsync("http://localhost:5000/profiles").Result;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Unexecute() { }
    }
}
using System;
using System.Linq;
using System.IO;
using TodoList.Exceptions;

namespace TodoList
{
    class Program
    {
        public static void Main()
        {
            Console.WriteLine("Работу выполнили Десятун Николаенко");

            string dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            var storage = new FileManager(dataDir);

            AppInfo.Profiles = storage.LoadProfiles().ToList();

            while (AppInfo.IsRunning)
            {
                try
                {
                    if (AppInfo.CurrentProfileId == null)
                    {
                        Console.Write("\nВойти в существующий профиль? [y/n]: ");
                        string choice = Console.ReadLine()?.ToLower();
                        switch (choice)
                        {
                            case "y":
                                Login(storage);
                                break;
                            case "n":
                                Register(storage);
                                break;
                            default:
                                Console.WriteLine("Некорректный ввод. Завершение программы.");
                                return;
                        }
                    }

                    if (AppInfo.CurrentProfileId == null)
                        continue;

                    var userId = AppInfo.CurrentProfileId.Value;
                    if (!AppInfo.TodosDictionary.ContainsKey(userId))
                    {
                        var loadedItems = storage.LoadTodos(userId);
                        var todoList = new TodoList();
                        foreach (var item in loadedItems)
                            todoList.Add(item, silent: true);
                        AppInfo.TodosDictionary[userId] = todoList;
                    }

                    var currentTodoList = AppInfo.CurrentTodoList;
                    currentTodoList.OnTodoAdded   += _ => SaveCurrentUserTodos(storage);
                    currentTodoList.OnTodoDeleted += _ => SaveCurrentUserTodos(storage);
                    currentTodoList.OnTodoUpdated += _ => SaveCurrentUserTodos(storage);
                    currentTodoList.OnStatusChanged += _ => SaveCurrentUserTodos(storage);

                    bool profileActive = true;
                    while (profileActive && AppInfo.CurrentProfileId != null && AppInfo.IsRunning)
                    {
                        Console.Write("\nВведите команду: ");
                        string input = Console.ReadLine();

                        ICommand cmd = CommandParser.Parse(input);
                        cmd?.Execute();

                        if (AppInfo.CurrentProfileId == null)
                            profileActive = false;
                    }
                }
                catch (TaskNotFoundException ex)     { Console.WriteLine($"Ошибка задачи: {ex.Message}"); }
                catch (AuthenticationException ex)   { Console.WriteLine($"Ошибка авторизации: {ex.Message}"); }
                catch (InvalidCommandException ex)   { Console.WriteLine($"Ошибка команды: {ex.Message}"); }
                catch (InvalidArgumentException ex)  { Console.WriteLine($"Ошибка аргументов: {ex.Message}"); }
                catch (DuplicateLoginException ex)   { Console.WriteLine($"Ошибка регистрации: {ex.Message}"); }
                catch (DataStorageException ex)      { Console.WriteLine($"Ошибка хранилища: {ex.Message}"); }
                catch (Exception ex)                 { Console.WriteLine($"Неожиданная ошибка: {ex.Message}"); }
            }
        }

        private static void SaveCurrentUserTodos(IDataStorage storage)
        {
            if (AppInfo.CurrentProfileId.HasValue && AppInfo.CurrentTodoList != null)
                storage.SaveTodos(AppInfo.CurrentProfileId.Value, AppInfo.CurrentTodoList.ToList());
        }

        private static void Login(IDataStorage storage)
        {
            Console.Write("Логин: ");
            string login = Console.ReadLine();
            Console.Write("Пароль: ");
            string password = Console.ReadLine();

            var profile = AppInfo.Profiles.FirstOrDefault(p => p.Login == login && p.VerifyPassword(password));
            if (profile != null)
            {
                AppInfo.SetCurrentProfile(profile.Id);
                Console.WriteLine($"Добро пожаловать, {profile.FirstName}!");
            }
            else
            {
                throw new AuthenticationException("Неверный логин или пароль.");
            }
        }

        private static void Register(IDataStorage storage)
        {
            Console.Write("Логин: ");
            string login = Console.ReadLine();
            if (AppInfo.Profiles.Any(p => p.Login == login))
                throw new DuplicateLoginException($"Пользователь с логином '{login}' уже существует.");

            Console.Write("Пароль: ");
            string password = Console.ReadLine();
            Console.Write("Имя: ");
            string firstName = Console.ReadLine();
            Console.Write("Фамилия: ");
            string lastName = Console.ReadLine();
            Console.Write("Год рождения: ");
            if (!int.TryParse(Console.ReadLine(), out int birthYear))
                throw new InvalidArgumentException("Год рождения должен быть числом.");

            var newProfile = new Profile(login, password, firstName, lastName, birthYear);
            AppInfo.Profiles.Add(newProfile);
            storage.SaveProfiles(AppInfo.Profiles);
            AppInfo.SetCurrentProfile(newProfile.Id);
            Console.WriteLine($"Профиль создан. Добро пожаловать, {firstName}!");
        }
    }
}
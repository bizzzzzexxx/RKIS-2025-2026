using System;
using TodoList.Exceptions;

namespace TodoList
{
    public class ProfileCommand : ICommand
    {
        private readonly bool _logout;

        public ProfileCommand(bool logout = false)
        {
            _logout = logout;
        }

        public void Execute()
        {
            if (_logout)
            {
                if (AppInfo.CurrentProfileId == null)
                {
                    Console.WriteLine("Вы не вошли в профиль.");
                    return;
                }
                AppInfo.Logout();
                Console.WriteLine("Вы вышли из профиля.");
            }
            else
            {
                if (AppInfo.CurrentProfileId == null)
                {
                    Console.WriteLine("Нет активного профиля.");
                    return;
                }
                var profile = AppInfo.Profiles.Find(p => p.Id == AppInfo.CurrentProfileId);
                if (profile != null)
                    Console.WriteLine(profile.GetInfo());
                else
                    Console.WriteLine("Профиль не найден.");
            }
        }

        public void Unexecute()
        {
        }
    }
}
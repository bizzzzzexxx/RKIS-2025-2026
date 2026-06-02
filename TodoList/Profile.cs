using System;

namespace TodoList;

public class Profile
{
    public Guid Id { get; }
    public string Login { get; }
    public string Password { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public int BirthYear { get; }

    public Profile(string login, string password, string firstName, string lastName, int birthYear)
    {
        Id = Guid.NewGuid();
        Login = login ?? throw new ArgumentNullException(nameof(login));
        Password = password ?? throw new ArgumentNullException(nameof(password));
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        BirthYear = birthYear;
    }

    public Profile(Guid id, string login, string password, string firstName, string lastName, int birthYear)
    {
        Id = id;
        Login = login ?? throw new ArgumentNullException(nameof(login));
        Password = password ?? throw new ArgumentNullException(nameof(password));
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        BirthYear = birthYear;
    }

    public bool VerifyPassword(string password) => Password == password;

    public string GetInfo() => $"{FirstName} {LastName} {DateTime.Now.Year - BirthYear}";
}
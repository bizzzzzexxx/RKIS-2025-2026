using System;

namespace TodoList.Exceptions
{
    public class ProfileNotFoundException : Exception
    {
        public ProfileNotFoundException(string message) : base(message) { }
    }
}
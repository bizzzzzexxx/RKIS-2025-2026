using System;

namespace TodoList.Exceptions
{
    public class InvalidCommandException : Exception
    {
        public InvalidCommandException(string message) : base(message) { }
    }
}
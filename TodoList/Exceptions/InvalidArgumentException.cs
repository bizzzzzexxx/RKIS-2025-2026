using System;

namespace TodoList.Exceptions
{
    public class InvalidArgumentException : Exception
    {
        public InvalidArgumentException(string message) : base(message) { }
    }
}
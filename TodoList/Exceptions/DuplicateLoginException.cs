using System;

namespace TodoList.Exceptions
{
    public class DuplicateLoginException : Exception
    {
        public DuplicateLoginException(string message) : base(message) { }
    }
}
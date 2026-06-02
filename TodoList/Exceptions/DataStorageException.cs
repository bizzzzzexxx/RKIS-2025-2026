using System;

namespace TodoList.Exceptions
{
    public class DataStorageException : Exception
    {
        public DataStorageException(string message) : base(message) { }
        public DataStorageException(string message, Exception inner) : base(message, inner) { }
    }
}
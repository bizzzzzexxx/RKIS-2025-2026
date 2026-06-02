using System;

namespace TodoList.Exceptions
{
    public class TaskNotFoundException : Exception
    {
        public TaskNotFoundException(string message) : base(message) { }
    }
}
namespace TodoList;

public interface ICommand
{
    void Execute();
    void Unexecute();
}
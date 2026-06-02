using System;

namespace TodoList
{
    public class HelpCommand : ICommand
    {
        public void Execute()
        {
            Console.WriteLine("""
            Доступные команды:
            help — справка
            profile [--out|-o] — информация о профиле или выход
            add "текст" [--multiline|-m] — добавить задачу
            view [--index|-i] [--status|-s] [--update-date|-d] [--all|-a] — просмотр задач
            search [флаги] — поиск задач с фильтрацией и сортировкой
            status индекс статус — изменить статус
            delete индекс — удалить задачу
            update индекс "новый текст" — обновить текст
            read индекс — полная информация
            undo — отменить
            redo — повторить
            load <количество> <размер> — параллельная загрузка с прогресс-барами
            sync --pull|--push — синхронизация с сервером (получить/отправить данные)
            exit — выход

            Флаги search:
              --contains <text>     текст содержит
              --starts-with <text>  текст начинается с
              --ends-with <text>    текст заканчивается на
              --from <yyyy-MM-dd>   дата изменения не раньше
              --to <yyyy-MM-dd>     дата изменения не позже
              --status <статус>     NotStarted, InProgress, Completed, Postponed, Failed
              --sort text|date      сортировка по тексту или дате
              --desc                сортировка по убыванию
              --top <n>             показать только первые n результатов
            """);
        }

        public void Unexecute() { }
    }
}
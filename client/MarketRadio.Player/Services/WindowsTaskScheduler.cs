using System;
using System.Linq;
using System.Text;
using Microsoft.Win32.TaskScheduler;
using Action = Microsoft.Win32.TaskScheduler.Action;

namespace MarketRadio.Player.Services
{
    public class WindowsTaskScheduler
    {//Этот код описывает класс WindowsTaskScheduler, который управляет созданием задач Windows для автоматического запуска приложения или выполнения заданного действия в определенное время. Рассмотрим подробнее его методы и логику работы:
        public void CreateTaskForStartup(TimeSpan when, string what)
        {//Определение пути задачи: Указывается путь, где будет храниться задача в планировщике.
            const string openAppTaskPath = "\\MarketRadio\\Open App";

            var existedTask = TaskService.Instance.GetTask(openAppTaskPath);// Проверка существования задачи: Сначала проверяется, существует ли уже задача с таким именем.

            var task = TaskService.Instance.NewTask(); //Создание новой задачи: Если задача еще не существует, создается новая с помощью TaskService.Instance.NewTask().
            task.Triggers.Add(GetDailyTrigger(when)); //Настройка триггера: Добавляется ежедневный триггер для запуска задачи с использованием времени, указанного в параметре when.
            task.Settings.StartWhenAvailable = true; //Настройка выполнения задачи: Задается автор задачи, команда для выполнения и другие настройки.
            task.RegistrationInfo.Author = "Market Radio";
            task.Actions.Add(new ExecAction(what));

            if (!NeedNewTask(existedTask?.Definition, task)) //Проверка на необходимость создания новой задачи: С помощью метода NeedNewTask проверяется, отличается ли новая задача от уже существующей.
            {
                return;
            }

            if (existedTask != null)
            {
                TaskService.Instance.RootFolder.DeleteTask(openAppTaskPath); //Удаление старой и регистрация новой задачи: Если задача существует и требуется создать новую, старая удаляется, и регистрируется новая задача.
            }
            TaskService.Instance.RootFolder.RegisterTaskDefinition(openAppTaskPath, task);
        }

        private DailyTrigger GetDailyTrigger(TimeSpan when)
        {
            return new DailyTrigger { DaysInterval = 1, StartBoundary = DateTime.Today.Add(when).AddDays(1) }; //Создает и возвращает ежедневный триггер для задачи, который будет запускаться в указанное время каждый день.
        }

        private bool NeedNewTask(TaskDefinition? oldTask, TaskDefinition newTask)
        {//Проверяет, нужно ли создать новую задачу, сравнивая определения старой и новой задач. Возвращает true, если задачи различаются и требуется создание новой.
            if (oldTask == null)
            {
                return true;
            }

            return GetTaskDefinitionString(oldTask) != GetTaskDefinitionString(newTask);
        }

        private string GetTaskDefinitionString(TaskDefinition taskDefinition)
        {//Генерирует строковое представление определения задачи, включая информацию о триггерах и действиях, для последующего сравнения в NeedNewTask.
            var sb = new StringBuilder();
            var triggers = taskDefinition.Triggers
                .Select(t => new
                {
                    StartTime = t.StartBoundary.TimeOfDay,
                    t.Repetition.Duration,
                    t.Repetition.Interval,
                    t.Repetition.StopAtDurationEnd,
                    t.TriggerType,
                    t.ExecutionTimeLimit
                })
                .OrderBy(t => t.StartTime);

            var taskActions = taskDefinition.Actions.Select(a => new
            {
                a.ActionType,
                Data = GetActionData(a)
            });

            sb.Append(string.Join("", triggers));
            sb.Append(Environment.NewLine);
            sb.Append(taskDefinition.Settings.StartWhenAvailable);
            sb.Append(Environment.NewLine);
            sb.Append(taskDefinition.RegistrationInfo.Author);
            sb.Append(Environment.NewLine);
            sb.Append(string.Join("", taskActions));
            return sb.ToString();
        }

        private string GetActionData(Action action)
        {//Возвращает строковое представление данных о действии задачи, включая аргументы и путь к выполняемому файлу.
            var sb = new StringBuilder();
            var ea = (ExecAction)action;

            return sb.Append(ea.Arguments).Append(ea.Path).Append(ea.WorkingDirectory)
                .Append(ea.ActionType).ToString();
        }
        //Этот класс позволяет легко интегрировать функциональность планировщика задач Windows в приложения .NET, автоматизируя процессы и повышая удобство использования программного обеспечения.
    }
}
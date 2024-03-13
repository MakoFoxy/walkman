using System;
using System.Linq;
using System.Text;
using Microsoft.Win32.TaskScheduler;
using Action = Microsoft.Win32.TaskScheduler.Action;

namespace MarketRadio.Player.Services
{
    public class WindowsTaskScheduler
    {
        public void CreateTaskForStartup(TimeSpan when, string what)
        {
            const string openAppTaskPath = "\\MarketRadio\\Open App";

            var existedTask = TaskService.Instance.GetTask(openAppTaskPath);

            var task = TaskService.Instance.NewTask();
            task.Triggers.Add(GetDailyTrigger(when));
            task.Settings.StartWhenAvailable = true;
            task.RegistrationInfo.Author = "Market Radio";
            task.Actions.Add(new ExecAction(what));

            if (!NeedNewTask(existedTask?.Definition, task))
            {
                return;
            }

            if (existedTask != null)
            {
                TaskService.Instance.RootFolder.DeleteTask(openAppTaskPath);
            }
            TaskService.Instance.RootFolder.RegisterTaskDefinition(openAppTaskPath, task);
        }

        private DailyTrigger GetDailyTrigger(TimeSpan when)
        {
            return new DailyTrigger {DaysInterval = 1, StartBoundary = DateTime.Today.Add(when).AddDays(1)};
        }

        private bool NeedNewTask(TaskDefinition? oldTask, TaskDefinition newTask)
        {
            if (oldTask == null)
            {
                return true;
            }
            
            return GetTaskDefinitionString(oldTask) != GetTaskDefinitionString(newTask);
        }

        private string GetTaskDefinitionString(TaskDefinition taskDefinition)
        {
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
        {
            var sb = new StringBuilder();
            var ea = (ExecAction)action;

            return sb.Append(ea.Arguments).Append(ea.Path).Append(ea.WorkingDirectory)
                .Append(ea.ActionType).ToString();
        }
    }
}
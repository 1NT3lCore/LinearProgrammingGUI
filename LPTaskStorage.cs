using System;
using System.Collections.Generic;

namespace LinearProgrammingGUI
{
    public static class LPTaskStorage
    {
        private static List<LPTaskData> tasks = new List<LPTaskData>();
        private static int nextId = 1;

        public static int Count => tasks.Count;

        public class LPTaskData
        {
            public int id { get; set; }
            public LPTask task { get; set; }
            public string description { get; set; }
            public DateTime created { get; set; }
        }

        public static int AddTask(LPTask task)
        {
            try
            {
                string description = GenerateDescription(task);
                LPTaskData taskData = new LPTaskData
                {
                    id = nextId++,
                    task = task,
                    description = description,
                    created = DateTime.Now
                };
                
                tasks.Add(taskData);
                return taskData.id;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при сохранении задачи: {ex.Message}");
            }
        }

        public static LPTask GetTask(int id)
        {
            foreach (var taskData in tasks)
            {
                if (taskData.id == id)
                {
                    return taskData.task;
                }
            }
            throw new ArgumentException($"Задача с ID {id} не найдена");
        }

        public static List<LPTaskData> GetAllTasks()
        {
            return new List<LPTaskData>(tasks);
        }

        public static bool RemoveTask(int id)
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                if (tasks[i].id == id)
                {
                    tasks.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public static void Clear()
        {
            tasks.Clear();
            nextId = 1;
        }

        private static string GenerateDescription(LPTask task)
        {
            string type = task.taskType == 1 ? "Max" : "Min";
            return $"{type} задача {task.n}x{task.m} от {DateTime.Now:dd.MM.yyyy HH:mm}";
        }
    }
}
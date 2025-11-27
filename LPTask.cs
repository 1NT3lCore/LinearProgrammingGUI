using System;

namespace LinearProgrammingGUI
{
    // Структура для хранения задачи ЛП
    public struct LPTask
    {
        public int n, m, taskType;
        public double[] c, b;
        public double[,] A;
        public string[] signs;

        // Конвертация в формат для новых методов
        public LinearProgrammingProblem ToLinearProgrammingProblem()
        {
            bool maximize = (taskType == 1); // 1 - максимизация, 2 - минимизация
            bool[] isLessOrEqual = new bool[m];

            for (int i = 0; i < m; i++)
            {
                isLessOrEqual[i] = (signs[i] == "<=");
            }

            return new LinearProgrammingProblem(c, A, b, isLessOrEqual, maximize);
        }
    }

    // Класс для представления задачи линейного программирования (для новых методов)
    public class LinearProgrammingProblem
    {
        public double[] C { get; set; }
        public double[,] A { get; set; }
        public double[] B { get; set; }
        public bool[] IsLessOrEqual { get; set; }
        public bool Maximize { get; set; }

        public LinearProgrammingProblem(double[] c, double[,] a, double[] b, bool[] isLessOrEqual, bool maximize)
        {
            C = c;
            A = a;
            B = b;
            IsLessOrEqual = isLessOrEqual;
            Maximize = maximize;
        }

        // Конвертация в LPTask
        public LPTask ToLPTask()
        {
            LPTask task = new LPTask();
            task.n = C.Length;
            task.m = B.Length; // Исправлено: используем B.Length вместо m
            task.taskType = Maximize ? 1 : 2;
            task.c = C;
            task.A = A;
            task.b = B;
            task.signs = new string[task.m]; // Исправлено: используем task.m

            for (int i = 0; i < task.m; i++) // Исправлено: используем task.m
            {
                task.signs[i] = IsLessOrEqual[i] ? "<=" : ">=";
            }

            return task;
        }
    }

    // Класс для результатов решения
    public class SolutionResult
    {
        public double[] Solution { get; set; }
        public double ObjectiveValue { get; set; }
        public bool IsOptimal { get; set; }
        public bool IsFeasible { get; set; }
        public bool IsUnbounded { get; set; }
        public string Message { get; set; }

        public SolutionResult()
        {
            IsOptimal = false;
            IsFeasible = false;
            IsUnbounded = false;
            Message = "";
        }
    }
}
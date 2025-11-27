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
    }
}
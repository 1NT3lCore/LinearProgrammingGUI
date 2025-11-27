using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinearProgrammingGUI
{
    public class SimplexSolver
    {
        private double[,] table;
        private int rows;
        private int cols;
        private List<int> basis;
        private int stepCount = 0;
        private LPTask task;

        public SimplexSolver(LPTask lpTask)
        {
            this.task = lpTask;

            // Преобразуем задачу в каноническую форму для симплекс-метода
            var canonical = ConvertToCanonicalForm(lpTask);

            rows = canonical.constraints.GetLength(0) + 1;
            cols = canonical.constraints.GetLength(1) + canonical.constraints.GetLength(0) + 1;

            table = new double[rows, cols];
            basis = new List<int>();

            InitializeTable(canonical.objective, canonical.constraints, canonical.rightHandSide);
        }

        private (double[] objective, double[,] constraints, double[] rightHandSide) ConvertToCanonicalForm(LPTask task)
        {
            int slackVars = task.m;
            int totalVars = task.n + slackVars;

            // Целевая функция (для максимизации оставляем как есть, для минимизации меняем знак)
            double[] objective = new double[totalVars];
            for (int i = 0; i < task.n; i++)
            {
                objective[i] = task.taskType == 1 ? task.c[i] : -task.c[i];
            }

            // Матрица ограничений с slack переменными
            double[,] constraints = new double[task.m, totalVars];
            double[] rightHandSide = new double[task.m];

            for (int i = 0; i < task.m; i++)
            {
                for (int j = 0; j < task.n; j++)
                {
                    constraints[i, j] = task.A[i, j];
                }

                // Добавляем slack переменные
                if (task.signs[i] == "<=")
                {
                    constraints[i, task.n + i] = 1; // + slack
                }
                else if (task.signs[i] == ">=")
                {
                    constraints[i, task.n + i] = -1; // - slack
                }
                // Для "=" не добавляем slack

                rightHandSide[i] = task.b[i];
            }

            return (objective, constraints, rightHandSide);
        }

        private void InitializeTable(double[] objectiveFunction, double[,] constraints, double[] rightHandSide)
        {
            // Целевая функция (в симплекс-таблице для минимизации)
            for (int j = 0; j < objectiveFunction.Length; j++)
            {
                table[0, j] = -objectiveFunction[j];
            }
            table[0, cols - 1] = 0;

            // Ограничения
            for (int i = 0; i < constraints.GetLength(0); i++)
            {
                for (int j = 0; j < constraints.GetLength(1); j++)
                {
                    table[i + 1, j] = constraints[i, j];
                }

                // Правая часть
                table[i + 1, cols - 1] = rightHandSide[i];

                // Добавляем в базис slack переменные
                if (constraints[i, task.n + i] == 1) // Только для slack переменных
                {
                    basis.Add(task.n + i);
                }
            }
        }

        public string SolveWithSteps()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════");
            sb.AppendLine("РЕШЕНИЕ СИМПЛЕКС-МЕТОДОМ");
            sb.AppendLine("═══════════════════════════════════════════════════");

            sb.AppendLine("\nНачальная симплекс-таблица:");
            sb.AppendLine(GetTableString());

            int pivotCol, pivotRow;

            while (!IsOptimal())
            {
                stepCount++;
                sb.AppendLine($"\n═══════════════════════════════════════════════════");
                sb.AppendLine($"ШАГ {stepCount}");
                sb.AppendLine($"═══════════════════════════════════════════════════");

                pivotCol = FindPivotColumn();
                if (pivotCol == -1) break;

                sb.AppendLine($"Ведущий столбец: {GetVariableName(pivotCol)} (индекс {pivotCol})");
                sb.AppendLine($"Коэффициент в целевой функции: {table[0, pivotCol]:F4}");

                pivotRow = FindPivotRow(pivotCol);
                if (pivotRow == -1)
                {
                    sb.AppendLine("ОШИБКА: Задача неограничена - нет положительных коэффициентов в ведущем столбце");
                    return sb.ToString();
                }

                sb.AppendLine($"Ведущая строка: строка {pivotRow} (базисная переменная {GetVariableName(basis[pivotRow - 1])})");
                sb.AppendLine($"Ведущий элемент: {table[pivotRow, pivotCol]:F4}");

                sb.AppendLine("\nОтношения для выбора ведущей строки:");
                for (int i = 1; i < rows; i++)
                {
                    if (table[i, pivotCol] > 0.0001)
                    {
                        double ratio = table[i, cols - 1] / table[i, pivotCol];
                        sb.AppendLine($"  Строка {i}: {table[i, cols - 1]:F2} / {table[i, pivotCol]:F2} = {ratio:F2}");
                    }
                }

                basis[pivotRow - 1] = pivotCol;
                sb.AppendLine($"Новая базисная переменная в строке {pivotRow}: {GetVariableName(pivotCol)}");

                MakePivotStep(pivotRow, pivotCol);
                sb.AppendLine("\nОбновленная симплекс-таблица:");
                sb.AppendLine(GetTableString());
            }

            if (IsOptimal())
            {
                sb.AppendLine("\n═══════════════════════════════════════════════════");
                sb.AppendLine("ОПТИМАЛЬНОЕ РЕШЕНИЕ ДОСТИГНУТО!");
                sb.AppendLine("═══════════════════════════════════════════════════");

                sb.AppendLine(GetFinalSolution());
            }

            return sb.ToString();
        }

        private bool IsOptimal()
        {
            for (int j = 0; j < cols - 1; j++)
            {
                if (table[0, j] < -0.0001)
                    return false;
            }
            return true;
        }

        private int FindPivotColumn()
        {
            int pivotCol = -1;
            double minValue = 0;

            for (int j = 0; j < cols - 1; j++)
            {
                if (table[0, j] < minValue)
                {
                    minValue = table[0, j];
                    pivotCol = j;
                }
            }

            return pivotCol;
        }

        private int FindPivotRow(int pivotCol)
        {
            int pivotRow = -1;
            double minRatio = double.MaxValue;

            for (int i = 1; i < rows; i++)
            {
                if (table[i, pivotCol] > 0.0001)
                {
                    double ratio = table[i, cols - 1] / table[i, pivotCol];
                    if (ratio < minRatio && ratio >= 0)
                    {
                        minRatio = ratio;
                        pivotRow = i;
                    }
                }
            }

            return pivotRow;
        }

        private void MakePivotStep(int pivotRow, int pivotCol)
        {
            double pivotValue = table[pivotRow, pivotCol];

            // Делим ведущую строку на ведущий элемент
            for (int j = 0; j < cols; j++)
            {
                table[pivotRow, j] /= pivotValue;
            }

            // Вычитаем ведущую строку из других строк
            for (int i = 0; i < rows; i++)
            {
                if (i != pivotRow)
                {
                    double factor = table[i, pivotCol];
                    for (int j = 0; j < cols; j++)
                    {
                        table[i, j] -= factor * table[pivotRow, j];
                    }
                }
            }
        }

        private string GetTableString()
        {
            StringBuilder sb = new StringBuilder();

            // Заголовок
            sb.Append("Базис\t");
            for (int j = 0; j < cols - 1; j++)
            {
                sb.Append($"{GetVariableName(j)}\t");
            }
            sb.AppendLine("Решение");

            // Строки
            for (int i = 0; i < rows; i++)
            {
                if (i == 0)
                    sb.Append("Z\t");
                else
                    sb.Append($"{GetVariableName(basis[i - 1])}\t");

                for (int j = 0; j < cols; j++)
                {
                    sb.Append($"{table[i, j]:F2}\t");
                }
                sb.AppendLine();
            }

            sb.Append("\nТекущий базис: " + string.Join(", ",
                basis.Select(b => $"{GetVariableName(b)} = {table[basis.IndexOf(b) + 1, cols - 1]:F2}")));

            return sb.ToString();
        }

        private string GetVariableName(int index)
        {
            if (index < task.n)
                return $"x{index + 1}";
            else
                return $"s{index - task.n + 1}";
        }

        private string GetFinalSolution()
        {
            StringBuilder sb = new StringBuilder();

            double[] solution = new double[cols - 1];
            for (int i = 0; i < basis.Count; i++)
            {
                solution[basis[i]] = table[i + 1, cols - 1];
            }

            sb.AppendLine("\nЗначения переменных:");
            for (int i = 0; i < task.n; i++)
            {
                sb.AppendLine($"  x{i + 1} = {solution[i]:F4}");
            }

            double objectiveValue = table[0, cols - 1];
            // Корректируем знак для минимизации
            if (task.taskType == 2) // Минимизация
                objectiveValue = -objectiveValue;

            sb.AppendLine($"\nОптимальное значение целевой функции:");
            sb.AppendLine($"  F(X) = {objectiveValue:F4}");

            sb.AppendLine("\nБазисные переменные:");
            foreach (var basisVar in basis)
            {
                if (basisVar < task.n) // Только основные переменные
                {
                    sb.AppendLine($"  {GetVariableName(basisVar)} = {table[basis.IndexOf(basisVar) + 1, cols - 1]:F4}");
                }
            }

            return sb.ToString();
        }
    }
}

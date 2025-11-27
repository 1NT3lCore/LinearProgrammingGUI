using System;
using System.Collections.Generic;
using System.Linq;

namespace LinearProgrammingGUI
{
    // Основной класс для решения задач ЛП
    public static class LPSolver
    {
        // Решение прямой и двойственной задачи одновременно
        public static (SolutionResult primal, SolutionResult dual) SolvePrimalDualSimultaneously(LPTask task)
        {
            var problem = task.ToLinearProgrammingProblem();
            var primalResult = SolvePrimalProblem(problem);
            var dualResult = SolveDualProblem(problem);

            return (primalResult, dualResult);
        }

        // Решение двойственным симплекс-методом
        public static SolutionResult SolveDualSimplex(LPTask task)
        {
            var problem = task.ToLinearProgrammingProblem();
            var canonicalForm = ConvertToCanonicalForm(problem);

            int m = canonicalForm.A.GetLength(0);
            int n = canonicalForm.A.GetLength(1);

            var basis = new List<int>();
            for (int i = 0; i < m; i++)
            {
                basis.Add(n - m + i);
            }

            double[,] table = CreateSimplexTable(canonicalForm, basis);

            return DualSimplexMethod(table, basis, canonicalForm.Maximize);
        }

        // Решение прямой задачи
        private static SolutionResult SolvePrimalProblem(LinearProgrammingProblem problem)
        {
            try
            {
                var canonicalForm = ConvertToCanonicalForm(problem);
                return SimplexMethod(canonicalForm);
            }
            catch (Exception ex)
            {
                return new SolutionResult { Message = $"Ошибка: {ex.Message}" };
            }
        }

        // Решение двойственной задачи
        private static SolutionResult SolveDualProblem(LinearProgrammingProblem problem)
        {
            try
            {
                var dualProblem = ConvertToDualProblem(problem);
                return SimplexMethod(dualProblem);
            }
            catch (Exception ex)
            {
                return new SolutionResult { Message = $"Ошибка: {ex.Message}" };
            }
        }

        // Преобразование к двойственной задаче
        private static LinearProgrammingProblem ConvertToDualProblem(LinearProgrammingProblem primal)
        {
            int m = primal.A.GetLength(0);
            int n = primal.A.GetLength(1);

            double[] dualC = new double[m];
            Array.Copy(primal.B, dualC, m);

            double[,] dualA = new double[n, m];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    dualA[i, j] = primal.A[j, i];
                }
            }

            double[] dualB = new double[n];
            Array.Copy(primal.C, dualB, n);

            bool[] dualIsLessOrEqual = new bool[n];
            for (int i = 0; i < n; i++)
            {
                dualIsLessOrEqual[i] = !primal.Maximize;
            }

            bool dualMaximize = !primal.Maximize;

            return new LinearProgrammingProblem(dualC, dualA, dualB, dualIsLessOrEqual, dualMaximize);
        }

        // Преобразование к канонической форме
        private static LinearProgrammingProblem ConvertToCanonicalForm(LinearProgrammingProblem problem)
        {
            int m = problem.A.GetLength(0);
            int n = problem.A.GetLength(1);

            int totalVars = n + m;

            double[] newC = new double[totalVars];
            Array.Copy(problem.C, newC, n);
            for (int i = n; i < totalVars; i++)
            {
                newC[i] = 0;
            }

            double[,] newA = new double[m, totalVars];
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    newA[i, j] = problem.A[i, j];
                }

                for (int j = 0; j < m; j++)
                {
                    if (i == j)
                    {
                        if (problem.IsLessOrEqual[i])
                            newA[i, n + j] = 1;
                        else
                            newA[i, n + j] = -1;
                    }
                    else
                    {
                        newA[i, n + j] = 0;
                    }
                }
            }

            return new LinearProgrammingProblem(newC, newA, problem.B, problem.IsLessOrEqual, problem.Maximize);
        }

        // Обычный симплекс-метод
        private static SolutionResult SimplexMethod(LinearProgrammingProblem problem)
        {
            var canonicalForm = ConvertToCanonicalForm(problem);
            int m = canonicalForm.A.GetLength(0);
            int n = canonicalForm.A.GetLength(1);

            var basis = new List<int>();
            for (int i = 0; i < m; i++)
            {
                basis.Add(n - m + i);
            }

            double[,] table = CreateSimplexTable(canonicalForm, basis);
            return PerformSimplex(table, basis, canonicalForm.Maximize);
        }

        // Создание симплекс-таблицы
        private static double[,] CreateSimplexTable(LinearProgrammingProblem problem, List<int> basis)
        {
            int m = problem.A.GetLength(0);
            int n = problem.A.GetLength(1);

            double[,] table = new double[m + 1, n + 1];

            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    table[i, j] = problem.A[i, j];
                }
                table[i, n] = problem.B[i];
            }

            for (int j = 0; j < n; j++)
            {
                table[m, j] = problem.Maximize ? -problem.C[j] : problem.C[j];
            }

            return table;
        }

        // Выполнение симплекс-метода
        private static SolutionResult PerformSimplex(double[,] table, List<int> basis, bool maximize)
        {
            int m = table.GetLength(0) - 1;
            int n = table.GetLength(1) - 1;
            int iterations = 0;
            const int maxIterations = 1000;

            while (iterations < maxIterations)
            {
                bool isOptimal = true;
                int pivotCol = -1;

                for (int j = 0; j < n; j++)
                {
                    if (table[m, j] < 0)
                    {
                        isOptimal = false;
                        if (pivotCol == -1 || table[m, j] < table[m, pivotCol])
                        {
                            pivotCol = j;
                        }
                    }
                }

                if (isOptimal)
                {
                    return ExtractSolution(table, basis, maximize);
                }

                int pivotRow = -1;
                double minRatio = double.MaxValue;

                for (int i = 0; i < m; i++)
                {
                    if (table[i, pivotCol] > 0)
                    {
                        double ratio = table[i, n] / table[i, pivotCol];
                        if (ratio < minRatio)
                        {
                            minRatio = ratio;
                            pivotRow = i;
                        }
                    }
                }

                if (pivotRow == -1)
                {
                    return new SolutionResult { IsUnbounded = true, Message = "Задача неограничена" };
                }

                basis[pivotRow] = pivotCol;
                UpdateSimplexTable(table, pivotRow, pivotCol);
                iterations++;
            }

            return new SolutionResult { Message = "Превышено максимальное количество итераций" };
        }

        // Двойственный симплекс-метод
        private static SolutionResult DualSimplexMethod(double[,] table, List<int> basis, bool maximize)
        {
            int m = table.GetLength(0) - 1;
            int n = table.GetLength(1) - 1;
            int iterations = 0;
            const int maxIterations = 1000;

            while (iterations < maxIterations)
            {
                int pivotRow = -1;
                double minB = 0;

                for (int i = 0; i < m; i++)
                {
                    if (table[i, n] < minB)
                    {
                        minB = table[i, n];
                        pivotRow = i;
                    }
                }

                if (pivotRow == -1)
                {
                    return ExtractSolution(table, basis, maximize);
                }

                int pivotCol = -1;
                double minRatio = double.MaxValue;

                for (int j = 0; j < n; j++)
                {
                    if (table[pivotRow, j] < 0)
                    {
                        double ratio = Math.Abs(table[m, j] / table[pivotRow, j]);
                        if (ratio < minRatio)
                        {
                            minRatio = ratio;
                            pivotCol = j;
                        }
                    }
                }

                if (pivotCol == -1)
                {
                    return new SolutionResult { IsFeasible = false, Message = "Задача несовместна" };
                }

                basis[pivotRow] = pivotCol;
                UpdateSimplexTable(table, pivotRow, pivotCol);
                iterations++;
            }

            return new SolutionResult { Message = "Превышено максимальное количество итераций" };
        }

        // Обновление симплекс-таблицы
        private static void UpdateSimplexTable(double[,] table, int pivotRow, int pivotCol)
        {
            int m = table.GetLength(0);
            int n = table.GetLength(1);

            double pivot = table[pivotRow, pivotCol];

            for (int j = 0; j < n; j++)
            {
                table[pivotRow, j] /= pivot;
            }

            for (int i = 0; i < m; i++)
            {
                if (i != pivotRow)
                {
                    double factor = table[i, pivotCol];
                    for (int j = 0; j < n; j++)
                    {
                        table[i, j] -= factor * table[pivotRow, j];
                    }
                }
            }
        }

        // Извлечение решения из симплекс-таблицы
        private static SolutionResult ExtractSolution(double[,] table, List<int> basis, bool maximize)
        {
            int m = table.GetLength(0) - 1;
            int n = table.GetLength(1) - 1;

            double[] solution = new double[n];
            Array.Fill(solution, 0);

            for (int i = 0; i < m; i++)
            {
                int varIndex = basis[i];
                if (varIndex < n)
                {
                    solution[varIndex] = table[i, n];
                }
            }

            double objectiveValue = table[m, n];
            if (maximize)
            {
                objectiveValue = -objectiveValue;
            }

            return new SolutionResult
            {
                Solution = solution,
                ObjectiveValue = objectiveValue,
                IsOptimal = true,
                IsFeasible = true,
                Message = "Оптимальное решение найдено"
            };
        }
    }
}
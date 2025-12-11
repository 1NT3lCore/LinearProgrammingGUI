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
        private int totalVars;
        private int artificialCount;
        private int slackCount;
        private StringBuilder log;
        private bool useTwoPhase = false;

        public SimplexSolver(LPTask lpTask)
        {
            this.task = lpTask;
            this.log = new StringBuilder();

            // Проверяем корректность задачи
            if (task.n == 0 || task.m == 0)
            {
                log.AppendLine("❌ Ошибка: задача некорректна (нулевая размерность)");
                return;
            }

            // Анализируем задачу
            useTwoPhase = task.signs.Any(s => s == "=") || task.signs.Any(s => s == ">=");

            if (useTwoPhase)
            {
                log.AppendLine("⚠️ Используем ДВУХФАЗНЫЙ симплекс-метод.");
                SolveTwoPhase();
            }
            else
            {
                log.AppendLine("✅ Используем ОДНОФАЗНЫЙ симплекс-метод.");
                SolveSinglePhase();
            }
        }

        private void SolveTwoPhase()
        {
            log.AppendLine("\n═══════════════════════════════════════════════════");
            log.AppendLine("ФАЗА I: Поиск допустимого решения");
            log.AppendLine("═══════════════════════════════════════════════════");

            // Шаг 1: Строим задачу для фазы I
            var phase1Data = BuildPhase1Problem();

            if (phase1Data.table == null)
            {
                log.AppendLine("❌ Ошибка построения задачи фазы I");
                return;
            }

            // Шаг 2: Решаем фазу I
            bool phase1Result = SolvePhase1(phase1Data.table, phase1Data.basis);

            if (!phase1Result)
            {
                log.AppendLine("\n❌ Задача не имеет допустимого решения!");
                return;
            }

            log.AppendLine("\n═══════════════════════════════════════════════════");
            log.AppendLine("ФАЗА II: Оптимизация целевой функции");
            log.AppendLine("═══════════════════════════════════════════════════");

            // Шаг 3: Строим задачу для фазы II
            var phase2Data = BuildPhase2Problem();

            if (phase2Data.table == null)
            {
                log.AppendLine("❌ Ошибка построения задачи фазы II");
                return;
            }

            // Шаг 4: Решаем фазу II
            SolvePhase2(phase2Data.table, phase2Data.basis);
        }

        private (double[,] table, List<int> basis) BuildPhase1Problem()
        {
            try
            {
                // Подсчитываем вспомогательные переменные
                CountAuxiliaryVariables();
                totalVars = task.n + slackCount + artificialCount;

                // Проверяем корректность
                if (totalVars <= 0)
                {
                    log.AppendLine("❌ Ошибка: некорректное количество переменных");
                    return (null, null);
                }

                // Строим начальную таблицу
                rows = task.m + 1;
                cols = totalVars + 1;
                double[,] table = new double[rows, cols];
                List<int> basis = new List<int>();

                int slackIndex = 0;
                int artificialIndex = 0;

                // Заполняем таблицу ограничений
                for (int i = 0; i < task.m; i++)
                {
                    // Основные переменные
                    for (int j = 0; j < task.n; j++)
                    {
                        table[i + 1, j] = task.A[i, j];
                    }

                    // Правая часть
                    table[i + 1, cols - 1] = task.b[i];

                    // Slack и искусственные переменные
                    if (task.signs[i] == "<=")
                    {
                        int slackPos = task.n + slackIndex;
                        table[i + 1, slackPos] = 1;
                        basis.Add(slackPos);
                        slackIndex++;
                    }
                    else if (task.signs[i] == ">=")
                    {
                        int slackPos = task.n + slackIndex;
                        int artPos = task.n + slackCount + artificialIndex;

                        table[i + 1, slackPos] = -1;
                        table[i + 1, artPos] = 1;
                        basis.Add(artPos);

                        slackIndex++;
                        artificialIndex++;
                    }
                    else if (task.signs[i] == "=")
                    {
                        int artPos = task.n + slackCount + artificialIndex;
                        table[i + 1, artPos] = 1;
                        basis.Add(artPos);
                        artificialIndex++;
                    }
                }

                // Целевая строка фазы I: W = -ΣR (минимизация суммы искусственных переменных)
                for (int j = 0; j < totalVars; j++)
                {
                    table[0, j] = 0;
                }
                for (int i = task.n + slackCount; i < totalVars; i++)
                {
                    table[0, i] = 1; // Коэффициент 1 для искусственных переменных
                }
                table[0, cols - 1] = 0;

                // Преобразуем целевую функцию для исключения искусственных переменных из базиса
                for (int i = 0; i < basis.Count; i++)
                {
                    int basisVar = basis[i];
                    if (basisVar >= task.n + slackCount) // Искусственная переменная
                    {
                        double coeff = table[0, basisVar];
                        for (int j = 0; j < cols; j++)
                        {
                            table[0, j] -= coeff * table[i + 1, j];
                        }
                    }
                }

                log.AppendLine($"\nПеременные: {totalVars} (исходные: {task.n}, slack: {slackCount}, искусственные: {artificialCount})");
                log.AppendLine("Начальная таблица фазы I:");
                log.AppendLine(GetTableString(table, basis));

                return (table, basis);
            }
            catch (Exception ex)
            {
                log.AppendLine($"❌ Ошибка в BuildPhase1Problem: {ex.Message}");
                return (null, null);
            }
        }

        private bool SolvePhase1(double[,] table, List<int> basis)
        {
            if (table == null || basis == null) return false;

            int iteration = 0;
            int maxIterations = 100;

            while (iteration < maxIterations)
            {
                iteration++;
                stepCount++;

                log.AppendLine($"\n═══════════════════════════════════════════════════");
                log.AppendLine($"ФАЗА I - ИТЕРАЦИЯ {iteration}:");
                log.AppendLine($"═══════════════════════════════════════════════════");

                log.AppendLine("\nТекущая таблица:");
                log.AppendLine(GetTableString(table, basis));

                // Проверяем, остались ли искусственные переменные в базисе
                bool hasArtificialInBasis = false;
                for (int i = 0; i < basis.Count; i++)
                {
                    if (basis[i] >= task.n + slackCount)
                    {
                        hasArtificialInBasis = true;
                        break;
                    }
                }

                if (!hasArtificialInBasis)
                {
                    log.AppendLine("\n✅ Все искусственные переменные исключены из базиса!");
                    double wValue = table[0, cols - 1];

                    if (Math.Abs(wValue) > 0.0001)
                    {
                        log.AppendLine($"❌ W = {wValue:F3} ≠ 0");
                        log.AppendLine("Задача не имеет допустимого решения!");
                        return false;
                    }

                    log.AppendLine($"✅ W = {wValue:F3} = 0");
                    log.AppendLine("Допустимое решение найдено!");

                    this.table = table;
                    this.basis = basis;
                    return true;
                }

                // Находим первую искусственную переменную в базисе
                int pivotRow = -1;
                int artificialVar = -1;
                for (int i = 0; i < basis.Count; i++)
                {
                    if (basis[i] >= task.n + slackCount)
                    {
                        pivotRow = i + 1;
                        artificialVar = basis[i];
                        break;
                    }
                }

                if (pivotRow == -1)
                {
                    log.AppendLine("❌ Не удалось найти искусственную переменную в базисе");
                    return false;
                }

                log.AppendLine($"\nИскусственная переменная в базисе: {GetVariableName(artificialVar)}");

                // Ищем переменную для ввода в базис вместо искусственной
                int pivotCol = -1;
                for (int j = 0; j < task.n + slackCount; j++) // Только не искусственные переменные
                {
                    if (Math.Abs(table[pivotRow, j]) > 0.0001 && !basis.Contains(j))
                    {
                        pivotCol = j;
                        break;
                    }
                }

                if (pivotCol == -1)
                {
                    // Проверяем, равна ли искусственная переменная 0
                    if (Math.Abs(table[pivotRow, cols - 1]) < 0.0001)
                    {
                        log.AppendLine($"✅ Искусственная переменная {GetVariableName(artificialVar)} = 0");
                        log.AppendLine("Можно удалить эту строку");

                        // Удаляем искусственную переменную из базиса
                        // Ищем любую небазисную переменную
                        for (int j = 0; j < task.n + slackCount; j++)
                        {
                            if (!basis.Contains(j))
                            {
                                basis[pivotRow - 1] = j;
                                break;
                            }
                        }
                        continue;
                    }

                    log.AppendLine($"❌ Не удалось найти замену для искусственной переменной");
                    return false;
                }

                log.AppendLine($"Вводим в базис: {GetVariableName(pivotCol)}");
                log.AppendLine($"Выводим из базиса: {GetVariableName(artificialVar)}");
                log.AppendLine($"Разрешающий элемент: a[{pivotRow},{pivotCol + 1}] = {table[pivotRow, pivotCol]:F3}");

                // Обновляем базис
                basis[pivotRow - 1] = pivotCol;

                // Жордановы преобразования
                double pivot = table[pivotRow, pivotCol];
                for (int j = 0; j < cols; j++)
                {
                    table[pivotRow, j] /= pivot;
                }

                for (int i = 0; i < rows; i++)
                {
                    if (i != pivotRow)
                    {
                        double factor = table[i, pivotCol];
                        if (Math.Abs(factor) > 0.0001)
                        {
                            for (int j = 0; j < cols; j++)
                            {
                                table[i, j] -= factor * table[pivotRow, j];
                            }
                        }
                    }
                }
            }

            log.AppendLine("❌ Превышено число итераций в фазе I!");
            return false;
        }

        private (double[,] table, List<int> basis) BuildPhase2Problem()
        {
            try
            {
                if (this.table == null || this.basis == null)
                {
                    log.AppendLine("❌ Ошибка: нет результатов фазы I");
                    return (null, null);
                }

                // Создаем таблицу для фазы II без искусственных переменных
                int phase2Vars = task.n + slackCount;

                double[,] table = new double[rows, phase2Vars + 1];
                List<int> basis = new List<int>();

                // Копируем строки ограничений (без искусственных переменных)
                for (int i = 1; i < rows; i++)
                {
                    for (int j = 0; j < phase2Vars; j++)
                    {
                        table[i, j] = this.table[i, j];
                    }
                    table[i, phase2Vars] = this.table[i, this.cols - 1];

                    // Копируем базисные переменные (только не искусственные)
                    int basisVar = this.basis[i - 1];
                    if (basisVar < phase2Vars) // Если не искусственная переменная
                    {
                        basis.Add(basisVar);
                    }
                }

                // Целевая функция фазы II
                // Для максимизации: Z = -cᵢxᵢ
                // Для минимизации: Z = cᵢxᵢ
                for (int j = 0; j < phase2Vars; j++)
                {
                    table[0, j] = 0;
                }

                if (task.taskType == 1) // Максимизация
                {
                    for (int i = 0; i < task.n; i++)
                    {
                        table[0, i] = -task.c[i]; // Для максимизации берем с отрицательным знаком
                    }
                }
                else // Минимизация
                {
                    for (int i = 0; i < task.n; i++)
                    {
                        table[0, i] = task.c[i]; // Для минимизации берем с положительным знаком
                    }
                }
                table[0, phase2Vars] = 0;

                // Преобразуем целевую функцию для текущего базиса
                for (int i = 0; i < basis.Count && i + 1 < rows; i++)
                {
                    int basisVar = basis[i];
                    double coeff = table[0, basisVar];
                    if (Math.Abs(coeff) > 0.0001)
                    {
                        for (int j = 0; j <= phase2Vars; j++)
                        {
                            table[0, j] -= coeff * table[i + 1, j];
                        }
                    }
                }

                log.AppendLine($"\nФаза II: {phase2Vars} переменных");
                log.AppendLine("Начальная таблица фазы II:");
                log.AppendLine(GetTableString(table, basis));

                return (table, basis);
            }
            catch (Exception ex)
            {
                log.AppendLine($"❌ Ошибка в BuildPhase2Problem: {ex.Message}");
                return (null, null);
            }
        }

        private void SolvePhase2(double[,] table, List<int> basis)
        {
            if (table == null || basis == null) return;

            int iteration = 0;
            int maxIterations = 100;

            while (iteration < maxIterations)
            {
                iteration++;

                log.AppendLine($"\n═══════════════════════════════════════════════════");
                log.AppendLine($"ФАЗА II - ИТЕРАЦИЯ {iteration}:");
                log.AppendLine($"═══════════════════════════════════════════════════");

                log.AppendLine("\nТекущая таблица:");
                log.AppendLine(GetTableString(table, basis));

                // Проверка оптимальности
                bool optimal = true;
                int pivotCol = -1;
                double minDelta = 0;

                log.Append("\nОценки: ");
                int tableCols = table.GetLength(1);
                for (int j = 0; j < tableCols - 1; j++)
                {
                    string sign = table[0, j] >= 0 ? "+" : "";
                    log.Append($"Δ{j + 1}={sign}{table[0, j]:F3} ");

                    if (table[0, j] < minDelta - 0.0001)
                    {
                        optimal = false;
                        minDelta = table[0, j];
                        pivotCol = j;
                    }
                }

                if (optimal)
                {
                    // Вычисляем значение целевой функции с правильным знаком
                    double objectiveValue = -table[0, tableCols - 1]; // Инвертируем знак

                    log.AppendLine($"\n\n✅ Оптимальное решение найдено!");
                    log.AppendLine($"Значение целевой функции: {objectiveValue:F6}");

                    this.table = table;
                    this.basis = basis;
                    this.cols = tableCols;
                    this.rows = table.GetLength(0);
                    break;
                }

                log.AppendLine($"\nВводим в базис: {GetVariableName(pivotCol)} (Δ={minDelta:F3})");

                // Выбор выводимой переменной
                int pivotRow = -1;
                double minRatio = double.MaxValue;
                int tableRows = table.GetLength(0);

                log.AppendLine("\nОтношения bᵢ/aᵢⱼ:");
                for (int i = 1; i < tableRows; i++)
                {
                    if (table[i, pivotCol] > 0.0001)
                    {
                        double ratio = table[i, tableCols - 1] / table[i, pivotCol];
                        log.AppendLine($"  Строка {i}: {table[i, tableCols - 1]:F3}/{table[i, pivotCol]:F3} = {ratio:F3}");

                        if (ratio < minRatio - 0.0001 && ratio >= 0)
                        {
                            minRatio = ratio;
                            pivotRow = i;
                        }
                    }
                    else
                    {
                        log.AppendLine($"  Строка {i}: aᵢⱼ ≤ 0 - не подходит");
                    }
                }

                if (pivotRow == -1)
                {
                    log.AppendLine("\n❌ Задача неограничена!");
                    return;
                }

                if (pivotRow - 1 >= basis.Count)
                {
                    log.AppendLine("\n❌ Ошибка: неверный индекс базисной переменной");
                    return;
                }

                log.AppendLine($"\nВыводим из базиса: {GetVariableName(basis[pivotRow - 1])}");
                log.AppendLine($"Разрешающий элемент: a[{pivotRow},{pivotCol + 1}] = {table[pivotRow, pivotCol]:F3}");

                // Обновляем базис
                basis[pivotRow - 1] = pivotCol;

                // Жордановы преобразования
                double pivot = table[pivotRow, pivotCol];
                for (int j = 0; j < tableCols; j++)
                {
                    table[pivotRow, j] /= pivot;
                }

                for (int i = 0; i < tableRows; i++)
                {
                    if (i != pivotRow)
                    {
                        double factor = table[i, pivotCol];
                        if (Math.Abs(factor) > 0.0001)
                        {
                            for (int j = 0; j < tableCols; j++)
                            {
                                table[i, j] -= factor * table[pivotRow, j];
                            }
                        }
                    }
                }
            }

            if (iteration >= maxIterations)
            {
                log.AppendLine("❌ Превышено число итераций в фазе II!");
            }
        }

        private void SolveSinglePhase()
        {
            log.AppendLine("\n═══════════════════════════════════════════════════");
            log.AppendLine("ОДНОФАЗНЫЙ СИМПЛЕКС-МЕТОД");
            log.AppendLine("═══════════════════════════════════════════════════");

            var data = BuildSinglePhaseProblem();
            if (data.table != null && data.basis != null)
            {
                SolvePhase2(data.table, data.basis);
            }
        }

        private (double[,] table, List<int> basis) BuildSinglePhaseProblem()
        {
            try
            {
                slackCount = task.signs.Count(s => s == "<=");
                totalVars = task.n + slackCount;

                rows = task.m + 1;
                cols = totalVars + 1;
                double[,] table = new double[rows, cols];
                List<int> basis = new List<int>();

                int slackIndex = 0;

                for (int i = 0; i < task.m; i++)
                {
                    for (int j = 0; j < task.n; j++)
                    {
                        table[i + 1, j] = task.A[i, j];
                    }

                    table[i + 1, cols - 1] = task.b[i];

                    if (task.signs[i] == "<=")
                    {
                        int slackPos = task.n + slackIndex;
                        table[i + 1, slackPos] = 1;
                        basis.Add(slackPos);
                        slackIndex++;
                    }
                }

                // Целевая функция
                for (int j = 0; j < totalVars; j++)
                {
                    table[0, j] = 0;
                }

                if (task.taskType == 1) // Максимизация
                {
                    for (int i = 0; i < task.n; i++)
                    {
                        table[0, i] = -task.c[i];
                    }
                }
                else // Минимизация
                {
                    for (int i = 0; i < task.n; i++)
                    {
                        table[0, i] = task.c[i];
                    }
                }
                table[0, cols - 1] = 0;

                // Преобразуем целевую функцию
                for (int i = 0; i < basis.Count; i++)
                {
                    int basisVar = basis[i];
                    double coeff = table[0, basisVar];
                    if (Math.Abs(coeff) > 0.0001)
                    {
                        for (int j = 0; j < cols; j++)
                        {
                            table[0, j] -= coeff * table[i + 1, j];
                        }
                    }
                }

                log.AppendLine($"\nПеременные: {totalVars} (исходные: {task.n}, slack: {slackCount})");
                log.AppendLine("Начальная таблица:");
                log.AppendLine(GetTableString(table, basis));

                return (table, basis);
            }
            catch (Exception ex)
            {
                log.AppendLine($"❌ Ошибка в BuildSinglePhaseProblem: {ex.Message}");
                return (null, null);
            }
        }

        private void CountAuxiliaryVariables()
        {
            slackCount = 0;
            artificialCount = 0;

            foreach (string sign in task.signs)
            {
                if (sign == "<=")
                    slackCount++;
                else if (sign == ">=")
                {
                    slackCount++;
                    artificialCount++;
                }
                else if (sign == "=")
                    artificialCount++;
            }
        }

        private string GetTableString(double[,] table, List<int> basis)
        {
            if (table == null) return "❌ Таблица не создана";

            StringBuilder sb = new StringBuilder();
            int vars = table.GetLength(1) - 1;
            int tabRows = table.GetLength(0);

            sb.Append("Базис\t");
            for (int j = 0; j < vars; j++)
            {
                sb.Append($"{GetVariableName(j),-10}");
            }
            sb.AppendLine("Решение");

            sb.Append(new string('─', 8 + vars * 11 + 12));
            sb.AppendLine();

            for (int i = 0; i < tabRows; i++)
            {
                if (i == 0)
                    sb.Append("Z\t");
                else if (i - 1 < basis.Count)
                    sb.Append($"{GetVariableName(basis[i - 1])}\t");
                else
                    sb.Append("-\t");

                for (int j = 0; j < table.GetLength(1); j++)
                {
                    string valueStr;
                    if (j == vars && i > 0)
                        valueStr = $"{table[i, j]:F6}";
                    else if (i == 0 && j < vars)
                        valueStr = $"{table[i, j]:+0.000;-0.000;0.000}";
                    else
                        valueStr = $"{table[i, j]:0.000}";

                    sb.Append($"{valueStr,-10}");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string GetVariableName(int index)
        {
            if (index < 0) return "-";

            if (index < task.n)
                return $"x{index + 1}";
            else if (index < task.n + slackCount)
                return $"s{index - task.n + 1}";
            else if (index < totalVars)
                return $"R{index - (task.n + slackCount) + 1}";
            else
                return $"v{index + 1}";
        }

        public string SolveWithSteps()
        {
            return log.ToString() + GetFinalSolution();
        }

        private string GetFinalSolution()
        {
            if (table == null || basis == null)
                return "\n❌ Решение не найдено!";

            try
            {
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("\n═══════════════════════════════════════════════════");
                sb.AppendLine("ФИНАЛЬНОЕ РЕШЕНИЕ:");
                sb.AppendLine("═══════════════════════════════════════════════════");

                double[] solution = new double[task.n];

                for (int i = 0; i < basis.Count && i + 1 < rows; i++)
                {
                    int basisVar = basis[i];
                    if (basisVar >= 0 && basisVar < task.n)
                    {
                        solution[basisVar] = table[i + 1, cols - 1];
                    }
                }

                sb.AppendLine("\nЗначения переменных:");
                for (int i = 0; i < task.n; i++)
                {
                    sb.AppendLine($"  x{i + 1} = {solution[i]:F6}");
                }

                double objectiveValue = 0;
                for (int i = 0; i < task.n; i++)
                {
                    objectiveValue += task.c[i] * solution[i];
                }

                sb.AppendLine($"\nЗначение целевой функции:");
                sb.AppendLine($"  F(X) = {objectiveValue:F6}");

                if (task.taskType == 2) // Минимизация
                {
                    sb.AppendLine($"  (задача на минимизацию)");
                }

                sb.AppendLine("\nБазисные переменные:");
                for (int i = 0; i < basis.Count && i + 1 < rows; i++)
                {
                    if (basis[i] >= 0)
                    {
                        sb.AppendLine($"  {GetVariableName(basis[i])} = {table[i + 1, cols - 1]:F6}");
                    }
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return $"\n❌ Ошибка при выводе решения: {ex.Message}";
            }
        }
    }
}
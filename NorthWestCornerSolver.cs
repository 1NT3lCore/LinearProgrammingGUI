using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinearProgrammingGUI
{
    public class NorthWestCornerSolver
    {
        private double[,] costs;
        private double[] supply;
        private double[] demand;
        private double[,] solution;
        private int stepCount = 0;

        public NorthWestCornerSolver(double[,] costMatrix, double[] supplyArray, double[] demandArray)
        {
            costs = costMatrix;
            supply = supplyArray;
            demand = demandArray;
            solution = new double[supply.Length, demand.Length];
        }

        public string SolveWithOutput()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════");
            sb.AppendLine("РЕШЕНИЕ ТРАНСПОРТНОЙ ЗАДАЧИ - МЕТОД СЕВЕРО-ЗАПАДНОГО УГЛА");
            sb.AppendLine("═══════════════════════════════════════════════════");

            // Проверка сбалансированности
            double totalSupply = supply.Sum();
            double totalDemand = demand.Sum();

            sb.AppendLine($"\nСумма запасов: {totalSupply}");
            sb.AppendLine($"Сумма потребностей: {totalDemand}");

            if (Math.Abs(totalSupply - totalDemand) > 0.0001)
            {
                sb.AppendLine("ОШИБКА: Задача не сбалансирована! Невозможно решить.");
                return sb.ToString();
            }

            sb.AppendLine("УСПЕХ: Условие баланса соблюдается. Запасы равны потребностям.");

            ShowInputData(sb);

            double[] remainingSupply = supply.ToArray();
            double[] remainingDemand = demand.ToArray();

            sb.AppendLine("\n═══════════════════════════════════════════════════");
            sb.AppendLine("ПОСТРОЕНИЕ ОПОРНОГО ПЛАНА");
            sb.AppendLine("═══════════════════════════════════════════════════");

            int i = 0, j = 0;

            while (i < supply.Length && j < demand.Length)
            {
                stepCount++;
                sb.AppendLine($"\nШАГ {stepCount}:");
                sb.AppendLine($"  Рассматриваем ячейку [S{i + 1}, П{j + 1}]");

                double allocation = Math.Min(remainingSupply[i], remainingDemand[j]);
                solution[i, j] = allocation;

                sb.AppendLine($"  Остаток у поставщика {i + 1}: {remainingSupply[i]}");
                sb.AppendLine($"  Потребность потребителя {j + 1}: {remainingDemand[j]}");
                sb.AppendLine($"  Выделяем: min({remainingSupply[i]}, {remainingDemand[j]}) = {allocation}");

                // Обновляем остатки
                remainingSupply[i] -= allocation;
                remainingDemand[j] -= allocation;

                sb.AppendLine($"  Новый остаток у поставщика {i + 1}: {remainingSupply[i]}");
                sb.AppendLine($"  Новая потребность потребителя {j + 1}: {remainingDemand[j]}");

                // Переходим к следующей ячейке согласно правилу СЗУ
                if (remainingSupply[i] == 0)
                {
                    sb.AppendLine($"  ЗАВЕРШЕНО: Запас поставщика {i + 1} исчерпан -> переходим к поставщику {i + 2}");
                    i++;
                }
                else
                {
                    sb.AppendLine($"  ЗАВЕРШЕНО: Потребность потребителя {j + 1} удовлетворена -> переходим к потребителю {j + 2}");
                    j++;
                }

                ShowCurrentSolution(sb, remainingSupply, remainingDemand);
            }

            // Проверка на вырожденность
            int occupiedCells = CountOccupiedCells();
            int requiredCells = supply.Length + demand.Length - 1;

            sb.AppendLine($"\nЧисло занятых клеток: {occupiedCells}");
            sb.AppendLine($"Должно быть: m + n - 1 = {supply.Length} + {demand.Length} - 1 = {requiredCells}");

            if (occupiedCells < requiredCells)
            {
                sb.AppendLine("ВЫРОЖДЕННОСТЬ: Опорный план является вырожденным");
                sb.AppendLine("ДЕЙСТВИЕ: Требуется добавить нулевые поставки в недостающие клетки");
                AddZeroAllocations();
            }
            else
            {
                sb.AppendLine("УСПЕХ: Опорный план является невырожденным");
            }

            ShowFinalSolution(sb);

            return sb.ToString();
        }

        private int CountOccupiedCells()
        {
            int count = 0;
            for (int i = 0; i < supply.Length; i++)
            {
                for (int j = 0; j < demand.Length; j++)
                {
                    if (solution[i, j] > 0)
                        count++;
                }
            }
            return count;
        }

        private void AddZeroAllocations()
        {
            int requiredCells = supply.Length + demand.Length - 1;
            int currentCells = CountOccupiedCells();

            for (int i = 0; i < supply.Length && currentCells < requiredCells; i++)
            {
                for (int j = 0; j < demand.Length && currentCells < requiredCells; j++)
                {
                    if (solution[i, j] == 0)
                    {
                        solution[i, j] = 0;
                        currentCells++;
                        break;
                    }
                }
            }
        }

        private void ShowInputData(StringBuilder sb)
        {
            sb.AppendLine("\n═══════════════════════════════════════════════════");
            sb.AppendLine("ВВЕДЕННЫЕ ДАННЫЕ:");
            sb.AppendLine("═══════════════════════════════════════════════════");

            sb.AppendLine("\nЗапасы поставщиков:");
            for (int i = 0; i < supply.Length; i++)
            {
                sb.AppendLine($"  S{i + 1}: {supply[i]}");
            }

            sb.AppendLine("\nПотребности потребителей:");
            for (int j = 0; j < demand.Length; j++)
            {
                sb.AppendLine($"  П{j + 1}: {demand[j]}");
            }

            sb.AppendLine("\nМатрица стоимостей:");
            sb.Append("        ");
            for (int j = 0; j < demand.Length; j++)
            {
                sb.Append($"П{j + 1}      ");
            }
            sb.AppendLine();

            for (int i = 0; i < supply.Length; i++)
            {
                sb.Append($"S{i + 1}    ");
                for (int j = 0; j < demand.Length; j++)
                {
                    sb.Append($"{costs[i, j],-6} ");
                }
                sb.AppendLine();
            }
        }

        private void ShowCurrentSolution(StringBuilder sb, double[] remSupply, double[] remDemand)
        {
            sb.AppendLine("\nТЕКУЩАЯ ТАБЛИЦА РАСПРЕДЕЛЕНИЯ:");

            // Заголовок
            sb.Append("        ");
            for (int j = 0; j < demand.Length; j++)
            {
                sb.Append($"П{j + 1}      ");
            }
            sb.AppendLine("Запасы");

            // Данные
            for (int i = 0; i < supply.Length; i++)
            {
                sb.Append($"S{i + 1}    ");
                for (int j = 0; j < demand.Length; j++)
                {
                    if (solution[i, j] > 0)
                        sb.Append($"{solution[i, j],-6} ");
                    else
                        sb.Append($"-      ");
                }
                sb.AppendLine($"{supply[i]}");
            }

            // Потребности
            sb.Append("Потр.  ");
            for (int j = 0; j < demand.Length; j++)
            {
                sb.Append($"{remDemand[j],-6} ");
            }
            sb.AppendLine();
        }

        private void ShowFinalSolution(StringBuilder sb)
        {
            sb.AppendLine("\n═══════════════════════════════════════════════════");
            sb.AppendLine("ФИНАЛЬНЫЙ ОПОРНЫЙ ПЛАН");
            sb.AppendLine("═══════════════════════════════════════════════════");

            double totalCost = 0;

            sb.AppendLine("\nРАСПРЕДЕЛИТЕЛЬНАЯ ТАБЛИЦА:");

            // Заголовок
            sb.Append("        ");
            for (int j = 0; j < demand.Length; j++)
            {
                sb.Append($"П{j + 1}      ");
            }
            sb.AppendLine("Запасы");

            // Данные
            for (int i = 0; i < supply.Length; i++)
            {
                sb.Append($"S{i + 1}    ");
                for (int j = 0; j < demand.Length; j++)
                {
                    if (solution[i, j] > 0)
                    {
                        sb.Append($"{solution[i, j]}[{costs[i, j]}]  ");
                        totalCost += solution[i, j] * costs[i, j];
                    }
                    else
                    {
                        sb.Append($"-       ");
                    }
                }
                sb.AppendLine($"{supply[i]}");
            }

            // Потребности
            sb.Append("Потр.  ");
            for (int j = 0; j < demand.Length; j++)
            {
                sb.Append($"{demand[j],-6} ");
            }
            sb.AppendLine();

            sb.AppendLine($"\nОБЩАЯ СТОИМОСТЬ ПЕРЕВОЗОК: {totalCost:F2}");

            // Детализация стоимости
            sb.AppendLine("\nДЕТАЛИЗАЦИЯ РАСЧЕТА СТОИМОСТИ:");
            for (int i = 0; i < supply.Length; i++)
            {
                for (int j = 0; j < demand.Length; j++)
                {
                    if (solution[i, j] > 0)
                    {
                        double cost = solution[i, j] * costs[i, j];
                        sb.AppendLine($"  S{i + 1} -> П{j + 1}: {solution[i, j]} × {costs[i, j]} = {cost:F2}");
                    }
                }
            }

            sb.AppendLine($"\nИТОГО: {totalCost:F2}");
        }

        public double GetTotalCost()
        {
            double totalCost = 0;
            for (int i = 0; i < supply.Length; i++)
            {
                for (int j = 0; j < demand.Length; j++)
                {
                    totalCost += solution[i, j] * costs[i, j];
                }
            }
            return totalCost;
        }
    }
}

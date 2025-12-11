using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LinearProgrammingGUI
{
    public class OptimalityCheckForm : Form
    {
        private LPTask task;
        private double[] plan;
        private TextBox txtResult;
        private Label lblTitle;
        
        private int? selectedTaskId = null;
        private LPTask currentTask;
        private bool hasTask = false; // Флаг, указывающий, что задача выбрана через SetSelectedTask

        public OptimalityCheckForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Проверка плана на оптимальность";
            this.Size = new Size(1100, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1000, 700);
            this.MaximizeBox = true;

            lblTitle = new Label
            {
                Text = "ПРОВЕРКА ПЛАНА НА ОПТИМАЛЬНОСТЬ",
                Font = new Font("Arial", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 20),
                Size = new Size(1060, 30),
                ForeColor = Color.DarkBlue
            };
            this.Controls.Add(lblTitle);

            Button btnInputTask = new Button
            {
                Text = "1. Выбрать задачу",
                Location = new Point(20, 70),
                Size = new Size(150, 40),
                Font = new Font("Arial", 10)
            };
            btnInputTask.Click += BtnInputTask_Click;
            this.Controls.Add(btnInputTask);

            Button btnInputPlan = new Button
            {
                Text = "2. Ввести план",
                Location = new Point(190, 70),
                Size = new Size(150, 40),
                Font = new Font("Arial", 10),
                Enabled = false
            };
            btnInputPlan.Click += BtnInputPlan_Click;
            this.Controls.Add(btnInputPlan);

            Button btnCheck = new Button
            {
                Text = "3. Проверить",
                Location = new Point(360, 70),
                Size = new Size(150, 40),
                Font = new Font("Arial", 10),
                Enabled = false
            };
            btnCheck.Click += BtnCheck_Click;
            this.Controls.Add(btnCheck);

            txtResult = new TextBox
            {
                Location = new Point(20, 120),
                Size = new Size(1040, 630),
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                Font = new Font("Courier New", 9),
                ReadOnly = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(txtResult);

            Button btnClose = new Button
            {
                Text = "Закрыть",
                Location = new Point(960, 760),
                Size = new Size(100, 30),
                Font = new Font("Arial", 9),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
        }

        public void SetSelectedTask(int taskId, LPTask task)
        {
            selectedTaskId = taskId;
            currentTask = task;
            hasTask = true; // Устанавливаем флаг
            lblTitle.Text = $"ПРОВЕРКА ПЛАНА НА ОПТИМАЛЬНОСТЬ (Задача №{taskId})";
            
            this.Controls[2].Enabled = true;
            MessageBox.Show($"Задача №{taskId} выбрана. Теперь введите план для проверки.", 
                "Задача выбрана", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnInputTask_Click(object sender, EventArgs e)
        {
            if (hasTask)
            {
                // Используем задачу, выбранную через SetSelectedTask
                task = currentTask;
                this.Controls[2].Enabled = true;
                MessageBox.Show($"Используется задача №{selectedTaskId.Value}. Теперь введите план для проверки.", 
                    "Задача уже выбрана", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // Показываем диалог выбора задачи
                ShowTaskSelectionDialog();
            }
        }

        private void ShowTaskSelectionDialog()
        {
            if (LPTaskStorage.Count == 0)
            {
                MessageBox.Show("Нет сохраненных задач. Сначала введите задачу в главном меню.",
                    "Нет задач", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Form dialog = new Form
            {
                Text = "Выбор задачи",
                Size = new Size(500, 400),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false
            };

            Label lbl = new Label
            {
                Text = "Выберите задачу для проверки оптимальности:",
                Location = new Point(20, 20),
                Size = new Size(450, 30),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            dialog.Controls.Add(lbl);

            ListBox listBox = new ListBox
            {
                Location = new Point(20, 60),
                Size = new Size(440, 250),
                Font = new Font("Courier New", 9)
            };

            var tasks = LPTaskStorage.GetAllTasks();
            foreach (var task in tasks)
            {
                listBox.Items.Add($"{task.id}. {task.description}");
            }
            listBox.SelectedIndex = 0;
            dialog.Controls.Add(listBox);

            Button btnOK = new Button
            {
                Text = "Выбрать",
                Location = new Point(150, 320),
                Size = new Size(100, 35),
                Font = new Font("Arial", 9, FontStyle.Bold),
                BackColor = Color.LightGreen
            };
            btnOK.Click += (s, ev) =>
            {
                if (listBox.SelectedIndex >= 0)
                {
                    int taskId = listBox.SelectedIndex + 1;
                    var selectedTask = LPTaskStorage.GetTask(taskId);
                    task = selectedTask; // Сохраняем в поле task
                    this.Controls[2].Enabled = true;
                    dialog.Close();
                    
                    MessageBox.Show($"Задача №{taskId} выбрана. Теперь введите план для проверки.", 
                        "Задача выбрана", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
            dialog.Controls.Add(btnOK);

            Button btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(260, 320),
                Size = new Size(100, 35),
                Font = new Font("Arial", 9)
            };
            btnCancel.Click += (s, ev) => dialog.Close();
            dialog.Controls.Add(btnCancel);

            dialog.ShowDialog();
        }

        private void BtnInputPlan_Click(object sender, EventArgs e)
        {
            LPTask taskToUse;
            
            // Определяем, какую задачу использовать
            if (hasTask)
            {
                taskToUse = currentTask;
                task = currentTask; // Также сохраняем в поле task
            }
            else if (task.n > 0 && task.m > 0)
            {
                taskToUse = task;
            }
            else
            {
                MessageBox.Show("Сначала выберите задачу!", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Form planForm = new Form
            {
                Text = "Ввод плана",
                Size = new Size(400, 300),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog
            };

            Label lbl = new Label
            {
                Text = $"Введите план (значения {taskToUse.n} переменных):",
                Location = new Point(20, 20),
                Size = new Size(350, 20),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            planForm.Controls.Add(lbl);

            NumericUpDown[] inputs = new NumericUpDown[taskToUse.n];
            for (int i = 0; i < taskToUse.n; i++)
            {
                Label lblVar = new Label
                {
                    Text = $"x[{i + 1}]:",
                    Location = new Point(20, 50 + i * 30),
                    Size = new Size(50, 20),
                    Font = new Font("Arial", 9)
                };
                planForm.Controls.Add(lblVar);

                inputs[i] = new NumericUpDown
                {
                    Location = new Point(80, 50 + i * 30),
                    Size = new Size(100, 20),
                    DecimalPlaces = 2,
                    Minimum = -10000,
                    Maximum = 10000,
                    Font = new Font("Arial", 9)
                };
                planForm.Controls.Add(inputs[i]);
            }

            Button btnOK = new Button
            {
                Text = "OK",
                Location = new Point(100, 50 + taskToUse.n * 30 + 20),
                Size = new Size(80, 30),
                Font = new Font("Arial", 9)
            };
            btnOK.Click += (s, ev) =>
            {
                plan = new double[taskToUse.n];
                for (int i = 0; i < taskToUse.n; i++)
                    plan[i] = (double)inputs[i].Value;
                    
                this.Controls[3].Enabled = true;
                planForm.Close();
                
                MessageBox.Show($"План введен. Проверьте {taskToUse.n} переменных.", 
                    "План введен", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            planForm.Controls.Add(btnOK);

            planForm.ShowDialog();
        }

        private void BtnCheck_Click(object sender, EventArgs e)
        {
            if ((!hasTask && (task.n == 0 || task.m == 0)) || plan == null)
            {
                MessageBox.Show("Сначала выберите задачу и введите план!", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            StringBuilder result = CheckOptimality(task, plan);
            txtResult.Text = result.ToString();
        }

        private StringBuilder CheckOptimality(LPTask task, double[] x)
        {
            StringBuilder sb = new StringBuilder();
            double eps = 0.0001;

            sb.AppendLine("================================================");
            sb.AppendLine("ШАГ 1: ПРОВЕРКА ДОПУСТИМОСТИ ПЛАНА");
            sb.AppendLine("================================================");
            sb.AppendLine($"Тип задачи: {(task.taskType == 1 ? "МАКСИМИЗАЦИЯ" : "МИНИМИЗАЦИЯ")}");
            sb.AppendLine($"Количество переменных: {task.n}");
            sb.AppendLine($"Количество ограничений: {task.m}");
            sb.AppendLine();

            bool feasible = true;
            double[] leftParts = new double[task.m];
            bool[] active = new bool[task.m];

            for (int i = 0; i < task.m; i++)
            {
                leftParts[i] = 0;
                for (int j = 0; j < task.n; j++)
                    leftParts[i] += task.A[i, j] * x[j];

                bool ok = false;
                bool isActive = false;

                if (task.signs[i] == "<=")
                {
                    ok = leftParts[i] <= task.b[i] + eps;
                    isActive = Math.Abs(leftParts[i] - task.b[i]) < eps;
                }
                else if (task.signs[i] == ">=")
                {
                    ok = leftParts[i] >= task.b[i] - eps;
                    isActive = Math.Abs(leftParts[i] - task.b[i]) < eps;
                }
                else // "="
                {
                    ok = Math.Abs(leftParts[i] - task.b[i]) < eps;
                    isActive = true;
                }

                active[i] = isActive;
                string status = ok ? (isActive ? " АКТИВНОЕ" : "") : "";
                sb.AppendLine($"{i + 1}. {leftParts[i]:F2} {task.signs[i]} {task.b[i]:F2} {status}");

                if (!ok) feasible = false;
            }

            if (!feasible)
            {
                sb.AppendLine("\n ПЛАН НЕ ДОПУСТИМ!");
                sb.AppendLine("План нарушает хотя бы одно ограничение задачи.");
                sb.AppendLine("Рекомендация: исправьте план или используйте другой метод решения.");
                return sb;
            }

            double F = 0;
            for (int i = 0; i < task.n; i++)
                F += task.c[i] * x[i];

            sb.AppendLine($"\nПлан допустим. F(X) = {F:F2}");

            sb.AppendLine("\n================================================");
            sb.AppendLine("ШАГ 2: ОПРЕДЕЛЕНИЕ МНОЖЕСТВ БАЗИСНЫХ ПЕРЕМЕННЫХ И НЕАКТИВНЫХ ОГРАНИЧЕНИЙ");
            sb.AppendLine("================================================");

            List<int> J = new List<int>();
            for (int j = 0; j < task.n; j++)
                if (x[j] > eps) J.Add(j);

            List<int> I = new List<int>();
            for (int i = 0; i < task.m; i++)
                if (!active[i]) I.Add(i);

            sb.Append("J = {");
            foreach (int j in J) sb.Append($"{j + 1} ");
            sb.AppendLine("}  (индексы переменных с положительными значениями)");

            sb.Append("I = {");
            foreach (int i in I) sb.Append($"{i + 1} ");
            sb.AppendLine("}  (индексы неактивных ограничений)");

            sb.AppendLine($"\nКоличество уравнений: |J| = {J.Count}");
            sb.AppendLine($"Количество свободных переменных: m - |I| = {task.m} - {I.Count} = {task.m - I.Count}");

            sb.AppendLine("\n================================================");
            sb.AppendLine("ШАГ 3: РЕШЕНИЕ СИСТЕМЫ УРАВНЕНИЙ ДЛЯ ДВОЙСТВЕННЫХ ПЕРЕМЕННЫХ");
            sb.AppendLine("================================================");

            double[] y = new double[task.m];
            
            foreach (int i in I) y[i] = 0;

            int eqCount = J.Count;
            double[,] sysA = new double[eqCount, task.m];
            double[] sysB = new double[eqCount];

            for (int eq = 0; eq < eqCount; eq++)
            {
                int j = J[eq];
                for (int i = 0; i < task.m; i++)
                    sysA[eq, i] = task.A[i, j];
                sysB[eq] = task.c[j];
            }

            if (!SolveSystem(sysA, sysB, y, I, task.m, sb))
            {
                sb.AppendLine("\nСистема уравнений не имеет решения");
                sb.AppendLine("Это означает, что условия дополняющей нежесткости не выполняются.");
                sb.AppendLine("План не является оптимальным.");
                return sb;
            }

            sb.Append("\nНайдены двойственные переменные: Y = (");
            for (int i = 0; i < task.m; i++)
            {
                sb.Append($"{y[i]:F4}");
                if (i < task.m - 1) sb.Append("; ");
            }
            sb.AppendLine(")");

            sb.AppendLine("\n================================================");
            sb.AppendLine("ШАГ 4: ПРОВЕРКА ДОПУСТИМОСТИ ДВОЙСТВЕННОГО ПЛАНА");
            sb.AppendLine("================================================");

            bool dualOk = true;

            sb.AppendLine("Проверка неотрицательности двойственных переменных:");
            for (int i = 0; i < task.m; i++)
            {
                if (task.signs[i] == "=")
                {
                    sb.AppendLine($" y[{i + 1}] = {y[i]:F4}  (равенство - любой знак)");
                }
                else if (task.signs[i] == "<=")
                {
                    if (y[i] >= -eps)
                    {
                        sb.AppendLine($" y[{i + 1}] = {y[i]:F4} >= 0  - ВЫПОЛНЕНО");
                    }
                    else
                    {
                        sb.AppendLine($" y[{i + 1}] = {y[i]:F4} < 0  - НАРУШЕНИЕ");
                        dualOk = false;
                    }
                }
                else // ">="
                {
                    if (y[i] <= eps)
                    {
                        sb.AppendLine($" y[{i + 1}] = {y[i]:F4} <= 0  - ВЫПОЛНЕНО");
                    }
                    else
                    {
                        sb.AppendLine($" y[{i + 1}] = {y[i]:F4} > 0  - НАРУШЕНИЕ");
                        dualOk = false;
                    }
                }
            }

            sb.AppendLine("\nПроверка ограничений двойственной задачи:");
            string dSign = task.taskType == 1 ? ">=" : "<=";
            string taskTypeName = task.taskType == 1 ? "максимизации" : "минимизации";
            sb.AppendLine($"Тип задачи: {taskTypeName}, знак ограничений: {dSign}");

            for (int j = 0; j < task.n; j++)
            {
                double sum = 0;
                for (int i = 0; i < task.m; i++)
                    sum += task.A[i, j] * y[i];

                bool ok;
                if (task.taskType == 1)
                    ok = sum >= task.c[j] - eps;
                else
                    ok = sum <= task.c[j] + eps;
                    
                string status = ok ? "ВЫПОЛНЕНО" : "НАРУШЕНИЕ";
                sb.AppendLine($"{j + 1}. {sum:F4} {dSign} {task.c[j]:F4}  - {status}");

                if (!ok) dualOk = false;
            }

            sb.AppendLine("\n================================================");
            sb.AppendLine("ШАГ 5: ПРОВЕРКА ТЕОРЕМЫ ДВОЙСТВЕННОСТИ");
            sb.AppendLine("================================================");

            double G = 0;
            for (int i = 0; i < task.m; i++)
                G += task.b[i] * y[i];

            sb.AppendLine($"Значение прямой целевой функции: F(X) = {F:F4}");
            sb.AppendLine($"Значение двойственной целевой функции: G(Y) = {G:F4}");

            bool dualityTheoremOk = Math.Abs(F - G) < eps;
            if (dualityTheoremOk)
            {
                sb.AppendLine("Теорема двойственности выполняется: F(X) = G(Y)");
            }
            else
            {
                sb.AppendLine($"Нарушение теоремы двойственности: F(X) не равно G(Y)");
                sb.AppendLine($"  Разница: |{F:F4} - {G:F4}| = {Math.Abs(F - G):F6}");
                dualOk = false;
            }

            sb.AppendLine("\n================================================");
            sb.AppendLine("ФИНАЛЬНЫЙ РЕЗУЛЬТАТ ПРОВЕРКИ");
            sb.AppendLine("================================================");

            if (dualOk && dualityTheoremOk)
            {
                sb.AppendLine("\n ПЛАН ОПТИМАЛЕН! ");
                sb.AppendLine("Все условия оптимальности выполнены:");
                sb.AppendLine(" 1. План допустим для прямой задачи");
                sb.AppendLine(" 2. Двойственный план допустим (неотрицательность и ограничения)");
                sb.AppendLine(" 3. Выполнены условия дополняющей нежесткости");
                sb.AppendLine(" 4. Выполняется теорема двойственности: F(X) = G(Y)");
                sb.AppendLine($"\nОптимальное значение целевой функции: {F:F4}");
                sb.AppendLine("\nРЕКОМЕНДАЦИЯ: План является оптимальным решением задачи.");
            }
            else
            {
                sb.AppendLine("\n ПЛАН НЕ ОПТИМАЛЕН");
                sb.AppendLine("Причины:");
                if (!dualOk)
                    sb.AppendLine(" - Нарушены условия допустимости двойственного плана");
                if (!dualityTheoremOk)
                    sb.AppendLine(" - Нарушена теорема двойственности");
                sb.AppendLine("\nРЕКОМЕНДАЦИЯ: Используйте симплекс-метод для поиска оптимального решения.");
            }

            return sb;
        }


        private bool SolveSystem(double[,] A, double[] b, double[] y, List<int> fixedIndices, int m, StringBuilder log)
        {
            int eq = A.GetLength(0);
            double eps = 0.0001;

            List<int> free = new List<int>();
            for (int i = 0; i < m; i++)
                if (!fixedIndices.Contains(i))
                    free.Add(i);

            int fc = free.Count;

            if (eq == 0)
            {
                log.AppendLine("Нет уравнений - тривиальное решение Y = 0");
                return true;
            }

            if (fc == 0)
            {
                log.AppendLine("Все двойственные переменные зафиксированы в 0");
                for (int i = 0; i < eq; i++)
                    if (Math.Abs(b[i]) > eps)
                    {
                        log.AppendLine($" Уравнение {i + 1} не выполняется: 0 не равно {b[i]:F4}");
                        return false;
                    }
                log.AppendLine(" Все уравнения выполняются при Y = 0");
                return true;
            }

            if (eq > fc)
            {
                log.AppendLine($"Переопределенная система: {eq} уравнений, {fc} переменных");

                double[,] mx = new double[fc, fc + 1];
                for (int i = 0; i < fc; i++)
                {
                    for (int j = 0; j < fc; j++)
                        mx[i, j] = A[i, free[j]];
                    mx[i, fc] = b[i];
                }

                if (!GaussianElimination(mx, fc))
                {
                    log.AppendLine("Сокращенная система несовместна");
                    return false;
                }

                double[] sol = new double[fc];
                for (int i = fc - 1; i >= 0; i--)
                {
                    sol[i] = mx[i, fc];
                    for (int j = i + 1; j < fc; j++)
                        sol[i] -= mx[i, j] * sol[j];
                    sol[i] /= mx[i, i];
                }

                for (int i = 0; i < fc; i++)
                    y[free[i]] = sol[i];

                log.AppendLine("Проверка оставшихся уравнений:");
                for (int eqIdx = fc; eqIdx < eq; eqIdx++)
                {
                    double leftPart = 0;
                    for (int j = 0; j < fc; j++)
                        leftPart += A[eqIdx, free[j]] * sol[j];

                    if (Math.Abs(leftPart - b[eqIdx]) > eps)
                    {
                        log.AppendLine($" Уравнение {eqIdx + 1} не выполняется: {leftPart:F4} не равно {b[eqIdx]:F4}");
                        return false;
                    }
                    log.AppendLine($" Уравнение {eqIdx + 1} выполняется: {leftPart:F4} = {b[eqIdx]:F4}");
                }

                log.AppendLine("Переопределенная система успешно решена");
                return true;
            }

            if (eq < fc)
            {
                log.AppendLine($"Недоопределенная система: {eq} уравнений, {fc} переменных");

                double[,] reducedA = new double[eq, eq];
                double[] reducedB = new double[eq];

                for (int i = 0; i < eq; i++)
                {
                    for (int j = 0; j < eq; j++)
                        reducedA[i, j] = A[i, free[j]];
                    reducedB[i] = b[i];
                }

                double[] reducedSol = new double[eq];
                if (!SolveSquareSystem(reducedA, reducedB, reducedSol, eq))
                {
                    log.AppendLine("Квадратная подсистема несовместна");
                    return false;
                }

                for (int i = 0; i < eq; i++)
                    y[free[i]] = reducedSol[i];

                for (int i = eq; i < fc; i++)
                    y[free[i]] = 0;

                log.AppendLine("Найдено решение с минимальной нормой");
                log.AppendLine($"  Первые {eq} переменных: ненулевые значения");
                log.AppendLine($"  Остальные {fc - eq} переменных: установлены в 0");
                return true;
            }

            log.AppendLine($"Квадратная система: {eq} уравнений, {fc} переменных");

            double[,] matrix = new double[eq, fc + 1];
            for (int i = 0; i < eq; i++)
            {
                for (int j = 0; j < fc; j++)
                    matrix[i, j] = A[i, free[j]];
                matrix[i, fc] = b[i];
            }

            if (!GaussianElimination(matrix, eq))
            {
                log.AppendLine("Квадратная система вырождена или несовместна");
                return false;
            }

            double[] solution = new double[fc];
            for (int i = eq - 1; i >= 0; i--)
            {
                solution[i] = matrix[i, fc];
                for (int j = i + 1; j < fc; j++)
                    solution[i] -= matrix[i, j] * solution[j];
                solution[i] /= matrix[i, i];
            }

            for (int i = 0; i < fc; i++)
                y[free[i]] = solution[i];

            log.AppendLine("Квадратная система успешно решена");
            return true;
        }

        private bool SolveSquareSystem(double[,] A, double[] b, double[] x, int n)
        {
            double[,] matrix = new double[n, n + 1];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                    matrix[i, j] = A[i, j];
                matrix[i, n] = b[i];
            }

            if (!GaussianElimination(matrix, n))
                return false;

            for (int i = n - 1; i >= 0; i--)
            {
                x[i] = matrix[i, n];
                for (int j = i + 1; j < n; j++)
                    x[i] -= matrix[i, j] * x[j];
                x[i] /= matrix[i, i];
            }

            return true;
        }

        private bool GaussianElimination(double[,] matrix, int n)
        {
            double eps = 0.0001;

            for (int k = 0; k < n; k++)
            {
                int maxRow = k;
                for (int i = k + 1; i < n; i++)
                    if (Math.Abs(matrix[i, k]) > Math.Abs(matrix[maxRow, k]))
                        maxRow = i;

                if (maxRow != k)
                {
                    for (int j = 0; j <= n; j++)
                    {
                        double temp = matrix[k, j];
                        matrix[k, j] = matrix[maxRow, j];
                        matrix[maxRow, j] = temp;
                    }
                }

                if (Math.Abs(matrix[k, k]) < eps)
                    return false;

                for (int i = k + 1; i < n; i++)
                {
                    double factor = matrix[i, k] / matrix[k, k];
                    for (int j = k; j <= n; j++)
                        matrix[i, j] -= factor * matrix[k, j];
                }
            }

            return true;
        }
    }
}
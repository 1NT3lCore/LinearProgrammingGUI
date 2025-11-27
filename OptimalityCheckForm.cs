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

        public OptimalityCheckForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Проверка плана на оптимальность";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            Button btnInputTask = new Button
            {
                Text = "1. Ввести задачу",
                Location = new Point(20, 20),
                Size = new Size(150, 40)
            };
            btnInputTask.Click += BtnInputTask_Click;
            this.Controls.Add(btnInputTask);

            Button btnInputPlan = new Button
            {
                Text = "2. Ввести план",
                Location = new Point(190, 20),
                Size = new Size(150, 40),
                Enabled = false
            };
            btnInputPlan.Click += BtnInputPlan_Click;
            this.Controls.Add(btnInputPlan);

            Button btnCheck = new Button
            {
                Text = "3. Проверить",
                Location = new Point(360, 20),
                Size = new Size(150, 40),
                Enabled = false
            };
            btnCheck.Click += BtnCheck_Click;
            this.Controls.Add(btnCheck);

            txtResult = new TextBox
            {
                Location = new Point(20, 80),
                Size = new Size(840, 550),
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                Font = new Font("Courier New", 9),
                ReadOnly = true
            };
            this.Controls.Add(txtResult);

            Button btnClose = new Button
            {
                Text = "Закрыть",
                Location = new Point(760, 640),
                Size = new Size(100, 30)
            };
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
        }

        private void BtnInputTask_Click(object sender, EventArgs e)
        {
            InputTaskForm inputForm = new InputTaskForm();
            inputForm.ShowDialog();

            if (inputForm.IsCompleted)
            {
                task = inputForm.Task;
                this.Controls[1].Enabled = true; // Включаем кнопку ввода плана
                MessageBox.Show("Задача введена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnInputPlan_Click(object sender, EventArgs e)
        {
            Form planForm = new Form
            {
                Text = "Ввод плана",
                Size = new Size(400, 300),
                StartPosition = FormStartPosition.CenterParent
            };

            Label lbl = new Label
            {
                Text = "Введите значения переменных:",
                Location = new Point(20, 20),
                Size = new Size(300, 20)
            };
            planForm.Controls.Add(lbl);

            NumericUpDown[] inputs = new NumericUpDown[task.n];
            for (int i = 0; i < task.n; i++)
            {
                Label lblVar = new Label
                {
                    Text = $"x[{i + 1}]:",
                    Location = new Point(20, 50 + i * 30),
                    Size = new Size(50, 20)
                };
                planForm.Controls.Add(lblVar);

                inputs[i] = new NumericUpDown
                {
                    Location = new Point(80, 50 + i * 30),
                    Size = new Size(100, 20),
                    DecimalPlaces = 2,
                    Minimum = 0,
                    Maximum = 10000
                };
                planForm.Controls.Add(inputs[i]);
            }

            Button btnOK = new Button
            {
                Text = "OK",
                Location = new Point(100, 50 + task.n * 30 + 20),
                Size = new Size(80, 30)
            };
            btnOK.Click += (s, ev) =>
            {
                plan = new double[task.n];
                for (int i = 0; i < task.n; i++)
                    plan[i] = (double)inputs[i].Value;
                planForm.Close();
                this.Controls[2].Enabled = true; // Включаем кнопку проверки
            };
            planForm.Controls.Add(btnOK);

            planForm.ShowDialog();
        }

        private void BtnCheck_Click(object sender, EventArgs e)
        {
            StringBuilder result = CheckOptimality(task, plan);
            txtResult.Text = result.ToString();
        }

        private StringBuilder CheckOptimality(LPTask task, double[] x)
        {
            StringBuilder sb = new StringBuilder();
            double eps = 0.0001;

            sb.AppendLine("═══════════════════════════════════════════════════");
            sb.AppendLine("ШАГ 1: ПРОВЕРКА ДОПУСТИМОСТИ");
            sb.AppendLine("═══════════════════════════════════════════════════");

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
                else
                {
                    ok = Math.Abs(leftParts[i] - task.b[i]) < eps;
                    isActive = true;
                }

                active[i] = isActive;
                sb.AppendLine($"{i + 1}. {leftParts[i]:F2} {task.signs[i]} {task.b[i]:F2} {(ok ? (isActive ? "✓ АКТИВНОЕ" : "✓") : "✗")}");

                if (!ok) feasible = false;
            }

            if (!feasible)
            {
                sb.AppendLine("\n✗ ПЛАН НЕ ДОПУСТИМ!");
                return sb;
            }

            double F = 0;
            for (int i = 0; i < task.n; i++)
                F += task.c[i] * x[i];

            sb.AppendLine($"\n✓ План допустим. F(X) = {F:F2}");

            // Множества J и I
            sb.AppendLine("\n═══════════════════════════════════════════════════");
            sb.AppendLine("ШАГ 2: МНОЖЕСТВА J И I");
            sb.AppendLine("═══════════════════════════════════════════════════");

            List<int> J = new List<int>();
            for (int j = 0; j < task.n; j++)
                if (x[j] > eps) J.Add(j);

            List<int> I = new List<int>();
            for (int i = 0; i < task.m; i++)
                if (!active[i]) I.Add(i);

            sb.Append("J = {");
            foreach (int j in J) sb.Append($"{j + 1} ");
            sb.AppendLine("}");

            sb.Append("I = {");
            foreach (int i in I) sb.Append($"{i + 1} ");
            sb.AppendLine("}");

            // Решение системы
            sb.AppendLine("\n═══════════════════════════════════════════════════");
            sb.AppendLine("ШАГ 3: РЕШЕНИЕ СИСТЕМЫ");
            sb.AppendLine("═══════════════════════════════════════════════════");

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
                sb.AppendLine("\n✗ Система не решается");
                return sb;
            }

            sb.Append("\n✓ Решение: Y = (");
            for (int i = 0; i < task.m; i++)
            {
                sb.Append($"{y[i]:F2}");
                if (i < task.m - 1) sb.Append("; ");
            }
            sb.AppendLine(")");

            // Проверка двойственного плана
            sb.AppendLine("\n═══════════════════════════════════════════════════");
            sb.AppendLine("ШАГ 4: ПРОВЕРКА ДВОЙСТВЕННОГО ПЛАНА");
            sb.AppendLine("═══════════════════════════════════════════════════");

            bool dualOk = true;

            for (int i = 0; i < task.m; i++)
            {
                if (y[i] < -eps)
                {
                    sb.AppendLine($"✗ y[{i + 1}] = {y[i]:F2} < 0");
                    dualOk = false;
                }
            }

            string dSign = task.taskType == 1 ? ">=" : "<=";
            for (int j = 0; j < task.n; j++)
            {
                double sum = 0;
                for (int i = 0; i < task.m; i++)
                    sum += task.A[i, j] * y[i];

                bool ok = task.taskType == 1 ? sum >= task.c[j] - eps : sum <= task.c[j] + eps;
                sb.AppendLine($"{j + 1}. {sum:F2} {dSign} {task.c[j]:F2} {(ok ? "✓" : "✗")}");

                if (!ok) dualOk = false;
            }

            // Результат
            sb.AppendLine("\n═══════════════════════════════════════════════════");
            sb.AppendLine("РЕЗУЛЬТАТ:");
            sb.AppendLine("═══════════════════════════════════════════════════");

            if (dualOk)
            {
                double G = 0;
                for (int i = 0; i < task.m; i++)
                    G += task.b[i] * y[i];

                sb.AppendLine("\n✓✓✓ ПЛАН ОПТИМАЛЕН! ✓✓✓");
                sb.AppendLine($"F(X) = {F:F2}");
                sb.AppendLine($"G(Y) = {G:F2}");
            }
            else
            {
                sb.AppendLine("\n✗ ПЛАН НЕ ОПТИМАЛЕН");
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

            if (eq == 0) return true;
            if (fc == 0)
            {
                for (int i = 0; i < eq; i++)
                    if (Math.Abs(b[i]) > eps) return false;
                return true;
            }

            if (eq > fc)
            {
                log.AppendLine($"Переопределённая система ({eq} уравнений, {fc} переменных)");

                double[,] mx = new double[fc, fc + 1];
                for (int i = 0; i < fc; i++)
                {
                    for (int j = 0; j < fc; j++)
                        mx[i, j] = A[i, free[j]];
                    mx[i, fc] = b[i];
                }

                for (int k = 0; k < fc; k++)
                {
                    int maxR = k;
                    for (int i = k + 1; i < fc; i++)
                        if (Math.Abs(mx[i, k]) > Math.Abs(mx[maxR, k]))
                            maxR = i;

                    for (int j = 0; j <= fc; j++)
                    {
                        double t = mx[k, j];
                        mx[k, j] = mx[maxR, j];
                        mx[maxR, j] = t;
                    }

                    if (Math.Abs(mx[k, k]) < eps) return false;

                    for (int i = k + 1; i < fc; i++)
                    {
                        double f = mx[i, k] / mx[k, k];
                        for (int j = k; j <= fc; j++)
                            mx[i, j] -= f * mx[k, j];
                    }
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

                for (int eqIdx = fc; eqIdx < eq; eqIdx++)
                {
                    double leftPart = 0;
                    for (int j = 0; j < fc; j++)
                        leftPart += A[eqIdx, free[j]] * sol[j];

                    if (Math.Abs(leftPart - b[eqIdx]) > eps)
                        return false;
                }

                return true;
            }

            if (eq < fc) return false;

            // Квадратная система
            double[,] matrix = new double[eq, fc + 1];
            for (int i = 0; i < eq; i++)
            {
                for (int j = 0; j < fc; j++)
                    matrix[i, j] = A[i, free[j]];
                matrix[i, fc] = b[i];
            }

            for (int k = 0; k < eq; k++)
            {
                int maxR = k;
                for (int i = k + 1; i < eq; i++)
                    if (Math.Abs(matrix[i, k]) > Math.Abs(matrix[maxR, k]))
                        maxR = i;

                for (int j = 0; j <= fc; j++)
                {
                    double t = matrix[k, j];
                    matrix[k, j] = matrix[maxR, j];
                    matrix[maxR, j] = t;
                }

                if (Math.Abs(matrix[k, k]) < eps) return false;

                for (int i = k + 1; i < eq; i++)
                {
                    double f = matrix[i, k] / matrix[k, k];
                    for (int j = k; j <= fc; j++)
                        matrix[i, j] -= f * matrix[k, j];
                }
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

            return true;
        }
    }
}
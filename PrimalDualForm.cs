using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LinearProgrammingGUI
{
    public class PrimalDualForm : Form
    {
        private TextBox txtResult;
        private LPTask currentTask;

        public PrimalDualForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Решение прямой и двойственной задачи";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            Button btnInput = new Button
            {
                Text = "Ввести задачу",
                Location = new Point(20, 20),
                Size = new Size(150, 40)
            };
            btnInput.Click += BtnInput_Click;
            this.Controls.Add(btnInput);

            Button btnSolve = new Button
            {
                Text = "Решить",
                Location = new Point(190, 20),
                Size = new Size(150, 40),
                Enabled = false
            };
            btnSolve.Click += BtnSolve_Click;
            this.Controls.Add(btnSolve);

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

        private void BtnInput_Click(object sender, EventArgs e)
        {
            InputTaskForm inputForm = new InputTaskForm();
            inputForm.ShowDialog();

            if (inputForm.IsCompleted)
            {
                currentTask = inputForm.Task;
                this.Controls[1].Enabled = true;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("═══════════════════════════════════════════════════");
                sb.AppendLine("ВВЕДЕННАЯ ЗАДАЧА:");
                sb.AppendLine("═══════════════════════════════════════════════════");
                sb.AppendLine(TaskToString(currentTask));
                txtResult.Text = sb.ToString();
            }
        }

        private void BtnSolve_Click(object sender, EventArgs e)
        {
            if (currentTask.n == 0 || currentTask.m == 0)
            {
                MessageBox.Show("Сначала введите задачу!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var (primal, dual) = LPSolver.SolvePrimalDualSimultaneously(currentTask);
                DisplayResults(primal, dual);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при решении: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayResults(SolutionResult primal, SolutionResult dual)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("═══════════════════════════════════════════════════");
            sb.AppendLine("РЕШЕНИЕ ПРЯМОЙ ЗАДАЧИ:");
            sb.AppendLine("═══════════════════════════════════════════════════");
            sb.AppendLine(FormatSolutionResult(primal, "F", "x"));

            sb.AppendLine();
            sb.AppendLine("═══════════════════════════════════════════════════");
            sb.AppendLine("РЕШЕНИЕ ДВОЙСТВЕННОЙ ЗАДАЧИ:");
            sb.AppendLine("═══════════════════════════════════════════════════");
            sb.AppendLine(FormatSolutionResult(dual, "G", "y"));

            sb.AppendLine();
            sb.AppendLine("═══════════════════════════════════════════════════");
            sb.AppendLine("АНАЛИЗ РЕЗУЛЬТАТОВ:");
            sb.AppendLine("═══════════════════════════════════════════════════");

            if (primal.IsOptimal && dual.IsOptimal)
            {
                sb.AppendLine("✓ Обе задачи решены оптимально");
                sb.AppendLine($"F(X) = {primal.ObjectiveValue:F4}");
                sb.AppendLine($"G(Y) = {dual.ObjectiveValue:F4}");

                if (Math.Abs(primal.ObjectiveValue - dual.ObjectiveValue) < 0.001)
                    sb.AppendLine("✓ Теорема двойственности выполняется: F(X) = G(Y)");
                else
                    sb.AppendLine("⚠ Замечено расхождение в значениях целевых функций");
            }
            else
            {
                sb.AppendLine("✗ Оптимальные решения не найдены для обеих задач");
            }

            txtResult.Text = sb.ToString();
        }

        private string FormatSolutionResult(SolutionResult result, string funcName, string varName)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(result.Message);
            sb.AppendLine($"Допустимость: {result.IsFeasible}");
            sb.AppendLine($"Оптимальность: {result.IsOptimal}");
            sb.AppendLine($"Неограниченность: {result.IsUnbounded}");

            if (result.IsOptimal && result.Solution != null)
            {
                sb.AppendLine($"Значение целевой функции: {funcName} = {result.ObjectiveValue:F4}");
                sb.Append($"{funcName}({varName}) = ");

                for (int i = 0; i < result.Solution.Length; i++)
                {
                    if (i > 0 && result.Solution[i] >= 0) sb.Append(" + ");
                    sb.Append($"{result.Solution[i]:F4}*{varName}{i + 1}");
                }
                sb.AppendLine();

                sb.Append("Решение: (");
                for (int i = 0; i < Math.Min(result.Solution.Length, 10); i++) // Ограничиваем вывод
                {
                    sb.Append($"{result.Solution[i]:F4}");
                    if (i < result.Solution.Length - 1) sb.Append("; ");
                }
                if (result.Solution.Length > 10) sb.Append("...");
                sb.AppendLine(")");
            }

            return sb.ToString();
        }

        private string TaskToString(LPTask task)
        {
            StringBuilder sb = new StringBuilder();
            string funcName = task.taskType == 1 ? "F" : "G";

            sb.Append($"{funcName}(x) = ");
            for (int i = 0; i < task.c.Length; i++)
            {
                if (i > 0 && task.c[i] >= 0) sb.Append(" + ");
                sb.Append($"{task.c[i]}*x{i + 1}");
            }
            sb.AppendLine(task.taskType == 1 ? " → max" : " → min");

            sb.AppendLine("\nОграничения:");
            for (int i = 0; i < task.m; i++)
            {
                sb.Append("  ");
                for (int j = 0; j < task.n; j++)
                {
                    if (j > 0 && task.A[i, j] >= 0) sb.Append(" + ");
                    sb.Append($"{task.A[i, j]}*x{j + 1}");
                }
                sb.AppendLine($" {task.signs[i]} {task.b[i]}");
            }

            return sb.ToString();
        }
    }
}
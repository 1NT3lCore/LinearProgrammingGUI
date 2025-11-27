using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LinearProgrammingGUI
{
    public class DualSimplexForm : Form
    {
        private TextBox txtResult;
        private LPTask currentTask;

        public DualSimplexForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Решение двойственным симплекс-методом";
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
                var result = LPSolver.SolveDualSimplex(currentTask);
                DisplayResult(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при решении: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayResult(SolutionResult result)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("═══════════════════════════════════════════════════");
            sb.AppendLine("РЕЗУЛЬТАТ РЕШЕНИЯ ДВОЙСТВЕННЫМ СИМПЛЕКС-МЕТОДОМ:");
            sb.AppendLine("═══════════════════════════════════════════════════");

            sb.AppendLine($"Сообщение: {result.Message}");
            sb.AppendLine($"Допустимость: {result.IsFeasible}");
            sb.AppendLine($"Оптимальность: {result.IsOptimal}");
            sb.AppendLine($"Неограниченность: {result.IsUnbounded}");

            if (result.IsOptimal && result.Solution != null)
            {
                sb.AppendLine($"\nЗначение целевой функции: F = {result.ObjectiveValue:F4}");

                sb.Append("Решение: (");
                for (int i = 0; i < Math.Min(result.Solution.Length, 10); i++)
                {
                    sb.Append($"x{i + 1} = {result.Solution[i]:F4}");
                    if (i < result.Solution.Length - 1) sb.Append("; ");
                }
                if (result.Solution.Length > 10) sb.Append("...");
                sb.AppendLine(")");

                sb.AppendLine("\nПроверка ограничений:");
                for (int i = 0; i < currentTask.m; i++)
                {
                    double leftPart = 0;
                    for (int j = 0; j < currentTask.n; j++)
                    {
                        leftPart += currentTask.A[i, j] * result.Solution[j];
                    }

                    string status = "✓";
                    if (currentTask.signs[i] == "<=" && leftPart > currentTask.b[i] + 0.001) status = "✗";
                    if (currentTask.signs[i] == ">=" && leftPart < currentTask.b[i] - 0.001) status = "✗";
                    if (currentTask.signs[i] == "=" && Math.Abs(leftPart - currentTask.b[i]) > 0.001) status = "✗";

                    sb.AppendLine($"{i + 1}. {leftPart:F4} {currentTask.signs[i]} {currentTask.b[i]:F4} {status}");
                }
            }

            txtResult.Text = sb.ToString();
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
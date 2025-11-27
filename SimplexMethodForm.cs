using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LinearProgrammingGUI
{
    public class SimplexMethodForm : Form
    {
        private TextBox txtResult;
        private LPTask task;

        public SimplexMethodForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Симплекс-метод решения ЗЛП";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Кнопка ввода задачи
            Button btnInputTask = new Button
            {
                Text = "Ввести задачу",
                Location = new Point(20, 20),
                Size = new Size(150, 40)
            };
            btnInputTask.Click += BtnInputTask_Click;
            this.Controls.Add(btnInputTask);

            // Кнопка решения
            Button btnSolve = new Button
            {
                Text = "Решить симплекс-методом",
                Location = new Point(190, 20),
                Size = new Size(200, 40),
                Enabled = false
            };
            btnSolve.Click += BtnSolve_Click;
            this.Controls.Add(btnSolve);

            // Поле вывода результатов
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

            // Кнопка закрытия
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
                this.Controls[1].Enabled = true; // Включаем кнопку решения

                // Показываем введенную задачу
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("═══════════════════════════════════════════════════");
                sb.AppendLine("ВВЕДЕННАЯ ЗАДАЧА:");
                sb.AppendLine("═══════════════════════════════════════════════════");
                sb.AppendLine(TaskToString(task));
                txtResult.Text = sb.ToString();
            }
        }

        private void BtnSolve_Click(object sender, EventArgs e)
        {
            if (task.n == 0 || task.m == 0)
            {
                MessageBox.Show("Сначала введите задачу!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Создаем решатель симплекс-метода
                SimplexSolver solver = new SimplexSolver(task);
                string result = solver.SolveWithSteps();
                txtResult.Text = result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при решении: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string TaskToString(LPTask task)
        {
            StringBuilder sb = new StringBuilder();

            // Целевая функция
            sb.Append("F(X) = ");
            for (int i = 0; i < task.c.Length; i++)
            {
                if (i > 0 && task.c[i] >= 0) sb.Append(" + ");
                sb.Append($"{task.c[i]}*x{i + 1}");
            }
            sb.AppendLine(task.taskType == 1 ? " → max" : " → min");

            // Ограничения
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

            sb.AppendLine($"\n  x[i] >= 0");

            return sb.ToString();
        }
    }
}

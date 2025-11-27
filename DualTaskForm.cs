using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LinearProgrammingGUI
{
    public class DualTaskForm : Form
    {
        private TextBox txtResult;

        public DualTaskForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Построение двойственной задачи";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            Button btnInput = new Button
            {
                Text = "Ввести задачу",
                Location = new Point(20, 20),
                Size = new Size(150, 40)
            };
            btnInput.Click += BtnInput_Click;
            this.Controls.Add(btnInput);

            txtResult = new TextBox
            {
                Location = new Point(20, 80),
                Size = new Size(740, 450),
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                Font = new Font("Courier New", 10),
                ReadOnly = true
            };
            this.Controls.Add(txtResult);

            Button btnClose = new Button
            {
                Text = "Закрыть",
                Location = new Point(650, 540),
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
                LPTask primal = inputForm.Task;
                LPTask dual = BuildDual(primal);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("═══════════════════════════════════════════════════");
                sb.AppendLine("ПРЯМАЯ ЗАДАЧА:");
                sb.AppendLine("═══════════════════════════════════════════════════");
                sb.AppendLine(TaskToString(primal, "x", "F"));

                sb.AppendLine();
                sb.AppendLine("═══════════════════════════════════════════════════");
                sb.AppendLine("ДВОЙСТВЕННАЯ ЗАДАЧА:");
                sb.AppendLine("═══════════════════════════════════════════════════");
                sb.AppendLine(TaskToString(dual, "y", "G"));

                sb.AppendLine();
                sb.AppendLine("Правила построения:");
                sb.AppendLine("• Матрица транспонируется");
                sb.AppendLine("• Коэффициенты целевой функции ↔ Правые части");
                sb.AppendLine("• Max → Min (и наоборот)");

                txtResult.Text = sb.ToString();
            }
        }

        private LPTask BuildDual(LPTask primal)
        {
            LPTask dual = new LPTask
            {
                n = primal.m,
                m = primal.n,
                taskType = 3 - primal.taskType,
                c = primal.b,
                b = primal.c,
                A = new double[primal.n, primal.m],
                signs = new string[primal.n]
            };

            // Транспонирование
            for (int i = 0; i < dual.m; i++)
                for (int j = 0; j < dual.n; j++)
                    dual.A[i, j] = primal.A[j, i];

            // Знаки
            string dualSign = primal.taskType == 1 ? ">=" : "<=";
            for (int i = 0; i < dual.m; i++)
                dual.signs[i] = dualSign;

            return dual;
        }

        private string TaskToString(LPTask task, string varName, string funcName)
        {
            StringBuilder sb = new StringBuilder();

            // Целевая функция
            sb.Append($"{funcName}({varName}) = ");
            for (int i = 0; i < task.c.Length; i++)
            {
                if (i > 0 && task.c[i] >= 0) sb.Append(" + ");
                sb.Append($"{task.c[i]}*{varName}{i + 1}");
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
                    sb.Append($"{task.A[i, j]}*{varName}{j + 1}");
                }
                sb.AppendLine($" {task.signs[i]} {task.b[i]}");
            }

            sb.AppendLine($"\n  {varName}[i] >= 0");

            return sb.ToString();
        }
    }
}
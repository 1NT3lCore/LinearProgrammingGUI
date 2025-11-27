using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LinearProgrammingGUI
{
    public class TransportProblemForm : Form
    {
        private TextBox txtResult;
        private double[,] costs;
        private double[] supply;
        private double[] demand;

        public TransportProblemForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Транспортная задача - Метод северо-западного угла";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Кнопка ввода данных
            Button btnInputData = new Button
            {
                Text = "Ввести данные транспортной задачи",
                Location = new Point(20, 20),
                Size = new Size(250, 40)
            };
            btnInputData.Click += BtnInputData_Click;
            this.Controls.Add(btnInputData);

            // Кнопка решения
            Button btnSolve = new Button
            {
                Text = "Решить методом СЗУ",
                Location = new Point(290, 20),
                Size = new Size(150, 40),
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

        private void BtnInputData_Click(object sender, EventArgs e)
        {
            TransportInputForm inputForm = new TransportInputForm();
            inputForm.ShowDialog();

            if (inputForm.IsCompleted)
            {
                costs = inputForm.Costs;
                supply = inputForm.Supply;
                demand = inputForm.Demand;
                this.Controls[1].Enabled = true; // Включаем кнопку решения

                // Показываем введенные данные
                ShowInputData();
            }
        }

        private void BtnSolve_Click(object sender, EventArgs e)
        {
            if (costs == null || supply == null || demand == null)
            {
                MessageBox.Show("Сначала введите данные задачи!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Создаем решатель транспортной задачи
                NorthWestCornerSolver solver = new NorthWestCornerSolver(costs, supply, demand);
                string result = solver.SolveWithOutput();
                txtResult.Text = result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при решении: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowInputData()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════");
            sb.AppendLine("ВВЕДЕННЫЕ ДАННЫЕ ТРАНСПОРТНОЙ ЗАДАЧИ:");
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

            txtResult.Text = sb.ToString();
        }
    }
}
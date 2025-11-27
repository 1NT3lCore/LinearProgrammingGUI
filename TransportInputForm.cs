using System;
using System.Drawing;
using System.Windows.Forms;

namespace LinearProgrammingGUI
{
    public class TransportInputForm : Form
    {
        private NumericUpDown numSuppliers, numConsumers;
        private DataGridView gridCosts, gridSupply, gridDemand;
        private Button btnOK, btnCancel;
        public double[,] Costs { get; private set; }
        public double[] Supply { get; private set; }
        public double[] Demand { get; private set; }
        public bool IsCompleted { get; private set; }

        public TransportInputForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Ввод данных транспортной задачи";
            this.Size = new Size(700, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            int yPos = 20;

            // Количество поставщиков
            Label lblSuppliers = new Label { Text = "Поставщиков:", Location = new Point(20, yPos), Size = new Size(100, 20) };
            numSuppliers = new NumericUpDown { Minimum = 1, Maximum = 10, Value = 3, Location = new Point(130, yPos), Size = new Size(60, 20) };
            this.Controls.Add(lblSuppliers);
            this.Controls.Add(numSuppliers);

            // Количество потребителей
            Label lblConsumers = new Label { Text = "Потребителей:", Location = new Point(220, yPos), Size = new Size(100, 20) };
            numConsumers = new NumericUpDown { Minimum = 1, Maximum = 10, Value = 4, Location = new Point(330, yPos), Size = new Size(60, 20) };
            this.Controls.Add(lblConsumers);
            this.Controls.Add(numConsumers);

            yPos += 30;

            // Кнопка создания таблиц
            Button btnCreate = new Button { Text = "Создать таблицы", Location = new Point(20, yPos), Size = new Size(200, 30) };
            btnCreate.Click += BtnCreate_Click;
            this.Controls.Add(btnCreate);

            yPos += 40;

            // Метка для запасов
            Label lblSupply = new Label { Text = "Запасы поставщиков:", Location = new Point(20, yPos), Size = new Size(150, 20) };
            this.Controls.Add(lblSupply);

            yPos += 25;

            // Таблица для запасов
            gridSupply = new DataGridView
            {
                Location = new Point(20, yPos),
                Size = new Size(200, 150),
                AllowUserToAddRows = false,
                RowHeadersVisible = true
            };
            this.Controls.Add(gridSupply);

            // Метка для потребностей
            Label lblDemand = new Label { Text = "Потребности потребителей:", Location = new Point(250, yPos - 25), Size = new Size(180, 20) };
            this.Controls.Add(lblDemand);

            // Таблица для потребностей
            gridDemand = new DataGridView
            {
                Location = new Point(250, yPos),
                Size = new Size(200, 150),
                AllowUserToAddRows = false,
                RowHeadersVisible = true
            };
            this.Controls.Add(gridDemand);

            yPos += 160;

            // Метка для матрицы стоимостей
            Label lblCosts = new Label { Text = "Матрица стоимостей:", Location = new Point(20, yPos), Size = new Size(150, 20) };
            this.Controls.Add(lblCosts);

            yPos += 25;

            // Таблица для стоимостей
            gridCosts = new DataGridView
            {
                Location = new Point(20, yPos),
                Size = new Size(600, 200),
                AllowUserToAddRows = false,
                RowHeadersVisible = true
            };
            this.Controls.Add(gridCosts);

            yPos += 210;

            // Кнопки OK и Отмена
            btnOK = new Button { Text = "OK", Location = new Point(200, yPos), Size = new Size(100, 30), Enabled = false };
            btnOK.Click += BtnOK_Click;
            this.Controls.Add(btnOK);

            btnCancel = new Button { Text = "Отмена", Location = new Point(320, yPos), Size = new Size(100, 30) };
            btnCancel.Click += (s, e) => { this.Close(); };
            this.Controls.Add(btnCancel);
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            int suppliers = (int)numSuppliers.Value;
            int consumers = (int)numConsumers.Value;

            // Создаем таблицу для запасов
            gridSupply.Columns.Clear();
            gridSupply.Columns.Add("supply", "Запас");
            gridSupply.Columns[0].Width = 150;
            gridSupply.Rows.Clear();
            for (int i = 0; i < suppliers; i++)
            {
                gridSupply.Rows.Add();
                gridSupply.Rows[i].HeaderCell.Value = $"S{i + 1}";
            }

            // Создаем таблицу для потребностей
            gridDemand.Columns.Clear();
            gridDemand.Columns.Add("demand", "Потребность");
            gridDemand.Columns[0].Width = 150;
            gridDemand.Rows.Clear();
            for (int j = 0; j < consumers; j++)
            {
                gridDemand.Rows.Add();
                gridDemand.Rows[j].HeaderCell.Value = $"П{j + 1}";
            }

            // Создаем таблицу для стоимостей
            gridCosts.Columns.Clear();
            for (int j = 0; j < consumers; j++)
            {
                gridCosts.Columns.Add($"cost{j}", $"П{j + 1}");
                gridCosts.Columns[j].Width = 60;
            }
            gridCosts.Rows.Clear();
            for (int i = 0; i < suppliers; i++)
            {
                gridCosts.Rows.Add();
                gridCosts.Rows[i].HeaderCell.Value = $"S{i + 1}";
            }

            btnOK.Enabled = true;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            try
            {
                int suppliers = (int)numSuppliers.Value;
                int consumers = (int)numConsumers.Value;

                // Читаем запасы
                Supply = new double[suppliers];
                for (int i = 0; i < suppliers; i++)
                {
                    if (gridSupply.Rows[i].Cells[0].Value == null)
                        throw new Exception($"Не введен запас для поставщика {i + 1}");
                    Supply[i] = Convert.ToDouble(gridSupply.Rows[i].Cells[0].Value);
                }

                // Читаем потребности
                Demand = new double[consumers];
                for (int j = 0; j < consumers; j++)
                {
                    if (gridDemand.Rows[j].Cells[0].Value == null)
                        throw new Exception($"Не введена потребность для потребителя {j + 1}");
                    Demand[j] = Convert.ToDouble(gridDemand.Rows[j].Cells[0].Value);
                }

                // Читаем матрицу стоимостей
                Costs = new double[suppliers, consumers];
                for (int i = 0; i < suppliers; i++)
                {
                    for (int j = 0; j < consumers; j++)
                    {
                        if (gridCosts.Rows[i].Cells[j].Value == null)
                            throw new Exception($"Не введена стоимость для S{i + 1}->П{j + 1}");
                        Costs[i, j] = Convert.ToDouble(gridCosts.Rows[i].Cells[j].Value);
                    }
                }

                this.IsCompleted = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

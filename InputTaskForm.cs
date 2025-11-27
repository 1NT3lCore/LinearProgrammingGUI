using System;
using System.Drawing;
using System.Windows.Forms;

namespace LinearProgrammingGUI
{
    public class InputTaskForm : Form
    {
        private NumericUpDown numN, numM;
        private ComboBox cmbTaskType;
        private DataGridView gridC, gridA, gridB, gridSigns;
        private Button btnOK, btnCancel;
        public LPTask Task { get; private set; }
        public bool IsCompleted { get; private set; }

        public InputTaskForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Ввод задачи ЛП";
            this.Size = new Size(700, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            int yPos = 20;

            // Количество переменных
            Label lblN = new Label { Text = "Переменных (n):", Location = new Point(20, yPos), Size = new Size(120, 20) };
            numN = new NumericUpDown { Minimum = 1, Maximum = 10, Value = 2, Location = new Point(150, yPos), Size = new Size(60, 20) };
            this.Controls.Add(lblN);
            this.Controls.Add(numN);

            yPos += 30;

            // Количество ограничений
            Label lblM = new Label { Text = "Ограничений (m):", Location = new Point(20, yPos), Size = new Size(120, 20) };
            numM = new NumericUpDown { Minimum = 1, Maximum = 10, Value = 2, Location = new Point(150, yPos), Size = new Size(60, 20) };
            this.Controls.Add(lblM);
            this.Controls.Add(numM);

            yPos += 30;

            // Тип задачи
            Label lblType = new Label { Text = "Тип задачи:", Location = new Point(20, yPos), Size = new Size(120, 20) };
            cmbTaskType = new ComboBox { Location = new Point(150, yPos), Size = new Size(150, 20) };
            cmbTaskType.Items.AddRange(new string[] { "Максимизация", "Минимизация" });
            cmbTaskType.SelectedIndex = 0;
            cmbTaskType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Controls.Add(lblType);
            this.Controls.Add(cmbTaskType);

            yPos += 40;

            // Кнопка создания таблиц
            Button btnCreate = new Button { Text = "Создать таблицы", Location = new Point(20, yPos), Size = new Size(200, 30) };
            btnCreate.Click += BtnCreate_Click;
            this.Controls.Add(btnCreate);

            yPos += 50;

            // Метка для коэффициентов c
            Label lblC = new Label { Text = "Коэффициенты целевой функции (c):", Location = new Point(20, yPos), Size = new Size(300, 20) };
            this.Controls.Add(lblC);

            yPos += 25;

            // Таблица для c
            gridC = new DataGridView
            {
                Location = new Point(20, yPos),
                Size = new Size(300, 60),
                AllowUserToAddRows = false,
                RowHeadersVisible = false
            };
            this.Controls.Add(gridC);

            yPos += 70;

            // Метка для матрицы A
            Label lblA = new Label { Text = "Матрица ограничений (A):", Location = new Point(20, yPos), Size = new Size(300, 20) };
            this.Controls.Add(lblA);

            yPos += 25;

            // Таблица для A
            gridA = new DataGridView
            {
                Location = new Point(20, yPos),
                Size = new Size(400, 120),
                AllowUserToAddRows = false,
                RowHeadersVisible = true
            };
            this.Controls.Add(gridA);

            yPos += 130;

            // Метка для b
            Label lblB = new Label { Text = "Правые части (b):", Location = new Point(20, yPos), Size = new Size(150, 20) };
            this.Controls.Add(lblB);

            yPos += 25;

            // Таблица для b
            gridB = new DataGridView
            {
                Location = new Point(20, yPos),
                Size = new Size(150, 60),
                AllowUserToAddRows = false,
                RowHeadersVisible = false
            };
            this.Controls.Add(gridB);

            // Метка для знаков
            Label lblSigns = new Label { Text = "Знаки:", Location = new Point(200, yPos - 25), Size = new Size(100, 20) };
            this.Controls.Add(lblSigns);

            // Таблица для знаков
            gridSigns = new DataGridView
            {
                Location = new Point(200, yPos),
                Size = new Size(150, 60),
                AllowUserToAddRows = false,
                RowHeadersVisible = false
            };
            this.Controls.Add(gridSigns);

            yPos += 80;

            // Кнопки OK и Отмена
            btnOK = new Button { Text = "OK", Location = new Point(150, yPos), Size = new Size(100, 30) };
            btnOK.Click += BtnOK_Click;
            this.Controls.Add(btnOK);

            btnCancel = new Button { Text = "Отмена", Location = new Point(270, yPos), Size = new Size(100, 30) };
            btnCancel.Click += (s, e) => { this.Close(); };
            this.Controls.Add(btnCancel);
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            int n = (int)numN.Value;
            int m = (int)numM.Value;

            // Создаем таблицу для c
            gridC.Columns.Clear();
            for (int i = 0; i < n; i++)
            {
                gridC.Columns.Add($"c{i + 1}", $"c[{i + 1}]");
                gridC.Columns[i].Width = 50;
            }
            gridC.Rows.Add();

            // Создаем таблицу для A
            gridA.Columns.Clear();
            for (int j = 0; j < n; j++)
            {
                gridA.Columns.Add($"a{j + 1}", $"x{j + 1}");
                gridA.Columns[j].Width = 60;
            }
            for (int i = 0; i < m; i++)
            {
                gridA.Rows.Add();
                gridA.Rows[i].HeaderCell.Value = $"Огр {i + 1}";
            }

            // Создаем таблицу для b
            gridB.Columns.Clear();
            gridB.Columns.Add("b", "b");
            gridB.Columns[0].Width = 100;
            for (int i = 0; i < m; i++)
            {
                gridB.Rows.Add();
            }

            // Создаем таблицу для знаков
            gridSigns.Columns.Clear();
            DataGridViewComboBoxColumn signColumn = new DataGridViewComboBoxColumn
            {
                Name = "sign",
                HeaderText = "Знак",
                Width = 100
            };
            signColumn.Items.AddRange("<=", ">=", "=");
            gridSigns.Columns.Add(signColumn);
            for (int i = 0; i < m; i++)
            {
                gridSigns.Rows.Add();
                gridSigns.Rows[i].Cells[0].Value = "<=";
            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            try
            {
                int n = (int)numN.Value;
                int m = (int)numM.Value;

                LPTask task = new LPTask
                {
                    n = n,
                    m = m,
                    taskType = cmbTaskType.SelectedIndex + 1,
                    c = new double[n],
                    b = new double[m],
                    A = new double[m, n],
                    signs = new string[m]
                };

                // Читаем c
                for (int i = 0; i < n; i++)
                {
                    if (gridC.Rows[0].Cells[i].Value == null)
                        throw new Exception($"Не введено значение c[{i + 1}]");
                    task.c[i] = Convert.ToDouble(gridC.Rows[0].Cells[i].Value);
                }

                // Читаем A
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (gridA.Rows[i].Cells[j].Value == null)
                            throw new Exception($"Не введено значение A[{i + 1},{j + 1}]");
                        task.A[i, j] = Convert.ToDouble(gridA.Rows[i].Cells[j].Value);
                    }
                }

                // Читаем b
                for (int i = 0; i < m; i++)
                {
                    if (gridB.Rows[i].Cells[0].Value == null)
                        throw new Exception($"Не введено значение b[{i + 1}]");
                    task.b[i] = Convert.ToDouble(gridB.Rows[i].Cells[0].Value);
                }

                // Читаем знаки
                for (int i = 0; i < m; i++)
                {
                    if (gridSigns.Rows[i].Cells[0].Value == null)
                        throw new Exception($"Не выбран знак для ограничения {i + 1}");
                    task.signs[i] = gridSigns.Rows[i].Cells[0].Value.ToString();
                }

                this.Task = task;
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
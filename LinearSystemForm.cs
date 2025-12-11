using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LinearProgrammingGUI
{
    public class LinearSystemForm : Form
    {
        private NumericUpDown numEquations, numVariables;
        private DataGridView gridMatrix, gridVector;
        private Button btnOK, btnCancel;
        public double[,] Matrix { get; private set; }
        public double[] Vector { get; private set; }
        public bool IsCompleted { get; private set; }

        public LinearSystemForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Ввод системы линейных уравнений (СЛАУ)";
            this.Size = new Size(1000, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(900, 700);
            this.MaximizeBox = true;

            int yPos = 20;

            // Количество уравнений
            Label lblEquations = new Label 
            { 
                Text = "Количество уравнений (m):", 
                Location = new Point(20, yPos), 
                Size = new Size(150, 20) 
            };
            numEquations = new NumericUpDown 
            { 
                Minimum = 1, 
                Maximum = 20, 
                Value = 3, 
                Location = new Point(180, yPos), 
                Size = new Size(60, 20) 
            };
            this.Controls.Add(lblEquations);
            this.Controls.Add(numEquations);

            // Количество переменных
            Label lblVariables = new Label 
            { 
                Text = "Количество переменных (n):", 
                Location = new Point(280, yPos), 
                Size = new Size(150, 20) 
            };
            numVariables = new NumericUpDown 
            { 
                Minimum = 1, 
                Maximum = 20, 
                Value = 3, 
                Location = new Point(440, yPos), 
                Size = new Size(60, 20) 
            };
            this.Controls.Add(lblVariables);
            this.Controls.Add(numVariables);

            yPos += 30;

            // Кнопка создания таблицы
            Button btnCreate = new Button 
            { 
                Text = "Создать таблицу СЛАУ", 
                Location = new Point(20, yPos), 
                Size = new Size(200, 30) 
            };
            btnCreate.Click += BtnCreate_Click;
            this.Controls.Add(btnCreate);

            yPos += 40;

            // Метка для матрицы
            Label lblMatrix = new Label 
            { 
                Text = "Матрица коэффициентов A (m × n):", 
                Location = new Point(20, yPos), 
                Size = new Size(250, 20) 
            };
            this.Controls.Add(lblMatrix);

            yPos += 25;

            // Таблица для матрицы A
            gridMatrix = new DataGridView
            {
                Location = new Point(20, yPos),
                Size = new Size(800, 400),
                AllowUserToAddRows = false,
                RowHeadersVisible = true,
                ColumnHeadersVisible = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(gridMatrix);

            yPos += 410;

            // Метка для вектора b
            Label lblVector = new Label 
            { 
                Text = "Вектор правых частей b (m × 1):", 
                Location = new Point(20, yPos), 
                Size = new Size(200, 20) 
            };
            this.Controls.Add(lblVector);

            yPos += 25;

            // Таблица для вектора b
            gridVector = new DataGridView
            {
                Location = new Point(20, yPos),
                Size = new Size(200, 200),
                AllowUserToAddRows = false,
                RowHeadersVisible = true,
                ColumnHeadersVisible = false,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            this.Controls.Add(gridVector);

            yPos += 210;

            // Кнопки OK и Отмена
            btnOK = new Button 
            { 
                Text = "Сохранить СЛАУ", 
                Location = new Point(300, yPos), 
                Size = new Size(150, 30), 
                Enabled = false 
            };
            btnOK.Click += BtnOK_Click;
            btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.Controls.Add(btnOK);

            btnCancel = new Button 
            { 
                Text = "Отмена", 
                Location = new Point(470, yPos), 
                Size = new Size(150, 30) 
            };
            btnCancel.Click += (s, e) => { this.Close(); };
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.Controls.Add(btnCancel);
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            int m = (int)numEquations.Value;
            int n = (int)numVariables.Value;

            // Создаем таблицу для матрицы A
            gridMatrix.Columns.Clear();
            for (int j = 0; j < n; j++)
            {
                gridMatrix.Columns.Add($"a{j}", $"x{j + 1}");
                gridMatrix.Columns[j].Width = 80;
            }
            gridMatrix.Rows.Clear();
            for (int i = 0; i < m; i++)
            {
                gridMatrix.Rows.Add();
                gridMatrix.Rows[i].HeaderCell.Value = $"Ур. {i + 1}";
            }

            // Создаем таблицу для вектора b
            gridVector.Columns.Clear();
            gridVector.Columns.Add("b", "b");
            gridVector.Columns[0].Width = 150;
            gridVector.Rows.Clear();
            for (int i = 0; i < m; i++)
            {
                gridVector.Rows.Add();
                gridVector.Rows[i].HeaderCell.Value = $"Ур. {i + 1}";
            }

            btnOK.Enabled = true;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            try
            {
                int m = (int)numEquations.Value;
                int n = (int)numVariables.Value;

                // Читаем матрицу A
                Matrix = new double[m, n];
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (gridMatrix.Rows[i].Cells[j].Value == null)
                            throw new Exception($"Не введен коэффициент A[{i + 1},{j + 1}]");
                        Matrix[i, j] = Convert.ToDouble(gridMatrix.Rows[i].Cells[j].Value);
                    }
                }

                // Читаем вектор b
                Vector = new double[m];
                for (int i = 0; i < m; i++)
                {
                    if (gridVector.Rows[i].Cells[0].Value == null)
                        throw new Exception($"Не введена правая часть b[{i + 1}]");
                    Vector[i] = Convert.ToDouble(gridVector.Rows[i].Cells[0].Value);
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
using System;
using System.Drawing;
using System.Windows.Forms;

namespace LinearProgrammingGUI
{
    public partial class InputTaskForm : Form
    {
        // Публичные свойства для доступа к результатам
        public LPTask Task { get; private set; }
        public bool IsCompleted { get; private set; } = false;

        // Поля для ввода данных
        private NumericUpDown numVariables, numConstraints;
        private RadioButton radioMax, radioMin;
        private Button btnNext, btnCancel;
        private Label lblStep;
        private Panel inputPanel;
        private int currentStep = 0;

        // Данные задачи
        private double[] c; // Коэффициенты целевой функции
        private double[,] A; // Матрица ограничений
        private double[] b; // Правые части ограничений
        private string[] signs; // Знаки ограничений

        public InputTaskForm()
        {
            InitializeComponent();
            ShowStep1();
        }

        private void InitializeComponent()
        {
            this.Text = "Ввод задачи линейного программирования";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 500);
            this.MaximizeBox = false;

            // Заголовок шага
            lblStep = new Label
            {
                Text = "ШАГ 1: ОБЩИЕ ПАРАМЕТРЫ ЗАДАЧИ",
                Font = new Font("Arial", 12, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(850, 30),
                ForeColor = Color.DarkBlue
            };
            this.Controls.Add(lblStep);

            // Панель для ввода данных
            inputPanel = new Panel
            {
                Location = new Point(20, 60),
                Size = new Size(850, 400),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.WhiteSmoke
            };
            this.Controls.Add(inputPanel);

            // Кнопка Далее
            btnNext = new Button
            {
                Text = "Далее",
                Location = new Point(600, 480),
                Size = new Size(120, 35),
                Font = new Font("Arial", 10),
                BackColor = Color.LightGreen
            };
            btnNext.Click += BtnNext_Click;
            this.Controls.Add(btnNext);

            // Кнопка Отмена
            btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(730, 480),
                Size = new Size(120, 35),
                Font = new Font("Arial", 10),
                BackColor = Color.LightCoral
            };
            btnCancel.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };
            this.Controls.Add(btnCancel);
        }

        private void ShowStep1()
        {
            currentStep = 1;
            lblStep.Text = "ШАГ 1: ОБЩИЕ ПАРАМЕТРЫ ЗАДАЧИ";
            inputPanel.Controls.Clear();

            // Количество переменных
            Label lblVariables = new Label
            {
                Text = "Количество переменных (n):",
                Location = new Point(20, 20),
                Size = new Size(200, 25),
                Font = new Font("Arial", 10)
            };
            inputPanel.Controls.Add(lblVariables);

            numVariables = new NumericUpDown
            {
                Location = new Point(250, 20),
                Size = new Size(100, 25),
                Minimum = 1,
                Maximum = 20,
                Value = 2,
                Font = new Font("Arial", 10)
            };
            inputPanel.Controls.Add(numVariables);

            // Количество ограничений
            Label lblConstraints = new Label
            {
                Text = "Количество ограничений (m):",
                Location = new Point(20, 60),
                Size = new Size(200, 25),
                Font = new Font("Arial", 10)
            };
            inputPanel.Controls.Add(lblConstraints);

            numConstraints = new NumericUpDown
            {
                Location = new Point(250, 60),
                Size = new Size(100, 25),
                Minimum = 1,
                Maximum = 20,
                Value = 2,
                Font = new Font("Arial", 10)
            };
            inputPanel.Controls.Add(numConstraints);

            // Тип задачи
            Label lblTaskType = new Label
            {
                Text = "Тип задачи:",
                Location = new Point(20, 100),
                Size = new Size(200, 25),
                Font = new Font("Arial", 10)
            };
            inputPanel.Controls.Add(lblTaskType);

            radioMax = new RadioButton
            {
                Text = "Максимизация (F → max)",
                Location = new Point(250, 100),
                Size = new Size(200, 25),
                Font = new Font("Arial", 10),
                Checked = true
            };
            inputPanel.Controls.Add(radioMax);

            radioMin = new RadioButton
            {
                Text = "Минимизация (F → min)",
                Location = new Point(250, 130),
                Size = new Size(200, 25),
                Font = new Font("Arial", 10)
            };
            inputPanel.Controls.Add(radioMin);
        }

        private void ShowStep2()
        {
            currentStep = 2;
            lblStep.Text = "ШАГ 2: ЦЕЛЕВАЯ ФУНКЦИЯ";
            inputPanel.Controls.Clear();
            btnNext.Text = "Далее";

            int n = (int)numVariables.Value;
            c = new double[n];

            Label lblInstruction = new Label
            {
                Text = $"Введите коэффициенты целевой функции для {n} переменных:",
                Location = new Point(20, 20),
                Size = new Size(800, 25),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            inputPanel.Controls.Add(lblInstruction);

            // Ввод коэффициентов целевой функции
            for (int i = 0; i < n; i++)
            {
                Label lblCoeff = new Label
                {
                    Text = $"c[{i + 1}] (коэффициент при x{i + 1}):",
                    Location = new Point(20, 60 + i * 40),
                    Size = new Size(200, 25),
                    Font = new Font("Arial", 10)
                };
                inputPanel.Controls.Add(lblCoeff);

                NumericUpDown numCoeff = new NumericUpDown
                {
                    Location = new Point(250, 60 + i * 40),
                    Size = new Size(100, 25),
                    DecimalPlaces = 2,
                    Minimum = -1000,
                    Maximum = 1000,
                    Value = 0,
                    Font = new Font("Arial", 10),
                    Tag = i // Сохраняем индекс для доступа
                };
                numCoeff.ValueChanged += (s, e) =>
                {
                    int index = (int)((NumericUpDown)s).Tag;
                    c[index] = (double)((NumericUpDown)s).Value;
                };
                inputPanel.Controls.Add(numCoeff);
            }
        }

        private void ShowStep3()
        {
            currentStep = 3;
            lblStep.Text = "ШАГ 3: ОГРАНИЧЕНИЯ";
            inputPanel.Controls.Clear();
            btnNext.Text = "Завершить";

            int n = (int)numVariables.Value;
            int m = (int)numConstraints.Value;
            A = new double[m, n];
            b = new double[m];
            signs = new string[m];

            Label lblInstruction = new Label
            {
                Text = $"Введите {m} ограничений для {n} переменных:",
                Location = new Point(20, 20),
                Size = new Size(800, 25),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            inputPanel.Controls.Add(lblInstruction);

            // Ввод ограничений
            for (int i = 0; i < m; i++)
            {
                Label lblConstraint = new Label
                {
                    Text = $"Ограничение {i + 1}:",
                    Location = new Point(20, 60 + i * 80),
                    Size = new Size(150, 25),
                    Font = new Font("Arial", 10)
                };
                inputPanel.Controls.Add(lblConstraint);

                // Ввод коэффициентов ограничения
                for (int j = 0; j < n; j++)
                {
                    Label lblVar = new Label
                    {
                        Text = $"x{j + 1}:",
                        Location = new Point(180 + j * 80, 60 + i * 80),
                        Size = new Size(30, 25),
                        Font = new Font("Arial", 10)
                    };
                    inputPanel.Controls.Add(lblVar);

                    NumericUpDown numCoeff = new NumericUpDown
                    {
                        Location = new Point(210 + j * 80, 60 + i * 80),
                        Size = new Size(60, 25),
                        DecimalPlaces = 2,
                        Minimum = -1000,
                        Maximum = 1000,
                        Value = 0,
                        Font = new Font("Arial", 10),
                        Tag = new Tuple<int, int>(i, j) // Индексы i, j
                    };
                    numCoeff.ValueChanged += (s, e) =>
                    {
                        var indices = (Tuple<int, int>)((NumericUpDown)s).Tag;
                        A[indices.Item1, indices.Item2] = (double)((NumericUpDown)s).Value;
                    };
                    inputPanel.Controls.Add(numCoeff);
                }

                // Знак ограничения
                ComboBox cmbSign = new ComboBox
                {
                    Location = new Point(180 + n * 80, 60 + i * 80),
                    Size = new Size(50, 25),
                    Font = new Font("Arial", 10),
                    Tag = i // Индекс ограничения
                };
                cmbSign.Items.AddRange(new string[] { "<=", ">=", "=" });
                cmbSign.SelectedIndex = 0;
                cmbSign.SelectedIndexChanged += (s, e) =>
                {
                    int index = (int)((ComboBox)s).Tag;
                    signs[index] = (string)((ComboBox)s).SelectedItem;
                };
                inputPanel.Controls.Add(cmbSign);
                signs[i] = "<="; // Значение по умолчанию

                // Правая часть
                Label lblRight = new Label
                {
                    Text = "b =",
                    Location = new Point(240 + n * 80, 60 + i * 80),
                    Size = new Size(30, 25),
                    Font = new Font("Arial", 10)
                };
                inputPanel.Controls.Add(lblRight);

                NumericUpDown numRight = new NumericUpDown
                {
                    Location = new Point(275 + n * 80, 60 + i * 80),
                    Size = new Size(80, 25),
                    DecimalPlaces = 2,
                    Minimum = -1000,
                    Maximum = 1000,
                    Value = 0,
                    Font = new Font("Arial", 10),
                    Tag = i // Индекс ограничения
                };
                numRight.ValueChanged += (s, e) =>
                {
                    int index = (int)((NumericUpDown)s).Tag;
                    b[index] = (double)((NumericUpDown)s).Value;
                };
                inputPanel.Controls.Add(numRight);
            }
        }

        private void BtnNext_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentStep == 1)
                {
                    // Переход ко второму шагу
                    ShowStep2();
                }
                else if (currentStep == 2)
                {
                    // Проверка целевой функции
                    bool allZero = true;
                    for (int i = 0; i < c.Length; i++)
                    {
                        if (Math.Abs(c[i]) > 0.001)
                        {
                            allZero = false;
                            break;
                        }
                    }

                    if (allZero)
                    {
                        DialogResult result = MessageBox.Show(
                            "Все коэффициенты целевой функции равны нулю. Продолжить?",
                            "Предупреждение",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning);

                        if (result == DialogResult.No)
                        {
                            return;
                        }
                    }

                    // Переход к третьему шагу
                    ShowStep3();
                }
                else if (currentStep == 3)
                {
                    // Проверка ограничений
                    for (int i = 0; i < signs.Length; i++)
                    {
                        if (string.IsNullOrEmpty(signs[i]))
                        {
                            signs[i] = "<="; // Значение по умолчанию
                        }
                    }

                    // Создание задачи
                    Task = new LPTask
                    {
                        n = (int)numVariables.Value,
                        m = (int)numConstraints.Value,
                        taskType = radioMax.Checked ? 1 : 2,
                        c = c,
                        A = A,
                        b = b,
                        signs = signs
                    };

                    Console.WriteLine($"Task created: n={Task.n}, m={Task.m}, type={Task.taskType}");
                    Console.WriteLine($"c length: {Task.c?.Length}, A dimensions: {Task.A?.GetLength(0)}x{Task.A?.GetLength(1)}");

                    // Установка флагов
                    IsCompleted = true;
                    this.DialogResult = DialogResult.OK;

                    Console.WriteLine($"IsCompleted set to: {IsCompleted}");
                    Console.WriteLine($"DialogResult set to: {this.DialogResult}");

                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"Error in BtnNext_Click: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }

        // Метод для быстрой отладки - создание тестовой задачи
        public void CreateTestTask()
        {
            // Тестовая задача: максимизация 3x1 + 5x2
            // Ограничения: x1 ≤ 4, 2x2 ≤ 12, 3x1 + 2x2 ≤ 18
            Task = new LPTask
            {
                n = 2,
                m = 3,
                taskType = 1, // максимизация
                c = new double[] { 3, 5 },
                A = new double[,] { { 1, 0 }, { 0, 2 }, { 3, 2 } },
                b = new double[] { 4, 12, 18 },
                signs = new string[] { "<=", "<=", "<=" }
            };

            IsCompleted = true;
        }
    }
}
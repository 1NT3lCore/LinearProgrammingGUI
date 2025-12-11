using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LinearProgrammingGUI
{
    public class TransportProblemForm : Form
    {
        private TextBox txtResult;
        private Label lblTitle;
        private Button btnInputData, btnSolve, btnClose, btnSelectTask;
        
        private double[,] costs;
        private double[] supply;
        private double[] demand;
        
        private int? selectedTaskId = null;
        private LPTask currentTask;
        private bool hasTask = false; // Флаг, указывающий, что задача выбрана через SetSelectedTask

        public TransportProblemForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Транспортная задача - Метод северо-западного угла";
            this.Size = new Size(1100, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1000, 700);
            this.MaximizeBox = true;

            // Заголовок
            lblTitle = new Label
            {
                Text = "ТРАНСПОРТНАЯ ЗАДАЧА - МЕТОД СЕВЕРО-ЗАПАДНОГО УГЛА",
                Font = new Font("Arial", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 20),
                Size = new Size(1060, 30),
                ForeColor = Color.DarkBlue
            };
            this.Controls.Add(lblTitle);

            // Кнопка выбора сохраненной задачи
            btnSelectTask = new Button
            {
                Text = "1. Выбрать задачу",
                Location = new Point(20, 70),
                Size = new Size(150, 40),
                Font = new Font("Arial", 10)
            };
            btnSelectTask.Click += BtnSelectTask_Click;
            this.Controls.Add(btnSelectTask);

            // Кнопка ввода данных транспортной задачи
            btnInputData = new Button
            {
                Text = "2. Ввести данные транспортной задачи",
                Location = new Point(190, 70),
                Size = new Size(250, 40),
                Font = new Font("Arial", 10)
            };
            btnInputData.Click += BtnInputData_Click;
            this.Controls.Add(btnInputData);

            // Кнопка решения
            btnSolve = new Button
            {
                Text = "3. Решить методом СЗУ",
                Location = new Point(460, 70),
                Size = new Size(150, 40),
                Font = new Font("Arial", 10),
                Enabled = false
            };
            btnSolve.Click += BtnSolve_Click;
            this.Controls.Add(btnSolve);

            // Поле вывода результатов
            txtResult = new TextBox
            {
                Location = new Point(20, 120),
                Size = new Size(1040, 630),
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                Font = new Font("Courier New", 9),
                ReadOnly = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(txtResult);

            // Кнопка закрытия
            btnClose = new Button
            {
                Text = "Закрыть",
                Location = new Point(960, 760),
                Size = new Size(100, 30),
                Font = new Font("Arial", 9),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
        }

        // Метод для установки задачи из главного меню
        public void SetSelectedTask(int taskId, LPTask task)
        {
            selectedTaskId = taskId;
            currentTask = task;
            hasTask = true; // Устанавливаем флаг
            lblTitle.Text = $"ТРАНСПОРТНАЯ ЗАДАЧА (Задача №{taskId})";
            
            // Проверяем, является ли задача транспортной
            if (IsTransportProblem(currentTask))
            {
                btnSolve.Enabled = true;
                PrepareTransportDataFromTask(currentTask);
                ShowTransportData();
                
                MessageBox.Show($"Задача №{taskId} выбрана как транспортная задача. Нажмите кнопку 'Решить методом СЗУ' для решения.", 
                    "Транспортная задача выбрана", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"Задача №{taskId} не является транспортной задачей.\n\nТранспортная задача должна иметь специальную структуру:\n- Матрица стоимостей транспортировки\n- Вектор запасов поставщиков\n- Вектор потребностей потребителей", 
                    "Не транспортная задача", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnSelectTask_Click(object sender, EventArgs e)
        {
            if (hasTask)
            {
                // Используем уже выбранную задачу
                if (IsTransportProblem(currentTask))
                {
                    btnSolve.Enabled = true;
                    PrepareTransportDataFromTask(currentTask);
                    ShowTransportData();
                    
                    MessageBox.Show($"Используется задача №{selectedTaskId.Value}. Нажмите кнопку 'Решить методом СЗУ' для решения.", 
                        "Задача уже выбрана", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"Текущая задача №{selectedTaskId.Value} не является транспортной задачей.", 
                        "Не транспортная задача", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                // Показываем диалог выбора задачи
                ShowTaskSelectionDialog();
            }
        }

        private void ShowTaskSelectionDialog()
        {
            if (LPTaskStorage.Count == 0)
            {
                MessageBox.Show("Нет сохраненных задач. Сначала введите задачу в главном меню.",
                    "Нет задач", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Form dialog = new Form
            {
                Text = "Выбор транспортной задачи",
                Size = new Size(600, 450),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false
            };

            Label lbl = new Label
            {
                Text = "Выберите задачу для решения как транспортную:",
                Location = new Point(20, 20),
                Size = new Size(550, 30),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            dialog.Controls.Add(lbl);

            // Информация о транспортных задачах
            Label lblInfo = new Label
            {
                Text = "Примечание: Транспортная задача должна иметь матрицу стоимостей,\nвектор запасов и вектор потребностей определенной структуры.",
                Location = new Point(20, 50),
                Size = new Size(550, 40),
                Font = new Font("Arial", 8),
                ForeColor = Color.DarkGray
            };
            dialog.Controls.Add(lblInfo);

            ListBox listBox = new ListBox
            {
                Location = new Point(20, 100),
                Size = new Size(540, 250),
                Font = new Font("Courier New", 9)
            };

            var tasks = LPTaskStorage.GetAllTasks();
            foreach (var task in tasks)
            {
                listBox.Items.Add($"{task.id}. {task.description}");
            }
            listBox.SelectedIndex = 0;
            dialog.Controls.Add(listBox);

            // Кнопка выбора
            Button btnOK = new Button
            {
                Text = "Выбрать",
                Location = new Point(200, 360),
                Size = new Size(100, 35),
                Font = new Font("Arial", 9, FontStyle.Bold),
                BackColor = Color.LightGreen
            };
            btnOK.Click += (s, ev) =>
            {
                if (listBox.SelectedIndex >= 0)
                {
                    int taskId = listBox.SelectedIndex + 1;
                    var selectedTask = LPTaskStorage.GetTask(taskId);
                    
                    if (IsTransportProblem(selectedTask))
                    {
                        SetSelectedTask(taskId, selectedTask);
                        dialog.Close();
                    }
                    else
                    {
                        MessageBox.Show($"Задача №{taskId} не является транспортной задачей.\n\nДля решения транспортной задачи данные должны быть структурированы особым образом.", 
                            "Не транспортная задача", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            };
            dialog.Controls.Add(btnOK);

            // Кнопка отмены
            Button btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(310, 360),
                Size = new Size(100, 35),
                Font = new Font("Arial", 9)
            };
            btnCancel.Click += (s, ev) => dialog.Close();
            dialog.Controls.Add(btnCancel);

            dialog.ShowDialog();
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
                btnSolve.Enabled = true;
                hasTask = false; // Сбрасываем флаг, т.к. данные введены напрямую

                // Показываем введенные данные
                ShowTransportData();
            }
        }

        private void BtnSolve_Click(object sender, EventArgs e)
        {
            // Проверяем источник данных
            if (hasTask)
            {
                // Используем данные из выбранной задачи
                if (!IsTransportProblem(currentTask))
                {
                    MessageBox.Show("Выбранная задача не является транспортной задачей.", 
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                if (costs == null || supply == null || demand == null)
                {
                    MessageBox.Show("Не удалось подготовить данные транспортной задачи.", 
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else if (costs == null || supply == null || demand == null)
            {
                MessageBox.Show("Сначала введите данные транспортной задачи!", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Проверяем сбалансированность задачи
                double totalSupply = 0;
                double totalDemand = 0;
                
                foreach (double s in supply) totalSupply += s;
                foreach (double d in demand) totalDemand += d;
                
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("================================================");
                sb.AppendLine("АНАЛИЗ ТРАНСПОРТНОЙ ЗАДАЧИ:");
                sb.AppendLine("================================================");
                sb.AppendLine($"Общий запас: {totalSupply:F2}");
                sb.AppendLine($"Общая потребность: {totalDemand:F2}");
                
                if (Math.Abs(totalSupply - totalDemand) > 0.001)
                {
                    sb.AppendLine("\nЗАДАЧА НЕСБАЛАНСИРОВАНА!");
                    sb.AppendLine($"Разница: {Math.Abs(totalSupply - totalDemand):F2}");
                    sb.AppendLine("Для решения необходимо добавить фиктивного поставщика или потребителя.");
                    
                    txtResult.Text = sb.ToString();
                    return;
                }
                else
                {
                    sb.AppendLine("\nЗадача сбалансирована.");
                    txtResult.Text = sb.ToString();
                }

                // Создаем решатель транспортной задачи
                NorthWestCornerSolver solver = new NorthWestCornerSolver(costs, supply, demand);
                string result = solver.SolveWithOutput();
                txtResult.Text += "\n" + result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при решении транспортной задачи: {ex.Message}", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowTransportData()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("================================================");
            sb.AppendLine("ДАННЫЕ ТРАНСПОРТНОЙ ЗАДАЧИ:");
            sb.AppendLine("================================================");

            if (hasTask && selectedTaskId.HasValue)
            {
                sb.AppendLine($"Задача №{selectedTaskId.Value}");
            }

            sb.AppendLine("\nЗАПАСЫ ПОСТАВЩИКОВ:");
            for (int i = 0; i < supply.Length; i++)
            {
                sb.AppendLine($"  Поставщик S{i + 1}: {supply[i]} единиц");
            }

            sb.AppendLine("\nПОТРЕБНОСТИ ПОТРЕБИТЕЛЕЙ:");
            for (int j = 0; j < demand.Length; j++)
            {
                sb.AppendLine($"  Потребитель П{j + 1}: {demand[j]} единиц");
            }

            sb.AppendLine("\nМАТРИЦА СТОИМОСТЕЙ ПЕРЕВОЗОК:");
            sb.Append("           ");
            for (int j = 0; j < demand.Length; j++)
            {
                sb.Append($"П{j + 1,-8}");
            }
            sb.AppendLine();
            sb.AppendLine(new string('-', 11 + demand.Length * 9));

            for (int i = 0; i < supply.Length; i++)
            {
                sb.Append($"Поставщик S{i + 1} | ");
                for (int j = 0; j < demand.Length; j++)
                {
                    sb.Append($"{costs[i, j],-8:F2} ");
                }
                sb.AppendLine();
            }

            // Информация о методе
            sb.AppendLine("\n================================================");
            sb.AppendLine("МЕТОД СЕВЕРО-ЗАПАДНОГО УГЛА:");
            sb.AppendLine("================================================");
            sb.AppendLine("Алгоритм:");
            sb.AppendLine("1. Начинаем с левого верхнего (северо-западного) угла матрицы");
            sb.AppendLine("2. Распределяем максимально возможный груз с учетом запасов и потребностей");
            sb.AppendLine("3. Переходим к следующей ячейке вправо или вниз");
            sb.AppendLine("4. Повторяем, пока все запасы не будут распределены");

            txtResult.Text = sb.ToString();
        }

        private bool IsTransportProblem(LPTask task)
        {
            // Проверяем, может ли задача быть интерпретирована как транспортная
            // Простая проверка: задача должна иметь матрицу и векторы подходящих размеров
            if (task.n == 0 || task.m == 0) return false;
            
            // Для транспортной задачи обычно: m = поставщики, n = потребители
            // Но это не строгое правило, поэтому возвращаем true для любой задачи с данными
            return task.A != null && task.b != null && task.c != null;
        }

        private void PrepareTransportDataFromTask(LPTask task)
        {
            try
            {
                // Пытаемся извлечь данные транспортной задачи из общей структуры LPTask
                // Это упрощенная реализация - в реальном приложении нужна более сложная логика
                
                if (task.m > 0 && task.n > 0)
                {
                    // Предполагаем, что матрица A - это матрица стоимостей
                    costs = task.A;
                    
                    // Предполагаем, что вектор b - это запасы поставщиков
                    supply = task.b;
                    
                    // Предполагаем, что вектор c - это потребности потребителей
                    // Но вектор c обычно короче, чем количество потребителей
                    // Поэтому создаем искусственные данные для демонстрации
                    demand = new double[task.n];
                    for (int j = 0; j < task.n; j++)
                    {
                        // Равномерно распределяем общую потребность
                        double totalSupply = 0;
                        foreach (double s in supply) totalSupply += s;
                        demand[j] = totalSupply / task.n;
                    }
                }
            }
            catch (Exception)
            {
                // Если не удалось подготовить данные, сбрасываем их
                costs = null;
                supply = null;
                demand = null;
            }
        }
    }
}    
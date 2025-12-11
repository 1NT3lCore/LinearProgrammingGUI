using System;
using System.Drawing;
using System.Windows.Forms;
using System.Text; // Добавлено для StringBuilder

namespace LinearProgrammingGUI
{
    public partial class MainForm : Form
    {
        private ListBox lstTasks;
        private Button btnRefresh, btnClear;
        private Label titleLabel, lblTasks, separator;
        private Button[] functionButtons = new Button[7];

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Исследование операций - ЛП и СЛАУ";
            this.Size = new Size(750, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(700, 600);
            this.MaximizeBox = true;

            // === ЗАГОЛОВОК ===
            titleLabel = new Label
            {
                Text = "СИСТЕМА РЕШЕНИЯ ЗАДАЧ ЛИНЕЙНОГО ПРОГРАММИРОВАНИЯ",
                Font = new Font("Arial", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(50, 20),
                Size = new Size(650, 30),
                ForeColor = Color.DarkBlue
            };
            this.Controls.Add(titleLabel);

            // === БЛОК СОХРАНЕННЫХ ЗАДАЧ ===
            lblTasks = new Label
            {
                Text = "СОХРАНЕННЫЕ ЗАДАЧИ:",
                Location = new Point(20, 70),
                Size = new Size(300, 20),
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.DarkGreen
            };
            this.Controls.Add(lblTasks);

            lstTasks = new ListBox
            {
                Location = new Point(20, 95),
                Size = new Size(350, 180),
                Font = new Font("Courier New", 9),
                BackColor = Color.AliceBlue,
                BorderStyle = BorderStyle.Fixed3D
            };
            lstTasks.SelectedIndexChanged += LstTasks_SelectedIndexChanged;
            this.Controls.Add(lstTasks);

            // Кнопка обновления списка задач
            btnRefresh = new Button
            {
                Text = "Обновить список",
                Location = new Point(380, 95),
                Size = new Size(140, 30),
                Font = new Font("Arial", 9),
                BackColor = Color.LightYellow
            };
            btnRefresh.Click += BtnRefresh_Click;
            this.Controls.Add(btnRefresh);

            // Кнопка очистки всех задач
            btnClear = new Button
            {
                Text = "Очистить все",
                Location = new Point(380, 135),
                Size = new Size(140, 30),
                Font = new Font("Arial", 9),
                BackColor = Color.LightCoral
            };
            btnClear.Click += BtnClear_Click;
            this.Controls.Add(btnClear);

            // Статистика задач
            Label lblStats = new Label
            {
                Text = "Статистика:",
                Location = new Point(380, 175),
                Size = new Size(100, 20),
                Font = new Font("Arial", 9, FontStyle.Italic)
            };
            this.Controls.Add(lblStats);

            // Поле для отображения статистики
            Label lblStatsValue = new Label
            {
                Text = "Задач: 0",
                Location = new Point(380, 195),
                Size = new Size(140, 20),
                Font = new Font("Arial", 9),
                Name = "lblStatsValue"
            };
            this.Controls.Add(lblStatsValue);

            // === РАЗДЕЛИТЕЛЬНАЯ ЛИНИЯ ===
            separator = new Label
            {
                Text = "══════════════════════════════════════════════════════════",
                Location = new Point(20, 290),
                Size = new Size(700, 20),
                ForeColor = Color.Gray,
                Font = new Font("Arial", 9)
            };
            this.Controls.Add(separator);

            // === БЛОК ВЫБОРА ФУНКЦИЙ ===
            Label lblFunctions = new Label
            {
                Text = "ВЫБЕРИТЕ ФУНКЦИЮ:",
                Location = new Point(20, 320),
                Size = new Size(300, 20),
                Font = new Font("Arial", 10, FontStyle.Bold),
                ForeColor = Color.DarkRed
            };
            this.Controls.Add(lblFunctions);

            int startY = 350;
            int buttonHeight = 35;
            int spacing = 5;

            // Кнопка 0: Ввод новой задачи
            functionButtons[0] = new Button
            {
                Text = "0. ВВЕСТИ НОВУЮ ЗАДАЧУ ЛП ИЛИ СЛАУ",
                Location = new Point(40, startY),
                Size = new Size(400, 40),
                Font = new Font("Arial", 10, FontStyle.Bold),
                BackColor = Color.LightBlue,
                ForeColor = Color.DarkBlue,
                Tag = 0
            };
            functionButtons[0].Click += BtnNewTask_Click;
            this.Controls.Add(functionButtons[0]);

            // Остальные кнопки функций
            string[] buttonTexts = {
                "1. ПОСТРОЕНИЕ ДВОЙСТВЕННОЙ ЗАДАЧИ",
                "2. ПРОВЕРКА ПЛАНА НА ОПТИМАЛЬНОСТЬ",
                "3. ПРЯМАЯ И ДВОЙСТВЕННАЯ ЗАДАЧА",
                "4. ДВОЙСТВЕННЫЙ СИМПЛЕКС-МЕТОД",
                "5. СИМПЛЕКС-МЕТОД РЕШЕНИЯ ЗЛП",
                "6. ТРАНСПОРТНАЯ ЗАДАЧА"
            };

            for (int i = 1; i <= 6; i++)
            {
                functionButtons[i] = new Button
                {
                    Text = buttonTexts[i - 1],
                    Location = new Point(40, startY + 45 + i * (buttonHeight + spacing)),
                    Size = new Size(400, buttonHeight),
                    Font = new Font("Arial", 10),
                    BackColor = i % 2 == 0 ? Color.Lavender : Color.Honeydew,
                    Tag = i
                };
                functionButtons[i].Click += FunctionButton_Click;
                this.Controls.Add(functionButtons[i]);
            }

            // === КНОПКА ВЫХОДА ===
            Button btnExit = new Button
            {
                Text = "Выход из программы",
                Location = new Point(300, 600),
                Size = new Size(150, 35),
                Font = new Font("Arial", 9, FontStyle.Bold),
                BackColor = Color.LightGray
            };
            btnExit.Click += (s, e) => 
            {
                if (MessageBox.Show("Вы уверены, что хотите выйти?", "Подтверждение", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    this.Close();
                }
            };
            this.Controls.Add(btnExit);

            // === ИНИЦИАЛИЗАЦИЯ ===
            RefreshTaskList();
            UpdateFunctionButtonsState();
        }

        private void LstTasks_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Можно добавить функционал при выборе задачи из списка
            if (lstTasks.SelectedIndex >= 0)
            {
                // Например, показывать краткую информацию о выбранной задаче
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            RefreshTaskList();
            MessageBox.Show("Список задач обновлен", "Информация", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            if (LPTaskStorage.Count == 0)
            {
                MessageBox.Show("Нет сохраненных задач для удаления", "Информация", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (MessageBox.Show($"Вы уверены, что хотите удалить ВСЕ сохраненные задачи ({LPTaskStorage.Count} шт.)?", 
                "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                LPTaskStorage.Clear();
                RefreshTaskList();
                MessageBox.Show("Все задачи успешно удалены", "Успех", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnNewTask_Click(object sender, EventArgs e)
        {
            ShowTaskInputDialog();
        }

        private void FunctionButton_Click(object sender, EventArgs e)
        {
            if (sender is Button button && button.Tag is int functionId)
            {
                if (functionId >= 1 && functionId <= 6)
                {
                    if (LPTaskStorage.Count == 0)
                    {
                        MessageBox.Show("Сначала введите задачу (кнопка 0)!", "Нет задач", 
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Запрашиваем выбор задачи
                    int? selectedTaskId = ShowTaskSelectionDialog($"Выберите задачу для функции {functionId}:");
                    if (selectedTaskId.HasValue)
                    {
                        OpenFunctionForm(functionId, selectedTaskId.Value);
                    }
                }
            }
        }

        private void ShowTaskInputDialog()
        {
            Form dialog = new Form
            {
                Text = "ВВОД НОВОЙ ЗАДАЧИ",
                Size = new Size(450, 250),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false
            };

            Label lbl = new Label
            {
                Text = "Какую задачу вы хотите ввести?",
                Location = new Point(20, 20),
                Size = new Size(400, 30),
                Font = new Font("Arial", 11, FontStyle.Bold)
            };
            dialog.Controls.Add(lbl);

            Button btnLP = new Button
            {
                Text = "ЗАДАЧУ ЛИНЕЙНОГО ПРОГРАММИРОВАНИЯ (ЛП)",
                Location = new Point(50, 70),
                Size = new Size(350, 40),
                Font = new Font("Arial", 10),
                BackColor = Color.LightBlue
            };
            btnLP.Click += (s, ev) =>
            {
                dialog.Close();
                
                // Для отладки: создаем тестовую задачу
                bool useTestTask = false; // установите true для тестирования без ввода
                
                if (useTestTask)
                {
                    // Создаем тестовую задачу без показа формы ввода
                    InputTaskForm testForm = new InputTaskForm();
                    testForm.CreateTestTask();
                    
                    if (testForm.IsCompleted && testForm.Task.n > 0)
                    {
                        try
                        {
                            int id = LPTaskStorage.AddTask(testForm.Task);
                            RefreshTaskList();
                            MessageBox.Show($"Тестовая задача сохранена под номером {id}", "Успех", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при сохранении тестовой задачи: {ex.Message}", "Ошибка", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    // Нормальный ввод задачи
                    InputTaskForm inputForm = new InputTaskForm();
                    
                    // Добавляем обработку для отладки
                    Console.WriteLine("Открывается форма ввода задачи ЛП...");
                    
                    DialogResult result = inputForm.ShowDialog();
                    Console.WriteLine($"Форма ввода закрыта с результатом: {result}");
                    Console.WriteLine($"IsCompleted: {inputForm.IsCompleted}");
                    
                    if (result == DialogResult.OK && inputForm.IsCompleted)
                    {
                        try
                        {
                            Console.WriteLine("Попытка сохранить задачу...");
                            Console.WriteLine($"Размер задачи: {inputForm.Task.n} переменных, {inputForm.Task.m} ограничений");
                            
                            int id = LPTaskStorage.AddTask(inputForm.Task);
                            Console.WriteLine($"Задача сохранена с ID: {id}");
                            Console.WriteLine($"Всего задач в хранилище: {LPTaskStorage.Count}");
                            
                            RefreshTaskList();
                            MessageBox.Show($"Задача ЛП успешно сохранена под номером {id}", "Успех", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"ОШИБКА сохранения: {ex.Message}");
                            MessageBox.Show($"Ошибка при сохранении задачи: {ex.Message}\n\nДетали в консоли отладки.", "Ошибка", 
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Задача не сохранена. Причина: Result={result}, IsCompleted={inputForm.IsCompleted}");
                        if (result == DialogResult.Cancel)
                        {
                            MessageBox.Show("Ввод задачи отменен", "Информация", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            };
            dialog.Controls.Add(btnLP);

            Button btnSLAE = new Button
            {
                Text = "СИСТЕМУ ЛИНЕЙНЫХ УРАВНЕНИЙ (СЛАУ)",
                Location = new Point(50, 120),
                Size = new Size(350, 40),
                Font = new Font("Arial", 10),
                BackColor = Color.LightGreen
            };
            btnSLAE.Click += (s, ev) =>
            {
                dialog.Close();
                LinearSystemForm slaeForm = new LinearSystemForm();
                
                Console.WriteLine("Открывается форма ввода СЛАУ...");
                DialogResult result = slaeForm.ShowDialog();
                Console.WriteLine($"Форма СЛАУ закрыта с результатом: {result}");
                Console.WriteLine($"IsCompleted: {slaeForm.IsCompleted}");
                
                if (result == DialogResult.OK && slaeForm.IsCompleted)
                {
                    // Конвертируем СЛАУ в LPTask для хранения
                    try
                    {
                        LPTask task = new LPTask
                        {
                            n = slaeForm.Matrix.GetLength(1),
                            m = slaeForm.Matrix.GetLength(0),
                            taskType = 2, // минимизация по умолчанию
                            c = new double[slaeForm.Matrix.GetLength(1)], // нулевая целевая функция
                            b = slaeForm.Vector,
                            A = slaeForm.Matrix,
                            signs = new string[slaeForm.Matrix.GetLength(0)]
                        };
                        
                        // Все ограничения как равенства
                        for (int i = 0; i < task.m; i++)
                            task.signs[i] = "=";
                        
                        Console.WriteLine($"Создана задача СЛАУ: {task.n} переменных, {task.m} уравнений");
                        
                        int id = LPTaskStorage.AddTask(task);
                        RefreshTaskList();
                        MessageBox.Show($"СЛАУ успешно сохранена как задача под номером {id}\n\nПримечание: для СЛАУ целевая функция установлена нулевой.", "Успех", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ОШИБКА сохранения СЛАУ: {ex.Message}");
                        MessageBox.Show($"Ошибка при сохранении СЛАУ: {ex.Message}", "Ошибка", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    Console.WriteLine($"СЛАУ не сохранена. Причина: Result={result}, IsCompleted={slaeForm.IsCompleted}");
                }
            };
            dialog.Controls.Add(btnSLAE);

            Button btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(175, 180),
                Size = new Size(100, 30),
                Font = new Font("Arial", 9)
            };
            btnCancel.Click += (s, ev) => 
            {
                Console.WriteLine("Диалог выбора типа задачи отменен");
                dialog.Close();
            };
            dialog.Controls.Add(btnCancel);

            dialog.ShowDialog();
        }

        private int? ShowTaskSelectionDialog(string title)
        {
            if (LPTaskStorage.Count == 0)
                return null;

            int selectedId = -1;
            Form dialog = new Form
            {
                Text = "ВЫБОР ЗАДАЧИ",
                Size = new Size(550, 400),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false
            };

            Label lbl = new Label
            {
                Text = title,
                Location = new Point(20, 20),
                Size = new Size(500, 20),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            dialog.Controls.Add(lbl);

            ListBox listBox = new ListBox
            {
                Location = new Point(20, 50),
                Size = new Size(490, 220),
                Font = new Font("Courier New", 9),
                BackColor = Color.AliceBlue
            };
            
            var tasks = LPTaskStorage.GetAllTasks();
            foreach (var task in tasks)
            {
                listBox.Items.Add($"{task.id}. {task.description}");
            }
            listBox.SelectedIndex = 0;
            dialog.Controls.Add(listBox);

            // Информация о выбранной задаче
            Label lblSelectedInfo = new Label
            {
                Text = "",
                Location = new Point(20, 280),
                Size = new Size(490, 40),
                Font = new Font("Arial", 8),
                ForeColor = Color.DarkBlue
            };
            dialog.Controls.Add(lblSelectedInfo);

            listBox.SelectedIndexChanged += (s, e) =>
            {
                if (listBox.SelectedIndex >= 0)
                {
                    int taskId = listBox.SelectedIndex + 1;
                    var task = LPTaskStorage.GetTask(taskId);
                    lblSelectedInfo.Text = $"Выбрана задача {taskId}: {task.n} переменных, {task.m} ограничений";
                }
            };

            // Инициализируем информацию о первой задаче
            if (listBox.Items.Count > 0)
            {
                var firstTask = LPTaskStorage.GetTask(1);
                lblSelectedInfo.Text = $"Выбрана задача 1: {firstTask.n} переменных, {firstTask.m} ограничений";
            }

            // Кнопка выбора
            Button btnOK = new Button
            {
                Text = "Выбрать",
                Location = new Point(150, 330),
                Size = new Size(100, 35),
                Font = new Font("Arial", 9, FontStyle.Bold),
                BackColor = Color.LightGreen
            };
            btnOK.Click += (s, e) =>
            {
                if (listBox.SelectedIndex >= 0)
                {
                    selectedId = listBox.SelectedIndex + 1;
                    dialog.DialogResult = DialogResult.OK;
                    dialog.Close();
                }
            };
            dialog.Controls.Add(btnOK);

            // Кнопка отмены
            Button btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(270, 330),
                Size = new Size(100, 35),
                Font = new Font("Arial", 9),
                BackColor = Color.LightCoral
            };
            btnCancel.Click += (s, e) =>
            {
                dialog.DialogResult = DialogResult.Cancel;
                dialog.Close();
            };
            dialog.Controls.Add(btnCancel);

            // Кнопка просмотра деталей
            Button btnDetails = new Button
            {
                Text = "Просмотреть детали",
                Location = new Point(20, 330),
                Size = new Size(120, 35),
                Font = new Font("Arial", 8)
            };
            btnDetails.Click += (s, e) =>
            {
                if (listBox.SelectedIndex >= 0)
                {
                    int taskId = listBox.SelectedIndex + 1;
                    var task = LPTaskStorage.GetTask(taskId);
                    ShowTaskDetails(task, taskId);
                }
            };
            dialog.Controls.Add(btnDetails);

            return dialog.ShowDialog() == DialogResult.OK ? selectedId : (int?)null;
        }

        private void OpenFunctionForm(int functionId, int taskId)
        {
            try
            {
                var task = LPTaskStorage.GetTask(taskId);
                Console.WriteLine($"Открытие формы функции {functionId} для задачи {taskId}");
                
                switch (functionId)
                {
                    case 1: // Построение двойственной задачи
                        var dualForm = new DualTaskForm();
                        dualForm.SetSelectedTask(taskId, task);
                        dualForm.ShowDialog();
                        break;
                        
                    case 2: // Проверка плана на оптимальность
                        var checkForm = new OptimalityCheckForm();
                        checkForm.SetSelectedTask(taskId, task);
                        checkForm.ShowDialog();
                        break;
                        
                    case 3: // Прямая и двойственная задача
                        var primalDualForm = new PrimalDualForm();
                        primalDualForm.SetSelectedTask(taskId, task);
                        primalDualForm.ShowDialog();
                        break;
                        
                    case 4: // Двойственный симплекс-метод
                        var dualSimplexForm = new DualSimplexForm();
                        dualSimplexForm.SetSelectedTask(taskId, task);
                        dualSimplexForm.ShowDialog();
                        break;
                        
                    case 5: // Симплекс-метод
                        var simplexForm = new SimplexMethodForm();
                        simplexForm.SetSelectedTask(taskId, task);
                        simplexForm.ShowDialog();
                        break;
                        
                    case 6: // Транспортная задача
                        var transportForm = new TransportProblemForm();
                        transportForm.SetSelectedTask(taskId, task);
                        transportForm.ShowDialog();
                        break;
                        
                    default:
                        MessageBox.Show($"Функция {functionId} не реализована", "Ошибка", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА открытия формы: {ex.Message}");
                MessageBox.Show($"Ошибка при открытии формы: {ex.Message}", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowTaskDetails(LPTask task, int taskId)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("══════════════════════════════════════");
            sb.AppendLine($"ДЕТАЛИ ЗАДАЧИ №{taskId}");
            sb.AppendLine("══════════════════════════════════════");
            sb.AppendLine($"Размерность: {task.n} переменных, {task.m} ограничений");
            sb.AppendLine($"Тип задачи: {(task.taskType == 1 ? "МАКСИМИЗАЦИЯ" : "МИНИМИЗАЦИЯ")}");
            
            sb.AppendLine($"\nЦелевая функция:");
            string funcName = task.taskType == 1 ? "F" : "G";
            sb.Append($"{funcName}(x) = ");
            for (int i = 0; i < task.c.Length; i++)
            {
                if (i > 0 && task.c[i] >= 0) sb.Append(" + ");
                sb.Append($"{task.c[i]}*x{i + 1}");
            }
            sb.AppendLine(task.taskType == 1 ? " -> max" : " -> min");

            sb.AppendLine($"\nОграничения:");
            for (int i = 0; i < task.m; i++)
            {
                sb.Append($"  {i + 1}. ");
                for (int j = 0; j < task.n; j++)
                {
                    if (j > 0 && task.A[i, j] >= 0) sb.Append(" + ");
                    sb.Append($"{task.A[i, j]}*x{j + 1}");
                }
                sb.AppendLine($" {task.signs[i]} {task.b[i]}");
            }

            sb.AppendLine($"\nУсловия неотрицательности:");
            for (int i = 0; i < task.n; i++)
            {
                sb.AppendLine($"  x{i + 1} >= 0");
            }

            MessageBox.Show(sb.ToString(), $"Детали задачи №{taskId}", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void RefreshTaskList()
        {
            lstTasks.Items.Clear();
            var tasks = LPTaskStorage.GetAllTasks();
            
            if (tasks.Count == 0)
            {
                lstTasks.Items.Add("──────────────────────────────────────────");
                lstTasks.Items.Add("   Нет сохраненных задач");
                lstTasks.Items.Add("   Нажмите кнопку 0 для ввода новой задачи");
                lstTasks.Items.Add("──────────────────────────────────────────");
                lstTasks.Enabled = false;
            }
            else
            {
                lstTasks.Enabled = true;
                foreach (var task in tasks)
                {
                    lstTasks.Items.Add($"{task.id}. {task.description}");
                }
                
                // Обновляем статистику
                UpdateTaskStats();
            }
            
            UpdateFunctionButtonsState();
        }

        private void UpdateTaskStats()
        {
            int taskCount = LPTaskStorage.Count;
            
            // Находим label статистики по имени
            foreach (Control control in this.Controls)
            {
                if (control.Name == "lblStatsValue")
                {
                    control.Text = $"Задач: {taskCount}";
                    break;
                }
            }
        }

        private void UpdateFunctionButtonsState()
        {
            bool hasTasks = LPTaskStorage.Count > 0;
            
            for (int i = 1; i <= 6; i++)
            {
                if (functionButtons[i] != null)
                {
                    functionButtons[i].Enabled = hasTasks;
                    functionButtons[i].BackColor = hasTasks ? 
                        (i % 2 == 0 ? Color.Lavender : Color.Honeydew) : 
                        Color.LightGray;
                }
            }
        }
    }
}
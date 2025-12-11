using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LinearProgrammingGUI
{
    public class SimplexMethodForm : Form
    {
        private TextBox txtResult;
        private Label lblTitle;
        private Button btnInputTask, btnSolve, btnClose;
        
        private int? selectedTaskId = null;
        private LPTask currentTask;
        private bool hasTask = false; // Флаг, указывающий, что задача выбрана через SetSelectedTask

        public SimplexMethodForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Симплекс-метод решения ЗЛП";
            this.Size = new Size(1100, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1000, 700);
            this.MaximizeBox = true;

            // Заголовок
            lblTitle = new Label
            {
                Text = "СИМПЛЕКС-МЕТОД РЕШЕНИЯ ЗАДАЧ ЛИНЕЙНОГО ПРОГРАММИРОВАНИЯ",
                Font = new Font("Arial", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 20),
                Size = new Size(1060, 30),
                ForeColor = Color.DarkBlue
            };
            this.Controls.Add(lblTitle);

            // Кнопка ввода задачи
            btnInputTask = new Button
            {
                Text = "1. Выбрать задачу",
                Location = new Point(20, 70),
                Size = new Size(150, 40),
                Font = new Font("Arial", 10)
            };
            btnInputTask.Click += BtnInputTask_Click;
            this.Controls.Add(btnInputTask);

            // Кнопка решения
            btnSolve = new Button
            {
                Text = "2. Решить симплекс-методом",
                Location = new Point(190, 70),
                Size = new Size(200, 40),
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
            lblTitle.Text = $"СИМПЛЕКС-МЕТОД (Задача №{taskId})";
            btnSolve.Enabled = true;

            // Показываем введенную задачу
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("================================================");
            sb.AppendLine("ВЫБРАННАЯ ЗАДАЧА:");
            sb.AppendLine("================================================");
            sb.AppendLine(TaskToString(currentTask));
            
            sb.AppendLine("\nОПИСАНИЕ МЕТОДА:");
            sb.AppendLine("Симплекс-метод - это алгоритм для решения задач линейного");
            sb.AppendLine("программирования. Он итеративно улучшает допустимое решение,");
            sb.AppendLine("двигаясь по вершинам многогранника допустимых решений,");
            sb.AppendLine("пока не достигнет оптимального решения.");
            
            txtResult.Text = sb.ToString();
            
            MessageBox.Show($"Задача №{taskId} выбрана. Нажмите кнопку 'Решить симплекс-методом' для решения.", 
                "Задача выбрана", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnInputTask_Click(object sender, EventArgs e)
        {
            if (hasTask)
            {
                // Используем уже выбранную задачу
                btnSolve.Enabled = true;
                
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("================================================");
                sb.AppendLine("ИСПОЛЬЗУЕТСЯ ВЫБРАННАЯ РАНЕЕ ЗАДАЧА:");
                sb.AppendLine("================================================");
                sb.AppendLine(TaskToString(currentTask));
                txtResult.Text = sb.ToString();
                
                MessageBox.Show($"Используется задача №{selectedTaskId.Value}. Нажмите кнопку 'Решить симплекс-методом' для решения.", 
                    "Задача уже выбрана", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                Text = "Выбор задачи",
                Size = new Size(500, 400),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false
            };

            Label lbl = new Label
            {
                Text = "Выберите задачу для решения симплекс-методом:",
                Location = new Point(20, 20),
                Size = new Size(450, 30),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            dialog.Controls.Add(lbl);

            ListBox listBox = new ListBox
            {
                Location = new Point(20, 60),
                Size = new Size(440, 250),
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
                Location = new Point(150, 320),
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
                    
                    currentTask = selectedTask;
                    selectedTaskId = taskId;
                    btnSolve.Enabled = true;
                    lblTitle.Text = $"СИМПЛЕКС-МЕТОД (Задача №{taskId})";
                    
                    dialog.Close();
                    
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("================================================");
                    sb.AppendLine("ВЫБРАННАЯ ЗАДАЧА:");
                    sb.AppendLine("================================================");
                    sb.AppendLine(TaskToString(currentTask));
                    txtResult.Text = sb.ToString();
                    
                    MessageBox.Show($"Задача №{taskId} выбрана. Нажмите кнопку 'Решить симплекс-методом' для решения.", 
                        "Задача выбрана", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
            dialog.Controls.Add(btnOK);

            // Кнопка отмены
            Button btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(260, 320),
                Size = new Size(100, 35),
                Font = new Font("Arial", 9)
            };
            btnCancel.Click += (s, ev) => dialog.Close();
            dialog.Controls.Add(btnCancel);

            dialog.ShowDialog();
        }

        private void BtnSolve_Click(object sender, EventArgs e)
        {
            // Определяем, какую задачу использовать
            LPTask taskToSolve;
            if (hasTask)
            {
                taskToSolve = currentTask;
            }
            else if (currentTask.n > 0 && currentTask.m > 0)
            {
                taskToSolve = currentTask;
            }
            else
            {
                MessageBox.Show("Сначала выберите задачу!", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Проверяем корректность задачи
            if (taskToSolve.n == 0 || taskToSolve.m == 0)
            {
                MessageBox.Show("Задача некорректна: отсутствуют переменные или ограничения", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Создаем решатель симплекс-метода
                SimplexSolver solver = new SimplexSolver(taskToSolve);
                string result = solver.SolveWithSteps();
                txtResult.Text = result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при решении симплекс-методом: {ex.Message}", 
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string TaskToString(LPTask task)
        {
            StringBuilder sb = new StringBuilder();
            string funcName = task.taskType == 1 ? "F" : "G";

            // Целевая функция
            sb.Append($"{funcName}(X) = ");
            for (int i = 0; i < task.c.Length; i++)
            {
                if (i > 0 && task.c[i] >= 0) sb.Append(" + ");
                sb.Append($"{task.c[i]}*x{i + 1}");
            }
            sb.AppendLine(task.taskType == 1 ? " -> max" : " -> min");

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

            // Условия неотрицательности
            sb.AppendLine("\nУсловия неотрицательности:");
            for (int i = 0; i < task.n; i++)
            {
                sb.AppendLine($"  x{i + 1} >= 0");
            }

            // Информация о размере задачи
            sb.AppendLine($"\nРазмерность задачи:");
            sb.AppendLine($"  Переменных: {task.n}");
            sb.AppendLine($"  Ограничений: {task.m}");
            sb.AppendLine($"  Тип задачи: {(task.taskType == 1 ? "МАКСИМИЗАЦИЯ" : "МИНИМИЗАЦИЯ")}");

            return sb.ToString();
        }
    }
}
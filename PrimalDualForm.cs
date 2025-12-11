using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LinearProgrammingGUI
{
    public class PrimalDualForm : Form
    {
        private TextBox txtResult;
        private LPTask currentTask;
        private Label lblTitle;
        private Button btnInput, btnSolve, btnClose;
        
        private int? selectedTaskId = null;
        private bool hasTask = false; // Флаг, указывающий, что задача выбрана через SetSelectedTask

        public PrimalDualForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Решение прямой и двойственной задачи";
            this.Size = new Size(1100, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1000, 700);
            this.MaximizeBox = true;

            // Заголовок
            lblTitle = new Label
            {
                Text = "РЕШЕНИЕ ПРЯМОЙ И ДВОЙСТВЕННОЙ ЗАДАЧИ",
                Font = new Font("Arial", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 20),
                Size = new Size(1060, 30),
                ForeColor = Color.DarkBlue
            };
            this.Controls.Add(lblTitle);

            // Кнопка выбора задачи
            btnInput = new Button
            {
                Text = "1. Выбрать задачу",
                Location = new Point(20, 70),
                Size = new Size(150, 40),
                Font = new Font("Arial", 10)
            };
            btnInput.Click += BtnInput_Click;
            this.Controls.Add(btnInput);

            // Кнопка решения
            btnSolve = new Button
            {
                Text = "2. Решить",
                Location = new Point(190, 70),
                Size = new Size(150, 40),
                Font = new Font("Arial", 10),
                Enabled = false
            };
            btnSolve.Click += BtnSolve_Click;
            this.Controls.Add(btnSolve);

            // Поле для вывода результатов
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
            lblTitle.Text = $"РЕШЕНИЕ ПРЯМОЙ И ДВОЙСТВЕННОЙ ЗАДАЧИ (Задача №{taskId})";
            btnSolve.Enabled = true;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("================================================");
            sb.AppendLine("ВЫБРАННАЯ ЗАДАЧА:");
            sb.AppendLine("================================================");
            sb.AppendLine(TaskToString(currentTask));
            txtResult.Text = sb.ToString();
            
            MessageBox.Show($"Задача №{taskId} выбрана. Нажмите кнопку 'Решить' для решения прямой и двойственной задачи.", 
                "Задача выбрана", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnInput_Click(object sender, EventArgs e)
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
                
                MessageBox.Show($"Используется задача №{selectedTaskId.Value}. Нажмите кнопку 'Решить' для решения.", 
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
                Text = "Выберите задачу для решения прямой и двойственной:",
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
                    SetSelectedTask(taskId, selectedTask);
                    dialog.Close();
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
            // Проверяем, что есть задача для решения
            LPTask taskToSolve;
            if (hasTask)
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
                var (primal, dual) = LPSolver.SolvePrimalDualSimultaneously(taskToSolve);
                DisplayResults(primal, dual);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при решении: {ex.Message}", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayResults(SolutionResult primal, SolutionResult dual)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("================================================");
            sb.AppendLine("РЕШЕНИЕ ПРЯМОЙ ЗАДАЧИ:");
            sb.AppendLine("================================================");
            sb.AppendLine(FormatSolutionResult(primal, "F", "x"));

            sb.AppendLine();
            sb.AppendLine("================================================");
            sb.AppendLine("РЕШЕНИЕ ДВОЙСТВЕННОЙ ЗАДАЧИ:");
            sb.AppendLine("================================================");
            sb.AppendLine(FormatSolutionResult(dual, "G", "y"));

            sb.AppendLine();
            sb.AppendLine("================================================");
            sb.AppendLine("АНАЛИЗ РЕЗУЛЬТАТОВ:");
            sb.AppendLine("================================================");

            if (primal.IsOptimal && dual.IsOptimal)
            {
                sb.AppendLine("Обе задачи решены оптимально");
                sb.AppendLine($"F(X) = {primal.ObjectiveValue:F4}");
                sb.AppendLine($"G(Y) = {dual.ObjectiveValue:F4}");

                if (Math.Abs(primal.ObjectiveValue - dual.ObjectiveValue) < 0.001)
                    sb.AppendLine("Теорема двойственности выполняется: F(X) = G(Y)");
                else
                    sb.AppendLine("Замечено расхождение в значениях целевых функций");
            }
            else if (!primal.IsFeasible && !dual.IsFeasible)
            {
                sb.AppendLine("Обе задачи не имеют допустимых решений");
            }
            else if (primal.IsUnbounded)
            {
                sb.AppendLine("Прямая задача неограничена");
                sb.AppendLine("Двойственная задача не имеет допустимых решений");
            }
            else if (dual.IsUnbounded)
            {
                sb.AppendLine("Двойственная задача неограничена");
                sb.AppendLine("Прямая задача не имеет допустимых решений");
            }
            else
            {
                sb.AppendLine("Оптимальные решения не найдены для обеих задач");
            }

            txtResult.Text = sb.ToString();
        }

        private string FormatSolutionResult(SolutionResult result, string funcName, string varName)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(result.Message);
            sb.AppendLine($"Допустимость: {result.IsFeasible}");
            sb.AppendLine($"Оптимальность: {result.IsOptimal}");
            sb.AppendLine($"Неограниченность: {result.IsUnbounded}");

            if (result.IsFeasible && result.Solution != null && result.Solution.Length > 0)
            {
                sb.AppendLine($"Значение целевой функции: {funcName} = {result.ObjectiveValue:F4}");
                
                sb.Append($"{funcName}({varName}) = ");
                for (int i = 0; i < result.Solution.Length; i++)
                {
                    if (i > 0 && result.Solution[i] >= 0) sb.Append(" + ");
                    sb.Append($"{result.Solution[i]:F4}*{varName}{i + 1}");
                }
                sb.AppendLine();

                sb.Append("Решение: (");
                for (int i = 0; i < Math.Min(result.Solution.Length, 10); i++)
                {
                    sb.Append($"{result.Solution[i]:F4}");
                    if (i < result.Solution.Length - 1) sb.Append("; ");
                }
                if (result.Solution.Length > 10) sb.Append("...");
                sb.AppendLine(")");
            }

            return sb.ToString();
        }

        private string TaskToString(LPTask task)
        {
            StringBuilder sb = new StringBuilder();
            string funcName = task.taskType == 1 ? "F" : "G";

            sb.Append($"{funcName}(x) = ");
            for (int i = 0; i < task.c.Length; i++)
            {
                if (i > 0 && task.c[i] >= 0) sb.Append(" + ");
                sb.Append($"{task.c[i]}*x{i + 1}");
            }
            sb.AppendLine(task.taskType == 1 ? " -> max" : " -> min");

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

            sb.AppendLine("\nУсловия неотрицательности:");
            for (int i = 0; i < task.n; i++)
            {
                sb.AppendLine($"  x{i + 1} >= 0");
            }

            return sb.ToString();
        }
    }
}
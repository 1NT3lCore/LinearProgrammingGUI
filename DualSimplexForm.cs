using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LinearProgrammingGUI
{
    public class DualSimplexForm : Form
    {
        private TextBox txtResult;
        private LPTask currentTask;
        private Label lblTitle;
        private Button btnInput, btnSolve, btnClose;
        
        private int? selectedTaskId = null;
        private bool hasTask = false; // Флаг, указывающий, что задача выбрана через SetSelectedTask

        public DualSimplexForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Решение двойственным симплекс-методом";
            this.Size = new Size(1100, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1000, 700);
            this.MaximizeBox = true;

            // Заголовок
            lblTitle = new Label
            {
                Text = "РЕШЕНИЕ ДВОЙСТВЕННЫМ СИМПЛЕКС-МЕТОДОМ",
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
            lblTitle.Text = $"РЕШЕНИЕ ДВОЙСТВЕННЫМ СИМПЛЕКС-МЕТОДОМ (Задача №{taskId})";
            btnSolve.Enabled = true;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("================================================");
            sb.AppendLine("ВЫБРАННАЯ ЗАДАЧА:");
            sb.AppendLine("================================================");
            sb.AppendLine(TaskToString(currentTask));
            
            sb.AppendLine("\nПРИМЕЧАНИЕ:");
            sb.AppendLine("Двойственный симплекс-метод применяется для решения задач,");
            sb.AppendLine("которые имеют допустимое базисное решение двойственной задачи,");
            sb.AppendLine("но недопустимое решение прямой задачи.");
            
            txtResult.Text = sb.ToString();
            
            MessageBox.Show($"Задача №{taskId} выбрана. Нажмите кнопку 'Решить' для решения двойственным симплекс-методом.", 
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
                Text = "Выберите задачу для решения двойственным симплекс-методом:",
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
                var result = LPSolver.SolveDualSimplex(taskToSolve);
                DisplayResult(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при решении: {ex.Message}", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayResult(SolutionResult result)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("================================================");
            sb.AppendLine("РЕЗУЛЬТАТ РЕШЕНИЯ ДВОЙСТВЕННЫМ СИМПЛЕКС-МЕТОДОМ:");
            sb.AppendLine("================================================");

            sb.AppendLine($"Сообщение: {result.Message}");
            sb.AppendLine($"Допустимость решения: {result.IsFeasible}");
            sb.AppendLine($"Оптимальность решения: {result.IsOptimal}");
            sb.AppendLine($"Неограниченность задачи: {result.IsUnbounded}");

            if (result.IsOptimal && result.Solution != null && result.Solution.Length > 0)
            {
                sb.AppendLine($"\nЗначение целевой функции: F = {result.ObjectiveValue:F4}");

                sb.Append("Оптимальное решение: (");
                for (int i = 0; i < Math.Min(result.Solution.Length, 10); i++)
                {
                    sb.Append($"x{i + 1} = {result.Solution[i]:F4}");
                    if (i < result.Solution.Length - 1) sb.Append("; ");
                }
                if (result.Solution.Length > 10) sb.Append("...");
                sb.AppendLine(")");

                sb.AppendLine("\nПРОВЕРКА ОГРАНИЧЕНИЙ:");
                sb.AppendLine("---------------------");
                
                bool allConstraintsSatisfied = true;
                for (int i = 0; i < currentTask.m; i++)
                {
                    double leftPart = 0;
                    for (int j = 0; j < currentTask.n; j++)
                    {
                        leftPart += currentTask.A[i, j] * result.Solution[j];
                    }

                    bool constraintSatisfied = false;
                    string status = "ВЫПОЛНЕНО";
                    
                    if (currentTask.signs[i] == "<=")
                    {
                        constraintSatisfied = leftPart <= currentTask.b[i] + 0.001;
                    }
                    else if (currentTask.signs[i] == ">=")
                    {
                        constraintSatisfied = leftPart >= currentTask.b[i] - 0.001;
                    }
                    else // "="
                    {
                        constraintSatisfied = Math.Abs(leftPart - currentTask.b[i]) <= 0.001;
                    }
                    
                    if (!constraintSatisfied)
                    {
                        status = "НАРУШЕНО";
                        allConstraintsSatisfied = false;
                    }

                    sb.AppendLine($"{i + 1}. {leftPart:F4} {currentTask.signs[i]} {currentTask.b[i]:F4}  -  {status}");
                }

                if (allConstraintsSatisfied)
                {
                    sb.AppendLine("\nВСЕ ОГРАНИЧЕНИЯ ВЫПОЛНЕНЫ!");
                }
                else
                {
                    sb.AppendLine("\nВНИМАНИЕ: Некоторые ограничения не выполнены!");
                }

                // Проверка неотрицательности переменных
                sb.AppendLine("\nПРОВЕРКА НЕОТРИЦАТЕЛЬНОСТИ ПЕРЕМЕННЫХ:");
                sb.AppendLine("------------------------------------");
                
                bool allVariablesNonNegative = true;
                for (int i = 0; i < result.Solution.Length; i++)
                {
                    string status = result.Solution[i] >= -0.001 ? ">= 0  - ВЫПОЛНЕНО" : "< 0  - НАРУШЕНО";
                    if (result.Solution[i] < -0.001) allVariablesNonNegative = false;
                    sb.AppendLine($"x{i + 1} = {result.Solution[i]:F4}  -  {status}");
                }
                
                if (!allVariablesNonNegative)
                {
                    sb.AppendLine("\nВНИМАНИЕ: Некоторые переменные отрицательные!");
                }
            }
            else if (!result.IsFeasible)
            {
                sb.AppendLine("\nЗадача не имеет допустимых решений.");
                sb.AppendLine("Причины могут быть следующие:");
                sb.AppendLine("- Противоречивые ограничения");
                sb.AppendLine("- Область допустимых решений пуста");
            }
            else if (result.IsUnbounded)
            {
                sb.AppendLine("\nЗадача неограничена.");
                sb.AppendLine("Целевая функция может принимать сколь угодно большие");
                sb.AppendLine("(для максимизации) или малые (для минимизации) значения.");
            }

            txtResult.Text = sb.ToString();
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
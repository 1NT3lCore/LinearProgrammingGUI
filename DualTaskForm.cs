using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LinearProgrammingGUI
{
    public class DualTaskForm : Form
    {
        private TextBox txtResult;
        private Button btnInput, btnClose;
        private Label lblTitle;
        
        private int? selectedTaskId = null;
        private LPTask currentTask;
        private bool hasTask = false; // Флаг, указывающий, что задача выбрана

        public DualTaskForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Построение двойственной задачи";
            this.Size = new Size(1100, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1000, 700);
            this.MaximizeBox = true;

            lblTitle = new Label
            {
                Text = "ПОСТРОЕНИЕ ДВОЙСТВЕННОЙ ЗАДАЧИ",
                Font = new Font("Arial", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(20, 20),
                Size = new Size(1060, 30),
                ForeColor = Color.DarkBlue
            };
            this.Controls.Add(lblTitle);

            btnInput = new Button
            {
                Text = "Выбрать задачу",
                Location = new Point(20, 70),
                Size = new Size(150, 40),
                Font = new Font("Arial", 10)
            };
            btnInput.Click += BtnInput_Click;
            this.Controls.Add(btnInput);

            txtResult = new TextBox
            {
                Location = new Point(20, 120),
                Size = new Size(1040, 610),
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                Font = new Font("Courier New", 10),
                ReadOnly = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(txtResult);

            btnClose = new Button
            {
                Text = "Закрыть",
                Location = new Point(960, 740),
                Size = new Size(100, 30),
                Font = new Font("Arial", 9),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
        }

        public void SetSelectedTask(int taskId, LPTask task)
        {
            selectedTaskId = taskId;
            currentTask = task;
            hasTask = true; // Устанавливаем флаг
            lblTitle.Text = $"ПОСТРОЕНИЕ ДВОЙСТВЕННОЙ ЗАДАЧИ (Задача №{taskId})";
            ProcessTask(currentTask);
        }

        private void BtnInput_Click(object sender, EventArgs e)
        {
            if (hasTask)
            {
                // Используем уже выбранную задачу
                ProcessTask(currentTask);
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
                Text = "Выберите задачу для построения двойственной:",
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
                    var task = LPTaskStorage.GetTask(taskId);
                    SetSelectedTask(taskId, task);
                    dialog.Close();
                }
            };
            dialog.Controls.Add(btnOK);

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

        private void ProcessTask(LPTask task)
        {
            try
            {
                // Проверяем, что задача корректна
                if (task.n == 0 || task.m == 0)
                {
                    MessageBox.Show("Задача некорректна: отсутствуют переменные или ограничения",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                LPTask dual = BuildDual(task);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("================================================");
                sb.AppendLine("ПРЯМАЯ ЗАДАЧА:");
                sb.AppendLine("================================================");
                sb.AppendLine(TaskToString(task, "x", "F"));

                sb.AppendLine();
                sb.AppendLine("================================================");
                sb.AppendLine("ДВОЙСТВЕННАЯ ЗАДАЧА:");
                sb.AppendLine("================================================");
                sb.AppendLine(TaskToString(dual, "y", "G"));

                sb.AppendLine();
                sb.AppendLine("ПРАВИЛА ПОСТРОЕНИЯ ДВОЙСТВЕННОЙ ЗАДАЧИ:");
                sb.AppendLine("1. Матрица коэффициентов транспонируется");
                sb.AppendLine("2. Коэффициенты целевой функции и правые части меняются местами");
                sb.AppendLine("3. Направление оптимизации меняется на противоположное:");
                sb.AppendLine("   - Максимизация становится минимизацией");
                sb.AppendLine("   - Минимизация становится максимизацией");
                sb.AppendLine("4. Знаки неравенств в ограничениях изменяются:");
                sb.AppendLine("   - В прямой задаче <=  в двойственной =>");
                sb.AppendLine("   - В прямой задаче =>  в двойственной <=");
                sb.AppendLine("   - Равенства остаются равенствами");

                txtResult.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при построении двойственной задачи: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private LPTask BuildDual(LPTask primal)
        {
            LPTask dual = new LPTask
            {
                n = primal.m,
                m = primal.n,
                taskType = 3 - primal.taskType, // 1->2 (max->min), 2->1 (min->max)
                c = primal.b,
                b = primal.c,
                A = new double[primal.n, primal.m],
                signs = new string[primal.n]
            };

            // Транспонирование матрицы
            for (int i = 0; i < dual.m; i++)
                for (int j = 0; j < dual.n; j++)
                    dual.A[i, j] = primal.A[j, i];

            // Преобразование знаков неравенств
            for (int i = 0; i < dual.m; i++)
            {
                if (primal.signs[i] == "<=")
                    dual.signs[i] = ">=";
                else if (primal.signs[i] == ">=")
                    dual.signs[i] = "<=";
                else // "="
                    dual.signs[i] = "=";
            }

            return dual;
        }

        private string TaskToString(LPTask task, string varName, string funcName)
        {
            StringBuilder sb = new StringBuilder();

            // Целевая функция
            sb.Append($"{funcName}({varName}) = ");
            for (int i = 0; i < task.c.Length; i++)
            {
                if (i > 0 && task.c[i] >= 0) sb.Append(" + ");
                sb.Append($"{task.c[i]}*{varName}{i + 1}");
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
                    sb.Append($"{task.A[i, j]}*{varName}{j + 1}");
                }
                sb.AppendLine($" {task.signs[i]} {task.b[i]}");
            }

            // Условия неотрицательности
            sb.AppendLine("\nУсловия неотрицательности:");
            for (int i = 0; i < task.n; i++)
            {
                sb.AppendLine($"  {varName}{i + 1} >= 0");
            }

            return sb.ToString();
        }
    }
}
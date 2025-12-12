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
        private bool hasTask = false;

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
            hasTask = true;
            lblTitle.Text = $"ПОСТРОЕНИЕ ДВОЙСТВЕННОЙ ЗАДАЧИ (Задача №{taskId})";
            ProcessTask(currentTask);
        }

        private void BtnInput_Click(object sender, EventArgs e)
        {
            if (hasTask)
            {
                ProcessTask(currentTask);
            }
            else
            {
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
                if (task.n == 0 || task.m == 0)
                {
                    MessageBox.Show("Задача некорректна: отсутствуют переменные или ограничения",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Строим двойственную задачу
                var dualResult = BuildDualWithDetails(task);
                LPTask dual = dualResult.DualTask;
                bool?[] dualVarSignRestrictions = dualResult.DualVarSigns;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("================================================");
                sb.AppendLine("ПРЯМАЯ ЗАДАЧА:");
                sb.AppendLine("================================================");
                sb.AppendLine(TaskToString(task, "x", task.taskType == 1 ? "F" : "G", null, true));

                sb.AppendLine();
                sb.AppendLine("================================================");
                sb.AppendLine("ДВОЙСТВЕННАЯ ЗАДАЧА:");
                sb.AppendLine("================================================");
                sb.AppendLine(TaskToString(dual, "y", dual.taskType == 1 ? "F" : "G", dualVarSignRestrictions, false));

                sb.AppendLine();
                sb.AppendLine("ПРАВИЛА ПОСТРОЕНИЯ ДВОЙСТВЕННОЙ ЗАДАЧИ:");
                sb.AppendLine("1. Матрица коэффициентов транспонируется");
                sb.AppendLine("2. Коэффициенты целевой функции и правые части меняются местами");
                sb.AppendLine("3. Направление оптимизации меняется на противоположное:");
                sb.AppendLine("   - Максимизация становится минимизацией");
                sb.AppendLine("   - Минимизация становится максимизацией");
                sb.AppendLine("4. Для ограничений прямой задачи:");
                sb.AppendLine("   - Если ограничение =, то переменная двойственной - любая");
                sb.AppendLine("   - Если ограничение <=, то переменная двойственной >= 0");
                sb.AppendLine("   - Если ограничение >=, то переменная двойственной <= 0");
                sb.AppendLine("5. Для переменных прямой задачи (все >= 0):");
                sb.AppendLine("   - Ограничения двойственной: >= при максимизации, <= при минимизации");

                txtResult.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при построении двойственной задачи: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private struct DualResult
        {
            public LPTask DualTask { get; set; }
            public bool?[] DualVarSigns { get; set; } // null = любая, true = >= 0, false = <= 0
        }

        private DualResult BuildDualWithDetails(LPTask primal)
        {
            LPTask dual = new LPTask
            {
                n = primal.m,
                m = primal.n,
                taskType = (primal.taskType == 1) ? 2 : 1,
                c = new double[primal.m],
                b = new double[primal.n],
                A = new double[primal.n, primal.m],
                signs = new string[primal.n]
            };

            bool?[] dualVarSigns = new bool?[primal.m];

            // Копируем правые части и определяем знаки двойственных переменных
            for (int i = 0; i < primal.m; i++)
            {
                dual.c[i] = primal.b[i];
                
                // Определяем знак двойственной переменной
                if (primal.signs[i] == "=")
                {
                    dualVarSigns[i] = null; // Произвольный знак
                }
                else if (primal.signs[i] == "<=")
                {
                    dualVarSigns[i] = true; // >= 0
                }
                else if (primal.signs[i] == ">=")
                {
                    dualVarSigns[i] = false; // <= 0
                }
                else
                {
                    dualVarSigns[i] = true; // По умолчанию >= 0
                }
            }

            // Копируем коэффициенты целевой функции
            for (int j = 0; j < primal.n; j++)
            {
                dual.b[j] = primal.c[j];
            }

            // Транспонирование матрицы
            for (int i = 0; i < primal.m; i++)
            {
                for (int j = 0; j < primal.n; j++)
                {
                    dual.A[j, i] = primal.A[i, j];
                }
            }

            // Знаки ограничений двойственной задачи
            for (int j = 0; j < dual.m; j++)
            {
                if (primal.taskType == 1) // Максимизация в прямой
                {
                    dual.signs[j] = ">=";
                }
                else // Минимизация в прямой
                {
                    dual.signs[j] = "<=";
                }
            }

            return new DualResult
            {
                DualTask = dual,
                DualVarSigns = dualVarSigns
            };
        }

        private string TaskToString(LPTask task, string varName, string funcName, bool?[] varSignRestrictions, bool isPrimal)
{
    StringBuilder sb = new StringBuilder();

    // Целевая функция
    sb.Append($"{funcName}({varName}) = ");
    for (int i = 0; i < task.c.Length; i++)
    {
        if (i > 0)
        {
            if (task.c[i] >= 0)
                sb.Append(" + ");
            else
                sb.Append(" - ");
        }
        sb.Append($"{Math.Abs(task.c[i])}*{varName}{i + 1}");
    }
    sb.AppendLine(task.taskType == 1 ? " -> max" : " -> min");

    // Ограничения
    sb.AppendLine("\nОграничения:");
    for (int i = 0; i < task.m; i++)
    {
        sb.Append("  ");
        bool firstTerm = true;
        for (int j = 0; j < task.n; j++)
        {
            double coeff = task.A[i, j];
            if (Math.Abs(coeff) > 0.0001)
            {
                if (!firstTerm)
                {
                    if (coeff >= 0)
                        sb.Append(" + ");
                    else
                        sb.Append(" - ");
                }
                else
                {
                    firstTerm = false;
                    if (coeff < 0)
                        sb.Append("-");
                }
                sb.Append($"{Math.Abs(coeff)}*{varName}{j + 1}");
            }
        }
        if (firstTerm)
        {
            sb.Append("0");
        }
        sb.AppendLine($" {task.signs[i]} {task.b[i]}");
    }

    // Условия на переменные
    sb.AppendLine("\nУсловия на переменные:");
    
    if (isPrimal)
    {
        // Прямая задача - все переменные >= 0
        for (int i = 0; i < task.n; i++)
        {
            sb.AppendLine($"  {varName}{i + 1} >= 0");
        }
    }
    else
    {
        // Двойственная задача
        if (varSignRestrictions != null && varSignRestrictions.Length >= task.n)
        {
            for (int i = 0; i < task.n; i++)
            {
                if (varSignRestrictions[i] == null)
                {
                    sb.AppendLine($"  {varName}{i + 1} - любая (произвольного знака)");
                }
                else if (varSignRestrictions[i] == true)
                {
                    sb.AppendLine($"  {varName}{i + 1} >= 0");
                }
                else
                {
                    sb.AppendLine($"  {varName}{i + 1} <= 0");
                }
            }
        }
        else
        {
            // Определяем по ограничениям текущей задачи (прямой)
            bool allEqualities = true;
            
            // Проверяем, что текущая задача инициализирована и есть знаки
            if (currentTask.signs != null && currentTask.m > 0)
            {
                for (int i = 0; i < currentTask.m; i++)
                {
                    if (currentTask.signs[i] != "=")
                    {
                        allEqualities = false;
                        break;
                    }
                }
            }
            else
            {
                allEqualities = false; // Если данных нет, считаем что не все равенства
            }
            
            if (allEqualities)
            {
                // Все ограничения прямой - равенства
                sb.Append("  ");
                for (int i = 0; i < task.n; i++)
                {
                    sb.Append($"{varName}{i + 1}");
                    if (i < task.n - 1) sb.Append(", ");
                }
                sb.AppendLine(" - любые (произвольного знака)");
            }
            else
            {
                // Есть неравенства - показываем по умолчанию >= 0
                for (int i = 0; i < task.n; i++)
                {
                    sb.AppendLine($"  {varName}{i + 1} >= 0");
                }
            }
        }
    }

    // Информация о размере
    sb.AppendLine($"\nРазмерность: {task.n} переменных, {task.m} ограничений");

    return sb.ToString();
}}}
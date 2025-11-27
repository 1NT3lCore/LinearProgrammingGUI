using System;
using System.Drawing;
using System.Windows.Forms;

namespace LinearProgrammingGUI
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Исследование операций - ЛП";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Заголовок
            Label titleLabel = new Label
            {
                Text = "ЛИНЕЙНОЕ ПРОГРАММИРОВАНИЕ",
                Font = new Font("Arial", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(50, 20),
                Size = new Size(400, 30)
            };
            this.Controls.Add(titleLabel);

            // Кнопка 1: Построение двойственной задачи
            Button btn1 = new Button
            {
                Text = "1. Построение двойственной задачи",
                Location = new Point(100, 70),
                Size = new Size(300, 40),
                Font = new Font("Arial", 10)
            };
            btn1.Click += (s, e) => { new DualTaskForm().ShowDialog(); };
            this.Controls.Add(btn1);

            // Кнопка 2: Проверка плана на оптимальность
            Button btn2 = new Button
            {
                Text = "2. Проверка плана на оптимальность",
                Location = new Point(100, 120),
                Size = new Size(300, 40),
                Font = new Font("Arial", 10)
            };
            btn2.Click += (s, e) => { new OptimalityCheckForm().ShowDialog(); };
            this.Controls.Add(btn2);


            // Кнопка 3: Одновременное решение прямой и двойственной задачи
            Button btn3 = new Button
            {
                Text = "3. Прямая и двойственная задача",
                Location = new Point(100, 200),
                Size = new Size(300, 35),
                Font = new Font("Arial", 10),
                Enabled = true
            };
            btn3.Click += (s, e) => { new PrimalDualForm().ShowDialog(); };
            this.Controls.Add(btn3);

            // Кнопка 4: Двойственный симплекс-метод
            Button btn4 = new Button
            {
                Text = "4. Двойственный симплекс-метод",
                Location = new Point(100, 245),
                Size = new Size(300, 35),
                Font = new Font("Arial", 10),
                Enabled = true
            };
            btn4.Click += (s, e) => { new DualSimplexForm().ShowDialog(); };
            this.Controls.Add(btn4);

            // Кнопка 5: Симплекс-метод 
            Button btn5 = new Button
            {
                Text = "5. Симплекс-метод",
                Location = new Point(100, 290),
                Size = new Size(300, 35),
                Font = new Font("Arial", 10),
                Enabled = true
            };
            btn5.Click += (s, e) => { new SimplexMethodForm().ShowDialog(); };
            this.Controls.Add(btn5);

            // Кнопка 6: Транспортная задача 
            Button btn6 = new Button
            {
                Text = "6. Транспортная задача",
                Location = new Point(100, 335),
                Size = new Size(300, 35),
                Font = new Font("Arial", 10),
                Enabled = true
            };
            btn6.Click += (s, e) => { new TransportProblemForm().ShowDialog(); };
            this.Controls.Add(btn6);

            // Кнопка выхода
            Button btnExit = new Button
            {
                Text = "Выход",
                Location = new Point(200, 380),
                Size = new Size(100, 30)
            };
            btnExit.Click += (s, e) => { this.Close(); };
            this.Controls.Add(btnExit);
        }
    }
}
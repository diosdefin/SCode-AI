using System;
using System.Windows.Forms;
using Konvertor.Services;
using Konvertor.Models;

namespace Konvertor.Forms
{
    public partial class HistoryForm : Form
    {
        private HistoryService _historyService;
        private MainForm _mainForm;

        public HistoryForm(MainForm mainForm)
        {
            InitializeComponent();
            _historyService = new HistoryService();
            _mainForm = mainForm;
            InitializeTheme();
        }

        private void InitializeTheme()
        {
            // Неоновая тема
            this.BackColor = System.Drawing.Color.FromArgb(10, 10, 15);
            this.ForeColor = System.Drawing.Color.White;
            this.DoubleBuffered = true;

            panelMain.BackColor = System.Drawing.Color.FromArgb(21, 21, 32);
            panelMain.BorderStyle = BorderStyle.FixedSingle;
            
            listHistory.BackColor = System.Drawing.Color.FromArgb(26, 26, 46);
            listHistory.ForeColor = System.Drawing.Color.White;
            listHistory.BorderStyle = BorderStyle.FixedSingle;
            listHistory.Font = new Font("Segoe UI", 10);
            
            txtDetails.BackColor = System.Drawing.Color.FromArgb(26, 26, 46);
            txtDetails.ForeColor = System.Drawing.Color.White;
            txtDetails.BorderStyle = BorderStyle.FixedSingle;
            txtDetails.Font = new Font("Consolas", 9);

            // Настройка кнопок с эффектами
            SetupNeonButton(btnClear, System.Drawing.Color.FromArgb(255, 87, 87), System.Drawing.Color.White, true);
            SetupNeonButton(btnLoad, System.Drawing.Color.FromArgb(0, 212, 255), System.Drawing.Color.Black, true);
            SetupNeonButton(btnClose, System.Drawing.Color.FromArgb(100, 100, 100), System.Drawing.Color.White, true);
            SetupNeonButton(btnExport, System.Drawing.Color.FromArgb(157, 0, 255), System.Drawing.Color.White, true);

            lblHistoryTitle.ForeColor = System.Drawing.Color.FromArgb(0, 212, 255);
            lblHistoryTitle.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblDetailsTitle.ForeColor = System.Drawing.Color.FromArgb(0, 212, 255);
            lblDetailsTitle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
        }

        private void SetupNeonButton(Button button, Color neonColor, Color textColor, bool filled)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 2;
            button.FlatAppearance.BorderColor = neonColor;
            button.ForeColor = textColor;
            button.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            
            if (filled)
            {
                button.BackColor = neonColor;
            }
            else
            {
                button.BackColor = Color.Transparent;
            }

            // Добавляем эффекты наведения
            button.MouseEnter += (s, e) =>
            {
                if (filled)
                {
                    button.BackColor = Color.FromArgb(
                        Math.Min(255, neonColor.R + 30),
                        Math.Min(255, neonColor.G + 30),
                        Math.Min(255, neonColor.B + 30));
                }
                else
                {
                    button.BackColor = Color.FromArgb(30, neonColor.R, neonColor.G, neonColor.B);
                }
                button.Cursor = Cursors.Hand;
            };

            button.MouseLeave += (s, e) =>
            {
                if (filled)
                {
                    button.BackColor = neonColor;
                }
                else
                {
                    button.BackColor = Color.Transparent;
                }
                button.Cursor = Cursors.Default;
            };
        }

        private void HistoryForm_Load(object sender, EventArgs e)
        {
            LoadHistoryList();
        }

        private void LoadHistoryList()
        {
            listHistory.Items.Clear();
            var history = _historyService.GetHistory();

            foreach (var item in history)
            {
                listHistory.Items.Add(item);
            }

            if (listHistory.Items.Count > 0)
                listHistory.SelectedIndex = 0;
            else
                txtDetails.Text = "История пуста";
        }

        private void listHistory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listHistory.SelectedItem is ConversionHistory historyItem)
            {
                if (historyItem.IsAnalysis)
                {
                    txtDetails.Text =
                        $"📅 Дата: {historyItem.Timestamp:dd.MM.yyyy HH:mm:ss}\r\n" +
                        $"🔍 Тип: Анализ языка\r\n" +
                        $"🎯 Определен: {historyItem.FromLanguage}\r\n" +
                        $"📏 Длина: {historyItem.SourceCode.Length} символов\r\n" +
                        $"✅ Статус: Успешно\r\n\r\n" +
                        $"📝 АНАЛИЗИРОВАННЫЙ КОД:\r\n{historyItem.SourceCode}";
                }
                else
                {
                    txtDetails.Text =
                        $"📅 Дата: {historyItem.Timestamp:dd.MM.yyyy HH:mm:ss}\r\n" +
                        $"🔤 Из: {historyItem.FromLanguage}\r\n" +
                        $"🎯 В: {historyItem.ToLanguage}\r\n" +
                        $"📏 Длина: {historyItem.SourceCode.Length} символов\r\n" +
                        $"✅ Статус: {(historyItem.Success ? "Успешно" : "Ошибка")}\r\n\r\n" +
                        $"📝 ИСХОДНЫЙ КОД:\r\n{historyItem.SourceCode}\r\n\r\n" +
                        $"✨ КОНВЕРТИРОВАННЫЙ КОД:\r\n{historyItem.ConvertedCode}";
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (DarkMessageBox.Show("Вы уверены, что хотите очистить всю историю?",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                _historyService.ClearHistory();
                LoadHistoryList();
                DarkMessageBox.Show("История очищена", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (listHistory.SelectedItem is ConversionHistory historyItem)
            {
                // Загружаем выбранную запись в главную форму
                _mainForm.LoadFromHistory(historyItem);
                this.Close();
            }
            else
            {
                DarkMessageBox.Show("Выберите запись из истории", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (listHistory.SelectedItem is ConversionHistory historyItem)
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                saveDialog.FileName = $"conversion_{historyItem.Timestamp:yyyyMMdd_HHmmss}.txt";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    string content =
                        $"=== Конвертация кода ===\r\n" +
                        $"Дата: {historyItem.Timestamp}\r\n" +
                        $"Из: {historyItem.FromLanguage}\r\n" +
                        $"В: {historyItem.ToLanguage}\r\n" +
                        $"Статус: {(historyItem.Success ? "Успешно" : "Ошибка")}\r\n\r\n" +
                        $"=== ИСХОДНЫЙ КОД ===\r\n{historyItem.SourceCode}\r\n\r\n" +
                        $"=== РЕЗУЛЬТАТ ===\r\n{historyItem.ConvertedCode}";

                    System.IO.File.WriteAllText(saveDialog.FileName, content);

                    DarkMessageBox.Show($"История экспортирована в файл:\n{saveDialog.FileName}",
                        "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}
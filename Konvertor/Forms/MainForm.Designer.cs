namespace Konvertor.Forms
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            splitContainer1 = new SplitContainer();
            panelLeft = new Panel();
            lblSource = new Label();
            lblFromLang = new Label();
            comboFromLanguage = new Konvertor.Controls.IconComboBox();
            btnAutoDetect = new Button();
            txtSourceCode = new RichTextBox();
            lblStats = new Label();
            panelRight = new Panel();
            lblResult = new Label();
            lblToLang = new Label();
            comboToLanguage = new Konvertor.Controls.IconComboBox();
            txtResult = new RichTextBox();
            lblConverting = new Label();
            progressBar = new ProgressBar();
            panelBottom = new Panel();
            lblBottomStats = new Label();
            lblStatus = new Label();
            btnHistory = new Button();
            btnCopy = new Button();
            btnConvert = new Button();
            menuStrip1 = new MenuStrip();
            файлToolStripMenuItem = new ToolStripMenuItem();
            импортToolStripMenuItem = new ToolStripMenuItem();
            экспортToolStripMenuItem = new ToolStripMenuItem();
            выходToolStripMenuItem = new ToolStripMenuItem();
            историяToolStripMenuItem = new ToolStripMenuItem();
            открытьИсториюToolStripMenuItem = new ToolStripMenuItem();
            очиститьИсториюToolStripMenuItem = new ToolStripMenuItem();
            помощьToolStripMenuItem = new ToolStripMenuItem();
            оПрограммеToolStripMenuItem = new ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            panelLeft.SuspendLayout();
            panelRight.SuspendLayout();
            panelBottom.SuspendLayout();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.BackColor = Color.FromArgb(18, 22, 33);
            splitContainer1.BorderStyle = BorderStyle.FixedSingle;
            splitContainer1.Dock = DockStyle.None;
            splitContainer1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            splitContainer1.FixedPanel = FixedPanel.None;
            splitContainer1.Location = new Point(0, 28);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.BackColor = Color.FromArgb(18, 22, 33);
            splitContainer1.Panel1.Controls.Add(panelLeft);
            splitContainer1.Panel1.Padding = new Padding(2);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.BackColor = Color.FromArgb(18, 22, 33);
            splitContainer1.Panel2.Controls.Add(panelRight);
            splitContainer1.Panel2.Padding = new Padding(2);
            splitContainer1.Size = new Size(1200, 590);
            splitContainer1.SplitterDistance = 595;
            splitContainer1.SplitterWidth = 4;
            splitContainer1.TabIndex = 0;
            // 
            // panelLeft
            // 
            panelLeft.BackColor = Color.FromArgb(18, 22, 33);
            panelLeft.Controls.Add(lblSource);
            panelLeft.Controls.Add(lblFromLang);
            panelLeft.Controls.Add(comboFromLanguage);
            panelLeft.Controls.Add(btnAutoDetect);
            panelLeft.Controls.Add(txtSourceCode);
            panelLeft.Controls.Add(lblStats);
            panelLeft.Dock = DockStyle.Fill;
            panelLeft.Location = new Point(2, 2);
            panelLeft.Name = "panelLeft";
            panelLeft.Padding = new Padding(2);
            panelLeft.Size = new Size(591, 586);
            panelLeft.TabIndex = 0;
            // 
            // lblSource
            // 
            lblSource.AutoSize = true;
            lblSource.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblSource.ForeColor = Color.FromArgb(0, 212, 255);
            lblSource.Location = new Point(4, 4);
            lblSource.Name = "lblSource";
            lblSource.Size = new Size(150, 28);
            lblSource.TabIndex = 0;
            lblSource.Text = "📥 Исходный код";
            // 
            // lblFromLang
            // 
            lblFromLang.AutoSize = true;
            lblFromLang.Font = new Font("Segoe UI", 9F);
            lblFromLang.ForeColor = Color.FromArgb(180, 190, 210);
            lblFromLang.Location = new Point(4, 44);
            lblFromLang.Name = "lblFromLang";
            lblFromLang.Size = new Size(58, 20);
            lblFromLang.TabIndex = 1;
            lblFromLang.Text = "Язык:";
            // 
            // comboFromLanguage
            // 
            comboFromLanguage.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboFromLanguage.BackColor = Color.FromArgb(25, 28, 40);
            comboFromLanguage.DrawMode = DrawMode.OwnerDrawFixed;
            comboFromLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            comboFromLanguage.FlatStyle = FlatStyle.Popup;
            comboFromLanguage.Font = new Font("Segoe UI", 10F);
            comboFromLanguage.ForeColor = Color.White;
            comboFromLanguage.FormattingEnabled = true;
            comboFromLanguage.ItemHeight = 29;
            comboFromLanguage.Location = new Point(74, 39);
            comboFromLanguage.Name = "comboFromLanguage";
            comboFromLanguage.Size = new Size(290, 36);
            comboFromLanguage.TabIndex = 3;
            // 
            // btnAutoDetect
            // 
            btnAutoDetect.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAutoDetect.FlatStyle = FlatStyle.Flat;
            btnAutoDetect.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnAutoDetect.ForeColor = Color.FromArgb(0, 212, 255);
            btnAutoDetect.Location = new Point(379, 39);
            btnAutoDetect.Name = "btnAutoDetect";
            btnAutoDetect.Size = new Size(130, 35);
            btnAutoDetect.TabIndex = 4;
            btnAutoDetect.Text = "🔍 Автоопределить";
            btnAutoDetect.UseVisualStyleBackColor = false;
            // 
            // txtSourceCode
            // 
            txtSourceCode.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtSourceCode.BackColor = Color.FromArgb(25, 28, 40);
            txtSourceCode.BorderStyle = BorderStyle.FixedSingle;
            txtSourceCode.Font = new Font("Consolas", 10F);
            txtSourceCode.ForeColor = Color.White;
            txtSourceCode.Location = new Point(4, 84);
            txtSourceCode.Name = "txtSourceCode";
            txtSourceCode.ScrollBars = RichTextBoxScrollBars.None;
            txtSourceCode.Size = new Size(549, 449);
            txtSourceCode.TabIndex = 2;
            txtSourceCode.WordWrap = false;
            // 
            // lblStats
            // 
            lblStats.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            lblStats.AutoSize = true;
            lblStats.Font = new Font("Segoe UI", 9F);
            lblStats.ForeColor = Color.FromArgb(180, 190, 210);
            lblStats.Location = new Point(4, 544);
            lblStats.Name = "lblStats";
            lblStats.Size = new Size(134, 20);
            lblStats.TabIndex = 5;
            lblStats.Text = "Строк: 0 | Символов: 0";
            // 
            // panelRight
            // 
            panelRight.BackColor = Color.FromArgb(18, 22, 33);
            panelRight.Controls.Add(lblResult);
            panelRight.Controls.Add(lblToLang);
            panelRight.Controls.Add(comboToLanguage);
            panelRight.Controls.Add(txtResult);
            panelRight.Controls.Add(lblConverting);
            panelRight.Controls.Add(progressBar);
            panelRight.Dock = DockStyle.Fill;
            panelRight.Location = new Point(2, 2);
            panelRight.Name = "panelRight";
            panelRight.Padding = new Padding(2);
            panelRight.Size = new Size(592, 586);
            panelRight.TabIndex = 1;
            // 
            // lblResult
            // 
            lblResult.AutoSize = true;
            lblResult.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblResult.ForeColor = Color.FromArgb(0, 255, 157);
            lblResult.Location = new Point(10, 10);
            lblResult.Name = "lblResult";
            lblResult.Size = new Size(110, 28);
            lblResult.TabIndex = 0;
            lblResult.Text = "📤 Результат";
            // 
            // lblToLang
            // 
            lblToLang.AutoSize = true;
            lblToLang.Font = new Font("Segoe UI", 9F);
            lblToLang.ForeColor = Color.FromArgb(180, 190, 210);
            lblToLang.Location = new Point(10, 50);
            lblToLang.Name = "lblToLang";
            lblToLang.Size = new Size(98, 20);
            lblToLang.TabIndex = 4;
            lblToLang.Text = "Конверт в:";
            // 
            // comboToLanguage
            // 
            comboToLanguage.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            comboToLanguage.BackColor = Color.FromArgb(25, 28, 40);
            comboToLanguage.DrawMode = DrawMode.OwnerDrawFixed;
            comboToLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
            comboToLanguage.FlatStyle = FlatStyle.Popup;
            comboToLanguage.Font = new Font("Segoe UI", 10F);
            comboToLanguage.ForeColor = Color.White;
            comboToLanguage.FormattingEnabled = true;
            comboToLanguage.ItemHeight = 29;
            comboToLanguage.Location = new Point(120, 45);
            comboToLanguage.Name = "comboToLanguage";
            comboToLanguage.Size = new Size(290, 36);
            comboToLanguage.TabIndex = 3;
            // 
            // txtResult
            // 
            txtResult.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtResult.BackColor = Color.FromArgb(25, 28, 40);
            txtResult.BorderStyle = BorderStyle.FixedSingle;
            txtResult.Font = new Font("Consolas", 10F);
            txtResult.ForeColor = Color.White;
            txtResult.Location = new Point(10, 90);
            txtResult.Name = "txtResult";
            txtResult.ReadOnly = true;
            txtResult.Size = new Size(558, 455);
            txtResult.TabIndex = 1;
            txtResult.Text = "";
            txtResult.WordWrap = false;
            txtResult.ScrollBars = RichTextBoxScrollBars.None;
            // 
            // lblConverting
            // 
            lblConverting.Anchor = AnchorStyles.None;
            lblConverting.AutoSize = true;
            lblConverting.Font = new Font("Segoe UI", 11F);
            lblConverting.ForeColor = Color.FromArgb(0, 212, 255);
            lblConverting.Location = new Point(200, 250);
            lblConverting.Name = "lblConverting";
            lblConverting.Size = new Size(189, 25);
            lblConverting.TabIndex = 6;
            lblConverting.Text = "Конвертируем код...";
            lblConverting.Visible = false;
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.None;
            progressBar.BackColor = Color.FromArgb(18, 22, 33);
            progressBar.ForeColor = Color.FromArgb(0, 212, 255);
            progressBar.Location = new Point(150, 290);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(289, 25);
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.TabIndex = 5;
            progressBar.Visible = false;
            // 
            // panelBottom
            // 
            panelBottom.BackColor = Color.FromArgb(25, 28, 40);
            panelBottom.BorderStyle = BorderStyle.None;
            panelBottom.Controls.Add(lblStatus);
            panelBottom.Controls.Add(btnHistory);
            panelBottom.Controls.Add(btnCopy);
            panelBottom.Controls.Add(btnConvert);
            panelBottom.Dock = DockStyle.Bottom;
            panelBottom.Location = new Point(0, 618);
            panelBottom.Name = "panelBottom";
            panelBottom.Size = new Size(1200, 82);
            panelBottom.TabIndex = 2;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Left;
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(180, 190, 210);
            lblStatus.Location = new Point(20, 31);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(62, 20);
            lblStatus.TabIndex = 3;
            lblStatus.Text = "Готово";
            // 
            // btnHistory
            // 
            btnHistory.Anchor = AnchorStyles.None;
            btnHistory.FlatStyle = FlatStyle.Flat;
            btnHistory.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnHistory.ForeColor = Color.White;
            btnHistory.Location = new Point(310, 21);
            btnHistory.Name = "btnHistory";
            btnHistory.Size = new Size(180, 40);
            btnHistory.TabIndex = 2;
            btnHistory.Text = "📜 История";
            btnHistory.UseVisualStyleBackColor = false;
            // 
            // btnCopy
            // 
            btnCopy.Anchor = AnchorStyles.None;
            btnCopy.FlatStyle = FlatStyle.Flat;
            btnCopy.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnCopy.ForeColor = Color.White;
            btnCopy.Location = new Point(710, 21);
            btnCopy.Name = "btnCopy";
            btnCopy.Size = new Size(180, 40);
            btnCopy.TabIndex = 1;
            btnCopy.Text = "📋 Копировать";
            btnCopy.UseVisualStyleBackColor = false;
            // 
            // btnConvert
            // 
            btnConvert.Anchor = AnchorStyles.None;
            btnConvert.FlatStyle = FlatStyle.Flat;
            btnConvert.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnConvert.ForeColor = Color.White;
            btnConvert.Location = new Point(510, 21);
            btnConvert.Name = "btnConvert";
            btnConvert.Size = new Size(180, 40);
            btnConvert.TabIndex = 0;
            btnConvert.Text = "🚀 Конвертировать";
            btnConvert.UseVisualStyleBackColor = false;
            // 
            // menuStrip1
            // 
            menuStrip1.BackColor = Color.FromArgb(18, 22, 33);
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { файлToolStripMenuItem, историяToolStripMenuItem, помощьToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1200, 28);
            menuStrip1.TabIndex = 6;
            menuStrip1.Text = "menuStrip1";
            // 
            // файлToolStripMenuItem
            // 
            файлToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { импортToolStripMenuItem, экспортToolStripMenuItem, выходToolStripMenuItem });
            файлToolStripMenuItem.ForeColor = Color.FromArgb(240, 245, 255);
            файлToolStripMenuItem.Name = "файлToolStripMenuItem";
            файлToolStripMenuItem.Size = new Size(59, 24);
            файлToolStripMenuItem.Text = "Файл";
            // 
            // импортToolStripMenuItem
            // 
            импортToolStripMenuItem.BackColor = Color.FromArgb(25, 28, 40);
            импортToolStripMenuItem.ForeColor = Color.FromArgb(240, 245, 255);
            импортToolStripMenuItem.Name = "импортToolStripMenuItem";
            импортToolStripMenuItem.Size = new Size(246, 26);
            импортToolStripMenuItem.Text = "Импорт файла";
            // 
            // экспортToolStripMenuItem
            // 
            экспортToolStripMenuItem.BackColor = Color.FromArgb(25, 28, 40);
            экспортToolStripMenuItem.ForeColor = Color.FromArgb(240, 245, 255);
            экспортToolStripMenuItem.Name = "экспортToolStripMenuItem";
            экспортToolStripMenuItem.Size = new Size(246, 26);
            экспортToolStripMenuItem.Text = "Экспорт результата";
            // 
            // выходToolStripMenuItem
            // 
            выходToolStripMenuItem.BackColor = Color.FromArgb(25, 28, 40);
            выходToolStripMenuItem.ForeColor = Color.FromArgb(240, 245, 255);
            выходToolStripMenuItem.Name = "выходToolStripMenuItem";
            выходToolStripMenuItem.Size = new Size(246, 26);
            выходToolStripMenuItem.Text = "Выход";
            // 
            // историяToolStripMenuItem
            // 
            историяToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { открытьИсториюToolStripMenuItem, очиститьИсториюToolStripMenuItem });
            историяToolStripMenuItem.ForeColor = Color.FromArgb(240, 245, 255);
            историяToolStripMenuItem.Name = "историяToolStripMenuItem";
            историяToolStripMenuItem.Size = new Size(81, 24);
            историяToolStripMenuItem.Text = "История";
            // 
            // открытьИсториюToolStripMenuItem
            // 
            открытьИсториюToolStripMenuItem.BackColor = Color.FromArgb(25, 28, 40);
            открытьИсториюToolStripMenuItem.ForeColor = Color.FromArgb(240, 245, 255);
            открытьИсториюToolStripMenuItem.Name = "открытьИсториюToolStripMenuItem";
            открытьИсториюToolStripMenuItem.Size = new Size(229, 26);
            открытьИсториюToolStripMenuItem.Text = "Открыть историю";
            // 
            // очиститьИсториюToolStripMenuItem
            // 
            очиститьИсториюToolStripMenuItem.BackColor = Color.FromArgb(25, 28, 40);
            очиститьИсториюToolStripMenuItem.ForeColor = Color.FromArgb(240, 245, 255);
            очиститьИсториюToolStripMenuItem.Name = "очиститьИсториюToolStripMenuItem";
            очиститьИсториюToolStripMenuItem.Size = new Size(229, 26);
            очиститьИсториюToolStripMenuItem.Text = "Очистить историю";
            // 
            // помощьToolStripMenuItem
            // 
            помощьToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { оПрограммеToolStripMenuItem });
            помощьToolStripMenuItem.ForeColor = Color.FromArgb(240, 245, 255);
            помощьToolStripMenuItem.Name = "помощьToolStripMenuItem";
            помощьToolStripMenuItem.Size = new Size(83, 24);
            помощьToolStripMenuItem.Text = "Помощь";
            // 
            // оПрограммеToolStripMenuItem
            // 
            оПрограммеToolStripMenuItem.BackColor = Color.FromArgb(25, 28, 40);
            оПрограммеToolStripMenuItem.ForeColor = Color.FromArgb(240, 245, 255);
            оПрограммеToolStripMenuItem.Name = "оПрограммеToolStripMenuItem";
            оПрограммеToolStripMenuItem.Size = new Size(202, 26);
            оПрограммеToolStripMenuItem.Text = "О программе";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(8, 10, 18);
            ClientSize = new Size(1200, 700);
            Controls.Add(splitContainer1);
            Controls.Add(panelBottom);
            Controls.Add(menuStrip1);
            ForeColor = Color.White;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            MinimumSize = new Size(900, 600);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SCode AI";
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.ResumeLayout(false);
            panelLeft.ResumeLayout(false);
            panelLeft.PerformLayout();
            panelRight.ResumeLayout(false);
            panelRight.PerformLayout();
            panelBottom.ResumeLayout(false);
            panelBottom.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private SplitContainer splitContainer1;
        private Panel panelLeft;
        private Label lblStats;
        private Button btnAutoDetect;
        private Konvertor.Controls.IconComboBox comboFromLanguage;
        private RichTextBox txtSourceCode;
        private Label lblSource;
        private Label lblFromLang;
        private Panel panelRight;
        private Label lblConverting;
        private ProgressBar progressBar;
        private Label lblToLang;
        private Konvertor.Controls.IconComboBox comboToLanguage;
        private RichTextBox txtResult;
        private Label lblResult;
        private Panel panelBottom;
        private Label lblBottomStats;
        private Button btnConvert;
        private Button btnCopy;
        private Button btnHistory;
        private Label lblStatus;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem файлToolStripMenuItem;
        private ToolStripMenuItem импортToolStripMenuItem;
        private ToolStripMenuItem экспортToolStripMenuItem;
        private ToolStripMenuItem выходToolStripMenuItem;
        private ToolStripMenuItem историяToolStripMenuItem;
        private ToolStripMenuItem открытьИсториюToolStripMenuItem;
        private ToolStripMenuItem очиститьИсториюToolStripMenuItem;
        private ToolStripMenuItem помощьToolStripMenuItem;
        private ToolStripMenuItem оПрограммеToolStripMenuItem;
    }
}
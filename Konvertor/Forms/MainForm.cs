using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
using Konvertor.Services;    // ← ЭТА СТРОКА ОБЯЗАТЕЛЬНО!
using Konvertor.Models;
using Timer = System.Windows.Forms.Timer;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Konvertor.Forms
{
    public partial class MainForm : Form
    {
        private PythonService _pythonService;
        private HistoryService _historyService;
        private readonly string _apiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY") ?? "YOUR_GROQ_API_KEY_HERE";

        private const int _editorPaddingLeft = 8;
        private const int _editorPaddingTop = 4;
        private const int _editorPaddingRight = 4;
        private const int _editorPaddingBottom = 2;

        private Button btnClearResult;
        private Panel lineNumbersLeft;
        private Panel lineNumbersRight;
        private Label lblResultStats;
        private System.Windows.Forms.Timer lineNumberTimer;
        private int _lastFirstVisibleLineLeft = -1;
        private int _lastFirstVisibleLineRight = -1;

        private bool _isHoveringConvert = false;
        private Color _convertColorCurrent;
        private Color _convertColorTarget;
        private System.Windows.Forms.Timer _convertColorTimer;
        private int _convertColorAnimStep = 0;
        private const int _convertColorAnimSteps = 12;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, ref Rectangle lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        private const int EM_SETRECT = 0xB3;
        private const int EM_GETFIRSTVISIBLELINE = 0xCE;

        // Scintilla для подсветки синтаксиса (не используется)
        // private Scintilla txtSourceCodeEditor;
        // private Scintilla txtResultEditor;

        // Современная синяя тема
        private Color _darkBg = Color.FromArgb(15, 23, 42); // Темно-синий фон
        private Color _panelBg = Color.FromArgb(30, 41, 59); // Светло-синий панели
        private Color _accentBlue = Color.FromArgb(59, 130, 246); // Ярко-синий акцент
        private Color _accentPurple = Color.FromArgb(147, 51, 234); // Фиолетовый акцент
        private Color _accentGreen = Color.FromArgb(34, 197, 94); // Зеленый акцент
        private Color _textColor = Color.FromArgb(226, 232, 240); // Светло-серый текст
        private Color _textSecondary = Color.FromArgb(148, 163, 184); // Темно-серый вторичный текст

        private Timer _sourceSyntaxTimer;
        private bool _isApplyingSourceHighlight;

        private enum ConvertActionMode
        {
            Convert = 0,
            Analyze = 1
        }

        private ConvertActionMode _convertMode = ConvertActionMode.Convert;
        private bool _suppressConvertClick;
        private ContextMenuStrip _convertModeMenu;

        private bool _isFullscreen;
        private FormBorderStyle _prevBorderStyle;
        private FormWindowState _prevWindowState;
        private Rectangle _prevBounds;
        private bool _prevMenuVisible;

        public MainForm()
        {
            InitializeComponent();
            InitializeNeonTheme();

            InitializeServices();
            LoadLanguages();

            SetupEventHandlers();

            // Фиксируем размеры
            this.Size = new Size(1180, 750);
            this.MinimumSize = new Size(1080, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Иконка
            if (File.Exists("Resources/main.ico"))
            {
                this.Icon = new Icon("Resources/main.ico");
            }

            // Устанавливаем начальные значения
            UpdateStats();

            _sourceSyntaxTimer = new Timer();
            _sourceSyntaxTimer.Interval = 250;
            _sourceSyntaxTimer.Tick += (s, e) =>
            {
                _sourceSyntaxTimer.Stop();
                ApplySourceSyntaxHighlight();
            };

            txtSourceCode.TextChanged += txtSourceCode_TextChanged;

            // Layout управляется через UpdateLayout() (как в стабильном Konvertor2)

            // Тестовый код для демонстрации
            txtSourceCode.Text = @"def fibonacci(n):

    if n <= 1:
        return n

    else:
        return fibonacci(n-1) + fibonacci(n-2)

print(fibonacci(10))";

            ApplyEditorPadding(txtSourceCode);
            ApplyEditorPadding(txtResult);
            ScheduleSourceSyntaxHighlight(true);
        }

        private void ApplyEditorPadding(RichTextBox box)
        {
            if (box == null)
                return;

            void apply()
            {
                try
                {
                    if (!box.IsHandleCreated) return;
                    var rect = box.ClientRectangle;
                    rect = new Rectangle(
                        rect.Left + _editorPaddingLeft,
                        rect.Top + _editorPaddingTop,
                        Math.Max(0, rect.Width - _editorPaddingLeft - _editorPaddingRight),
                        Math.Max(0, rect.Height - _editorPaddingTop - _editorPaddingBottom)
                    );
                    SendMessage(box.Handle, EM_SETRECT, 0, ref rect);
                }
                catch
                {
                }
            }

            if (box.IsHandleCreated)
            {
                apply();
            }
            else
            {
                box.HandleCreated += (s, e) => apply();
            }

            box.Resize += (s, e) => apply();
        }

        private void SetupEventHandlers()
        {
            btnConvert.Click += btnConvert_Click;
            btnCopy.Click += btnCopy_Click;
            btnHistory.Click += btnHistory_Click;
            btnAutoDetect.Click += btnAutoDetect_Click;

            KeyPreview = true;
            KeyDown += MainForm_KeyDown;

            txtSourceCode.TextChanged += (s, e) => { InvalidateLineNumbers(); UpdateStats(); };
            txtResult.TextChanged += (s, e) => { InvalidateLineNumbers(); UpdateResultStats(); };

            lineNumberTimer = new System.Windows.Forms.Timer();
            lineNumberTimer.Interval = 150;
            lineNumberTimer.Tick += (s, e) =>
            {
                int left = GetFirstVisibleLineSafe(txtSourceCode);
                int right = GetFirstVisibleLineSafe(txtResult);

                if (left != _lastFirstVisibleLineLeft)
                {
                    _lastFirstVisibleLineLeft = left;
                    lineNumbersLeft?.Invalidate();
                }

                if (right != _lastFirstVisibleLineRight)
                {
                    _lastFirstVisibleLineRight = right;
                    lineNumbersRight?.Invalidate();
                }
            };
            lineNumberTimer.Start();

            panelLeft.Resize += (s, e) => { UpdateLayout(); InvalidateLineNumbers(); };
            panelRight.Resize += (s, e) => { UpdateLayout(); InvalidateLineNumbers(); };
            splitContainer1.SplitterMoved += (s, e) => { UpdateLayout(); InvalidateLineNumbers(); };
            splitContainer1.SplitterMoving += (s, e) => { SuspendLayout(); };
            splitContainer1.SplitterMoved += (s, e) => { ResumeLayout(); };

            Resize += (s, e) => { if (WindowState != FormWindowState.Minimized) UpdateLayout(); };
            Shown += (s, e) => UpdateLayout();

            импортToolStripMenuItem.Click += импортToolStripMenuItem_Click;
            экспортToolStripMenuItem.Click += экспортToolStripMenuItem_Click;
            выходToolStripMenuItem.Click += выходToolStripMenuItem_Click;
            открытьИсториюToolStripMenuItem.Click += открытьИсториюToolStripMenuItem_Click;
            очиститьИсториюToolStripMenuItem.Click += очиститьИсториюToolStripMenuItem_Click;
            оПрограммеToolStripMenuItem.Click += оПрограммеToolStripMenuItem_Click;
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.F11)
                return;

            e.Handled = true;
            ToggleFullscreen();
        }

        private void ToggleFullscreen()
        {
            try
            {
                if (!_isFullscreen)
                {
                    _isFullscreen = true;
                    _prevBorderStyle = FormBorderStyle;
                    _prevWindowState = WindowState;
                    _prevBounds = Bounds;
                    _prevMenuVisible = menuStrip1?.Visible ?? true;

                    if (menuStrip1 != null)
                        menuStrip1.Visible = false;

                    FormBorderStyle = FormBorderStyle.None;
                    WindowState = FormWindowState.Normal;
                    Bounds = Screen.FromControl(this).Bounds;
                }
                else
                {
                    _isFullscreen = false;

                    if (menuStrip1 != null)
                        menuStrip1.Visible = _prevMenuVisible;

                    FormBorderStyle = _prevBorderStyle;
                    WindowState = FormWindowState.Normal;
                    Bounds = _prevBounds;
                    WindowState = _prevWindowState;
                }

                UpdateLayout();
                InvalidateLineNumbers();
            }
            catch
            {
            }
        }

        private sealed class DarkMenuRenderer : ToolStripProfessionalRenderer
        {
            private readonly Color _bg;
            private readonly Color _itemHover;
            private readonly Color _itemPressed;
            private readonly Color _text;

            public DarkMenuRenderer(Color bg, Color itemHover, Color itemPressed, Color text)
                : base(new ProfessionalColorTable())
            {
                _bg = bg;
                _itemHover = itemHover;
                _itemPressed = itemPressed;
                _text = text;
                RoundedEdges = false;
            }

            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                e.Graphics.Clear(_bg);
            }

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                Color c = _bg;
                if (e.Item.Pressed) c = _itemPressed;
                else if (e.Item.Selected) c = _itemHover;

                using (var b = new SolidBrush(c))
                {
                    e.Graphics.FillRectangle(b, new Rectangle(Point.Empty, e.Item.Size));
                }
            }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = _text;
                base.OnRenderItemText(e);
            }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
            }
        }

        private void InitializeNeonTheme()
        {
            // Настройка темы формы
            this.BackColor = _darkBg;
            this.ForeColor = _textColor;

            this.Font = new Font("Segoe UI", 9);
            this.DoubleBuffered = true;

            // Настройка цветов контролов
            panelLeft.BackColor = _panelBg;
            panelRight.BackColor = _panelBg;

            // Настройка RichTextBox
            txtResult.Font = new Font("Consolas", 10);
            txtResult.WordWrap = false;

            btnClearResult = new Button();
            btnClearResult.Text = "🗑️ Очистить";
            btnClearResult.Size = new Size(130, 35);
            btnClearResult.Click += btnClearResult_Click;
            panelRight.Controls.Add(btnClearResult);

            lineNumbersLeft = new Panel();
            lineNumbersLeft.BackColor = panelLeft.BackColor;
            lineNumbersLeft.Width = 26;
            lineNumbersLeft.Paint += LineNumbersLeft_Paint;
            EnableDoubleBuffering(lineNumbersLeft);
            panelLeft.Controls.Add(lineNumbersLeft);

            lineNumbersRight = new Panel();
            lineNumbersRight.BackColor = panelRight.BackColor;
            lineNumbersRight.Width = 26;
            lineNumbersRight.Paint += LineNumbersRight_Paint;
            EnableDoubleBuffering(lineNumbersRight);
            panelRight.Controls.Add(lineNumbersRight);

            lblResultStats = new Label();
            lblResultStats.AutoSize = true;
            lblResultStats.Font = new Font("Segoe UI", 9F);
            lblResultStats.ForeColor = _textSecondary;
            lblResultStats.Text = "📊 Строк: 0 | Символов: 0 | Слов: 0";
            panelRight.Controls.Add(lblResultStats);

            // Настройка заголовков
            lblSource.ForeColor = _accentBlue;
            lblResult.ForeColor = _accentGreen;

            // Настройка текстовых полей
            txtSourceCode.BackColor = Color.FromArgb(26, 26, 46);
            txtSourceCode.ForeColor = _textColor;
            txtSourceCode.BorderStyle = BorderStyle.FixedSingle;
            txtSourceCode.HideSelection = false;

            txtResult.BackColor = Color.FromArgb(26, 26, 46);
            txtResult.ForeColor = _textColor;
            txtResult.BorderStyle = BorderStyle.FixedSingle;
            txtResult.HideSelection = false;

            // Настройка кнопок
            _convertColorCurrent = _accentPurple;
            _convertColorTarget = _accentPurple;
            StyleButtonGradient(btnConvert, _accentPurple);
            btnConvert.MouseEnter += BtnConvert_MouseEnter;
            btnConvert.MouseLeave += BtnConvert_MouseLeave;
            btnConvert.MouseDown += BtnConvert_MouseDown;

            btnAutoDetect.BackColor = Color.Transparent;
            btnAutoDetect.ForeColor = _accentBlue;
            btnAutoDetect.FlatStyle = FlatStyle.Flat;
            btnAutoDetect.FlatAppearance.BorderColor = _accentBlue;
            btnAutoDetect.FlatAppearance.BorderSize = 1;
            btnAutoDetect.FlatAppearance.MouseOverBackColor = Color.FromArgb(35, 45, 70);
            btnAutoDetect.FlatAppearance.MouseDownBackColor = Color.FromArgb(45, 58, 90);
            btnAutoDetect.UseVisualStyleBackColor = false;

            btnCopy.BackColor = Color.Transparent;
            btnCopy.ForeColor = _accentGreen;
            btnCopy.FlatStyle = FlatStyle.Flat;
            btnCopy.FlatAppearance.BorderColor = _accentGreen;
            btnCopy.FlatAppearance.BorderSize = 1;
            btnCopy.FlatAppearance.MouseOverBackColor = Color.FromArgb(35, 45, 70);
            btnCopy.FlatAppearance.MouseDownBackColor = Color.FromArgb(45, 58, 90);
            btnCopy.UseVisualStyleBackColor = false;

            // Настройка комбобоксов
            comboFromLanguage.BackColor = Color.FromArgb(26, 26, 46);
            comboFromLanguage.ForeColor = _textColor;
            comboFromLanguage.FlatStyle = FlatStyle.Popup;

            comboFromLanguage.MinimumSize = new Size(130, 35);
            comboFromLanguage.Size = new Size(130, 35);

            comboToLanguage.BackColor = Color.FromArgb(26, 26, 46);
            comboToLanguage.ForeColor = _textColor;
            comboToLanguage.FlatStyle = FlatStyle.Popup;

            comboToLanguage.MinimumSize = new Size(130, 35);
            comboToLanguage.Size = new Size(130, 35);

            // Настройка меток
            lblStats.ForeColor = _textSecondary;
            lblStatus.ForeColor = _textSecondary;
            lblFromLang.ForeColor = _textSecondary;
            lblToLang.ForeColor = _textSecondary;

            menuStrip1.Renderer = new DarkMenuRenderer(
                _darkBg,
                Color.FromArgb(35, 45, 70),
                Color.FromArgb(45, 58, 90),
                _textColor
            );

            InitializeConvertModeMenu();

            UpdateLayout();

            ApplyEditorPadding(txtSourceCode);
            ApplyEditorPadding(txtResult);
        }

        private void InitializeConvertModeMenu()
        {
            _convertModeMenu = new ContextMenuStrip();
            _convertModeMenu.Renderer = new DarkMenuRenderer(
                _darkBg,
                Color.FromArgb(35, 45, 70),
                Color.FromArgb(45, 58, 90),
                _textColor
            );

            var itemConvert = new ToolStripMenuItem("Конвертация");
            var itemAnalyze = new ToolStripMenuItem("Анализ");

            itemConvert.Click += (s, e) => SetConvertMode(ConvertActionMode.Convert);
            itemAnalyze.Click += (s, e) => SetConvertMode(ConvertActionMode.Analyze);

            _convertModeMenu.Items.Add(itemConvert);
            _convertModeMenu.Items.Add(itemAnalyze);

            UpdateConvertModeMenuChecks();
        }

        private void SetConvertMode(ConvertActionMode mode)
        {
            _convertMode = mode;
            UpdateConvertModeMenuChecks();
            UpdateButtonColors();
            btnConvert.Invalidate();

            lblStatus.Text = _convertMode == ConvertActionMode.Convert
                ? "Режим: конвертация"
                : "Режим: анализ";
        }

        private void UpdateConvertModeMenuChecks()
        {
            if (_convertModeMenu == null) return;
            if (_convertModeMenu.Items.Count < 2) return;

            if (_convertModeMenu.Items[0] is ToolStripMenuItem c)
                c.Checked = _convertMode == ConvertActionMode.Convert;
            if (_convertModeMenu.Items[1] is ToolStripMenuItem a)
                a.Checked = _convertMode == ConvertActionMode.Analyze;
        }

        private void BtnConvert_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;

            const int arrowAreaWidth = 28;
            if (e.X >= btnConvert.Width - arrowAreaWidth)
            {
                _suppressConvertClick = true;
                UpdateConvertModeMenuChecks();
                _convertModeMenu?.Show(btnConvert, new Point(btnConvert.Width, 0));
            }
        }

        private void btnClearResult_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtResult.Text))
            {
                txtResult.Clear();
                lblStatus.Text = "✅ Результат очищен";
                UpdateResultStats();
            }
        }

        private void UpdateResultStats()
        {
            try
            {
                var text = txtResult.Text;
                var lines = string.IsNullOrWhiteSpace(text)
                    ? 0
                    : text.Split('\n')
                        .Select(l => l.TrimEnd('\r'))
                        .Count(l => !string.IsNullOrWhiteSpace(l));
                var chars = text.Count(ch => !char.IsWhiteSpace(ch));
                var words = text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
                if (lblResultStats != null)
                    lblResultStats.Text = $"📊 Строк: {lines} | Символов: {chars} | Слов: {words}";
            }
            catch
            {
                if (lblResultStats != null)
                    lblResultStats.Text = "📊 Строк: 0 | Символов: 0 | Слов: 0";
            }
        }

        private void InvalidateLineNumbers()
        {
            lineNumbersLeft?.Invalidate();
            lineNumbersRight?.Invalidate();
        }

        private static int GetFirstVisibleLineSafe(Control control)
        {
            try
            {
                if (control == null || !control.IsHandleCreated) return 0;
                return Math.Max(0, SendMessage(control.Handle, EM_GETFIRSTVISIBLELINE, 0, 0));
            }
            catch
            {
                return 0;
            }
        }

        private static void EnableDoubleBuffering(Control control)
        {
            if (control == null) return;
            try
            {
                typeof(Control)
                    .GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(control, true, null);
            }
            catch
            {
            }
        }

        private void LineNumbersLeft_Paint(object sender, PaintEventArgs e)
        {
            if (lineNumbersLeft == null || txtSourceCode == null) return;

            int firstLine = GetFirstVisibleLineSafe(txtSourceCode);
            int lineHeight = TextRenderer.MeasureText("A", txtSourceCode.Font).Height;

            e.Graphics.Clear(lineNumbersLeft.BackColor);
            using (var brush = new SolidBrush(_textSecondary))
            {
                int visibleLines = Math.Max(1, lineNumbersLeft.Height / lineHeight);
                for (int i = 0; i <= visibleLines; i++)
                {
                    int lineNumber = firstLine + i + 1;
                    int y = (i * lineHeight) + _editorPaddingTop;
                    var text = lineNumber.ToString();
                    var size = e.Graphics.MeasureString(text, txtSourceCode.Font);
                    float x = lineNumbersLeft.Width - size.Width - 2;
                    e.Graphics.DrawString(text, txtSourceCode.Font, brush, x, y);
                }
            }
        }

        private void LineNumbersRight_Paint(object sender, PaintEventArgs e)
        {
            if (lineNumbersRight == null || txtResult == null) return;

            e.Graphics.Clear(lineNumbersRight.BackColor);

            int firstLine = GetFirstVisibleLineSafe(txtResult);
            int lineHeight = TextRenderer.MeasureText("A", txtResult.Font).Height;
            using (var brush = new SolidBrush(_textSecondary))
            {
                int visibleLines = Math.Max(1, lineNumbersRight.Height / lineHeight);
                for (int i = 0; i <= visibleLines; i++)
                {
                    int lineNumber = firstLine + i + 1;
                    int y = (i * lineHeight) + _editorPaddingTop;
                    var text = lineNumber.ToString();
                    var size = e.Graphics.MeasureString(text, txtResult.Font);
                    float x = lineNumbersRight.Width - size.Width - 2;
                    e.Graphics.DrawString(text, txtResult.Font, brush, x, y);
                }
            }
        }

        private void StyleButtonBorder(Button button, Color glowColor)
        {
            if (button == null) return;
            button.BackColor = Color.Transparent;
            button.ForeColor = glowColor;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = glowColor;

            button.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            button.UseVisualStyleBackColor = false;
        }

        private void StyleButtonGradient(Button button, Color baseColor)
        {
            if (button == null) return;

            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.Transparent;
            button.FlatAppearance.MouseDownBackColor = Color.Transparent;
            button.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            button.BackColor = baseColor;
            button.UseVisualStyleBackColor = false;
            button.Cursor = Cursors.Hand;

            ApplyRoundedRegion(button, 12);
            button.Resize += (s, e) => ApplyRoundedRegion(button, 12);

            button.Paint += (s, e) =>
            {
                var btn = s as Button;
                if (btn == null) return;

                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                if (btn.Parent != null)
                    e.Graphics.Clear(btn.Parent.BackColor);

                var currentBase = (btn == btnConvert) ? _convertColorCurrent : btn.BackColor;

                Color topColor;
                Color bottomColor;
                if (_isHoveringConvert && btn == btnConvert)
                {
                    topColor = Color.FromArgb(
                        Math.Min(255, currentBase.R + 50),
                        Math.Min(255, currentBase.G + 50),
                        Math.Min(255, currentBase.B + 50));
                    bottomColor = Color.FromArgb(
                        Math.Max(0, currentBase.R - 10),
                        Math.Max(0, currentBase.G - 10),
                        Math.Max(0, currentBase.B - 10));
                }
                else
                {
                    topColor = Color.FromArgb(
                        Math.Min(255, currentBase.R + 35),
                        Math.Min(255, currentBase.G + 35),
                        Math.Min(255, currentBase.B + 35));
                    bottomColor = Color.FromArgb(
                        Math.Max(0, currentBase.R - 20),
                        Math.Max(0, currentBase.G - 20),
                        Math.Max(0, currentBase.B - 20));
                }

                var rect = new Rectangle(0, 0, btn.Width - 1, btn.Height - 1);
                using (var path = CreateRoundedRectanglePath(rect, 12))
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(rect, topColor, bottomColor, System.Drawing.Drawing2D.LinearGradientMode.Vertical))
                {
                    e.Graphics.FillPath(brush, path);
                }

                TextRenderer.DrawText(e.Graphics, btn.Text, btn.Font, btn.ClientRectangle, btn.ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                if (btn == btnConvert)
                {
                    const int arrowAreaWidth = 28;
                    int x = btn.Width - arrowAreaWidth;

                    using (var pen = new Pen(Color.FromArgb(80, 255, 255, 255)))
                    {
                        e.Graphics.DrawLine(pen, x, 8, x, btn.Height - 8);
                    }

                    var arrowRect = new Rectangle(x, 0, arrowAreaWidth, btn.Height);
                    TextRenderer.DrawText(e.Graphics, "▼", btn.Font, arrowRect, btn.ForeColor,
                        TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                }
            };
        }

        private void BtnConvert_MouseEnter(object sender, EventArgs e)
        {
            _isHoveringConvert = true;
            btnConvert.Invalidate();
        }

        private void BtnConvert_MouseLeave(object sender, EventArgs e)
        {
            _isHoveringConvert = false;
            btnConvert.Invalidate();
        }

        private static System.Drawing.Drawing2D.GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static void ApplyRoundedRegion(Control control, int radius)
        {
            if (control == null) return;
            if (control.Width <= 1 || control.Height <= 1) return;

            using (var path = CreateRoundedRectanglePath(new Rectangle(0, 0, control.Width - 1, control.Height - 1), radius))
            {
                control.Region?.Dispose();
                control.Region = new Region(path);
            }
        }

        private void UpdateLayout()
        {
            try
            {
                SuspendLayout();

                int menuHeight = menuStrip1.Height;
                int bottomPanelHeight = panelBottom.Height;
                int margin = 4;

                panelBottom.Dock = DockStyle.Bottom;
                panelBottom.Width = ClientSize.Width;
                panelBottom.Location = new Point(0, ClientSize.Height - panelBottom.Height);
                panelBottom.BringToFront();

                splitContainer1.Location = new Point(margin, menuHeight + margin);
                splitContainer1.Size = new Size(
                    ClientSize.Width - (margin * 2),
                    ClientSize.Height - menuHeight - bottomPanelHeight - (margin * 2)
                );

                int leftPanelWidth = panelLeft.ClientSize.Width;
                int rightPanelWidth = panelRight.ClientSize.Width;

                int comboWidth = 130;
                int buttonWidth = 130;
                int languageY = 45;

                int leftBaseX = 70;
                int rightBaseX = 110;

                int leftAvailableWidth = leftPanelWidth - 90;
                bool leftHasSpace = leftAvailableWidth >= comboWidth + buttonWidth + 10;

                if (leftHasSpace)
                {
                    comboFromLanguage.Location = new Point(leftBaseX, languageY);
                    comboFromLanguage.Size = new Size(comboWidth, 35);
                    btnAutoDetect.Location = new Point(comboFromLanguage.Right + 10, languageY);
                    btnAutoDetect.Size = new Size(buttonWidth, 35);
                }
                else
                {
                    comboFromLanguage.Location = new Point(leftBaseX, languageY);
                    comboFromLanguage.Size = new Size(Math.Min(comboWidth, leftAvailableWidth), 35);
                    btnAutoDetect.Location = new Point(leftBaseX, comboFromLanguage.Bottom + 5);
                    btnAutoDetect.Size = new Size(Math.Min(buttonWidth, leftAvailableWidth), 35);
                }

                int rightAvailableWidth = rightPanelWidth - 130;
                bool rightHasSpace = rightAvailableWidth >= comboWidth + buttonWidth + 10;

                if (rightHasSpace)
                {
                    comboToLanguage.Location = new Point(rightBaseX, languageY);
                    comboToLanguage.Size = new Size(comboWidth, 35);
                    btnClearResult.Location = new Point(comboToLanguage.Right + 10, languageY);
                    btnClearResult.Size = new Size(buttonWidth, 35);
                }
                else
                {
                    comboToLanguage.Location = new Point(rightBaseX, languageY);
                    comboToLanguage.Size = new Size(Math.Min(comboWidth, rightAvailableWidth), 35);
                    btnClearResult.Location = new Point(rightBaseX, comboToLanguage.Bottom + 5);
                    btnClearResult.Size = new Size(Math.Min(buttonWidth, rightAvailableWidth), 35);
                }

                int leftHeaderBottom = Math.Max(comboFromLanguage.Bottom, btnAutoDetect.Bottom);
                int rightHeaderBottom = Math.Max(comboToLanguage.Bottom, btnClearResult.Bottom);
                int leftTextTop = leftHeaderBottom + 8;
                int rightTextTop = rightHeaderBottom + 8;

                int textPadding = 6;
                int lineNumberWidth = 26;

                if (lineNumbersLeft != null)
                {
                    lineNumbersLeft.Location = new Point(textPadding, leftTextTop);
                    lineNumbersLeft.Size = new Size(lineNumberWidth, panelLeft.ClientSize.Height - leftTextTop - 42);
                    lineNumbersLeft.BringToFront();
                }

                txtSourceCode.Location = new Point(textPadding + lineNumberWidth + 4, leftTextTop);
                txtSourceCode.Size = new Size(
                    leftPanelWidth - (textPadding * 2) - lineNumberWidth - 4,
                    panelLeft.ClientSize.Height - leftTextTop - 42
                );

                ApplyEditorPadding(txtSourceCode);

                if (lineNumbersRight != null)
                {
                    lineNumbersRight.Location = new Point(textPadding, rightTextTop);
                    lineNumbersRight.Size = new Size(lineNumberWidth, panelRight.ClientSize.Height - rightTextTop - 42);
                    lineNumbersRight.BringToFront();
                }

                txtResult.Location = new Point(textPadding + lineNumberWidth + 4, rightTextTop);
                txtResult.Size = new Size(
                    rightPanelWidth - (textPadding * 2) - lineNumberWidth - 4,
                    panelRight.ClientSize.Height - rightTextTop - 42
                );

                ApplyEditorPadding(txtResult);

                if (lblStats != null)
                {
                    lblStats.Location = new Point(textPadding + lineNumberWidth + 5, panelLeft.ClientSize.Height - 24);
                    lblStats.BringToFront();
                }

                if (lblResultStats != null)
                {
                    lblResultStats.Location = new Point(textPadding + lineNumberWidth + 5, panelRight.ClientSize.Height - 24);
                    lblResultStats.BringToFront();
                }

                int buttonSpacing = 20;
                btnHistory.Size = new Size(180, 40);
                btnConvert.Size = new Size(180, 40);
                btnCopy.Size = new Size(180, 40);
                int totalButtonsWidth = btnHistory.Width + btnConvert.Width + btnCopy.Width + (buttonSpacing * 2);
                int startX = (panelBottom.Width - totalButtonsWidth) / 2;
                int bottomPadding = 12;

                btnHistory.Location = new Point(Math.Max(startX, 10), (panelBottom.Height - btnHistory.Height - bottomPadding));
                btnConvert.Location = new Point(btnHistory.Right + buttonSpacing, (panelBottom.Height - btnConvert.Height - bottomPadding));
                btnCopy.Location = new Point(btnConvert.Right + buttonSpacing, (panelBottom.Height - btnCopy.Height - bottomPadding));

                ResumeLayout(true);
                PerformLayout();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Layout error: {ex.Message}");
            }
        }

        private void InitializeServices()
        {
            try
            {
                _pythonService = new PythonService(_apiKey);
                _historyService = new HistoryService();

                lblStatus.Text = "✅ Сервисы инициализированы";
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"❌ Ошибка инициализации: {ex.Message}";
                btnConvert.Enabled = false;
            }
        }

        private void LoadLanguages()
        {
            try
            {
                var languages = Language.GetAvailableLanguages();

                comboFromLanguage.DataSource = null;
                comboFromLanguage.Items.Clear();
                comboFromLanguage.DisplayMember = "Name";
                comboFromLanguage.ValueMember = "Id";
                comboFromLanguage.DataSource = languages.ToList();

                comboToLanguage.DataSource = null;
                comboToLanguage.Items.Clear();
                comboToLanguage.DisplayMember = "Name";
                comboToLanguage.ValueMember = "Id";
                comboToLanguage.DataSource = languages.ToList();

                // Устанавливаем значения по умолчанию
                comboFromLanguage.SelectedIndex = 0; // Python
                comboToLanguage.SelectedIndex = 1;   // C#

                // Обновляем Labels при выборе языка
                comboFromLanguage.SelectedIndexChanged += (s, e) =>
                {
                    if (comboFromLanguage.SelectedItem is Language lang)
                    {
                        lblFromLang.Text = $"Язык:";
                        UpdateButtonColors();

                        ScheduleSourceSyntaxHighlight(true);
                    }
                };

                comboToLanguage.SelectedIndexChanged += (s, e) =>
                {
                    if (comboToLanguage.SelectedItem is Language lang)
                    {
                        lblToLang.Text = $"Конверт в:";
                        UpdateButtonColors();
                        // Обновляем подсветку синтаксиса для результата
                        if (!string.IsNullOrEmpty(txtResult.Text))
                        {
                            ApplySyntaxHighlighting(lang.Name);
                        }
                    }
                };

                UpdateButtonColors();

                // ↓↓↓ И эти тоже для начальных значений ↓↓↓
                lblFromLang.Text = $"Язык:";
                lblToLang.Text = $"Конверт в:";
                // ↑↑↑ И эти тоже для начальных значений ↑↑↑
            }
            catch (Exception ex)
            {
                DarkMessageBox.Show($"Ошибка загрузки языков: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnConvert_Click(object sender, EventArgs e)
        {
            if (_suppressConvertClick)
            {
                _suppressConvertClick = false;
                return;
            }

            if (_convertMode == ConvertActionMode.Analyze)
            {
                await AnalyzeCodeAsync();
                return;
            }

            await ConvertCodeAsync();
        }

        private async Task ConvertCodeAsync()
        {
            if (string.IsNullOrWhiteSpace(txtSourceCode.Text))
            {
                DarkMessageBox.Show("Введите код для конвертации", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Проверка, что введён код, а не произвольный текст
            if (!IsCode(txtSourceCode.Text))
            {
                DarkMessageBox.Show("Код не обнаружен. Пожалуйста, введите программный код.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (comboFromLanguage.SelectedItem == null || comboToLanguage.SelectedItem == null)
            {
                DarkMessageBox.Show("Выберите исходный и целевой языки", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var fromLang = (Language)comboFromLanguage.SelectedItem;
            var toLang = (Language)comboToLanguage.SelectedItem;

            if (fromLang.Id == toLang.Id)
            {
                DarkMessageBox.Show("Выберите разные языки для конвертации", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Показываем прогресс
            ShowLoading(true);
            lblStatus.Text = "Конвертация начата...";

            try
            {
                var result = await _pythonService.ConvertCodeAsync(
                    txtSourceCode.Text,
                    fromLang.Name,
                    toLang.Name
                );

                if (result.Success)
                {
                    // Используем новый метод для отображения
                    DisplayFormattedCode(result.ConvertedCode, toLang.Name);
                    UpdateStats();
                    lblStatus.Text = $"✅ Конвертация завершена: {fromLang.Name} → {toLang.Name}";

                    // Сохраняем в историю только успешные конвертации
                    _historyService.AddToHistory(result);
                }
                else
                {
                    txtResult.Text = $"❌ Ошибка конвертации:\n{result.ErrorMessage}";
                    lblStatus.Text = "❌ Произошла ошибка";

                    // Не сохраняем ошибки в историю
                }
            }
            catch (Exception ex)
            {
                txtResult.Text = $"💥 Критическая ошибка:\n{ex.Message}";
                lblStatus.Text = "💥 Ошибка соединения";
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private void DisplayFormattedCode(string code, string language)
        {
            // Очищаем текст
            txtResult.Clear();

            // Устанавливаем моноширинный шрифт
            txtResult.Font = new Font("Consolas", 10);
            txtResult.WordWrap = false; // Не переносить слова

            // Вставляем текст
            txtResult.Text = code;

            // Применяем подсветку синтаксиса если нужно
            ApplySyntaxHighlighting(language);
        }

        private string GetLanguageId(string languageName)
        {
            switch (languageName.ToLower())
            {
                case "python": return "python";
                case "javascript": return "javascript";
                case "c#": case "csharp": return "csharp";
                default: return "python";
            }
        }

        private bool IsCode(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            // Простая эвристика: проверка на наличие ключевых элементов кода
            var codeIndicators = new[]
            {
                "=", "(", ")", "{", "}", "[", "]", ";", ":", "def ", "function ", "class ", "if ", "for ", "while ",
                "import ", "using ", "var ", "let ", "const ", "int ", "string ", "public ", "private "
            };

            var hasCodeElements = codeIndicators.Any(indicator => text.Contains(indicator));

            // Дополнительно: проверка на наличие слов, типичных для кода
            var words = text.Split(new[] { ' ', '\n', '\r', '\t', '.', ',', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            var codeWords = words.Count(word => word.Length > 2 && !IsCommonWord(word.ToLower()));

            // Если есть элементы кода или много "кодовых" слов, считаем что это код
            return hasCodeElements || codeWords > words.Length * 0.3;
        }

        private bool IsCommonWord(string word)
        {
            var commonWords = new[] { "the", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with", "by", "an", "a", "is", "are", "was", "were", "be", "been", "have", "has", "had", "do", "does", "did", "will", "would", "could", "should", "may", "might", "must", "can", "this", "that", "these", "those", "i", "you", "he", "she", "it", "we", "they", "me", "him", "her", "us", "them" };
            return commonWords.Contains(word);
        }

        private void ApplySyntaxHighlighting(string language)
        {
            if (string.IsNullOrEmpty(txtResult.Text))
                return;

            try
            {
                txtResult.SuspendLayout(); // Отключаем обновление для скорости

                // Сохраняем текущую позицию курсора
                int start = txtResult.SelectionStart;

                // Сбрасываем все форматирование
                txtResult.SelectAll();
                txtResult.SelectionColor = Color.White;
                txtResult.SelectionFont = txtResult.Font;
                txtResult.DeselectAll();

                // Подсветка для Python
                if (language.ToLower() == "python")
                {
                    HighlightPython();
                }
                // Подсветка для JavaScript
                else if (language.ToLower() == "javascript" || language.ToLower() == "js")
                {
                    HighlightJavaScript();
                }
                // Подсветка для C#
                else if (language.ToLower() == "csharp" || language.ToLower() == "c#")
                {
                    HighlightCSharp();
                }

                // Восстанавливаем позицию
                txtResult.SelectionStart = start;
                txtResult.SelectionLength = 0;

                txtResult.ResumeLayout(); // Включаем обновление
            }
            catch (Exception ex)
            {
                // Если подсветка не сработала, просто оставляем обычный текст
                Console.WriteLine($"Ошибка подсветки: {ex.Message}");
            }
        }

        private void HighlightPython()
        {
            // Ключевые слова Python
            string[] keywords = {
        "def", "class", "if", "else", "elif", "for", "while",
        "return", "import", "from", "as", "try", "except",
        "finally", "with", "in", "is", "not", "and", "or",
        "True", "False", "None", "lambda", "yield", "async", "await"
    };

            HighlightKeywords(keywords, _accentBlue); // Синий
            HighlightStrings(_accentGreen); // Зеленый для строк
            HighlightComments(_textSecondary); // Серый для комментариев
        }

        private void HighlightJavaScript()
        {
            string[] keywords = {
        "function", "var", "let", "const", "if", "else",
        "for", "while", "return", "class", "import", "export",
        "try", "catch", "finally", "async", "await", "new",
        "this", "typeof", "instanceof", "true", "false", "null", "undefined"
    };

            HighlightKeywords(keywords, _accentBlue); // Синий
            HighlightStrings(_accentGreen); // Зеленый для строк
            HighlightComments(_textSecondary); // Серый для комментариев
        }

        private void HighlightCSharp()
        {
            string[] keywords = {
        "public", "private", "protected", "class", "interface",
        "namespace", "using", "void", "int", "string", "bool",
        "var", "if", "else", "for", "foreach", "while",
        "return", "new", "this", "base", "static", "async",
        "await", "try", "catch", "finally", "true", "false", "null"
    };

            HighlightKeywords(keywords, _accentBlue); // Синий
            HighlightStrings(_accentGreen); // Зеленый для строк
            HighlightComments(_textSecondary); // Серый для комментариев
        }

        private void HighlightKeywords(string[] keywords, Color color)
        {
            foreach (string keyword in keywords)
            {
                int startIndex = 0;
                while (startIndex < txtResult.TextLength)
                {
                    int wordStartIndex = txtResult.Find(keyword, startIndex, RichTextBoxFinds.WholeWord);
                    if (wordStartIndex == -1) break;

                    txtResult.SelectionStart = wordStartIndex;
                    txtResult.SelectionLength = keyword.Length;
                    txtResult.SelectionColor = color;
                    txtResult.SelectionFont = new Font(txtResult.Font, FontStyle.Bold);

                    startIndex = wordStartIndex + keyword.Length;
                }
            }
        }

        private void HighlightStrings(Color color)
        {
            // Ищем строки в кавычках
            string text = txtResult.Text;
            bool inString = false;
            char stringChar = '"';

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '"' || text[i] == '\'')
                {
                    if (!inString)
                    {
                        inString = true;
                        stringChar = text[i];
                        int start = i;

                        // Ищем конец строки
                        int end = text.IndexOf(stringChar, i + 1);
                        if (end != -1)
                        {
                            txtResult.Select(start, end - start + 1);
                            txtResult.SelectionColor = color;
                            i = end;
                            inString = false;
                        }
                    }
                    else if (text[i] == stringChar)
                    {
                        inString = false;
                    }
                }
            }
        }

        private void HighlightComments(Color color)
        {
            // Ищем однострочные комментарии
            int startIndex = 0;
            while (startIndex < txtResult.TextLength)
            {
                int commentIndex = txtResult.Find("//", startIndex, RichTextBoxFinds.None);
                if (commentIndex == -1) break;

                // Находим конец строки
                int lineEnd = txtResult.Text.IndexOf('\n', commentIndex);
                if (lineEnd == -1) lineEnd = txtResult.TextLength;

                txtResult.Select(commentIndex, lineEnd - commentIndex);
                txtResult.SelectionColor = color;

                startIndex = lineEnd;
            }

            // Ищем многострочные комментарии для C# /* */
            startIndex = 0;
            while (startIndex < txtResult.TextLength)
            {
                int commentStart = txtResult.Text.IndexOf("/*", startIndex);
                if (commentStart == -1) break;

                int commentEnd = txtResult.Text.IndexOf("*/", commentStart + 2);
                if (commentEnd == -1) break;

                txtResult.Select(commentStart, commentEnd - commentStart + 2);
                txtResult.SelectionColor = color;

                startIndex = commentEnd + 2;
            }
        }

        private void btnAutoDetect_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSourceCode.Text))
            {
                DarkMessageBox.Show("Введите код для автоопределения", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var detectedLang = CodeDetector.DetectLanguage(txtSourceCode.Text);
            if (detectedLang != null)
            {
                // Находим этот язык в комбобоксе
                var languages = Language.GetAvailableLanguages().ToList();
                var foundLang = languages.FirstOrDefault(l => l.Id == detectedLang.Id);

                if (foundLang != null)
                {
                    // ↓↓↓ ИЗМЕНИ ЭТИ СТРОЧКИ ↓↓↓
                    // Сначала снимаем выделение
                    comboFromLanguage.SelectedIndex = -1;

                    // Устанавливаем значение через SelectedValue
                    comboFromLanguage.SelectedValue = foundLang.Id;

                    // ИЛИ альтернативно принудительно вызываем событие
                    comboFromLanguage.SelectedItem = foundLang;
                    comboFromLanguage.Refresh(); // Обновляем отображение

                    // Обновляем Label
                    lblFromLang.Text = $"Язык:";
                    // ↑↑↑ ИЗМЕНИ ЭТИ СТРОЧКИ ↑↑↑

                    // Показываем уведомление
                    lblStatus.Text = $"🔍 Определен язык: {detectedLang.Name} (сохранено в историю)";

                    // Сохраняем анализ в историю
                    _historyService.AddAnalysisToHistory(txtSourceCode.Text, detectedLang.Name);

                    // Автоматически меняем целевой язык на другой
                    var otherLang = languages.FirstOrDefault(l => l.Id != detectedLang.Id);
                    if (otherLang != null)
                    {
                        comboToLanguage.SelectedValue = otherLang.Id;
                        lblToLang.Text = $"Конверт в:";
                    }
                }
            }
            else
            {
                lblStatus.Text = "❌ Язык не определен";
            }
        }

        private async Task AnalyzeCodeAsync()
        {
            if (string.IsNullOrWhiteSpace(txtSourceCode.Text))
            {
                DarkMessageBox.Show("Введите код для анализа", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!IsCode(txtSourceCode.Text))
            {
                DarkMessageBox.Show("Код не обнаружен. Пожалуйста, введите программный код.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var fromLang = comboFromLanguage.SelectedItem as Language;
            var language = fromLang?.Name ?? string.Empty;

            ShowLoading(true);
            lblStatus.Text = "Анализ кода...";

            try
            {
                var result = await _pythonService.AnalyzeCodeAsync(txtSourceCode.Text, language);
                if (result.Success)
                {
                    txtResult.Clear();
                    txtResult.Font = new Font("Segoe UI", 10);
                    txtResult.WordWrap = true;
                    txtResult.BackColor = Color.FromArgb(26, 26, 46);
                    txtResult.ForeColor = _textColor;
                    txtResult.Text = result.Explanation;
                    UpdateResultStats();
                    lblStatus.Text = "✅ Анализ завершён";
                }
                else
                {
                    txtResult.Text = $"❌ Ошибка анализа:\n{result.ErrorMessage}";
                    lblStatus.Text = "❌ Произошла ошибка";
                }
            }
            catch (Exception ex)
            {
                txtResult.Text = $"💥 Критическая ошибка:\n{ex.Message}";
                lblStatus.Text = "💥 Ошибка соединения";
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtResult.Text))
            {
                try
                {
                    Clipboard.SetText(txtResult.Text);

                    // Анимация кнопки
                    var originalText = btnCopy.Text;
                    var originalColor = btnCopy.ForeColor;

                    btnCopy.Text = "✅ Скопировано!";
                    btnCopy.ForeColor = _accentGreen;

                    // Таймер для возврата исходного текста
                    var timer = new Timer();
                    timer.Interval = 1500;
                    timer.Tick += (s, args) =>
                    {
                        btnCopy.Text = originalText;
                        btnCopy.ForeColor = originalColor;
                        timer.Stop();
                        timer.Dispose();
                    };
                    timer.Start();

                    lblStatus.Text = "📋 Результат скопирован в буфер";
                }
                catch (Exception ex)
                {
                    lblStatus.Text = $"❌ Ошибка копирования: {ex.Message}";
                }
            }
            else
            {
                DarkMessageBox.Show("Нет результата для копирования", "Внимание",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            try
            {
                var historyForm = new HistoryForm(this);
                historyForm.ShowDialog();
            }
            catch (Exception ex)
            {
                DarkMessageBox.Show($"Ошибка открытия истории: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtSourceCode_TextChanged(object sender, EventArgs e)
        {
            UpdateStats();
            ScheduleSourceSyntaxHighlight();
        }

        private void UpdateStats()
        {
            try
            {
                var text = txtSourceCode.Text;
                var lines = string.IsNullOrWhiteSpace(text)
                    ? 0
                    : text.Split('\n')
                        .Select(l => l.TrimEnd('\r'))
                        .Count(l => !string.IsNullOrWhiteSpace(l));
                var chars = text.Count(ch => !char.IsWhiteSpace(ch));
                var words = text.Split(new[] { ' ', '\n', '\r', '\t' },
                    StringSplitOptions.RemoveEmptyEntries).Length;

                lblStats.Text = $"📊 Строк: {lines} | Символов: {chars} | Слов: {words}";
            }
            catch
            {
                lblStats.Text = "📊 Строк: 0 | Символов: 0 | Слов: 0";
            }
        }

        private void ShowLoading(bool show)
        {
            progressBar.Visible = show;
            lblConverting.Visible = show;
            txtResult.Visible = !show;
            btnConvert.Enabled = !show;
            btnAutoDetect.Enabled = !show;
            btnCopy.Enabled = !show;
            btnHistory.Enabled = !show;
            comboFromLanguage.Enabled = !show;
            comboToLanguage.Enabled = !show;

            if (show)
            {
                progressBar.Style = ProgressBarStyle.Marquee;

                if (_convertMode == ConvertActionMode.Analyze)
                {
                    lblConverting.Text = $"Анализ {((Language)comboFromLanguage.SelectedItem)?.Name}...";
                }
                else
                {
                    lblConverting.Text = $"Конвертация {((Language)comboFromLanguage.SelectedItem)?.Name} → {((Language)comboToLanguage.SelectedItem)?.Name}...";
                }

                try
                {
                    int cx = panelRight.ClientSize.Width / 2;
                    int cy = panelRight.ClientSize.Height / 2;

                    progressBar.Left = cx - (progressBar.Width / 2);
                    progressBar.Top = cy + 16;
                    progressBar.BringToFront();

                    lblConverting.Left = cx - (lblConverting.Width / 2);
                    lblConverting.Top = cy - 16;
                    lblConverting.BringToFront();
                }
                catch
                {
                }
            }
            else
            {
                progressBar.Style = ProgressBarStyle.Continuous;
            }
        }

        private void UpdateButtonColors()
        {
            if (comboFromLanguage.SelectedItem != null && comboToLanguage.SelectedItem != null)
            {
                var fromLang = (Language)comboFromLanguage.SelectedItem;
                var toLang = (Language)comboToLanguage.SelectedItem;

                var fromId = (fromLang.Id ?? string.Empty).ToLower();
                var toId = (toLang.Id ?? string.Empty).ToLower();

                string fromLabel = GetShortLanguageLabel(fromId, fromLang.Name);
                string toLabel = GetShortLanguageLabel(toId, toLang.Name);

                if (_convertMode == ConvertActionMode.Analyze)
                {
                    btnConvert.Text = $"🧠 Анализ ({fromLabel})";
                }
                else
                {
                    btnConvert.Text = $"🚀 {fromLabel}→{toLabel}";
                }

                Color target;
                Color textColor = Color.White;

                switch (fromId)
                {
                    case "python":
                        target = Color.FromArgb(0, 180, 255);
                        break;
                    case "csharp":
                    case "c#":
                        target = _accentPurple;
                        break;
                    case "java":
                        target = Color.FromArgb(255, 140, 0);
                        break;
                    case "cpp":
                    case "c++":
                        target = _accentBlue;
                        break;
                    case "javascript":
                    case "js":
                        target = Color.FromArgb(255, 193, 7);
                        textColor = Color.Black;
                        break;
                    case "typescript":
                    case "ts":
                        target = Color.FromArgb(0, 180, 255);
                        break;
                    case "go":
                        target = Color.FromArgb(0, 204, 255);
                        break;
                    case "rust":
                        target = Color.FromArgb(15, 15, 15);
                        break;
                    default:
                        target = _accentPurple;
                        break;
                }

                btnConvert.BackColor = target;
                btnConvert.ForeColor = textColor;
                _convertColorCurrent = target;
                _convertColorTarget = target;
                btnConvert.Invalidate();
            }
        }

        private static string GetShortLanguageLabel(string id, string fallbackName)
        {
            switch ((id ?? string.Empty).ToLower())
            {
                case "python":
                    return "Py";
                case "javascript":
                case "js":
                    return "JS";
                case "typescript":
                case "ts":
                    return "TS";
                case "csharp":
                case "c#":
                    return "C#";
                case "java":
                    return "Java";
                case "cpp":
                case "c++":
                    return "C++";
                case "go":
                    return "Go";
                case "rust":
                    return "Rust";
                case "php":
                    return "PHP";
                default:
                    return string.IsNullOrWhiteSpace(fallbackName) ? id : fallbackName;
            }
        }

        // Метод для загрузки из истории (вызывается из HistoryForm)
        public void LoadFromHistory(ConversionHistory historyItem)
        {
            try
            {
                txtSourceCode.Text = historyItem.SourceCode;

                if (historyItem.IsAnalysis)
                {
                    txtResult.Text = "";
                    lblStatus.Text = $"🔍 Загружен анализ: {historyItem.FromLanguage} - {historyItem.Timestamp:HH:mm}";

                    // Устанавливаем только исходный язык
                    var languages = Language.GetAvailableLanguages().ToList();
                    var fromLang = languages.FirstOrDefault(l => l.Name == historyItem.FromLanguage);
                    if (fromLang != null)
                        comboFromLanguage.SelectedItem = fromLang;
                }
                else
                {
                    txtResult.Text = historyItem.ConvertedCode;
                    lblStatus.Text = $"📝 Загружено из истории: {historyItem.Timestamp:HH:mm}";

                    // Находим языки в комбобоксах
                    var languages = Language.GetAvailableLanguages().ToList();
                    var fromLang = languages.FirstOrDefault(l => l.Name == historyItem.FromLanguage);
                    var toLang = languages.FirstOrDefault(l => l.Name == historyItem.ToLanguage);

                    if (fromLang != null)
                        comboFromLanguage.SelectedItem = fromLang;

                    if (toLang != null)
                        comboToLanguage.SelectedItem = toLang;
                }

                UpdateStats();
            }
            catch (Exception ex)
            {
                DarkMessageBox.Show($"Ошибка загрузки из истории: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Обработчик закрытия формы
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                lblStatus.Text = "Завершение работы...";
            }
            catch
            {
                // Игнорируем ошибки при закрытии
            }
        }

        // Обработчик изменения размера формы
        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
                return;

            try
            {
                BeginInvoke((Action)UpdateLayout);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Resize error: {ex.Message}");
            }
        }

        private void импортToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Code files|*.txt;*.py;*.js;*.ts;*.cs;*.java;*.cpp;*.c;*.h|All files|*.*";
                    ofd.Title = "Импорт файла";
                    if (ofd.ShowDialog(this) != DialogResult.OK)
                        return;

                    txtSourceCode.Text = File.ReadAllText(ofd.FileName);
                    lblStatus.Text = "✅ Файл импортирован";
                }
            }
            catch (Exception ex)
            {
                DarkMessageBox.Show($"Ошибка импорта: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void экспортToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtResult.Text))
                {
                    DarkMessageBox.Show("Нет результата для экспорта", "Внимание",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var sfd = new SaveFileDialog())
                {
                    sfd.Filter = "Text file|*.txt|All files|*.*";
                    sfd.Title = "Экспорт результата";
                    sfd.FileName = "converted.txt";
                    if (sfd.ShowDialog(this) != DialogResult.OK)
                        return;

                    File.WriteAllText(sfd.FileName, txtResult.Text);
                    lblStatus.Text = "✅ Результат экспортирован";
                }
            }
            catch (Exception ex)
            {
                DarkMessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void открытьИсториюToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnHistory_Click(sender, e);
        }

        private void очиститьИсториюToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var res = DarkMessageBox.Show("Очистить историю конвертаций?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (res != DialogResult.Yes)
                    return;

                _historyService?.ClearHistory();
                lblStatus.Text = "✅ История очищена";
            }
            catch (Exception ex)
            {
                DarkMessageBox.Show($"Ошибка очистки истории: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string aboutText = @"Конвертер кода между языками программирования
Использует искусственный интеллект для точной конвертации

📊 Техническая информация:
• Версия: 1.0.0
• API: Groq Llama 3.1 8B Instant
• Платформа: .NET 8.0 Windows

🌐 Поддерживаемые языки:
Python • C# • Java • JavaScript
TypeScript • C++ • Go • Rust • PHP

🛠️ Разработчик: Daniel Aibek uulu
📧 Контакты: diosdefin@gmail.com

© 2025 SCode AI";

            DarkMessageBox.Show(
                aboutText,
                "О программе",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void ScheduleSourceSyntaxHighlight(bool immediate = false)
        {
            if (_isApplyingSourceHighlight) return;
            if (_sourceSyntaxTimer == null) return;

            _sourceSyntaxTimer.Stop();
            if (immediate)
            {
                ApplySourceSyntaxHighlight();
                return;
            }

            _sourceSyntaxTimer.Start();
        }

        private void ApplySourceSyntaxHighlight()
        {
            if (_isApplyingSourceHighlight) return;
            if (txtSourceCode == null) return;
            if (string.IsNullOrEmpty(txtSourceCode.Text)) return;

            try
            {
                _isApplyingSourceHighlight = true;

                var lang = comboFromLanguage.SelectedItem as Language;
                string language = lang?.Name ?? string.Empty;

                int start = txtSourceCode.SelectionStart;
                int length = txtSourceCode.SelectionLength;

                txtSourceCode.SuspendLayout();

                txtSourceCode.SelectAll();
                txtSourceCode.SelectionColor = _textColor;
                txtSourceCode.SelectionFont = txtSourceCode.Font;

                ApplySyntaxHighlightingTo(txtSourceCode, language);

                txtSourceCode.SelectionStart = start;
                txtSourceCode.SelectionLength = length;

                txtSourceCode.ResumeLayout();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                _isApplyingSourceHighlight = false;
            }
        }

        private void ApplySyntaxHighlightingTo(RichTextBox box, string language)
        {
            if (box == null || string.IsNullOrEmpty(box.Text)) return;

            string lang = (language ?? string.Empty).ToLower();

            if (lang == "python")
            {
                string[] keywords = {
                    "def", "class", "if", "else", "elif", "for", "while",
                    "return", "import", "from", "as", "try", "except",
                    "finally", "with", "in", "is", "not", "and", "or",
                    "True", "False", "None", "lambda", "yield", "async", "await", "print"
                };
                string[] types = { "int", "str", "float", "bool", "list", "dict", "set", "tuple" };
                HighlightKeywordsIn(box, keywords, _accentBlue);
                HighlightKeywordsIn(box, types, _accentPurple);
                HighlightStringsIn(box, _accentGreen);
                HighlightCommentsIn(box, _textSecondary, "#");
                return;
            }

            if (lang == "javascript" || lang == "js" || lang == "typescript")
            {
                string[] keywords = {
                    "function", "var", "let", "const", "if", "else",
                    "for", "while", "return", "class", "import", "export",
                    "try", "catch", "finally", "async", "await", "new",
                    "this", "typeof", "instanceof", "true", "false", "null", "undefined"
                };
                string[] types = { "string", "number", "boolean", "any", "void", "unknown", "never" };
                HighlightKeywordsIn(box, keywords, _accentBlue);
                HighlightKeywordsIn(box, types, _accentPurple);
                HighlightStringsIn(box, _accentGreen);
                HighlightCommentsIn(box, _textSecondary, "//");
                return;
            }

            if (lang == "csharp" || lang == "c#")
            {
                string[] keywords = {
                    "public", "private", "protected", "class", "interface",
                    "namespace", "using", "void", "var", "if", "else", "for",
                    "foreach", "while", "return", "new", "this", "base", "static",
                    "async", "await", "try", "catch", "finally", "true", "false", "null"
                };
                string[] types = { "int", "string", "bool", "double", "float", "decimal", "long", "short", "byte", "char", "object" };
                HighlightKeywordsIn(box, keywords, _accentBlue);
                HighlightKeywordsIn(box, types, _accentPurple);
                HighlightStringsIn(box, _accentGreen);
                HighlightCommentsIn(box, _textSecondary, "//");
                return;
            }

            string[] fallback = { "return", "if", "else", "for", "while", "try", "catch", "class", "new" };
            HighlightKeywordsIn(box, fallback, _accentBlue);
            HighlightStringsIn(box, _accentGreen);
            HighlightCommentsIn(box, _textSecondary, "//");
        }

        private void HighlightKeywordsIn(RichTextBox box, string[] keywords, Color color)
        {
            foreach (string keyword in keywords)
            {
                int startIndex = 0;
                while (startIndex < box.TextLength)
                {
                    int wordStartIndex = box.Find(keyword, startIndex, RichTextBoxFinds.WholeWord);
                    if (wordStartIndex == -1) break;

                    box.SelectionStart = wordStartIndex;
                    box.SelectionLength = keyword.Length;
                    box.SelectionColor = color;
                    box.SelectionFont = new Font(box.Font, FontStyle.Bold);

                    startIndex = wordStartIndex + keyword.Length;
                }
            }
        }

        private void HighlightStringsIn(RichTextBox box, Color color)
        {
            string text = box.Text;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '"' || text[i] == '\'')
                {
                    char q = text[i];
                    int start = i;
                    int end = text.IndexOf(q, i + 1);
                    if (end != -1)
                    {
                        box.Select(start, end - start + 1);
                        box.SelectionColor = color;
                        i = end;
                    }
                }
            }
        }

        private void HighlightCommentsIn(RichTextBox box, Color color, string marker)
        {
            int startIndex = 0;
            while (startIndex < box.TextLength)
            {
                int commentIndex = box.Find(marker, startIndex, RichTextBoxFinds.None);
                if (commentIndex == -1) break;

                int lineEnd = box.Text.IndexOf('\n', commentIndex);
                if (lineEnd == -1) lineEnd = box.TextLength;

                box.Select(commentIndex, lineEnd - commentIndex);
                box.SelectionColor = color;
                startIndex = lineEnd;
            }
        }

        // Горячие клавиши
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.F5:
                    btnConvert_Click(null, null);
                    return true;

                case Keys.Control | Keys.C:
                    btnCopy_Click(null, null);
                    return true;

                case Keys.Control | Keys.H:
                    btnHistory_Click(null, null);
                    return true;

                case Keys.Control | Keys.D:
                    btnAutoDetect_Click(null, null);
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }
}
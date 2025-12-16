using System;
using System.Drawing;
using System.Windows.Forms;

namespace Konvertor.Forms
{
    internal static class DarkMessageBox
    {
        private static readonly Color Back = Color.FromArgb(18, 22, 33);
        private static readonly Color Panel = Color.FromArgb(25, 28, 40);
        private static readonly Color TextColor = Color.FromArgb(240, 245, 255);
        private static readonly Color Accent = Color.FromArgb(0, 212, 255);
        private static readonly Color Border = Color.FromArgb(60, 70, 95);

        public static DialogResult Show(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            using (var dlg = new DarkDialog(message, title, buttons, icon))
            {
                return dlg.ShowDialog();
            }
        }

        public static DialogResult Show(string message, string title)
        {
            return Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.None);
        }

        public static DialogResult Confirm(string message, string title)
        {
            return Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        private sealed class DarkDialog : Form
        {
            private readonly MessageBoxButtons _buttons;

            public DarkDialog(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
            {
                _buttons = buttons;

                Text = title ?? string.Empty;
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                ShowInTaskbar = false;
                BackColor = Back;
                ForeColor = TextColor;
                Font = new Font("Segoe UI", 9F);
                Padding = new Padding(14);
                AutoScaleMode = AutoScaleMode.Font;

                var root = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Panel,
                    Padding = new Padding(14)
                };

                var layout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    ColumnCount = 2,
                    RowCount = 2
                };
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40));
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));

                var pic = new PictureBox
                {
                    SizeMode = PictureBoxSizeMode.CenterImage,
                    Dock = DockStyle.Fill,
                    BackColor = Panel
                };

                var iconBmp = GetIconBitmap(icon);
                if (iconBmp != null)
                {
                    pic.Image = iconBmp;
                }

                var lbl = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = message ?? string.Empty,
                    ForeColor = TextColor,
                    BackColor = Panel,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                var buttonsPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.RightToLeft,
                    WrapContents = false,
                    BackColor = Panel,
                    Padding = new Padding(0, 10, 0, 0)
                };

                AddButtons(buttonsPanel);

                layout.Controls.Add(pic, 0, 0);
                layout.Controls.Add(lbl, 1, 0);
                layout.Controls.Add(new Panel { Dock = DockStyle.Fill, BackColor = Panel }, 0, 1);
                layout.Controls.Add(buttonsPanel, 1, 1);

                root.Controls.Add(layout);
                Controls.Add(root);

                // Adjust size based on message length
                using (var g = CreateGraphics())
                {
                    var size = g.MeasureString(message ?? string.Empty, lbl.Font, 480);
                    int height = (int)size.Height + 100; // Add space for buttons and padding
                    ClientSize = new Size(520, Math.Max(170, height));
                }

                Shown += (_, __) =>
                {
                    MinimumSize = Size;
                    MaximumSize = new Size(Screen.FromControl(this).WorkingArea.Width - 100, Screen.FromControl(this).WorkingArea.Height - 100);
                };
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                using (var pen = new Pen(Border, 1))
                {
                    var r = new Rectangle(0, 0, Width - 1, Height - 1);
                    e.Graphics.DrawRectangle(pen, r);
                }
            }

            private void AddButtons(FlowLayoutPanel panel)
            {
                if (_buttons == MessageBoxButtons.OK)
                {
                    var ok = CreateButton("OK", DialogResult.OK, true);
                    panel.Controls.Add(ok);
                    AcceptButton = ok;
                    CancelButton = ok;
                    return;
                }

                if (_buttons == MessageBoxButtons.YesNo)
                {
                    var no = CreateButton("Нет", DialogResult.No, false);
                    var yes = CreateButton("Да", DialogResult.Yes, true);
                    panel.Controls.Add(no);
                    panel.Controls.Add(yes);
                    AcceptButton = yes;
                    CancelButton = no;
                    return;
                }

                if (_buttons == MessageBoxButtons.OKCancel)
                {
                    var cancel = CreateButton("Отмена", DialogResult.Cancel, false);
                    var ok = CreateButton("OK", DialogResult.OK, true);
                    panel.Controls.Add(cancel);
                    panel.Controls.Add(ok);
                    AcceptButton = ok;
                    CancelButton = cancel;
                    return;
                }

                var fallback = CreateButton("OK", DialogResult.OK, true);
                panel.Controls.Add(fallback);
                AcceptButton = fallback;
                CancelButton = fallback;
            }

            private Button CreateButton(string text, DialogResult result, bool isPrimary)
            {
                var btn = new Button
                {
                    Text = text,
                    DialogResult = result,
                    Width = 110,
                    Height = 34,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = isPrimary ? Accent : Color.Transparent,
                    ForeColor = isPrimary ? Color.Black : TextColor,
                    UseVisualStyleBackColor = false
                };

                btn.FlatAppearance.BorderSize = 1;
                btn.FlatAppearance.BorderColor = isPrimary ? Accent : Border;
                btn.FlatAppearance.MouseOverBackColor = isPrimary ? Color.FromArgb(30, 255, 255, 255) : Color.FromArgb(35, 255, 255, 255);
                btn.FlatAppearance.MouseDownBackColor = isPrimary ? Color.FromArgb(60, 255, 255, 255) : Color.FromArgb(55, 255, 255, 255);

                btn.Click += (_, __) => Close();
                return btn;
            }

            private static Bitmap GetIconBitmap(MessageBoxIcon icon)
            {
                Icon ico = null;
                switch (icon)
                {
                    case MessageBoxIcon.Error:
                        ico = SystemIcons.Error;
                        break;
                    case MessageBoxIcon.Warning:
                        ico = SystemIcons.Warning;
                        break;
                    case MessageBoxIcon.Information:
                        ico = SystemIcons.Information;
                        break;
                    case MessageBoxIcon.Question:
                        ico = SystemIcons.Question;
                        break;
                }

                if (ico == null) return null;
                return ico.ToBitmap();
            }
        }
    }
}

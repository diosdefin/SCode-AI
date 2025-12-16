using System;
using System.Drawing;
using System.Windows.Forms;
using Konvertor.Models;

namespace Konvertor.Controls
{
    public class IconComboBox : ComboBox
    {
        public IconComboBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            ItemHeight = 31;
            DropDownStyle = ComboBoxStyle.DropDownList;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            base.OnDrawItem(e);

            if (e.Index < 0 || e.Index >= Items.Count)
                return;

            e.DrawBackground();

            if (Items[e.Index] is Language language)
            {
                if (language.Icon != null)
                {
                    Rectangle iconRect = new Rectangle(e.Bounds.X + 2, e.Bounds.Y + 2, 24, 24);
                    e.Graphics.DrawImage(language.Icon, iconRect);
                }

                // Рисуем текст с вертикальным центрированием
                Rectangle textRect = new Rectangle(e.Bounds.X + 30, e.Bounds.Y, e.Bounds.Width - 30, e.Bounds.Height);
                using (Brush brush = new SolidBrush(e.ForeColor))
                using (StringFormat sf = new StringFormat())
                {
                    sf.LineAlignment = StringAlignment.Center;
                    sf.Alignment = StringAlignment.Near;
                    e.Graphics.DrawString(language.Name, e.Font, brush, textRect, sf);
                }
            }

            e.DrawFocusRectangle();
        }
    }
}
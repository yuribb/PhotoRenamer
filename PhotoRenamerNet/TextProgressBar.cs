using System;
using System.Drawing;
using System.Windows.Forms;

namespace PhotoRenamerNet
{
    internal class TextProgressBar : ProgressBar
    {
        public TextProgressBar()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var rect = ClientRectangle;
            var g = e.Graphics;

            ProgressBarRenderer.DrawHorizontalBar(g, rect);
            rect.Inflate(-3, -3);
            if (Value > 0)
            {
                var clip = new Rectangle(rect.X, rect.Y, (int)Math.Round((float)Value / Maximum * rect.Width), rect.Height);
                ProgressBarRenderer.DrawHorizontalChunks(g, clip);
            }

            using (var f = new Font(FontFamily.GenericSerif, 9))
            {
                var len = g.MeasureString(Text, f);
                var location = new Point(Convert.ToInt32(Width / 2 - len.Width / 2), Convert.ToInt32(Height / 2 - len.Height / 2));
                g.DrawString(Text, f, Brushes.Black, location);
            }
        }
    }
}
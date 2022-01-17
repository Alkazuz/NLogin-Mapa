using AdvancedBot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Request = NLogin_Mapa.MapaBypass.CaptchaSolveRequest;
namespace NLogin_Mapa
{
    public partial class FrmMapa : Form
    {

        private Request Current = null;
        private object drawSync = new object();
        public FrmMapa()
        {
            InitializeComponent();
            Icon = Program.FrmMain.Icon;
            timer1.Enabled = true;
            DoubleBuffered = true;
        }

        private void FrmMapa_Load(object sender, EventArgs e)
        {

        }

        private void FrmMapa_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MapaBypass.isFormInitializing = false;
        }

        private void TryGetNextCaptcha()
        {
            listBox1.Items.Clear();
            Queue<Request> queue = MapaBypass.Captchas;
            lock (queue)
            {
                lock (drawSync)
                {
                    if (Current != null)
                    {
                        Current.Image.Dispose();
                        Current = null;
                        Invalidate();
                    }
                    if (queue.Count > 0)
                    {
                        Current = queue.Dequeue();
                        Invalidate();
                        foreach (JsonMessage jsons in Main.jsons[Current.Client])
                        {
                            listBox1.Items.Add(jsons.message);
                        }
                        listBox1.SelectedIndex = 0;
                    }
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            lock (drawSync)
            {
                if (Current != null)
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(Current.Image, new Rectangle(1, 1, 256, 156));
                }
            }
            g.DrawRectangle(Pens.Gray, new Rectangle(1, 1, 256, 156));
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (Current == null || !Current.Client.IsBeingTicked())
            {
                TryGetNextCaptcha();
            }
            string user = Current == null ? "" : ("Atual: " + Current.Client.Username + " ");
            lbInfo.Text = $"{user}Restante: {MapaBypass.Captchas.Count} ";

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Current.Client.SendMessage(byMessage(listBox1.SelectedItem.ToString()).command);
            Current.Bypasser.finish();
            Current = null;
        }
        public JsonMessage byMessage(String msg)
        {
            foreach (JsonMessage jsons in Main.jsons[Current.Client])
            {
                if (jsons.message.Equals(msg))
                {
                    return jsons;
                }
            }
            return null;
        }
    }
}

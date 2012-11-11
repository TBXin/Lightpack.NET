using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;
using LightpackNetApi;
using LightpackNetApi.Answers;

namespace LightpackApiTest
{
    public partial class Main : Form
    {
        private readonly Lightpack lp = new Lightpack("localhost", 3636, new byte[] { 7, 6, 2, 1, 3, 4, 5, 10, 9, 8 }, "test");

        public Main()
        {
            InitializeComponent();
            lp.Connect();
        }

        private void ButtonSnakeClick(object sender, EventArgs e)
        {
            if (lp.Lock() != LockAnswer.Success)
                return;

            lp.SetGamma(0.01f);
            lp.SetSmooth(10);
            lp.SetColorToAll(Color.Black);

            for (byte i = 0; i < 80; i++)
            {
                for (byte k = 0; k < 10; k++)
                {
                    var idx = (byte)((i + k) % 10);

                    lp.SetColor(idx, k > 3 ? Color.Red : Color.FromArgb(0, 0, 125));

                    Thread.Sleep(10);
                }
            }

            lp.Unlock();
        }

        private void ButtonRandomColorClick(object sender, EventArgs e)
        {
            if (lp.Lock() != LockAnswer.Success)
                return;

            lp.SetGamma(0.01f);
            lp.SetSmooth(250);
            lp.SetColorToAll(Color.White);

            var rnd = new Random();
            var ark = new List<byte>();

            for (byte i = 0; i < 101; i++)
            {
                var n = (byte) (i % 2);

                if (n == 0)
                    ark.Add(0);
                else
                    ark.Add((byte)rnd.Next(200, 255));
            }

            Shuffle(ark);

            for (byte i = 0; i < 20; i++)
            {
                var num = (byte)rnd.Next(0, 9);
                var r = ark[rnd.Next(0, ark.Count)];
                var g = ark[rnd.Next(0, ark.Count)];
                var b = ark[rnd.Next(0, ark.Count)];

                if (num != 0 && num != 9)
                {
                    lp.SetColor(num, Color.FromArgb(r,g,b));
                    Thread.Sleep(rnd.Next(0, 1000));
                    lp.SetColor((byte)(num - 1), Color.FromArgb(r, g, b));
                    lp.SetColor((byte)(num + 1), Color.FromArgb(r, g, b));  
                }
                else if (num == 0)
                {
                    lp.SetColor((byte)(num + 1), Color.FromArgb(r, g, b));
                    Thread.Sleep(rnd.Next(0, 1000));
                    lp.SetColor(num, Color.FromArgb(r, g, b));
                    lp.SetColor((byte)(num + 2), Color.FromArgb(r, g, b));  
                }
                else if (num == 9)
                {
                    lp.SetColor((byte)(num - 1), Color.FromArgb(r, g, b));
                    Thread.Sleep(rnd.Next(0, 1000));
                    lp.SetColor(num, Color.FromArgb(r, g, b));
                    lp.SetColor((byte)(num - 2), Color.FromArgb(r, g, b));  
                }
                Thread.Sleep(rnd.Next(1000, 3000));
            }

            lp.Unlock();
        }

        private static void Shuffle<T>(IList<T> list)
        {
            var provider = new RNGCryptoServiceProvider();
            var n = list.Count;

            while (n > 1)
            {
                var box = new byte[1];

                do
                {
                    provider.GetBytes(box);
                } while (!(box[0] < n * (Byte.MaxValue / n)));

                var k = (box[0] % n);
                n--;

                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}

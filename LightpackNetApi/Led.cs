using System.Drawing;

namespace LightpackNetApi
{
    public class Led
    {
        public byte Number { get; set; }
        public Color Color { get; set; }

        public Led()
        {
            Color = Color.Black;
        }

        public Led(byte number, Color color)
        {
            Number = number;
            Color = color;
        }
    }
}

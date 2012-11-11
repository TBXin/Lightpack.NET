using System.Drawing;

namespace LightpackNet
{
    /// <summary>
    /// Агрегация светодиода.
    /// </summary>
    public class Led
    {
        /// <summary>
        /// Номер.
        /// </summary>
        public byte Number { get; set; }

        /// <summary>
        /// Цвет.
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Инициализация объекта светодиода.
        /// </summary>
        /// <param name="number">Номер</param>
        /// <param name="color">Цвет</param>
        public Led(byte number, Color color)
        {
            Number = number;
            Color = color;
        }
    }
}

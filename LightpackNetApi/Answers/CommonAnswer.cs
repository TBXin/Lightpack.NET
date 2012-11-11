using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightpackNetApi.Answers
{
    public enum CommonAnswer
    {
        /// <summary>
        /// Выполнено успешно.
        /// </summary>
        Ok,
        /// <summary>
        /// Неверный API ключ.
        /// </summary>
        Fail,
        /// <summary>
        /// Ошибка при выполнении.
        /// </summary>
        Error,
        /// <summary>
        /// Устройство занято.
        /// </summary>
        Busy,
        /// <summary>
        /// Устройство не в эксклюзивном режиме.
        /// </summary>
        NotLocked
    }
}

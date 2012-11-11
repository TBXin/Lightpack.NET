using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using LightpackNetApi.Answers;
using LightpackNetApi.Consts;

namespace LightpackNetApi
{
    /// <summary>
    /// Lightpack API v1.3
    /// </summary>
    public class Lightpack
    {
        private readonly TelnetClient client;
        private readonly byte[] ledMap;
        private readonly string apiKey;

        public Lightpack(string host, int port, byte[] ledMap, string apiKey = null)
        {
            client = new TelnetClient(host, port);
            this.ledMap = ledMap;
            this.apiKey = apiKey;
        }

        public void Connect()
        {
            client.Connect();

            if (!String.IsNullOrEmpty(apiKey))
                ApiKey(apiKey);
        }

        public void Disconnect()
        {
            client.Disconnect();
        }

        public string Send(string command)
        {
            client.Write(command);
            var result = client.Read();

            if (result.ToLower().StartsWith("authorization required"))
                throw new InvalidOperationException("Authorization required");

            return result;
        }

        /// <summary>
        /// Ввод ключа авторизации (API ключ) для взаимодействия с устройством.
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns></returns>
        public CommonAnswer ApiKey(string key)
        {
            return GetCommonAnswer(Send(String.Format(LightpackCommands.ApiKey, key)));
        }

        /// <summary>
        /// Устанавливает эксклюзивную блокировку устройства. Открывает доступ к set командам, запрещая другим клиентам доступ к ним.
        /// </summary>
        /// <returns></returns>
        public LockAnswer Lock()
        {
            return GetLockAnswer(Send(LightpackCommands.Lock));
        }

        /// <summary>
        /// Снимает эксклюзивную блокировку устройства. Восстанавливает настройки устройства из текущего профиля.
        /// </summary>
        /// <returns></returns>
        public LockAnswer Unlock()
        {
            return GetLockAnswer(Send(LightpackCommands.Unlock));
        }

        /// <summary>
        /// Получает состояние устройства.
        /// </summary>
        /// <returns></returns>
        public StatusAnswer GetStatus()
        {
            return GetStatusAnswer(Send(LightpackCommands.GetStatus));
        }

        /// <summary>
        /// Получает состояние API устройсва.
        /// </summary>
        /// <returns></returns>
        public ApiStatusAnswer GetStatusApi()
        {
            return GetStatusApi(Send(LightpackCommands.GetStatusApi));
        }

        /// <summary>
        /// Получает название текущего профиля.
        /// </summary>
        /// <returns></returns>
        public string GetProfile()
        {
            var result = Send(LightpackCommands.GetProfile).TrimEnd('\r', '\n');

            var parts = result.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
                throw new InvalidOperationException(String.Format("Unknown get profile answer: '{0}'", result));

            return parts[1];
        }

        /// <summary>
        /// Получает названия всех доступных профилей.
        /// </summary>
        /// <returns></returns>
        public List<string> GetProfiles()
        {
            var result = Send(LightpackCommands.GetProfiles).TrimEnd('\r', '\n');

            var parts = result.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
                throw new InvalidOperationException(String.Format("Unknown get profiles answer: '{0}'", result));

            var profiles = parts[1].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            return profiles;
        }

        /// <summary>
        /// Получает кол-во светодиодов на текущем профиле.
        /// </summary>
        /// <returns></returns>
        public byte GetCountLed()
        {
            var result = Send(LightpackCommands.GetCountLeds).TrimEnd('\r', '\n');

            var parts = result.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
                throw new InvalidOperationException(String.Format("Unknown get count led answer: '{0}'", result));

            return Byte.Parse(parts[1]);
        }

        /// <summary>
        /// Устанавливает цвет светодиода. Требует эсклюзивный режим захвата устройства.
        /// </summary>
        /// <param name="number">Номер светодиода</param>
        /// <param name="color">Цвет</param>
        /// <returns></returns>
        public CommonAnswer SetColor(byte number, Color color)
        {
            return GetCommonAnswer(Send(String.Format(LightpackCommands.SetColor, MapLed(number), color.R, color.G, color.B)));
        }

        /// <summary>
        /// Устанавливает цвет светодиода. Требует эсклюзивный режим захвата устройства.
        /// </summary>
        /// <param name="led">Агрегация светодиода</param>
        /// <returns></returns>
        public CommonAnswer SetColor(Led led)
        {
            if (led == null)
                throw new ArgumentNullException("led");

            return GetCommonAnswer(Send(String.Format(LightpackCommands.SetColor, MapLed(led.Number), led.Color.R, led.Color.G, led.Color.B)));
        }

        /// <summary>
        /// Устанавливает цвета для указанных светодиодов. Требует эсклюзивный режим захвата устройства.
        /// </summary>
        /// <param name="leds">Список светодиодов</param>
        /// <returns></returns>
        public CommonAnswer SetColor(List<Led> leds)
        {
            if (leds == null)
                throw new ArgumentNullException("leds");

            if (leds.Count == 0)
                return CommonAnswer.Ok;

            var commandBuilder = new StringBuilder();

            commandBuilder.Append(LightpackCommands.SetColorCommand);
            foreach (var led in leds)
            {
                commandBuilder.AppendFormat(LightpackCommands.SetColorArgs, MapLed(led.Number), led.Color.R, led.Color.G, led.Color.B);
            }

            return GetCommonAnswer(Send(commandBuilder.ToString()));
        }

        /// <summary>
        /// Устанавливает цвета всех светодиодов. Требует эсклюзивный режим захвата устройства.
        /// </summary>
        /// <param name="color">Цвет</param>
        /// <returns></returns>
        public CommonAnswer SetColorToAll(Color color)
        {
            var commandBuilder = new StringBuilder();

            commandBuilder.Append(LightpackCommands.SetColorCommand);
            for (byte i = 0; i < ledMap.Length; i++)
            {
                commandBuilder.AppendFormat(LightpackCommands.SetColorArgs, ledMap[i], color.R, color.G, color.B);
            }

            return GetCommonAnswer(Send(commandBuilder.ToString()));
        }

        /// <summary>
        /// Устанавливает значение гамма-коррекции. Требует эсклюзивный режим захвата устройства.
        /// </summary>
        /// <param name="value">Уровень гамма-коррекции. Допустимые значения [0.01 - 10].</param>
        /// <returns></returns>
        public CommonAnswer SetGamma(float value)
        {
            if (value < 0.01f || value > 10f)
                throw new ArgumentOutOfRangeException("value", "gamma correction value [0.01 - 10]");

            return GetCommonAnswer(Send(String.Format(LightpackCommands.SetGamma, value.ToString(CultureInfo.InvariantCulture))));
        }

        /// <summary>
        /// Устанавливает значение яркости. Требует эсклюзивный режим захвата устройства.
        /// </summary>
        /// <param name="value">Уровень яркости Допустимые значения [0 - 100].</param>
        /// <returns></returns>
        public CommonAnswer SetBrightness(byte value)
        {
            if(value > 100)
                throw new ArgumentOutOfRangeException("value", "brightness value [0 - 100]");

            return GetCommonAnswer(Send(String.Format(LightpackCommands.SetBrightness, value)));
        }

        /// <summary>
        /// Устанавливает плавность смены цвета. Требует эсклюзивный режим захвата устройства.
        /// </summary>
        /// <param name="value">Уровень плавности</param>
        /// <returns></returns>
        public CommonAnswer SetSmooth(byte value)
        {
            return GetCommonAnswer(Send(String.Format(LightpackCommands.SetSmooth, value)));
        }

        /// <summary>
        /// Устанавливает текущий профиль. Требует эсклюзивный режим захвата устройства.
        /// </summary>
        /// <param name="profileName">Название профиля</param>
        /// <returns></returns>
        public CommonAnswer SetProfile(string profileName)
        {
            return GetCommonAnswer(Send(String.Format(LightpackCommands.SetProfile, profileName)));
        }

        /// <summary>
        /// Включает / Выключает подсветку. Требует эсклюзивный режим захвата устройства.
        /// </summary>
        /// <param name="status">Состояние</param>
        /// <returns></returns>
        public CommonAnswer SetStatus(Status status)
        {
            return GetCommonAnswer(Send(String.Format(LightpackCommands.SetStatus, status.ToString().ToLower())));
        }

        private CommonAnswer GetCommonAnswer(string answer)
        {
            answer = answer.TrimEnd('\r', '\n');

            if (answer.Equals(LightpackAnswers.Ok, StringComparison.OrdinalIgnoreCase))
                return CommonAnswer.Ok;

            if (answer.Equals(LightpackAnswers.Fail, StringComparison.OrdinalIgnoreCase))
                return CommonAnswer.Fail;

            if (answer.Equals(LightpackAnswers.Error, StringComparison.OrdinalIgnoreCase))
                return CommonAnswer.Error;

            if (answer.Equals(LightpackAnswers.Busy, StringComparison.OrdinalIgnoreCase))
                return CommonAnswer.Busy;

            if (answer.Equals(LightpackAnswers.NotLocked, StringComparison.OrdinalIgnoreCase))
                return CommonAnswer.NotLocked;

            throw new InvalidOperationException(String.Format("Unknown common result '{0}'", answer));
        }

        private LockAnswer GetLockAnswer(string answer)
        {
            answer = answer.TrimStart("unlock:".ToArray()).TrimEnd('\r', '\n');

            if (answer.Equals(LightpackAnswers.Success, StringComparison.OrdinalIgnoreCase))
                return LockAnswer.Success;

            if (answer.Equals(LightpackAnswers.Busy, StringComparison.OrdinalIgnoreCase))
                return LockAnswer.Busy;

            if (answer.Equals(LightpackAnswers.NotLocked, StringComparison.OrdinalIgnoreCase))
                return LockAnswer.NotLocked;

            throw new InvalidOperationException(String.Format("Unknown lock result '{0}'", answer));
        }

        private StatusAnswer GetStatusAnswer(string answer)
        {
            var parts = answer.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
                throw new InvalidOperationException(String.Format("Unknown status answer: '{0}'", answer));

            answer = parts[1].TrimEnd('\r', '\n');

            if (answer.Equals(LightpackAnswers.On, StringComparison.OrdinalIgnoreCase))
                return StatusAnswer.On;

            if (answer.Equals(LightpackAnswers.Off, StringComparison.OrdinalIgnoreCase))
                return StatusAnswer.Off;

            if (answer.Equals(LightpackAnswers.DeviceError, StringComparison.OrdinalIgnoreCase))
                return StatusAnswer.DeviceError;

            if (answer.Equals(LightpackAnswers.Unknown, StringComparison.OrdinalIgnoreCase))
                return StatusAnswer.Unknown;

            throw new InvalidOperationException(String.Format("Unknown status result '{0}'", answer));
        }

        private ApiStatusAnswer GetStatusApi(string answer)
        {
            var parts = answer.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
                throw new InvalidOperationException(String.Format("Unknown status answer: '{0}'", answer));

            answer = parts[1].TrimEnd('\r', '\n');

            if (answer.Equals(LightpackAnswers.Idle, StringComparison.OrdinalIgnoreCase))
                return ApiStatusAnswer.Idle;

            if (answer.Equals(LightpackAnswers.Busy, StringComparison.OrdinalIgnoreCase))
                return ApiStatusAnswer.Busy;

            throw new InvalidOperationException(String.Format("Unknown status api result '{0}'", answer));
        }

        private byte MapLed(byte number)
        {
            if (number > ledMap.Length - 1)
                throw new InvalidOperationException(String.Format("Can't map led with number '{0}', because only {1} leds in map", number, ledMap));

            return ledMap[number];
        }
    }
}

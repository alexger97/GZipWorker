using System;
using System.Collections.Generic;
using System.Text;

namespace GzipOperator.Model
{
    public delegate void ErrorDelegate(string info, Exception x);

    /// <summary>
    /// Абстрактный класс для сервиса оповещений пользователя
    /// </summary>
    public abstract class Reporter
    {
        /// <summary> Функция формирования и вывода сообщения об ошибке   /// </summary>
        public void ShowExceptionMessage(string information, Exception x)
        {
            string s = "Произошла ошибка. При операции : " + information + $"\n Подробно :\n {x.ToString()}";
            ShowMessage(s);
        }

        /// <summary>Функция непосредственного вывода сообщения пользователю</summary>
        public abstract void ShowMessage(string info);

    }
}

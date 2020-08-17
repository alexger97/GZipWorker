using System;
using System.Collections.Generic;
using System.Text;

namespace GzipOperator.Model
{
    public delegate void ErrorDelegate(string info, Exception x);
    public abstract class Reporter
    {
        public void ShowExceptionMessage(string information, Exception x)
        {
            string s = "Произошла ошибка. При операции : " + information + $"\n Подробно :\n {x.ToString()}";
            ShowMessage(s);
        }

        public abstract void ShowMessage(string info);

    }
}

using GzipOperator.Model;
using GzipOperator.Model.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace GzipConsole.Model
{
    class ConsoleReporter : Reporter
    {
        public override void ShowMessage(string info) => Console.WriteLine(info);

    }
}

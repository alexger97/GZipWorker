using GzipOperator.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GzipOperator.Model.Interface
{
    public delegate void FinalDelegate();

    public delegate void ProgressDelegate(double i);
    public interface IService
    {
        void ReadFile(FileInfo inputfile);

        void WriteFile(FileInfo outputfile);

        void Processing();

        event FinalDelegate StopInput;

        event FinalDelegate StopOutput;


        event ProgressDelegate InputProgress;

        event ProgressDelegate OutputProgress;

        event ProgressDelegate ProcessingProgress;

        event ErrorDelegate Error;

    }
}

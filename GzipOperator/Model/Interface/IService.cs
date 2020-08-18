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
        /// <summary> Чтение из файла </summary>
        void ReadFile(FileInfo inputfile);


        /// <summary> Запись в файл </summary>
        void WriteFile(FileInfo outputfile);


        /// <summary> Функция преобразования данных   </summary>
        void Processing();


        /// <summary> Событие остановки чтения файла </summary>
        event FinalDelegate StopInput;

        /// <summary> Событие остановки записи в файл </summary>
        event FinalDelegate StopOutput;

        /// <summary> Событие оповещения о прогрессе чтения </summary>
        event ProgressDelegate InputProgress;

        /// <summary> Событие оповещения о прогрессе записи </summary>
        event ProgressDelegate OutputProgress;

        /// <summary> Событие оповещения о прогрессе обработки данных </summary>
        event ProgressDelegate ProcessingProgress;

        /// <summary> Событие оповещения о произошедшей ошибке </summary>
        event ErrorDelegate Error;

    }
}

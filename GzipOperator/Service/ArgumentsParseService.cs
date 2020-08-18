using GzipOperator.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GzipOperator.Service
{
    /// <summary>Сервис по обработке поступившей на вход информации </summary>
    struct ArgumentsParseService
    {
        static public TaskInfo TryGetTaskInfo(string[] info)
        {
            TypeOfOperation typeOfOperation;
            if (info.Length != 3)
                throw new ApplicationException("Ошибка. Неверное количество параметров передано программе");

            switch (info[0])
            {
                case "compress":
                    typeOfOperation = TypeOfOperation.Compress;
                    break;
                case "decompress":
                    typeOfOperation = TypeOfOperation.Decompress;
                    break;
                default:
                    throw new ApplicationException("Ошибка. Неверное указание параметров выполенения программы");
            }

            if (!File.Exists(info[1]))
                throw new FileNotFoundException("Ошибка. Указанного  пути к файлу. Файл не существует");

            if (!Directory.Exists(Path.GetDirectoryName(info[2])))
                throw new DirectoryNotFoundException("Ошибка. Путь к целевому каталогу указан не верно или каталог не существует");

            return new TaskInfo(new FileInfo(info[1]), new FileInfo(info[2]), typeOfOperation);
        }
    }
}


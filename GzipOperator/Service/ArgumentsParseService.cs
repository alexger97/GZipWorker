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
        public static event ErrorDelegate Error;
        static public void TryGetTaskInfo(string[] info, ref TaskInfo taskInfo)
        {

            try
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

                taskInfo.FileInputInfo = new FileInfo(info[1]);
                taskInfo.FileOutputInfo = new FileInfo(info[2]);
                taskInfo.TypeOfOperation = typeOfOperation;
            }
            catch (Exception x)
            {
                Error?.Invoke("Обработке входящих параметров", x);
                return;
            }

        }
    }
}


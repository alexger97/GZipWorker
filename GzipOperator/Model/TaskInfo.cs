using GzipOperator.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GzipOperator.Model
{

    /// <summary> Структура, описывающая поступившую задачу   </summary>

    public struct TaskInfo
    {
        /// <summary> Входящий файл  </summary>  
        public FileInfo FileInputInfo { get; set; }
        /// <summary> Файл, содержащий обработаннаю последовательность байт  </summary>  
        public FileInfo FileOutputInfo { get; set; }
        /// <summary> Тип операции  </summary>  
        public TypeOfOperation TypeOfOperation { get; set; }
        /// <summary> Количество блоков  </summary>  
        public static ushort START_BLOCK_COUNT { get; set; }

        public TaskInfo(FileInfo file_info1, FileInfo fileInfo2, TypeOfOperation typeOfOperation)
        {
            FileInputInfo = file_info1;
            FileOutputInfo = fileInfo2;
            TypeOfOperation = typeOfOperation;
            GetStartBlockCount(file_info1);
        }

        //Расчет количества блоков 
        void GetStartBlockCount(FileInfo fileInfo)
        {
            START_BLOCK_COUNT = (ushort)(fileInfo.Length / GZIPOperation.SIZE_BUFFER);

            if (fileInfo.Length % GZIPOperation.SIZE_BUFFER > 0)
                START_BLOCK_COUNT++;
        }
    }
}

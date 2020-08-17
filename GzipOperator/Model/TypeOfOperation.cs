using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GzipOperator.Model
{
    /// <summary>
    /// Тип операции
    /// </summary>
    public enum TypeOfOperation
    {
        /// <summary> Сжатие данных   </summary>
        Compress,
        /// <summary> Разжатие данных   </summary>
        Decompress
    }
}

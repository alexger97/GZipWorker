using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GzipOperator.Model
{
    /// <summary> Тип очереди </summary>
    public enum TypeOfQueue
    {
        /// <summary> Для первоначально считанных данных </summary>
        Input,
        /// <summary> Для обработанных данных </summary>
        Output
    }
}

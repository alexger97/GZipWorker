using GzipOperator.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace GzipOperator.Service
{
    public class SynchronizationContextService
    {
        /// <summary>
        ///Очереди для блоков : необработанных и обработанных
        /// </summary>
        private Queue<KeyValuePair<int, byte[]>> InputQueue { get; set; }
        private Queue<KeyValuePair<int, byte[]>> OutputQueue { get; set; }

        // Флаги для отображения полного прочтения и полной обработки блоков
        private volatile bool InputFinal = false;
        private volatile bool OutputFinal = false;

        public SynchronizationContextService(TaskInfo taskinfo)
        {
            InputQueue = new Queue<KeyValuePair<int, byte[]>>();

            OutputQueue = new Queue<KeyValuePair<int, byte[]>>();
        }


        /// <summary>
        /// Функция помещения блока байт в очередь
        /// </summary>
        /// <param name="block">Блок байт</param>
        /// <param name="block_number"> Номер блока </param>
        /// <param name="typeOfQueue"> Тип очереди для вставки блока</param>
        /// <returns></returns>
        public bool SetBlock(byte[] block, int block_number, TypeOfQueue typeOfQueue)
        {
            Queue<KeyValuePair<int, byte[]>> DestanationQueue = (typeOfQueue == TypeOfQueue.Input) ? InputQueue : OutputQueue;
            try
            {
                lock (DestanationQueue)
                {
                    DestanationQueue.Enqueue(new KeyValuePair<int, byte[]>(block_number, block));
                }
            }
            catch (Exception x)
            {
                Console.WriteLine("Ошибка при установке блока в очередь. Подробно: \n" + x.ToString());
                return false;
            }
            return true;
        }

        /// <summary>
        /// Функция получкния блока байтов из очереди
        /// </summary>
        /// <param name="block">Блок байт</param>
        /// <param name="block_number">Номер блока </param>
        /// <param name="typeOfQueue">Тип очереди извлечения блока</param>
        /// <returns></returns>
        public bool GetBlock(out byte[] block, out int block_number, TypeOfQueue typeOfQueue)
        {
            Queue<KeyValuePair<int, byte[]>> SourceQueue = (typeOfQueue == TypeOfQueue.Output) ? OutputQueue : InputQueue;

            KeyValuePair<int, byte[]> queue_element;
            block = null;
            block_number = 0;
            //while (SourceQueue.Count == 0)
            //    if (!Check())
            //        break;

            try
            {
                Monitor.Enter(SourceQueue);

                while (SourceQueue.Count == 0)
                    if (!Check())
                        return false;
                    else
                        Monitor.Wait(SourceQueue);

                if (SourceQueue.Count > 0)
                {
                    queue_element = SourceQueue.Dequeue();
                    block = queue_element.Value;
                    block_number = queue_element.Key;
                    Monitor.Pulse(SourceQueue);
                    return true;
                }
                else
                    return false;

            }
            catch (Exception x)
            {
                Console.WriteLine("Ошибка при получении блока из очереди. Подробно: \n" + x.ToString());
                return false;
            }
            finally
            {
                Monitor.Exit(SourceQueue);
            }

            bool Check()
            {
                if (typeOfQueue == TypeOfQueue.Input && InputFinal == true)
                    return false;

                if (typeOfQueue == TypeOfQueue.Output && OutputFinal == true)
                    return false;

                return true;
            }

        }

        #region Final_Signal
        //Функции оповещения о полноте прочтения/записи
        public void FinalInput()
        {
            lock (InputQueue)
            {
                InputFinal = true;
                Monitor.PulseAll(InputQueue);
            }
        }

        public void FinalOutput()
        {
            lock (OutputQueue)
            {
                OutputFinal = true;
                Monitor.PulseAll(OutputQueue);
            }

        }
        #endregion
    }
}

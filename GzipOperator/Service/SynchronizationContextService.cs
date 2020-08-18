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

        public event ErrorDelegate Error;

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
                    if (!Check())
                        return false;
                    while (DestanationQueue.Count >= Starter.Threads.Length)
                        Monitor.Wait(DestanationQueue);

                    if (!Check())
                        return false;

                    DestanationQueue.Enqueue(new KeyValuePair<int, byte[]>(block_number, block));

                    Monitor.Pulse(DestanationQueue);

                    return true;
                }
            }
            catch (Exception x)
            {
                Error("Установка блока в очередь", x);
                return false;
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
                Error("Получение блока из очереди", x);
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

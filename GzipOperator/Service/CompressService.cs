using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Compression;
using System.IO;
using GzipOperator.Model;
using GzipOperator.Model.Interface;
using System.Threading;

namespace GzipOperator.Service
{
    /// <summary>
    /// Класс реализущий функционал для сжатия данных
    /// </summary>
    class CompressService : IService
    {
        private int process_counter = 0;

        private SynchronizationContextService SyncService;

        public event FinalDelegate StopInput;
        public event FinalDelegate StopOutput;

        public event ProgressDelegate InputProgress;
        public event ProgressDelegate OutputProgress;
        public event ProgressDelegate ProcessingProgress;

        public event ErrorDelegate Error;

        public CompressService(ref SynchronizationContextService synchronizationContextService) => SyncService = synchronizationContextService;

        /// <summary>
        /// Функция для чтения сжатого файла
        /// </summary>
        /// <param name="inputfile">Исходный файл</param>
        public void ReadFile(FileInfo inputfile/*, SynchronizationContextService synchronizationContext*/)
        {
            using (BinaryReader br = new BinaryReader(inputfile.OpenRead()))
            {
                try
                {
                    for (int i = 0; i < TaskInfo.START_BLOCK_COUNT; i++)
                    {
                        byte[] block = br.ReadBytes(GZIPOperation.SIZE_BUFFER);

                        if (SyncService.SetBlock(block, i, TypeOfQueue.Input))
                        {
                            
                            double c = (double) (i / TaskInfo.START_BLOCK_COUNT);
                            InputProgress?.Invoke(c);
                        }
                            
                    }
                    StopInput();
                }
                catch (Exception x)
                {
                    Error?.Invoke("Чтение из файла", x);
                    StopInput();
                    br.Close();
                }
            }
        }

        /// <summary>
        /// Функция для записи файла  со сжатой последовательностью байт
        /// </summary>
        /// <param name="outputfile">Целевой файл</param>
        public void WriteFile(FileInfo outputfile/*, SynchronizationContextService synchronizationContext*/)
        {
            byte[] block = null;
            int block_number = 0;
            int write_counter = default;

            using (BinaryWriter br = new BinaryWriter(outputfile.Open(FileMode.Create, FileAccess.Write)))
            {
                try
                {
                    br.Write(TaskInfo.START_BLOCK_COUNT);
                    while (write_counter < TaskInfo.START_BLOCK_COUNT)
                    {
                        if (SyncService.GetBlock(out block, out block_number, TypeOfQueue.Output))
                        {
                            if (block == null)
                                break;

                            br.Write(block_number);
                            br.Write(block.Length);
                            br.Write(block);
                            write_counter++;
                            OutputProgress?.Invoke((double)(write_counter / TaskInfo.START_BLOCK_COUNT));

                        }
                        else
                            break;
                        if (write_counter == TaskInfo.START_BLOCK_COUNT)
                            break;
                    }
                    br.Close();
                }
                catch (Exception x)
                {
                    Error?.Invoke("Запись в файл сжатых данных", x);
                    StopOutput();
                    br.Close();
                    outputfile.Delete();
                }
            }
        }

        /// <summary>
        /// Функция для обработки данных
        /// </summary>
        public void Processing()
        {
            byte[] block = null;
            int block_number = 0;
            try
            {
                while (true)
                {
                    if (SyncService.GetBlock(out block, out block_number, TypeOfQueue.Input))
                    {
                        if (block == null)
                            break;
                        byte[] compressedBLock = GZIPOperation.Compress(block);

                        if (!SyncService.SetBlock(compressedBLock, block_number, TypeOfQueue.Output))
                            break;
                        else
                        {
                            Interlocked.Increment(ref process_counter);
                            ProcessingProgress?.Invoke((double)(process_counter / TaskInfo.START_BLOCK_COUNT));
                        }

                        if (process_counter == TaskInfo.START_BLOCK_COUNT)
                        {
                            StopOutput();
                            break;
                        }
                    }
                    else
                        break;
                }
            }
            catch (Exception x)
            {
                Error?.Invoke("Обработка данных", x);
                StopOutput();
                StopInput();
                return;
            }
        }
    }
}

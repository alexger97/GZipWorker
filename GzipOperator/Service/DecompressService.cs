using GzipOperator.Model;
using GzipOperator.Model.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace GzipOperator.Service
{
    /// <summary>
    /// Класс реализущий функционал для разжатия данных
    /// </summary>
    class DecompressService : IService
    {
        private int original_count_parts;
        private long original_lenght;
        private int process_counter;

        private SynchronizationContextService SyncService;

        public event FinalDelegate StopInput;
        public event FinalDelegate StopOutput;

        public event ProgressDelegate InputProgress;
        public event ProgressDelegate OutputProgress;
        public event ProgressDelegate ProcessingProgress;
        public event ErrorDelegate Error;

        public DecompressService(ref SynchronizationContextService synchronizationContextService) => SyncService = synchronizationContextService;

        /// <summary>
        /// Функция для чтения сжатого файла
        /// </summary>
        /// <param name="inputfile">Исходный файл</param>
        public void ReadFile(FileInfo inputfile)
        {
            using (BinaryReader br = new BinaryReader(inputfile.OpenRead()))
            {
                try
                {
                    original_count_parts = br.ReadUInt16();
                    original_lenght = br.ReadInt64();

                    for (int i = 0; i < original_count_parts; i++)
                    {
                        int block_number = br.ReadInt32();
                        int block_length = br.ReadInt32();
                        byte[] block = br.ReadBytes(block_length);

                        if (SyncService.SetBlock(block, block_number, Model.TypeOfQueue.Input))
                            InputProgress(i * 100 / original_count_parts);
                        else
                            break;
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
        /// Функция для записи файла  с оригинальной последовательностью байт
        /// </summary>
        /// <param name="outputfile">Целевой файл</param>
        public void WriteFile(FileInfo outputfile)
        {
            byte[] block = null;
            int block_number = default;
            int write_counter = default;

            var driveInfo = new DriveInfo(outputfile.Directory.Root.FullName);
            if (driveInfo.IsReady)
            {
                if (driveInfo.AvailableFreeSpace < original_lenght)
                    throw new System.IO.IOException($"Недостаточно места на диске {driveInfo.Name}");
                if (driveInfo.DriveFormat == "FAT32" && original_lenght > 4294967295)
                    throw new System.IO.IOException("Ограничение на запись данного объема данных в FAT32");
            }
            else
                throw new System.IO.IOException($"Диск {driveInfo.Name} не готов для записи");

            Dictionary<long, byte[]> UnsortedBuffer = new Dictionary<long, byte[]>();

            using (BinaryWriter br = new BinaryWriter(outputfile.Open(FileMode.Create, FileAccess.Write)))
            {
                try
                {
                    while (write_counter < original_count_parts)
                    {
                        if (SyncService.GetBlock(out block, out block_number, Model.TypeOfQueue.Output))
                        {
                            if (block == null)
                                break;
                            UnsortedBuffer.Add(block_number, block);
                        }
                        else
                            if (UnsortedBuffer.Count == 0)
                            break;
                        if (UnsortedBuffer.ContainsKey(write_counter))
                        {
                            br.Write(UnsortedBuffer[write_counter]);
                            OutputProgress(write_counter * 100 / original_count_parts);
                            UnsortedBuffer.Remove(write_counter);
                            write_counter++;
                        }
                    }
                    br.Close();
                }
                catch (Exception x)
                {
                    Error?.Invoke("Запись в файл сжатых данных", x);
                    StopOutput();
                    br.Close();
                    outputfile.Delete();
                    return;
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
                    if (SyncService.GetBlock(out block, out block_number, Model.TypeOfQueue.Input))
                    {
                        if (block == null)
                            break;

                        byte[] decompressed_byte = GZIPOperation.Decompress(block);

                        if (!SyncService.SetBlock(decompressed_byte, block_number, Model.TypeOfQueue.Output))
                            break;
                        else
                        {
                            Interlocked.Increment(ref process_counter);
                            ProcessingProgress(process_counter * 100 / original_count_parts);
                        }
                        if (process_counter == original_count_parts)
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

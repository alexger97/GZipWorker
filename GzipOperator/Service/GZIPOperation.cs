using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace GzipOperator.Service
{
    /// <summary> Структура, реализующая функционал по непосредственному сжатию и разжатию данных   </summary>
    struct GZIPOperation
    {
        /// <summary> Размер блока </summary>
        public const int SIZE_BUFFER = 1024 * 1024;

        /// <summary>
        /// Функция по сжатию блока байтов с использованием GZip
        /// </summary>
        /// <param name="source">Блок байт</param>
        /// <returns>Сжатый блок</returns>
        public static byte[] Compress(byte[] source)
        {
            MemoryStream memoryStream = new MemoryStream();

            using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                gZipStream.Write(source, 0, source.Length);

            return memoryStream.ToArray();
        }

        /// <summary>
        /// Функция по разжатию блока байтов с использованием GZip
        /// </summary>
        /// <param name="source">Блок байт</param>
        /// <returns>Разжатый блок блок</returns>
        public static byte[] Decompress(byte[] source)
        {
            int original_size;

            byte[] decompressedBlock = new byte[SIZE_BUFFER];

            using (MemoryStream decompressedStream = new MemoryStream(source))
            {
                using (GZipStream zip = new GZipStream(decompressedStream, CompressionMode.Decompress))
                    original_size = zip.Read(decompressedBlock, 0, SIZE_BUFFER);

            }
            return decompressedBlock.Take(original_size).ToArray();
        }
    }
}

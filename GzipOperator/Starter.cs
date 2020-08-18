
using GzipOperator.Model;
using GzipOperator.Model.Interface;
using GzipOperator.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace GzipOperator
{
    /// <summary>
    /// Класс-стартер для сервиса
    /// </summary>
    public static class Starter
    {
        public static IService Service;
        public static Reporter Reporter;

        public static TaskInfo ActualTaskInfo;
        public static SynchronizationContextService SyncContext;

        internal static Thread[] Threads;

        private static Thread ReadThread;
        private static Thread WriteThread;

        private static bool IsSucces = true;

        /// <summary>
        /// Конфигурация сервиса
        /// </summary>
        /// <param name="args">Аргументы</param>
        /// <param name="errorReporter"> Репортер</param>
        public static bool Configure(string[] args, Reporter errorReporter)
        {
            try
            {
                Reporter = errorReporter;
                ActualTaskInfo = ArgumentsParseService.TryGetTaskInfo(args);

                SyncContext = new SynchronizationContextService(ActualTaskInfo);
                SyncContext.Error += Reporter.ShowExceptionMessage;
                SyncContext.Error += (x, s) => CheckException();

                //Буду исходить из того, что имеется процессор, который имеет на каждое физическое ядро - два логических.
                //Тогда создадим соответсвующее количество пототоков.
                Threads = new Thread[Environment.ProcessorCount * 2];

                if (ActualTaskInfo.TypeOfOperation == TypeOfOperation.Compress)
                    Service = new CompressService(ref SyncContext);
                else
                    Service = new DecompressService(ref SyncContext);

                Service.Error += Reporter.ShowExceptionMessage;
                Service.Error += (x, s) => CheckException();

                Service.StopInput += SyncContext.FinalInput;
                Service.StopOutput += SyncContext.FinalOutput;

                ReadThread = new Thread(() => Service.ReadFile(ActualTaskInfo.FileInputInfo));
                WriteThread = new Thread(() => Service.WriteFile(ActualTaskInfo.FileOutputInfo));

                for (int i = 0; i < Threads.Length; i++)
                    Threads[i] = new Thread(() => Service.Processing());
                return true;

            }
            catch (Exception x)
            {
                Reporter.ShowExceptionMessage("Конфигурация программы", x);
                return false;
            }
        }

        /// <summary>
        /// Старт сервиса
        /// </summary>
        public static bool Start()
        {
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                ReadThread.Start();
                for (int i = 0; i < Threads.Length; i++)
                    Threads[i].Start();
                WriteThread.Start();

                WriteThread.Join();

                for (int i = 0; i < Threads.Length; i++)
                    Threads[i].Join();

                ReadThread.Join();

                stopwatch.Stop();
                if (IsSucces)
                {
                    Reporter.ShowMessage($"Задача выполнена. Потраченное время : {stopwatch.Elapsed.ToString()}");
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        static void CheckException() => IsSucces = false;
    }
}

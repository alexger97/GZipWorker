
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
    public static class Starter
    {


        public static IService Service;
        public static Reporter Reporter;

        public static TaskInfo ActualTaskInfo;
        public static SynchronizationContextService SyncContext;

        private static Thread[] Threads;

        private static Thread ReadThread;
        private static Thread WriteThread;

        private static bool IsSucces = true;

        public static bool Configure(string[] args, Reporter errorReporter)
        {
            try
            {
                Reporter = errorReporter;

                ArgumentsParseService.Error += Reporter.ShowExceptionMessage;
                ArgumentsParseService.Error += (x, s) => CheckError();
                ArgumentsParseService.TryGetTaskInfo(args, ref ActualTaskInfo);



                SyncContext = new SynchronizationContextService(ActualTaskInfo);
                //SyncContext.Error += Reporter.ShowExceptionMessage;

                Threads = new Thread[Environment.ProcessorCount * 2];

                if (ActualTaskInfo.TypeOfOperation == TypeOfOperation.Compress)
                    Service = new CompressService(ref SyncContext);
                else
                    Service = new DecompressService(ref SyncContext);

                Service.Error += Reporter.ShowExceptionMessage;
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


        public static bool Start()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            ReadThread.Start();
            for (int i = 0; i < Threads.Length; i++)
                Threads[i].Start();
            WriteThread.Start();

            
            WriteThread.Join();
       
            ReadThread.Join();
            for (int i = 0; i < Threads.Length; i++)
                Threads[i].Join();
            stopwatch.Stop();

            if (IsSucces)
            {
                Reporter.ShowMessage($"Задача выполнена. Потраченное время : {stopwatch.Elapsed.ToString()}");
                return true;
            }
            return false;


        }
        static void CheckError() => IsSucces = false;
    }
}

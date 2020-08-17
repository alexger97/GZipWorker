using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using WPF_Interface.ViewModel.Command;

namespace WPF_Interface.ViewModel
{
    class MainViewModel : ViewModelBase
    {

        private FileInfo inputFile;
        FileInfo InputFile { get => inputFile; set { inputFile = value; OnPropertyChanged("InputFile"); } }

        private FileInfo outputFile;
        FileInfo OutputFile { get => outputFile; set { outputFile = value; OnPropertyChanged("OutputFile"); } }

        private double read_Progress = 0;

        public double Read_Progress { get => read_Progress; set { read_Progress = value; OnPropertyChanged("Read_Progress"); } }


        private double write_Progress = 0;
        public double Write_Progress { get => write_Progress; set { write_Progress = value; OnPropertyChanged("Write_Progress"); } }

        private double process_Progress = 0;
        public double Process_Progress { get => process_Progress; set { process_Progress = value; OnPropertyChanged("Process_Progress"); } }

        public MainViewModel(FileInfo fileInfo1, FileInfo fileInfo2)
        {
        //    GzipOperator.Starter.Configure(new string[] { "compress", "C:\\Test\\1.cr2", "C:\\Test\\1_1.cr2" });
        //    GzipOperator.Starter.Start();
        }

        RelayCommand clickButton;
        public RelayCommand ClickButton
        {
            get
            {
                if (clickButton == null)
                    clickButton = new RelayCommand(ExecuteClickButton, CanExecuteClickButton);
                return clickButton;
            }
        }

        public void ExecuteClickButton(object parameter)
        {

            //GzipOperator.Starter.Configure(new string[] { "compress", "C:\\Test\\1.cr2", "C:\\Test\\1_1.cr2" });
            //GzipOperator.Starter.Service.InputProgress += (x) => { Read_Progress++; };
            //GzipOperator.Starter.Service.OutputProgress += (x) => { Write_Progress++; };
            //GzipOperator.Starter.Service.ProcessingProgress += (x) => { Process_Progress++; };
            //new Thread(() => GzipOperator.Starter.Start()).Start();

        }
        public bool CanExecuteClickButton(object parameter)
        {
            return true;
        }




    }
}

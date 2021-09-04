using Maple.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Maple.ViewModels
{
    class JobsWindowViewModel : ViewModelBase
    {
        private ObservableCollection<string> _jobsDataList;
        public ObservableCollection<string> JobsDataList 
        { 
            get { return _jobsDataList; }
            set { _jobsDataList = value; NotifyPropertyChanged(); }
        }

        private string _selectedJobData;

        public string SelectedJobData
        {
            get { return _selectedJobData; }
            set { _selectedJobData = value; NotifyPropertyChanged(); }
        }

        private ICommand _editJobCommand;

        public ICommand EditJobCommand
        {
            get
            {
                return _editJobCommand ?? (_editJobCommand = new CommandHandler(() => EditJob(), () => true));
            }
        }

        private void EditJob()
        {
            Windows.JobEdit jobEditWindow = new Windows.JobEdit((Jobs)Enum.Parse(typeof(Jobs), SelectedJobData));
            IsActive = false;
            jobEditWindow.ShowDialog();
            IsActive = true;
        }

        private ICommand _createNewJobCommand;

        public ICommand CreateNewJobCommand
        {
            get
            {
                return _createNewJobCommand ?? (_createNewJobCommand = new CommandHandler(() => CreateNewJob(), () => true));
            }
        }

        private void CreateNewJob()
        {
            Windows.JobEdit jobEditWindow = new Windows.JobEdit(null);
            IsActive = false;
            jobEditWindow.ShowDialog();
            IsActive = true;
        }

        public JobsWindowViewModel()
        {
            IsActive = true;
            IsClosing = false;
            var classExportLocation = System.Configuration.ConfigurationManager.AppSettings["ClassExportLocation"];
            var jobsWithDirectories = Directory.GetFiles(classExportLocation).ToList();
            var jobs = jobsWithDirectories.Select(x => { return x.Split('\\').Last(); }).ToList();
            JobsDataList = new ObservableCollection<string>(jobs);
            SelectedJobData = JobsDataList.FirstOrDefault();
        }
    }
}

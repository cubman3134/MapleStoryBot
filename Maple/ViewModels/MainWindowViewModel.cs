using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Maple.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {

        private ICommand _alterJobsCommand;

        public ICommand AlterJobsCommand
        {
            get
            {
                return _alterJobsCommand ?? (_alterJobsCommand = new CommandHandler(() => AlterJobs(), () => true));
            }
        }

        private void AlterJobs()
        {
            Windows.JobsWindow jobsWindow = new Windows.JobsWindow();
            IsActive = false;
            jobsWindow.ShowDialog();
            IsActive = true;
        }

        private ICommand _createNewMapCommand;
        public ICommand CreateNewMapCommand
        {
            get
            {
                return _createNewMapCommand ?? (_createNewMapCommand = new CommandHandler(() => CreateNewMap(), () => true));
            }
        }

        public void CreateNewMap()
        {
            Windows.MapCreator mapCreatorWindow = new Windows.MapCreator();
            IsActive = false;
            mapCreatorWindow.ShowDialog();
            IsActive = true;
        }

        public MainWindowViewModel()
        {
            IsActive = true;
            IsClosing = false;
        }

    }
}

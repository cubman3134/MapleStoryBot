using Maple.Data;
using Maple.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Maple.Windows
{
    /// <summary>
    /// Interaction logic for JobEdit.xaml
    /// </summary>
    public partial class JobEdit : Window
    {
        JobEditViewModel ModelData
        {
            get { return DataContext as JobEditViewModel; }
        }
        public JobEdit(Jobs? selectedJob)
        {
            InitializeComponent();
            DataContext = new JobEditViewModel(selectedJob, this);
        }
    }
}

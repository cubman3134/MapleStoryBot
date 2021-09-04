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
    /// Interaction logic for SkillEdit.xaml
    /// </summary>
    public partial class SkillEdit : Window
    {
        SkillEditViewModel ModelData
        {
            get { return DataContext as SkillEditViewModel; }
        }

        public SkillData SkillDataData
        {
            get { return ModelData.SkillDataData; }
        }

        public bool IsWindowAccepted
        {
            get { return ModelData.WindowAccepted; }
        }

        public SkillEdit()
        {
            InitializeComponent();
            DataContext = new SkillEditViewModel(this);
        }
    }
}

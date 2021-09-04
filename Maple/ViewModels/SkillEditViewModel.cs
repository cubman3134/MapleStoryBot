using Maple.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Maple.ViewModels
{
    class SkillEditViewModel : ViewModelBase
    {
        private SkillData _skillDataData;

        public SkillData SkillDataData
        {
            get { return _skillDataData; }
            set { _skillDataData = value; NotifyPropertyChanged(); }
        }

        public string SkillName
        {
            get { return _skillDataData.SkillName; }
            set { _skillDataData.SkillName = value; NotifyPropertyChanged(); }
        }

        public int DiscrepencyTimeMillis
        {
            get { return _skillDataData.DiscrepencyTimeMillis; }
            set { _skillDataData.DiscrepencyTimeMillis = value; NotifyPropertyChanged(); }
        }

        public int HoldTimeMills
        {
            get { return _skillDataData.HoldMillis; }
            set { _skillDataData.HoldMillis = value; NotifyPropertyChanged(); }
        }

        public string Key
        {
            get { return _skillDataData.Key.ToString(); }
            set { char c; char.TryParse(value[0].ToString(), out c); _skillDataData.Key = c; NotifyPropertyChanged(); }
        }

        public bool UseOnLogin
        {
            get { return _skillDataData.UseOnLogin; }
            set { _skillDataData.UseOnLogin = value; NotifyPropertyChanged(); }
        }

        private ICommand _acceptCommand;

        public ICommand AcceptCommand
        {
            get
            {
                return _acceptCommand ?? (_acceptCommand = new CommandHandler(() => Accept(), () => true));
            }
        }

        private void Accept()
        {
            WindowAccepted = true;
            _parentWindow.Close();
        }

        private ICommand _cancelCommand;

        public ICommand CancelCommand
        {
            get
            {
                return _cancelCommand ?? (_cancelCommand = new CommandHandler(() => Cancel(), () => true));
            }
        }

        private void Cancel()
        {
            _parentWindow.Close();
        }

        private bool _windowAccepted;
        public bool WindowAccepted
        {
            get { return _windowAccepted; }
            set { _windowAccepted = value; NotifyPropertyChanged(); }
        }

        private Window _parentWindow;

        public SkillEditViewModel(Window parentWindow)
        {
            _skillDataData = new SkillData();
            _parentWindow = parentWindow;
            WindowAccepted = false;
            IsActive = true;
            IsClosing = false;
        }
    }
}

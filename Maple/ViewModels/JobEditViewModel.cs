using Maple.Data;
using Maple.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Maple.ViewModels
{
    class JobEditViewModel : ViewModelBase
    {
        private CharacterData _characterData;

        public ObservableCollection<string> PotentialJobs
        {
            get { return new ObservableCollection<string>(Enum.GetNames(typeof(Jobs))); }
        }

        public string SelectedJob
        {
            get { return _characterData.CharacterJob.ToString(); }
            set { _characterData.CharacterJob = (Jobs)Enum.Parse(typeof(Jobs), value); NotifyPropertyChanged(); }
        }

        public string CharacterName
        {
            get { return _characterData.CharacterName; }
            set { _characterData.CharacterName = value; NotifyPropertyChanged(); }
        }


        //private List<SkillData> _skillDataList;
        public List<SkillData> SkillDataList
        {
            get { return _characterData.SkillDataList; }
            set { _characterData.SkillDataList = value; NotifyPropertyChanged("SkillNamesDataList"); NotifyPropertyChanged(); }
        }

        public ObservableCollection<string> SkillNamesDataList
        {
            get { return new ObservableCollection<string>(SkillDataList?.Select(x => x.SkillName) ?? new List<string>()); }
        }

        //private int _characterSelectionLocation;
        public int CharacterSelectionLocation
        {
            get { return _characterData.CharacterSelectionLocation; }
            set { _characterData.CharacterSelectionLocation = value; NotifyPropertyChanged(); }
        }

        private string _selectedSkillName;
        public string SelectedSkillName
        {
            get { return _selectedSkillName; }
            set { _selectedSkillName = value; NotifyPropertyChanged(); }
        }

        private ICommand _addSkillCommand;

        public ICommand AddSkillCommand
        {
            get
            {
                return _addSkillCommand ?? (_addSkillCommand = new CommandHandler(() => AddSkill(), () => true));
            }
        }

        private void AddSkill()
        {
            SkillEdit skillEditWindowData = new SkillEdit();
            skillEditWindowData.ShowDialog();
            if (skillEditWindowData.IsWindowAccepted)
            {
                SkillDataList.Add(skillEditWindowData.SkillDataData);
            }
        }

        private ICommand _deleteSkillCommand;

        public ICommand DeleteSkillCommand
        {
            get
            {
                return _deleteSkillCommand ?? (_deleteSkillCommand = new CommandHandler(() => DeleteSkill(), () => true));
            }
        }

        private void DeleteSkill()
        {
            var skillToRemove = _characterData.SkillDataList.Where(x => x.SkillName == SelectedSkillName).FirstOrDefault();
            if (skillToRemove == null)
            {
                return;
            }
            _characterData.SkillDataList.Remove(skillToRemove);
            SelectedSkillName = SkillDataList?.FirstOrDefault()?.SkillName ?? "";
        }

        private ICommand _recordJumpsCommand;

        public ICommand RecordJumpsCommand
        {
            get
            {
                return _recordJumpsCommand ?? (_recordJumpsCommand = new CommandHandler(() => RecordJumps(), () => true));
            }
        }

        private void RecordJumps()
        {
            Input.ResetMouse();
            Input.ClickMouse();
            Thread.Sleep(10);
            Input.ReleaseMouse();
            _characterData.JumpDataData = JumpData.GenerateJumpData();
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
            string fileName = _characterData.CharacterJob.ToString();
            string jsonString = JsonConvert.SerializeObject(_characterData, Formatting.Indented);
            string characterExportLocation = System.Configuration.ConfigurationManager.AppSettings["ClassExportLocation"];
            string filePath = Path.Combine(characterExportLocation, fileName);
            File.WriteAllText(filePath, jsonString);
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

        private Window _parentWindow;

        public JobEditViewModel(Jobs? selectedJob, Window window)
        {
            if (selectedJob == null)
            {
                _characterData = new CharacterData();
            }
            else
            {
                string fileLocation = System.Configuration.ConfigurationManager.AppSettings["ClassExportLocation"];
                string fullPath = Path.Combine(fileLocation, selectedJob.ToString());
                string jsonString = File.ReadAllText(fullPath);
                _characterData = JsonConvert.DeserializeObject<CharacterData>(jsonString);
            }
            _parentWindow = window;
            IsActive = true;
            IsClosing = false;
            SelectedJob = PotentialJobs[0];
        }
    }
}

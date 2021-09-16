using Maple.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Maple.ViewModels
{
    class GamePlayerViewModel : ViewModelBase
    {
        public GameData GameDataData;

        private Thread _gameBrainThread;

        public void StartGameBrain()
        {
            GameDataData = new GameData();
            GameDataData.GameBrain();
        }

        public GamePlayerViewModel()
        {
            Input.NewInputEnabled = true;
            IsActive = true;
            IsClosing = false;
            _gameBrainThread = new Thread(StartGameBrain) { Name = "Game Brain Thread" };
            _gameBrainThread.Start();
        }
    }
}

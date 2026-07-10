using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace TemplateGenerator.Core.Models
{
    public class ProgramModel
    {
        public int RobotNumber { get; set; }
        public string ProgramName { get; set; }
        public string ProgramNameLowerChar { get; set; }
        public string ProgramNameUpperChar { get; set; }
        public int StationNumber { get; set; }
        public bool StationFreeEnabled;
        public string HellaProgramName { get; set; }
        public string HellaProgramNameLowerChar { get; set; }
        public string HellaProgramNameUpperChar { get; set; }
        public ObservableCollection<StationModel> Stations { get; set; } = new ObservableCollection<StationModel>();

        public ProgramModel(string programName, int robotNumber)
        {
            RobotNumber = robotNumber;
            ProgramName = programName;
            ProgramNameLowerChar = ProgramName.ToLower();
            ProgramNameUpperChar = ProgramName.ToUpper();
            HellaProgramName = programName;
            HellaProgramNameLowerChar = programName.ToLower();
            HellaProgramNameUpperChar = programName.ToUpper();
        }

        public void AddStation(string stationName, bool stationFreeEnabled)
        {
            Stations.Add(new StationModel(stationName, stationFreeEnabled));
        }
    }
}

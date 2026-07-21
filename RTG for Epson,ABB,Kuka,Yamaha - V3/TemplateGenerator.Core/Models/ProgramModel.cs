using DocumentFormat.OpenXml.Bibliography;
using MvvmCross.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace TemplateGenerator.Core.Models
{
    public class ProgramModel
    {
        //private int i;
        public int RobotNumber { get; set; } = 0;
        public string ProgramName { get; set; } = "robot";
        public string ProgramNameLowerChar { get; set; }
        public string ProgramNameUpperChar { get; set; }
        public int StationNumber { get; set; } 
        public bool StationFreeEnabled;
        public bool SimulationProgram;
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
            //this.i++;
            Stations.Add(new StationModel(stationName, stationFreeEnabled));
        }

        public void RemoveStation(int i)
        {
            Stations.Remove(Stations[i-1]);
            //i--;
        }
    }
}

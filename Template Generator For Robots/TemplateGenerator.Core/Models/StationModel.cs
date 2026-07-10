using System;
using System.Collections.Generic;
using System.Text;
using MvvmCross.Commands;



namespace TemplateGenerator.Core.Models
{
    public class StationModel
    {
        //private string _robotStationName;
        public string RobotStationName { get; set; }
        
        public string RobotStationNameToUpper { get; set; }

        public bool StationFreeEnabled { get; set; }

        public bool Pallet { get; set; }

        public int Positions { get; set; } = 1;

        public StationModel(string stationName)
        {
            RobotStationName = stationName;
        }

        public StationModel(string stationName, bool stationFreeEnabled)
        {
            RobotStationName = stationName;
            StationFreeEnabled = stationFreeEnabled;

            if (string.IsNullOrWhiteSpace(stationName))
                RobotStationNameToUpper = "";
            StationNameToUpper(stationName);
        }

        private void StationNameToUpper (string stationName)
        {
            StringBuilder newText = new StringBuilder(stationName.Length * 2);
            newText.Append(stationName[0]);
            for (int i = 1; i < stationName.Length; i++)
            {
                if (char.IsUpper(stationName[i]) && stationName[i - 1] != '_')
                    newText.Append('_');
                newText.Append(stationName[i]);
            }
            RobotStationNameToUpper = newText.ToString().ToUpper();
        }

        //potrebno za primerjat če ime postaje že obstaja
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is StationModel objAsPart)) return false;
            else return Equals(objAsPart);
        }

        public int PartId;

        public override int GetHashCode()
        {
            return PartId;
        }

        public bool Equals(StationModel other)
        {
            if (other == null) return false;
            return (this.RobotStationName.Equals(other.RobotStationName));
        }
    }
}

using MvvmCross.ViewModels;
using MvvmCross.Commands;
using TemplateGenerator.Core.Models;
using TemplateGenerator.Core.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows;


namespace TemplateGenerator.Core.ViewModels
{
    public class ShellViewModel : MvxViewModel
    {
        private int i;
        private int j = 1;
        public int currstation;
        private string _textUpdate = null;
        private string _stationName;
        private string _stationFullName;
        private bool _stationFreeEnabled;
        private string _selectedTemplate;
        private string _programName;
        private bool _generateSimpleProgram = true;
        private bool _simulationProgram;
        private ProgramModel _selectedProgram;
        private StationModel _selectedStation;
        public ObservableCollection<ProgramModel> Program { get; set; } = new ObservableCollection<ProgramModel>();
        GenerateExcelIO excell = new GenerateExcelIO();

        public static List<string> Templates { get; set; }

        public ShellViewModel()
        {
            AddProgramCommand = new MvxCommand(AddProgram);
            RemoveProgramCommand = new MvxCommand(RemoveProgram);
            AddStationCommand = new MvxCommand(AddStation);
            RemoveStationCommand = new MvxCommand(RemoveStation);
            TestButtonCommand = new MvxCommand(TestButton);
            ManualButtonCommand = new MvxCommand(ManualOpenTxt);

            //add default program
            ProgramName = "robot";
            i = 1;
            Program.Add(new ProgramModel(ProgramName, i));
            TextUpdate = $"Program {ProgramName} added.";
            Program[i - 1].AddStation("Home", false);
            SelectedProgram = Program[i - 1];
            StationFreeEnabled = true;
            //SimulationProgram = true;


            Templates = new List<string>()
            {
                "Epson Hidria",
                "KUKA Hella",
                "ABB Hidria",
                "ABB Simulacija",
                "Yamaha Hidria",
                "Kawasaki Hidria"
            };
        }

        public ProgramModel SelectedProgram
        // da vemo kerega smo označili v comboboxu
        {
            get { return _selectedProgram; }
            set
            {
                _selectedProgram = value;
                RaisePropertyChanged(() => SelectedProgram);
            }
        }

        public bool GenerateSimpleProgram
        {
            get { return _generateSimpleProgram; }
            set
            {
                SetProperty(ref _generateSimpleProgram, value);
                RaisePropertyChanged(() => GenerateSimpleProgram);
            }
        }

        public StationModel SelectedStation
        {
            get { return _selectedStation; }
            set
            {
                SetProperty(ref _selectedStation, value);
                RaisePropertyChanged(() => SelectedStation);
            }
        }

        //public IMvxCommand SimulationProgramCommand { get; set; }
        public bool SimulationProgram
        {
            get { return _simulationProgram; }
            set
            {
                SetProperty(ref _simulationProgram, value);
                //_stationFreeEnabled = value;
                RaisePropertyChanged(() => SimulationProgram);
            }
        }
        /// ////////////////////////////////////////////////////////////////////
        //Robot
        /// ////////////////////////////////////////////////////////////////////

        public string SelectedTemplate
        {
            get { return _selectedTemplate; }
            set
            {
                SetProperty(ref _selectedTemplate, value);
                RaisePropertyChanged(() => SelectedTemplate);
            }
        }

        public string TextUpdate
        {   //Text za prikaz dodanega programa
            get { return _textUpdate; }
            set
            {
                SetProperty(ref _textUpdate, value);
                RaisePropertyChanged(() => TextUpdate);
            }
        }

        public string ProgramName
        {
            get { return _programName; }
            set
            {
                SetProperty(ref _programName, value);
                RaisePropertyChanged(() => ProgramName);
            }
        }

        public IMvxCommand AddProgramCommand { get; set; }

        public void AddProgram()
        // add button 
        {
            this.i++;
            Program.Add(new ProgramModel(ProgramName, i));
            TextUpdate = $"Program {ProgramName} added.";
            Program[i - 1].AddStation("Home", false);
            ProgramName = "";
        }

        public IMvxCommand RemoveProgramCommand { get; set; }

        public void RemoveProgram()
        {
            Program.Remove(Program[i - 1]);
            TextUpdate = "Robot" + i + " removed";
            i--;
        }

        /// ////////////////////////////////////////////////////////////////////
        //Station
        /// ////////////////////////////////////////////////////////////////////
        /// 
        // Oppen StationEditView

        public string StationName
        {//Ime ki ga preberemo iz textboxa
            get { return _stationName; }
            set
            {
                SetProperty(ref _stationName, value);
                RaisePropertyChanged(() => StationName);
            }
        }

        public IMvxCommand StationFreeEnabledCommand { get; set; }
        public bool StationFreeEnabled
        {
            get { return _stationFreeEnabled; }
            set
            {
                _stationFreeEnabled = value;
                RaisePropertyChanged(() => StationFreeEnabled);
            }
        }

        public string StationFullName
        {
            get { return _stationFullName; }
            set
            {
                SetProperty(ref _stationFullName, value);
                RaisePropertyChanged(() => StationFullName);
            }
        }

        public bool CanAddStation()
        {
            if (string.IsNullOrWhiteSpace(StationName) || SelectedProgram == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public IMvxCommand AddStationCommand { get; set; }
        public void AddStation()
        {
            if (StationName.All(char.IsLetterOrDigit) & StationName.Length < 10)  //samo črke in številke so dovoljene
            {
                if (SelectedProgram.Stations.Contains(new StationModel(StationName)))
                {
                    //postaja že obstaja v zbranem robotu
                    TextUpdate = $"Station {StationName} already exist in {SelectedProgram.ProgramName}";
                    StationName = "";
                }
                else
                {
                    this.j++;
                    SelectedProgram.AddStation(StationName, StationFreeEnabled);
                    TextUpdate = $"Station {StationName} added to { SelectedProgram.ProgramName}.";
                    StationName = "";

                }
            }
            else if (StationName.Length >= 10)
            {
                TextUpdate = "Maximum of 9 characters is allowed";
            }
            else
            {
                TextUpdate = "Only alphanumeric chars are allowed";
            }
        }

        public IMvxCommand RemoveStationCommand { get; set; }
        public void RemoveStation()
        {
            SelectedProgram.RemoveStation(j);
            TextUpdate = $"Station {StationName} removed.";
            StationName = "";
            j--;
        }


        public IMvxCommand TestButtonCommand { get; set; }
        public void TestButton()
        {

        }

        public IMvxCommand ManualButtonCommand { get; set; }
        public void ManualOpenTxt()
        {
            Process.Start("explorer.exe", @".\Templates\Manuals");
        }

        #region // GENERATE TEMPLATE //
        //public IMvxCommand GenerateProjectCommand { get; set; }
        public void GenerateProject(string s)
        {
            string path = s;

            if (SelectedTemplate == "Epson Hidria")
            {
                string DateAndTime = DateTime.Now.ToString(@"dd.MM.yyyy_HH.mm.ss");
                Directory.CreateDirectory(path + "\\"  + string.Format("EpsonGeneratedTemplate_{0}", DateAndTime).ToString());
                path = path + "\\" + string.Format("EpsonGeneratedTemplate_{0}", DateAndTime).ToString();

                if (path != String.Empty)
                {
                    using (StreamWriter sw = File.CreateText(path + "//" + "Main.prg"))
                    {
                        sw.Write(Template.GetMainFunc(Program));
                        sw.Write(Template.GetErrorHandlingFunc(Program));
                        sw.Write(Template.GetEstopHandlingFunc(Program));
                    }

                    foreach (ProgramModel robot in Program)
                    {
                        //Generate robot file(s)
                        using (StreamWriter sw = File.CreateText(path + "//" + $"{robot.ProgramName.ToLower()}.prg"))
                        {
                            sw.Write(Template.GetHeader(robot));
                            sw.Write(Template.GetPalletsDefinitions(robot));
                            sw.Write(Template.GetInitFunc(robot));
                            sw.Write(Template.GetHomingFunc(robot, SimulationProgram));
                            sw.Write(Template.GetTestFunc(robot));
                            sw.Write(Template.GetOperationFunc(robot));
                            if (GenerateSimpleProgram)
                            {
                                sw.Write(Template.GetMovementsSimpleProgramFunc(robot));
                                sw.Write(Template.GetMoveOnStationSimpleProgramFunc(robot));
                                sw.Write(Template.GetMoveAwaySimpleProgramFunc(robot));
                                //sw.Write(Template.GetGoOnAdditionalPositionsFunc(robot));
                            }
                            else
                            {
                                sw.Write(Template.GetMovementFuncs(robot));
                                sw.Write(Template.GetMoveOnStationFunc(robot));
                                sw.Write(Template.GetMoveAwayFunc(robot));
                            }
                            sw.Write(Template.GetPowerModeFunc(robot));
                            sw.Write(Template.GetFullPowerModeFunc(robot));
                            sw.Write(Template.GetSlowPowerModeFunc(robot));
                            sw.Write(Template.GetCpBarrierFunc(robot));
                            sw.Write(Template.GetStationsFreeFunc(robot));
                            sw.Write(Template.GetStationsBusyFunc(robot));
                            sw.Write(Template.GetResetDepartLocationsFunc(robot));
                            sw.Write(Template.GetResetDestLocationsFunc(robot));
                            sw.Write(Template.GetResetProfibusOutputsFunc(robot));
                            sw.Write(Template.GetResetFlagsFunc(robot));
                        }

                        // Generate io file
                        using (StreamWriter sw = File.CreateText(path + "//" + $"{robot.ProgramName}.io"))
                        {
                            sw.Write(Template.GetAllIOLabels(robot));
                        }
                        using (StreamWriter sw = File.CreateText(path + "//" + "IOLabels.dat"))
                        {
                            sw.Write(Template.GetIOLablesFunc(robot));
                        }
                        using (StreamWriter sw = File.CreateText(path + "//" + $"{robot.ProgramName}1.pts"))
                        {
                            sw.Write(Template.GeneratePointsFunc(robot));
                        }

                        // Generate UserError file
                        using (StreamWriter sw = File.CreateText(path + "//" + "UserErrors.dat"))
                        {
                            sw.Write(Template.GetUserError());
                        }

                        excell.GenerateIO("Epson",robot, path);
                    }
                }
                TextUpdate = "Epson program generated: " + path;
                Process.Start("explorer.exe", path);
                // TO DO - Odpri direktori

            }
            else if (SelectedTemplate == "KUKA Hella")
            {
                if (path != String.Empty)
                {

                    Directory.CreateDirectory(path + "//" + "R1/Program/");
                    using (StreamWriter sw = File.CreateText(path + "//" + "R1/Program/Main.src"))
                    {
                        sw.Write(Template.GetKukaHellaRobotMainFunctFromat(Program));
                    }
                    foreach (ProgramModel program in Program)
                    {
                        using (StreamWriter sw = File.CreateText(path + "//" + $"R1/Program/{ program.ProgramName.ToLower()}_main.dat."))
                        {
                            sw.Write(Template.GetKukaHellaProgramMainDat());
                        }
                        using (StreamWriter sw = File.CreateText(path + "//" + $"R1/Program/{ program.ProgramName.ToLower()}_main.src"))
                        {
                            sw.Write(Template.GetKukaHellaRobotProgInitFromat(program));
                            if (GenerateSimpleProgram)
                            { sw.Write(Template.GetKukaHellaProgHomingFromatSimpleProgram(program)); }
                            else
                            { sw.Write(Template.GetKukaHellaProgHomingFromat(program)); }
                            sw.Write(Template.GetKukaHellaProgMainTaskFromat(program));
                        }

                        using (StreamWriter sw = File.CreateText(path + "//" + $"R1/Program/{ program.ProgramName.ToLower()}_motion.src"))
                        {
                            if (GenerateSimpleProgram)
                            {
                                sw.Write(Template.GetKukaHellaMotionProgramFormatSimpleProgram(program));
                                sw.Write(Template.GetKukaHellaMoveAwayFormatSimpleProgram(program));
                                sw.Write(Template.GetKukaHellaMoveOnStationFormatSimpleProgram(program));
                            }
                            else
                            {
                                sw.Write(Template.GetKukaHellaMotionProgramFormat(program));
                                sw.Write(Template.GetKukaHellaMoveAwayFormat(program));
                                sw.Write(Template.GetKukaHellaMoveOnStationFormat(program));
                            }
                        }
                        using (StreamWriter sw = File.CreateText(path + "//" + $"R1/Program/{ program.ProgramName.ToLower()}_motion.dat"))
                        {
                            sw.Write(Template.GetKukaHellaMotionDatFormat(program));
                        }
                    }

                    Directory.CreateDirectory(path + "//" + "R1/Program/Functions");
                    using (StreamWriter sw = File.CreateText(path + "//" + "R1/Program/Functions/robotFunctions.src"))
                    {
                        foreach (ProgramModel program in Program)
                        {
                            sw.Write(Template.GetKukaHellaStandardFunctionsFromat(program));
                        }
                        sw.Write(Template.GetKukaHellaDistFunctionFormat());
                        sw.Write(Template.GetKukaHellaPalletFunctionFormat());
                    }

                    using (StreamWriter sw = File.CreateText(path + "//" + "R1/Program/Functions/robotFunctions.dat"))
                    {
                        sw.Write(Template.GetKukaHellaFunctionsDatFormat());
                    }

                    using (StreamWriter sw = File.CreateText(path + "//" + "R1/Cell.src"))
                    {
                        sw.Write(Template.GetKukaHellaCellFormat());
                    }

                    Directory.CreateDirectory(path + "//" + "R1/System/");
                    using (StreamWriter sw = File.CreateText(path + "//" + "R1/System/sps.sub"))
                    {
                        sw.Write(Template.GetKukaHellaSpsFormat());
                    }

                    using (StreamWriter sw = File.CreateText(path + "//" + "R1/System/$config.dat"))
                    {
                        foreach (ProgramModel program in Program)
                        {
                            sw.Write(Template.GetKukaHellaConfigDatFormat(program));
                        }
                    }

                }
                TextUpdate = "KUKA program generated: " + path;
                Process.Start("explorer.exe", path);
            }
            else if (SelectedTemplate == "ABB Simulacija")
            {
                if (path != String.Empty)
                {
                    foreach (ProgramModel robot in Program)
                    {
                        using (StreamWriter sw = File.CreateText(path + "//" + "Global.mod"))
                        sw.Write(Template.GetABBGlobalFunc(robot));


                        using (StreamWriter sw = File.CreateText(path + "//" + "Module1.mod"))
                        {
                            sw.Write(Template.GetABBModule1Func(robot));
                        }
                    }
                    TextUpdate = "ABB sim program generated: " + path;
                    Process.Start("explorer.exe", path);
                }
            }
            else if (SelectedTemplate == "ABB Hidria")
            {
                {

                    if (path != String.Empty)
                    {
                        using (StreamWriter sw = File.CreateText(path + "//" + "Module1.mod"))
                        {
                            sw.Write(Template.GetABBHidriaModule1Func(Program));
                        }

                        using (StreamWriter sw = File.CreateText(path + "//" + "navodila.txt"))
                        {
                            sw.Write(Template.ABBHidriaNavodilaFunc);
                        }

                        using (StreamWriter sw = File.CreateText(path + "//" + "SYS.cfg"))
                        {
                            sw.Write(Template.ABBHidriaSYSFunc);
                        }

                        using (StreamWriter sw = File.CreateText(path + "//" + "Global.mod"))
                        {
                            sw.Write(Template.GetABBHidriaGlobalFunc(Program));
                        }

                        foreach (ProgramModel program in Program)
                        {
                            

                            using (StreamWriter sw = File.CreateText(path + "//" + "Communication.mod"))
                            {
                                sw.Write(Template.GetABBHidriaCommunicationFunc(Program));
                            }

                            using (StreamWriter sw = File.CreateText(path + "//" + $"{program.ProgramName}_Motion.mod"))
                            {
                                sw.Write(Template.GetABBHidriaMotionFunc(program));
                            }

                            using (StreamWriter sw = File.CreateText(path + "//" + "robot_Main.mod"))
                            {
                                sw.Write(Template.GetABBHidriaMainFunc(Program));
                            }

                            using (StreamWriter sw = File.CreateText(path + "//" + "OtherFucntions.mod"))
                            {
                                sw.Write(Template.GetABBHidriaOtherFunctionFunc(Program));
                            }
                            using (StreamWriter sw = File.CreateText(path + "//" + "EIORobot.cfg"))
                            {
                                sw.Write(Template.GetABBHidriaEIORobotFunc(program));
                            }

                            using (StreamWriter sw = File.CreateText(path + "//" + "EIOSimulacija.cfg"))
                            {
                                sw.Write(Template.GetABBHidriaEIOSimulationFunc(program));
                            }
                            excell.GenerateIO("ABB", program, path);
                        }              

                        TextUpdate = "ABB program generated: " + path;
                        Process.Start("explorer.exe", path);
                    }
                }
            }
            else if (SelectedTemplate == "Yamaha Hidria")
            {

                if (path != String.Empty)
                {
                    string DateAndTime = DateTime.Now.ToString(@"dd.MM.yyyy_HH.mm.ss");
                    Directory.CreateDirectory(path + "\\" + string.Format("YamahaGeneratedTemplate_{0}", DateAndTime).ToString());
                    path = path + "\\" + string.Format("YamahaGeneratedTemplate_{0}", DateAndTime).ToString();

                    using (StreamWriter sw = File.CreateText(path + "//" + string.Format("BackupFile.all").ToString()))
                    {
                        sw.Write(Template.GetYamahaMainProg(Program));

                        foreach (ProgramModel robot in Program)
                        {
                            //Generate robot file(s)
                            excell.GenerateIO("Yamaha", robot, path);
                            //using (StreamWriter sw = File.CreateText(path + "//" + "Main.all"))
                            //{
                            sw.Write(Template.GetYamahaRobotProgHeader(robot));
                            sw.Write(Template.GetYamahaRobotProgInit(robot));
                            sw.Write(Template.GetYamahaRobotProgHoming(robot));
                            //
                            sw.Write(Template.GetYamahaRobotProgTest(robot));
                            sw.Write(Template.GetYamahaRobotProgMainTask(robot));
                            //
                            sw.Write(Template.GetYamahaRobotProgGoHome(robot));
                            //
                            for (int n = 1; n < robot.Stations.Count; n++)
                            {
                                currstation = n;
                                sw.Write(Template.GetYamahaRobotProgGoStation(robot, currstation));
                            }
                            //
                            sw.Write(Template.GetYamahaRobotProgMoveOnStation(robot)); 
                            sw.Write(Template.GetYamahaRobotProgMoveAway(robot));
                            //
                            sw.Write(Template.GetYamahaCommonProg(robot));
                            //
                            sw.Write(Template.GetYamahaPointsFile(robot)); 
                            //
                            sw.Write(Template.GetYamahaIoFile(robot));
                        }
                        sw.Write("\n" + "[END]");

                    }
                }
                TextUpdate = "Yamaha program generated: " + path;
                Process.Start("explorer.exe", path);
                // TO DO - Odpri direktori

            }
            else if (SelectedTemplate == "Kawasaki Hidria")
            {
                if (path != String.Empty)
                {
                    string DateAndTime = DateTime.Now.ToString(@"dd.MM.yyyy_HH.mm.ss");
                    Directory.CreateDirectory(path + "\\" + string.Format("KawasakiGeneratedTemplate_{0}", DateAndTime).ToString());
                    path = path + "\\" + string.Format("KawasakiGeneratedTemplate_{0}", DateAndTime).ToString();

                    //string.Format("KawasakiGeneratedProgram.as", DateAndTime).ToString();
                    using (StreamWriter sw = File.CreateText(path + "//" + string.Format("ProgramFile.as").ToString()))
                    {
                        //sw.Write(Template.GetKawasakiDocumentation(Program));  //unnecessary
                        //sw.Write(Template.GetYamahaMainProg(Program));

                        foreach (ProgramModel robot in Program)
                        {
                            excell.GenerateIO("Kawasaki", robot, path);
                            //Generate robot file(s)
                            sw.Write(Template.GetKawasakiTesting(robot));
                            //
                            sw.Write(Template.GetKawasakiMain(robot));
                            sw.Write(Template.GetKawasakiInit(robot));
                            sw.Write(Template.GetKawasakiHoming(robot));
                            sw.Write(Template.GetKawasakiMainTask(robot)); 
                            //
                            sw.Write(Template.GetKawasakiMoveAway(robot));
                            sw.Write(Template.GetKawasakiMoveOnStation(robot));
                            sw.Write(Template.GetKawasakiGoHome(robot));
                            for (int n = 1; n < robot.Stations.Count; n++)
                            {
                                currstation = n;
                                sw.Write(Template.GetKawasakiGoStation(robot, currstation));
                            }
                            //
                            sw.Write(Template.GetKawasakiFunctions(robot));
                            sw.Write(Template.GetKawasakiCalibration(robot));
                            //
                            sw.Write(Template.GetKawasakiPoints(robot));
                            //
                            sw.Write(Template.GetKawasakiIO(robot));
                            sw.Write(Template.GetKawasakiIOhandling(robot));
                            //
                            sw.Write(Template.GetKawasakiComment(robot));

                        }

                    }
                }
                TextUpdate = "Kawasaki program generated: " + path;
                Process.Start("explorer.exe", path);
                // TO DO - Odpri direktori
            }

            else
            {
                TextUpdate = "select a template";
            }
        } 
        #endregion

        //public void testTask (string s)

        //{
        //    TextUpdate = s;
        //}
    }
   
}

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


namespace TemplateGenerator.Core.ViewModels
{
    public class ShellViewModel : MvxViewModel
    {
        private int i;
        private string _textUpdate = null;
        private string _stationName;
        private string _stationFullName;
        private bool _stationFreeEnabled;
        private string _selectedTemplate;
        private string _programName;
        private bool _generateSimpleProgram = true;
        private ProgramModel _selectedProgram;
        private StationModel _selectedStation;
        public ObservableCollection<ProgramModel> Program { get; set; } = new ObservableCollection<ProgramModel>();
        GenerateExcelIO excell = new GenerateExcelIO();

        // Za vsak uvožen program hrani število postaj ob zadnjem uvozu/posodobitvi -
        // UpdateProject nato ve, katere postaje na koncu seznama so nove in jih je treba vstaviti.
        private readonly Dictionary<ProgramModel, int> _stationBaseline = new Dictionary<ProgramModel, int>();

        // Proizvajalec zadnjega uspešnega uvoza ("Epson Hidria" / "KUKA Hella" / "ABB Hidria") -
        // UpdateProject ga potrebuje, da ve, kateri Updater naj pokliče.
        private string _importedVendor;

        public static List<string> Templates { get; set; }

        public ShellViewModel()
        {
            AddProgramCommand = new MvxCommand(AddProgram);
            RemoveProgramCommand = new MvxCommand(RemoveProgram);
            AddStationCommand = new MvxCommand(AddStation);
            TestButtonCommand = new MvxCommand(TestButton);

            Templates = new List<string>()
            {
                "Epson Hidria",
                "KUKA Hella",
                "ABB Hidria",
                "ABB Simulacija"
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
            if (StationName.All(char.IsLetterOrDigit))  //samo črke in številke so dovoljene
            {
                if (SelectedProgram.Stations.Contains(new StationModel(StationName)))
                {
                    //postaja že obstaja v zbranem robotu
                    TextUpdate = $"Station {StationName} already exist in {SelectedProgram.ProgramName}";
                    StationName = "";
                }
                else
                {
                    SelectedProgram.AddStation(StationName, StationFreeEnabled);
                    TextUpdate = $"Station {StationName} added to { SelectedProgram.ProgramName}.";
                    StationName = "";
                }
            }
            else
            {
                TextUpdate = "Only alphanumeric chars are allowed";
            }
        }

        public IMvxCommand TestButtonCommand { get; set; }
        public void TestButton()
        {

        }

        #region // GENERATE TEMPLATE //
        //public IMvxCommand GenerateProjectCommand { get; set; }
        public void GenerateProject(string s)
        {
            string path = s;

            if (SelectedTemplate == "Epson Hidria")
            {

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
                            sw.Write(Template.GetHomingFunc(robot));
                            sw.Write(Template.GetOperationFunc(robot));
                            if (GenerateSimpleProgram)
                            {
                                sw.Write(Template.GetMovementsSimpleProgramFunc(robot));
                                sw.Write(Template.GetMoveOnStationSimpleProgramFunc(robot));
                                sw.Write(Template.GetMoveAwaySimpleProgramFunc(robot));
                                sw.Write(Template.GetGoOnAdditionalPositionsFunc(robot));
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
            else
            {
                TextUpdate = "select a template";
            }
        }

        // Prebere obstoječi, s tem orodjem že generiran projekt (Epson Hidria / KUKA Hella / ABB
        // Hidria - proizvajalec se zazna samodejno iz strukture izbrane mape) nazaj v model
        // (Program), da lahko uporabnik nadaljuje delo (npr. doda novo postajo).
        public void ImportProject(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                ObservableCollection<ProgramModel> imported;
                string vendor;

                if (Directory.GetFiles(path, "*_Motion.mod").Length > 0)
                {
                    imported = AbbHidriaProjectImporter.Import(path);
                    vendor = "ABB Hidria";
                }
                else if (Directory.Exists(Path.Combine(path, "R1", "Program")))
                {
                    imported = KukaHellaProjectImporter.Import(path);
                    vendor = "KUKA Hella";
                }
                else if (Directory.GetFiles(path, "*.prg").Length > 0)
                {
                    imported = EpsonProjectImporter.Import(path);
                    vendor = "Epson Hidria";
                }
                else
                {
                    TextUpdate = "V izbrani mapi ni bilo mogoče zaznati Epson/KUKA Hella/ABB Hidria projekta.";
                    return;
                }

                Program.Clear();
                _stationBaseline.Clear();
                foreach (var program in imported)
                {
                    Program.Add(program);
                    _stationBaseline[program] = program.Stations.Count;
                }

                i = Program.Count;
                _importedVendor = vendor;
                SelectedTemplate = vendor;
                TextUpdate = $"Uvoženih programov ({vendor}): {Program.Count}.";
            }
            catch (Exception ex)
            {
                TextUpdate = "Napaka pri uvozu: " + ex.Message;
            }
        }

        // Kirurško vstavi na novo dodane postaje (dodane po zadnjem uvozu/posodobitvi) v obstoječe
        // generirane datoteke na disku, brez polnega prepisa. Pred urejanjem naredi varnostno kopijo mape.
        public void UpdateProject(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            if (_importedVendor == null)
            {
                TextUpdate = "Najprej uvozite projekt (Import Project), šele nato ga lahko posodobite.";
                return;
            }

            try
            {
                var programsToUpdate = Program
                    .Where(p => _stationBaseline.ContainsKey(p) && p.Stations.Count > _stationBaseline[p])
                    .ToList();

                if (programsToUpdate.Count == 0)
                {
                    TextUpdate = "Ni novih postaj za posodobitev (najprej uvozite projekt in dodajte novo postajo).";
                    return;
                }

                string backupPath;
                switch (_importedVendor)
                {
                    case "Epson Hidria": backupPath = EpsonProjectUpdater.BackupFolder(path); break;
                    case "KUKA Hella": backupPath = KukaHellaProjectUpdater.BackupFolder(path); break;
                    case "ABB Hidria": backupPath = AbbHidriaProjectUpdater.BackupFolder(path); break;
                    default: throw new InvalidOperationException($"Neznan proizvajalec '{_importedVendor}'.");
                }

                foreach (var program in programsToUpdate)
                {
                    int baseline = _stationBaseline[program];
                    var newStations = program.Stations.Skip(baseline).ToList();

                    switch (_importedVendor)
                    {
                        case "Epson Hidria": EpsonProjectUpdater.UpdateProgram(program, newStations, path); break;
                        case "KUKA Hella": KukaHellaProjectUpdater.UpdateProgram(program, newStations, path); break;
                        case "ABB Hidria": AbbHidriaProjectUpdater.UpdateProgram(program, newStations, path); break;
                    }

                    _stationBaseline[program] = program.Stations.Count;
                }

                TextUpdate = $"Projekt posodobljen ({programsToUpdate.Count} program(ov)). Varnostna kopija: {backupPath}";
                Process.Start("explorer.exe", path);
            }
            catch (Exception ex)
            {
                TextUpdate = "Napaka pri posodabljanju: " + ex.Message;
            }
        }
        #endregion

        //public void testTask (string s)

        //{
        //    TextUpdate = s;
        //}
    }
   
}

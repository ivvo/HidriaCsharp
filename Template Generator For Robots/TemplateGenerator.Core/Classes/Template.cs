using TemplateGenerator.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Linq;

namespace TemplateGenerator.Core.Classes
{
    class Template
    {
        public static bool FormatsLoaded = false;
        public static string LoadError = null;
        //EPSON//
        private static readonly string EpsonHeaderFormatEng = "";
        private static readonly string EpsonMainFuncFormatEng = "";
        private static readonly string EpsonErrorHandlingFuncFormatEng = "";
        private static readonly string EpsonEstopHandlingFuncFormatEng = "";
        private static readonly string EpsonInitFuncFormatEng = "";
        private static readonly string EpsonHomingFuncFormatEng = "";
        private static readonly string EpsonOperationFuncFormatEng = "";
        private static readonly string EpsonMovementFuncFormatEng = "";
        private static readonly string EpsonMoveOnStationFuncFormatEng = "";
        private static readonly string EpsonMoveAwayStationFuncFormatEng = "";
        private static readonly string EpsonPowerModeFuncFormatEng = "";
        private static readonly string EpsonFullPowerModeFuncFormatEng = "";
        private static readonly string EpsonSlowPowerModeFuncFormatEng = "";
        private static readonly string EpsonCpBarrierFuncFormatEng = "";
        private static readonly string EpsonStationsFreeFuncFormatEng = "";
        private static readonly string EpsonStationsBusyFuncFormatEng = "";
        private static readonly string EpsonResetDepartLocationsFuncFormatEng = "";
        private static readonly string EpsonResetDestLocationsFuncFormatEng = "";
        private static readonly string EpsonResetProfibusOutputsFuncFormatEng = "";
        private static readonly string EpsonResetFlagsFuncFormatEng = "";
        private static readonly string EpsonIOLablesFuncFormatEng = "";
        private static readonly string EpsonIOGenerateOutputFreeFormat = "";
        private static readonly string EpsonIOGenerateMemoryToFormat = "";
        private static readonly string EpsonIOGenerateMemoryFromFormat = "";
        private static readonly string EpsonGeneratePoints = "";
        private static readonly string EpsonUserError = "";


        //KUKA Hella//
        private static readonly string KukaHellaRobotMainFunctFromat = "";
        private static readonly string KukaHellaRobotMainDatFormat = "";
        private static readonly string KukaHellaRobotProgInitFromat = "";
        private static readonly string KukaHellaProgHomingFromat = "";
        private static readonly string KukaHellaProgHomingFromatSimpleProgram = "";
        private static readonly string KukaHellaProgMainTaskFromat = "";
        private static readonly string KukaHellaMoveAwayFormat = "";
        private static readonly string KukaHellaMoveOnStationFormat = "";
        private static readonly string KukaHellaStandardFunctionsFromat = "";
        private static readonly string KukaHellaPalletFunctionFormat = "";
        private static readonly string KukaHellaDistFunctionFormat = "";
        private static readonly string KukaHellaFunctionsDat = "";
        private static readonly string KukaHellaCellFormat = "";
        private static readonly string KukaHellaSpsFormat = "";
        private static readonly string KukaHellaConfigDatFormat = "";
        private static readonly string KukaHellaMotionDatFormat = "";

        //ABB Simulacija //
        private static readonly string ABBSimulaciaGlobal = "";
        private static readonly string ABBSimulaciaModule1 = "";

        //ABB Hidria //
        private static readonly string ABBHidriaModule1FuncFormat = "";
        private static readonly string ABBHidriaCommunicationFuncFormat = "";
        private static readonly string ABBHidriaGlobalFuncFormat = "";
        private static readonly string ABBHidriaMotionFuncFormat = "";
        private static readonly string ABBHidriaMainFuncFormat = "";
        private static readonly string ABBHidriaOtherFuntionsFormat = "";
        public static readonly string ABBHidriaNavodilaFunc = "";
        public static readonly string ABBHidriaSYSFunc = "";
        private static readonly string ABBHidriaEIORobotFunc = "";
        private static readonly string ABBHidriaEIOSimulationFunc = "";

        //Yamaha//
        private static readonly string YamahaMainPgm = "";
        private static readonly string YamahaRobotPgmHeader = "";
        private static readonly string YamahaHoming = "";
        private static readonly string YamahaGoFunction = "";
        private static readonly string YamahaMainTask = "";
        private static readonly string YamahaMoveOnStation = "";
        private static readonly string YamahaMoveAway = "";
        private static readonly string YamahaCommonPgm = "";
        private static readonly string YamahaPoints = "";
        private static readonly string YamahaIO = "";

        static Template()
        {
            // Read all formats from files
            try
            {
                // Formats loaded
                FormatsLoaded = true;

                //EPSON//
                EpsonMainFuncFormatEng = File.ReadAllText("./Templates/Epson/MainFuncEng.txt");
                EpsonErrorHandlingFuncFormatEng = File.ReadAllText("./Templates/Epson/ErrorFuncEng.txt");
                EpsonEstopHandlingFuncFormatEng = File.ReadAllText("./Templates/Epson/EstopFuncEng.txt");
                EpsonHeaderFormatEng = File.ReadAllText("./Templates/Epson/HeaderEng.txt");
                EpsonInitFuncFormatEng = File.ReadAllText("./Templates/Epson/InitFuncEng.txt");
                EpsonHomingFuncFormatEng = File.ReadAllText("./Templates/Epson/HomingFuncEng.txt");
                EpsonOperationFuncFormatEng = File.ReadAllText("./Templates/Epson/OperationFuncEng.txt");
                EpsonMovementFuncFormatEng = File.ReadAllText("./Templates/Epson/MovementFuncEng.txt");
                EpsonMoveOnStationFuncFormatEng = File.ReadAllText("./Templates/Epson/MoveOnStationFuncEng.txt");
                EpsonMoveAwayStationFuncFormatEng = File.ReadAllText("./Templates/Epson/MoveAwayFuncEng.txt");
                EpsonPowerModeFuncFormatEng = File.ReadAllText("./Templates/Epson/PowerModeFuncEng.txt");
                EpsonFullPowerModeFuncFormatEng = File.ReadAllText("./Templates/Epson/FullPowerModeFuncEng.txt");
                EpsonSlowPowerModeFuncFormatEng = File.ReadAllText("./Templates/Epson/SlowPowerModeFuncEng.txt");
                EpsonCpBarrierFuncFormatEng = File.ReadAllText("./Templates/Epson/CpBarrierFuncEng.txt");
                EpsonStationsFreeFuncFormatEng = File.ReadAllText("./Templates/Epson/StationsFreeFuncEng.txt");
                EpsonStationsBusyFuncFormatEng = File.ReadAllText("./Templates/Epson/StationsBusyFuncEng.txt");
                EpsonResetDepartLocationsFuncFormatEng = File.ReadAllText("./Templates/Epson/ResetDepartLocationsFuncEng.txt");
                EpsonResetDestLocationsFuncFormatEng = File.ReadAllText("./Templates/Epson/ResetDestLocationsFuncEng.txt");
                EpsonResetProfibusOutputsFuncFormatEng = File.ReadAllText("./Templates/Epson/ResetProfibusOutputsFuncEng.txt");
                EpsonResetFlagsFuncFormatEng = File.ReadAllText("./Templates/Epson/ResetFlagsEng.txt");
                EpsonIOLablesFuncFormatEng = File.ReadAllText("./Templates/Epson/IOLablesFunc.txt");
                EpsonIOGenerateOutputFreeFormat = File.ReadAllText("./Templates/Epson/IOGenerateOutputFree.txt");
                EpsonIOGenerateMemoryToFormat = File.ReadAllText("./Templates/Epson/IOGenerateMemoryTo.txt");
                EpsonIOGenerateMemoryFromFormat = File.ReadAllText("./Templates/Epson/IOGenerateMemoryFrom.txt");
                EpsonGeneratePoints = File.ReadAllText("./Templates/Epson/GeneratePoints.txt");
                EpsonUserError = File.ReadAllText("./Templates/Epson/UserError.txt");

                //KUKA HELLA//
                KukaHellaRobotMainFunctFromat = File.ReadAllText("./Templates/KUKA Hella/KUKARobotMain.txt");
                KukaHellaRobotMainDatFormat = File.ReadAllText("./Templates/KUKA Hella/KUKARobotMainDat.txt");
                KukaHellaRobotProgInitFromat = File.ReadAllText("./Templates/KUKA Hella/KUKAProgInit.txt");
                KukaHellaProgHomingFromat = File.ReadAllText("./Templates/KUKA Hella/KUKAProgHoming.txt");
                KukaHellaProgHomingFromatSimpleProgram = File.ReadAllText("./Templates/KUKA Hella/KUKAProgHomingSimpleProgram.txt");
                KukaHellaProgMainTaskFromat = File.ReadAllText("./Templates/KUKA Hella/KUKAMainTask.txt");
                KukaHellaStandardFunctionsFromat = File.ReadAllText("./Templates/KUKA Hella/Functions/KUKAStandardFunctions.txt");
                KukaHellaPalletFunctionFormat = File.ReadAllText("./Templates/KUKA Hella/Functions/KUKAPalletFunction.txt");
                KukaHellaDistFunctionFormat = File.ReadAllText("./Templates/KUKA Hella/Functions/KUKADistFunction.txt");
                KukaHellaFunctionsDat = File.ReadAllText("./Templates/KUKA Hella/Functions/KUKAFunctionsDat.txt");
                KukaHellaCellFormat = File.ReadAllText("./Templates/KUKA Hella/KUKACell.txt");
                KukaHellaSpsFormat = File.ReadAllText("./Templates/KUKA Hella/KUKASpsSub.txt");
                KukaHellaConfigDatFormat = File.ReadAllText("./Templates/KUKA Hella/KUKAConfigDat.txt");
                KukaHellaMoveAwayFormat = File.ReadAllText("./Templates/KUKA Hella/KUKAMoveAway.txt");
                KukaHellaMoveOnStationFormat = File.ReadAllText("./Templates/KUKA Hella/KUKAMoveOnStation.txt");
                KukaHellaMotionDatFormat = File.ReadAllText("./Templates/KUKA Hella/KUKAMotionDat.txt");

                // ABB Simulacija //
                ABBSimulaciaGlobal = File.ReadAllText("./Templates/ABB Simulacija/Global.txt");
                ABBSimulaciaModule1 = File.ReadAllText("./Templates/ABB Simulacija/Module1.txt");

                // ABB Hidria //
                ABBHidriaModule1FuncFormat = File.ReadAllText("./Templates/ABB Hidria/Module1.txt");
                ABBHidriaCommunicationFuncFormat = File.ReadAllText("./Templates/ABB Hidria/Communication.txt");
                ABBHidriaGlobalFuncFormat = File.ReadAllText("./Templates/ABB Hidria/Global.txt");
                ABBHidriaMotionFuncFormat = File.ReadAllText("./Templates/ABB Hidria/Motion.txt");
                ABBHidriaMainFuncFormat = File.ReadAllText("./Templates/ABB Hidria/main.txt");
                ABBHidriaOtherFuntionsFormat = File.ReadAllText("./Templates/ABB Hidria/OtherFunctions.txt");
                ABBHidriaNavodilaFunc = File.ReadAllText("./Templates/ABB Hidria/navodila.txt");
                ABBHidriaSYSFunc = File.ReadAllText("./Templates/ABB Hidria/SYS.txt");
                ABBHidriaEIORobotFunc = File.ReadAllText("./Templates/ABB Hidria/EIOrobot.txt");
                ABBHidriaEIOSimulationFunc = File.ReadAllText("./Templates/ABB Hidria/EIOsimulation.txt");

                //Yamaha//
                YamahaMainPgm = File.ReadAllText("./Templates/Yamaha/MainPgm.txt");
                YamahaRobotPgmHeader = File.ReadAllText("./Templates/Yamaha/RobotPgmHeader.txt");
                YamahaHoming = File.ReadAllText("./Templates/Yamaha/Homing.txt");
                YamahaGoFunction = File.ReadAllText("./Templates/Yamaha/GoFunction.txt");
                YamahaMainTask = File.ReadAllText("./Templates/Yamaha/MainTask.txt");
                YamahaMoveOnStation = File.ReadAllText("./Templates/Yamaha/MoveOnStation.txt");
                YamahaMoveAway = File.ReadAllText("./Templates/Yamaha/MoveAway.txt");
                YamahaCommonPgm = File.ReadAllText("./Templates/Yamaha/CommonPgm.txt");
                YamahaPoints = File.ReadAllText("./Templates/Yamaha/Points.txt");
                YamahaIO = File.ReadAllText("./Templates/Yamaha/IO.txt");
            }
            catch (Exception ex)
            {
                // Formats not loaded
                FormatsLoaded = false;
                LoadError = ex.Message;
            }
        }

        #region ////// ABB Hidria //////
        public static string GetABBHidriaModule1Func(ObservableCollection<ProgramModel> program)
        {
            string Format = ABBHidriaModule1FuncFormat;

            return string.Format(Format);
        }
        public static string GetABBHidriaCommunicationFunc(ObservableCollection<ProgramModel> type)
        {
            string Format = ABBHidriaCommunicationFuncFormat;
            StringBuilder StationsFree = new StringBuilder();
            StringBuilder StationsBusy = new StringBuilder();
            StringBuilder ResetDepartLocations = new StringBuilder();
            StringBuilder RessetDestLocations = new StringBuilder();

            for (int j = 0; j < type.Count; j++)
            { 
                for (int i = 0; i < type[j].Stations.Count; i++)
                {
                    if (type[j].Stations[i].StationFreeEnabled == true)
                    {
                        StationsFree.AppendFormat($"\t\tSet do{type[j].Stations[i].RobotStationName}Free;\n");
                        StationsBusy.AppendFormat($"\t\tReset do{type[j].Stations[i].RobotStationName}Free;\n");
                    }
                    ResetDepartLocations.AppendFormat($"\t\tbRobotFrom{type[j].Stations[i].RobotStationName} := FALSE;\n");
                    RessetDestLocations.AppendFormat($"\t\tbRobotTo{type[j].Stations[i].RobotStationName} := FALSE;\n");
            }
            }
            return string.Format(Format, StationsFree, StationsBusy, ResetDepartLocations, RessetDestLocations).ToString();
        }
        public static string GetABBHidriaGlobalFunc(ObservableCollection<ProgramModel> type)
        {
            string Format = ABBHidriaGlobalFuncFormat;
            StringBuilder RobotFromLoc = new StringBuilder();
            StringBuilder RobotToLoc = new StringBuilder();
            StringBuilder RobTargets = new StringBuilder();
            StringBuilder PalletNumber = new StringBuilder();
            int pallet;
            pallet = 0;
            int t;
            t = 0;
            string[] stringArray= {""};
            foreach (ProgramModel program in type)
            { 
                for (int i = 0; i < program.Stations.Count; i++)
                {
                    if (!stringArray.Contains(type[t].Stations[i].RobotStationName))
                    {
                        RobotFromLoc.AppendFormat($"\tVAR bool bRobotFrom{type[t].Stations[i].RobotStationName} ;\n");
                        RobotToLoc.AppendFormat($"\tVAR bool bRobotTo{type[t].Stations[i].RobotStationName} ;\n");
                        Array.Resize(ref stringArray, stringArray.Length + 1);
                        stringArray[stringArray.Length - 1] = type[t].Stations[i].RobotStationName;
                    }


                    if (type.Count > 1)
                    { 
                        if (type[t].Stations[i].Pallet)
                        {
                            pallet++;
                            //Define pallet variables
                            PalletNumber.AppendFormat($"\tVAR num n{type[t].ProgramName}_{type[t].Stations[i].RobotStationName} :=  {pallet};\n");

                            //Joint targets
                            RobTargets.AppendFormat($"\tCONST robtarget j{type[t].ProgramName}_{type[t].Stations[i].RobotStationName}1 :=[[0,0,0],[0.0,0.0,0.0,0.0],[0,0,1,0],[9E+09,9E+09,9E+09,9E+09,9E+09,9E+09]];\n");
                            RobTargets.AppendFormat($"\tCONST robtarget j{type[t].ProgramName}_{type[t].Stations[i].RobotStationName}2 :=[[0,0,0],[0.0,0.0,0.0,0.0],[0,0,1,0],[9E+09,9E+09,9E+09,9E+09,9E+09,9E+09]];\n");
                            RobTargets.AppendFormat($"\tCONST robtarget j{type[t].ProgramName}_{type[t].Stations[i].RobotStationName}3 :=[[0,0,0],[0.0,0.0,0.0,0.0],[0,0,1,0],[9E+09,9E+09,9E+09,9E+09,9E+09,9E+09]];\n");
                        }
                        else if (1 != program.Stations[i].Positions)
                        {
                            for (int j = 1; j < type[t].Stations[i].Positions + 1; j++)
                            {
                                RobTargets.AppendFormat($"\tCONST robtarget j{type[t].ProgramName}_{type[t].Stations[i].RobotStationName}{j} :=[[0,0,0],[0.0,0.0,0.0,0.0],[0,0,1,0],[9E+09,9E+09,9E+09,9E+09,9E+09,9E+09]];\n");
                            }
                        }
                        else
                        {
                            RobTargets.AppendFormat($"\tCONST robtarget j{type[t].ProgramName}_{type[t].Stations[i].RobotStationName} :=[[0,0,0],[0.0,0.0,0.0,0.0],[0,0,1,0],[9E+09,9E+09,9E+09,9E+09,9E+09,9E+09]];\n");
                        }
                    }
                    else
                    {
                        if (type[t].Stations[i].Pallet)
                        {
                            pallet++;
                            //Define pallet variables
                            PalletNumber.AppendFormat($"\tVAR num n{type[t].Stations[i].RobotStationName} :=  {pallet};\n");

                            //Joint targets
                            RobTargets.AppendFormat($"\tCONST robtarget j{type[t].Stations[i].RobotStationName}1 :=[[0,0,0],[0.0,0.0,0.0,0.0],[0,0,1,0],[9E+09,9E+09,9E+09,9E+09,9E+09,9E+09]];\n");
                            RobTargets.AppendFormat($"\tCONST robtarget j{type[t].Stations[i].RobotStationName}2 :=[[0,0,0],[0.0,0.0,0.0,0.0],[0,0,1,0],[9E+09,9E+09,9E+09,9E+09,9E+09,9E+09]];\n");
                            RobTargets.AppendFormat($"\tCONST robtarget j{type[t].Stations[i].RobotStationName}3 :=[[0,0,0],[0.0,0.0,0.0,0.0],[0,0,1,0],[9E+09,9E+09,9E+09,9E+09,9E+09,9E+09]];\n");
                        }
                        else if (1 != program.Stations[i].Positions)
                        {
                            for (int j = 1; j < type[t].Stations[i].Positions + 1; j++)
                            {
                                RobTargets.AppendFormat($"\tCONST robtarget j{type[t].ProgramName}_{type[t].Stations[i].RobotStationName}{j} :=[[0,0,0],[0.0,0.0,0.0,0.0],[0,0,1,0],[9E+09,9E+09,9E+09,9E+09,9E+09,9E+09]];\n");
                            }
                        }
                        else
                        {
                            RobTargets.AppendFormat($"\tCONST robtarget j{type[t].ProgramName}_{type[t].Stations[i].RobotStationName} :=[[0,0,0],[0.0,0.0,0.0,0.0],[0,0,1,0],[9E+09,9E+09,9E+09,9E+09,9E+09,9E+09]];\n");
                        }
                    }
                }
                t++;
            }

            return string.Format(Format, RobotFromLoc, RobotToLoc, RobTargets, PalletNumber).ToString();
        }
        public static string GetABBHidriaMotionFunc(ProgramModel program)
        {
            string Format = ABBHidriaMotionFuncFormat;
            StringBuilder FormatArgumentDestinations = new StringBuilder();
            StringBuilder FormatArgumentStationFree = new StringBuilder();
            List<StringBuilder> FormatArgumentPoints = new List<StringBuilder>();
            StringBuilder FormatArgumentDestination = new StringBuilder();
            StringBuilder FormatargumentOnStation = new StringBuilder();
            StringBuilder FormatArgumentMoveAway = new StringBuilder();
            StringBuilder FormatArgumentAditionalPositions = new StringBuilder();

            // Generate robot_go
            for (int i = 0; i < program.Stations.Count; i++)
            {
                FormatArgumentPoints.Add(new StringBuilder());
                if (i == 0)
                {
                    FormatArgumentPoints[i].AppendFormat($"\t\t\tMoveJ j{program.Stations[i].RobotStationName},vFullSpeed,z100,tool0; \n");
                }
                else
                {
                    if (program.Stations[i].Pallet)
                    {
                        FormatArgumentPoints[i].AppendFormat("\t\t\tIF diRobotOnStation = 1 THEN\n");
                        FormatArgumentPoints[i].AppendFormat($"\t\t\t\tMoveJ Offs (jPallett,0,0,nRobotStationZOffset),vFullSpeed,z100,tool0;\n");
                        FormatArgumentPoints[i].AppendFormat("\t\t\tELSE\n");
                        FormatArgumentPoints[i].AppendFormat($"\t\t\t\tMoveJ Offs (jPallett,0,0,nRobotStationZOffset),vFullSpeed,fine,tool0;\n");
                        FormatArgumentPoints[i].AppendFormat("\t\t\tENDIF\n");
                    }
                    else if (1 != program.Stations[i].Positions)
                    {
                        //FormatArgumentPoints[i].AppendFormat("\t\t\tENDIF\n");
                        FormatArgumentPoints[i].AppendFormat($"\t\t\tMoveJ{program.Stations[i].RobotStationName}MaxZHeight nAdditionalPos;\n");
                    }
                    else
                    {
                        FormatArgumentPoints[i].AppendFormat("\t\t\tIF diRobotOnStation = 1 THEN\n");
                        FormatArgumentPoints[i].AppendFormat($"\t\t\t\tMoveJ Offs (j{program.Stations[i].RobotStationName},0,0,nRobotStationZOffset),vFullSpeed,z100,tool0;\n");
                        FormatArgumentPoints[i].AppendFormat("\t\t\tELSE\n");
                        FormatArgumentPoints[i].AppendFormat($"\t\t\t\tMoveJ Offs (j{program.Stations[i].RobotStationName},0,0,nRobotStationZOffset),vFullSpeed,fine,tool0;\n");
                        FormatArgumentPoints[i].AppendFormat("\t\t\tENDIF\n");
                    }
                }
            }

            for (int i = 0; i < program.Stations.Count; i++)
            {
                if (program.Stations[i].RobotStationName == "Home")
                {
                    FormatArgumentDestinations.AppendFormat($"If bFrom{program.Stations[i].RobotStationName} = TRUE THEN\n");
                    FormatArgumentDestinations.AppendFormat(@"{{0}}");
                    FormatArgumentDestinations.AppendFormat("\n");
                }
                else
                {
                    FormatArgumentDestinations.AppendFormat($"\t\tELSEIF bFrom{program.Stations[i].RobotStationName} = TRUE THEN\n");
                    FormatArgumentDestinations.AppendFormat(@"{{0}}");
                    FormatArgumentDestinations.AppendFormat("\n");
                }
            }

            for (int i = 0; i < program.Stations.Count; i++)
            {
                string Destinations = FormatArgumentDestinations.ToString();
                FormatArgumentDestination.AppendFormat("\tPROC robot_go{0}()\n", program.Stations[i].RobotStationName);
                FormatArgumentDestination.AppendFormat("\t\t! Set location to which the robot is going\n");
                FormatArgumentDestination.AppendFormat("\t\tbRobotTo{0} := TRUE;\n\n", program.Stations[i].RobotStationName);
                FormatArgumentDestination.AppendFormat("\t\t! Move away\n");
                FormatArgumentDestination.AppendFormat($"\t\t{program.ProgramName}_moveAway;\n\n");
                if (program.Stations[i].Pallet)
                {
                    FormatArgumentDestination.AppendFormat($"\t\tGetPalletTarget n{program.Stations[i].RobotStationName},nAdditionalPos;\n\n");
                }
                FormatArgumentDestination.AppendFormat("\t\t! Set Power mode\n");
                FormatArgumentDestination.AppendFormat("\t\trobot_setPowerMode;\n\n");
                if (program.Stations[i].StationFreeEnabled)
                {
                    FormatArgumentDestination.AppendFormat("\t\tIF diRobotOnStation = 1 THEN\n");
                    FormatArgumentDestination.AppendFormat($"\t\t\tReset do{program.Stations[i].RobotStationName}Free;\n");
                    FormatArgumentDestination.AppendFormat("\t\tENDIF\n\n");
                }
                FormatArgumentDestination.AppendFormat("\t\t", program.Stations[i].RobotStationName);
                FormatArgumentDestination.AppendFormat(Destinations, FormatArgumentPoints[i]);

                FormatArgumentDestination.AppendFormat("\t\tELSE\n");
                FormatArgumentDestination.AppendFormat(@"            error_ProgramError(""Function robot_go{0}()"");", program.Stations[i].RobotStationName);
                FormatArgumentDestination.AppendFormat("\n\n\t\tENDIF\n\n");
                FormatArgumentDestination.AppendFormat("\t\t! Go on station\n");
                FormatArgumentDestination.AppendFormat($"\t\t{program.ProgramName}_moveOnStation;\n\n");
                FormatArgumentDestination.AppendFormat("\t\t! Reset depart locations\n");
                FormatArgumentDestination.AppendFormat("\t\trobot_resetDepartLocations;\n\n");
                FormatArgumentDestination.AppendFormat("\t\t! Set the depart location as {0} (for the next move -> robot needs to know from where it is starting in the next move) and reset the destination location (depart location is now the destination location)\n", program.Stations[i].RobotStationName);
                FormatArgumentDestination.AppendFormat("\t\tbRobotFrom{0}:= TRUE;\n", program.Stations[i].RobotStationName);
                FormatArgumentDestination.AppendFormat("\t\tbRobotTo{0}:= FALSE;\n", program.Stations[i].RobotStationName);
                FormatArgumentDestination.AppendFormat("\tENDPROC\n\n");
                FormatArgumentStationFree.Clear();
            }

            // Generate robot_moveOnStation
            for (int i = 0; i < program.Stations.Count; i++)
            {
                if (i == 0)
                {
                    FormatargumentOnStation.AppendFormat("\t\t\tIF bRobotTo{0} = TRUE THEN \n", program.Stations[i].RobotStationName);
                    FormatargumentOnStation.AppendFormat("\t\t\t\tMoveL j{0}, vMoveOnStation, fine, Tool0; \n\n", program.Stations[i].RobotStationName);
                }

                else
                {
                    FormatargumentOnStation.AppendFormat("\t\t\tELSEIF bRobotTo{0} = TRUE THEN \n", program.Stations[i].RobotStationName);
                    if (program.Stations[i].StationFreeEnabled)
                    {
                        FormatargumentOnStation.AppendFormat($"\t\t\t\tReset do{program.Stations[i].RobotStationName}Free;\n");
                    }

                    if (program.Stations[i].Pallet)
                    {
                        FormatargumentOnStation.AppendFormat($"\t\t\t\tMoveJ Offs(jPallett,0,0,nRobotStationZOffset),vFullSpeed, z50, Tool0; \n");
                        FormatargumentOnStation.AppendFormat($"\t\t\t\tMoveL jPallett, vMoveOnStation, fine, Tool0; \n\n");
                    }
                    else if (1 != program.Stations[i].Positions)
                    {
                        FormatargumentOnStation.AppendFormat($"\t\t\t\tMoveJ{program.Stations[i].RobotStationName}NearStation nAdditionalPos;\n");
                        FormatargumentOnStation.AppendFormat($"\t\t\t\tMoveL{program.Stations[i].RobotStationName} nAdditionalPos;\n\n");
                    }
                    else
                    {
                        FormatargumentOnStation.AppendFormat($"\t\t\t\tMoveJ Offs(j{program.Stations[i].RobotStationName},0,0,nRobotStationZOffset),vFullSpeed, z50, Tool0; \n");
                        FormatargumentOnStation.AppendFormat($"\t\t\t\tMoveL j{program.Stations[i].RobotStationName}, vMoveOnStation, fine, Tool0; \n\n");
                    }
                }
            }

            // Generate robot_moveAway
            for (int i = 0; i < program.Stations.Count; i++)
            {
                if (i == 0)
                {
                    FormatArgumentMoveAway.AppendFormat("\t\t\tIF bRobotFrom{0} = TRUE THEN \n", program.Stations[i].RobotStationName);
                    FormatArgumentMoveAway.AppendFormat("\t\t\t\t! Do nothing \n\n");
                }

                else
                {
                    FormatArgumentMoveAway.AppendFormat("\t\t\tELSEIF bRobotFrom{0} = TRUE THEN \n", program.Stations[i].RobotStationName);

                    FormatArgumentMoveAway.AppendFormat($"\t\t\t\tAccSet 30,30\n");

                    if (program.Stations[i].StationFreeEnabled)
                    {
                        FormatArgumentMoveAway.AppendFormat($"\t\t\t\tIF nPosition = 99 THEN\n");
                        FormatArgumentMoveAway.AppendFormat($"\t\t\t\t\tMoveLDo Offs(jCurPos,0,0,nRobotStationZOffset),vMoveAway,fine,tool0,do{program.Stations[i].RobotStationName}Free,1;\n");
                        FormatArgumentMoveAway.AppendFormat($"\t\t\t\tELSE\n");
                        FormatArgumentMoveAway.AppendFormat($"\t\t\t\t\tMoveLDo Offs(jCurPos,0,0,nRobotStationZOffset),vMoveAway,z50,tool0,do{program.Stations[i].RobotStationName}Free,1;\n");
                        FormatArgumentMoveAway.AppendFormat($"\t\t\t\tENDIF\n\n");
                    }
                    else
                    {
                        FormatArgumentMoveAway.AppendFormat($"\t\t\t\tIF nPosition = 99 THEN\n");
                        FormatArgumentMoveAway.AppendFormat($"\t\t\t\t\tMoveL Offs(jCurPos,0,0,nRobotStationZOffset),vMoveAway,fine,tool0;\n");
                        FormatArgumentMoveAway.AppendFormat($"\t\t\t\tELSE\n");
                        FormatArgumentMoveAway.AppendFormat($"\t\t\t\t\tMoveL Offs(jCurPos,0,0,nRobotStationZOffset),vMoveAway,z50,tool0;\n");
                        FormatArgumentMoveAway.AppendFormat($"\t\t\t\tENDIF\n\n");
                    }
                }
            }

            // Aditional Positions //
            for (int i = 0; i < program.Stations.Count; i++)
            {
                if (1 != program.Stations[i].Positions)
                {
                    // Max Z
                    FormatArgumentAditionalPositions.AppendFormat($"\tPROC MoveJ{program.Stations[i].RobotStationName}MaxZHeight(num nAddPos)\n");
                    FormatArgumentAditionalPositions.AppendFormat("\t\tTEST nAddPos\n");

                    for (int j = 1; j < program.Stations[i].Positions + 1; j++)
                    {
                        FormatArgumentAditionalPositions.AppendFormat($"\t\tCase {j}:\n");
                        FormatArgumentAditionalPositions.AppendFormat($"\t\t\tIF diRobotOnStation = 1 THEN\n");
                        FormatArgumentAditionalPositions.AppendFormat($"\t\t\t\tMoveJ Offs (j{program.Stations[i].RobotStationName}{j},0,0,nRobotStationZOffset),vFullSpeed,z100,tool0;\n");
                        FormatArgumentAditionalPositions.AppendFormat($"\t\t\tELSE\n");
                        FormatArgumentAditionalPositions.AppendFormat($"\t\t\t\tMoveJ Offs (j{program.Stations[i].RobotStationName}{j},0,0,nRobotStationZOffset),vFullSpeed,fine,tool0;\n");
                        FormatArgumentAditionalPositions.AppendFormat($"\t\t\tENDIF\n\n");
                    }
                    FormatArgumentAditionalPositions.AppendFormat("\t\tDEFAULT:\n");
                    FormatArgumentAditionalPositions.AppendFormat(@"            error_WrongAdditionalNumber(""Function MoveJ{0}MaxZHeight "",);", program.Stations[i].RobotStationName);
                    FormatArgumentAditionalPositions.AppendFormat("\n");
                    FormatArgumentAditionalPositions.AppendFormat("\t\tENDTEST\n");
                    FormatArgumentAditionalPositions.AppendFormat("\tENDPROC\n\n");

                    // Near station
                    FormatArgumentAditionalPositions.AppendFormat($"\tPROC MoveJ{program.Stations[i].RobotStationName}NearStation(num nAddPos)\n");
                    FormatArgumentAditionalPositions.AppendFormat("\t\tTEST nAddPos\n");

                    for (int j = 1; j < program.Stations[i].Positions + 1; j++)
                    {
                        FormatArgumentAditionalPositions.AppendFormat($"\t\tCase {j}:\n");
                        FormatArgumentAditionalPositions.AppendFormat($"\t\t\tMoveJ Offs (j{program.Stations[i].RobotStationName}{j},0,0,nRobotStationSlowZOffset),vFullSpeed,z50,tool0;\n\n");

                    }
                    FormatArgumentAditionalPositions.AppendFormat("\t\tDEFAULT:\n");
                    FormatArgumentAditionalPositions.AppendFormat(@"            error_WrongAdditionalNumber(""Function MoveJ{0}NearStation "",);", program.Stations[i].RobotStationName );
                    FormatArgumentAditionalPositions.AppendFormat("\n");
                    FormatArgumentAditionalPositions.AppendFormat("\t\tENDTEST\n");
                    FormatArgumentAditionalPositions.AppendFormat("\tENDPROC\n\n");

                    // On station
                    FormatArgumentAditionalPositions.AppendFormat($"\tPROC MoveL{program.Stations[i].RobotStationName}(num nAddPos)\n");
                    FormatArgumentAditionalPositions.AppendFormat("\t\tTEST nAddPos\n");

                    for (int j = 1; j < program.Stations[i].Positions + 1; j++)
                    {
                        FormatArgumentAditionalPositions.AppendFormat($"\t\tCase {j}:\n");
                        FormatArgumentAditionalPositions.AppendFormat($"\t\t\tMoveL j{program.Stations[i].RobotStationName}{j},vMoveOnStation,fine,tool0;\n\n");
                    }
                    FormatArgumentAditionalPositions.AppendFormat("\t\tDEFAULT:\n");
                    FormatArgumentAditionalPositions.AppendFormat(@"            error_WrongAdditionalNumber(""Function MoveL{0} "",);", program.Stations[i].RobotStationName );
                    FormatArgumentAditionalPositions.AppendFormat("\n");
                    FormatArgumentAditionalPositions.AppendFormat("\t\tENDTEST\n");
                    FormatArgumentAditionalPositions.AppendFormat("\tENDPROC\n\n");
                }
            }

            return string.Format(Format, program.ProgramName, FormatArgumentDestination, FormatArgumentMoveAway, FormatargumentOnStation, FormatArgumentAditionalPositions).ToString();

        }
        public static string GetABBHidriaMainFuncaaa(ProgramModel program)
        {
            string Format = ABBHidriaMainFuncFormat;
            StringBuilder FormatArgument = new StringBuilder();
            StringBuilder FormatArgumentDefinePallet = new StringBuilder();
            int palletNum = 0;

            for (int i = 0; i < program.Stations.Count; i++)
            {
                if (program.Stations[i].Pallet)
                {
                    palletNum++;
                    FormatArgumentDefinePallet.AppendFormat($"\t\tCreatePallet n{program.Stations[i].RobotStationName} , j{program.Stations[i].RobotStationName}1, j{program.Stations[i].RobotStationName}2, j{program.Stations[i].RobotStationName}3 , 0 , 0;\n");
                }

                FormatArgument.AppendFormat($"\t\t\t\t\tCase {i}:\n");
                FormatArgument.AppendFormat($"\t\t\t\t\t\t{program.ProgramName}_go{program.Stations[i].RobotStationName};\n\n");
            }
            return string.Format(Format, program.ProgramName, FormatArgument, FormatArgumentDefinePallet).ToString();
        }

        public static string GetABBHidriaMainFunc(ObservableCollection<ProgramModel> type)
        {
            string Format = ABBHidriaMainFuncFormat;
            StringBuilder FormatArgument = new StringBuilder();
            StringBuilder FormatArgumentDefinePallet = new StringBuilder();
            StringBuilder FormatArgumentType = new StringBuilder();
            int palletNum = 0;

            //If we have more than 1 type, generatey type select TEST/CASE scenario, else generate "robot_go" scenario
            if (type.Count > 1)
            {
                FormatArgument.AppendFormat($"\t\t\t\t! Get type\n");
                FormatArgument.AppendFormat($"\t\t\t\tnType := giType;\n");
                FormatArgument.AppendFormat($"\t\t\t\tTEST nType\n");
                for (int j = 0; j < type.Count; j++)
                {
                    for (int i = 0; i < type[j].Stations.Count; i++)
                    {
                        if (type[j].Stations[i].Pallet)
                        {
                            if (type.Count > 0)
                            { 
                                palletNum++;
                                FormatArgumentDefinePallet.AppendFormat($"\t\tCreatePallet n{type[j].ProgramName}_{type[j].Stations[i].RobotStationName} , j{type[j].Stations[i].RobotStationName}1, j{type[j].Stations[i].RobotStationName}2, j{type[j].Stations[i].RobotStationName}3 , 0 , 0;\n");
                            }
                            else
                            {
                                palletNum++;
                                FormatArgumentDefinePallet.AppendFormat($"\t\tCreatePallet n{type[j].Stations[i].RobotStationName} , j{type[j].Stations[i].RobotStationName}1, j{type[j].Stations[i].RobotStationName}2, j{type[j].Stations[i].RobotStationName}3 , 0 , 0;\n");

                            }
                        }
                    }

                    FormatArgument.AppendFormat($"\t\t\t\t\tCASE {j+1}:\n");
                    FormatArgument.AppendFormat($"\t\t\t\t\t\tCASE {type[j].ProgramName} nPosition:\n\n");
                }
                FormatArgument.AppendFormat($"\t\t\t\tDEFAULT:\n");
                FormatArgument.AppendFormat(@"                     error_WrongTypeNumber(""robot_mainTask, wrong Type"");");
            }
            else
            {
                FormatArgument.AppendFormat($"\t\t\t\tTEST nPosition\n");
                for (int i = 0; i < type[0].Stations.Count; i++)
                {
                    if (type[0].Stations[i].Pallet)
                    {
                        palletNum++;
                        FormatArgumentDefinePallet.AppendFormat($"\t\tCreatePallet n{type[0].Stations[i].RobotStationName} , j{type[0].Stations[i].RobotStationName}1, j{type[0].Stations[i].RobotStationName}2, j{type[0].Stations[i].RobotStationName}3 , 0 , 0;\n");
                    }

                    FormatArgument.AppendFormat($"\t\t\t\t\tCase {i}:\n");
                    FormatArgument.AppendFormat($"\t\t\t\t\t\t{type[0].ProgramName}_go{type[0].Stations[i].RobotStationName};\n\n");
                }
                FormatArgument.AppendFormat($"\t\t\t\t\tDEFAULT:\n");
                FormatArgument.AppendFormat($"\t\t\t\t\t\trobot_moveAway;");
                FormatArgument.AppendFormat("\n");
            }

            //If we have more than 1 type, generate "type_go" PROCedures for diferent types

            if (type.Count > 1)
            {
                for (int i = 0; i < type.Count; i++)
                {
                    FormatArgumentType.AppendFormat($"\tPROC {type[i].ProgramName} (var num nPos)\n");
                    FormatArgumentType.AppendFormat($"\t\tTEST nPos\n");
                    for (int j = 0; j < type[i].Stations.Count; j++)
                    {
                        FormatArgumentType.AppendFormat($"\t\t\tCASE {j}:\n");
                        FormatArgumentType.AppendFormat($"\t\t\t\t{type[i].ProgramName}_go{type[i].Stations[j].RobotStationName};\n\n");
                    }
                    FormatArgumentType.AppendFormat($"\t\t\tDEFAULT:\n");
                    FormatArgumentType.AppendFormat($"\t\t\t\t{type[i].ProgramName}_moveAway;\n\n");
                    FormatArgumentType.AppendFormat($"\t\t\tENDTEST\n");
                    FormatArgumentType.AppendFormat($"\tENDPROC\n");
                }
            }

            return string.Format(Format, type[0].ProgramName, FormatArgument, FormatArgumentDefinePallet, FormatArgumentType).ToString();
        }
        public static string GetABBHidriaOtherFunctionFunc(ObservableCollection<ProgramModel> type)
        {
            string Format = ABBHidriaOtherFuntionsFormat;
            StringBuilder FormatArgument = new StringBuilder();
            int t;
            t = 0;

            foreach (ProgramModel program in type)
                for (int i = 0; i < program.Stations.Count; i++)
                {
                    if (program.Stations[i].Pallet || program.Stations[i].Positions != 1)
                    {
                        FormatArgument.AppendFormat($"\t\tIF nDist > Distance(jCurPos.trans,j{type[t].ProgramName}{program.Stations[i].RobotStationName}1.trans) THEN\n");
                        FormatArgument.AppendFormat($"\t\t\tnDist:= Distance(jCurPos.trans, j{type[t].ProgramName}{program.Stations[i].RobotStationName}1.trans);\n");
                    }
                    else
                    {
                        FormatArgument.AppendFormat($"\t\tIF nDist > Distance(jCurPos.trans,j{type[t].ProgramName}{program.Stations[i].RobotStationName}.trans) THEN\n");
                        FormatArgument.AppendFormat($"\t\t\tnDist:= Distance(jCurPos.trans, j{type[t].ProgramName}{program.Stations[i].RobotStationName}.trans);\n");
                    }
                
                    FormatArgument.AppendFormat($"\t\t\trobot_resetDepartLocations;\n");
                    FormatArgument.AppendFormat($"\t\t\tbRobotFrom{program.Stations[i].RobotStationName} := TRUE;\n");
                    FormatArgument.AppendFormat($"\t\t\tbRobot_onStation := TRUE;");
                    FormatArgument.AppendFormat($"\t\tENDIF\n\n");
                }

            return string.Format(Format, FormatArgument).ToString();
        }
        public static string GetABBHidriaEIORobotFunc(ProgramModel program)
        {
            string Format = ABBHidriaEIORobotFunc;
            StringBuilder FormatArgumentOutputs = new StringBuilder();
            int j;
            j = 24;

            for (int i = 0; i < program.Stations.Count; i++)
            {
                if (program.Stations[i].StationFreeEnabled)
                {
                    FormatArgumentOutputs.AppendFormat($"\t  -Name \"do{program.Stations[i].RobotStationName}free\" -SignalType \"DO\" -Device \"PN_Internal_Anybus\"\\\n");
                    FormatArgumentOutputs.AppendFormat($"\t  -DeviceMap \"{j}\"\n\n");
                    j++;
                }
            }
                return string.Format(Format, FormatArgumentOutputs).ToString();
        }
        public static string GetABBHidriaEIOSimulationFunc(ProgramModel program)
        {
            string Format = ABBHidriaEIOSimulationFunc;
            StringBuilder FormatArgumentOutputs = new StringBuilder();

            for (int i = 0; i < program.Stations.Count; i++)
            {
                if (program.Stations[i].StationFreeEnabled)
                {
                    FormatArgumentOutputs.AppendFormat($"\t  -Name \"do{program.Stations[i].RobotStationName}free\" -SignalType \"DO\"\n\n");
                }
            }
            return string.Format(Format, FormatArgumentOutputs).ToString();
        }


        #endregion


        #region ////// ABB Simulacija//////
        public static string GetABBGlobalFunc(ProgramModel program)
        {
            string Format = ABBSimulaciaGlobal;
            StringBuilder FromLocation = new StringBuilder();
            StringBuilder ToLocation = new StringBuilder();
            StringBuilder ResetFromLocation = new StringBuilder();
            StringBuilder ResetToLocation = new StringBuilder();
            StringBuilder Points = new StringBuilder();

            for (int i = 0; i < program.Stations.Count; i++)
            {
                // Define depart and destination variables
                FromLocation.AppendFormat("\tVAR bool bRobotFrom{0};\n", program.Stations[i].RobotStationName);
                ToLocation.AppendFormat("\tVAR bool bRobotTo{0};\n", program.Stations[i].RobotStationName);

                // Reset depart and destination locations
                ResetFromLocation.AppendFormat("\t\tbRobotFrom{0} := FALSE;\n", program.Stations[i].RobotStationName);
                ResetToLocation.AppendFormat("\t\tbRobotTo{0} := FALSE;\n", program.Stations[i].RobotStationName);

                //Generate points
                Points.AppendFormat("\tCONST robtarget j{0}:=[[0,0,0],[0.0,0.0,0.0,0.0],[0,0,1,0],[9E+9,9E+9,9E+9,9E+9,9E+9,9E+9]];\n", program.Stations[i].RobotStationName);
            }



            return string.Format(Format, FromLocation, ToLocation, Points, ResetFromLocation, ResetToLocation).ToString();
        }

        public static string GetABBModule1Func(ProgramModel program)
        {
            string Format = ABBSimulaciaModule1;
            StringBuilder FormatArgumentDestinations = new StringBuilder();
            StringBuilder FormatArgumentStationFree = new StringBuilder();
            List<StringBuilder> FormatArgumentPoints = new List<StringBuilder>();
            StringBuilder FormatArgumentDestination = new StringBuilder();
            StringBuilder FormatargumentOnStation = new StringBuilder();
            StringBuilder FormatArgumentMoveAway = new StringBuilder();

            // Generate robot_go
            for (int i = 0; i < program.Stations.Count; i++)
            {

                FormatArgumentPoints.Add(new StringBuilder());
                if (i == 0)
                    FormatArgumentPoints[i].AppendFormat("\t\t\tMoveJ j{0},vFullSpeed,z100,tool0; \n", program.Stations[i].RobotStationName);
                else
                    FormatArgumentPoints[i].AppendFormat("\t\t\tMoveJ Offs (j{0},0,0,nRobotStationZOffset),vFullSpeed,z100,tool0; \n", program.Stations[i].RobotStationName);

            }

            for (int i = 0; i < program.Stations.Count; i++)
            {
                if (program.Stations[i].RobotStationName == "Home")
                {
                    FormatArgumentDestinations.AppendFormat("If b{0}From{1} = TRUE THEN\n", program.ProgramName, program.Stations[i].RobotStationName);
                    FormatArgumentDestinations.AppendFormat(@"{{0}}");
                    FormatArgumentDestinations.AppendFormat("\n");
                }

                else
                {
                    FormatArgumentDestinations.AppendFormat("\t\tELSEIF b{0}From{1} = TRUE THEN\n", program.ProgramName, program.Stations[i].RobotStationName);
                    FormatArgumentDestinations.AppendFormat(@"{{0}}");
                    FormatArgumentDestinations.AppendFormat("\n");
                }
            }

            for (int i = 0; i < program.Stations.Count; i++)
            {
                string Destinations = FormatArgumentDestinations.ToString();
                FormatArgumentDestination.AppendFormat("\tPROC robot_go{0}()\n", program.Stations[i].RobotStationName);
                FormatArgumentDestination.AppendFormat("\t\t! Set location to which the robot is going\n");
                FormatArgumentDestination.AppendFormat("\t\tbRobotTo{0} := TRUE;\n\n", program.Stations[i].RobotStationName);
                FormatArgumentDestination.AppendFormat("\t\t! Move away\n");
                FormatArgumentDestination.AppendFormat($"\t\t{program.ProgramName}_moveAway;\n\n");
                FormatArgumentDestination.AppendFormat("\t\t", program.Stations[i].RobotStationName);
                FormatArgumentDestination.AppendFormat(Destinations, FormatArgumentPoints[i]);
                FormatArgumentDestination.AppendFormat("\t\tENDIF\n\n");
                FormatArgumentDestination.AppendFormat("\t\t! Go on station\n");
                FormatArgumentDestination.AppendFormat($"\t\t{program.ProgramName}_moveOnStation;\n\n");
                FormatArgumentDestination.AppendFormat("\t\t! Reset depart locations\n");
                FormatArgumentDestination.AppendFormat("\t\trobot_resetDepartLocations;\n\n");
                FormatArgumentDestination.AppendFormat("\t\t! Set the depart location as {0} (for the next move -> robot needs to know from where it is starting in the next move) and reset the destination location (depart location is now the destination location)\n", program.Stations[i].RobotStationName);
                FormatArgumentDestination.AppendFormat("\t\tbRobotFrom{0}:= TRUE;\n", program.Stations[i].RobotStationName);
                FormatArgumentDestination.AppendFormat("\t\tbRobotTo{0}:= FALSE;\n", program.Stations[i].RobotStationName);
                FormatArgumentDestination.AppendFormat("\tENDPROC\n\n");
                FormatArgumentStationFree.Clear();
            }

            // Generate robot_moveOnStation
            for (int i = 0; i < program.Stations.Count; i++)
            {   
                if (i == 0)
                { 
                    FormatargumentOnStation.AppendFormat("\t\tIF bRobotTo{0} = TRUE THEN \n", program.Stations[i].RobotStationName);
                    FormatargumentOnStation.AppendFormat("\t\t\tMoveJ Offs(j{0},0,0,nRobotStationZOffset),vFullSpeed, z50, Tool0; \n\n", program.Stations[i].RobotStationName);
                    FormatargumentOnStation.AppendFormat("\t\t\tMoveL j{0}, vMoveOnStation, fine, Tool0; \n\n", program.Stations[i].RobotStationName);
                }

                else
                { 
                    FormatargumentOnStation.AppendFormat("\t\tELSEIF bRobotTo{0} = TRUE THEN \n", program.Stations[i].RobotStationName);
                    FormatargumentOnStation.AppendFormat("\t\t\tMoveL j{0}, vMoveOnStation, fine, Tool0; \n\n", program.Stations[i].RobotStationName);
                }
            }
            FormatargumentOnStation.AppendFormat("\t\tENDIF\n\n");

            // Generate robot_moveAway
            for (int i = 0; i < program.Stations.Count; i++)
            {
                if (i == 0)
                {
                    FormatArgumentMoveAway.AppendFormat("\t\tIF bRobotFrom{0} = TRUE THEN \n", program.Stations[i].RobotStationName);
                    FormatArgumentMoveAway.AppendFormat("\t\t\t! Do nothing \n\n");
                }

                else
                {
                    FormatArgumentMoveAway.AppendFormat("\t\tELSEIF bRobotFrom{0} = TRUE THEN \n", program.Stations[i].RobotStationName);
                    FormatArgumentMoveAway.AppendFormat("\t\t\tMoveL Offs(jCurPos,0,0,nRobotStationZOffset),vMoveAway,z50,tool0; \n\n");
                }
            }
            FormatArgumentMoveAway.AppendFormat("\t\tENDIF\n\n");

            return string.Format(Format, FormatArgumentDestination, FormatargumentOnStation, FormatArgumentMoveAway).ToString();

        }
        #endregion


        #region ////// KUKA Hella //////
        // Generate Main.src

        public static string GetKukaHellaRobotMainFunctFromat(ObservableCollection<ProgramModel> program)
        {
            StringBuilder FormatArgument = new StringBuilder();

            string Fromat = KukaHellaRobotMainFunctFromat;
            // Go through all programs
            for (int i = 0; i < program.Count; i++)
            {
                if (i == program.Count - 1)
                    FormatArgument.AppendFormat("\t{0}_init ()", program[i].ProgramName.ToLower());
                else
                    FormatArgument.AppendFormat("\t{0}_init ()\n", program[i].ProgramName.ToLower());
            }
            return string.Format(Fromat, FormatArgument).ToString() + "\n\n";
        }

        // Generate programMain.src
        public static string GetKukaHellaRobotProgInitFromat(ProgramModel program)
        {
            StringBuilder FormatArgument = new StringBuilder();

            string Fromat = KukaHellaRobotProgInitFromat;

            FormatArgument.AppendFormat("{0}_main", program.ProgramName.ToLower());


            return string.Format(Fromat, FormatArgument).ToString() + "\n\n";
        }

        public static string GetKukaHellaProgHomingFromat(ProgramModel program)
        {
            StringBuilder FormatArgument = new StringBuilder();

            string Fromat = KukaHellaProgHomingFromat;

            FormatArgument.AppendFormat("{0}", program.ProgramName.ToLower());

            return string.Format(Fromat, FormatArgument).ToString() + "\n";
        }

        public static string GetKukaHellaProgHomingFromatSimpleProgram(ProgramModel program)
        {
            StringBuilder FormatArgument = new StringBuilder();
            StringBuilder FormatArgumentGenerateHomming = new StringBuilder();

            string Fromat = KukaHellaProgHomingFromatSimpleProgram;

            FormatArgument.AppendFormat("{0}", program.ProgramName.ToLower());

            for (int i = 0; i < program.Stations.Count; i++)
            {
                if (i == program.Stations.Count - 1)
                {
                    FormatArgumentGenerateHomming.AppendFormat("\t\t\tIF Dist(j{0},$POS_ACT) < ndistance THEN\n", program.Stations[i].RobotStationName);
                    FormatArgumentGenerateHomming.AppendFormat("\t\t\t\tndistance = Dist(j{0},$POS_ACT)\n", program.Stations[i].RobotStationName);
                    FormatArgumentGenerateHomming.AppendFormat("\t\t\t\tFROM_LOC = #FROM_{0}\n", program.Stations[i].RobotStationName.ToUpper());
                    FormatArgumentGenerateHomming.AppendFormat("\t\t\t\tbRobot_onStation = TRUE\n");
                    FormatArgumentGenerateHomming.AppendFormat("\t\t\tENDIF\n");
                }
                else if (i == 0)
                {
                    FormatArgumentGenerateHomming.AppendFormat("IF Dist(j{0},$POS_ACT) < ndistance THEN\n", program.Stations[i].RobotStationName);
                    FormatArgumentGenerateHomming.AppendFormat("\t\t\t\tndistance = Dist(j{0},$POS_ACT)\n", program.Stations[i].RobotStationName);
                    FormatArgumentGenerateHomming.AppendFormat("\t\t\t\tFROM_LOC = #FROM_{0}\n", program.Stations[i].RobotStationName.ToUpper());
                    FormatArgumentGenerateHomming.AppendFormat("\t\t\t\tbRobot_onStation = TRUE\n");
                    FormatArgumentGenerateHomming.AppendFormat("\t\t\tENDIF\n\n");
                }
                else
                {
                    FormatArgumentGenerateHomming.AppendFormat("\t\t\tIF Dist(j{0},$POS_ACT) < ndistance THEN\n", program.Stations[i].RobotStationName);
                    FormatArgumentGenerateHomming.AppendFormat("\t\t\t\tndistance = Dist(j{0},$POS_ACT)\n", program.Stations[i].RobotStationName);
                    FormatArgumentGenerateHomming.AppendFormat("\t\t\t\tFROM_LOC = #FROM_{0}\n", program.Stations[i].RobotStationName.ToUpper());
                    FormatArgumentGenerateHomming.AppendFormat("\t\t\t\tbRobot_onStation = TRUE\n");
                    FormatArgumentGenerateHomming.AppendFormat("\t\t\tENDIF\n\n");
                }
            }

            return string.Format(Fromat, FormatArgument, FormatArgumentGenerateHomming).ToString() + "\n";
        }

        public static string GetKukaHellaProgMainTaskFromat(ProgramModel program)
        {
            StringBuilder FormatArgument = new StringBuilder();
            StringBuilder FormatArgumentGenerateMain = new StringBuilder();

            string Fromat = KukaHellaProgMainTaskFromat;

            FormatArgument.AppendFormat("{0}", program.ProgramName.ToLower());

            for (int i = 0; i < program.Stations.Count; i++)
            {
                if (i == program.Stations.Count - 1)
                {
                    FormatArgumentGenerateMain.AppendFormat("\t\t\t\tCase {0}\n", (i));
                    FormatArgumentGenerateMain.AppendFormat("\t\t\t\t\trobot_go{0}()\n", program.Stations[i].RobotStationName);

                }
                else if (i == 0)
                {
                    FormatArgumentGenerateMain.AppendFormat("Case {0}\n", (i));
                    FormatArgumentGenerateMain.AppendFormat("\t\t\t\t\trobot_go{0}()\n\n", program.Stations[i].RobotStationName);
                }

                else
                {
                    FormatArgumentGenerateMain.AppendFormat("\t\t\t\tCase {0}\n", (i));
                    FormatArgumentGenerateMain.AppendFormat("\t\t\t\t\trobot_go{0}()\n\n", program.Stations[i].RobotStationName);
                }
            }
            return string.Format(Fromat, FormatArgument, FormatArgumentGenerateMain).ToString() + "\n";
        }
        // Generate programMain.dat
        public static string GetKukaHellaProgramMainDat()
        {
            StringBuilder FormatArgumentToLoc = new StringBuilder();

            string Format = KukaHellaRobotMainDatFormat;

            return string.Format(Format).ToString() + "\n\n";
        }

        // Generate programMotion.src
        public static string GetKukaHellaMotionProgramFormat(ProgramModel program)
        {
            StringBuilder FormatArgument = new StringBuilder();
            StringBuilder FormatArgumentDestinations = new StringBuilder();

            string Format = "{0}";

            FormatArgument.AppendFormat("DEF {0}_motion ()\n\n", program.ProgramName.ToLower());
            FormatArgument.AppendFormat("END\n\n");

            for (int i = 0; i < program.Stations.Count; i++)
            {
                FormatArgumentDestinations.AppendFormat("\t\tCASE #FROM_{0}\n\n", program.Stations[i].RobotStationName.ToUpper());
            }

            for (int i = 0; i < program.Stations.Count; i++)
            {
                FormatArgument.AppendFormat("DEF robot_go{0} ()\n", program.Stations[i].RobotStationName);
                FormatArgument.AppendFormat("\t; Set location to which the robot is going\n");
                FormatArgument.AppendFormat("\tTO_LOC = #TO_{0}\n\n", program.Stations[i].RobotStationName.ToUpper());
                FormatArgument.AppendFormat("\t; Move away\n");
                FormatArgument.AppendFormat("\trobot_moveAway(); \n\n");
                FormatArgument.AppendFormat("\t; Set power mode \n");
                FormatArgument.AppendFormat("\trobot_setPowerMode() \n\n");
                FormatArgument.AppendFormat("\tSWITCH FROM_LOC \n");
                FormatArgument.AppendFormat("{0}", FormatArgumentDestinations);
                FormatArgument.AppendFormat("\tDEFAULT \n");
                FormatArgument.AppendFormat("\t\tmainHalt ()\n\n");
                FormatArgument.AppendFormat("\tENDSWITCH\n\n");
                FormatArgument.AppendFormat("\t; Go on station\n");
                FormatArgument.AppendFormat($"\t{program.ProgramName}_moveOnStation()\n\n");
                FormatArgument.AppendFormat(@"  ; Set the depart location as ""{0}"" (for the next move -> robot needs to know from where it is starting in the next move) and reset the destination location (depart location is now the destination location)" + "\n", program.Stations[i].RobotStationName); ;
                FormatArgument.AppendFormat("\tFROM_LOC = #FROM_{0}\n", program.Stations[i].RobotStationName.ToUpper());
                FormatArgument.AppendFormat("\tTO_LOC = #TO_NONE\n");
                FormatArgument.AppendFormat("END\n\n");
            }

            return string.Format(Format, FormatArgument).ToString();
        }
        public static string GetKukaHellaMotionProgramFormatSimpleProgram(ProgramModel program)
        {
            StringBuilder FormatArgument = new StringBuilder();
            StringBuilder FormatArgumentDestinations = new StringBuilder();
            List<StringBuilder> FormatArgumentPoints = new List<StringBuilder>();

            string Format = "{0}";

            FormatArgument.AppendFormat("DEF {0}_motion ()\n\n", program.ProgramName.ToLower());
            FormatArgument.AppendFormat("END\n\n");

            for (int i = 0; i < program.Stations.Count; i++)
            {
                FormatArgumentPoints.Add(new StringBuilder());
                if (program.Stations[i].RobotStationName == "Home")
                { FormatArgumentPoints[i].AppendFormat("\t\t\tPTP xHome \n"); }
                else
                { FormatArgumentPoints[i].AppendFormat("\t\t\tPTP frABOVE_STATION_Z_OFFSET : j{0} C_Ptp\n", program.Stations[i].RobotStationName); }
            }

            for (int i = 0; i < program.Stations.Count; i++)
            {
                FormatArgumentDestinations.AppendFormat("\t\tCASE #FROM_{0}\n", program.Stations[i].RobotStationName.ToUpper());
                FormatArgumentDestinations.AppendFormat(@"{{0}}" + "\n");
            }

            for (int i = 0; i < program.Stations.Count; i++)
            {
                FormatArgument.AppendFormat("GLOBAL DEF robot_go{0} ()\n", program.Stations[i].RobotStationName);
                FormatArgument.AppendFormat("\t; Set location to which the robot is going\n");
                FormatArgument.AppendFormat("\tTO_LOC = #TO_{0}\n\n", program.Stations[i].RobotStationName.ToUpper());
                FormatArgument.AppendFormat("\t; Move away\n");
                FormatArgument.AppendFormat("\trobot_moveAway(); \n\n");
                FormatArgument.AppendFormat("\t; Set power mode \n");
                FormatArgument.AppendFormat("\trobot_setPowerMode() \n\n");
                FormatArgument.AppendFormat("\tSWITCH FROM_LOC \n");
                FormatArgument.AppendFormat($"{FormatArgumentDestinations}", FormatArgumentPoints[i]);
                FormatArgument.AppendFormat("\tDEFAULT \n");
                FormatArgument.AppendFormat("\t\tmainHalt ()\n\n");
                FormatArgument.AppendFormat("\tENDSWITCH\n\n");
                FormatArgument.AppendFormat("\t; Go on station\n");
                FormatArgument.AppendFormat("\trobot_moveOnStation()\n\n");
                FormatArgument.AppendFormat(@"  ; Set the depart location as ""{0}"" (for the next move -> robot needs to know from where it is starting in the next move) and reset the destination location (depart location is now the destination location)" + "\n", program.Stations[i].RobotStationName); ;
                FormatArgument.AppendFormat("\tFROM_LOC = #FROM_{0}\n", program.Stations[i].RobotStationName.ToUpper());
                FormatArgument.AppendFormat("\tTO_LOC = #TO_NONE\n");
                FormatArgument.AppendFormat("END\n\n");
            }

            return string.Format(Format, FormatArgument).ToString();
        }

        public static string GetKukaHellaMoveAwayFormat(ProgramModel program)
        {
            StringBuilder FormatArgument = new StringBuilder();

            string Format = KukaHellaMoveAwayFormat;

            for (int i = 0; i < program.Stations.Count; i++)
            {
                if (program.Stations[i].RobotStationName == "Home")
                {
                    FormatArgument.AppendFormat("\tCASE #FROM_{0}\n", program.Stations[i].RobotStationName.ToUpper());
                    FormatArgument.AppendFormat("\t\t\tPTP xHOME C_Dis\n");
                }
                else
                {
                    FormatArgument.AppendFormat("\t\tCASE #FROM_{0}\n", program.Stations[i].RobotStationName.ToUpper());
                }
            }
            return string.Format(Format, FormatArgument).ToString() + "\n";
        }
        public static string GetKukaHellaMoveAwayFormatSimpleProgram(ProgramModel program)
        {
            StringBuilder FormatArgument = new StringBuilder();

            string Format = KukaHellaMoveAwayFormat;

            for (int i = 0; i < program.Stations.Count; i++)
            {
                if (program.Stations[i].RobotStationName == "Home")
                {
                    FormatArgument.AppendFormat("\tCASE #FROM_{0}\n", program.Stations[i].RobotStationName.ToUpper());
                    FormatArgument.AppendFormat("\t\t\tPTP xHOME C_Dis\n\n");
                }
                else
                {
                    FormatArgument.AppendFormat("\t\tCASE #FROM_{0}\n", program.Stations[i].RobotStationName.ToUpper());
                    FormatArgument.AppendFormat("\t\t\tIF (bROBOT_LOCAL_RESET_FLAG == FALSE) and (bROBOT_LOCAL_HOMING_FLAG == FALSE) THEN\n");
                    FormatArgument.AppendFormat("\t\t\t\t; Go slower working mode\n");
                    FormatArgument.AppendFormat("\t\t\t\trobot_slowerWorkingMode(nROBOT_SLOW_PTP_SPEED, nROBOT_SLOW_LIN_SPEED, nROBOT_DEPART_ACCEL)\n");
                    FormatArgument.AppendFormat("\t\t\t\tLIN frNEAR_STATION_OFFSET : j{0} C_Dis \n", program.Stations[i].RobotStationName);
                    FormatArgument.AppendFormat("\t\t\tENDIF\n");
                    FormatArgument.AppendFormat("\t\t\t; Go full working speed\n");
                    FormatArgument.AppendFormat("\t\t\trobot_fullWorkingSpeed()\n");
                    if (program.Stations[i].StationFreeEnabled)
                    {
                        FormatArgument.AppendFormat("\t\t\tTRIGGER WHEN DISTANCE = 1 DELAY = 0 DO robot_stationsFree() PRIO=-1\n");
                    }
                    FormatArgument.AppendFormat("\t\t\tTRIGGER WHEN DISTANCE = 1 DELAY = 0 DO bRobot_onStation = False\n");
                    if (i == program.Stations.Count - 1)
                    {
                        FormatArgument.AppendFormat("\t\t\tPTP frABOVE_STATION_Z_OFFSET : j{0} C_Ptp\n", program.Stations[i].RobotStationName);
                    }
                    else
                    {
                        FormatArgument.AppendFormat("\t\t\tPTP frABOVE_STATION_Z_OFFSET : j{0} C_Ptp\n\n", program.Stations[i].RobotStationName);
                    }
                }
            }
            return string.Format(Format, FormatArgument).ToString() + "\n";
        }

        public static string GetKukaHellaMoveOnStationFormat(ProgramModel program)
        {
            StringBuilder FormatArgument = new StringBuilder();

            string Format = KukaHellaMoveOnStationFormat;

            for (int i = 0; i < program.Stations.Count; i++)
            {
                FormatArgument.AppendFormat("\t\tCASE #TO_{0}\n", program.Stations[i].RobotStationName.ToUpper());
                if (program.Stations[i].RobotStationName == "Home")
                {
                    FormatArgument.AppendFormat("\t\t\tPTP xHOME \n\n");
                }
                else
                {
                    if (program.Stations[i].StationFreeEnabled)
                    {
                        FormatArgument.AppendFormat("\t\t\tTRIGGER WHEN DISTANCE = 0 DELAY = 0 DO do{0}_FREE = FALSE\n", program.Stations[i].RobotStationName.ToUpper());
                    }
                }
            }
            return string.Format(Format, FormatArgument).ToString();
        }
        public static string GetKukaHellaMoveOnStationFormatSimpleProgram(ProgramModel program)
        {
            StringBuilder FormatArgument = new StringBuilder();

            string Format = KukaHellaMoveOnStationFormat;

            for (int i = 0; i < program.Stations.Count; i++)
            {
                FormatArgument.AppendFormat("\t\tCASE #TO_{0}\n", program.Stations[i].RobotStationName.ToUpper());
                if (program.Stations[i].RobotStationName == "Home")
                {
                    FormatArgument.AppendFormat("\t\t\tPTP xHOME\n\n");
                }
                else
                {
                    if (program.Stations[i].StationFreeEnabled)
                    {
                        FormatArgument.AppendFormat("\t\t\tTRIGGER WHEN DISTANCE = 0 DELAY = 0 DO do{0}_FREE = FALSE\n", program.Stations[i].RobotStationName.ToUpper());
                    }
                    FormatArgument.AppendFormat("\t\t\tPTP frNEAR_STATION_OFFSET : j{0} C_Ptp\n", program.Stations[i].RobotStationName);
                    FormatArgument.AppendFormat("\t\t\trobot_slowerWorkingMode(nROBOT_SLOW_PTP_SPEED, nROBOT_SLOW_LIN_SPEED, nROBOT_DEPART_ACCEL)\n");
                    FormatArgument.AppendFormat("\t\t\tTRIGGER WHEN DISTANCE = 1 DELAY = 0 DO bRobot_onStation = True\n");
                    if (i == program.Stations.Count - 1)
                    {
                        FormatArgument.AppendFormat("\t\t\tLIN j{0}\n", program.Stations[i].RobotStationName);
                    }
                    else
                    {
                        FormatArgument.AppendFormat("\t\t\tLIN j{0}\n\n", program.Stations[i].RobotStationName);
                    }
                }
            }
            return string.Format(Format, FormatArgument).ToString();
        }

        // Generate programMotion.dat
        public static string GetKukaHellaMotionDatFormat(ProgramModel program)
        {
            string ProgName;
            StringBuilder FormatArgumentPoints = new StringBuilder();

            string Format = KukaHellaMotionDatFormat;

            ProgName = program.ProgramName.ToString();

            for (int i = 0; i < program.Stations.Count; i++)
            {
                FormatArgumentPoints.AppendFormat("DECL GLOBAL E6POS j{0}" + "={{X 0,Y 0.0,Z 0,A 0,B 0,C 0,S 0,T 0,E1 0.0,E2 0.0,E3 0.0,E4 0.0,E5 0.0,E6 0.0}}\n", program.Stations[i].RobotStationName);
            }
            return string.Format(Format, ProgName, FormatArgumentPoints).ToString() + "\n\n";
        }


        // Generate /Functions/robotFunctions.src
        public static string GetKukaHellaStandardFunctionsFromat(ProgramModel program)
        {
            StringBuilder FormatArgumentStationFree = new StringBuilder();

            StringBuilder FormatArgumentStationBusy = new StringBuilder();

            string Format = KukaHellaStandardFunctionsFromat;

            for (int i = 0; i < program.Stations.Count; i++)
            {
                if (program.Stations[i].StationFreeEnabled)
                { FormatArgumentStationFree.AppendFormat("\tdo{0}_FREE = TRUE\n", program.Stations[i].RobotStationName.ToUpper()); }
            }

            for (int i = 0; i < program.Stations.Count; i++)
            {
                if (program.Stations[i].StationFreeEnabled)
                { FormatArgumentStationBusy.AppendFormat("\tdo{0}_FREE = FALSE\n", program.Stations[i].RobotStationName.ToUpper()); }
            }

            return string.Format(Format, FormatArgumentStationFree, FormatArgumentStationBusy).ToString() + "\n";
        }

        public static string GetKukaHellaPalletFunctionFormat()
        {
            string Format = KukaHellaPalletFunctionFormat;

            return string.Format(Format).ToString() + "\n\n";
        }

        public static string GetKukaHellaDistFunctionFormat()
        {
            string Fromat = KukaHellaDistFunctionFormat;

            return string.Format(Fromat).ToString() + "\n\n";
        }

        // Generate Function.dat

        public static string GetKukaHellaFunctionsDatFormat()
        {
            string Fromat = KukaHellaFunctionsDat;

            return string.Format(Fromat).ToString() + "\n\n";
        }
        // Generate Cell.sps

        public static string GetKukaHellaCellFormat()
        {
            string Fromat = KukaHellaCellFormat;

            return string.Format(Fromat).ToString() + "\n\n";
        }
        // Generate Sps

        public static string GetKukaHellaSpsFormat()
        {
            string Fromat = KukaHellaSpsFormat;

            return string.Format(Fromat).ToString() + "\n\n";
        }
        // Generate $Config

        public static string GetKukaHellaConfigDatFormat(ProgramModel program)
        {
            StringBuilder FormatArgumentFromLoc = new StringBuilder();
            StringBuilder FormatArgumentToLoc = new StringBuilder();
            StringBuilder FormatArgument = new StringBuilder();
            int j = 33;
            string Fromat = KukaHellaConfigDatFormat;

            for (int i = 0; i < program.Stations.Count; i++)
            {
                if (program.Stations[i].StationFreeEnabled)
                {
                    FormatArgument.AppendFormat("SIGNAL do{0}_FREE $OUT[{1}]\n", program.Stations[i].RobotStationName.ToUpper(), j);
                    j++;
                }

                FormatArgumentFromLoc.AppendFormat(", FROM_{0}", program.Stations[i].RobotStationName.ToUpper());
                FormatArgumentToLoc.AppendFormat(", TO_{0}", program.Stations[i].RobotStationName.ToUpper());
            }
            return string.Format(Fromat, FormatArgument, FormatArgumentFromLoc, FormatArgumentToLoc).ToString() + "\n\n";
        }
        #endregion

        #region///// EPSON /////

        // Generate Main.prg
        public static string GetMainFunc(ObservableCollection<ProgramModel> robot)
        {
            string Format = EpsonMainFuncFormatEng;
            StringBuilder FormatArgument = new StringBuilder();

            // Go through all robots
            for (int i = 0; i < robot.Count; i++)
            {
                string RobotNameLowerChar = robot[i].ProgramName.ToLower();
                if (i == robot.Count - 1)
                    FormatArgument.AppendFormat("\tCall {0}_init", RobotNameLowerChar);
                else
                    FormatArgument.AppendFormat("\tCall {0}_init\n", RobotNameLowerChar);
            }
            return string.Format(Format, FormatArgument).ToString() + "\n\n";
        }

        public static string GetErrorHandlingFunc(ObservableCollection<ProgramModel> robot)
        {
            string Format = EpsonErrorHandlingFuncFormatEng;
            StringBuilder FormatArgumentInMotion = new StringBuilder();

            // Go through all the robots and append off commands for in motion
            for (int i = 0; i < robot.Count; i++)
            {
                string RobotNameUpperChar = robot[i].ProgramName.ToUpper();

                if (i < robot.Count - 1)
                    FormatArgumentInMotion.AppendFormat("\tOFF {0}_O_IN_MOTION, Forced\n", RobotNameUpperChar);
                else
                    FormatArgumentInMotion.AppendFormat("\tOFF {0}_O_IN_MOTION, Forced", RobotNameUpperChar);
            }

            return string.Format(Format, FormatArgumentInMotion).ToString() + "\n\n";
        }

        public static string GetEstopHandlingFunc(ObservableCollection<ProgramModel> robot)
        {
            string Format = EpsonEstopHandlingFuncFormatEng;
            StringBuilder FormatArgumentInMotion = new StringBuilder();

            // Go through all the robots and append off commands for in motion
            for (int i = 0; i < robot.Count; i++)
            {
                string RobotNameUpperChar = robot[i].ProgramName.ToUpper();

                if (i < robot.Count - 1)
                    FormatArgumentInMotion.AppendFormat($"\tOFF {RobotNameUpperChar}_O_IN_MOTION, Forced\n");
                else
                    FormatArgumentInMotion.AppendFormat($"\tOFF {RobotNameUpperChar}_O_IN_MOTION, Forced");
            }
            return string.Format(Format, FormatArgumentInMotion).ToString() + "\n\n";
        }

        // Generate robot.prg
        public static string GetHeader(ProgramModel robot)
        {
            string Format = EpsonHeaderFormatEng;

            return string.Format(Format, robot.ProgramNameUpperChar, robot.ProgramNameLowerChar) + "\n\n";
        }

        public static string GetPalletsDefinitions(ProgramModel program)
        {
            StringBuilder FormatArgument = new StringBuilder();
            for ( int i = 0; i < program.Stations.Count; i++)
            {
                if (program.Stations[i].Pallet)
                {
                    FormatArgument.AppendFormat($"#define {program.Stations[i].RobotStationName} {i}\n");
                }
            }
            return FormatArgument.ToString()+ "\n";
        }

        public static string GetInitFunc(ProgramModel robot)
        {
            string Format = EpsonInitFuncFormatEng;
            StringBuilder FormatArgument = new StringBuilder();

            for (int i = 0; i < robot.Stations.Count; i++)
            {
                if (robot.Stations[i].Pallet)
                {
                    FormatArgument.AppendFormat($"\tPallet {robot.Stations[i].RobotStationName}, p{robot.Stations[i].RobotStationName}P1, p{robot.Stations[i].RobotStationName}P2, p{robot.Stations[i].RobotStationName}P3,p{robot.Stations[i].RobotStationName}P4, 1, 1 \n" );
                }
            }

            return string.Format(Format, robot.ProgramNameLowerChar, robot.ProgramNameUpperChar, robot.RobotNumber, FormatArgument).ToString() + "\n\n";
        }

        public static string GetHomingFunc(ProgramModel robot)
        {
            string Format = EpsonHomingFuncFormatEng;
            StringBuilder FormatArgument = new StringBuilder();

            for (int i = 0; i < robot.Stations.Count; i++)
            {
                if (i == 0)
                {
                    FormatArgument.AppendFormat("\t\t\t\tCase {0}\n", i);
                    FormatArgument.AppendFormat("\t\t\t\t\tMemOn ROBOT_FROM_{0}\n", robot.Stations[i].RobotStationNameToUpper);
                }

                else if (i == robot.Stations.Count - 1)
                {
                    FormatArgument.AppendFormat("\t\t\t\tCase {0}\n", i);
                    FormatArgument.AppendFormat("\t\t\t\t\tMemOn ROBOT_FROM_{0}\n", robot.Stations[i].RobotStationNameToUpper);
                    FormatArgument.AppendFormat("\t\t\t\t\trobot_onStation = True\n");
                }
                else
                {
                    FormatArgument.AppendFormat("\t\t\t\tCase {0}\n", i);
                    FormatArgument.AppendFormat("\t\t\t\t\tMemOn ROBOT_FROM_{0}\n", robot.Stations[i].RobotStationNameToUpper);
                    FormatArgument.AppendFormat("\t\t\t\t\trobot_onStation = True\n\n");
                }
            }

            return string.Format(Format, robot.ProgramNameLowerChar, robot.ProgramNameUpperChar, robot.RobotNumber, robot.Stations.Count - 1, FormatArgument).ToString() + "\n\n";
        }

        public static string GetOperationFunc(ProgramModel program)
        {
            string Format = EpsonOperationFuncFormatEng;
            StringBuilder FormatArgument = new StringBuilder();

            for (int i = 0; i < program.Stations.Count; i++)
            {
                if (i == 0)
                {
                    FormatArgument.AppendFormat("Case {0}\n", (i));
                    FormatArgument.AppendFormat("\t\t\t\t\tCall {0}_go{1}()\n\n", program.ProgramNameLowerChar, program.Stations[i].RobotStationName);
                }

                else if (i == program.Stations.Count - 1)
                {
                    FormatArgument.AppendFormat("\t\t\t\tCase {0}\n", (i));
                    FormatArgument.AppendFormat("\t\t\t\t\tCall {0}_go{1}()\n", program.ProgramNameLowerChar, program.Stations[i].RobotStationName);
                }

                else
                {
                    FormatArgument.AppendFormat("\t\t\t\tCase {0}\n", (i));
                    FormatArgument.AppendFormat("\t\t\t\t\tCall {0}_go{1}()\n\n", program.ProgramNameLowerChar, program.Stations[i].RobotStationName);
                }
            }
            return string.Format(Format, program.ProgramNameLowerChar, program.ProgramNameUpperChar, program.RobotNumber, FormatArgument).ToString() + "\n\n";
        }


        public static string GetMovementFuncs(ProgramModel program)
        {
            StringBuilder FormatArgument = new StringBuilder();
            StringBuilder FormatArgumentDestinations = new StringBuilder();
            string Movement = EpsonMovementFuncFormatEng;

            string Format = "{0}";

            for (int i = 0; i < program.Stations.Count; i++)
            {
                if (program.Stations[i].RobotStationName == "Home")
                {
                    FormatArgumentDestinations.AppendFormat("\tIf MemSw ({0}_FROM_{1}) = On Then\n\n", program.ProgramNameUpperChar, program.Stations[i].RobotStationNameToUpper);
                }
                if (i == (program.Stations.Count - 1))
                {
                    FormatArgumentDestinations.AppendFormat("\tIf MemSw ({0}_FROM_{1}) = On Then\n", program.ProgramNameUpperChar, program.Stations[i].RobotStationNameToUpper);
                }
                else
                {
                    FormatArgumentDestinations.AppendFormat("\tElseIf MemSw ({0}_FROM_{1}) = On Then\n\n", program.ProgramNameUpperChar, program.Stations[i].RobotStationNameToUpper);
                }

            }

            for (int i = 0; i < program.Stations.Count; i++)
            {
                FormatArgument.AppendFormat(Movement + "\n\n", program.ProgramNameLowerChar, program.Stations[i].RobotStationName, program.ProgramNameUpperChar, program.Stations[i].RobotStationNameToUpper, FormatArgumentDestinations);
            }

            return string.Format(Format, FormatArgument).ToString();
        }
        public static string GetMovementsSimpleProgramFunc(ProgramModel program)
        {
            StringBuilder FormatArgument = new StringBuilder();
            StringBuilder FormatArgumentDestinations = new StringBuilder();
            StringBuilder FormatArgumentStationFree = new StringBuilder();
            List<StringBuilder> FormatArgumentPoints = new List<StringBuilder>();
            StringBuilder FormatArgumentDestinationSimpleProgram = new StringBuilder();
            string Movement = EpsonMovementFuncFormatEng;

            string Format = "{0}";

                for (int i = 0; i < program.Stations.Count; i++)
            {

                FormatArgumentPoints.Add(new StringBuilder());
                if (program.Stations[i].Pallet)
                    FormatArgumentPoints[i].AppendFormat("\t\tGo Pallet ({0}, additionalPos) :Z({1}_MAX_Z_HEIGHT) \n\n", program.Stations[i].RobotStationName, program.ProgramNameUpperChar);
                else if (1 != program.Stations[i].Positions)
                    FormatArgumentPoints[i].AppendFormat("\t\tCall go{0}MaxZHeight \n\n", program.Stations[i].RobotStationName, program.ProgramNameUpperChar);
                else
                    FormatArgumentPoints[i].AppendFormat("\t\tGo p{0} :Z({1}_MAX_Z_HEIGHT) \n\n", program.Stations[i].RobotStationName, program.ProgramNameUpperChar);

            }

            for (int i = 0; i < program.Stations.Count; i++)
            {
                if (program.Stations[i].RobotStationName == "Home")
                {
                    FormatArgumentDestinations.AppendFormat("\tIf MemSw ({0}_FROM_{1}) = On Then\n", program.ProgramNameUpperChar, program.Stations[i].RobotStationNameToUpper);
                    FormatArgumentDestinations.AppendFormat(@"{{0}}");
                }

                else
                {
                    FormatArgumentDestinations.AppendFormat("\tElseIf MemSw ({0}_FROM_{1}) = On Then\n", program.ProgramNameUpperChar, program.Stations[i].RobotStationNameToUpper);
                    FormatArgumentDestinations.AppendFormat(@"{{0}}");
                }

                //FormatArgumentDestinations.AppendFormat(FormatArgumentPoints[i].ToString());
            }

            for (int i = 0; i < program.Stations.Count; i++)
            {
                string Destinations = FormatArgumentDestinations.ToString();
                FormatArgumentDestinationSimpleProgram.AppendFormat(Destinations, FormatArgumentPoints[i]);
            }

            for (int i = 0; i < program.Stations.Count; i++)
            {

                if (program.Stations[i].StationFreeEnabled)
                {
                    FormatArgumentStationFree.AppendFormat("\n\tIf Sw(ROBOT_I_ON_STATION) = On Then\n");
                    FormatArgumentStationFree.AppendFormat("\t\tOff {0}_O_{1}_FREE\n", program.ProgramNameUpperChar, program.Stations[i].RobotStationNameToUpper);
                    FormatArgumentStationFree.AppendFormat("\tEndIf\n");
                }

                string Destinations = FormatArgumentDestinations.ToString();
                FormatArgumentDestinationSimpleProgram.Clear();
                FormatArgumentDestinationSimpleProgram.AppendFormat(Destinations, FormatArgumentPoints[i]);
                FormatArgument.AppendFormat(Movement + "\n\n", program.ProgramNameLowerChar, program.Stations[i].RobotStationName, program.ProgramNameUpperChar, program.Stations[i].RobotStationNameToUpper, FormatArgumentDestinationSimpleProgram, FormatArgumentStationFree);
                FormatArgumentStationFree.Clear();
             }

            return string.Format(Format, FormatArgument).ToString();
        }

        public static string GetMoveOnStationFunc(ProgramModel program)
        {
            StringBuilder FormatArgumentIfStatements = new StringBuilder();
            string Format = "";

            Format = EpsonMoveOnStationFuncFormatEng;

            // Append if statements
            for (int i = 0; i < program.Stations.Count; i++)
            {
                if (i == 0)
                    FormatArgumentIfStatements.AppendFormat("\t\tIf MemSw({0}_TO_{1}) = On Then\n", program.ProgramNameUpperChar, program.Stations[i].RobotStationNameToUpper);
                else
                    FormatArgumentIfStatements.AppendFormat("\t\tElseIf MemSw({0}_TO_{1}) = On Then\n", program.ProgramNameUpperChar, program.Stations[i].RobotStationNameToUpper);

                // Check if free bit needs to be included
                if (program.Stations[i].StationFreeEnabled)
                    FormatArgumentIfStatements.AppendFormat("\t\t\tOff {0}_O_{1}_FREE\n\n", program.ProgramNameUpperChar, program.Stations[i].RobotStationNameToUpper);
                else
                    FormatArgumentIfStatements.AppendFormat("\n");
            }
            FormatArgumentIfStatements.AppendLine("\t\tElse");

            // Append error based on the language
            FormatArgumentIfStatements.AppendLine("\t\t\tError PROGRAM_ERROR");

            FormatArgumentIfStatements.AppendFormat("\t\tEndIf");

            return string.Format(Format, program.ProgramNameLowerChar, program.ProgramNameUpperChar, program.RobotNumber, FormatArgumentIfStatements) + "\n\n";
        }

        public static string GetMoveOnStationSimpleProgramFunc(ProgramModel program)
        {
            StringBuilder FormatArgumentIfStatements = new StringBuilder();
            string Format = "";

            Format = EpsonMoveOnStationFuncFormatEng;

            // Append if statements
            for (int i = 0; i < program.Stations.Count; i++)
            {
                if (i == 0)
                {
                    FormatArgumentIfStatements.AppendFormat("\t\tIf MemSw({0}_TO_{1}) = On Then\n", program.ProgramNameUpperChar, program.Stations[i].RobotStationNameToUpper);
                    FormatArgumentIfStatements.AppendFormat("\t\t\tGo p{0}\n\n", program.Stations[i].RobotStationName);
                }
                else
                {
                    FormatArgumentIfStatements.AppendFormat("\t\tElseIf MemSw({0}_TO_{1}) = On Then\n", program.ProgramNameUpperChar, program.Stations[i].RobotStationNameToUpper);
                    // Check if free bit needs to be included
                    if (program.Stations[i].StationFreeEnabled)
                    {
                        FormatArgumentIfStatements.AppendFormat("\t\t\tOff {0}_O_{1}_FREE\n\n", program.ProgramNameUpperChar, program.Stations[i].RobotStationNameToUpper);
                    }

                    if (program.Stations[i].Positions != 1)
                    { 
                        
                    }
                    else
                    {
                        
                    }


                    if (program.Stations[i].Pallet)
                    {
                        FormatArgumentIfStatements.AppendFormat("\t\t\tIf CZ(CurPos) > (CZ(p{0}P1) + {1}_STATION_Z_OFFSET) = True Then\n", program.Stations[i].RobotStationName, program.ProgramNameUpperChar);
                        FormatArgumentIfStatements.AppendFormat("\t\t\t\tGo Pallet ({0} , additionalPos) +Z({1}_STATION_Z_OFFSET)\n", program.Stations[i].RobotStationName, program.ProgramNameUpperChar);
                        FormatArgumentIfStatements.AppendFormat("\t\t\tEndIf\n");
                        FormatArgumentIfStatements.AppendFormat("\t\t\t' Go slower working mode\n");
                        FormatArgumentIfStatements.AppendFormat("\t\t\tCall {0}_slowerWorkingMode({1}_SLOWER_SPEED, {1}_APPROACH_ACCEL, {1}_APPROACH_DECEL)\n", program.ProgramNameLowerChar, program.ProgramNameUpperChar);
                        FormatArgumentIfStatements.AppendFormat("\t\t\tGo Pallet ({0} , additionalPos)\n\n", program.Stations[i].RobotStationName);
                    }
                    else if (program.Stations[i].Positions != 1)
                    {
                        FormatArgumentIfStatements.AppendFormat("\t\t\tIf CZ(CurPos) > (CZ(p{0}1) + {1}_STATION_Z_OFFSET) = True Then\n", program.Stations[i].RobotStationName, program.ProgramNameUpperChar);
                        FormatArgumentIfStatements.AppendFormat("\t\t\t\tCall go{0}NearStation\n", program.Stations[i].RobotStationName, program.ProgramNameUpperChar);
                        FormatArgumentIfStatements.AppendFormat("\t\t\tEndIf\n");
                        FormatArgumentIfStatements.AppendFormat("\t\t\t' Go slower working mode\n");
                        FormatArgumentIfStatements.AppendFormat("\t\t\tCall {0}_slowerWorkingMode({1}_SLOWER_SPEED, {1}_APPROACH_ACCEL, {1}_APPROACH_DECEL)\n", program.ProgramNameLowerChar, program.ProgramNameUpperChar);
                        FormatArgumentIfStatements.AppendFormat("\t\t\tCall go{0}\n\n", program.Stations[i].RobotStationName);
                    }
                    else
                    {
                        FormatArgumentIfStatements.AppendFormat("\t\t\tIf CZ(CurPos) > (CZ(p{0}) + {1}_STATION_Z_OFFSET) = True Then\n", program.Stations[i].RobotStationName, program.ProgramNameUpperChar);
                        FormatArgumentIfStatements.AppendFormat("\t\t\t\tGo p{0} +Z({1}_STATION_Z_OFFSET)\n", program.Stations[i].RobotStationName, program.ProgramNameUpperChar);
                        FormatArgumentIfStatements.AppendFormat("\t\t\tEndIf\n");
                        FormatArgumentIfStatements.AppendFormat("\t\t\t' Go slower working mode\n");
                        FormatArgumentIfStatements.AppendFormat("\t\t\tCall {0}_slowerWorkingMode({1}_SLOWER_SPEED, {1}_APPROACH_ACCEL, {1}_APPROACH_DECEL)\n", program.ProgramNameLowerChar, program.ProgramNameUpperChar);
                        FormatArgumentIfStatements.AppendFormat("\t\t\tGo p{0}\n\n", program.Stations[i].RobotStationName);
                    }                
                }
            }
            FormatArgumentIfStatements.AppendLine("\t\tElse");

            // Append error based on the language
            FormatArgumentIfStatements.AppendLine("\t\t\tError PROGRAM_ERROR\n");

            FormatArgumentIfStatements.AppendFormat("\t\tEndIf");

            return string.Format(Format, program.ProgramNameLowerChar, program.ProgramNameUpperChar, program.RobotNumber, FormatArgumentIfStatements) + "\n\n";
        }
        public static string GetMoveAwayFunc(ProgramModel robot)
        {
            StringBuilder FormatArgumentIfStatements = new StringBuilder();
            string Format = "";

            Format = EpsonMoveAwayStationFuncFormatEng;


            // Append if statements
            for (int i = 0; i < robot.Stations.Count; i++)
            {
                if (i == 0)
                    FormatArgumentIfStatements.AppendFormat("\t\tIf MemSw({0}_FROM_{1}) = On Then\n\n", robot.ProgramNameUpperChar, robot.Stations[i].RobotStationNameToUpper);
                else
                    FormatArgumentIfStatements.AppendFormat("\t\tElseIf MemSw({0}_FROM_{1}) = On Then\n\n", robot.ProgramNameUpperChar, robot.Stations[i].RobotStationNameToUpper);
            }
            FormatArgumentIfStatements.AppendLine("\t\tElse");

            // Append error 
            FormatArgumentIfStatements.AppendLine("\t\t\tError PROGRAM_ERROR");

            FormatArgumentIfStatements.AppendFormat("\t\tEndIf");

            return string.Format(Format, robot.ProgramNameLowerChar, robot.ProgramNameUpperChar, robot.RobotNumber, FormatArgumentIfStatements) + "\n\n";
        }

        public static string GetMoveAwaySimpleProgramFunc(ProgramModel robot)
        {
            StringBuilder FormatArgumentIfStatements = new StringBuilder();
            string Format = "";

            Format = EpsonMoveAwayStationFuncFormatEng;


            // Append if statements
            for (int i = 0; i < robot.Stations.Count; i++)
            {
                if (i == 0)
                {
                    FormatArgumentIfStatements.AppendFormat("\t\tIf MemSw({0}_FROM_{1}) = On Then\n", robot.ProgramNameUpperChar, robot.Stations[i].RobotStationNameToUpper);
                    FormatArgumentIfStatements.AppendFormat("\t\t\tGo p{0}\n\n", robot.Stations[i].RobotStationName);
                }
                else
                {
                    FormatArgumentIfStatements.AppendFormat("\t\tElseIf MemSw({0}_FROM_{1}) = On Then\n", robot.ProgramNameUpperChar, robot.Stations[i].RobotStationNameToUpper);
                    FormatArgumentIfStatements.AppendFormat("\t\t\tIf CZ(CurPos) + {0}_STATION_Z_OFFSET < {0}_MAX_Z_HEIGHT = True Then\n", robot.ProgramNameUpperChar);
                    FormatArgumentIfStatements.AppendFormat("\t\t\t\t' Go slower depart working speed\n");
                    FormatArgumentIfStatements.AppendFormat("\t\t\t\tCall {0}_slowerWorkingMode({1}_SLOWER_SPEED, {1}_DEPART_ACCEL, {1}_DEPART_DECEL)\n", robot.ProgramNameLowerChar, robot.ProgramNameUpperChar);
                    FormatArgumentIfStatements.AppendFormat("\t\t\t\tGo RealPos +Z({0}_STATION_Z_OFFSET)\n", robot.ProgramNameUpperChar);
                    FormatArgumentIfStatements.AppendFormat("\t\t\tEndif\n");
                    FormatArgumentIfStatements.AppendFormat("\t\t\t' Go full wroking speed\n");
                    FormatArgumentIfStatements.AppendFormat("\t\t\tCall {0}_fullWorkingSpeed\n", robot.ProgramNameLowerChar);
                    FormatArgumentIfStatements.AppendFormat("\t\t\tGo RealPos :Z({0}_MAX_Z_HEIGHT)\n\n", robot.ProgramNameUpperChar);
                    if (robot.Stations[i].StationFreeEnabled)
                    { 
                        FormatArgumentIfStatements.AppendFormat("\t\t\tOn {0}_O_{1}_FREE\n\n", robot.ProgramNameUpperChar, robot.Stations[i].RobotStationNameToUpper);
                    }
                }
            }
            FormatArgumentIfStatements.AppendLine("\t\tElse");

            // Append error 
            FormatArgumentIfStatements.AppendLine("\t\t\tError PROGRAM_ERROR");

            FormatArgumentIfStatements.AppendFormat("\t\tEndIf");

            return string.Format(Format, robot.ProgramNameLowerChar, robot.ProgramNameUpperChar, robot.RobotNumber, FormatArgumentIfStatements) + "\n\n";
        }

        public static string GetGoOnAdditionalPositionsFunc(ProgramModel robot)
        {
            StringBuilder FormatArgument = new StringBuilder();
            for (int i = 0; i < robot.Stations.Count ; i++)
            {
                if (1 != robot.Stations[i].Positions)
                {
                    // Max Z
                    FormatArgument.AppendFormat("Function go" + robot.Stations[i].RobotStationName + "MaxZHeight\n");
                    FormatArgument.AppendFormat("\tSelect additionalPos\n");
                    for (int j = 1; j < robot.Stations[i].Positions +1; j++)
                    {
                        FormatArgument.AppendFormat("\t\tCase " + j+ "\n");
                        FormatArgument.AppendFormat("\t\t\tGo p" + robot.Stations[i].RobotStationName + j + " :Z(ROBOT_MAX_Z_HEIGHT)\n\n");
                    }
                    FormatArgument.AppendFormat("\t\tDefault\n");
                    FormatArgument.AppendFormat("\t\t\tError POSITION_ERROR\n");
                    FormatArgument.AppendFormat("\tSend\n");
                    FormatArgument.AppendFormat("Fend\n\n");

                    // Near station
                    FormatArgument.AppendFormat("Function go" + robot.Stations[i].RobotStationName  + "NearStation\n");
                    FormatArgument.AppendFormat("\tSelect additionalPos\n");
                    for (int j = 1; j < robot.Stations[i].Positions + 1; j++)
                    {
                        FormatArgument.AppendFormat("\t\tCase " + j + "\n");
                        FormatArgument.AppendFormat("\t\t\tGo p" + robot.Stations[i].RobotStationName + j + " +Z(ROBOT_STATION_Z_OFFSET)\n\n");
                    }
                    FormatArgument.AppendFormat("\t\tDefault\n");
                    FormatArgument.AppendFormat("\t\t\tError POSITION_ERROR\n");
                    FormatArgument.AppendFormat("\tSend\n");
                    FormatArgument.AppendFormat("Fend\n\n");

                    // On station
                    FormatArgument.AppendFormat("Function go" + robot.Stations[i].RobotStationName  + "\n");
                    FormatArgument.AppendFormat("\tSelect additionalPos\n");
                    for (int j = 1; j < robot.Stations[i].Positions + 1; j++)
                    {
                        FormatArgument.AppendFormat("\t\tCase " + j + "\n");
                        FormatArgument.AppendFormat("\t\t\tGo p" + robot.Stations[i].RobotStationName + j + "\n\n");
                    }
                    FormatArgument.AppendFormat("\t\tDefault\n");
                    FormatArgument.AppendFormat("\t\t\tError POSITION_ERROR\n");
                    FormatArgument.AppendFormat("\tSend\n");
                    FormatArgument.AppendFormat("Fend\n\n");
                }

            }
            return string.Format(FormatArgument.ToString()) + "\n\n";
        }
        public static string GetPowerModeFunc(ProgramModel robot)
        {
            string Format = "";

            Format = EpsonPowerModeFuncFormatEng;

            return string.Format(Format, robot.ProgramNameLowerChar, robot.ProgramNameUpperChar, robot.RobotNumber).ToString() + "\n\n";
        }

        public static string GetFullPowerModeFunc(ProgramModel robot)
        {
            string Format = "";

            Format = EpsonFullPowerModeFuncFormatEng;

            return string.Format(Format, robot.ProgramNameLowerChar, robot.ProgramNameUpperChar, robot.RobotNumber).ToString() + "\n\n";
        }

        public static string GetSlowPowerModeFunc(ProgramModel robot)
        {
            string Format = "";

            Format = EpsonSlowPowerModeFuncFormatEng;


            return string.Format(Format, robot.ProgramNameLowerChar, robot.ProgramNameUpperChar, robot.RobotNumber).ToString() + "\n\n";
        }

        public static string GetCpBarrierFunc(ProgramModel robot)
        {
            string RobotNameUpperChar = robot.ProgramName.ToUpper();
            string RobotNameLowerChar = robot.ProgramName.ToLower();
            string Format = "";

            Format = EpsonCpBarrierFuncFormatEng;

            return string.Format(Format, RobotNameLowerChar, RobotNameUpperChar, robot.RobotNumber).ToString() + "\n\n";
        }

        public static string GetStationsFreeFunc(ProgramModel robot)
        {
            StringBuilder FormatArgumentOnCommands = new StringBuilder();
            //  string Format = "";

            string Format = EpsonStationsFreeFuncFormatEng;


            // Go through all stations
            for (int i = 0; i < robot.Stations.Count; i++)
            {
                // Check if free bit needs to be included
                if (robot.Stations[i].StationFreeEnabled)
                {
                    if (FormatArgumentOnCommands.Length == 0)
                        FormatArgumentOnCommands.AppendFormat("\tOn {0}_O_{1}_FREE", robot.ProgramNameUpperChar, robot.Stations[i].RobotStationNameToUpper);
                    else
                        FormatArgumentOnCommands.AppendFormat("\n\tOn {0}_O_{1}_FREE", robot.ProgramNameUpperChar, robot.Stations[i].RobotStationNameToUpper);
                }
            }

            return string.Format(Format, robot.ProgramNameLowerChar, FormatArgumentOnCommands).ToString() + "\n\n";
        }

        public static string GetStationsBusyFunc(ProgramModel robot)
        {
            StringBuilder FormatArgumentOffCommands = new StringBuilder();
            string Format = "";

            Format = EpsonStationsBusyFuncFormatEng;


            // Go through all stations
            for (int i = 0; i < robot.Stations.Count; i++)
            {
                // Check if free bit needs to be included
                if (robot.Stations[i].StationFreeEnabled)
                {
                    if (FormatArgumentOffCommands.Length == 0)
                        FormatArgumentOffCommands.AppendFormat("\tOff {0}_O_{1}_FREE", robot.ProgramNameUpperChar, robot.Stations[i].RobotStationNameToUpper);
                    else
                        FormatArgumentOffCommands.AppendFormat("\n\tOff {0}_O_{1}_FREE", robot.ProgramNameUpperChar, robot.Stations[i].RobotStationNameToUpper);
                }
            }

            return string.Format(Format, robot.ProgramNameLowerChar, FormatArgumentOffCommands).ToString() + "\n\n";
        }

        public static string GetResetDepartLocationsFunc(ProgramModel robot)
        {
            StringBuilder FormatArgumentOffCommands = new StringBuilder();
            string Format = "";

            Format = EpsonResetDepartLocationsFuncFormatEng;


            // Go through all stations
            for (int i = 0; i < robot.Stations.Count; i++)
            {
                if (i == 0)
                    FormatArgumentOffCommands.AppendFormat("\tMemOff {0}_FROM_{1}", robot.ProgramNameUpperChar, robot.Stations[i].RobotStationNameToUpper);
                else
                    FormatArgumentOffCommands.AppendFormat("\n\tMemOff {0}_FROM_{1}", robot.ProgramNameUpperChar, robot.Stations[i].RobotStationNameToUpper);
            }

            return string.Format(Format, robot.ProgramNameLowerChar, FormatArgumentOffCommands).ToString() + "\n\n";
        }

        public static string GetResetDestLocationsFunc(ProgramModel robot)
        {
            StringBuilder FormatArgumentOffCommands = new StringBuilder();
            string Format = "";

            Format = EpsonResetDestLocationsFuncFormatEng;

            // Go through all stations
            for (int i = 0; i < robot.Stations.Count; i++)
            {
                if (i == 0)
                    FormatArgumentOffCommands.AppendFormat("\tMemOff {0}_TO_{1}", robot.ProgramNameUpperChar, robot.Stations[i].RobotStationNameToUpper);
                else
                    FormatArgumentOffCommands.AppendFormat("\n\tMemOff {0}_TO_{1}", robot.ProgramNameUpperChar, robot.Stations[i].RobotStationNameToUpper);
            }

            return string.Format(Format, robot.ProgramNameLowerChar, FormatArgumentOffCommands).ToString() + "\n\n";
        }

        public static string GetResetProfibusOutputsFunc(ProgramModel robot)
        {

            string Format = "";

            Format = EpsonResetProfibusOutputsFuncFormatEng;

            return string.Format(Format, robot.ProgramNameLowerChar, robot.ProgramNameUpperChar, robot.RobotNumber).ToString() + "\n\n";
        }

        public static string GetResetFlagsFunc(ProgramModel robot)
        {

            string Format = "";

            Format = EpsonResetFlagsFuncFormatEng;

            return string.Format(Format, robot.ProgramNameLowerChar, robot.ProgramNameUpperChar, robot.RobotNumber).ToString() + "\n\n";
        }

        public static string GetAllIOLabels(ProgramModel robot)
        {
            StringBuilder FormatIoLabels = new StringBuilder();

            // Create local flags
            FormatIoLabels.AppendLine("Memory labels - local flags:");
            FormatIoLabels.AppendFormat("{0}_LOCAL_HOMING_FLAG\n", robot.ProgramNameUpperChar);
            FormatIoLabels.AppendFormat("{0}_LOCAL_RESET_FLAG\n", robot.ProgramNameUpperChar);
            FormatIoLabels.AppendLine("");

            // Create depart locations
            FormatIoLabels.AppendLine("Memory labels - depart locations:");
            for (int i = 0; i < robot.Stations.Count; i++)
            {
                FormatIoLabels.AppendFormat("{0}_FROM_{1}\n", robot.ProgramNameUpperChar, robot.Stations[i].RobotStationNameToUpper);
            }
            FormatIoLabels.AppendLine("");

            // Create dest locations
            FormatIoLabels.AppendLine("Memory labels - dest locations:");
            for (int i = 0; i < robot.Stations.Count; i++)
            {
                FormatIoLabels.AppendFormat("{0}_TO_{1}\n", robot.ProgramNameUpperChar, robot.Stations[i].RobotStationNameToUpper);
            }
            FormatIoLabels.AppendLine("");

            // Create input words
            FormatIoLabels.AppendLine("Inputs - words:");
            FormatIoLabels.AppendFormat("{0}_I_POSITION_TO\n", robot.ProgramNameUpperChar);
            FormatIoLabels.AppendLine("");

            // Create input system bits
            FormatIoLabels.AppendLine("Inputs - system bits:");
            FormatIoLabels.AppendLine("ROBO_I_S_START");
            FormatIoLabels.AppendLine("ROBO_I_S_STOP");
            FormatIoLabels.AppendLine("ROBO_I_S_PAUSE");
            FormatIoLabels.AppendLine("ROBO_I_S_CONTINUE");
            FormatIoLabels.AppendLine("ROBO_I_S_RESET");
            FormatIoLabels.AppendLine("ROBO_I_S_FORCE_POWER_LOW");
            FormatIoLabels.AppendLine("");

            // Create input bits
            FormatIoLabels.AppendLine("Inputs - bits:");
            FormatIoLabels.AppendFormat("{0}_I_HOMING\n", robot.ProgramNameUpperChar);
            FormatIoLabels.AppendFormat("{0}_I_RESET\n", robot.ProgramNameUpperChar);
            FormatIoLabels.AppendFormat("{0}_I_RUN\n", robot.ProgramNameUpperChar);
            FormatIoLabels.AppendFormat("{0}_I_STEP\n", robot.ProgramNameUpperChar);
            FormatIoLabels.AppendFormat("{0}_I_START\n", robot.ProgramNameUpperChar);
            FormatIoLabels.AppendFormat("{0}_I_ON_STATION\n", robot.ProgramNameUpperChar);
            FormatIoLabels.AppendFormat("{0}_I_GRIFFER_OPEN\n", robot.ProgramNameUpperChar);
            FormatIoLabels.AppendFormat("{0}_I_GRIFFER_CLOSE\n", robot.ProgramNameUpperChar);
            FormatIoLabels.AppendLine("");

            // Create output words
            FormatIoLabels.AppendLine("Outputs - words:");
            FormatIoLabels.AppendFormat("{0}_O_POSITION_AT\n", robot.ProgramNameUpperChar);
            FormatIoLabels.AppendLine("");

            // Create output system bits
            FormatIoLabels.AppendLine("Outputs - system bits:");
            FormatIoLabels.AppendLine("ROBO_O_S_READY");
            FormatIoLabels.AppendLine("ROBO_O_S_RUNNING");
            FormatIoLabels.AppendLine("ROBO_O_S_PAUSE");
            FormatIoLabels.AppendLine("ROBO_O_S_ERROR");
            FormatIoLabels.AppendLine("ROBO_O_S_ESTOPON");
            FormatIoLabels.AppendLine("ROBO_O_S_SAFEGUARD");
            FormatIoLabels.AppendLine("ROBO_O_S_SERROR");
            FormatIoLabels.AppendLine("ROBO_O_S_WARNING");
            FormatIoLabels.AppendLine("");

            // Create output bits
            FormatIoLabels.AppendLine("Outputs - bits:");
            FormatIoLabels.AppendFormat("{0}_O_HOMING_REQUEST\n", robot.ProgramNameUpperChar);
            FormatIoLabels.AppendFormat("{0}_O_RESET_OK\n", robot.ProgramNameUpperChar);
            FormatIoLabels.AppendFormat("{0}_O_HOMING_OK\n", robot.ProgramNameUpperChar);
            FormatIoLabels.AppendFormat("{0}_O_END\n", robot.ProgramNameUpperChar);
            FormatIoLabels.AppendFormat("{0}_O_IN_MOTION\n", robot.ProgramNameUpperChar);
            FormatIoLabels.AppendFormat("{0}_O_LOW_POWER_MODE\n", robot.ProgramNameUpperChar);
            FormatIoLabels.AppendFormat("{0}_O_HIGH_POWER_MODE\n", robot.ProgramNameUpperChar);

            for (int i = 0; i < robot.Stations.Count; i++)
            {
                // Check if free bit needs to be included
                if (robot.Stations[i].StationFreeEnabled)
                    FormatIoLabels.AppendFormat("{0}_O_{1}_FREE\n", robot.ProgramNameUpperChar, robot.Stations[i].RobotStationNameToUpper);
            }

            return FormatIoLabels.ToString();
        }

        //Generate IOLables.dat
        public static string GetIOLablesFunc(ProgramModel robot)
        {
            string Format = "";
            string MemoryFromFormat = "";
            string MemoryToFormat = "";
            string OutputFreeFormat = "";
            Format = EpsonIOLablesFuncFormatEng;
            // StringBuilder FormatInputLabels = new StringBuilder();
            // StringBuilder FormatMemoryBitLables = new StringBuilder();
            int Label;
            int nBit;
            string OutputLabel = "";
            string MemoryLabel = "";
            StringBuilder OutputFreeFormatSB = new StringBuilder();
            StringBuilder MemoryBitsFromatSB = new StringBuilder();

            OutputFreeFormat = EpsonIOGenerateOutputFreeFormat;
            MemoryToFormat = EpsonIOGenerateMemoryToFormat;
            MemoryFromFormat = EpsonIOGenerateMemoryFromFormat;
            // Generating Inputs

            Label = 13;
            nBit = 527;
            for (int i = 0; i < robot.Stations.Count; i++)
            {
                if (robot.Stations[i].StationFreeEnabled)
                {
                    Label++;
                    nBit++;
                    if (i == 1)
                    {
                        OutputFreeFormatSB.AppendFormat("\t");
                        OutputFreeFormatSB.AppendFormat(OutputFreeFormat, Label.ToString(), nBit.ToString(), robot.Stations[i].RobotStationNameToUpper);
                    }
                    else
                    {
                        OutputFreeFormatSB.AppendFormat("\n");
                        OutputFreeFormatSB.AppendFormat("\t");
                        OutputFreeFormatSB.AppendFormat(OutputFreeFormat, Label.ToString(), nBit.ToString(), robot.Stations[i].RobotStationNameToUpper);
                    }

                }
                OutputLabel = Label.ToString();
            }

            Label = 2;
            nBit = 1;
            for (int i = 0; i < robot.Stations.Count; i++)
            {
                Label++;
                nBit++;
                if (i == 0)
                {
                    MemoryBitsFromatSB.AppendFormat("\t");
                    MemoryBitsFromatSB.AppendFormat(MemoryFromFormat, Label.ToString(), nBit.ToString(), robot.Stations[i].RobotStationNameToUpper);
                }
                else
                {
                    MemoryBitsFromatSB.AppendFormat("\n");
                    MemoryBitsFromatSB.AppendFormat("\t");
                    MemoryBitsFromatSB.AppendFormat(MemoryFromFormat, Label.ToString(), nBit.ToString(), robot.Stations[i].RobotStationNameToUpper);
                }
            }

            for (int i = 0; i < robot.Stations.Count; i++)
            {
                Label++;
                nBit++;

                MemoryBitsFromatSB.AppendFormat("\n");
                MemoryBitsFromatSB.AppendFormat("\t");
                MemoryBitsFromatSB.AppendFormat(MemoryToFormat, Label.ToString(), nBit.ToString(), robot.Stations[i].RobotStationNameToUpper);

                MemoryLabel = Label.ToString();
            }
            return string.Format(Format, OutputFreeFormatSB, OutputLabel, MemoryBitsFromatSB, MemoryLabel).ToString();
        }

        public static string GetUserError()
        {
            return EpsonUserError;
        }

        public static string GeneratePointsFunc(ProgramModel robot)
        {
            int j = robot.Stations.Count;
            int points = robot.Stations.Count;
            string Fromat = "{0}";
            StringBuilder GeneratePoints = new StringBuilder();

            for (int i = 0; i < robot.Stations.Count; i++)
            {
                if (robot.Stations[i].Pallet)
                    points += 3;
                if (robot.Stations[i].Positions != 1)
                    points += robot.Stations[i].Positions - 1;
            }

            GeneratePoints.AppendFormat("ENVT0100,LM:2022/11/23 11:42:41:137,DS:0000011367,CS:02932,MA:C8-D9-D2-AE-3E-19,EI:\n");
            GeneratePoints.AppendFormat("sVersion=\"2.0.0\"\n");
            GeneratePoints.AppendFormat("bDisplayR=False\n");
            GeneratePoints.AppendFormat("bDisplayS=False\n");
            GeneratePoints.AppendFormat("bDisplayT=False\n");
            GeneratePoints.AppendFormat("nNumberOfPoints={0}", points);
            GeneratePoints.AppendFormat("\n");

            for (int i = 0; i < robot.Stations.Count; i++)
            {
                if (robot.Stations[i].Pallet)
                { 
                    GeneratePoints.AppendFormat(EpsonGeneratePoints, i + 1, i, robot.Stations[i].RobotStationName+"P1");
                    GeneratePoints.AppendFormat("\n");
                }

                else if (robot.Stations[i].Positions != 1)
                {
                    GeneratePoints.AppendFormat(EpsonGeneratePoints, i + 1, i, robot.Stations[i].RobotStationName + "1");
                    GeneratePoints.AppendFormat("\n");
                }

                else
                { 
                    GeneratePoints.AppendFormat(EpsonGeneratePoints, i + 1, i, robot.Stations[i].RobotStationName);
                    GeneratePoints.AppendFormat("\n");
                }
            }

            for (int i = 0; i < robot.Stations.Count; i++)
            {
                if (robot.Stations[i].Pallet)
                {
                    GeneratePoints.AppendFormat(EpsonGeneratePoints, j + 1, j, robot.Stations[i].RobotStationName + "P2");
                    GeneratePoints.AppendFormat("\n");
                    GeneratePoints.AppendFormat(EpsonGeneratePoints, j + 2, j + 1, robot.Stations[i].RobotStationName + "P3");
                    GeneratePoints.AppendFormat("\n");
                    GeneratePoints.AppendFormat(EpsonGeneratePoints, j + 3, j + 2, robot.Stations[i].RobotStationName + "P4");
                    GeneratePoints.AppendFormat("\n");
                    j += 3;
                }

                else if (robot.Stations[i].Positions != 1)
                {
                    for (int k = 2; k < robot.Stations[i].Positions + 1; k++)
                    {
                        GeneratePoints.AppendFormat(EpsonGeneratePoints, j + 1, j, robot.Stations[i].RobotStationName + k);
                        GeneratePoints.AppendFormat("\n");
                        j += 1;
                    }
                }
            }
            return string.Format(Fromat, GeneratePoints).ToString();
        }
        #endregion

        #region ////// YAMAHA //////

        // Yamaha zapakira celoten program (kodo + naučene točke + IO oznake) v ENO ".all" datoteko
        // na robota, v nasprotju z Epson/KUKA/ABB, ki uporabljajo več ločenih datotek. Postaje se
        // primarno indeksirajo po svojem zaporednem mestu v Stations (0-based) - homing v
        // *findClosestPoint: išče najbližjo NAUČENO točko po tem istem številčnem indeksu
        // (FOR i = 0 TO N), zato morata seznam postaj in seznam točk ostati usklajena - enaka
        // past kot pri Epsonu. Pallet ni podprt (v referenčnem primeru ni bilo zaslediti), Positions
        // (več fizičnih točk na postajo) pa je podprt, ker se v referenčnem primeru dejansko pojavi.
        //
        // POMEMBNO: ta predloga je bila izpeljana iz ENEGA samega, že prilagojenega produkcijskega
        // projekta (ne iz uradne Yamaha dokumentacije), zato je priporočljivo generirano datoteko
        // pred uporabo na resničnem robotu preveriti/naložiti v Yamaha razvojno okolje. Odseki
        // [PRM]/[RTO] (kalibracija strežnih pogonov robota) se namenoma NE generirajo - ti so
        // vezani na fizično robotsko enoto, ne na razporeditev postaj, in jih je treba združiti iz
        // dejanskega backupa krmilnika.

        // Ime PRIMARNE (naučene) točke postaje. Yamaha ne podpira Pallet, zato je edino razlikovanje
        // večpozicijskost: pri Positions != 1 se primarna točka imenuje "<Postaja>1" (sledijo ji
        // "<Postaja>2".."<Postaja>N"), sicer preprosto "<Postaja>". Uporabljata jo tudi Importer/Updater.
        public static string PrimaryPointName(StationModel station)
        {
            return station.Positions != 1 ? $"{station.RobotStationName}1" : station.RobotStationName;
        }

        public static string GetYamahaMainPgm()
        {
            return YamahaMainPgm + "\n\n";
        }

        public static string GetYamahaRobotPgmHeader(ProgramModel program)
        {
            var positionVars = new StringBuilder();
            for (int i = 0; i < program.Stations.Count; i++)
                positionVars.AppendFormat("{0} = {1}\n", program.Stations[i].RobotStationName, i);

            return string.Format(YamahaRobotPgmHeader, positionVars) + "\n";
        }

        public static string GetYamahaHoming(ProgramModel program)
        {
            var cases = new StringBuilder();
            for (int i = 0; i < program.Stations.Count; i++)
                cases.Append(BuildYamahaHomingCase(program.Stations[i], i));

            return string.Format(YamahaHoming, cases) + "\n";
        }

        // Ena "CASE <index>" veja v *robot_homing: SELECT closestPoint (za dano postajo na danem
        // številčnem indeksu). Uporablja jo tudi YamahaProjectUpdater pri dodajanju nove postaje.
        public static string BuildYamahaHomingCase(StationModel station, int index)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("        CASE {0}\n", index);
            sb.AppendFormat("            PositionFrom = {0}\n", station.RobotStationName);
            sb.Append("            in_station=1\n");
            sb.Append("            '\n");
            return sb.ToString();
        }

        public static string GetYamahaGoFunctions(ProgramModel program)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < program.Stations.Count; i++)
            {
                sb.Append(GetYamahaGoFunction(program, program.Stations[i]));
                sb.Append("\n");
            }
            return sb.ToString();
        }

        // Zgradi celoten "*robot_go<Postaja>:" blok. Notranja veriga "IF PositionFrom = ..." mora
        // poznati VSE postaje programa kot možen izvor ("origin completeness"), zato sprejme celoten
        // program. Uporablja jo tudi YamahaProjectUpdater pri dodajanju nove postaje.
        public static string GetYamahaGoFunction(ProgramModel program, StationModel station)
        {
            string freeBlock = "";
            if (station.StationFreeEnabled)
            {
                freeBlock = "    'if going directly 'on station' then set the station immediately as BUSY (NOT FREE)\n" +
                            "    IF I_ON_STATION=1 THEN\n" +
                            $"        RESET O_{station.RobotStationName}_FREE\n" +
                            "    ENDIF\n" +
                            "    '\n";
            }

            var ifChain = new StringBuilder();
            for (int i = 0; i < program.Stations.Count; i++)
            {
                string s = program.Stations[i].RobotStationName;
                ifChain.AppendFormat(i == 0 ? "    IF PositionFrom = {0} THEN\n" : "    ELSEIF PositionFrom = {0} THEN\n", s);
                ifChain.Append("        MOVE P, pAboveStation, CONT\n");
                ifChain.Append("        '\n");
            }
            ifChain.Append("    ELSE\n");
            ifChain.Append("        'output error\n");
            ifChain.Append("        SET O_PROGRAM_ERROR\n");
            ifChain.Append("        HOLD\n");
            ifChain.Append("        '\n");
            ifChain.Append("    ENDIF");

            return string.Format(YamahaGoFunction, station.RobotStationName, freeBlock, ifChain, PrimaryPointName(station));
        }

        public static string GetYamahaMainTask(ProgramModel program)
        {
            var cases = new StringBuilder();
            for (int i = 0; i < program.Stations.Count; i++)
                cases.Append(BuildYamahaMainTaskCase(program.Stations[i]));

            return string.Format(YamahaMainTask, cases) + "\n";
        }

        // Ena "CASE <Postaja>" veja v *robot_mainTask: SELECT position. Uporablja jo tudi Updater.
        public static string BuildYamahaMainTaskCase(StationModel station)
        {
            string s = station.RobotStationName;
            var sb = new StringBuilder();
            sb.AppendFormat("                CASE {0}\n", s);
            sb.AppendFormat("                    GOSUB *robot_go{0}\n", s);
            sb.Append("                    '\n");
            return sb.ToString();
        }

        public static string GetYamahaMoveOnStation(ProgramModel program)
        {
            var ifChain = new StringBuilder();
            for (int i = 0; i < program.Stations.Count; i++)
                ifChain.Append(BuildYamahaMoveOnStationBranch(program.Stations[i], i == 0));

            return string.Format(YamahaMoveOnStation, ifChain) + "\n";
        }

        // Ena "IF/ELSEIF PositionTo = <Postaja> THEN ..." veja v *robot_moveOnStation:. Za
        // večpozicijske postaje (Positions != 1) vsebuje še notranji "additional_pos" izbor.
        // Uporablja jo tudi Updater (tam vedno kot ELSEIF, saj Home vedno obstaja pred njo).
        public static string BuildYamahaMoveOnStationBranch(StationModel station, bool first)
        {
            string s = station.RobotStationName;
            var ifChain = new StringBuilder();
            ifChain.AppendFormat(first ? "        IF PositionTo = {0} THEN\n" : "        ELSEIF PositionTo = {0} THEN\n", s);

            if (station.Positions != 1)
            {
                ifChain.Append("            'define FINAL point in station\n");
                for (int j = 1; j <= station.Positions; j++)
                {
                    ifChain.AppendFormat(j == 1 ? "            IF additional_pos={0} THEN\n" : "            ELSEIF additional_pos={0} THEN\n", j);
                    ifChain.AppendFormat("                p{0}Final = p{0}{1}\n", s, j);
                }
                ifChain.Append("            ELSE\n");
                ifChain.Append("                SET O_PROGRAM_ERROR\n");
                ifChain.Append("                HOLD\n");
                ifChain.Append("            ENDIF\n");
                ifChain.Append("            '\n");
            }

            if (station.StationFreeEnabled)
                ifChain.AppendFormat("            RESET O_{0}_FREE\n", s);

            ifChain.Append("            GOSUB *robot_fullWorkingSpeed\n");
            string finalPoint = station.Positions != 1 ? $"p{s}Final" : $"p{PrimaryPointName(station)}";
            ifChain.AppendFormat("            pAboveStation = {0}\n", finalPoint);
            ifChain.Append("            LOC3(pAboveStation) = ROBOT_MAX_Z\n");
            ifChain.Append("            MOVE P, pAboveStation, CONT\n");
            ifChain.Append("            '\n");
            ifChain.Append("            CALL *robot_slowWorkingSpeed (ROBOT_APPROACH_SPEED%, ROBOT_APPROACH_ACCEL%, ROBOT_APPROACH_DECEL%, power_mode)\n");
            ifChain.AppendFormat("            MOVE P, {0}\n", finalPoint);
            ifChain.Append("            '\n");
            return ifChain.ToString();
        }

        public static string GetYamahaMoveAway(ProgramModel program)
        {
            var ifChain = new StringBuilder();
            for (int i = 0; i < program.Stations.Count; i++)
                ifChain.Append(BuildYamahaMoveAwayBranch(program.Stations[i], i == 0));

            return string.Format(YamahaMoveAway, ifChain) + "\n";
        }

        // Ena "IF/ELSEIF PositionFrom = <Postaja> THEN ..." veja v *robot_moveAway:. Uporablja jo
        // tudi Updater (tam vedno kot ELSEIF).
        public static string BuildYamahaMoveAwayBranch(StationModel station, bool first)
        {
            string s = station.RobotStationName;
            var ifChain = new StringBuilder();
            ifChain.AppendFormat(first ? "        IF PositionFrom = {0} THEN\n" : "        ELSEIF PositionFrom = {0} THEN\n", s);
            ifChain.Append("            GOSUB *robot_fullWorkingSpeed\n");
            ifChain.Append("            LOC3 (pAboveStation) = ROBOT_MAX_Z\n");
            ifChain.Append("            MOVE P, pAboveStation, CONT\n");
            ifChain.Append("            '\n");
            if (station.StationFreeEnabled)
            {
                ifChain.Append("            'turn ON station free signal\n");
                ifChain.AppendFormat("            SET O_{0}_FREE\n", s);
                ifChain.Append("            '\n");
            }
            return ifChain.ToString();
        }

        public static string GetYamahaCommonPgm(ProgramModel program)
        {
            var busy = new StringBuilder();
            var free = new StringBuilder();
            foreach (var station in program.Stations)
            {
                if (!station.StationFreeEnabled) continue;
                busy.AppendFormat("    RESET O_{0}_FREE\n", station.RobotStationName);
                free.AppendFormat("    SET O_{0}_FREE\n", station.RobotStationName);
            }

            int totalPoints = program.Stations.Count + program.Stations.Sum(s => s.Positions != 1 ? s.Positions - 1 : 0);
            int homingBound = totalPoints - 1;

            return string.Format(YamahaCommonPgm, busy, free, homingBound) + "\n";
        }

        public static string GetYamahaPoints(ProgramModel program)
        {
            const string placeholderCoords = "0.000 0.000 0.000 0.000 0.000 0.000 1 0 0";
            var pnt = new StringBuilder();
            var pcm = new StringBuilder();
            var pnm = new StringBuilder();

            int index = 0;
            foreach (var station in program.Stations)
            {
                pnt.AppendFormat("P{0}={1}\n", index, placeholderCoords);
                pcm.AppendFormat("PC{0}=ni ustimano !!!\n", index);
                pnm.AppendFormat("PN{0}=p{1}\n", index, PrimaryPointName(station));
                index++;
            }

            foreach (var station in program.Stations)
            {
                if (station.Positions == 1) continue;
                for (int j = 2; j <= station.Positions; j++)
                {
                    pnt.AppendFormat("P{0}={1}\n", index, placeholderCoords);
                    pcm.AppendFormat("PC{0}=ni ustimano !!!\n", index);
                    pnm.AppendFormat("PN{0}=p{1}{2}\n", index, station.RobotStationName, j);
                    index++;
                }
            }

            return string.Format(YamahaPoints, pnt.ToString().TrimEnd('\n'), pcm.ToString().TrimEnd('\n'), pnm.ToString().TrimEnd('\n')) + "\n";
        }

        public static string GetYamahaIO(ProgramModel program)
        {
            var freeSignals = new StringBuilder();
            int k = 0;
            foreach (var station in program.Stations)
            {
                if (!station.StationFreeEnabled) continue;
                freeSignals.AppendFormat("SONM4({0})=O_{1}_FREE\n", k, station.RobotStationName);
                k++;
            }

            return string.Format(YamahaIO, freeSignals.ToString().TrimEnd('\n')) + "\n";
        }

        #endregion
    }
}

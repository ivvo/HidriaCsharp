using TemplateGenerator.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Linq;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using System.Threading;
using System.Globalization;
using TemplateGenerator.Core.ViewModels;

namespace TemplateGenerator.Core.Classes
{
    class Template
    {
        public static bool FormatsLoaded = false;
        //EPSON//
        private static readonly string EpsonHeaderFormatEng = "";
        private static readonly string EpsonMainFuncFormatEng = "";
        private static readonly string EpsonErrorHandlingFuncFormatEng = "";
        private static readonly string EpsonEstopHandlingFuncFormatEng = "";
        private static readonly string EpsonInitFuncFormatEng = "";
        private static readonly string EpsonHomingFuncFormatEng = "";
        private static readonly string EpsonOperationFuncFormatEng = "";
        private static readonly string EpsonTestFuncFormatEng = "";
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

        // Yamaha //
        private static readonly string YamahaMAINprog = "";
        private static readonly string YamahaROBOTprogHeader = "";
        private static readonly string YamahaROBOTprogInit = ""; 
        private static readonly string YamahaROBOTprogHoming = "";
        private static readonly string YamahaROBOTprogMainTask = "";
        private static readonly string YamahaROBOTprogTest = "";
        private static readonly string YamahaROBOTprogGoHome = "";
        private static readonly string YamahaROBOTprogGoStation = ""; 
        private static readonly string YamahaROBOTprogMoveOnStation = "";
        private static readonly string YamahaROBOTprogMoveAway = "";
        private static readonly string YamahaCOMMONprog = "";
        private static readonly string YamahaPOINTSfile = "";
        private static readonly string YamahaIOfile = "";

        // Kawasaki //
        private static readonly string KawasakiDOCUMENTATION = "";
        private static readonly string KawasakiTESTING = "";
        private static readonly string KawasakiMAIN_Main = "";
        private static readonly string KawasakiMAIN_Init = "";
        private static readonly string KawasakiMAIN_Homing = "";
        private static readonly string KawasakiMAIN_MainTask = "";
        private static readonly string KawasakiMOVEMENTS_MoveAway = "";
        private static readonly string KawasakiMOVEMENTS_MoveOnStation = "";
        private static readonly string KawasakiMOVEMENTS_GoHome = "";
        private static readonly string KawasakiMOVEMENTS_GoStation = "";
        private static readonly string KawasakiFUNCTIONS = "";
        private static readonly string KawasakiCALIBRATION = "";
        private static readonly string KawasakiPOINTS = "";
        private static readonly string KawasakiIO = "";
        private static readonly string KawasakiIOhandling = "";
        private static readonly string KawasakiCOMMENT = "";



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
                EpsonTestFuncFormatEng = File.ReadAllText("./Templates/Epson/TestFuncEng.txt");
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

                // Yamaha //
                YamahaMAINprog = File.ReadAllText("./Templates/Yamaha/YamahaMAINprog.txt");
                YamahaROBOTprogHeader = File.ReadAllText("./Templates/Yamaha/YamahaROBOTprogHeader.txt");
                YamahaROBOTprogInit = File.ReadAllText("./Templates/Yamaha/YamahaROBOTprogInit.txt");
                YamahaROBOTprogHoming = File.ReadAllText("./Templates/Yamaha/YamahaROBOTprogHoming.txt");
                YamahaROBOTprogMainTask = File.ReadAllText("./Templates/Yamaha/YamahaROBOTprogMainTask.txt");
                YamahaROBOTprogTest = File.ReadAllText("./Templates/Yamaha/YamahaROBOTprogTest.txt");
                YamahaROBOTprogGoHome = File.ReadAllText("./Templates/Yamaha/YamahaROBOTprogGoHome.txt");
                YamahaROBOTprogGoStation = File.ReadAllText("./Templates/Yamaha/YamahaROBOTprogGoStation.txt");
                YamahaROBOTprogMoveOnStation = File.ReadAllText("./Templates/Yamaha/YamahaROBOTprogMoveOnStation.txt");
                YamahaROBOTprogMoveAway = File.ReadAllText("./Templates/Yamaha/YamahaROBOTprogMoveAway.txt");
                YamahaCOMMONprog = File.ReadAllText("./Templates/Yamaha/YamahaCOMMONprog.txt");
                YamahaPOINTSfile = File.ReadAllText("./Templates/Yamaha/YamahaPOINTSfile.txt");
                YamahaIOfile = File.ReadAllText("./Templates/Yamaha/YamahaIOfile.txt");

                // Kawasaki //
                KawasakiDOCUMENTATION = File.ReadAllText("./Templates/Kawasaki/KawasakiDOCUMENTATION.txt");
                KawasakiTESTING = File.ReadAllText("./Templates/Kawasaki/KawasakiTESTING.txt");
                KawasakiMAIN_Main= File.ReadAllText("./Templates/Kawasaki/KawasakiMAIN-Main.txt");
                KawasakiMAIN_Init = File.ReadAllText("./Templates/Kawasaki/KawasakiMAIN-Init.txt");
                KawasakiMAIN_Homing = File.ReadAllText("./Templates/Kawasaki/KawasakiMAIN-Homing.txt");
                KawasakiMAIN_MainTask = File.ReadAllText("./Templates/Kawasaki/KawasakiMAIN-MainTask.txt");
                KawasakiMOVEMENTS_MoveAway = File.ReadAllText("./Templates/Kawasaki/KawasakiMOVEMENTS-MoveAway.txt");
                KawasakiMOVEMENTS_MoveOnStation = File.ReadAllText("./Templates/Kawasaki/KawasakiMOVEMENTS-MoveOnStation.txt");
                KawasakiMOVEMENTS_GoHome = File.ReadAllText("./Templates/Kawasaki/KawasakiMOVEMENTS-GoHome.txt");
                KawasakiMOVEMENTS_GoStation = File.ReadAllText("./Templates/Kawasaki/KawasakiMOVEMENTS-GoStation.txt");
                KawasakiFUNCTIONS = File.ReadAllText("./Templates/Kawasaki/KawasakiFUNCTIONS.txt");
                KawasakiCALIBRATION = File.ReadAllText("./Templates/Kawasaki/KawasakiCALIBRATION.txt");
                KawasakiPOINTS = File.ReadAllText("./Templates/Kawasaki/KawasakiPOINTS.txt");
                KawasakiIO = File.ReadAllText("./Templates/Kawasaki/Kawasaki-IO.txt");
                KawasakiIOhandling = File.ReadAllText("./Templates/Kawasaki/Kawasaki-IOhandling.txt");
                KawasakiCOMMENT = File.ReadAllText("./Templates/Kawasaki/KawasakiCOMMENT______().txt");

            }
            catch
            {
                // Formats not loaded
                FormatsLoaded = false;
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
            int palletNo = 0;

            for ( int i = 0; i < program.Stations.Count; i++)
            {
                if (program.Stations[i].Pallet)
                {
                    FormatArgument.AppendFormat($"#define {program.Stations[i].RobotStationName} {palletNo}\n");
                    palletNo++;
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

        public static string GetHomingFunc(ProgramModel robot, bool simulation)
        {
            //int pointNo = 0;
            int TotalPoints = 0;
            int pointNo = 0;// robot.Stations.Count;
            string Format = EpsonHomingFuncFormatEng;
            StringBuilder FormatArgument = new StringBuilder();
            StringBuilder FormatArgument2 = new StringBuilder();
            StringBuilder IfSimulation = new StringBuilder();

            for (int i = 0; i < robot.Stations.Count; i++)
            {
                if (i == 0)
                {
                    FormatArgument.AppendFormat("\t\t\t\tCase {0}\n", i);
                    FormatArgument.AppendFormat("\t\t\t\t\tMemOn ROBOT_FROM_{0}\n\n", robot.Stations[i].RobotStationNameToUpper);
                    pointNo++;
                }

                else
                {
                    if (robot.Stations[i].Pallet)
                    {
                        FormatArgument.AppendFormat("\t\t\t\tCase {0} Or {1} Or {2} Or {3}\n", pointNo, pointNo+1, pointNo+2, pointNo+3);

                        FormatArgument.AppendFormat("\t\t\t\t\tMemOn ROBOT_FROM_{0}\n", robot.Stations[i].RobotStationNameToUpper); 
                        FormatArgument.AppendFormat("\t\t\t\t\trobot_inStation = True\n\n");
                        pointNo += 4;
                    }


                    else if (robot.Stations[i].Positions > 1)
                    {
                        FormatArgument2.AppendFormat("{0}", pointNo);
                        pointNo++;

                        for (int j = 0; j < robot.Stations[i].Positions - 1; j++)
                        {
                            FormatArgument2.AppendFormat(" Or {0}", pointNo);
                            pointNo++;
                        }

                        FormatArgument.AppendFormat("\t\t\t\tCase {0}\n", FormatArgument2);
                        FormatArgument2.Length = 0;

                        FormatArgument.AppendFormat("\t\t\t\t\tMemOn ROBOT_FROM_{0}\n", robot.Stations[i].RobotStationNameToUpper);
                        FormatArgument.AppendFormat("\t\t\t\t\trobot_inStation = True\n\n");
                        //pointNo ++;
                    }
                    else
                    {
                        FormatArgument.AppendFormat("\t\t\t\tCase {0}\n", pointNo);
                        FormatArgument.AppendFormat("\t\t\t\t\tMemOn ROBOT_FROM_{0}\n", robot.Stations[i].RobotStationNameToUpper);
                        FormatArgument.AppendFormat("\t\t\t\t\trobot_inStation = True\n\n");
                        pointNo++;
                    }
                    
                }

                TotalPoints += robot.Stations[i].Positions;
            }

            if (simulation)
            {
                IfSimulation.AppendFormat("Or 1 = 1 ");
            }


            //for (int i = 0; i < robot.Stations.Count; i++)
            //{
            //    if (i == 0)
            //    {
            //        FormatArgument.AppendFormat("\t\t\t\tCase {0}\n", i);
            //        FormatArgument.AppendFormat("\t\t\t\t\tMemOn ROBOT_FROM_{0}\n\n", robot.Stations[i].RobotStationNameToUpper);
            //    }

            //    else
            //    {
            //        FormatArgument.AppendFormat("\t\t\t\tCase {0}\n", i);
            //        FormatArgument.AppendFormat("\t\t\t\t\tMemOn ROBOT_FROM_{0}\n", robot.Stations[i].RobotStationNameToUpper);
            //        FormatArgument.AppendFormat("\t\t\t\t\trobot_inStation = True\n\n");
            //    }

            //    TotalPoints += robot.Stations[i].Positions;
            //}

            //int pointNo = robot.Stations.Count;

            //for (int i = 0; i < robot.Stations.Count; i++)
            //{
            //    if (robot.Stations[i].Positions > 1)
            //    {
            //        FormatArgument2.AppendFormat("{0}", pointNo);
            //        pointNo++;

            //        for (int j = 1; j < robot.Stations[i].Positions - 1; j++)
            //        {
            //            FormatArgument2.AppendFormat(" Or {0}", pointNo);
            //            pointNo++;
            //        }

            //        FormatArgument.AppendFormat("\t\t\t\tCase {0}\n", FormatArgument2);
            //        FormatArgument2.Length = 0;

            //        FormatArgument.AppendFormat("\t\t\t\t\tMemOn ROBOT_FROM_{0}\n", robot.Stations[i].RobotStationNameToUpper);
            //        FormatArgument.AppendFormat("\t\t\t\t\trobot_inStation = True\n\n");
            //        //pointNo ++;
            //    }

            //}

            FormatArgument.Length--;
            //return string.Format(Format, robot.ProgramNameLowerChar, robot.ProgramNameUpperChar, robot.RobotNumber, robot.Stations.Count - 1, FormatArgument).ToString() + "\n\n";
            return string.Format(Format, robot.ProgramNameLowerChar, robot.ProgramNameUpperChar, robot.RobotNumber, TotalPoints-1, FormatArgument, IfSimulation).ToString() + "\n\n";
        }

        public static string GetTestFunc(ProgramModel program)
        {
            string Format = EpsonTestFuncFormatEng;
            StringBuilder FormatArgument = new StringBuilder();
            StringBuilder FormatArgument2 = new StringBuilder();

            for (int i = 1; i < program.Stations.Count; i++)
            {

                FormatArgument.AppendFormat("\t\tCall {0}_go{1}()\n", program.ProgramNameLowerChar, program.Stations[i].RobotStationName);
                FormatArgument.AppendFormat("\t\tWait gripper\n\n");

                FormatArgument2.AppendFormat("\t\t\tCase {0}\n", (i));
                FormatArgument2.AppendFormat("\t\t\t\tCall {0}_go{1}()\n", program.ProgramNameLowerChar, program.Stations[i].RobotStationName);

            }

            FormatArgument.Length--;
            FormatArgument2.Length--;
            return string.Format(Format, program.ProgramNameLowerChar, program.Stations.Count-1, FormatArgument, FormatArgument2).ToString() + "\n\n";
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
            StringBuilder FormatArgumentFinalPoint = new StringBuilder();
            StringBuilder FormatArgumentAboveStation = new StringBuilder();
            string Movement = EpsonMovementFuncFormatEng;

            string Format = "{0}";


            for (int i = 0; i < program.Stations.Count; i++)
            {

                FormatArgumentPoints.Add(new StringBuilder());
                if (program.Stations[i].Pallet)
                {
                    FormatArgumentPoints[i].AppendFormat("\t\tGo Pallet ({0}, additionalPos) :Z({1}_MAX_Z_HEIGHT) \n\n", program.Stations[i].RobotStationName, program.ProgramNameUpperChar);
                    //FormatArgumentAboveStation.AppendFormat("P1");
                }
                    
                else if (1 != program.Stations[i].Positions)
                {
                    //FormatArgumentPoints[i].AppendFormat("\t\tCall go{0}MaxZHeight \n\n", program.Stations[i].RobotStationName, program.ProgramNameUpperChar);
                    FormatArgumentPoints[i].AppendFormat("\t\tGo pAboveStation \n\n");
                }

                else
                {
                    //FormatArgumentPoints[i].AppendFormat("\t\tGo p{0} :Z({1}_MAX_Z_HEIGHT) \n\n", program.Stations[i].RobotStationName, program.ProgramNameUpperChar);
                    FormatArgumentPoints[i].AppendFormat("\t\tGo pAboveStation \n\n");
                }

                
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

            //for (int i = 0; i < program.Stations.Count; i++)
            //{
            //    string Destinations = FormatArgumentDestinations.ToString();
            //    FormatArgumentDestinationSimpleProgram.AppendFormat(Destinations, FormatArgumentPoints[i]);
            //}

            for (int i = 0; i < program.Stations.Count; i++)
            {

                if (program.Stations[i].StationFreeEnabled)
                {
                    FormatArgumentStationFree.AppendFormat("\n\tIf Sw(ROBOT_I_ON_STATION) = On Then\n");
                    FormatArgumentStationFree.AppendFormat("\t\tOff {0}_O_{1}_FREE\n", program.ProgramNameUpperChar, program.Stations[i].RobotStationNameToUpper);
                    FormatArgumentStationFree.AppendFormat("\tEndIf\n");
                }

                if (program.Stations[i].Positions != 1)
                {
                    FormatArgumentFinalPoint.AppendFormat("\n");
                    FormatArgumentFinalPoint.AppendFormat("\t' define final point in station\n");
                    FormatArgumentFinalPoint.AppendFormat("\tSelect additionalPos\n");
                    for (int j = 1; j <= program.Stations[i].Positions; j++)
                    {
                        FormatArgumentFinalPoint.AppendFormat("\t\tCase {0}\n", j);
                        FormatArgumentFinalPoint.AppendFormat("\t\t\tp{0}Final = p{0}{1}\n", program.Stations[i].RobotStationName, j);
                    }
                    FormatArgumentFinalPoint.AppendFormat("\t\tDefault\n");
                    FormatArgumentFinalPoint.AppendFormat("\t\t\tError POSITION_ERROR\n");
                    FormatArgumentFinalPoint.AppendFormat("\tSend\n");

                    FormatArgumentAboveStation.AppendFormat("1");
                }

                if (program.Stations[i].Pallet)
                {
                    FormatArgumentAboveStation.AppendFormat("P1");
                }

                string Destinations = FormatArgumentDestinations.ToString();
                FormatArgumentDestinationSimpleProgram.Clear();
                FormatArgumentDestinationSimpleProgram.AppendFormat(Destinations, FormatArgumentPoints[i]);
                FormatArgument.AppendFormat(Movement + "\n\n", program.ProgramNameLowerChar, program.Stations[i].RobotStationName, program.ProgramNameUpperChar, program.Stations[i].RobotStationNameToUpper, FormatArgumentDestinationSimpleProgram, FormatArgumentStationFree, FormatArgumentFinalPoint, FormatArgumentAboveStation);
                FormatArgumentStationFree.Clear();
                FormatArgumentFinalPoint.Clear();
                FormatArgumentAboveStation.Clear();
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
                        FormatArgumentIfStatements.AppendFormat("\t\t\tIf CZ(CurPos) > (CZ(p{0}Final) + {1}_STATION_Z_OFFSET) = True Then\n", program.Stations[i].RobotStationName, program.ProgramNameUpperChar);
                        FormatArgumentIfStatements.AppendFormat("\t\t\t\tGo p{0}Final +Z({1}_STATION_Z_OFFSET)\n", program.Stations[i].RobotStationName, program.ProgramNameUpperChar);
                        FormatArgumentIfStatements.AppendFormat("\t\t\tEndIf\n");
                        FormatArgumentIfStatements.AppendFormat("\t\t\t' Go slower working mode\n");
                        FormatArgumentIfStatements.AppendFormat("\t\t\tCall {0}_slowerWorkingMode({1}_SLOWER_SPEED, {1}_APPROACH_ACCEL, {1}_APPROACH_DECEL)\n", program.ProgramNameLowerChar, program.ProgramNameUpperChar);
                        FormatArgumentIfStatements.AppendFormat("\t\t\tGo p{0}Final\n\n", program.Stations[i].RobotStationName);
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

                    if (robot.Stations[i].Pallet)
                    {
                        FormatArgumentIfStatements.AppendFormat("\t\t\tIf CZ(CurPos) + {0}_STATION_Z_OFFSET < {0}_MAX_Z_HEIGHT = True And CZ(CurPos) < CZ(p{1}P1) + ROBOT_STATION_Z_OFFSET = True Then\n", robot.ProgramNameUpperChar, robot.Stations[i].RobotStationName);
                    }
                    else if (robot.Stations[i].Positions == 1)
                    {
                        FormatArgumentIfStatements.AppendFormat("\t\t\tIf CZ(CurPos) + {0}_STATION_Z_OFFSET < {0}_MAX_Z_HEIGHT = True And CZ(CurPos) < CZ(p{1}) + ROBOT_STATION_Z_OFFSET = True Then\n", robot.ProgramNameUpperChar, robot.Stations[i].RobotStationName);
                    }
                    else 
                    {
                        FormatArgumentIfStatements.AppendFormat("\t\t\tIf CZ(CurPos) + {0}_STATION_Z_OFFSET < {0}_MAX_Z_HEIGHT = True And CZ(CurPos) < CZ(p{1}1) + ROBOT_STATION_Z_OFFSET = True Then\n", robot.ProgramNameUpperChar, robot.Stations[i].RobotStationName);
                    }
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
            //int j = robot.Stations.Count;
            int points = robot.Stations.Count;
            int pointNo = 0;
            string Fromat = "{0}";
            StringBuilder GeneratePoints = new StringBuilder();
            NumberFormatInfo dot = new NumberFormatInfo();
            dot.NumberDecimalSeparator = ".";

            for (int i = 0; i < robot.Stations.Count; i++)
            {
                if (robot.Stations[i].Pallet)
                    points += 3;
                if (robot.Stations[i].Positions != 1)
                    points += robot.Stations[i].Positions;
            }
            points += 1; //one for pAboveStation

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
                    GeneratePoints.AppendFormat(EpsonGeneratePoints, pointNo + 1, pointNo, robot.Stations[i].RobotStationName + "P1", robot.Stations[i].RobotStationComment, robot.Stations[i].Xcord.ToString(dot), robot.Stations[i].Ycord.ToString(dot), robot.Stations[i].Zcord.ToString(dot));
                    GeneratePoints.AppendFormat("\n"); 
                    GeneratePoints.AppendFormat(EpsonGeneratePoints, pointNo + 2, pointNo + 1, robot.Stations[i].RobotStationName + "P2", robot.Stations[i].RobotStationComment, robot.Stations[i].Xcord.ToString(dot), robot.Stations[i].Ycord.ToString(dot), robot.Stations[i].Zcord.ToString(dot));
                    GeneratePoints.AppendFormat("\n");
                    GeneratePoints.AppendFormat(EpsonGeneratePoints, pointNo + 3, pointNo + 2, robot.Stations[i].RobotStationName + "P3", robot.Stations[i].RobotStationComment, robot.Stations[i].Xcord.ToString(dot), robot.Stations[i].Ycord.ToString(dot), robot.Stations[i].Zcord.ToString(dot));
                    GeneratePoints.AppendFormat("\n");
                    GeneratePoints.AppendFormat(EpsonGeneratePoints, pointNo + 4, pointNo + 3, robot.Stations[i].RobotStationName + "P4", robot.Stations[i].RobotStationComment, robot.Stations[i].Xcord.ToString(dot), robot.Stations[i].Ycord.ToString(dot), robot.Stations[i].Zcord.ToString(dot));
                    GeneratePoints.AppendFormat("\n");
                    pointNo += 4;
                }

                else if (robot.Stations[i].Positions != 1)
                {
                    //GeneratePoints.AppendFormat(EpsonGeneratePoints, point + 1, point, robot.Stations[i].RobotStationName + "1", robot.Stations[i].RobotStationComment, robot.Stations[i].Xcord.ToString(dot), robot.Stations[i].Ycord.ToString(dot), robot.Stations[i].Zcord.ToString(dot));
                    //GeneratePoints.AppendFormat("\n");
                    //point++;

                    for (int k = 1; k < robot.Stations[i].Positions + 1; k++)
                    {
                        GeneratePoints.AppendFormat(EpsonGeneratePoints, pointNo + 1, pointNo, robot.Stations[i].RobotStationName + k, robot.Stations[i].RobotStationComment, robot.Stations[i].Xcord.ToString(dot), robot.Stations[i].Ycord.ToString(dot), robot.Stations[i].Zcord.ToString(dot));
                        GeneratePoints.AppendFormat("\n");
                        pointNo++;
                    }
                }

                else
                {
                    GeneratePoints.AppendFormat(EpsonGeneratePoints, pointNo + 1, pointNo, robot.Stations[i].RobotStationName, robot.Stations[i].RobotStationComment, robot.Stations[i].Xcord.ToString(dot), robot.Stations[i].Ycord.ToString(dot), robot.Stations[i].Zcord.ToString(dot));
                    GeneratePoints.AppendFormat("\n");
                    pointNo++;
                }
            }




            //for (int i = 0; i < robot.Stations.Count; i++)
            //{
            //    if (robot.Stations[i].Pallet)
            //    { 
            //        GeneratePoints.AppendFormat(EpsonGeneratePoints, i + 1, i, robot.Stations[i].RobotStationName+"P1", robot.Stations[i].RobotStationComment, robot.Stations[i].Xcord.ToString(dot), robot.Stations[i].Ycord.ToString(dot), robot.Stations[i].Zcord.ToString(dot));
            //        GeneratePoints.AppendFormat("\n");
            //    }

            //    else if (robot.Stations[i].Positions != 1)
            //    {
            //        GeneratePoints.AppendFormat(EpsonGeneratePoints, i + 1, i, robot.Stations[i].RobotStationName + "1", robot.Stations[i].RobotStationComment, robot.Stations[i].Xcord.ToString(dot), robot.Stations[i].Ycord.ToString(dot), robot.Stations[i].Zcord.ToString(dot));
            //        GeneratePoints.AppendFormat("\n");
            //    }

            //    else
            //    { 
            //        GeneratePoints.AppendFormat(EpsonGeneratePoints, i + 1, i, robot.Stations[i].RobotStationName, robot.Stations[i].RobotStationComment, robot.Stations[i].Xcord.ToString(dot), robot.Stations[i].Ycord.ToString(dot), robot.Stations[i].Zcord.ToString(dot));
            //        GeneratePoints.AppendFormat("\n");
            //    }
            //}

            //for (int i = 0; i < robot.Stations.Count; i++)
            //{
            //    if (robot.Stations[i].Pallet)
            //    {
            //        GeneratePoints.AppendFormat(EpsonGeneratePoints, j + 1, j, robot.Stations[i].RobotStationName + "P2", robot.Stations[i].RobotStationComment, robot.Stations[i].Xcord.ToString(dot), robot.Stations[i].Ycord.ToString(dot), robot.Stations[i].Zcord.ToString(dot));
            //        GeneratePoints.AppendFormat("\n");
            //        GeneratePoints.AppendFormat(EpsonGeneratePoints, j + 2, j + 1, robot.Stations[i].RobotStationName + "P3", robot.Stations[i].RobotStationComment, robot.Stations[i].Xcord.ToString(dot), robot.Stations[i].Ycord.ToString(dot), robot.Stations[i].Zcord.ToString(dot));
            //        GeneratePoints.AppendFormat("\n");
            //        GeneratePoints.AppendFormat(EpsonGeneratePoints, j + 3, j + 2, robot.Stations[i].RobotStationName + "P4", robot.Stations[i].RobotStationComment, robot.Stations[i].Xcord.ToString(dot), robot.Stations[i].Ycord.ToString(dot), robot.Stations[i].Zcord.ToString(dot));
            //        GeneratePoints.AppendFormat("\n");
            //        j += 3;
            //    }

            //    else if (robot.Stations[i].Positions != 1)
            //    {
            //        for (int k = 2; k < robot.Stations[i].Positions + 1; k++)
            //        {
            //            GeneratePoints.AppendFormat(EpsonGeneratePoints, j + 1, j, robot.Stations[i].RobotStationName + k, robot.Stations[i].RobotStationComment, robot.Stations[i].Xcord.ToString(dot), robot.Stations[i].Ycord.ToString(dot), robot.Stations[i].Zcord.ToString(dot));
            //            GeneratePoints.AppendFormat("\n");
            //            j += 1;
            //        }
            //    }

            //}

            //GeneratePoints.AppendFormat(EpsonGeneratePoints, j + 1, j, "", "", "", "", "");
            //GeneratePoints.AppendFormat("\n");
            //j += 1;

            for (int i = 0; i < robot.Stations.Count; i++)
            {
                if (robot.Stations[i].Positions != 1)
                {
                    GeneratePoints.AppendFormat(EpsonGeneratePoints, pointNo + 1, pointNo + 1, robot.Stations[i].RobotStationName + "Final", "variable point", robot.Stations[i].Xcord.ToString(dot), robot.Stations[i].Ycord.ToString(dot), robot.Stations[i].Zcord.ToString(dot));
                    GeneratePoints.AppendFormat("\n");
                    pointNo += 1;
                }
            }

            GeneratePoints.AppendFormat(EpsonGeneratePoints, pointNo + 1, pointNo + 1, "AboveStation", "variable point", "0", "0", "0");
            GeneratePoints.AppendFormat("\n");
            pointNo += 1;

            return string.Format(Fromat, GeneratePoints).ToString();
        }
        #endregion


        #region///// Yamaha /////


        ////Generate MAIN program
        public static string GetYamahaMainProg(ObservableCollection<ProgramModel> robot)
        {
            string Format = YamahaMAINprog;
            //StringBuilder FormatArgument = new StringBuilder();
            int i = 0;  //Če je več robotov, ta del zbriši

            //// Go through all robots
            //for (int i = 0; i < robot.Count; i++)
            //{
            string RobotNameUpperChar = robot[i].ProgramName.ToUpper();
            //    if (i == robot.Count - 1)
                    //FormatArgument.AppendFormat("SWI <{0}>", RobotNameUpperChar);
            //    else
            //        FormatArgument.AppendFormat("SWI {0}_init\n", RobotNameLowerChar);
            //}
            return string.Format(Format, RobotNameUpperChar).ToString() + "\n";
        }

        //// Generate ROBOT program
        // Expand ROBOT program with Header
        public static string GetYamahaRobotProgHeader(ProgramModel robot)
        {
            string Format = YamahaROBOTprogHeader;
            StringBuilder FormatArgument = new StringBuilder();

            for (int i = 0; i < robot.Stations.Count; i++)
            {
                
                FormatArgument.AppendFormat("{0} = {1}\n", robot.Stations[i].RobotStationName, i);

            }
            FormatArgument.Length--;
            return string.Format(Format, robot.ProgramNameUpperChar, FormatArgument, robot.RobotNumber+1).ToString();
        }

        // Expand ROBOT program with Init
        public static string GetYamahaRobotProgInit(ProgramModel robot)
        {
            string Format = YamahaROBOTprogInit;
            StringBuilder FormatArgument = new StringBuilder();

            int i = 0;  //Če je več robotov, ta del zbriši

            //// Go through all robots
            //for (int i = 0; i < robot.Count; i++)
            //{
            string RobotNameLowerChar = robot.ProgramName.ToLower();
            //    if (i == robot.Count - 1)
            //FormatArgument.AppendFormat("SWI <{0}>", RobotNameUpperChar);
            //    else
            //        FormatArgument.AppendFormat("SWI {0}_init\n", RobotNameLowerChar);
            //}
            return string.Format(Format, RobotNameLowerChar, i+1).ToString();
        }

        // Expand ROBOT program with Homing
        public static string GetYamahaRobotProgHoming(ProgramModel robot)
        {
            int pointNo = robot.Stations.Count; ;
            string Format = YamahaROBOTprogHoming;
            StringBuilder FormatArgument = new StringBuilder();
            StringBuilder FormatArgument2 = new StringBuilder();


            for (int i = 0; i < robot.Stations.Count; i++)
            {
                if (robot.Stations[i].Positions > 1)
                {
                    
                    FormatArgument2.AppendFormat("{0}", i);
                    //pointNo++;

                    for (int j = 1; j < robot.Stations[i].Positions; j++)
                    {
                        FormatArgument2.AppendFormat(",{0}", pointNo);
                        pointNo++;
                    }

                    FormatArgument.AppendFormat("\t\tCASE {0}\n", FormatArgument2);
                    FormatArgument2.Length = 0;

                    FormatArgument.AppendFormat("\t\t\tPositionFrom = {0}\n", robot.Stations[i].RobotStationName);
                    FormatArgument.AppendFormat("\t\t\tin_station=1\n");
                    FormatArgument.AppendFormat("\t\t\t'\n");
                    //pointNo ++;
                }
                else
                {
                    FormatArgument.AppendFormat("\t\tCASE {0}\n", i);
                    FormatArgument.AppendFormat("\t\t\tPositionFrom = {0}\n", robot.Stations[i].RobotStationName);
                    FormatArgument.AppendFormat("\t\t\tin_station=1\n");
                    FormatArgument.AppendFormat("\t\t\t'\n");
                    //pointNo++;
                }

            }


            //int pointNo = 0;
            //string Format = YamahaROBOTprogHoming;
            //StringBuilder FormatArgument = new StringBuilder();
            //StringBuilder FormatArgument2 = new StringBuilder();

            //for (int i = 0; i < robot.Stations.Count; i++)
            //{

            //    FormatArgument.AppendFormat("\t\tCASE {0}\n", i);
            //    FormatArgument.AppendFormat("\t\t\tPositionFrom = {0}\n", robot.Stations[i].RobotStationName);
            //    FormatArgument.AppendFormat("\t\t\tin_station=1\n");
            //    FormatArgument.AppendFormat("\t\t\t'\n");
            //    pointNo++;

            //}

            //for (int i = 0; i < robot.Stations.Count; i++)
            //{
            //    if (robot.Stations[i].Positions > 1)
            //    {
            //        FormatArgument2.AppendFormat("{0}", pointNo);
            //        pointNo++;

            //        for (int j = 1; j < robot.Stations[i].Positions-1; j++)
            //        {
            //            FormatArgument2.AppendFormat(",{0}", pointNo);
            //            pointNo ++;
            //        }

            //        FormatArgument.AppendFormat("\t\tCASE {0}\n", FormatArgument2);
            //        FormatArgument2.Length = 0;

            //        FormatArgument.AppendFormat("\t\t\tPositionFrom = {0}\n", robot.Stations[i].RobotStationName);
            //        FormatArgument.AppendFormat("\t\t\tin_station=1\n");
            //        FormatArgument.AppendFormat("\t\t\t'\n");
            //        //pointNo ++;
            //    }

            //}

            FormatArgument.Length--;
            return string.Format(Format, robot.ProgramNameLowerChar, robot.ProgramNameUpperChar, FormatArgument).ToString();

        }

        // Expand ROBOT program with Main Task
        public static string GetYamahaRobotProgMainTask(ProgramModel robot)
        {
            string Format = YamahaROBOTprogMainTask;
            StringBuilder FormatArgument = new StringBuilder();

            for (int i = 0; i < robot.Stations.Count; i++)
            {

                FormatArgument.AppendFormat("\t\t\t\tCASE {0}\n", robot.Stations[i].RobotStationName);
                FormatArgument.AppendFormat("\t\t\t\t\tGOSUB *{0}_go{1}\n", robot.ProgramNameLowerChar, robot.Stations[i].RobotStationName);
                FormatArgument.AppendFormat("\t\t\t\t\t'\n");
            }

            return string.Format(Format, robot.ProgramNameLowerChar, FormatArgument).ToString();
        }

        // Expand ROBOT program with Test task
        public static string GetYamahaRobotProgTest(ProgramModel robot)
        {
            string Format = YamahaROBOTprogTest;
            StringBuilder FormatArgument = new StringBuilder();
            StringBuilder FormatArgument2 = new StringBuilder();

            for (int i = 1; i < robot.Stations.Count; i++)
            {
                FormatArgument.AppendFormat("\t\tGOSUB *{0}_go{1}\n", robot.ProgramNameLowerChar, robot.Stations[i].RobotStationName);
                FormatArgument.AppendFormat("\t\tDELAY GRIPPER\n");
                FormatArgument.AppendFormat("\t\t'\n");

                FormatArgument2.AppendFormat("\t\tCASE {0}\n", robot.Stations[i].RobotStationName);
                FormatArgument2.AppendFormat("\t\t\tGOSUB *{0}_go{1}\n", robot.ProgramNameLowerChar, robot.Stations[i].RobotStationName);
            }


            return string.Format(Format, robot.ProgramNameLowerChar, FormatArgument, robot.Stations.Count-1, FormatArgument2).ToString();
        }

        // Expand ROBOT program with Go_Home
        public static string GetYamahaRobotProgGoHome(ProgramModel robot)
        {
            string Format = YamahaROBOTprogGoHome;
            StringBuilder FormatArgument = new StringBuilder();

            for (int i = 1; i < robot.Stations.Count; i++)
            {
                FormatArgument.AppendFormat("\tELSEIF PositionFrom = {0} THEN\n", robot.Stations[i].RobotStationName);
                FormatArgument.AppendFormat("\t\tMOVE P, pHome\n");
                if (i == robot.Stations.Count - 1) { FormatArgument.AppendFormat("\t\t'"); } else { FormatArgument.AppendFormat("\t\t'\n"); }
            }

            return string.Format(Format, robot.ProgramNameLowerChar, FormatArgument).ToString();
        }

        // Expand ROBOT program with Go_Station
        public static string GetYamahaRobotProgGoStation(ProgramModel robot, int currstation)
        {
            string Format = YamahaROBOTprogGoStation;
            string num;
            StringBuilder FormatArgument = new StringBuilder();
            StringBuilder FormatArgument2 = new StringBuilder();
            StringBuilder FormatArgument3 = new StringBuilder();

            for (int i = 1; i < robot.Stations.Count; i++)
            {
                FormatArgument.AppendFormat("\tELSEIF PositionFrom = {0} THEN\n", robot.Stations[i].RobotStationName);
                if (currstation == i && robot.Stations[currstation].Positions != 1)
                {
                    //nothing
                }
                else
                {
                    FormatArgument.AppendFormat("\t\tMOVE P, pAboveStation\n");
                }

                if (i == robot.Stations.Count - 1) { FormatArgument.AppendFormat("\t\t'"); } else { FormatArgument.AppendFormat("\t\t'\n"); }
            }
            if (robot.Stations[currstation].Positions != 1){num = ("1");}else{num = ("");}

            if (robot.Stations[currstation].StationFreeEnabled)
            {
                FormatArgument2.AppendFormat("\t'\n");
                FormatArgument2.AppendFormat("\t'if going directly 'on station' then set the station immediately as BUSY (NOT FREE)\n");
                FormatArgument2.AppendFormat("\tIF I_ON_STATION=1 THEN\n");
                FormatArgument2.AppendFormat("\t\tRESET O_{0}_FREE\n", robot.Stations[currstation].RobotStationName);
                FormatArgument2.AppendFormat("\tENDIF\n");
                FormatArgument2.AppendFormat("\t'");
            }
            else
            {
                FormatArgument2.AppendFormat("\t'");
            }

            if (robot.Stations[currstation].Positions != 1)
            {
                FormatArgument3.AppendFormat("\t'define FINAL point in station\n");
                FormatArgument3.AppendFormat("\tIF additional_pos=1 THEN\n");
                FormatArgument3.AppendFormat("\t\tp{0}Final = p{0}1\n", robot.Stations[currstation].RobotStationName);
                for (int j = 2; j <= robot.Stations[currstation].Positions; j++)
                {
                    FormatArgument3.AppendFormat("\tELSEIF additional_pos={0} THEN\n", j);
                    FormatArgument3.AppendFormat("\t\tp{0}Final = p{0}{1}\n", robot.Stations[currstation].RobotStationName, j);
                }
                FormatArgument3.AppendFormat("\tELSE\n");
                FormatArgument3.AppendFormat("\t\t'output error\n");
                FormatArgument3.AppendFormat("\t\tSET O_PROGRAM_ERROR\n");
                FormatArgument3.AppendFormat("\t\tHOLD\n");
                FormatArgument3.AppendFormat("\t\t'\n");
                FormatArgument3.AppendFormat("\tENDIF\n");
                FormatArgument3.AppendFormat("\t'\n");
            }
            //else
            //{
            //    FormatArgument3.Length--;
            //}


            return string.Format(Format, robot.ProgramNameLowerChar, robot.Stations[currstation].RobotStationName, FormatArgument, robot.ProgramNameUpperChar, num, FormatArgument2, FormatArgument3).ToString();

        }

        // Expand ROBOT program with Move On Station
        public static string GetYamahaRobotProgMoveOnStation(ProgramModel robot)
        {
            string Format = YamahaROBOTprogMoveOnStation;
            StringBuilder FormatArgument = new StringBuilder();

            for (int i = 1; i < robot.Stations.Count; i++)
            {
                FormatArgument.AppendFormat("\t\tELSEIF PositionTo = {0} THEN\n", robot.Stations[i].RobotStationName);
                
                if (robot.Stations[i].StationFreeEnabled) 
                {
                    FormatArgument.AppendFormat("\t\t\t'turn OFF station free signal\n");
                    FormatArgument.AppendFormat("\t\t\tRESET O_{0}_FREE\n", robot.Stations[i].RobotStationName);
                    FormatArgument.AppendFormat("\t\t\t'\n");
                }

                FormatArgument.AppendFormat("\t\t\tGOSUB *robot_fullWorkingSpeed\n");

                if (robot.Stations[i].Positions == 1)  
                {
                    FormatArgument.AppendFormat("\t\t\tpAboveStation = p{0}\n", robot.Stations[i].RobotStationName);
                    FormatArgument.AppendFormat("\t\t\tLOC3(pAboveStation) = LOC3(p{0}) - Z_STATION_FREE\n", robot.Stations[i].RobotStationName);
                    FormatArgument.AppendFormat("\t\t\tIF LOC3(pAboveStation) > ROBOT_MAX_Z THEN\n");
                    FormatArgument.AppendFormat("\t\t\t\tMOVE P, pAboveStation, CONT\n");
                    FormatArgument.AppendFormat("\t\t\tENDIF\n");
                    FormatArgument.AppendFormat("\t\t\t'\n");

                    FormatArgument.AppendFormat("\t\t\tCALL *{0}_slowWorkingSpeed ({1}_APPROACH_SPEED%, {1}_APPROACH_ACCEL%, {1}_APPROACH_DECEL%, power_mode)\n", robot.ProgramNameLowerChar, robot.ProgramNameUpperChar);
                    FormatArgument.AppendFormat("\t\t\tLOC3(pAboveStation) = LOC3(p{0}) - Z_STATION_SLOW\n", robot.Stations[i].RobotStationName);
                    //FormatArgument.AppendFormat("\t\t\tIF LOC3(pAboveStation) > ROBOT_MAX_Z THEN\n");
                    FormatArgument.AppendFormat("\t\t\tMOVE P, pAboveStation, CONT\n");
                    //FormatArgument.AppendFormat("\t\t\tENDIF\n");
                    FormatArgument.AppendFormat("\t\t\t'\n");

                    FormatArgument.AppendFormat("\t\t\tCALL *{0}_slowWorkingSpeed ({1}_VERY_SLOW_SPEED%, {1}_APPROACH_ACCEL%, {1}_VERY_SLOW_DECEL%, power_mode)\n", robot.ProgramNameLowerChar, robot.ProgramNameUpperChar);
                    FormatArgument.AppendFormat("\t\t\tMOVE P, p{0}\n", robot.Stations[i].RobotStationName);
                    if (i == robot.Stations.Count-1) { FormatArgument.AppendFormat("\t\t\t'"); } else { FormatArgument.AppendFormat("\t\t\t'\n"); }

                }
                else //IF THERE ARE ADDITIONAL POSITIONS
                {

                    FormatArgument.AppendFormat("\t\t\tpAboveStation = p{0}Final\n", robot.Stations[i].RobotStationName);
                    FormatArgument.AppendFormat("\t\t\tLOC3(pAboveStation) = LOC3(p{0}Final) - Z_STATION_FREE\n", robot.Stations[i].RobotStationName);
                    FormatArgument.AppendFormat("\t\t\tIF LOC3(pAboveStation) > ROBOT_MAX_Z THEN\n");
                    FormatArgument.AppendFormat("\t\t\t\tMOVE P, pAboveStation, CONT\n");
                    FormatArgument.AppendFormat("\t\t\tENDIF\n");

                    FormatArgument.AppendFormat("\t\t\tCALL *{0}_slowWorkingSpeed ({1}_APPROACH_SPEED%, {1}_APPROACH_ACCEL%, {1}_APPROACH_DECEL%, power_mode)\n", robot.ProgramNameLowerChar, robot.ProgramNameUpperChar);
                    FormatArgument.AppendFormat("\t\t\tLOC3(pAboveStation) = LOC3(p{0}Final) - Z_STATION_SLOW\n", robot.Stations[i].RobotStationName);
                    //FormatArgument.AppendFormat("\t\t\tIF LOC3(pAboveStation) > ROBOT_MAX_Z THEN\n");
                    FormatArgument.AppendFormat("\t\t\tMOVE P, pAboveStation, CONT\n");
                    //FormatArgument.AppendFormat("\t\t\tENDIF\n");
                    FormatArgument.AppendFormat("\t\t\t'\n");

                    FormatArgument.AppendFormat("\t\t\tCALL *{0}_slowWorkingSpeed ({1}_VERY_SLOW_SPEED%, {1}_APPROACH_ACCEL%, {1}_VERY_SLOW_DECEL%, power_mode)\n", robot.ProgramNameLowerChar, robot.ProgramNameUpperChar);
                    FormatArgument.AppendFormat("\t\t\tMOVE P, p{0}Final\n", robot.Stations[i].RobotStationName);
                    if (i == robot.Stations.Count - 1) { FormatArgument.AppendFormat("\t\t\t'"); } else { FormatArgument.AppendFormat("\t\t\t'\n"); }
                }
                
            }

            return string.Format(Format, robot.ProgramNameLowerChar, FormatArgument).ToString();
        }

        // Expand ROBOT program with Move Away
        public static string GetYamahaRobotProgMoveAway(ProgramModel robot)
        {
            string Format = YamahaROBOTprogMoveAway;
            StringBuilder FormatArgument = new StringBuilder();

            for (int i = 1; i < robot.Stations.Count; i++)
            {
                FormatArgument.AppendFormat("\t\tELSEIF PositionFrom = {0} THEN\n", robot.Stations[i].RobotStationName);
                //FormatArgument.AppendFormat("\t\t\tIF LOC3(WHRXY) - Z_STATION_SLOW > ROBOT_MAX_Z THEN\n");

                if (robot.Stations[i].Positions != 1){
                    FormatArgument.AppendFormat("\t\t\tIF LOC3(WHRXY) - Z_STATION_SLOW > ROBOT_MAX_Z AND LOC3(WHRXY) > LOC3 (p{0}1) - Z_STATION_SLOW THEN\n", robot.Stations[i].RobotStationName);}
                else{
                    FormatArgument.AppendFormat("\t\t\tIF LOC3(WHRXY) - Z_STATION_SLOW > ROBOT_MAX_Z AND LOC3(WHRXY) > LOC3 (p{0}) - Z_STATION_SLOW THEN\n", robot.Stations[i].RobotStationName);}

                FormatArgument.AppendFormat("\t\t\t\tCALL *{0}_slowWorkingSpeed ({1}_VERY_SLOW_SPEED%, {1}_VERY_SLOW_ACCEL%, {1}_DEPART_DECEL%, power_mode)\n", robot.ProgramNameLowerChar, robot.ProgramNameUpperChar);

                if (robot.Stations[i].Positions != 1){
                    FormatArgument.AppendFormat("\t\t\t\tLOC3 (pAboveStation) = LOC3 (p{0}1) - Z_STATION_SLOW\n", robot.Stations[i].RobotStationName);}
                else{
                    FormatArgument.AppendFormat("\t\t\t\tLOC3 (pAboveStation) = LOC3 (p{0}) - Z_STATION_SLOW\n", robot.Stations[i].RobotStationName);}
                
                FormatArgument.AppendFormat("\t\t\t\tMOVE P, pAboveStation, CONT\n");
                FormatArgument.AppendFormat("\t\t\tENDIF\n");
                FormatArgument.AppendFormat("\t\t\t'\n");

                FormatArgument.AppendFormat("\t\t\tGOSUB *{0}_fullWorkingSpeed \n", robot.ProgramNameLowerChar);
                FormatArgument.AppendFormat("\t\t\tLOC3 (pAboveStation) = {0}_MAX_Z\n", robot.ProgramNameUpperChar);
                FormatArgument.AppendFormat("\t\t\tMOVE P, pAboveStation, CONT\n");
                FormatArgument.AppendFormat("\t\t\t'\n");
                //if (i == robot.Stations.Count - 1 & robot.Stations[i].Positions == 1) { FormatArgument.AppendFormat("\t\t\t'"); } else { FormatArgument.AppendFormat("\t\t\t'\n"); }

                if (robot.Stations[i].Positions != 1)
                {
                    FormatArgument.AppendFormat("\t\t\tIF PositionTo = {0} THEN\n", robot.Stations[i].RobotStationName);
                    FormatArgument.AppendFormat("\t\t\t\t'nothing\n");
                    FormatArgument.AppendFormat("\t\t\tELSE\n");
                    FormatArgument.AppendFormat("\t\t\t\tpAboveStation = p{0}1\n", robot.Stations[i].RobotStationName);
                    FormatArgument.AppendFormat("\t\t\t\tLOC3 (pAboveStation) = {0}_MAX_Z\n", robot.ProgramNameUpperChar);
                    FormatArgument.AppendFormat("\t\t\t\tMOVE L, pAboveStation, CONT\n");
                    FormatArgument.AppendFormat("\t\t\tENDIF\n");
                    if (i == robot.Stations.Count - 1 & robot.Stations[i].StationFreeEnabled==false) { FormatArgument.AppendFormat("\t\t\t'"); } else { FormatArgument.AppendFormat("\t\t\t'\n"); }
                }

                if (robot.Stations[i].StationFreeEnabled)
                {
                    FormatArgument.AppendFormat("\t\t\t'turn ON station free signal\n");
                    FormatArgument.AppendFormat("\t\t\tSET O_{0}_FREE\n", robot.Stations[i].RobotStationName);
                    if (i == robot.Stations.Count - 1) { FormatArgument.AppendFormat("\t\t\t'"); } else { FormatArgument.AppendFormat("\t\t\t'\n"); }
                }

            }

            return string.Format(Format, robot.ProgramNameLowerChar, FormatArgument).ToString();
        }

        //// Generate COMMON program
        public static string GetYamahaCommonProg(ProgramModel robot)
        {
            int pointNo = 0;
            string Format = YamahaCOMMONprog;
            StringBuilder FormatArgument = new StringBuilder();
            StringBuilder FormatArgument2 = new StringBuilder();

            for (int i = 0; i < robot.Stations.Count; i++)
            {
                if (robot.Stations[i].StationFreeEnabled)
                {
                    FormatArgument.AppendFormat("\tRESET O_{0}_FREE\n", robot.Stations[i].RobotStationName, i);
                    FormatArgument2.AppendFormat("\tSET O_{0}_FREE\n", robot.Stations[i].RobotStationName, i);
                }

                pointNo += robot.Stations[i].Positions;

            }

            //return string.Format(Format, robot.ProgramNameLowerChar, robot.ProgramNameUpperChar, FormatArgument, FormatArgument2, robot.Stations.Count-1).ToString();
            return string.Format(Format, robot.ProgramNameLowerChar, robot.ProgramNameUpperChar, FormatArgument, FormatArgument2, pointNo-1).ToString();
        }

        //// Gennerate POINTS file
        public static string GetYamahaPointsFile(ProgramModel robot)
        {
            NumberFormatInfo dot = new NumberFormatInfo();
            dot.NumberDecimalSeparator = ".";
            dot.NumberDecimalDigits = 3;
            string Format = YamahaPOINTSfile;
            int pointNo = 0;
            int x = 250; int y = 1000; int z = 0; int r = 90; int arm = 2;
            StringBuilder FormatArgument = new StringBuilder();
            StringBuilder FormatArgument2 = new StringBuilder();
            StringBuilder FormatArgument3 = new StringBuilder();

            for (int i = 0; i < robot.Stations.Count; i++)
            {
                //if (robot.Stations[i].Xcord < 0) {
                //    robot.Stations[i].R1cord = ((Math.Atan(robot.Stations[i].Ycord / robot.Stations[i].Xcord))+ Math.PI) * (180 / Math.PI);
                //    robot.Stations[i].R1cord = Math.Round(robot.Stations[i].R1cord, 3);
                //}
                //else{
                //    robot.Stations[i].R1cord = (Math.Atan(robot.Stations[i].Ycord / robot.Stations[i].Xcord)) * (180 / Math.PI);
                //    robot.Stations[i].R1cord = Math.Round(robot.Stations[i].R1cord, 3);
                //}

                //robot.Stations[i].R1cord = 0.000;
                //robot.Stations[i].R2cord = 0.000;
                //robot.Stations[i].R3cord = 0.000;

                //if (pointNo == 0)
                //{
                //    robot.Stations[i].Zcord = 0.000;
                //}

                if (robot.Stations[i].Positions != 1) //create first additional position station points
                {
                    FormatArgument.AppendFormat("P{0}={1} {2} {3} {4} {5} {6} {7} 0 0\n", pointNo, robot.Stations[i].Xcord.ToString("F3", dot), robot.Stations[i].Ycord.ToString("F3", dot), robot.Stations[i].Zcord.ToString("F3", dot), robot.Stations[i].R1cord.ToString("F3", dot), robot.Stations[i].R2cord.ToString("F3", dot), robot.Stations[i].R3cord.ToString("F3", dot), arm);
                    FormatArgument2.AppendFormat("PC{0}={1}\n", pointNo, robot.Stations[i].RobotStationComment);
                    FormatArgument3.AppendFormat("PN{0}=p{1}1\n", pointNo, robot.Stations[i].RobotStationName);
                    pointNo += 1;
                }
                else //create normal station points
                {
                    FormatArgument.AppendFormat("P{0}={1} {2} {3} {4} {5} {6} {7} 0 0\n", pointNo, robot.Stations[i].Xcord.ToString("F3", dot), robot.Stations[i].Ycord.ToString("F3", dot), robot.Stations[i].Zcord.ToString("F3", dot), robot.Stations[i].R1cord.ToString("F3", dot), robot.Stations[i].R2cord.ToString("F3", dot), robot.Stations[i].R3cord.ToString("F3", dot), arm);
                    FormatArgument2.AppendFormat("PC{0}={1}\n", pointNo, robot.Stations[i].RobotStationComment);
                    FormatArgument3.AppendFormat("PN{0}=p{1}\n", pointNo, robot.Stations[i].RobotStationName);
                    pointNo += 1;
                }
            }

            //pointNo += 2;

            for (int i = 0; i < robot.Stations.Count; i++)
            {
                if (robot.Stations[i].Positions != 1) //create other additional position station points
                {

                    for (int j = 2; j < robot.Stations[i].Positions + 1; j++) //create additional points
                    {
                        FormatArgument.AppendFormat("P{0}={1} {2} {3} {4} {5} {6} {7} 0 0\n", pointNo, robot.Stations[i].Xcord.ToString("F3", dot), robot.Stations[i].Ycord.ToString("F3", dot), robot.Stations[i].Zcord.ToString("F3", dot), robot.Stations[i].R1cord.ToString("F3", dot), robot.Stations[i].R2cord.ToString("F3", dot), robot.Stations[i].R3cord.ToString("F3", dot), arm);
                        FormatArgument2.AppendFormat("PC{0}={1}\n", pointNo, robot.Stations[i].RobotStationComment);
                        FormatArgument3.AppendFormat("PN{0}=p{1}{2}\n", pointNo, robot.Stations[i].RobotStationName, j);
                        pointNo += 1;
                    }

                    //FormatArgument.AppendFormat("P{0}=0.000 0.000 0.000 0.000 0.000 0.000 2 0 0\n", pointNo);
                    //FormatArgument2.AppendFormat("PC{0}=variable point\n", pointNo);
                    //FormatArgument3.AppendFormat("PN{0}=p{1}Final\n", pointNo, robot.Stations[i].RobotStationName);
                    //pointNo += 1;


                }
            }

            pointNo += 2;

            for (int i = 0; i < robot.Stations.Count; i++)
            {
                if (robot.Stations[i].Positions != 1) //create other additional position station points
                {

                    //for (int j = 2; j < robot.Stations[i].Positions + 1; j++) //create additional points
                    //{
                    //    FormatArgument.AppendFormat("P{0}={1} {2} {3} {4} {5} {6} {7} 0 0\n", pointNo, robot.Stations[i].Xcord.ToString("F3", dot), robot.Stations[i].Ycord.ToString("F3", dot), robot.Stations[i].Zcord.ToString("F3", dot), robot.Stations[i].R1cord.ToString("F3", dot), robot.Stations[i].R2cord.ToString("F3", dot), robot.Stations[i].R3cord.ToString("F3", dot), arm);
                    //    FormatArgument2.AppendFormat("PC{0}={1}\n", pointNo, robot.Stations[i].RobotStationComment);
                    //    FormatArgument3.AppendFormat("PN{0}=p{1}{2}\n", pointNo, robot.Stations[i].RobotStationName, j);
                    //    pointNo += 1;
                    //}

                    FormatArgument.AppendFormat("P{0}=0.000 0.000 0.000 0.000 0.000 0.000 2 0 0\n", pointNo);
                    FormatArgument2.AppendFormat("PC{0}=variable point\n", pointNo);
                    FormatArgument3.AppendFormat("PN{0}=p{1}Final\n", pointNo, robot.Stations[i].RobotStationName);
                    pointNo += 1;


                }
            }

            //create normal pAboveStation, pSafeL, pSafeR
            FormatArgument.AppendFormat("P{0}=0.000 0.000 0.000 0.000 0.000 0.000 2 0 0\n", pointNo);
            FormatArgument2.AppendFormat("PC{0}=variable point\n", pointNo);
            FormatArgument3.AppendFormat("PN{0}=pAboveStation\n", pointNo);
            pointNo += 1;

            pointNo += 2;

            FormatArgument.AppendFormat("P{0}={1}.000 {2}.000 {3}.000 {4}.000 0.000 0.000 {5} 0 0\n", pointNo, x, y, z, r, arm);
            FormatArgument2.AppendFormat("PC{0}=safe ARM Left\n", pointNo);
            FormatArgument3.AppendFormat("PN{0}=pSafeL\n", pointNo);
            pointNo += 1;

            FormatArgument.AppendFormat("P{0}=-{1}.000 {2}.000 {3}.000 {4}.000 0.000 0.000 {5} 0 0\n", pointNo, x, y, z, r, arm-1);
            FormatArgument2.AppendFormat("PC{0}=safe ARM Right\n", pointNo);
            FormatArgument3.AppendFormat("PN{0}=pSafeR\n", pointNo);

            return string.Format(Format, FormatArgument, FormatArgument2, FormatArgument3).ToString();
        
        }

        //// Gennerate IO file
        public static string GetYamahaIoFile(ProgramModel robot)
        {
            string Format = YamahaIOfile;
            int out1 = 4;
            int out2 = 0;
            StringBuilder FormatArgument = new StringBuilder();
            StringBuilder FormatArgument2 = new StringBuilder();
            StringBuilder FormatArgument3 = new StringBuilder();

            for (int i = 0; i < robot.Stations.Count; i++)
            {
                if (robot.Stations[i].StationFreeEnabled)
                {
                    FormatArgument.AppendFormat("SONM{0}({1})=O_{2}_FREE\n",out1,out2, robot.Stations[i].RobotStationName);
                    out2 += 1;
                    if (out2 >= 8) {out1 += 1;out2 = 0;}
                }
            }

            return string.Format(Format, FormatArgument).ToString();

        }


        #endregion


        #region///// Kawasaki /////

        ////Generate DOCUMENTATION program
        public static string GetKawasakiDocumentation(ObservableCollection<ProgramModel> robot)
        {
           string Format = KawasakiDOCUMENTATION;

           return string.Format(Format).ToString() ;

        }

        ////Generate TESTING program
        public static string GetKawasakiTesting(ProgramModel robot)
        {
            string Format = KawasakiTESTING;
            StringBuilder FormatArgument = new StringBuilder();

            for (int i = 1; i < robot.Stations.Count; i++)
            {

                FormatArgument.AppendFormat("\t\tVALUE {0}:\n", robot.Stations[i].RobotStationName);
                FormatArgument.AppendFormat("\t\t\tCALL go{0}\n", robot.Stations[i].RobotStationName);
                FormatArgument.AppendFormat("\t\t\t;\n");
                FormatArgument.AppendFormat("\t\t\t;\n");
            }

            return string.Format(Format, FormatArgument, robot.Stations.Count-1).ToString();

        }

        ////Generate MAIN_Main program
        public static string GetKawasakiMain(ProgramModel robot)
        {
            string Format = KawasakiMAIN_Main;

            return string.Format(Format).ToString() + "\n";

        }

        ////Generate MAIN_Init program
        public static string GetKawasakiInit(ProgramModel robot)
        {
            string Format = KawasakiMAIN_Init;

            return string.Format(Format).ToString();

        }

        ////Generate MAIN_Homing program
        public static string GetKawasakiHoming(ProgramModel robot)
        {
            string Format = KawasakiMAIN_Homing;
            StringBuilder FormatArgument = new StringBuilder();

            for (int i = 1; i < robot.Stations.Count; i++)
            {

                FormatArgument.AppendFormat("\t\t\t\tVALUE {0}:\n", robot.Stations[i].RobotStationName);
                if (robot.Stations[i].Positions == 1)
                {
                    FormatArgument.AppendFormat("\t\t\t\t\tIF DZ (HERE) < (DZ (p{0}) + station_z) THEN\n", robot.Stations[i].RobotStationName);
                }
                else
                {
                    FormatArgument.AppendFormat("\t\t\t\t\tIF DZ (HERE) < (DZ (p{0}1) + station_z) THEN\n", robot.Stations[i].RobotStationName);
                } 
                FormatArgument.AppendFormat("\t\t\t\t\t\tSIGNAL (in_station)\n");
                FormatArgument.AppendFormat("\t\t\t\t\tELSE\n");
                FormatArgument.AppendFormat("\t\t\t\t\t\tSIGNAL (-in_station)\n");
                FormatArgument.AppendFormat("\t\t\t\t\tEND\n");
                FormatArgument.AppendFormat("\t\t\t\t\t;\n");
                FormatArgument.AppendFormat("\t\t\t\t\t;\n");
            }
            FormatArgument.Remove(FormatArgument.Length - 1, 1);

            return string.Format(Format, FormatArgument).ToString();

        }

        ////Generate MAIN_MainTask program
        public static string GetKawasakiMainTask(ProgramModel robot)
        {
            string Format = KawasakiMAIN_MainTask;
            StringBuilder FormatArgument = new StringBuilder();

            for (int i = 1; i < robot.Stations.Count; i++)
            {

                FormatArgument.AppendFormat("\t\t\t\tVALUE {0}:\n", robot.Stations[i].RobotStationName);
                if(robot.Stations[i].Positions != 1)
                {
                    FormatArgument.AppendFormat("\t\t\t\t\taddpos = BITS(I_ADD_POS, 16)\n");
                }
                FormatArgument.AppendFormat("\t\t\t\t\tCALL go{0}\n", robot.Stations[i].RobotStationName);
                FormatArgument.AppendFormat("\t\t\t\t\t;\n");
                FormatArgument.AppendFormat("\t\t\t\t\t;\n");
            }
            FormatArgument.Remove(FormatArgument.Length - 1, 1);

            return string.Format(Format, FormatArgument).ToString();

        }

        ////Generate MOVEMENTS_MoveAway program
        public static string GetKawasakiMoveAway(ProgramModel robot)
        {
            string Format = KawasakiMOVEMENTS_MoveAway;
            StringBuilder FormatArgument = new StringBuilder();

            for (int i = 1; i < robot.Stations.Count; i++)
            {

                FormatArgument.AppendFormat("\t\t\tVALUE {0}:\n", robot.Stations[i].RobotStationName);
                FormatArgument.AppendFormat("\t\t\t\tCALL slowerWorkingMd (very_slow_speed, depart_acc, depart_dec)\n");
                FormatArgument.AppendFormat("\t\t\t\tLMOVE TRANS (0, 0, slow_z) + pOutsideStation\n");
                FormatArgument.AppendFormat("\t\t\t\t;\n");

                FormatArgument.AppendFormat("\t\t\t\tCALL slowerWorkingMd (slower_speed, depart_acc, depart_dec)\n");
                FormatArgument.AppendFormat("\t\t\t\tLMOVE TRANS (0, 0, station_z) + pOutsideStation\n");
                FormatArgument.AppendFormat("\t\t\t\t;\n");

                FormatArgument.AppendFormat("\t\t\t\tPOINT/Z pOutsideStation = TRANS(0, 0, max_z)\n"); 
                FormatArgument.AppendFormat("\t\t\t\tCALL fullWorkingSpd\n");   
                FormatArgument.AppendFormat("\t\t\t\tLMOVE pOutsideStation\n");
                FormatArgument.AppendFormat("\t\t\t\t;\n");

                FormatArgument.AppendFormat("\t\t\t\tSIGNAL (-in_station)\n");
                if (robot.Stations[i].StationFreeEnabled)
                {
                    FormatArgument.AppendFormat("\t\t\t\tSIGNAL (O_{0}_FREE)\n", robot.Stations[i].RobotStationName);
                }
                FormatArgument.AppendFormat("\t\t\t\t;\n");
            }
            FormatArgument.Remove(FormatArgument.Length - 1, 1);

            return string.Format(Format, FormatArgument).ToString();

        }

        ////Generate MOVEMENTS_MoveOnStation program
        public static string GetKawasakiMoveOnStation(ProgramModel robot)
        {
            string Format = KawasakiMOVEMENTS_MoveOnStation;
            StringBuilder FormatArgument = new StringBuilder();

            for (int i = 1; i < robot.Stations.Count; i++)
            {

                FormatArgument.AppendFormat("\t\t\tVALUE {0}:\n", robot.Stations[i].RobotStationName);
                if (robot.Stations[i].StationFreeEnabled)
                {
                    FormatArgument.AppendFormat("\t\t\t\t;\"turn OFF station free signal\"\n");
                    FormatArgument.AppendFormat("\t\t\t\tSIGNAL (-O_{0}_FREE)\n", robot.Stations[i].RobotStationName);
                    FormatArgument.AppendFormat("\t\t\t\t;\n");
                }

                if (robot.Stations[i].Positions == 1) 
                {
                    FormatArgument.AppendFormat("\t\t\t\tCALL fullWorkingSpd\n");
                    FormatArgument.AppendFormat("\t\t\t\tLMOVE TRANS (0, 0, station_z) + p{0}\n", robot.Stations[i].RobotStationName);
                    FormatArgument.AppendFormat("\t\t\t\t;\n");

                    FormatArgument.AppendFormat("\t\t\t\tCALL slowerWorkingMd (slower_speed, approach_acc, approach_dec)\n");
                    FormatArgument.AppendFormat("\t\t\t\tLMOVE TRANS (0, 0, slow_z) + p{0}\n", robot.Stations[i].RobotStationName);
                    FormatArgument.AppendFormat("\t\t\t\t;\n");

                    FormatArgument.AppendFormat("\t\t\t\tCALL slowerWorkingMd (very_slow_speed, approach_acc, approach_dec)\n");
                    FormatArgument.AppendFormat("\t\t\t\tLMOVE p{0}\n", robot.Stations[i].RobotStationName);
                    FormatArgument.AppendFormat("\t\t\t\t;\n");
                }
                else
                {
                    FormatArgument.AppendFormat("\t\t\t\tCALL fullWorkingSpd\n");
                    FormatArgument.AppendFormat("\t\t\t\tLMOVE TRANS (0, 0, station_z) + p{0}Final\n", robot.Stations[i].RobotStationName);
                    FormatArgument.AppendFormat("\t\t\t\t;\n");

                    FormatArgument.AppendFormat("\t\t\t\tCALL slowerWorkingMd (slower_speed, approach_acc, approach_dec)\n");
                    FormatArgument.AppendFormat("\t\t\t\tLMOVE TRANS (0, 0, slow_z) + p{0}Final\n", robot.Stations[i].RobotStationName);
                    FormatArgument.AppendFormat("\t\t\t\t;\n");

                    FormatArgument.AppendFormat("\t\t\t\tCALL slowerWorkingMd (very_slow_speed, approach_acc, approach_dec)\n");
                    FormatArgument.AppendFormat("\t\t\t\tLMOVE p{0}Final\n", robot.Stations[i].RobotStationName);
                    FormatArgument.AppendFormat("\t\t\t\t;\n");
                }

            }
            FormatArgument.Remove(FormatArgument.Length - 1, 1);

            return string.Format(Format, FormatArgument).ToString();

        }

        ////Generate MOVEMENTS_GoHome program
        public static string GetKawasakiGoHome(ProgramModel robot)
        {
            string Format = KawasakiMOVEMENTS_GoHome;
            StringBuilder FormatArgument = new StringBuilder();

            for (int i = 1; i < robot.Stations.Count; i++)
            {

                FormatArgument.AppendFormat("\t\tVALUE {0}:\n", robot.Stations[i].RobotStationName);
                FormatArgument.AppendFormat("\t\t\tJMOVE #pHome\n");
                FormatArgument.AppendFormat("\t\t\t;\n");
            }
            FormatArgument.Remove(FormatArgument.Length - 1, 1);

            return string.Format(Format, FormatArgument).ToString();

        }

        ////Generate MOVEMENTS_GoStation program
        public static string GetKawasakiGoStation(ProgramModel robot, int currstation)
        {
            string Format = KawasakiMOVEMENTS_GoStation;
            StringBuilder FormatArgument = new StringBuilder();
            StringBuilder FormatArgument2 = new StringBuilder();
            StringBuilder FormatArgument3 = new StringBuilder();

            for (int i = 1; i < robot.Stations.Count; i++)
            {

                FormatArgument.AppendFormat("\t\tVALUE {0}:\n", robot.Stations[i].RobotStationName);
                FormatArgument.AppendFormat("\t\t\tJMOVE pOutsideStation\n");
                FormatArgument.AppendFormat("\t\t\t;\n");
            }
            FormatArgument.Remove(FormatArgument.Length - 1, 1);

            ///

            if (robot.Stations[currstation].StationFreeEnabled) 
            {
                FormatArgument2.AppendFormat("\t;\"if going directly 'on station' then set the station immediately as BUSY (NOT FREE)\"\n");
                FormatArgument2.AppendFormat("\tIF SIG (I_ON_STATION) THEN\n");
                FormatArgument2.AppendFormat("\t\tSIGNAL (-O_{0}_FREE)\n", robot.Stations[currstation].RobotStationName);
                FormatArgument2.AppendFormat("\tEND\n");
                FormatArgument2.AppendFormat("\t;");
            }
            else 
            { 
                FormatArgument2.AppendFormat("\t;"); 
            }

            ///

            if (robot.Stations[currstation].Positions != 1)
            {
                FormatArgument3.AppendFormat("\tCASE addPos OF\n");
                for (int j = 1; j <= robot.Stations[currstation].Positions; j++)
                {    
                    FormatArgument3.AppendFormat("\t\tVALUE {0}:\n", j);
                    FormatArgument3.AppendFormat("\t\t\tPOINT p{0}Final = p{0}{1}\n", robot.Stations[currstation].RobotStationName, j);
                    FormatArgument3.AppendFormat("\t\t\t;\n");
                }
                FormatArgument3.AppendFormat("\t\tANY :\n");
                FormatArgument3.AppendFormat("\t\t\tPRINT 2: \"ERROR: add_pos data invalid\"\n");
                FormatArgument3.AppendFormat("\t\t\tSIGNAL O_PROGRAM_ERROR, O_S_ERROR\n");
                FormatArgument3.AppendFormat("\tEND\n");
                FormatArgument3.AppendFormat("\t;\n");
                //
                FormatArgument3.AppendFormat("\tPOINT pOutsideStation = p{0}Final\n", robot.Stations[currstation].RobotStationName);
                FormatArgument3.AppendFormat("\tPOINT/Z pOutsideStation = TRANS (0, 0, max_z)\n");
            }
            else
            {
                FormatArgument3.AppendFormat("\tPOINT pOutsideStation = p{0}\n", robot.Stations[currstation].RobotStationName);
                FormatArgument3.AppendFormat("\tPOINT/Z pOutsideStation = TRANS (0, 0, max_z)\n");
            }
            FormatArgument3.Remove(FormatArgument3.Length - 1, 1);
            ///

            return string.Format(Format, robot.Stations[currstation].RobotStationName, FormatArgument, FormatArgument2, FormatArgument3).ToString();

        }

        ////Generate FUNCTIONS program
        public static string GetKawasakiFunctions(ProgramModel robot)
        {
            string Format = KawasakiFUNCTIONS;
            StringBuilder FormatArgument0 = new StringBuilder();
            StringBuilder FormatArgument1 = new StringBuilder();
            StringBuilder FormatArgument2 = new StringBuilder();
            StringBuilder FormatArgument3 = new StringBuilder();

            for (int i = 1; i < robot.Stations.Count; i++)
            {
                FormatArgument0.AppendFormat("\t{0} = {1}\n", robot.Stations[i].RobotStationName,i);
                //
                if (robot.Stations[i].StationFreeEnabled)
                {
                    FormatArgument1.AppendFormat("\tSIGNAL O_{0}_FREE\n", robot.Stations[i].RobotStationName);
                    FormatArgument2.AppendFormat("\tSIGNAL -O_{0}_FREE\n", robot.Stations[i].RobotStationName);
                }
                //
                if (robot.Stations[i].Positions == 1)
                {
                    FormatArgument3.AppendFormat("\ta = DISTANCE (HERE, p{0})\n", robot.Stations[i].RobotStationName);
                }
                else
                {
                    FormatArgument3.AppendFormat("\ta = DISTANCE (HERE, p{0}1)\n", robot.Stations[i].RobotStationName);
                }
                FormatArgument3.AppendFormat("\tIF a < dist THEN\n");
                FormatArgument3.AppendFormat("\t\tdist = a\n");
                FormatArgument3.AppendFormat("\t\tclosestpoint = {0}\n", robot.Stations[i].RobotStationName);
                FormatArgument3.AppendFormat("\tEND\n");
                FormatArgument3.AppendFormat("\t;\n");
            }
            FormatArgument0.Remove(FormatArgument0.Length - 1, 1);
            FormatArgument1.Remove(FormatArgument1.Length - 1, 1);
            FormatArgument2.Remove(FormatArgument2.Length - 1, 1);
            FormatArgument3.Remove(FormatArgument3.Length - 1, 1);

            return string.Format(Format, FormatArgument0, FormatArgument1, FormatArgument2, FormatArgument3).ToString();

        }

        ////Generate CALIBRATION program
        public static string GetKawasakiCalibration(ProgramModel robot)
        {
            string Format = KawasakiCALIBRATION;

            return string.Format(Format).ToString();

        }

        ////Generate POINTS program
        public static string GetKawasakiPoints(ProgramModel robot)
        {
            NumberFormatInfo dot = new NumberFormatInfo();
            dot.NumberDecimalSeparator = ".";
            string Format = KawasakiPOINTS;
            StringBuilder FormatArgument = new StringBuilder();
            StringBuilder FormatArgument2 = new StringBuilder();

            for (int i = 1; i < robot.Stations.Count; i++)
            {
                if (robot.Stations[i].Positions != 1)
                {
                    for (int j = 1; j < robot.Stations[i].Positions + 1; j++) //create additional points
                    {
                        FormatArgument.AppendFormat("p{0}{7} {1} {2} {3} {4} {5} {6}\n", robot.Stations[i].RobotStationName, robot.Stations[i].Xcord.ToString(dot), robot.Stations[i].Ycord.ToString(dot), robot.Stations[i].Zcord.ToString(dot), robot.Stations[i].R1cord.ToString(dot), robot.Stations[i].R2cord.ToString(dot), robot.Stations[i].R3cord.ToString(dot),j);
                    }
                    //create final point for additional points
                    FormatArgument.AppendFormat("p{0}Final {1} {2} {3} {4} {5} {6}\n", robot.Stations[i].RobotStationName, robot.Stations[i].Xcord.ToString(dot), robot.Stations[i].Ycord.ToString(dot), robot.Stations[i].Zcord.ToString(dot), robot.Stations[i].R1cord.ToString(dot), robot.Stations[i].R2cord.ToString(dot), robot.Stations[i].R3cord.ToString(dot));
                }
                else //create normal station points
                {
                    FormatArgument.AppendFormat("p{0} {1} {2} {3} {4} {5} {6}\n", robot.Stations[i].RobotStationName, robot.Stations[i].Xcord.ToString(dot), robot.Stations[i].Ycord.ToString(dot), robot.Stations[i].Zcord.ToString(dot), robot.Stations[i].R1cord.ToString(dot), robot.Stations[i].R2cord.ToString(dot), robot.Stations[i].R3cord.ToString(dot));
                }
                
            }
            FormatArgument.Remove(FormatArgument.Length - 1, 1);

            FormatArgument2.AppendFormat("#p{0} 0 -20 -20 0 -90 0", robot.Stations[0].RobotStationName);

            return string.Format(Format,FormatArgument,FormatArgument2).ToString();

        }

        ////Generate IO program
        public static string GetKawasakiIO(ProgramModel robot)
        {
            string Format = KawasakiIO;
            int out1 = 57;
            StringBuilder FormatArgument = new StringBuilder();
            StringBuilder FormatArgument2 = new StringBuilder();

            for (int i = 1; i < robot.Stations.Count; i++)
            {
                if (robot.Stations[i].StationFreeEnabled)
                {
                    FormatArgument.AppendFormat("N_OX{0}    \"O_{1}_FREE  \"\n", out1, robot.Stations[i].RobotStationName);
                    FormatArgument2.AppendFormat("O_{0}_FREE = {1}\n", robot.Stations[i].RobotStationName, out1);
                    out1++;
                }
                FormatArgument2.AppendFormat("{0} = {1}\n", robot.Stations[i].RobotStationName, i);
            }
            FormatArgument.Length--;
            FormatArgument2.Length--;

            return string.Format(Format, FormatArgument, FormatArgument2).ToString();

        }

        ////Generate IO handling program
        public static string GetKawasakiIOhandling(ProgramModel robot)
        {
            string Format = KawasakiIOhandling;

            return string.Format(Format).ToString() + "\n";

        }

        ////Generate COMMENT program
        public static string GetKawasakiComment(ProgramModel robot)
        {
            string Format = KawasakiCOMMENT;
            StringBuilder FormatArgument0 = new StringBuilder();
            StringBuilder FormatArgument1 = new StringBuilder();
            StringBuilder FormatArgument2 = new StringBuilder();

            for (int i = 1; i < robot.Stations.Count; i++)
            {

                FormatArgument0.AppendFormat("\t; 3:go{0}:F\n", robot.Stations[i].RobotStationName);


                if (robot.Stations[i].Positions != 1)
                {
                    for (int j = 1; j < robot.Stations[i].Positions + 1; j++) //create additional points
                    {
                        FormatArgument1.AppendFormat("\t; p{0}{2} {1}: {2}\n", robot.Stations[i].RobotStationName, robot.Stations[i].RobotStationComment, j);
                    }
                    //create final point for additional points
                    FormatArgument1.AppendFormat("\t; p{0}Final {1} - final pos\n", robot.Stations[i].RobotStationName, robot.Stations[i].RobotStationComment);
                }
                else //create normal station points
                {
                    FormatArgument1.AppendFormat("\t; p{0} {1}\n", robot.Stations[i].RobotStationName, robot.Stations[i].RobotStationComment);
                }


                if (robot.Stations[i].StationFreeEnabled)
                {
                    FormatArgument2.AppendFormat("\t; O_{0}_FREE\n", robot.Stations[i].RobotStationName);
                }


            }
            FormatArgument0.Remove(FormatArgument0.Length - 1, 1);
            FormatArgument1.Remove(FormatArgument1.Length - 1, 1);
            FormatArgument2.Remove(FormatArgument2.Length - 1, 1);

            return string.Format(Format, FormatArgument0, FormatArgument1, FormatArgument2).ToString();

        }


        #endregion


    }
}



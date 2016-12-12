using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Revival.Services;
using Revival.Common;
using System.IO;
using Hellgate;

namespace Revival
{
    class Hellpack
    {
        static string _defaultDat = "sp_hellgate_1337";
        const string UsageMsg = " /t\tCook excel tables\n" +
                                " /x\tCook xml files\n" +
                                " /lr\tCook level rules\n" +
                                " /rd\tCook room definitions\n" +
                                " /p\tPack all files into .dat\n" +
                                " /s\tSearch current directory for files\n" +
                                " /e\tDo not pack source files\n" +
                                " /p:%\t% = .idx .dat filename\n" +
                                " /h:%\t% = Path to Hellgate installation\n";

        bool doCookTxt;
        bool doCookXml;
        bool doPackDat; // do pack any referenced files into a hellgate london dat/idx
        bool doSearchCd; // search the current directory for files to cook and pack
        bool doExcludeRaw; // do not pack source files, only cooked versions.
        bool doLevelRules;
        bool doRoomDefinitions;

        string hellgatePath;
        string currentDir;
        string dataDir;
        string dataCommonDir;

        List<string> filesToPack;
        List<string> excelFilesToCook;
        List<string> stringFilesToCook;
        List<string> xmlFilesToCook;
        List<string> levelRulesFilesToSerialize;
        List<string> roomDefinitionFilesSerialize;

        public void Init()
        {
            hellgatePath = Config.HglDir;
            currentDir = Directory.GetCurrentDirectory();
            dataDir = Path.Combine(currentDir, Hellgate.Common.DataPath);
            dataCommonDir = Path.Combine(currentDir, Hellgate.Common.DataCommonPath);

            filesToPack = new List<string>();
            excelFilesToCook = new List<string>();
            stringFilesToCook = new List<string>();
            xmlFilesToCook = new List<string>();
            levelRulesFilesToSerialize = new List<string>();
            roomDefinitionFilesSerialize = new List<string>();

            Console.WriteLine("Hellpack - the Hellgate London compiler.\nWritten by the Revival Team, 2012\nhttp://www.hellgateaus.net");
            Console.WriteLine(String.Empty);
        }

        public Boolean ParseInput(String[] args)
        {
            if(args.Length == 0)
            {
                InitDefault();
                return true;
            }

            foreach (string arg in args)
            {
                if(!HandleFlag(arg) && !HandlePath(arg))
                {                     
                    Console.WriteLine(String.Format("Incorrect argument given: {0}", arg));
                    Console.WriteLine(UsageMsg);
                    return false;
                }
            }

            return true;
        }

        public Boolean HandleFlag(String arg)
        {
            switch (arg)
            {
                case "/t":
                    doCookTxt = true;
                    break;
                case "/x":
                    doCookXml = true;
                    break;
                case "/p":
                    doPackDat = true;
                    break;
                case "/s":
                    doSearchCd = true;
                    break;
                case "/e":
                    doExcludeRaw = true;
                    break;
                case "/lr":
                    doLevelRules = true;
                    break;
                case "/rd":
                    doRoomDefinitions = true;
                    break;
                case "/?":
                case "/help":
                    Console.WriteLine(UsageMsg);
                    return false;
                default:
                    return false;
            }

            return true;
        }

        public Boolean HandlePath(String arg)
        {
            if (arg.StartsWith("/p:"))
            {
                _defaultDat = arg.Replace("/p:", "");
                // Trim in case someone has appended the extention
                if (_defaultDat.EndsWith(".idx"))
                    _defaultDat = _defaultDat.Replace(".idx", "");
                else if (_defaultDat.EndsWith(".dat"))
                    _defaultDat = _defaultDat.Replace(".dat", "");
            }
            else if (arg.StartsWith("/h:"))
            {
                hellgatePath = arg.Replace("/h:", "");
            }
            else if (arg.EndsWith(ExcelFile.ExtensionDeserialised))
            {
                excelFilesToCook.Add(arg);
                doCookTxt = true;
            }
            else if (arg.EndsWith(StringsFile.ExtensionDeserialised))
            {
                stringFilesToCook.Add(arg);
                doCookTxt = true;
            }
            else if (arg.EndsWith(LevelRulesFile.ExtensionDeserialised))
            {
                levelRulesFilesToSerialize.Add(arg);
                doLevelRules = true;
            }
            else if (arg.EndsWith(RoomDefinitionFile.ExtensionDeserialised))
            {
                roomDefinitionFilesSerialize.Add(arg);
                doRoomDefinitions = true;
            }
            else if (arg.EndsWith(XmlCookedFile.ExtensionDeserialised))
            {
                xmlFilesToCook.Add(arg);
                doCookXml = true;
            }
            else
            {
                return false;
            }

            return true;
        }

        private void InitDefault()
        {
            // No arguments defined - this is the default program
            doCookTxt = true;
            doCookXml = true;
            doPackDat = true;
            doSearchCd = true;
            doExcludeRaw = true;
            doLevelRules = true;
            doRoomDefinitions = true;
        }

        public void Execute()
        {
            // Search for Txt files to cook
            if (doSearchCd && doCookTxt)
            {
                string[] result = FileSearchService.SearchForExcelFiles(currentDir);
                if (result != null) excelFilesToCook.AddRange(result);

                result = FileSearchService.SearchForStringFiles(currentDir);
                if (result != null) stringFilesToCook.AddRange(result);
            }

            // Search for Xml files to cook
            if (doSearchCd && doCookXml)
            {
                String[] xmlToCook = FileSearchService.SearchForXmlFiles(currentDir);
                if (xmlToCook != null) xmlFilesToCook.AddRange(xmlToCook);
            }

            // Search for .drl Level Rules files to cook
            if (doSearchCd && doLevelRules)
            {
                IEnumerable<String> drlXmlToCompile = FileSearchService.SearchForDrlXmlFiles(currentDir);
                if (drlXmlToCompile != null) levelRulesFilesToSerialize.AddRange(drlXmlToCompile);
            }

            // Search for .rom Level Rules files to cook
            if (doSearchCd && doRoomDefinitions)
            {
                // todo
            }


            // need for code/name -> row index lookups
            Console.WriteLine("Loading FileManager...");
            FileManager fileManager = new FileManager(hellgatePath);
            fileManager.BeginAllDatReadAccess();
            Console.WriteLine("Loading strings and tables...");
            fileManager.LoadTableFiles();
            fileManager.EndAllDatAccess();


            // Cook Txt files)
            if (doCookTxt)
            {
                CookingService.CookExcelFiles(excelFilesToCook.ToArray(), fileManager);
                CookingService.CookStringFiles(stringFilesToCook.ToArray());
            }

            // Cook Xml files
            if (doCookXml)
            {
                // ensure we have the correct hellgate installation path
                if (Directory.Exists(hellgatePath))
                {
                    if (fileManager.HasIntegrity == false)
                    {
                        Console.WriteLine("Warning: XML could not be cooked - fileManager.Integrity = false");
                    }
                    else
                    {
                        CookingService.CookXmlFiles(xmlFilesToCook.ToArray(), fileManager);
                    }
                }
                else
                {
                    Console.WriteLine("Warning: Can not cook XML, Hellgate London directory missing.");
                }
            }

            // cook .drl Level Rules files
            if (doLevelRules)
            {
                CookingService.CookLevelRulesFiles(levelRulesFilesToSerialize);
            }

            // cook .rom Room Definition files
            if (doRoomDefinitions)
            {
                CookingService.CookRoomDefinitionFiles(roomDefinitionFilesSerialize);
            }

            // Files to pack
            if (doPackDat)
            {
                filesToPack.AddRange(FileSearchService.SearchForFilesToPack(currentDir, doExcludeRaw));
                PackingService.PackDatFile(filesToPack.ToArray(), fileManager, Path.Combine(dataDir, _defaultDat + ".idx"), false);
            }

            return;
        }
    }
}

using Hellgate;
using Revival.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Revival.Services
{
    class CookingService
    {
        // really should make a base for .drl and .rom
        public static void CookLevelRulesFiles(IEnumerable<String> levelRulesFiles)
        {
            Console.WriteLine("Processing level rules...");
            foreach (String filePath in levelRulesFiles)
            {
                try
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(filePath);

                    LevelRulesFile levelRulesFile = new LevelRulesFile(filePath, null);
                    levelRulesFile.ParseXmlDocument(xmlDocument);
                    byte[] fileBytes = levelRulesFile.ToByteArray();

                    File.WriteAllBytes(filePath.Replace(LevelRulesFile.ExtensionDeserialised, LevelRulesFile.Extension), fileBytes);
                }
                catch (Exception e)
                {
                    ExceptionLogger.LogException(e);
                    Console.WriteLine(String.Format("Error: Failed to serialize file {0}", filePath));
                }
            }
        }

        public static void CookRoomDefinitionFiles(IEnumerable<String> levelRulesFiles)
        {
            foreach (String filePath in levelRulesFiles)
            {
                try
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.Load(filePath);

                    RoomDefinitionFile roomDefinitionFile = new RoomDefinitionFile(filePath);
                    roomDefinitionFile.ParseXmlDocument(xmlDocument);
                    byte[] fileBytes = roomDefinitionFile.ToByteArray();

                    File.WriteAllBytes(filePath.Replace(RoomDefinitionFile.ExtensionDeserialised, RoomDefinitionFile.Extension), fileBytes);
                }
                catch (Exception e)
                {
                    ExceptionLogger.LogException(e);
                    Console.WriteLine(String.Format("Error: Failed to serialize file {0}", filePath));
                }
            }
        }


        public static void CookExcelFiles(IEnumerable<String> excelFilesToCook, FileManager fileManager)
        {
            Dictionary<String, ExcelFile> excelFiles = new Dictionary<String, ExcelFile>();

            Console.WriteLine("Reading Excel CSV content...");
            foreach (String excelPath in excelFilesToCook)
            {
                Console.Write(Path.GetFileName(excelPath) + "... ");

                byte[] fileBytes;
                try
                {
                    fileBytes = File.ReadAllBytes(excelPath);
                }
                catch (Exception e)
                {
                    ExceptionLogger.LogException(e);

                    Console.WriteLine("\nFailed to read file contents!\nIgnore and Continue? [Y/N]: ");
                    char c = (char)Console.Read();
                    if (c == 'y' || c == 'Y') continue;

                    return;
                }

                ExcelFile excelFile = new ExcelFile(excelPath);
                try
                {
                    excelFile.LoadCSV(fileBytes);
                }
                catch (Exception e)
                {
                    ExceptionLogger.LogException(e);

                    Console.WriteLine("\nFailed to load CSV contents!\nIgnore and Continue? [Y/N]: ");
                    char c = (char)Console.Read();
                    if (c == 'y' || c == 'Y') continue;

                    return;
                }

                excelFiles.Add(excelFile.StringId, excelFile);
            }

            if (excelFiles.Count == 0) return;

            Console.WriteLine("\nProcessing Excel CSV content...");
            foreach (ExcelFile excelFile in excelFiles.Values)
            {
                Console.WriteLine("Cooking " + Path.GetFileName(excelFile.FilePath));

                try
                {
                    excelFile.ParseCSV(fileManager, excelFiles);
                }
                catch (Exception e)
                {
                    ExceptionLogger.LogException(e);

                    Console.WriteLine("Failed to parse CSV data!\nIgnore and Continue? [Y/N]: ");
                    char c = (char)Console.Read();
                    if (c == 'y' || c == 'Y') continue;

                    return;
                }

                if (excelFile.HasIntegrity == false)
                {
                    Console.WriteLine("Failed to parse CSV data!\nIgnore and Continue? [Y/N]: ");
                    char c = (char)Console.Read();
                    if (c == 'y' || c == 'Y') continue;

                    return;
                }

                byte[] cookedFileBytes;
                try
                {
                    cookedFileBytes = excelFile.ToByteArray();
                }
                catch (Exception e)
                {
                    ExceptionLogger.LogException(e);

                    Console.WriteLine("Failed to serialise CSV data!\nIgnore and Continue? [Y/N]: ");
                    char c = (char)Console.Read();
                    if (c == 'y' || c == 'Y') continue;

                    return;
                }

                if (cookedFileBytes == null)
                {
                    Console.WriteLine("Failed to serialise CSV data!\nIgnore and Continue? [Y/N]: ");
                    char c = (char)Console.Read();
                    if (c == 'y' || c == 'Y') continue;

                    return;
                }

                String savePath = excelFile.FilePath.Replace(ExcelFile.ExtensionDeserialised, ExcelFile.Extension);
                try
                {
                    File.WriteAllBytes(savePath, cookedFileBytes);
                }
                catch (Exception e)
                {
                    ExceptionLogger.LogException(e);

                    Console.WriteLine("Failed to write cooked excel file!\nIgnore and Continue? [Y/N]: ");
                    char c = (char)Console.Read();
                    if (c == 'y' || c == 'Y') continue;

                    return;
                }
            }
        }

        public static bool CookExcelFile(string excelPath, FileManager fileManager)
        {
            byte[] excelBuffer = null;
            try
            {
                excelBuffer = File.ReadAllBytes(excelPath);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                Console.WriteLine(String.Format("Error reading file {0}", excelPath));
                return false;
            }

            ExcelFile excelFile = new ExcelFile(excelPath);
            try
            {
                excelFile.ParseCSV(excelBuffer, fileManager);
            }
            catch (Exception e)
            {
                Console.WriteLine("Critical Error:\n" + e);
                return false;
            }
            if (excelFile.HasIntegrity == false)
            {
                Console.WriteLine(String.Format("Failed to parse excel file {0}", excelPath));
                return false;
            }

            Console.WriteLine(String.Format("Cooking {0}", Path.GetFileNameWithoutExtension(excelPath)));
            excelBuffer = excelFile.ToByteArray();
            if (excelBuffer == null)
            {
                Console.WriteLine(String.Format("Failed to serialize excel file {0}", excelFile.StringId));
                return false;
            }

            String writeToPath = excelPath + ".cooked";
            try
            {
                File.WriteAllBytes(writeToPath, excelBuffer);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                Console.WriteLine(String.Format("Failed to write cooked file {0} ", writeToPath));
                return false;
            }

            return true;
        }

        public static void CookStringFiles(string[] stringFilesToCook)
        {
            foreach (String stringPath in stringFilesToCook)
            {
                CookStringFile(stringPath);
            }
        }

        public static bool CookStringFile(string stringPath)
        {
            byte[] stringsBuffer = null;
            try
            {
                stringsBuffer = File.ReadAllBytes(stringPath);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                Console.WriteLine(String.Format("Error reading file {0}", stringPath));
                return false;
            }

            StringsFile stringsFile = new StringsFile(stringsBuffer, Path.GetFileName(stringPath).ToUpper());
            if (stringsFile.HasIntegrity == false)
            {
                Console.WriteLine(String.Format("Failed to parse strings file {0}", stringPath));
                return false;
            }

            Console.WriteLine(String.Format("Cooking {0}", Path.GetFileNameWithoutExtension(stringPath)));
            stringsBuffer = stringsFile.ToByteArray();
            if (stringsBuffer == null)
            {
                Console.WriteLine(String.Format("Failed to serialize strings file {0}", stringsFile.StringId));
                return false;
            }

            String writeToPath = stringPath + ".cooked";
            try
            {
                File.WriteAllBytes(writeToPath, stringsBuffer);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                Console.WriteLine(String.Format("Failed to write cooked file {0} ", writeToPath));
                return false;
            }

            return true;
        }

        public static void CookXmlFiles(String[] xmlFilesToCook, FileManager fileManager)
        {
            if (xmlFilesToCook.Length <= 0) return;

            Console.WriteLine("Cooking XML Files... Loading Data Tables...");

            foreach (String xmlPath in xmlFilesToCook)
            {
                CookXmlFile(xmlPath, fileManager);
            }
        }

        public static void CookXmlFile(String xmlPath, FileManager fileManager)
        {
            try
            {
                Console.WriteLine(String.Format("Cooking {0}", Path.GetFileNameWithoutExtension(xmlPath)));

                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(xmlPath);

                XmlCookedFile cookedXmlFile = new XmlCookedFile(fileManager, xmlPath);
                byte[] xmlCookedData = cookedXmlFile.CookXmlDocument(xmlDocument);
                File.WriteAllBytes(xmlPath + ".cooked", xmlCookedData);
            }
            catch (Exception ex)
            {
                String error = "Error: Failed to cook XML file: " + ex.Message;
                ExceptionLogger.LogException(ex, error);
                Console.WriteLine(error);
            }
        }
    }
}

using Hellgate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Revival.Services
{
    class FileSearchService
    {

        /// <summary>
        /// Searches the path for raw excel files to cook.
        /// </summary>
        /// <param name="hellgatePath">The path the search.</param>
        /// <returns>Paths found as an array.</returns>
        public static string[] SearchForExcelFiles(string hellgatePath)
        {
            List<string> excelFilesToCook = new List<string>();
            string dataDir = Path.Combine(hellgatePath, "data");
            string dataCommonDir = Path.Combine(hellgatePath, "data_common");
            string excelDataDir = Path.Combine(dataDir, ExcelFile.FolderPath);
            string excelDataCommonDir = Path.Combine(dataCommonDir, ExcelFile.FolderPath);
            string excelWildCard = String.Format("*{0}", ExcelFile.ExtensionDeserialised);

            if (Directory.Exists(excelDataDir))
            {
                string[] result = Directory.GetFiles(excelDataDir, excelWildCard, SearchOption.TopDirectoryOnly);
                excelFilesToCook.AddRange(result);
            }

            if (Directory.Exists(excelDataCommonDir))
            {
                string[] result = Directory.GetFiles(excelDataCommonDir, excelWildCard, SearchOption.TopDirectoryOnly);
                excelFilesToCook.AddRange(result);
            }

            return excelFilesToCook.ToArray();
        }

        public static string[] SearchForStringFiles(string hellgatePath)
        {
            string[] stringPaths = null;
            string dataDir = Path.Combine(hellgatePath, "data");
            string dataCommonDir = Path.Combine(hellgatePath, "data_common");
            string stringDataDir = Path.Combine(dataDir, StringsFile.FolderPath);
            string stringWildCard = String.Format("*{0}", StringsFile.ExtensionDeserialised);

            if (Directory.Exists(stringDataDir))
            {
                stringPaths = Directory.GetFiles(stringDataDir, stringWildCard, SearchOption.AllDirectories);
            }

            return stringPaths;
        }

        public static string[] SearchForXmlFiles(string hellgatePath)
        {
            string[] xmlPaths = null;
            string dataDir = Path.Combine(hellgatePath, "data");

            if (Directory.Exists(dataDir))
            {
                string xmlWildCard = String.Format("*{0}", XmlCookedFile.ExtensionDeserialised);
                xmlPaths = Directory.GetFiles(dataDir, xmlWildCard, SearchOption.AllDirectories);
                xmlPaths = xmlPaths.Where(str => !str.Contains("uix") && !str.EndsWith(LevelRulesFile.ExtensionDeserialised)).ToArray(); // remove uix xml files and .drl.xml files
            }

            return xmlPaths;
        }


        public static IEnumerable<String> SearchForDrlXmlFiles(String hellgatePath)
        {
            String dataDir = Path.Combine(hellgatePath, "data");
            if (!Directory.Exists(dataDir)) return null;

            String xmlWildCard = String.Format("*{0}", LevelRulesFile.ExtensionDeserialised);
            String[] paths = Directory.GetFiles(dataDir, xmlWildCard, SearchOption.AllDirectories);
            return paths;
        }

        public static string[] SearchForFilesToPack(string hellgatePath, bool excludeRaw)
        {
            List<string> filesToPack = new List<string>();
            string dataDir = Path.Combine(hellgatePath, "data");
            string dataCommonDir = Path.Combine(hellgatePath, "data_common");

            if (Directory.Exists(dataDir))
            {
                string[] result = Directory.GetFiles(dataDir, "*", SearchOption.AllDirectories);
                filesToPack.AddRange(result.Where(s => !s.Contains(".idx") && !s.Contains(".dat"))); // ignore existing .dat .idx files
            }

            if (Directory.Exists(dataCommonDir))
            {
                string[] result = Directory.GetFiles(dataCommonDir, "*", SearchOption.AllDirectories);
                filesToPack.AddRange(result);
            }

            if (excludeRaw == true)
            {
                // todo
            }

            return filesToPack.ToArray();
        }
    }
}

using Hellgate;
using Revival.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Revival.Services
{
    class PackingService
    {
        public static bool PackDatFile(IEnumerable<String> filesToPack, FileManager fileManager, String outputPath, bool forceCreateNewIndex)
        {
            IndexFile indexFile = null;
            bool isAppend = false;
            if (!forceCreateNewIndex && File.Exists(outputPath))
            {
                Console.Write("Hellpack has detected an existing index. Append to previous index? [Y/N]: ");
                char ans = (char)Console.Read();
                if (ans == 'y' || ans == 'Y')
                {
                    indexFile = new IndexFile(outputPath, File.ReadAllBytes(outputPath));
                    isAppend = true;
                }
            }

            if (indexFile == null) indexFile = new IndexFile(outputPath);

            foreach (String filePath in filesToPack)
            {
                DateTime lastModified = File.GetLastWriteTime(filePath);

                // if we're appending, check if we've already added this file by checking the modified time
                if (isAppend)
                {
                    PackFileEntry fileEntry = indexFile.GetFileEntry(filePath);
                    if (fileEntry != null && fileEntry.LastModified == lastModified) continue;
                }

                String fileName = Path.GetFileName(filePath);
                String directory = Path.GetDirectoryName(filePath);
                int dataCursor = directory.IndexOf("data");
                directory = directory.Remove(0, dataCursor) + "\\";

                byte[] buffer;
                try
                {
                    buffer = File.ReadAllBytes(filePath);
                }
                catch (Exception ex)
                {
                    ExceptionLogger.LogException(ex);
                    Console.WriteLine(String.Format("Warning: Could not read file {0}", filePath));
                    continue;
                }

                Console.WriteLine("Packing " + directory + fileName);
                if (indexFile.AddFile(directory, fileName, buffer, lastModified) == false)
                {
                    Console.WriteLine("Warning: Failed to add file to index...");
                }
            }

            foreach (PackFile file in fileManager.IndexFiles)
                file.EndDatAccess();

            string thisPack = Path.GetFileNameWithoutExtension(outputPath);
            byte[] indexBytes = indexFile.ToByteArray();
            Crypt.Encrypt(indexBytes);
            Console.WriteLine("Writing " + thisPack);
            try
            {
                File.WriteAllBytes(outputPath, indexBytes);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                Console.WriteLine(String.Format("Fatal error: Could not write index {0}", thisPack));
                return false;
            }

            Console.WriteLine(String.Format("{0} generation complete.", thisPack));
            return true;
        }
    }
}

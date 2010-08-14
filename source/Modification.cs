﻿using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using Reanimator.ExcelDefinitions;
using Reanimator.Forms;
using System.Data;
using System.Linq;

namespace Reanimator
{
    public class Modification
    {
        TableDataSet _tableDataSet;
        List<Index> _index;

        public List<Package> ModPackage { get; set; }
        public string ModPackPath { get { return Config.HglDir + "\\modpacks\\"; } }

        public Modification(TableDataSet tableDataSet)
        {
            _tableDataSet = tableDataSet;
            ModPackage = new List<Package>();
        }
        public void Open(ProgressForm progress, Object argument)
        {
            // Unzip the package
            string path = argument as string;
            bool unzipResult = Unzip(path);
            if (!unzipResult) return;

            string modDir = PathTools.FileNameFromPath(path, true) + "\\";
            path = ModPackPath + modDir + "Config.xml";

            // Open the Config file
            try
            {
                using (TextReader textReader = new StreamReader(path))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Package));
                    Package package = serializer.Deserialize(textReader) as Package;
                    ModPackage.Add(package);
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex, "Modification.Open");
                return;
            }

            Package casePackage = ModPackage.Last();
            if (casePackage == null) return;

            // Parse each pack
            foreach (Pack pack in casePackage.Pack)
            {
                pack.Apply = (pack.Type == "required" || pack.Type == "recommended") ? true : false;
                pack.Path = ModPackPath + modDir + pack.Dir + "\\";
                string imagePath = pack.Path + "preview.png";

                if (System.IO.File.Exists(imagePath))
                {
                    pack.Image = Image.FromFile(imagePath);
                }

                string repackDir = pack.Path + "repack\\";
                if (Directory.Exists(repackDir))
                {
                    pack.Repack = Directory.GetFiles(repackDir, "*", SearchOption.AllDirectories);
                    for (int i = 0; i < pack.Repack.Length; i++)
                    {
                        pack.Repack[i] = pack.Repack[i].Remove(0, repackDir.Length);
                    }
                }

                string unpackDir = pack.Path + "unpack\\";
                if (Directory.Exists(unpackDir))
                {
                    pack.Unpack = Directory.GetFiles(unpackDir, "*", SearchOption.AllDirectories);
                    for (int i = 0; i < pack.Unpack.Length; i++)
                    {
                        pack.Unpack[i] = pack.Unpack[i].Remove(0, unpackDir.Length);
                    }
                }

                string scriptDir = pack.Path + "scripts\\";
                if (!Directory.Exists(scriptDir)) continue; // no scripts? continue
                string[] scripts = Directory.GetFiles(scriptDir, "*.xml");
                pack.Scripts = new Script[scripts.Length];

                for (int i = 0; i < scripts.Length; i++)
                {
                    // Open the script file
                    try
                    {
                        using (TextReader textReader = new StreamReader(scripts[i]))
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(Script));
                            Script script = serializer.Deserialize(textReader) as Script;
                            pack.Scripts[i] = script;
                        }
                    }
                    catch (Exception exception)
                    {
                        ExceptionLogger.LogException(exception, "Modification.Open");
                        return;
                    }
                }
            }
        }
        public void Apply(ProgressForm progress, Object argument)
        {
            foreach (Index index in _index)
            {

            }

            foreach (Package package in ModPackage)
            {
                foreach (Pack pack in package.Pack)
                {
                    if (!pack.Apply) continue;
                    
                    //repack files

                    //unpack files

                    //manipulate files

                }
            }
        }
        public void Save(ProgressForm progress, Object argument)
        {

        }
        private bool Unzip(string path)
        {
            try
            {
                using (Ionic.Zip.ZipFile zipFile = new Ionic.Zip.ZipFile(path))
                {
                    zipFile.ExtractAll(ModPackPath, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
                }

                return true;
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex, "Modification.Unzip");
                return false;
            }
        }
        private bool Manipulate(File file)
        {
            try
            {
                DataTable table = _tableDataSet.XlsDataSet.Tables[file.ID];
                ExcelFile excel = _tableDataSet.TableFiles.GetExcelTableFromId(file.ID);

                foreach (Entity entity in file.Entities)
                {
                    foreach (Attribute attribute in entity.Attributes)
                    {
                        int step = 0;
                        int min = 1;
                        int max = 0;
                        int last = 0; //for recursive function
                        string function;
                        int[] list;

                        //determine the range
                        if (entity.ID.Contains(","))
                        {
                            string[] explode = entity.ID.Split(',');
                            list = new int[explode.Length];

                            for (int i = 0; i < list.Length; i++)
                            {
                                list[i] = Convert.ToInt32(explode[i]);
                            }
                        }
                        else
                        {
                            if (entity.ID.Contains("*"))
                            {
                                min = 0;
                                max = table.Rows.Count - 1;
                            }
                            else if (entity.ID.Contains("-"))
                            {
                                int idx = entity.ID.IndexOf('-');
                                int len = entity.ID.Length - idx - 1;
                                min = Convert.ToInt32(entity.ID.Substring(0, idx));
                                max = Convert.ToInt32(entity.ID.Substring(idx + 1, len));
                            }
                            else
                            {
                                min = Convert.ToInt32(entity.ID);
                                max = Convert.ToInt32(entity.ID);
                            }

                            int listlen = max - min + 1;
                            list = new int[listlen];
                            int i = 0;

                            for (int row = min; row <= max; row++)
                            {
                                list[i++] = row;
                            }
                        }

                        //determine function
                        if (attribute.Bit != null)
                        {
                            function = "bitwise";
                        }
                        else if (attribute.Operation == null)
                        {
                            function = "replace";
                        }
                        else if (attribute.Operation.Contains("*"))
                        {
                            function = "multiply";
                        }
                        else if (attribute.Operation.Contains("/"))
                        {
                            function = "divide";
                        }
                        else if (attribute.Operation.Contains("+"))
                        {
                            string s = attribute.Operation.Remove(0);
                            step = Convert.ToInt32(s);
                            function = "recursive";
                        }
                        else if (attribute.Operation.Contains("-"))
                        {
                            step = Convert.ToInt32(attribute.Operation);
                            function = "recursive";
                        }
                        else
                        {
                            continue; // syntax error
                        }

                        //main loop, alters the dataset
                        foreach (int row in list)
                        {
                            object obj = null;
                            string col = attribute.ID;
                            Type type = table.Columns[col].DataType;
                            DataRow dataRow;

                            //add blank/reserved rows if required
                            while (row >= table.Rows.Count)
                            {
                                dataRow = table.NewRow();
                                table.Rows.Add(dataRow);
                            }

                            dataRow = table.Rows[row];

                            switch (function)
                            {
                                case "replace":
                                    obj = attribute.Value;
                                    break;

                                case "multiply":
                                    if (type.Equals(typeof(int)))
                                        obj = (int)dataRow[col] * Convert.ToInt32(attribute.Value);
                                    else if (type.Equals(typeof(float)))
                                        obj = (float)dataRow[col] * Convert.ToSingle(attribute.Value);
                                    break;

                                case "divide":
                                    if (type.Equals(typeof(int)))
                                        obj = (int)dataRow[col] / Convert.ToInt32(attribute.Value);
                                    else if (type.Equals(typeof(float)))
                                        obj = (float)dataRow[col] / Convert.ToSingle(attribute.Value);
                                    break;

                                case "bitwise":
                                    uint bit = (uint)Enum.Parse(type, attribute.Bit, true);
                                    uint mask = (uint)dataRow[col];
                                    bool flick = Convert.ToBoolean(attribute.Value);
                                    bool current = (mask & bit) > 0;
                                    if (flick != current)
                                        obj = mask ^= bit;
                                    else
                                        obj = mask;
                                    break;

                                case "recursive":
                                    if (row.Equals(min))//first time only
                                    {
                                        if (type.Equals(typeof(int)))
                                        {
                                            obj = Convert.ToInt32(attribute.Value);
                                            last = (int)obj;
                                        }
                                    }
                                    else 
                                    {
                                        last += step;
                                        obj = last;
                                    }
                                    break;
                            }
                            dataRow[col] = obj;
                        } //end main loop
                    } //attribute
                } //entity

                return true;
            }
            catch (Exception exception)
            {
                ExceptionLogger.LogException(exception, "Modification.Manipulate");
                return false;
            }
        }

        [XmlRoot("package")]
        public class Package
        {
            [XmlElement("pack", typeof(Pack))]
            public Pack[] Pack { get; set; }

            [XmlIgnore]
            public string Path { get; set; }
            [XmlIgnore]
            public string Title { get; set; }
            [XmlIgnore]
            public string Config { get; set; } // config.xml
        }
        public class Pack
        {
            [XmlAttribute("id")]
            public string Dir { get; set; }

            [XmlElement("title")]
            public string Title { get; set; }
            [XmlElement("version")]
            public string Version { get; set; }
            [XmlElement("author")]
            public string Author { get; set; }
            [XmlElement("website")]
            public string Website { get; set; }
            [XmlElement("notes")]
            public string Notes { get; set; }
            [XmlElement("type")]
            public string Type { get; set; }

            [XmlIgnore]
            public Image Image;
            [XmlIgnore]
            public bool Apply { get; set; }
            [XmlIgnore]
            public string Path { get; set; }
            [XmlIgnore]
            public Script[] Scripts { get; set; }
            [XmlIgnore]
            public string[] Repack { get; set; }
            [XmlIgnore]
            public string[] Unpack { get; set; }
        }
        [XmlRoot("modification")]
        public class Script
        {
            [XmlElement("file", typeof(File))]
            public File[] Files { get; set; }

            [XmlIgnore]
            public string Path { get; set; }
        }
        public class File
        {
            [XmlAttribute("id")]
            public string ID { get; set; }

            [XmlElement("entity", typeof(Entity))]
            public Entity[] Entities { get; set; }
        }
        public class Entity
        {
            [XmlAttribute("id")]
            public string ID { get; set; }

            [XmlElement("attribute", typeof(Attribute))]
            public Attribute[] Attributes { get; set; }
        }
        public class Attribute
        {
            [XmlAttribute("id")]
            public string ID { get; set; }

            [XmlAttribute("bit")]
            public string Bit { get; set; }

            [XmlAttribute("param")]
            public string Operation { get; set; }

            [XmlText]
            public string Value { get; set; }
        }
    }
}

﻿using System;
using System.Data;
using System.IO;
using Hellgate;
using MediaWiki.Articles;
using MediaWiki.Parser;
using MediaWiki.Parser.Class;

namespace MediaWiki
{
    class Program
    {
        static void Main(string[] args)
        {
            var manager = new FileManager(@"C:\Hellgate");
            manager.BeginAllDatReadAccess();
            manager.LoadTableFiles();
            manager.EndAllDatAccess();

            //TODO: beam damage types (listed as direct but apparently uses some other calculation)
            //TODO: sword attack rates

            // *****while this message is still here, correct the following manually after every item export*****
            //TODO: fix stats that are super inaccurate (manually changed in DB for now):
            //Beryl Dragon - damage is 0-1, missing stream depletion -20
            
            

            args = new[] { "ITEMS" };

            //new Items(manager).WriteAllUniqueLegendaryItemPages();
            //return;

            string sqlStatement;
            WikiScript script;
            foreach (string arg in args)
            {                
                switch (arg)
                {
                    case "ACHIEVEMENTS":
                        script = new Achievements(manager);
                        break;
                    case "AFFIXES":
                        script = new Affixes(manager);
                        break;
                    case "ITEMLEVELS":
                        script = new ItemLevels(manager);
                        break;
                    case "MONSTERS":
                        script = new Monsters(manager);
                        break;
                    case "PVPRANKS":
                        script = new PVPRanks(manager);
                        break;
                    case "LEVELSCALING":
                        script = new LevelScaling(manager);
                        break;
                    case "ITEMS":
                        script = new Items(manager);
                        break;
                    case "ITEM_QUALITY":
                        script = new ItemQuality(manager);
                        break;
                    case "TREASURE":
                        script = new Treasure(manager);
                        break;
                    case "MONSTER_QUALITY":
                        script = new MonsterQuality(manager);
                        break;
                    case "RECIPES":
                        script = new Recipes(manager);
                        break;
                    default:
                        throw new Exception("Unknown WikiScript: " + arg);
                }

                sqlStatement = script.ExportTableInsertScript();

                File.WriteAllText(arg.ToLower() + ".sql", sqlStatement);
            }

            return;
        }
    }
}

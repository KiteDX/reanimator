using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using Hellgate;
using Revival.Common;
using ExceptionLogger = Revival.Common.ExceptionLogger;


namespace Revival
{
    public static class Program
    {
        static void Main(string[] args)
        {
            Hellpack hellpack = new Hellpack();
            hellpack.Init();
            if (hellpack.ParseInput(args))
            { 
                hellpack.Execute();
            }
        }        
    }
}

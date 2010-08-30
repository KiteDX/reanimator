﻿using System;
using System.Runtime.InteropServices;
using ExcelOutput = Reanimator.ExcelFile.ExcelOutputAttribute;

namespace Reanimator.ExcelDefinitions
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    class MaterialsCollisionTCv4Row
    {
        ExcelFile.TableHeader header;
        [ExcelOutput(SortId = 1)]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string name;

        public Int32 materialNumber;
        [ExcelOutput(IsBool = true)]
        public Int32 floor;//bool
        public Int32 TCv4_1;
        public Int32 mapsTo;//idx
        public float directOcclusion;
        public float reverbOcclusion;
        public Int32 debugColor;//idx
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string TCv4_string;
    }
}
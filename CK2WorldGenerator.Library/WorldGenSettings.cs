using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK2WorldGenerator.Library
{
    public class WorldGenSettings
    {
        public string RandomSeed = "";
        public int MapWidth = 3072;
        public int MapHeight = 2048;
        public byte SeaLevel = 111;
        public byte BaseHeight = 128;
        public byte HeightScale = 64;
        public double Roughness = 0.65;
        public double Noise = 0.05;
        public bool OceanBorder = true;
        public int LandProvincesMin = 1000;
        public int LandProvincesMax = 1000;
        public int SeaProvincesMin = 500;
        public int SeaProvincesMax = 500;
        public int CultureGroupsMin = 10;
        public int CultureGroupsMax = 25;
        public int CulturesMin = 30;
        public int CulturesMax = 100;
    }
}

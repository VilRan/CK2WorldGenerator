using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK2WorldGenerator.Library
{
    public class WorldGenerator
    {
        private delegate Color ColorFunction(int x, int y);

        private static Color TerrainWater = Color.FromArgb(69, 91, 186);
        private static Color TerrainPlains = Color.FromArgb(86, 124, 27);
        private static Color TerrainForest = Color.FromArgb(0, 86, 6);
        private static Color TerrainSnowlessMountain = Color.FromArgb(65, 42, 17);
        private static Color TerrainSnowyMountain = Color.FromArgb(155, 155, 155);
        private static Color TerrainVerySnowyMountain = Color.FromArgb(255, 255, 255);
        private static Color TerrainDesert = Color.FromArgb(206, 169, 99);
        private static Color TerrainCoastalDesert = Color.FromArgb(130, 158, 75);
        private static Color TerrainFarmlands = Color.FromArgb(138, 11, 26);
        private static Color TerrainArctic = Color.FromArgb(13, 96, 62);
        private static Color TerrainJungle = Color.FromArgb(40, 180, 149);
        private static Color TerrainDesertMountain = Color.FromArgb(86, 46, 0);
        private static Color TerrainSandyMountain = Color.FromArgb(112, 74, 31);
        private static Color TerrainSteppe = Color.FromArgb(255, 186, 0);

        private static Color RiversLand = Color.FromArgb(255, 255, 255);
        private static Color RiversWater = Color.FromArgb(255, 0, 128);

        public Bitmap TopologyMap;
        public Bitmap TerrainMap;
        public Bitmap RiversMap;
        public Bitmap ProvincesMap;
        public Bitmap CulturesMap;
        public IGeneratorFrontend Frontend;
        public List<Province> LandProvinces;
        public List<Province> SeaProvinces;
        public List<CultureGroup> CultureGroups;
        public List<Culture> Cultures;
        public MapGenerator MapGenerator;
        //public ProceduralHeightmap MapGenerator;
        public WorldGenSettings Settings = new WorldGenSettings();

        private Random Random;
        private Province[,] ProvinceGrid;
        private List<Province>[,] LandProvinceSectors;
        private List<Province>[,] SeaProvinceSectors;
        private int SectorWidth = 64;
        private int SectorHeight = 64;
        private int SectorsX;
        private int SectorsY;

        public WorldGenerator(IGeneratorFrontend frontend)
        {
            Frontend = frontend;

            int mapWidth = Settings.MapWidth;
            int mapHeight = Settings.MapHeight;
            SectorsX = mapWidth / SectorWidth;
            SectorsY = mapHeight / SectorHeight;
            TopologyMap = new Bitmap(mapWidth, mapHeight);
            TerrainMap = new Bitmap(mapWidth, mapHeight);
            RiversMap = new Bitmap(mapWidth, mapHeight);
            ProvincesMap = new Bitmap(mapWidth, mapHeight);
            CulturesMap = new Bitmap(mapWidth, mapHeight);
            //MapGenerator = new ProceduralHeightmap(mapWidth, mapHeight, 128, 64, 0.65, 0.05, false, false, true);
            MapGenerator = new MapGenerator(Settings);
            ProvinceGrid = new Province[mapWidth, mapHeight];
        }

        public void GenerateWorld()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            AddLogEntry("Began generating a new world...");

            SeedRandom();
            GenerateHeightmap();
            GenerateRainmap();

            Task topologyTask = new Task(() => DrawTopology());
            topologyTask.Start();
            Task terrainTask = new Task(() => DrawTerrain());
            terrainTask.Start();
            Task riverTask = new Task(() => DrawRivers());
            riverTask.Start();
            
            GenerateProvinces();
            
            Task provincesTask = new Task(() => DrawProvinces());
            provincesTask.Start();
            
            GenerateCultures();
            DrawCulturesPreview();

            Task.WaitAll(topologyTask, terrainTask, riverTask, provincesTask);
            stopwatch.Stop();
            AddLogEntry("Finished! (" + stopwatch.Elapsed.TotalSeconds + " s)");
            AddLogEntry("--------------------------------------------------------------------------------", false);
        }

        private void AddLogEntry(string text, bool showTime = true)
        {
            if (showTime)
                Frontend.AddLogEntry(DateTime.Now.TimeOfDay + ": " + text + Environment.NewLine);
            else
                Frontend.AddLogEntry(text + Environment.NewLine);
        }

        private void SeedRandom()
        {
            string seedText = Settings.RandomSeed;
            if (seedText == "")
                Random = new Random();
            else
            {
                int seed = 0;
                foreach (char c in seedText)
                    seed += c;
                Random = new Random(seed);
            }
        }

        private void GenerateHeightmap()
        {
            AddLogEntry("Began generating heightmap...");
            MapGenerator.GenerateHeightmap(Random);
            //MapGenerator.Generate(Random);
            AddLogEntry("Finished generating heightmap...");
        }

        private void GenerateRainmap()
        {
            AddLogEntry("Began generating rainmap...");
            MapGenerator.GenerateRainmap();
            AddLogEntry("Finished generating rainmap.");
        }

        private void GenerateBitmap(Bitmap bitmap, ColorFunction colorFunction)
        {
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            int stride = data.Stride;
            unsafe
            {
                byte* pointer = (byte*)data.Scan0;
                for (int x = 0; x < bitmap.Width; x++)
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        Color color = colorFunction(x, y);
                        pointer[x * 3 + y * stride] = color.B;
                        pointer[x * 3 + y * stride + 1] = color.G;
                        pointer[x * 3 + y * stride + 2] = color.R;
                    }
            }
            bitmap.UnlockBits(data);
        }

        private void DrawTopology()
        {
            AddLogEntry("Began drawing topology.bmp...");
            ColorFunction colorFunction = delegate (int x, int y)
            {
                byte altitude = (byte)MapGenerator.GetAltitude(x, y);
                return Color.FromArgb(altitude, altitude, altitude);
            };
            GenerateBitmap(TopologyMap, colorFunction);
            AddLogEntry("Finished drawing topology.bmp.");
        }

        private void DrawTerrain()
        {
            AddLogEntry("Began drawing terrain.bmp...");
            int sealevel = Settings.SeaLevel;
            ColorFunction colorFunction = delegate (int x, int y)
            {
                byte altitude = (byte)MapGenerator.GetAltitude(x, y);
                double temperature = MapGenerator.GetTemperature(x, y);
                return GetTerrain(altitude, temperature);
            };
            GenerateBitmap(TerrainMap, colorFunction);
            AddLogEntry("Finished drawing terrain.bmp.");
        }

        private Color GetTerrain(byte altitude, double temperature)
        {
            if (altitude >= 160)
                return GetHighTerrain(temperature);
            else if (altitude >= Settings.SeaLevel)
                return GetLowTerrain(temperature);
            return TerrainWater;
        }

        private Color GetHighTerrain(double temperature)
        {
            if (temperature > 0)
                return TerrainSnowlessMountain;
            else if (temperature > -10)
                return TerrainSnowyMountain;
            else
                return TerrainVerySnowyMountain;
        }

        private Color GetLowTerrain(double temperature)
        {
            if (temperature > 30)
                return TerrainDesert;
            else if (temperature > 0)
                return TerrainPlains;
            else
                return TerrainArctic;
        }

        private void DrawRivers()
        {
            AddLogEntry("Began drawing rivers.bmp...");
            int sealevel = Settings.SeaLevel;
            ColorFunction colorFunction = delegate (int x, int y)
            {
                byte altitude = (byte)MapGenerator.GetAltitude(x, y);
                Color color = RiversWater;
                if (altitude >= sealevel)
                    color = RiversLand;
                return color;
            };
            GenerateBitmap(RiversMap, colorFunction);
            AddLogEntry("Finished drawing rivers.bmp.");
        }

        private void GenerateProvinces()
        {
            AddLogEntry("Began generating provinces...");

            int sealevel = Settings.SeaLevel;
            int mapWidth = Settings.MapWidth;
            int mapHeight = Settings.MapHeight;
            LandProvinceSectors = new List<Province>[SectorsX, SectorsY];
            SeaProvinceSectors = new List<Province>[SectorsX, SectorsY];
            for (int x = 0; x < SectorsX; x++)
                for (int y = 0; y < SectorsY; y++)
                {
                    LandProvinceSectors[x, y] = new List<Province>();
                    SeaProvinceSectors[x, y] = new List<Province>();
                }

            Color minColor = Color.FromArgb(0, 64, 0);
            Color maxColor = Color.FromArgb(255, 255, 63);
            int min = Settings.LandProvincesMin;
            int max = Settings.LandProvincesMax + 1;
            int n = Random.Next(min, max);
            LandProvinces = new List<Province>(n);
            for (int i = 1; i <= n; i++)
            {
                Province province = new Province();
                int x = Random.Next(mapWidth);
                int y = Random.Next(mapHeight);
                while (MapGenerator.GetAltitude(x, y) <= sealevel)
                {
                    x = Random.Next(mapWidth);
                    y = Random.Next(mapHeight);
                }
                province.Position = new Point(x, y);
                //province.Color = Color.FromArgb(Random.Next(256), Random.Next(64, 256), Random.Next(0, 64));
                province.Color = ColorUtility.CreateColorFromRange(i, n, minColor, maxColor);
                LandProvinces.Add(province);
                LandProvinceSectors[x / SectorWidth, y / SectorHeight].Add(province);
            }

            minColor = Color.FromArgb(0, 0, 64);
            maxColor = Color.FromArgb(255, 63, 255);
            min = Settings.SeaProvincesMin;
            max = Settings.SeaProvincesMax + 1;
            n = Random.Next(min, max);
            SeaProvinces = new List<Province>(n);
            for (int i = 1; i <= n; i++)
            {
                Province province = new Province();
                int x = Random.Next(mapWidth);
                int y = Random.Next(mapHeight);
                while (MapGenerator.GetAltitude(x, y) > sealevel)
                {
                    x = Random.Next(mapWidth);
                    y = Random.Next(mapHeight);
                }
                province.Position = new Point(x, y);
                //province.Color = Color.FromArgb(Random.Next(256), Random.Next(0, 64), Random.Next(64, 256));
                province.Color = ColorUtility.CreateColorFromRange(i, n, minColor, maxColor);
                SeaProvinces.Add(province);
                SeaProvinceSectors[x / SectorWidth, y / SectorHeight].Add(province);
            }

#if DEBUG
            AssignProvincesToGrid(0, 0, mapWidth, mapHeight);
#else
            int divsX = 2;
            int divsY = 2;
            int blockWidth = mapWidth / divsX;
            int blockHeight = mapHeight / divsY;
            Task[] tasks = new Task[divsX * divsY];
            for (int x = 0; x < divsX; x++)
                for (int y = 0; y < divsY; y++)
                {
                    int i = x + y * divsX;
                    int blockX = x * blockWidth;
                    int blockY = y * blockHeight;

                    tasks[i] = new Task(() => AssignProvincesToGrid(blockX, blockY, blockWidth, blockHeight));
                    tasks[i].Start();
                }
            Task.WaitAll(tasks);
#endif
            AddLogEntry("Finished generating provinces.");
        }

        private void AssignProvincesToGrid(int startX, int startY, int width, int height)
        {
            int sealevel = Settings.SeaLevel;
            int maxX = startX + width;
            int maxY = startY + height;
            for (int x = startX; x < maxX; x++)
                for (int y = startY; y < maxY; y++)
                {
                    Province province = LandProvinces[0];
                    int shortestDistance = int.MaxValue;
                    if (MapGenerator.GetAltitude(x, y) > sealevel)
                        foreach (Province test in GetNearbyProvinces(x, y, LandProvinceSectors))
                        {
                            int testDistance = test.DistanceSquared(x, y);
                            if (testDistance < shortestDistance)
                            {
                                province = test;
                                shortestDistance = testDistance;
                            }
                        }
                    else
                        foreach (Province test in GetNearbyProvinces(x, y, SeaProvinceSectors))
                        {
                            int testDistance = test.DistanceSquared(x, y);
                            if (testDistance < shortestDistance)
                            {
                                province = test;
                                shortestDistance = testDistance;
                            }
                        }
                    ProvinceGrid[x, y] = province;
                }
        }

        private IEnumerable<Province> GetNearbyProvinces(int x, int y, List<Province>[,] sectors)
        {
            int n = 0, range = 0;
            int sectorX = x / SectorWidth;
            int sectorY = y / SectorHeight;
            do
            {
                range++;
                foreach (List<Province> sector in GetNearbySectors(sectorX, sectorY, sectors, range))
                    foreach (Province province in sector)
                    {
                        yield return province;
                        n++;
                    }
            }
            while (n == 0);
        }

        private IEnumerable<List<Province>> GetNearbySectors(int sectorX, int sectorY, List<Province>[,] sectors, int range)
        {
            int minX = Math.Max(sectorX - range, 0);
            int maxX = Math.Min(sectorX + range, SectorsX - 1);
            int minY = Math.Max(sectorY - range, 0);
            int maxY = Math.Min(sectorY + range, SectorsY - 1);
            for (int x = minX; x <= maxX; x++)
                for (int y = minY; y <= maxY; y++)
                    yield return sectors[x, y];
        }

        private void DrawProvinces()
        {
            AddLogEntry("Began drawing provinces.bmp...");
            ColorFunction colorFunction = delegate (int x, int y)
            {
                return ProvinceGrid[x, y].Color;
            };
            GenerateBitmap(ProvincesMap, colorFunction);
            AddLogEntry("Finished drawing provinces.bmp.");
        }

        private void DrawCulturesPreview()
        {
            AddLogEntry("Began drawing cultures preview...");
            ColorFunction colorFunction = delegate (int x, int y)
            {
                Province province = ProvinceGrid[x, y];
                if (province.Culture != null)
                    return province.Culture.Color;
                return Color.Black;
            };
            GenerateBitmap(CulturesMap, colorFunction);
            AddLogEntry("Finished drawing cultures preview.");
        }

        private void GenerateCultures()
        {
            AddLogEntry("Began generating cultures...");
            int colorRange = 32;
            int n = Random.Next(Settings.CultureGroupsMin, Settings.CultureGroupsMax);
            CultureGroups = new List<CultureGroup>(n);
            Cultures = new List<Culture>(Settings.CultureGroupsMax);
            for (int i = 0; i < n; i++)
            {
                CultureGroup group = new CultureGroup();
                group.BaseColor = Color.FromArgb(
                    colorRange + Random.Next(256 - colorRange * 2),
                    colorRange + Random.Next(256 - colorRange * 2),
                    colorRange + Random.Next(256 - colorRange * 2));
                do
                    group.Origin = LandProvinces[Random.Next(LandProvinces.Count)];
                while (group.Origin.CultureGroup != null);
                group.Origin.CultureGroup = group;
                AddCulture(group); // Guarantee at least one culture per group.
                CultureGroups.Add(group);
            }
            
            n = Random.Next(Settings.CulturesMin, Settings.CulturesMax);
            n -= Cultures.Count;
            for (int i = 0; i < n; i++)
            {
                CultureGroup group = CultureGroups[Random.Next(CultureGroups.Count)];
                AddCulture(group);
            }
            

            foreach (Province province in LandProvinces)
            {
                Culture closestCulture = null;
                int closestDistance = int.MaxValue;
                foreach (Culture culture in Cultures)
                {
                    int testDistance = culture.Origin.DistanceSquared(province.Position);
                    if (testDistance < closestDistance)
                    {
                        closestCulture = culture;
                        closestDistance = testDistance;
                    }
                }
                province.Culture = closestCulture;
            }

            AddLogEntry("Finished generating cultures.");
        }

        private void AddCulture(CultureGroup group)
        {
            int colorRange = 32;
            Culture culture = new Culture();
            culture.Group = group;
            culture.Color = Color.FromArgb(
                group.BaseColor.R + Random.Next(-colorRange, colorRange),
                group.BaseColor.G + Random.Next(-colorRange, colorRange),
                group.BaseColor.B + Random.Next(-colorRange, colorRange));
            /*
            do
                culture.Origin = LandProvinces[Random.Next(LandProvinces.Count)];
            while (culture.Origin.Culture != null);
            culture.Origin.Culture = culture;
            Province origin = LandProvinces[0];
            */
            int smallestScore = int.MaxValue;
            foreach (Province province in LandProvinces)
            {
                if (province.Culture == null)
                {
                    int score = province.DistanceSquared(culture.Group.Origin);
                    score += Random.Next(0, 100000);
                    if (score < smallestScore)
                    {
                        culture.Origin = province;
                        smallestScore = score;
                    }
                }
            }
            culture.Origin.Culture = culture;

            group.Cultures.Add(culture);
            Cultures.Add(culture);
        }

        private void GenerateTitles()
        {

        }
    }
}

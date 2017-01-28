using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK2WorldGenerator.Library
{
    public class MapGenerator
    {
        public double[,] Altitude;
        public double[,] Clouds;
        public double[,] Rain;

        private WorldGenSettings Settings;
        private double[,] CloudsTemp;

        public MapGenerator(WorldGenSettings settings)
        {
            Settings = settings;
        }

        public void GenerateHeightmap(Random random)
        {
            int mapWidth = Settings.MapWidth;
            int mapHeight = Settings.MapHeight;
            Altitude = new double[mapWidth, mapHeight];
            double[,] heightmap = new DiamondSquare(random, mapWidth, mapHeight, 1, Settings.Roughness, false, false).GenerateHeightmap();
            
            double heightmapWidth = heightmap.GetUpperBound(0);
            double heightmapDepth = heightmap.GetUpperBound(1);
            double widthRatio = heightmapWidth / mapWidth;
            double heightRatio = heightmapDepth / mapHeight;
            int middleX = mapWidth / 2;
            int middleY = mapHeight / 2;
            int edgeSizeX = mapWidth / 4;
            int edgeSizeY = mapHeight / 4;
            int smoothThresholdX = middleX - edgeSizeX;
            int smoothThresholdY = middleY - edgeSizeY;
            double heightScale = Settings.HeightScale;
            double floorHeight = Settings.BaseHeight - heightScale;

            for (int x = 0; x < mapWidth; x++)
                for (int y = 0; y < mapHeight; y++)
                {
                    double multiplier = 1.0;
                    if (Settings.OceanBorder)
                    {
                        int dx = Math.Abs(middleX - x);
                        int dy = Math.Abs(middleY - y);
                        if (dx >= smoothThresholdX)
                            multiplier *= 1 - (dx - smoothThresholdX) / (double)(edgeSizeX);
                        if (dy >= smoothThresholdY)
                            multiplier *= 1 - (dy - smoothThresholdY) / (double)(edgeSizeY);
                    }
                    if (Settings.Noise > 0)
                        multiplier *= random.NextDouble(1 - Settings.Noise, 1);

                    int x2 = (int)Math.Round((x) * widthRatio);
                    int y2 = (int)Math.Round((y) * heightRatio);
                    Altitude[x, y] = floorHeight + (heightmap[x2, y2] * heightScale + heightScale) * multiplier;
                }
            
        }
        /*
        private void GenerateHeightmap(Random random, double[,] heightmap, int startX, int startY, int width, int height)
        {
            int mapWidth = Settings.MapWidth;
            int mapHeight = Settings.MapHeight;
            double heightmapWidth = heightmap.GetUpperBound(0);
            double heightmapDepth = heightmap.GetUpperBound(1);
            double widthRatio = heightmapWidth / mapWidth;
            double heightRatio = heightmapDepth / mapHeight;
            bool smoothEdges = Settings.OceanBorder;
            int middleX = mapWidth / 2;
            int middleY = mapHeight / 2;
            int edgeSizeX = mapWidth / 4;
            int edgeSizeY = mapHeight / 4;
            int smoothThresholdX = middleX - edgeSizeX;
            int smoothThresholdY = middleY - edgeSizeY;
            double noise = Settings.Noise;
            double heightScale = Settings.HeightScale;
            double floorHeight = Settings.BaseHeight - heightScale;

            int maxX = startX + width;
            int maxY = startY + height;
            for (int x = startX; x < maxX; x++)
                for (int y = startY; y < maxY; y++)
                {
                    double multiplier = 1.0;
                    if (smoothEdges)
                    {
                        int dx = Math.Abs(middleX - x);
                        int dy = Math.Abs(middleY - y);
                        if (dx >= smoothThresholdX)
                        {
                            multiplier *= 1 - (dx - smoothThresholdX) / (double)(edgeSizeX);
                        }
                        if (dy >= smoothThresholdY)
                        {
                            multiplier *= 1 - (dy - smoothThresholdY) / (double)(edgeSizeY);
                        }
                    }
                    if (noise > 0)
                        multiplier *= random.NextDouble(1 - noise, 1);

                    int x2 = (int)Math.Round((x) * widthRatio);
                    int y2 = (int)Math.Round((y) * heightRatio);
                    Altitude[x, y] = floorHeight + (heightmap[x2, y2] * heightScale + heightScale) * multiplier;
                }
        }
        */
        public void GenerateRainmap()
        {
            Rain = new double[Settings.MapWidth, Settings.MapHeight];

        }

        /*
        public void GenerateRainmap()
        {
            Rain = new double[Settings.MapWidth, Settings.MapHeight];
            Clouds = new double[Settings.MapWidth, Settings.MapHeight];
            CloudsTemp = new double[Settings.MapWidth, Settings.MapHeight];
            for (int i = 0; i < 20; i++)
                SimulateClimate();
        }

        public void SimulateClimate()
        {
            int mapWidth = Settings.MapWidth;
            int mapHeight = Settings.MapHeight;
            //int seaLevel = Settings.SeaLevel;
            //double evaporationAmount = 1.0;
            
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

                    tasks[i] = new Task(() => SimulateClouds(blockX, blockY, blockWidth, blockHeight));
                    tasks[i].Start();
                }
            Task.WaitAll(tasks);

            for (int x = 0; x < mapWidth; x++)
                for (int y = 0; y < mapHeight; y++)
                {
                    Clouds[x, y] = CloudsTemp[x, y];

                    double rain = Clouds[x, y] / 10;
                    Rain[x, y] += rain;
                    Clouds[x, y] -= rain;

                    CloudsTemp[x, y] = 0;
                }
        }

        private void SimulateClouds(int startX, int startY, int width, int height)
        {
            int seaLevel = Settings.SeaLevel;
            int endX = startX + width;
            int endY = startY + height;
            double evaporationAmount = 1.0;

            for (int x = startX; x < endX; x++)
                for (int y = startY; y < endY; y++)
                {
                    if (Altitude[x, y] <= seaLevel)
                        Clouds[x, y] += evaporationAmount;

                    if (Clouds[x, y] > 0)
                    {
                        int minX = Math.Max(x - 1, 0);
                        int maxX = Math.Min(x + 1, endX - 1);
                        int minY = Math.Max(y - 1, 0);
                        int maxY = Math.Min(y + 1, endY - 1);
                        int tiles = (1 + maxX - minX) * (1 + maxY - minY);

                        for (int x2 = minX; x2 <= maxX; x2++)
                            for (int y2 = minY; y2 <= maxY; y2++)
                                CloudsTemp[x2, y2] += Clouds[x, y] / tiles;
                    }
                }
        }
        */
        public double GetAltitude(int x, int y)
        {
            return Altitude[x, y];
        }

        public double GetAltitude(Point point)
        {
            return Altitude[point.X, point.Y];
        }

        public double GetRain(int x, int y)
        {
            return Rain[x, y];
        }

        public double GetTemperature(int x, int y)
        {
            double latitude = (double)y / Settings.MapHeight;
            return 40 * latitude - 0.01 * Altitude[x, y] * 10;
        }
    }
}

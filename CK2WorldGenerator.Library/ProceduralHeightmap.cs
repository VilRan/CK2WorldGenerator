using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK2WorldGenerator.Library
{
    public class ProceduralHeightmap
    {
        public double[,] Altitude;
        public double BaseHeight, AltitudeScale, Roughness, Noise;
        public int SizeX, SizeY;
        public bool WrapX, WrapY;
        public bool SmoothEdges;

        private double FloorHeight { get { return BaseHeight - AltitudeScale; } }

        public ProceduralHeightmap(int sizeX, int sizeY,
            double baseHeight = 0.0, double altitudeScale = 1.0, double roughness = 0.65, double noise = 0.0,
            bool wrapX = true, bool wrapY = true, bool smoothEdges = false)
        {
            SizeX = sizeX;
            SizeY = sizeY;
            BaseHeight = baseHeight;
            AltitudeScale = altitudeScale;
            Roughness = roughness;
            Noise = noise;
            WrapX = wrapX;
            WrapY = wrapY;
            SmoothEdges = smoothEdges;
        }

        public void Generate(Random rng)
        {
            Altitude = new double[SizeX, SizeY];

            double[,] heightmap = new DiamondSquare(rng, SizeX, SizeY, 1, Roughness, WrapX, WrapY).GenerateHeightmap();
            double heightmapWidth = heightmap.GetUpperBound(0);
            double heightmapDepth = heightmap.GetUpperBound(1);
            double widthRatio = heightmapWidth / SizeX;
            double heightRatio = heightmapDepth / SizeY;
            int middleX = SizeX / 2;
            int middleY = SizeY / 2;
            int edgeSizeX = SizeX / 4;
            int edgeSizeY = SizeY / 4;
            int smoothThresholdX = middleX - edgeSizeX;
            int smoothThresholdY = middleY - edgeSizeY;

            for (int x = 0; x < SizeX; x++)
                for (int y = 0; y < SizeY; y++)
                {
                    double multiplier = 1.0;
                    if (SmoothEdges)
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
                    if (Noise > 0)
                        multiplier *= rng.NextDouble(1 - Noise, 1);

                    int x2 = (int)Math.Round((x) * widthRatio);
                    int y2 = (int)Math.Round((y) * heightRatio);
                    Altitude[x, y] = FloorHeight + (heightmap[x2, y2] * AltitudeScale + AltitudeScale) * multiplier;
                }
        }

        public double GetAltitude(int x, int y)
        {
            return Altitude[x, y];
        }

        public double GetAltitude(Point point)
        {
            return Altitude[point.X, point.Y];
        }
    }
}

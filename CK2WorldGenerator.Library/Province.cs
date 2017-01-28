using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK2WorldGenerator.Library
{
    public class Province
    {
        public Color Color;
        public Point Position;
        public Culture Culture;
        public CultureGroup CultureGroup;
        public County County;

        public int DistanceSquared(Province other)
        {
            return DistanceSquared(other.Position);
        }

        public int DistanceSquared(Point point)
        {
            int dx = point.X - Position.X;
            int dy = point.Y - Position.Y;
            return dx * dx + dy * dy;
        }

        public int DistanceSquared(int x, int y)
        {
            int dx = x - Position.X;
            int dy = y - Position.Y;
            return dx * dx + dy * dy;
        }
    }
}

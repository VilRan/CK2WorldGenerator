using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK2WorldGenerator.Library
{
    public class DiamondSquare
    {
        public double[,] Heightmap;
        public Random RNG;
        public double Amplitude, Roughness;
        private int _SizeX, _SizeY;
        private bool _WrapX, _WrapY;

        public int SizeX
        {
            set
            {
                _SizeX = 2;
                while (_SizeX < value)
                    _SizeX *= 2;
                if (!_WrapX)
                    _SizeX++;
            }
            get
            {
                return _SizeX;
            }
        }
        public int SizeY
        {
            set
            {
                _SizeY = 2;
                while (_SizeY < value)
                    _SizeY *= 2;
                if (!_WrapY)
                    _SizeY++;
            }
            get
            {
                return _SizeY;
            }
        }

        public bool WrapX
        {
            set
            {
                _WrapX = value;
                SizeX = _SizeX;
            }
            get
            {
                return _WrapX;
            }
        }
        public bool WrapY
        {
            set
            {
                _WrapY = value;
                SizeY = _SizeY;
            }
            get
            {
                return _WrapY;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rng">The class needs a random number generator in order to randomize the heightmap.</param>
        /// <param name="desiredWidth">The actual Width will be the smallest power of 2 equal to or greater than this value (+1 if not wrapping)</param>
        /// <param name="desiredHeight">The actual Height will be the smallest power of 2 equal to or greater than this value (+1 if not wrapping)</param>
        /// <param name="amplitude">Determines the range of values the heightmap can have.</param>
        /// <param name="roughness">Lower values make the map smoother.</param>
        /// <param name="wrapX">Whether the heightmap should wrap horizontally.</param>
        /// <param name="wrapY">Whether the heightmap should wrap vertically.</param>
        public DiamondSquare(Random rng, int desiredWidth, int desiredHeight, double amplitude, double roughness, bool wrapX, bool wrapY)
        {
            RNG = rng;
            Amplitude = amplitude;
            Roughness = roughness;
            _WrapX = wrapX;
            _WrapY = wrapY;
            SizeX = desiredWidth;
            SizeY = desiredHeight;
        }

        public double[,] GenerateHeightmap()
        {
            Heightmap = new double[_SizeX, _SizeY];
            double range = Amplitude;
            int offset = ((_SizeX < _SizeY) ? _SizeX : _SizeY) - 1; // Smaller dimension decides initial offset.

            while (offset > 1)
            {
                Diamond(range, offset);
                Square(range, offset);
                range *= Roughness;
                offset /= 2;
            }

            return Heightmap;
        }

        private void Diamond(double range, int offset)
        {
            double height;
            int x, y = 0;
            while (y < (_SizeY - 1))
            {
                x = 0;
                while (x < (_SizeX - 1))
                {
                    // Calculate the average height of the diamonds's corners and add a random value.
                    height = Get(x, y);
                    height += Get(x + offset, y);
                    height += Get(x, y + offset);
                    height += Get(x + offset, y + offset);
                    height /= 4;
                    height += RNG.NextDouble(-range, range);

                    Set(x + (offset / 2), y + (offset / 2), height);

                    x += offset;
                }

                y += offset;
            }
        }

        private void Square(double range, int offset)
        {
            double height;
            int x, y = 0;
            while (y < _SizeY)
            {
                x = (y + (offset / 2)) % offset;

                while (x < _SizeX)
                {
                    // Calculate the average height of the square's corners and add a random value.
                    height = Get(x - (offset / 2), y);
                    height += Get(x + (offset / 2), y);
                    height += Get(x, y - (offset / 2));
                    height += Get(x, y + (offset / 2));
                    height /= 4;
                    height += RNG.NextDouble(-range, range);

                    Set(x, y, height);

                    x += offset;
                }

                y += offset / 2;
            }
        }

        private double Get(int x, int y)
        {
            if (_WrapX)
                x = MathExtra.Wrap(x, _SizeX);
            else
                x = MathExtra.Clamp(x, 0, _SizeX - 1);
            if (_WrapY)
                y = MathExtra.Wrap(y, _SizeY);
            else
                y = MathExtra.Clamp(y, 0, _SizeY - 1);
            return Heightmap[x, y];
        }

        private void Set(int x, int y, double value)
        {
            if (_WrapX)
                x = MathExtra.Wrap(x, _SizeX);
            else
                x = MathExtra.Clamp(x, 0, _SizeX - 1);
            if (_WrapY)
                y = MathExtra.Wrap(y, _SizeY);
            else
                y = MathExtra.Clamp(y, 0, _SizeY - 1);
            Heightmap[x, y] = value;
        }
    }
}

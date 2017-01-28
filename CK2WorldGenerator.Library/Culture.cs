using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK2WorldGenerator.Library
{
    public class CultureGroup
    {
        public List<Culture> Cultures = new List<Culture>();
        public Color BaseColor;
        public Province Origin;
    }

    public class Culture
    {
        public CultureGroup Group;
        public Color Color;
        public Province Origin;
    }
}

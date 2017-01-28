using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK2WorldGenerator.Library
{
    public class DeJureTitle
    {
        public Color Color;
    }

    public class Barony : DeJureTitle
    {
        public County County;
    }

    public class County : DeJureTitle
    {
        public Province Province;
        public Duchy Duchy;
        public List<Barony> Baronies = new List<Barony>();
    }

    public class Duchy : DeJureTitle
    {
        public Kingdom Kingdom;
        public List<County> Counties = new List<County>();
    }

    public class Kingdom : DeJureTitle
    {
        public Empire Empire;
        public List<Duchy> Duchies = new List<Duchy>();
    }

    public class Empire : DeJureTitle
    {
        public List<Kingdom> Kingdoms = new List<Kingdom>();
    }
}

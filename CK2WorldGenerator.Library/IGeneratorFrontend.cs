using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK2WorldGenerator.Library
{
    public interface IGeneratorFrontend
    {
        void AddLogEntry(string text);
    }
}

using CK2WorldGenerator.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CKWorldGeneratorConsole
{
    public class Program: IGeneratorFrontend
    {
        public static void Main(string[] args)
        {
            new Program().Run();
        }

        public void Run()
        {
            WorldGenerator generator = new WorldGenerator(this);
            generator.GenerateWorld();
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        public void AddLogEntry(string text)
        {
            Console.Write(text);
        }
    }
}

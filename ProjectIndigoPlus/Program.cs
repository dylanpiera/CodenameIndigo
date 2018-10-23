using System;
using System.Threading.Tasks;

namespace IndigoBot
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var b = new Bot())
            {
                b.RunAsync().Wait();
            }
        }
    }
}

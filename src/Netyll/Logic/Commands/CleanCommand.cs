using System;

namespace Netyll.Logic.Commands
{
    public class CleanCommand
    {
        public int Run()
        {
            Console.WriteLine($"{nameof(CleanCommand)}.{nameof(Run)}");
            return 0;
        }
    }
}

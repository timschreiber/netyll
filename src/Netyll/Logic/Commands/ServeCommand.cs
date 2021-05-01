using System;

namespace Netyll.Logic.Commands
{
    public class ServeCommand
    {
        public int Run()
        {
            Console.WriteLine($"{nameof(ServeCommand)}.{nameof(Run)}");
            return 0;
        }
    }
}

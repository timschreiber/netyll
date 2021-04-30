using System;
using System.Collections.Generic;
using System.Text;

namespace Netyll.Commands
{
    public class ServeCommand : BaseCommand
    {
        public override int Run()
        {
            Console.WriteLine($"{nameof(ServeCommand)}.{nameof(Run)}");
            return 0;
        }
    }
}

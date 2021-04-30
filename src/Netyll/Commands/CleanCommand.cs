using System;
using System.Collections.Generic;
using System.Text;

namespace Netyll.Commands
{
    public class CleanCommand : BaseCommand
    {
        public override int Run()
        {
            Console.WriteLine($"{nameof(CleanCommand)}.{nameof(Run)}");
            return 0;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Netyll.Commands
{
    public class BuildCommand : BaseCommand
    {
        public override int Run()
        {
            Console.WriteLine($"{nameof(BuildCommand)}.{nameof(Run)}");
            return 0;
        }
    }
}

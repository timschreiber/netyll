using System;
using System.Collections.Generic;
using System.Text;

namespace Netyll.Commands
{
    public class NewCommand : BaseCommand
    {
        public override int Run()
        {
            Console.WriteLine($"{nameof(NewCommand)}.{nameof(Run)}");
            return 0;
        }
    }
}

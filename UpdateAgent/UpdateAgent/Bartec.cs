using Syroot.Windows.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateAgent
{
    class Bartec : Platform
    {

    }

    class BartecPlatform : PlatformFactory
    {
        public Platform Create()
        {
            return new Bartec();
        }
    }
}

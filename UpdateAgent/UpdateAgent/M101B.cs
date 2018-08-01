using Syroot.Windows.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UpdateAgent
{
    class M101B : Platform
    {
        
    }

    class M101BPlatform : PlatformFactory
    {
        public Platform Create()
        {
            return new M101B();
        }
    }
}

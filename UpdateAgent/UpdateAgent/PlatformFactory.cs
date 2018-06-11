using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateAgent
{
    interface PlatformFactory
    {
        Platform Create();
    }
}

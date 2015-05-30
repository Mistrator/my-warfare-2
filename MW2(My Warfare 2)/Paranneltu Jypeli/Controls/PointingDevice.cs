using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jypeli.Controls
{
    interface PointingDevice
    {
        int ActiveChannels { get; }
        bool IsTriggeredOnChannel( int ch, Listener l );
    }
}

﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace LockNess.Communication.SocketFac
{
    public interface ISocketBuilder
    {
        Socket Create();
    }
}

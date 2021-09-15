using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Scanner.Scanister
{
    class LMS511:SickScanner
    {
        public LMS511(string ip, int port,ProtocolType protocol) :base(ip,port,protocol){

        }
    }
}
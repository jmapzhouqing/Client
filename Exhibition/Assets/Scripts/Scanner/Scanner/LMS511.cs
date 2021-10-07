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
        public LMS511(string name,string ip, int port) :base(name,ip,port){

        }
    }
}
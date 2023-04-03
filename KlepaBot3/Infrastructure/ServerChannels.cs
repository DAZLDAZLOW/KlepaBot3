using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlepaBot3.Infrastructure
{
    internal class ServerChannels
    {
        public ulong ServerId;
        public List<KlepaChannel> Channels = new List<KlepaChannel>();
    }
}

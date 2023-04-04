using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlepaBot3.Infrastructure
{
    public class KlepaChannel //TODO: Придумать нормальное название для этой сущности
    {
        public ulong TextChannelId { get; set; }
        public ulong VoiceChannelId { get; set; }
        public int Index = 0;
        public ulong ChannelOwnerId { get; set; }
    }
}

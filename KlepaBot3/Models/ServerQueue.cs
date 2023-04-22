using DSharpPlus.Lavalink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlepaBot3.Models
{
    public class ServerQueue
    {
        public ulong ServerId { get; set; }
        public Queue<LavalinkTrack> Queue { get; set; } = new Queue<LavalinkTrack>();
        public bool IsPlaying { get; set; } = false; 
    }
}

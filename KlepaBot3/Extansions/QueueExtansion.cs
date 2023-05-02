using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlepaBot3.Extansions
{
    public static class QueueExtansion
    {
        public static Queue<T> Shuffle<T>(this Queue<T> queue)
        {
            var rnd = new  Random(); 
            return new Queue<T>(queue.OrderBy(x => rnd.Next()).ToArray());
        }
    }
}

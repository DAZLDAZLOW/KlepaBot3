using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class Server
    {
        public ulong Id { get; set; }

        public string? Name { get; set; }   

        public ChannelsSetup ChannelsSetup { get; set; } = new ChannelsSetup();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Models
{
    public class ChannelsSetup
    {
        public int Id { get; set; }

        public ulong? PrivateChannelsCategoryId { get; set; }

        public ulong? PublicChannelsCategoryId { get; set; }

        public ulong? PublicMotherChannelId { get; set; }

        public ulong? PrivateMotherChannelId { get; set; }

        public string PublicChannelDefaultName { get; set; } = "Публичный канал ";

        public bool AddToPublicChannelCounter { get; set; } = true;

        public string PrivateChannelDefaultName { get; set; } = "Приватный канал ";

        public bool AddToPrivateChannelCounter { get; set; } = true;

    }
}

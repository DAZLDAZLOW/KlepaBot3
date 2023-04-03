using DataAccess;
using DataAccess.Models;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using KlepaBot3.Modules;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KlepaBot3
{
    internal class ChannelManager
    {
        private readonly KlepaDbContext context;
        private readonly PublicChannelManager publicChannelManager;
        private readonly PrivateChannelManager privateChannelManager;

        public ChannelManager(KlepaDbContext context)
        {
            this.context = context;
            publicChannelManager = new PublicChannelManager(context);
            privateChannelManager = new PrivateChannelManager(context);
        }

        public async Task VoiceStateUpdatedHandler(DiscordClient sender, DSharpPlus.EventArgs.VoiceStateUpdateEventArgs e)
        {
            if (e.Before != null && e.Before.Channel.Users.Count == 0)
            {
                await publicChannelManager.ClearEmptyChannel(e);
                await privateChannelManager.ClearEmptyChannel(e);
            }

            if (e.Channel == null)
                return;

            Server? server = await context.Servers.Include(x => x.ChannelsSetup).FirstOrDefaultAsync(x => x.Id == e.Guild.Id);
            if (server == null) return;

            if (server.ChannelsSetup.PublicMotherChannelId == e.Channel.Id) await publicChannelManager.CreateChannelAndMoveUser(e, server);
            else if (server.ChannelsSetup.PrivateMotherChannelId == e.Channel.Id) await privateChannelManager.CreateChannelAndMoveUser(e, server);
        }
    }
}

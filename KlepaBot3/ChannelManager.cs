using DataAccess;
using DataAccess.Models;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
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
        private readonly List<ServerChannels> servers = new List<ServerChannels>();

        public ChannelManager(KlepaDbContext context)
        {
            this.context = context;
        }

        public async Task VoiceStateUpdatedResolve(DiscordClient sender, DSharpPlus.EventArgs.VoiceStateUpdateEventArgs e)
        {
            if (e.Before != null && e.Before.Channel.Users.Count == 0)
                await ClearChannelIfEmpty(e);
            if (e.Channel == null)
                return;
            Server? server = await context.Servers.Include(x => x.ChannelsSetup).FirstOrDefaultAsync(x => x.Id == e.Guild.Id);
            if (server == null) return;
            if (server.ChannelsSetup.PublicMotherChannelId == e.Channel.Id) await CreatePublicChannelAndMoveUser(e, server);
            else if (server.ChannelsSetup.PrivateMotherChannelId == e.Channel.Id) await CreatePrivateChannelAndMoveUser(e, server);
        }

        private async Task CreatePrivateChannelAndMoveUser(VoiceStateUpdateEventArgs e, Server server)
        {
            var serverchennels = servers.FirstOrDefault(x => x.ServerId == server.Id);

            if (serverchennels == null)
            {
                serverchennels = new ServerChannels { ServerId = server.Id };
                servers.Add(serverchennels);
            }

            serverchennels.PrivateChannelsCounter++;

            var newChannelName = (server.ChannelsSetup.AddToPrivateChannelCounter) ? server.ChannelsSetup.PrivateChannelDefaultName + serverchennels.PrivateChannelsCounter : server.ChannelsSetup.PrivateChannelDefaultName;
            var parentCategory = (server.ChannelsSetup.PrivateChannelsCategoryId != null) ? e.Guild.GetChannel((ulong)server.ChannelsSetup.PrivateChannelsCategoryId) : null; 

            var createdTextChannel = await e.Guild.CreateChannelAsync(newChannelName, ChannelType.Text, overwrites: GetPermissonsForPrivateChannnel(e), parent:parentCategory);
            var createdVoiceChannel = await e.Guild.CreateChannelAsync(newChannelName, ChannelType.Voice, overwrites: GetPermissonsForPrivateChannnel(e), parent:parentCategory);
            if (createdVoiceChannel == null) return;
            serverchennels.PrivateChannels.Add(new PrivateChannel { VoiceChannelId = createdVoiceChannel.Id, TextChannelId = createdTextChannel.Id });
            await createdVoiceChannel.PlaceMemberAsync(e.Guild.Members[e.User.Id]);
        }

        private IEnumerable<DiscordOverwriteBuilder> GetPermissonsForPrivateChannnel(VoiceStateUpdateEventArgs e)
        {
            DiscordOverwriteBuilder[] dobs = new DiscordOverwriteBuilder[]{
                new DiscordOverwriteBuilder().For(e.Guild.EveryoneRole).Deny(Permissions.AccessChannels),
                new DiscordOverwriteBuilder().For(e.User as DiscordMember).Allow(Permissions.AccessChannels)
            };
            return dobs;
        }

        private async Task CreatePublicChannelAndMoveUser(VoiceStateUpdateEventArgs e, Server server)
        {
            var serverchennels = servers.FirstOrDefault(x => x.ServerId == server.Id);
            if (serverchennels == null)
            {
                serverchennels = new ServerChannels { ServerId = server.Id };
                servers.Add(serverchennels);
            }
            serverchennels.PublicChannelsCounter++;
            var newChannelName = (server.ChannelsSetup.AddToPublicChannelCounter) ? server.ChannelsSetup.PublicChannelDefaultName + serverchennels.PublicChannelsCounter : server.ChannelsSetup.PublicChannelDefaultName;
            var createdChannel = await e.Guild.CreateChannelAsync(newChannelName, ChannelType.Voice);
            if (createdChannel == null) return;
            serverchennels.PublicChannels.Add(createdChannel.Id);
            await createdChannel.PlaceMemberAsync(e.Guild.Members[e.User.Id]);
        }

        //Если пользователь вышел из СОЗДАННОГО канала и в нём больше никого нет, удаляет этот канал.
        private async Task ClearChannelIfEmpty(VoiceStateUpdateEventArgs e)
        {
            var serverchannels = servers.FirstOrDefault(x => x.ServerId == e.Guild.Id);
            if (serverchannels == null) return;
            if (serverchannels.PublicChannels.Any(x => x == e.Before.Channel.Id))
            {
                serverchannels.PublicChannels.Remove(e.Before.Channel.Id);
                if (serverchannels.PublicChannels.Count == 0)
                {
                    serverchannels.PublicChannelsCounter = 0;
                }
                await e.Before.Channel.DeleteAsync();
            }
            else
            {
                var privateChannels = serverchannels.PrivateChannels.FirstOrDefault(x => x.VoiceChannelId == e.Before.Channel.Id);
                if(privateChannels == null) return;
                await e.Guild.GetChannel(privateChannels.TextChannelId).DeleteAsync(); 
                await e.Before.Channel.DeleteAsync();
                serverchannels.PrivateChannels.Remove(privateChannels);
                if (serverchannels.PrivateChannels.Count == 0)
                {
                    serverchannels.PrivateChannelsCounter = 0;
                }

            }
        }
    }

    internal class ServerChannels
    {
        public ulong ServerId;
        public List<ulong> PublicChannels { get; set; } = new List<ulong>();
        public int PublicChannelsCounter = 0;
        public List<PrivateChannel> PrivateChannels { get; set; } = new List<PrivateChannel>();
        public int PrivateChannelsCounter = 0;
    }

    internal class PrivateChannel
    {
        public ulong TextChannelId { get; set; }
        public ulong VoiceChannelId { get; set; }
    }
}

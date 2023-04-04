using DataAccess;
using DataAccess.Models;
using DSharpPlus.EventArgs;
using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlepaBot3.Infrastructure;

namespace KlepaBot3.Modules
{
    public class PublicChannelManager
    {
        private readonly KlepaDbContext context;
        private readonly List<ServerChannels> servers = new List<ServerChannels>();

        public PublicChannelManager(KlepaDbContext context)
        {
            this.context = context;
        }

        public async Task CreateChannelAndMoveUser(VoiceStateUpdateEventArgs e, Server server)
        {
            var serverchennels = servers.FirstOrDefault(x => x.ServerId == server.Id);
            if (serverchennels == null)
            {
                serverchennels = new ServerChannels { ServerId = server.Id };
                servers.Add(serverchennels);
            }
            var newChannelIndex = (serverchennels.Channels.Count > 0)? serverchennels.Channels.Last().Index + 1 : 1; //Проверить работоспособность номеров
            var newChannelName = (server.ChannelsSetup.AddToPublicChannelCounter) ? server.ChannelsSetup.PublicChannelDefaultName + newChannelIndex : server.ChannelsSetup.PublicChannelDefaultName;
            var parentCategory = (server.ChannelsSetup.PublicChannelsCategoryId != null) ? e.Guild.GetChannel((ulong)server.ChannelsSetup.PublicChannelsCategoryId) : null;

            var createdTextChannel = await e.Guild.CreateChannelAsync(newChannelName, ChannelType.Text, parent: parentCategory);
            if (createdTextChannel == null)
            {
                throw new Exception($"Не удалось создать публичный текстовый канал на сервере '{server.Id}'.");
            }
            var createdVoiceChannel = await e.Guild.CreateChannelAsync(newChannelName, ChannelType.Voice, parent: parentCategory);
            if (createdVoiceChannel == null)
            {
                await createdTextChannel.DeleteAsync();
                throw new Exception($"Не удалось создать публичный голосовой канал на сервере '{server.Id}'.");
            }
            serverchennels.Channels.Add(new KlepaChannel {ChannelOwnerId = e.User.Id, Index = newChannelIndex, TextChannelId = createdTextChannel.Id, VoiceChannelId = createdVoiceChannel.Id });
            await createdVoiceChannel.PlaceMemberAsync(e.Guild.Members[e.User.Id]);
        }

        public async Task ClearEmptyChannel(VoiceStateUpdateEventArgs e)
        {
            var serverchannels = servers.FirstOrDefault(x => x.ServerId == e.Guild.Id);
            if (serverchannels == null) return;
            var channel = serverchannels.Channels.FirstOrDefault(x => x.VoiceChannelId == e.Before.Channel.Id);
            if (channel == null) return;
            await e.Guild.GetChannel(channel.TextChannelId).DeleteAsync();
            await e.Before.Channel.DeleteAsync();
            serverchannels.Channels.Remove(channel);
        }
    }
}

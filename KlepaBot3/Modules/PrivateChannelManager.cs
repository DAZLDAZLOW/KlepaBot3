using DataAccess.Models;
using DataAccess;
using DSharpPlus.EventArgs;
using DSharpPlus;
using KlepaBot3.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace KlepaBot3.Modules
{
    public class PrivateChannelManager
    {
        private readonly KlepaDbContext context;
        public static readonly List<ServerChannels> servers = new List<ServerChannels>();

        public PrivateChannelManager(KlepaDbContext context)
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
            var newChannelIndex = (serverchennels.Channels.Count > 0) ? serverchennels.Channels.Last().Index + 1 : 1; //Проверить работоспособность номеров
            var newChannelName = (server.ChannelsSetup.AddToPrivateChannelCounter) ? server.ChannelsSetup.PrivateChannelDefaultName + newChannelIndex : server.ChannelsSetup.PrivateChannelDefaultName;
            var parentCategory = (server.ChannelsSetup.PrivateChannelsCategoryId != null) ? e.Guild.GetChannel((ulong)server.ChannelsSetup.PrivateChannelsCategoryId) : null;
            var permissions = GetPermissonsForPrivateChannnel(e);

            var createdTextChannel = await e.Guild.CreateChannelAsync(newChannelName, ChannelType.Text,overwrites:permissions, parent: parentCategory);
            if (createdTextChannel == null)
            {
                throw new Exception($"Не удалось создать приватный текстовый канал на сервере '{server.Id}'.");
            }
            var createdVoiceChannel = await e.Guild.CreateChannelAsync(newChannelName, ChannelType.Voice,overwrites:permissions, parent: parentCategory);
            if (createdVoiceChannel == null)
            {
                await createdTextChannel.DeleteAsync();
                throw new Exception($"Не удалось создать приватный голосовой канал на сервере '{server.Id}'.");
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

        private IEnumerable<DiscordOverwriteBuilder> GetPermissonsForPrivateChannnel(VoiceStateUpdateEventArgs e)
        {
            DiscordOverwriteBuilder[] dobs = new DiscordOverwriteBuilder[]{
                new DiscordOverwriteBuilder().For(e.Guild.EveryoneRole).Deny(Permissions.AccessChannels),
                new DiscordOverwriteBuilder().For(e.User as DiscordMember).Allow(Permissions.AccessChannels)
            };
            return dobs;
        }
    }
}

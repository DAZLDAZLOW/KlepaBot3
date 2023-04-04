﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using KlepaBot3.Infrastructure;
using KlepaBot3.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace KlepaBot3.Commands
{
    public class PrivateChannelCommands : BaseCommandModule
    {
        [Command("add")]
        public async Task AddCommand(CommandContext ctx, DiscordMember discordMember)
        {
            var channels = GetChannel(ctx);
            if (channels == null) { return; }
            var voiceChannel = ctx.Guild.GetChannel(channels.VoiceChannelId);
            var textChannel = ctx.Guild.GetChannel(channels.TextChannelId);
            await voiceChannel.AddOverwriteAsync(discordMember, DSharpPlus.Permissions.AccessChannels);
            await textChannel.AddOverwriteAsync(discordMember, DSharpPlus.Permissions.AccessChannels);
            await ctx.RespondAsync($"{discordMember.Username} добавлен в приватный канал");
        }

        private KlepaChannel? GetChannel(CommandContext ctx)
        {
            if (ctx.Member == null) return null;
            if (!PrivateChannelManager.servers.Any(x => x.ServerId == ctx.Channel.GuildId)) return null;
            
            KlepaChannel? channel = PrivateChannelManager.servers.First(x => x.ServerId == ctx.Guild.Id).Channels.FirstOrDefault(x => x.TextChannelId == ctx.Channel.Id);
            if (channel?.ChannelOwnerId != ctx.Member!.Id) return null;
            //if (ctx.Member.Id == PrivateChannelManager.servers.First(x => x.ServerId == ctx.Guild.Id).Channels.Fi)
            return channel;
        }
    }
}

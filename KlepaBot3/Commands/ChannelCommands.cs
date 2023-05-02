using DSharpPlus.CommandsNext;
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
    public class ChannelCommands : BaseCommandModule
    {
        //Общие команды

        [Command("rename")]
        [RequireGuild]
        public async Task RenameCommand(CommandContext ctx,params string[] newNameArray)
        {
            var newName = String.Join(' ',newNameArray);
            var channels = GetChannel(ctx);
            if (channels == null) { return; }
            var voiceChannel = ctx.Guild.GetChannel(channels.VoiceChannelId);
            var textChannel = ctx.Guild.GetChannel(channels.TextChannelId);
            await voiceChannel.ModifyAsync((channel) =>
            {
                channel.Name = newName;
            });
            await textChannel.ModifyAsync((channel) =>
            {
                channel.Name = newName;
            });
            await ctx.Channel.SendMessageAsync($"Канал успешно перименован в '{newName}'");
        }


        //Команды для публичных каналов




        //Команды для приватных каналов

        //Добавить команду, которая выводит список всех у кто видит этот канал

        [Command("add")]
        [RequireGuild]
        public async Task AddCommand(CommandContext ctx, DiscordMember discordMember)
        {
            var channels = GetChannel(ctx);
            if (channels == null) { return; }
            if (channels.IsPrivate == false) { return; }
            var voiceChannel = ctx.Guild.GetChannel(channels.VoiceChannelId);
            var textChannel = ctx.Guild.GetChannel(channels.TextChannelId);
            await voiceChannel.AddOverwriteAsync(discordMember, DSharpPlus.Permissions.AccessChannels);
            await textChannel.AddOverwriteAsync(discordMember, DSharpPlus.Permissions.AccessChannels);
            await ctx.Channel.SendMessageAsync($"{discordMember.Username} добавлен в приватный канал");
        }

        private KlepaChannel? GetChannel(CommandContext ctx)
        {
            if (ctx.Member == null) return null;
            if (!PrivateChannelManager.servers.Any(x => x.ServerId == ctx.Channel.GuildId)) return null;
            
            KlepaChannel? channel = PrivateChannelManager.servers.First(x => x.ServerId == ctx.Guild.Id).Channels.FirstOrDefault(x => x.TextChannelId == ctx.Channel.Id);
            if (channel?.ChannelOwnerId != ctx.Member!.Id) return null;
            return channel;
        }
    }
}

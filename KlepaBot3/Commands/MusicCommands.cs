using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KlepaBot3.Managers;

namespace KlepaBot3.Commands
{
    public class MusicCommands : BaseCommandModule
    {
        public MusicManager Music { private get;  set; } 

        [Command]
        public async Task Join(CommandContext ctx)
        {
            DiscordChannel? channel = GetVoiceChannel(ctx);
            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("Я не могу подключиться");
                return;
            }
            var node = lava.ConnectedNodes.Values.First();

            if(channel == null)
            {
                await ctx.RespondAsync("Ты не в войсе, олух...");
                return;
            }
            await node.ConnectAsync(channel);
            await ctx.RespondAsync("Я тута");
        }

        [Command]
        public async Task Leave(CommandContext ctx)
        {
            DiscordChannel? channel = GetVoiceChannel(ctx);
            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                return;
            }

            var node = lava.ConnectedNodes.Values.First();

            if(channel ==null)
            {
                await ctx.RespondAsync("Ты не туда зашел");
                return;
            }

            var conn = node.GetGuildConnection(channel.Guild);

            if(conn == null)
            {
                await ctx.RespondAsync("Музыки не будет!");
                return;
            }
            await conn.DisconnectAsync();
            await ctx.RespondAsync("Я вышел");
        }

        [Command]
        public async Task Search(CommandContext ctx, [RemainingText] string search)
        {
            string response = await Music.PlayAsync(ctx, search, false);
            await ctx.RespondAsync(response);
        }

        //[Command]
        //public async Task Play(CommandContext ctx, [RemainingText] string search)
        //{
        //    if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
        //    {
        //        await ctx.RespondAsync("You are not in a voice channel.");
        //        return;
        //    }

        //    var lava = ctx.Client.GetLavalink();
        //    var node = lava.ConnectedNodes.Values.First();
        //    var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

        //    if (conn == null)
        //    {
        //        await ctx.RespondAsync("Lavalink is not connected.");
        //        return;
        //    }
        //    var loadResult = await node.Rest.GetTracksAsync(search);
        //    //If something went wrong on Lavalink's end                          
        //    if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed

        //        //or it just couldn't find anything.
        //        || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
        //    {
        //        await ctx.RespondAsync($"Track search failed for {search}.");
        //        return;
        //    }
        //    var track = loadResult.Tracks.First();
        //    await conn.PlayAsync(track);

        //    await ctx.RespondAsync($"Now playing {track.Title}!");
        //}



        [Command]
        public async Task Play(CommandContext ctx, Uri url)
        {
            string response = await Music.PlayAsync(ctx, url.OriginalString);
            await ctx.RespondAsync(response);
        }

        [Command]
        public async Task Pause(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }

            await conn.PauseAsync();
        }

        [Command]
        public async Task Resume(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }

            await conn.ResumeAsync();
        }

        private DiscordChannel? GetVoiceChannel(CommandContext ctx)
        {
            if(ctx.Member == null) return null;
            if(ctx.Member.VoiceState == null) return null;
            var channel = ctx.Member.VoiceState.Channel;
            if(channel == null) return null;
            if (channel.Type != ChannelType.Voice) return null;
            return channel;
        }
    }
}

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
        public async Task Stop(CommandContext ctx)
        {
            string response = await Music.StopAsync(ctx);
            if(response != string.Empty)
            {
                await ctx.RespondAsync(response);
            }
        }

        [Command]
        public async Task Search(CommandContext ctx, [RemainingText] string search)
        {
            string response = await Music.PlayAsync(ctx, search, false);
            await ctx.RespondAsync(response);
        }

        [Command]
        public async Task Play(CommandContext ctx, Uri url, int startFrom = 1)
        {
            string response = await Music.PlayAsync(ctx, url.OriginalString,startFrom:startFrom);
            await ctx.RespondAsync(response);
        }

        [Command]
        public async Task Pause(CommandContext ctx)
        {
            string response = await Music.PauseAsync(ctx);
            await ctx.RespondAsync(response);
        }

        [Command]
        public async Task Resume(CommandContext ctx)
        {
            string response = await Music.ResumeAsync(ctx);
            await ctx.RespondAsync(response);
        }

        [Command]
        public async Task Skip(CommandContext ctx)
        {
            string response = await Music.SkipAsync(ctx);
            await ctx.RespondAsync(response);
        }
    }
}

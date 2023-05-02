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
        [RequireGuild]
        [Aliases("h")]
        public async Task Helpme(CommandContext ctx)//Временная команда, которую в будущем можно заменить на нормальную помощь
        {
            var eb = new DiscordEmbedBuilder()
            {
                Title = "Команды управления музыкой"
            };
            var sb = new StringBuilder();
            sb.AppendLine("!play/p [url] - Включить трек с ютуба");
            sb.AppendLine("!pl [url] [Число(с какого трека в плейлисте начать)] [-s(Вперемешку)] - Включить плейлист с ютуба");
            sb.AppendLine("!search/src [запрос] - Найти трек по запросу с ютуба");
            sb.AppendLine("!now - Показывает какой трек сейчас играет");
            sb.AppendLine("!check [Число] - Показывает [Число] треков из очереди");
            sb.AppendLine("!stop - Останавливает воспроизведение треков и очищает очередь");
            sb.AppendLine("!pause - Ставит на паузу воспроизведение трека");
            sb.AppendLine("!resume - Продолжает воспроизведение трека");
            eb.WithDescription(sb.ToString());
            await ctx.Channel.SendMessageAsync(eb);
        }


        [Command]
        [RequireGuild]
        [Aliases("ck")]
        public async Task Check(CommandContext ctx, int maxTracks = 10)
        {
            var queue = Music.GetQueueTracks(ctx, maxTracks + 1).ToArray();
            if(queue == Enumerable.Empty<LavalinkTrack>())
            {
                await ctx.Channel.SendMessageAsync("Сейчас ничего не воспроизводится");
            }
            else
            {
                StringBuilder sb = new StringBuilder("");
                sb.AppendLine($"**--> {queue[0].Title}** (<{queue[0].Uri}>)");
                for (int i = 1; i < queue.Length; i++)
                {
                    sb.AppendLine($"{i}: {queue[i].Title} (<{queue[i].Uri}>)");
                }
                var eb = new DiscordEmbedBuilder()
                {
                    Title = "Треки в очереди",
                    Description = sb.ToString()
                };
                await ctx.Channel.SendMessageAsync(eb);
            }
        }


        [Command]
        [RequireGuild]
        public async Task Now(CommandContext ctx)
        {
            if (ctx.Member!.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.Channel.SendMessageAsync("Для применения этой команды нужно находиться в голосовом канале");
            }
            var track = Music.GetCurrnetTrack(ctx);
            if (track != null)
                await ctx.Channel.SendMessageAsync($"Сейчас играет:{track.Title}\n{track.Uri}");
            else
                await ctx.Channel.SendMessageAsync($"Сейчас ничего не играет");
        }

        [Command("pl")]
        [RequireGuild]
        public async Task Playlist(CommandContext ctx, Uri uri, int startFrom = 1, string argument = "")
        {
            string response;
            if (argument == "-s") 
            {
                response = await Music.PlayAsync(ctx, uri.OriginalString, startFrom: startFrom, shufflePlaylist: true);
            }
            else
            {
                response = await Music.PlayAsync(ctx, uri.OriginalString, startFrom: startFrom);
            }
            await ctx.Channel.SendMessageAsync(response);
        }

        [Command("pl")]
        [RequireGuild]
        public async Task Playlist(CommandContext ctx, string Url, string argument = "")
        {
            string response;
            if (argument == "-s")
            {
                response = await Music.PlayAsync(ctx, Url, shufflePlaylist: true);
            }
            else
            {
                response = await Music.PlayAsync(ctx, Url);
            }
            await ctx.Channel.SendMessageAsync(response);
        }

        [Command]
        [RequireGuild]
        [Aliases("src")]
        public async Task Search(CommandContext ctx, [RemainingText] string search)
        {
            string response = await Music.PlayAsync(ctx, search, false);
            await ctx.RespondAsync(response);
        }


        [Command]
        [RequireGuild]
        public async Task Stop(CommandContext ctx)
        {
            string response = await Music.StopAsync(ctx);
            if(response != string.Empty)
            {
                await ctx.Channel.SendMessageAsync(response);
            }
        }

        [Command]
        [RequireGuild]
        [Aliases("p")]
        public async Task Play(CommandContext ctx, Uri url)
        {
            string response = await Music.PlayAsync(ctx, url.OriginalString);
            await ctx.Channel.SendMessageAsync(response);
        }

        [Command]
        [RequireGuild]
        public async Task Pause(CommandContext ctx)
        {
            string response = await Music.PauseAsync(ctx);
            await ctx.Channel.SendMessageAsync(response);
        }

        [Command]
        [RequireGuild]
        public async Task Resume(CommandContext ctx)
        {
            string response = await Music.ResumeAsync(ctx);
            await ctx.Channel.SendMessageAsync(response);
        }

        [Command]
        [RequireGuild]
        public async Task Skip(CommandContext ctx)
        {
            string response = await Music.SkipAsync(ctx);
            await ctx.Channel.SendMessageAsync(response);
        }

        [Command]
        [RequireGuild]
        public async Task Gachi(CommandContext ctx, int startFrom = 1)
        {
            var response = await Music.PlayAsync(ctx, "https://www.youtube.com/playlist?list=PL4Xu4_vhHcV5DcNbDqRMiF7aIBXcr_wGV",startFrom:startFrom, shufflePlaylist: true);
            await ctx.Channel.SendMessageAsync(response);
        }
    }
}

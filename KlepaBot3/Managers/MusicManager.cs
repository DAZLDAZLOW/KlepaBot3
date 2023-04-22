using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using KlepaBot3.Models;

namespace KlepaBot3.Managers
{
    public class MusicManager
    {
      //  private LavalinkExtension? lava;
        private List<ServerQueue> servers = new List<ServerQueue>();

        public async Task<string> PlayAsync(CommandContext ctx, string searchQuery, bool IsUrlSearch = true)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            if (ctx.Member!.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                return "Для воспроизведения музыки нужно находиться в голосовом канале";
            }
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null )
            {
                var result = await JoinAsync(ctx);
                if(result != string.Empty)
                {
                    return result;
                }
                conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            }else if(conn.Channel != ctx.Member!.VoiceState.Channel)
            {
                await conn.DisconnectAsync();
                var result = await JoinAsync(ctx);
                if (result != string.Empty)
                {
                    return result;
                }
                conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            }

            conn.PlaybackFinished += PlaybackFinishedHandler;

            var searchType = (IsUrlSearch) ? LavalinkSearchType.Plain : LavalinkSearchType.Youtube;

            var loadResult = await node.Rest.GetTracksAsync(searchQuery, searchType);
            //If something went wrong on Lavalink's end                          
            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed

                //or it just couldn't find anything.
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                return $"Track search failed for {searchQuery}.";
            }
            var track = loadResult.Tracks.First();

            var server = servers.FirstOrDefault(x => x.ServerId == ctx.Guild.Id);
            if (server == null)
            {
                server = new ServerQueue() { ServerId = ctx.Guild.Id };
                servers.Add(server);
            }

            if (server.IsPlaying)
            {
                server.Queue.Enqueue(track);
                return $"Трек добавлен в очередь.\nНомер в очереди:{server.Queue.Count + 1}";
            }
            else
            {
                if(server.Queue.Count == 0)
                {
                    server.IsPlaying = true; //Проверить что трек запустился, потом только менять на тру
                    return await PlayTrackAsync(track, conn);
                }
                else
                {
                    server.Queue.Enqueue(track);
                    return $"Трек добавлен в очередь.\nНомер в очереди:{server.Queue.Count + 1}";
                }
            }
        }

        private async Task PlaybackFinishedHandler(LavalinkGuildConnection sender, DSharpPlus.Lavalink.EventArgs.TrackFinishEventArgs e)
        {
            var server = servers.First(x => x.ServerId == e.Player.Guild.Id);
            if (server.Queue.Count == 0 || e.Reason.MayStartNext() == false)
            {
                server.IsPlaying = false;
                return;
            }
            await PlayTrackAsync(server.Queue.Dequeue(), sender);
        }

        public async Task Play(CommandContext ctx, Uri uri)
        {

        }

        private async Task<string> PlayTrackAsync(LavalinkTrack track, LavalinkGuildConnection conn)
        {
            

            if (conn == null)
            {
                return "Lavalink is not connected.";
                
            }
            await conn.PlayAsync(track);
            return "Трек воспроизводится";
        }

        private async Task<string> JoinAsync(CommandContext ctx)
        {
            DiscordChannel? channel = GetVoiceChannel(ctx);
            if (channel == null)
            {
                return "Ты не в войсе, олух...";
            }
            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                return "Я не могу подключиться";
            }
            var node = lava.ConnectedNodes.Values.First();
            await node.ConnectAsync(channel);
            return string.Empty;
        }

        private DiscordChannel? GetVoiceChannel(CommandContext ctx)
        {
            if (ctx.Member == null) return null;
            if (ctx.Member.VoiceState == null) return null;
            var channel = ctx.Member.VoiceState.Channel;
            if (channel == null) return null;
            if (channel.Type != ChannelType.Voice) return null;
            return channel;
        }

    }
}

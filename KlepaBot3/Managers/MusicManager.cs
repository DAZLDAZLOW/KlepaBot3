﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using KlepaBot3.Extansions;
using KlepaBot3.Models;

namespace KlepaBot3.Managers
{
    public class MusicManager
    {
        private List<ServerQueue> servers = new List<ServerQueue>();

        public async Task<string> PlayAsync(CommandContext ctx, string searchQuery, bool isUrlSearch = true, int startFrom = 1, bool shufflePlaylist = false)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            if (ctx.Member!.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                return "Для воспроизведения музыки нужно находиться в голосовом канале";
            }
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                var result = await JoinAsync(ctx);
                if (result != string.Empty)
                {
                    return result;
                }
                conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            }
            else if (conn.Channel != ctx.Member!.VoiceState.Channel)
            {
                await conn.DisconnectAsync();
                var result = await JoinAsync(ctx);
                if (result != string.Empty)
                {
                    return result;
                }
                conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            }

            var searchType = (isUrlSearch) ? LavalinkSearchType.Plain : LavalinkSearchType.Youtube;

            var loadResult = await node.Rest.GetTracksAsync(searchQuery, searchType);
            //If something went wrong on Lavalink's end                          
            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed

                //or it just couldn't find anything.
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                return $"Track search failed for {searchQuery}.";
            }

            var server = servers.FirstOrDefault(x => x.ServerId == ctx.Guild.Id);
            if (server == null)
            {
                server = new ServerQueue() { ServerId = ctx.Guild.Id };
                servers.Add(server);
            }

            LavalinkTrack track;
            
            if (loadResult.LoadResultType == LavalinkLoadResultType.PlaylistLoaded)
            {
                if(startFrom < 1 || startFrom > loadResult.Tracks.Count())
                {
                    return "Стартовая точка больше чем треков в плейлисте";
                }
                //int i = startFrom;
                foreach (var (tr, i) in loadResult.Tracks.Select((value, ind) => (value, ind)))
                {
                    if(i>=startFrom-1)server.Queue.Enqueue(tr);
                }
                await ctx.RespondAsync($"Загруженно {server.Queue.Count} треков");
                if (shufflePlaylist)
                {
                    server.Queue = server.Queue.Shuffle();
                }
                track = server.Queue.Dequeue();
            }
            else {
                track = loadResult.Tracks.First();
            }
            if (server.IsHandled == false)
            {
                conn.PlaybackFinished += PlaybackFinishedHandler; //Скорее всего из за этого сломана очередь
                server.IsHandled = true;
            }

            if (server.IsPlaying)
            {
                server.Queue.Enqueue(track);
                return $"Трек добавлен в очередь.\nНомер в очереди: {server.Queue.Count + 1}";
            }
            else
            {
                if (server.Queue.Count == 0 || loadResult.LoadResultType == LavalinkLoadResultType.PlaylistLoaded)
                {
                    server.IsPlaying = true; //Проверить что трек запустился, потом только менять на тру
                    return await PlayTrackAsync(track, conn);
                }
                else
                {
                    server.Queue.Enqueue(track);
                    return $"Трек добавлен в очередь.\nНомер в очереди: {server.Queue.Count + 1}";
                }
            }
        }

        public LavalinkTrack? GetCurrnetTrack(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            var track = conn.CurrentState.CurrentTrack;
            if (track == null)
            {
                return null;
            }
            return track;
        }

        public IEnumerable<LavalinkTrack> GetQueueTracks(CommandContext ctx, int MaxTracks = 10)
        {
            if(MaxTracks <= 0)
                return Enumerable.Empty<LavalinkTrack>();

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            var track = conn.CurrentState.CurrentTrack;

            if (track == null)
                return Enumerable.Empty<LavalinkTrack>();

            var list = new List<LavalinkTrack>
            {
                track
            };
            var server = servers.FirstOrDefault(x => x.ServerId == ctx.Guild.Id);

            if (server == null)
                return Enumerable.Empty<LavalinkTrack>();

            if (server.Queue.Count > 0 || MaxTracks > 1)
            {
                var takeCount = (server.Queue.Count >= (MaxTracks - 1)) ? MaxTracks - 1 : server.Queue.Count;
                list.AddRange(server.Queue.Take(takeCount));
            }

            return list;
        }

        public async Task<string> PauseAsync(CommandContext ctx)
        {
            if (ctx.Member!.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                return "Для воспроизведения музыки нужно находиться в голосовом канале";
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                return "Lavalink is not connected.(Напиши дазлу)";
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                return "Нет трека - нет паузы";
            }

            await conn.PauseAsync();
            return "Трек приостановлен";
        }

        public async Task<string> ResumeAsync(CommandContext ctx)
        {
            if (ctx.Member!.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                return "Для воспроизведения музыки нужно находиться в голосовом канале";
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                return "Lavalink is not connected.(Напиши дазлу)";
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                return "Нет трека - нет продолжения";
            }

            await conn.ResumeAsync();
            return "Трек воспроизводится";
        }

        public async Task<string> StopAsync(CommandContext ctx)
        {
            DiscordChannel? channel = GetVoiceChannel(ctx);
            if (channel == null)
            {
                return "Нужно быть в канале с ботом, чтобы остановить его";
            }

            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                return "Нечего остонавливать";
            }

            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(channel.Guild);

            if (conn == null)
            {
                return "Нечего остонавливать";
            }
            await conn.StopAsync();
            var server = servers.FirstOrDefault(x => x.ServerId == ctx.Guild.Id);
            if (server != null)
            {
                server.Queue.Clear();
                server.IsPlaying = false;
            }
            return string.Empty;
        }
        
        public async Task<string> SkipAsync(CommandContext ctx)
        {
            if (ctx.Member!.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                return "Для воспроизведения музыки нужно находиться в голосовом канале";
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                return "Lavalink is not connected.(Напиши дазлу)";
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                return "Нет трека - нет скипа";
            }
            var server = servers.FirstOrDefault(x => x.ServerId == ctx.Guild.Id);
            if (server == null)
            {
                return "Нет трека - нет скипа";
            }
            if (server.Queue.Count == 0)
            {
                await conn.StopAsync();
            }
            else
            {
                await PlayTrackAsync(server.Queue.Dequeue(), conn);
            }
                return "Трек пропущен";
        }

        private async Task PlaybackFinishedHandler(LavalinkGuildConnection sender, DSharpPlus.Lavalink.EventArgs.TrackFinishEventArgs e)
        {
            if (e.Reason == DSharpPlus.Lavalink.EventArgs.TrackEndReason.Replaced || e.Reason == DSharpPlus.Lavalink.EventArgs.TrackEndReason.Stopped) return;
            var server = servers.First(x => x.ServerId == e.Player.Guild.Id);
            if (server.Queue.Count == 0 || e.Reason.MayStartNext() == false)
            {
                server.IsPlaying = false;
                await sender.DisconnectAsync();
                return;
            }
            else
            {
                await PlayTrackAsync(server.Queue.Dequeue(), sender);
            }
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

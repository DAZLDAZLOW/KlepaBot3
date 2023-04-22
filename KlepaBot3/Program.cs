using DSharpPlus;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DSharpPlus.EventArgs;
using DataAccess;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.EntityFrameworkCore;
using DataAccess.Models;
using DSharpPlus.CommandsNext;
using System.Reflection;
using System.Net;
using KlepaBot3.Managers;

namespace KlepaBot3
{
    public class Program
    {
        private static KlepaDbContext DataContext;
        private static ChannelManager ChannelManager;
        private static IConfiguration Config;
        private static DiscordClient Client;
        private static LavalinkNodeConnection LavalinkNode;

        static async Task Main()
        {
            
            //Вызов метода преднастройки бота
            await Setup();
            //var channelsetup = new ChannelsSetup()
            //{
            //    PublicMotherChannelId = 1084877469942816768,
            //    PublicChannelDefaultName = "Public Test Channel ",
            //    PrivateChannelDefaultName = "Private Test Chennel",
            //    PrivateMotherChannelId = 1085609310677110924,
            //    PrivateChannelsCategoryId = 1085929597335515227
            //};
            //var server = new Server
            //{
            //    ChannelsSetup = channelsetup,
            //    Id = 327533115151089665,
            //    Name = "RELIFE"
            //};
            ////var a = DataContext.Servers.Include(x => x.ChannelsSetup).First();
            ////a.ChannelsSetup.PrivateMotherChannelId = 1085609310677110924;
            //DataContext.Servers.Add(server);
            //DataContext.SaveChanges();


            //Секция задания обработчиков событий
            Client.VoiceStateUpdated += ChannelManager.VoiceStateUpdatedHandler;


            //-----------------------------------

            await Client.ConnectAsync();

            //Lavalink

            var lavalinkConfig = new LavalinkConfiguration
            {
                Password = "assport"
            };

            var lavalink = Client.UseLavalink();

            LavalinkNode = await lavalink.ConnectAsync(lavalinkConfig);

            await Task.Delay(-1);
        }

        static async Task Setup()
        {
            //Конфиг
            Config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<Program>()
                .Build();

            //Создание дб контекста
            var optsBuilder = new DbContextOptionsBuilder<KlepaDbContext>();
            optsBuilder.UseSqlite(Config.GetConnectionString("DataConnection"));
            DataContext = new KlepaDbContext(optsBuilder.Options);

            //DSharp дискорд клиент
            Client = new DiscordClient(new DiscordConfiguration()
            {
                Token = Config.GetValue<string>("BotToken"),
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug
            });

            var services = new ServiceCollection() // Добавление в DI контейнер для команд
                .AddDbContext<KlepaDbContext>(x =>
                {
                    x.UseSqlite(Config.GetConnectionString("DataConnection"));
                }).AddSingleton<MusicManager>()
                .BuildServiceProvider();

            //Команды
            var commands = Client.UseCommandsNext(new CommandsNextConfiguration()
            {
                Services = services,
                StringPrefixes = new[] { "!!" }
            });

            commands.RegisterCommands(Assembly.GetExecutingAssembly());

            

            //Создание ChannelManager
            ChannelManager = new ChannelManager(DataContext);

        }
    }
}
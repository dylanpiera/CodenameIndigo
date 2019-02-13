using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using ProjectIndigoPlus.Entities;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectIndigoPlus
{
    public class Bot : IDisposable
    {
        private DiscordClient _client;
        private InteractivityModule _interactivity;
        private CommandsNextModule _cnext;
        internal static Config _config;
        private StartTimes _starttimes;
        private CancellationTokenSource _cts;
        internal static DebugLogger DebugLogger { get; private set; }

        public Bot()
        {
            if (Config.UseFile)
            {
                if (!File.Exists("config.json"))
                {
                    new Config().SaveToFile("config.json");
                    #region !! Report to user that config has not been set yet !! (aesthetics)
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.Black;
                    WriteCenter("▒▒▒▒▒▒▒▒▒▄▄▄▄▒▒▒▒▒▒▒", 2);
                    WriteCenter("▒▒▒▒▒▒▄▀▀▓▓▓▀█▒▒▒▒▒▒");
                    WriteCenter("▒▒▒▒▄▀▓▓▄██████▄▒▒▒▒");
                    WriteCenter("▒▒▒▄█▄█▀░░▄░▄░█▀▒▒▒▒");
                    WriteCenter("▒▒▄▀░██▄░░▀░▀░▀▄▒▒▒▒");
                    WriteCenter("▒▒▀▄░░▀░▄█▄▄░░▄█▄▒▒▒");
                    WriteCenter("▒▒▒▒▀█▄▄░░▀▀▀█▀▒▒▒▒▒");
                    WriteCenter("▒▒▒▄▀▓▓▓▀██▀▀█▄▀▀▄▒▒");
                    WriteCenter("▒▒█▓▓▄▀▀▀▄█▄▓▓▀█░█▒▒");
                    WriteCenter("▒▒▀▄█░░░░░█▀▀▄▄▀█▒▒▒");
                    WriteCenter("▒▒▒▄▀▀▄▄▄██▄▄█▀▓▓█▒▒");
                    WriteCenter("▒▒█▀▓█████████▓▓▓█▒▒");
                    WriteCenter("▒▒█▓▓██▀▀▀▒▒▒▀▄▄█▀▒▒");
                    WriteCenter("▒▒▒▀▀▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒");
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    WriteCenter("WARNING", 3);
                    Console.ResetColor();
                    WriteCenter("Thank you Mario!", 1);
                    WriteCenter("But our config.json is in another castle!");
                    WriteCenter("(Please fill in the config.json that was generated.)", 2);
                    WriteCenter("Press any key to exit..", 1);
                    Console.SetCursorPosition(0, 0);
                    Console.ReadKey();
                    #endregion
                    Environment.Exit(0);
                }

                _config = Config.LoadFromFile("config.json");
            }
            else
            {
                _config = Config.LoadFromCS();
            }

            #region !! Welcome Message !! (aesthetics)
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            WriteCenter(@"  ____              _   _               _____           _ _               ____        _   ");
            WriteCenter(@" |  _ \            | | (_)             |_   _|         | (_)             |  _ \      | |  ");
            WriteCenter(@" | |_) | ___   ___ | |_ _ _ __   __ _    | |  _ __   __| |_  __ _  ___   | |_) | ___ | |_ ");
            WriteCenter(@" |  _ < / _ \ / _ \| __| | '_ \ / _` |   | | | '_ \ / _` | |/ _` |/ _ \  |  _ < / _ \| __|");
            WriteCenter(@" | |_) | (_) | (_) | |_| | | | | (_| |  _| |_| | | | (_| | | (_| | (_) | | |_) | (_) | |_ ");
            WriteCenter(@" |____/ \___/ \___/ \__|_|_| |_|\__, | |_____|_| |_|\__,_|_|\__, |\___/  |____/ \___/ \__|");
            WriteCenter(@"                                 __/ |                       __/ |                        ");
            WriteCenter(@"                                |___/                       |___/                         ");
            WriteCenter();
            Console.ResetColor();
            #endregion

            _client = new DiscordClient(new DiscordConfiguration()
            {
                AutoReconnect = true,
                EnableCompression = true,
                LogLevel = LogLevel.Debug,
                Token = _config.Token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true
            });

            _interactivity = _client.UseInteractivity(new InteractivityConfiguration()
            {
                PaginationBehaviour = TimeoutBehaviour.Delete,
                PaginationTimeout = TimeSpan.FromSeconds(30),
                Timeout = TimeSpan.FromMinutes(5)
            });

            _starttimes = new StartTimes()
            {
                BotStart = DateTime.Now,
                SocketStart = DateTime.MinValue
            };

            _cts = new CancellationTokenSource();

            DependencyCollection dep = null;
            using (DependencyCollectionBuilder d = new DependencyCollectionBuilder())
            {
                d.AddInstance(new Dependencies()
                {
                    Interactivity = _interactivity,
                    StartTimes = _starttimes,
                    Cts = _cts
                });
                dep = d.Build();
            }

            _cnext = _client.UseCommandsNext(new CommandsNextConfiguration()
            {
                CaseSensitive = false,
                EnableDefaultHelp = true,
                EnableMentionPrefix = true,
                StringPrefix = _config.Prefix,
                IgnoreExtraArguments = true,
                Dependencies = dep
            });

            DebugLogger = _client.DebugLogger;

            RegisterCommands(_cnext);

            _client.Ready += OnReadyAsync;
        }

        private void RegisterCommands(CommandsNextModule cnext)
        {
            _cnext.RegisterCommands<Modules.Commands.Owner>();
            _cnext.RegisterCommands<Modules.Commands.Interactivity>();
            _cnext.RegisterCommands<Modules.Commands.Register>();
            _cnext.RegisterCommands<Modules.Commands.Deregister>();
            _cnext.RegisterCommands<Modules.Commands.List>();
            _cnext.RegisterCommands<Modules.Commands.EditRegistration>();
            _cnext.RegisterCommands<Modules.Commands.Battle>();

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            WriteCenter("Loaded commands:");
            Console.ResetColor();
            WriteCenter("-----");
            foreach (System.Collections.Generic.KeyValuePair<string, Command> command in _cnext.RegisteredCommands)
            {
                WriteCenter(_config.Prefix + command.Key);
            }
            WriteCenter("-----");
            WriteCenter();

        }

        public async Task RunAsync()
        {
            await _client.ConnectAsync();
            await WaitForCancellationAsync();
        }

        private async Task WaitForCancellationAsync()
        {
            while (!_cts.IsCancellationRequested)
            {
                await Task.Delay(500);
            }
        }

        private async Task OnReadyAsync(ReadyEventArgs e)
        {
            await Task.Yield();
            _starttimes.SocketStart = DateTime.Now;
        }

        public void Dispose()
        {
            _client.Dispose();
            _interactivity = null;
            _cnext = null;
            _config = null;
        }

        internal void WriteCenter(string value = "", int skipline = 0)
        {
            for (int i = 0; i < skipline; i++)
            {
                Console.WriteLine();
            }
            try
            {
                Console.SetCursorPosition((Console.WindowWidth - value.Length) / 2, Console.CursorTop);
            }
            catch
            {
            }
            Console.WriteLine(value);
        }
    }
}
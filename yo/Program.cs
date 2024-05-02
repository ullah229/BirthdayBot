using System;
using System.Timers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using Timer = System.Threading.Timer;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using BdayBot.Modules;
using System.Runtime.Remoting.Channels;
using Discord.Net;

namespace BdayBot
{
    public class Program
    {
        static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        public DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        private BirthdayCommands bdaycommands = new BirthdayCommands();
        private Timer timer;

        public async Task RunBotAsync()
        {
            var config = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.All
            };
            _client = new DiscordSocketClient(config);
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            string token = "xxxx_TOKEN_xxx";

            _client.Log += _client_Log;
            _client.Ready += ChannelReady;
            // _client.Ready += HappyBirthday;
            await RegisterCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, token);

            await _client.StartAsync();
           // _client.SlashCommandExecuted += SlashCommandHandler;


            await Task.Delay(-1);

        }

        private Task _client_Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }
/*
        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
        }
*/
        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);
            if (message.Author.IsBot) return;

            int argPos = 0;
            if (message.HasStringPrefix("/", ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
                if (result.Error.Equals(CommandError.UnmetPrecondition)) await message.Channel.SendMessageAsync(result.ErrorReason);
            }
        }

        public async Task InstallCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                                            services: null);
        }

        public async Task ChannelReady()
        {
             // timer = new Timer(async _ => await HappyBirthday(), null, TimeSpan.Zero, TimeSpan.FromDays(1));
            timer = new Timer(async _ => await HappyBirthdayReminder(), null, TimeSpan.Zero, TimeSpan.FromDays(1));
        }
       
        public async Task HappyBirthdayReminder()
        {
            string username;
            string json = File.ReadAllText("birthdays.json");
            Dictionary<ulong, DateTime> _birthdays = System.Text.Json.JsonSerializer.Deserialize<Dictionary<ulong, DateTime>>(json);
            var remindchnl = _client.GetChannel(1176500020023918622) as IMessageChannel;
            var wishchnl = _client.GetChannel(1176501152288870410) as IMessageChannel;
            var birthdayyy = _birthdays.Where(kv => kv.Value.Month == DateTime.Now.Month).Where(kv => kv.Value.Day == DateTime.Now.Day);
            if (birthdayyy.Any())
            {
                var mylist = birthdayyy.ToList();
                foreach (var bday in mylist)
                {
                    username = $"<@{bday.Key}>";
                    await remindchnl.SendMessageAsync("Guys today is "+ username + "'s birthday. Please wish them a happy birthday 🎂 🥳 🎉  in birthday wish channel thanks");
                    await wishchnl.SendMessageAsync("Happy Birthday : " + username);
                }
                await remindchnl.SendMessageAsync("@everyone");
            }
        }
    }
}
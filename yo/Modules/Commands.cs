using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;

namespace BdayBot.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public async Task Ping()
        {
            await ReplyAsync("Pong");
        }
        [Command("hello")]
        public async Task HelloCommand()
        {
            await ReplyAsync($"Hello {Context.User.Mention}!");
        }
        [Command("goodbye")]
        public async Task ByeCommand()
        {
            await ReplyAsync("GoodBye!");
        }
    }
}

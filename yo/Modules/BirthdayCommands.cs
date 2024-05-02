using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Discord;
using System.Globalization;
using Newtonsoft.Json;
using Discord.WebSocket;

namespace BdayBot.Modules
{
    public class BirthdayCommands : ModuleBase<SocketCommandContext>
    {
        private Dictionary<ulong, DateTime> _birthdays;

        public BirthdayCommands()
        {
            _birthdays =  LoadBirthdays().Result;
        }


        public async void SaveBirthdays()
        {
            Dictionary<ulong, DateTime> exbday = LoadBirthdays().Result;

            foreach(var kvp in _birthdays)
            {
                exbday[kvp.Key] = kvp.Value;
            }

            string json = System.Text.Json.JsonSerializer.Serialize(exbday);
            File.WriteAllText("birthdays.json", json);
        } 

        private async Task SaveBirthdaysToFileAsync()
        {
            // Serialize _birthdays dictionary to JSON string
            var json = JsonConvert.SerializeObject(_birthdays, Formatting.Indented);

            // Write JSON string to file
            File.WriteAllText("birthdays.json", json);
        }

        [Command("setbirthday")]
        [Summary("Set a user's or your birthday.")]
        public async Task SetBirthdayCommand(DateTime birthday, IGuildUser user = null)
        {
            if (user == null)
            {
                    var userid = Context.User.Id;
                    _birthdays[Context.User.Id] = birthday;
                    SaveBirthdays();
                    await ReplyAsync("Birthday set successfully!");
                    await ReplyAsync($"Your birthday is on {birthday.ToString("MMMM dd", CultureInfo.GetCultureInfo("en-US"))}!");
            }
            else
            {
                _birthdays[user.Id] = birthday;
                SaveBirthdays();
                await ReplyAsync("Birthday set successfully!");
                await ReplyAsync($"{user.Mention}'s birthday is set on {birthday.ToString("MMMM dd", CultureInfo.GetCultureInfo("en-US"))}!");
            }
        }
        [Command("remove")]
        [Summary("Remove a user's birthday.")]
        public async Task DelBirthdayCommand(IGuildUser user = null)
        {
            var userid = Context.User.Id;
            if (user == null)
            {
                _birthdays.Remove(userid);
                await SaveBirthdaysToFileAsync();
                await ReplyAsync("Your Birthday removed successfully!");
            }
            else
            {
                _birthdays.Remove(user.Id);
                await SaveBirthdaysToFileAsync();
                await ReplyAsync($"{user.Mention}'s Birthday removed successfully!");
            }
        }

        [Command("getbd")]
        [Summary("Gets user's birthday")]
        public async Task GetBirthdayCommand(IGuildUser user = null)
        {
            await LoadBirthdays();
            var userId = Context.User.Id;
            if (user == null)
            {
            
                if (_birthdays.ContainsKey(userId) && user == null)
                {
                    DateTime birthday = _birthdays[userId];
                    await ReplyAsync($"Your birthday is on {birthday.ToString("MMMM dd", CultureInfo.GetCultureInfo("en-US"))}!");
                }
                else
                {
                    await ReplyAsync("No Bday!");
                }
                return;
            }
              

                if (_birthdays.ContainsKey(user.Id))
                {
                    DateTime birthday = _birthdays[user.Id];
                    await ReplyAsync($"{user.Mention} is on {birthday.ToString("MMMM dd", CultureInfo.GetCultureInfo("en-US"))}!");
                }
                else
                {
                    await ReplyAsync("No Bday!");
                }
            
        }

        [Command("getbd")]
        [Summary("Get the birthday list of a specific month")]
        public Task CurrentMonthBirthdays(int? currentmonth = null)
        {
            _ = Task.Run(async () => {
            int currentm;
            var guild = Context.Guild;
            var channel = guild.GetTextChannel(1176160155419803708);
            await LoadBirthdays();
            if (currentmonth == null)
            {
                currentm = DateTime.Now.Month;
            }
            else
            {
                currentm = (int)currentmonth;
            }
            var birthdayyy = _birthdays.Where(kv => kv.Value.Month == currentm);
            await ReplyAsync($"Here are the users who have birthday in the month of {CultureInfo.GetCultureInfo("en-US").DateTimeFormat.GetMonthName(currentm)}:");
            if (birthdayyy.Any())
            {
                var mylist = birthdayyy.ToList().OrderBy(x => x.Value);
                foreach (var bday in mylist)
                {
                    await ReplyAsync($"<@{bday.Key}> : {bday.Value.ToString("MMMM dd", CultureInfo.GetCultureInfo("en-US"))}");
                }
            }
            else
            {
                await ReplyAsync("Nothing!");
            }
            });
            return Task.CompletedTask;
        }

        [Command("getall")]
        [Summary("Gets all user's birthdays")]
        public Task CurrentAllBirthdays()
        {
            _ = Task.Run(async () => {
                await LoadBirthdays();
                await ReplyAsync("Here is the birthday list of all users :");

                var mylist = from _birthdays in _birthdays orderby _birthdays.Value ascending select _birthdays;
                foreach (var bday in mylist)
                {
                    await ReplyAsync($"<@{bday.Key}> : {bday.Value.ToString("MMMM dd", CultureInfo.GetCultureInfo("en-US"))}");
                }
            });
            return Task.CompletedTask;
        }

        private async Task <Dictionary<ulong, DateTime>> LoadBirthdays()
        {
            if (File.Exists("birthdays.json"))
            {
                string json = File.ReadAllText("birthdays.json");
                return System.Text.Json.JsonSerializer.Deserialize<Dictionary<ulong, DateTime>>(json);
            }
            else
            {
                return new Dictionary<ulong, DateTime>();
            }
        }
    }
}

using CodenameIndigo.Modules.Preconditions;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CodenameIndigo.Modules.Commands
{
    public class StartTourneyCommand : InteractiveBase
    {
        [Command("start"), MaintenancePrecon()]
        public async Task StartTourney(int tid)
        {
            await Context.Channel.SendMessageAsync("", false, new EmbedBuilder() {Title = "Starting Tourney...", Color = Color.LightOrange, Description = $"Starting tourney with id {tid} please wait a moment."});

            if (await (await RandomizationHelper.GetPlayersInTourney(tid)).GenerateRandomBrackets(tid))
            {
                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder() { Title = "Tourney Started", Color = Color.Green, Description = $"The tourney has begun. Posting brackets..." });
            }
            else
            {
                await Context.Channel.SendMessageAsync("", false, new EmbedBuilder() { Title = "Tourney Failed to Start", Color = Color.DarkRed, Description = $"Error. Please contact an admin." });
            }            
        }
    }
}

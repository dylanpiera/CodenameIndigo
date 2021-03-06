﻿using Discord.Addons.Interactive;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CodenameIndigo.Modules.Commands
{
    public class PlayerListCommand : InteractiveBase
    {
        [Command("playerlist")]
        public async Task PlayerList()
        {
            List<Player> participants = await RandomizationHelper.GetPlayersInTourney((await DatabaseHelper.GetLatestTourneyAsync()).ID);

            string participantString = "Discord Username - Showdown Username\n\n";
            foreach (Player player in participants)
            {
                participantString += $"{player.DiscordName} - {player.ShowdownName}\n";
            }

            await Context.Channel.SendMessageAsync($"**Full Player List:**\n```{participantString}```");
        }
    }
}

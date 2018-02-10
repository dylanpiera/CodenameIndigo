using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using System.Linq;

namespace CodenameIndigo.Modules.Preconditions
{
    class MaintenancePrecon : PreconditionAttribute
    {
        private static readonly ulong maintenanceID = 206612036380000257;

        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if ((context.User as IGuildUser).RoleIds.Contains(maintenanceID))
                return PreconditionResult.FromSuccess();

            if (context.Channel.Id == (await context.User.GetOrCreateDMChannelAsync()).Id)
                await context.Channel.SendMessageAsync("", false, new EmbedBuilder() { Title = "Hey Listen!", Color = Color.Orange, Description = "You can only use this command from the Bulbagarden Discord!" });
            return PreconditionResult.FromError("");
        }
    }
}

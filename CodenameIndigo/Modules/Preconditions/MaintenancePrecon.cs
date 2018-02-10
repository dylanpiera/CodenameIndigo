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

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if ((context.User as IGuildUser).RoleIds.Contains(maintenanceID))
                return PreconditionResult.FromSuccess();
            return PreconditionResult.FromError("");
        }
    }
}

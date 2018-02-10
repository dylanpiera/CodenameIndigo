using Discord;
using Discord.Commands;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CodenameIndigo.Modules.Preconditions
{
    class InSignupPrecon : PreconditionAttribute
    {
        public async override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            return PreconditionResult.FromSuccess();
        }
    }
}

using Discord.Addons.Interactive;
using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace CodenameIndigo.Modules.Criteria
{
    class EnsureChannelCriterion : ICriterion<SocketMessage>
    {
        private readonly ulong _id;
        public EnsureChannelCriterion(ulong id) => _id = id;

        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, SocketMessage parameter)
        {
            bool ok = _id == parameter.Channel.Id && parameter.Author.Id != sourceContext.Client.CurrentUser.Id;
            return Task.FromResult(ok);
        }
    }
}

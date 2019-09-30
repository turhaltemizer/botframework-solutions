// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Schema;

namespace SimpleRootBot.Bots
{
    public class RootBot : IBot
    {
        private readonly SkillConnector _skillConnector;

        public RootBot(SkillConnector skillConnector)
        {
            _skillConnector = skillConnector;
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = new CancellationToken())
        {
            var ret = await _skillConnector.ProcessActivityAsync(turnContext, turnContext.Activity, cancellationToken);

            if (ret.Status == SkillTurnStatus.Complete)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("The skill has ended"), cancellationToken);
            }
        }
    }

    public static class ContextEx
    {
        // Draft approach to Forward hanging from context
        // var ret = await turnContext.ForwardToSkillAsync("bookingDialog", turnContext.Activity, cancellationToken);
        public static async Task<SkillTurnResult> ForwardToSkillAsync(this ITurnContext turnContext, string skillId, Activity activity, CancellationToken cancellationToken)
        {
            // TODO: how do I find the connector for the skillId?
            return new SkillTurnResult(SkillTurnStatus.Complete, null);
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
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
            var ret = await _skillConnector.ForwardActivityAsync(turnContext, turnContext.Activity, InterceptHandler, cancellationToken);
            if (ret != null && ret.Type == ActivityTypes.EndOfConversation)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("The skill has ended"), cancellationToken);
            }
        }

        private static async Task<ResourceResponse[]> InterceptHandler(ITurnContext turnContext, List<Activity> activities, Func<Task<ResourceResponse[]>> next)
        {
            foreach (var activity in activities)
            {
                await turnContext.SendActivityAsync($"Intercepted {activity.Type} {activity.Text}");
            }

            // We can return null if we don't want the activity to continue
            return await next().ConfigureAwait(false);
        }
    }
}

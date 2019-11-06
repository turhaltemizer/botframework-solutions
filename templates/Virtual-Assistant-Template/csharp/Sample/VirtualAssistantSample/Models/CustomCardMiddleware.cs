using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;

namespace VirtualAssistantSample.Models
{
    public class CustomCardMiddleware : IMiddleware
    {
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken = default)
        {
            turnContext.OnSendActivities(SetCustomCard);
            await next(cancellationToken).ConfigureAwait(false);
        }

        private async Task<ResourceResponse[]> SetCustomCard(ITurnContext turnContext, List<Activity> activities, Func<Task<ResourceResponse[]>> next)
        {
            var messageActivities = activities
                .Where(a => a.Type == ActivityTypes.Message)
                .ToList();

            // If the bot is sending message activities to the user (as opposed to trace activities)
            if (messageActivities.Any())
            {
                foreach (var activity in messageActivities)
                {
                    if (activity.Attachments.First().ContentType.Equals("application/vnd.microsoft.card.adaptive"))
                    {
                        activity.Attachments.First().ContentType = "application/vnd.microsoft.card.custom";
                    }
                }
            }

            return await next();
        }
    }
}

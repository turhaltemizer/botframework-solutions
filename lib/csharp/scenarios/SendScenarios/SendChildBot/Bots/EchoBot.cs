// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace SendChildBot.Bots
{
    public class EchoBot : ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var activity = (Activity)turnContext.Activity;
            await turnContext.SendActivityAsync(MessageFactory.Text($"Echo: {activity.Text}"), cancellationToken);

            if (activity.SemanticAction != null)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text($"Semantic Action: {activity.SemanticAction.Id}"), cancellationToken);
                foreach (var entity in activity.SemanticAction.Entities)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Entity: {entity.Key} {JsonConvert.SerializeObject(entity.Value)}"), cancellationToken);
                }
            }

            // Send End of conversation at the end.
            await turnContext.SendActivityAsync(new Activity(ActivityTypes.EndOfConversation), cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Hello and welcome!"), cancellationToken);
                }
            }
        }
    }
}

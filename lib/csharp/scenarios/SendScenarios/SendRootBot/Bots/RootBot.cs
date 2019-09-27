// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Schema;

namespace SendRootBot.Bots
{
    public class RootBot : ActivityHandler
    {
        private readonly IStatePropertyAccessor<Dictionary<string, object>> _convoState;
        private readonly SkillConnector _skillConnector;

        public RootBot(ConversationState conversationState, SkillConnector skillConnector)
        {
            _skillConnector = skillConnector;
            _convoState = conversationState.CreateProperty<Dictionary<string, object>>("CurrentTask");
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var state = await _convoState.GetAsync(turnContext, () => new Dictionary<string, object>(), cancellationToken);

            Activity ret;
            switch (turnContext.Activity.Text)
            {
                case "SendAsIs":
                    ret = await _skillConnector.ForwardActivityAsync(turnContext, turnContext.Activity as Activity, cancellationToken);
                    break;

                case "SendAsIsWithValues":
                    var activityWithValues = (Activity)turnContext.Activity;
                    var actionInfo = new SemanticAction("BookFlight");
                    activityWithValues.SemanticAction = actionInfo;
                    activityWithValues.SemanticAction.Entities = new Dictionary<string, Entity>
                    {
                        { "bookingInfo", new Entity() },
                    };
                    activityWithValues.SemanticAction.Entities["bookingInfo"].SetAs(new BookingDetails()
                    {
                        Destination = "NY",
                        Origin = "SEA",
                        TravelDate = "Tomorrow",
                    });

                    ret = await _skillConnector.ForwardActivityAsync(turnContext, activityWithValues, cancellationToken);
                    break;

                default:
                    await turnContext.SendActivityAsync(MessageFactory.Text("Didn't get that"), cancellationToken);
                    return;
            }

            if (ret != null && ret.Type == ActivityTypes.EndOfConversation)
            {
                await turnContext.SendActivityAsync(MessageFactory.Text("The skill has ended"), cancellationToken);
                await SendMainMenuAsync(turnContext, cancellationToken);
            }
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await SendMainMenuAsync(turnContext, cancellationToken);
                }
            }
        }

        private static async Task SendMainMenuAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            IEnumerable<string> actions = new List<string>
            {
                "SendAsIs",
                "SendAsIsWithValues",
            };
            var msg = MessageFactory.SuggestedActions(actions, "Hello and welcome to the Send Scenarios bot!");
            await turnContext.SendActivityAsync(msg, cancellationToken);
        }

        private class BookingDetails
        {
            public string Destination { get; set; }

            public string Origin { get; set; }

            public string TravelDate { get; set; }
        }
    }
}

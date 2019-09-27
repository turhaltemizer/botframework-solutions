// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Skills.Gaps;
using Microsoft.Bot.Schema;

namespace SendChildBot.Bots
{
    public class ChildBot<T> : ActivityHandler
        where T : Dialog
    {
        public ChildBot(ConversationState conversationState, T dialog)
        {
            ConversationState = conversationState;
            Dialog = dialog;
        }

        protected BotState ConversationState { get; }

        protected Dialog Dialog { get; }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Run the Dialog with the new message Activity.
            var result = await Dialog.InvokeAsync(turnContext, ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);

            if (result.Status == DialogTurnStatus.Complete)
            {
                // Send End of conversation at the end.
                await turnContext.SendActivityAsync(new Activity(ActivityTypes.EndOfConversation), cancellationToken);
            }
        }
    }
}

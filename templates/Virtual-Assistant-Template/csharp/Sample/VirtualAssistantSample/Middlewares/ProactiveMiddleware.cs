using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace VirtualAssistantSample.Middlewares
{
    public class ProactiveMiddleware : IMiddleware
    {
        private ConcurrentDictionary<string, ConversationReference> _conversationReferences;

        public ProactiveMiddleware(
            ConcurrentDictionary<string, ConversationReference> conversationReferences)
        {
            _conversationReferences = conversationReferences;
        }

        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate nextTurn, CancellationToken cancellationToken)
        {
            var conversationReference = turnContext.Activity.GetConversationReference();
            var channelConversation = $"{turnContext.Activity.ChannelId}/{turnContext.Activity.Conversation.Id}";
            _conversationReferences.AddOrUpdate(channelConversation, conversationReference, (key, oldValue) =>
            {
                return conversationReference;
            });

            await nextTurn(cancellationToken).ConfigureAwait(false);
        }
    }
}

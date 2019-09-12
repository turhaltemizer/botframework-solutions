using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.Solutions;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using VirtualAssistantSample.Bots;
using VirtualAssistantSample.Dialogs;
using VirtualAssistantSample.Services;

namespace VirtualAssistantSample.Controllers
{
    public class ProactiveController : ControllerBase
    {
        // TODO directline?
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        private ConcurrentDictionary<string, ConversationReference> _conversationReferences;
        ConversationState _conversationState;
        MainDialog _mainDialog;
        IBot _dialogBot;

        public ProactiveController(
            BotSettings botSettings,
            ConversationState conversationState,
            MainDialog mainDialog,
            IBot dialogBot,
            IBotFrameworkHttpAdapter botFrameworkHttpAdapter,
            ConcurrentDictionary<string, ConversationReference> conversationReferences)
        {
            _conversationState = conversationState;
            _mainDialog = mainDialog;
            _dialogBot = dialogBot;
            _adapter = botFrameworkHttpAdapter;
            _conversationReferences = conversationReferences;
            _appId = botSettings.MicrosoftAppId;
        }

        [HttpGet, Route("proactive/send")]
        public async Task SendAsync()
        {
            using (var writer = new StreamWriter(Response.Body))
            {
                foreach (var pair in _conversationReferences)
                {
                    await ((BotFrameworkHttpAdapter)_adapter).ContinueConversationAsync(_appId, pair.Value, async (tc, ct) =>
                    {
                        var act = tc.Activity.CreateReply("context-less proactive message");
                        await tc.SendActivityAsync(act);
                        writer.WriteLine($"send context-less proactive message to {pair.Key}");
                    }, default(CancellationToken));
                }
            }
        }

        [HttpGet, Route("proactive/dialog")]
        public async Task DialogAsync()
        {
            using (var writer = new StreamWriter(Response.Body))
            {
                foreach (var pair in _conversationReferences)
                {
                    await ((BotFrameworkHttpAdapter)_adapter).ContinueConversationAsync(_appId, pair.Value, async (tc, ct) =>
                    {
                        var jobj = new JObject();
                        jobj["action"] = "startOnboarding";
                        tc.Activity.Value = jobj;

                        // make use of RouterDialog::OnEventAsync
                        await _dialogBot.OnTurnAsync(tc, ct);
                        writer.WriteLine($"send dialog proactive message to {pair.Key}");
                    }, default(CancellationToken));
                }
            }
        }
    }
}

using System.Globalization;
using EmailSkill.Responses.Shared;
using EmailSkill.Services;
using EmailSkill.Utilities;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Declarative;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Builder.Solutions.Middleware;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;

namespace EmailSkill.Adapters
{
    public class DefaultAdapter : BotFrameworkHttpAdapter
    {
        public DefaultAdapter(
            BotSettings settings,
            ICredentialProvider credentialProvider,
            IBotTelemetryClient telemetryClient,
            ResourceExplorer resourceExplorer,
            UserState userState,
            ConversationState conversationState,
            TelemetryInitializerMiddleware telemetryMiddleware)
            : base(credentialProvider)
        {
            OnTurnError = async (context, exception) =>
            {
                CultureInfo.CurrentUICulture = new CultureInfo(context.Activity.Locale);

                // var activity = await LGHelper.GenerateMessageAsync(_lgMultiLangEngine, context, "[EmailErrorMessage]", null);

                var activity = await LGHelper.GenerateMessageAsync(context, EmailSharedResponses.EmailErrorMessage, null);
                await context.SendActivityAsync(activity);

                await context.SendActivityAsync(new Activity(type: ActivityTypes.Trace, text: $"Email Skill Error: {exception.Message} | {exception.StackTrace}"));
                telemetryClient.TrackException(exception);
            };

            Use(telemetryMiddleware);
            Use(new TranscriptLoggerMiddleware(new AzureBlobTranscriptStore(settings.BlobStorage.ConnectionString, settings.BlobStorage.Container)));
            Use(new TelemetryLoggerMiddleware(telemetryClient, logPersonalInformation: true));
            Use(new ShowTypingMiddleware());
            Use(new SetLocaleMiddleware(settings.DefaultLocale ?? "en-us"));
            Use(new EventDebuggerMiddleware());

            this.UseState(userState, conversationState);
            this.UseResourceExplorer(resourceExplorer);
            this.UseLanguageGeneration(resourceExplorer, "ResponsesAndTexts.lg");
        }
    }
}
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Bot.StreamingExtensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Skills.Integration
{
    /// <summary>
    /// Handles the websocket responses returned by a remote skill.
    /// </summary>
    internal class SkillWebSocketsResponseHandler : RequestHandler, ISkillResponseHandler
    {
        private readonly IBotTelemetryClient _botTelemetryClient;
        private readonly Router _router;
        private Activity _endOfConversationActivity;

        public SkillWebSocketsResponseHandler(ITurnContext turnContext, SendActivitiesHandler activitiesHandler, IBotTelemetryClient botTelemetryClient)
        {
            _botTelemetryClient = botTelemetryClient;

            var routes = new[]
            {
                new RouteTemplate()
                {
                    Method = "POST",
                    Path = "/activities/{activityId}",
                    ActionAsync = async (request, routeData, cancellationToken) =>
                    {
                        var activity = request.ReadBodyAsJson<Activity>();
                        if (activity != null)
                        {
                            // Store the end of conversation activity.
                            if (activity.Type == ActivityTypes.EndOfConversation)
                            {
                                _endOfConversationActivity = activity;
                            }

                            return await OnSendActivityAsync(turnContext, activity, activitiesHandler, cancellationToken).ConfigureAwait(false);
                        }

                        throw new Exception("Error deserializing activity response!");
                    },
                },
                new RouteTemplate
                {
                    Method = "PUT",
                    Path = "/activities/{activityId}",
                    ActionAsync = async (request, routeData, cancellationToken) =>
                    {
                        var activity = request.ReadBodyAsJson<Activity>();
                        return await OnUpdateActivityAsync(turnContext, activity, activitiesHandler, cancellationToken).ConfigureAwait(false);
                    },
                },
                new RouteTemplate
                {
                    Method = "DELETE",
                    Path = "/activities/{activityId}",
                    ActionAsync = async (request, routeData, cancellationToken) =>
                    {
                        return await OnDeleteActivityAsync(turnContext, routeData.activityId, activitiesHandler, cancellationToken);
                    },
                },
            };

            _router = new Router(routes);
        }

        public override async Task<StreamingResponse> ProcessRequestAsync(ReceiveRequest request, ILogger<RequestHandler> logger, object context = null, CancellationToken cancellationToken = default)
        {
            var routeContext = _router.GetRoute(request);
            if (routeContext == null)
            {
                // TODO: should we throw an exception if we can't find a rout for the request?
                return StreamingResponse.NotFound();
            }

            try
            {
                var responseBody = await routeContext.ActionAsync(request, routeContext.RouteData, cancellationToken).ConfigureAwait(false);
                var response = new StreamingResponse { StatusCode = (int)HttpStatusCode.OK };
                string content = JsonConvert.SerializeObject(responseBody, SerializationSettings.DefaultSerializationSettings);
                response.SetBody(content);
                return response;
            }
#pragma warning disable CA1031 // Do not catch general exception types (disable, using exception data to populate the response.
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _botTelemetryClient.TrackException(ex);
                var response = new StreamingResponse { StatusCode = (int)HttpStatusCode.InternalServerError };
                var content = JsonConvert.SerializeObject(ex, SerializationSettings.DefaultSerializationSettings);
                response.SetBody(content);
                return response;
            }
        }

        public async Task<ResourceResponse> OnSendActivityAsync(ITurnContext context, Activity activity, SendActivitiesHandler activitiesHandler, CancellationToken cancellationToken)
        {
            if (activitiesHandler != null)
            {
                var r = await activitiesHandler.Invoke(context, new List<Activity> { activity }, async () =>
                {
                    var response = await context.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
                    var rr = new ResourceResponse[1] { response };
                    return rr;
                }).ConfigureAwait(false);
                return r[0];
            }

            return await context.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
        }

        public async Task<ResourceResponse> OnUpdateActivityAsync(ITurnContext context, Activity activity, SendActivitiesHandler activitiesHandler, CancellationToken cancellationToken)
        {
            return await context.UpdateActivityAsync(activity, cancellationToken).ConfigureAwait(false);
        }

        public async Task OnDeleteActivityAsync(ITurnContext context, string activityId, SendActivitiesHandler activitiesHandler, CancellationToken cancellationToken)
        {
            await context.DeleteActivityAsync(activityId, cancellationToken).ConfigureAwait(false);
        }

        public Activity GetEndOfConversationActivity()
        {
            return _endOfConversationActivity;
        }
    }
}

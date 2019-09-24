// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;

namespace Microsoft.Bot.Builder.Skills
{
    public interface ISkillResponseHandler
    {
        Task<ResourceResponse> OnSendActivityAsync(ITurnContext context, Activity activity, SendActivitiesHandler activitiesHandler, CancellationToken cancellationToken);

        Task<ResourceResponse> OnUpdateActivityAsync(ITurnContext context, Activity activity, SendActivitiesHandler activitiesHandler, CancellationToken cancellationToken);

        Task OnDeleteActivityAsync(ITurnContext context, string activityId, SendActivitiesHandler activitiesHandler, CancellationToken cancellationToken);

        Activity GetEndOfConversationActivity();
    }
}

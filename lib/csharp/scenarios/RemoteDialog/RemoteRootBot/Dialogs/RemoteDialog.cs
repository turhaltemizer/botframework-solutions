// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace RemoteRootBot.Dialogs
{
    // TODO: work with Steve and see if we can make this a Dialog instead of a component dialog.
    public class BookingDialog : ComponentDialog
    {
        private readonly SkillConnector _skillConnector;

        public BookingDialog(SkillConnector skillConnector)
            : base(nameof(BookingDialog))
        {
            _skillConnector = skillConnector;
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var turnContext = dc.Context;
            var bookFlightActivityWithData = turnContext.Activity;

            // Set the action and the entities on the activity before sending it to the remote skill.
            bookFlightActivityWithData.SemanticAction = new SemanticAction("BookFlight")
            {
                Entities = new Dictionary<string, Entity>
                {
                    { "bookingInfo", new Entity() },
                },
            };

            var bookingDetails = (BookingDetails)options;
            bookFlightActivityWithData.SemanticAction.Entities["bookingInfo"].SetAs(bookingDetails);

            return await SendToSkill(dc, bookFlightActivityWithData, cancellationToken);
        }

        public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default)
        {
            var turnContext = dc.Context;
            var activity = turnContext.Activity;
            return await SendToSkill(dc, activity, cancellationToken);
        }

        private async Task<DialogTurnResult> SendToSkill(DialogContext dc, Activity activity, CancellationToken cancellationToken)
        {
            var ret = await _skillConnector.ForwardActivityAsync(dc.Context, activity, cancellationToken);

            var turnResult = new DialogTurnResult(DialogTurnStatus.Waiting);

            // Check if the remote skill ended.
            if (ret != null && ret.Type == ActivityTypes.EndOfConversation)
            {
                // TODO: figure out an elegant way of casting the return value.
                var bookingDetails = JsonConvert.DeserializeObject<BookingDetails>(JsonConvert.SerializeObject(ret.Value));
                return await EndComponentAsync(dc, bookingDetails, cancellationToken);
            }

            return turnResult;
        }
    }
}

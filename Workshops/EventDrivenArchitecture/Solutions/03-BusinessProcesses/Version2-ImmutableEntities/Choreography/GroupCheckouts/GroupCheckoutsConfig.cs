﻿using BusinessProcesses.Core;
using BusinessProcesses.Version2_ImmutableEntities.Choreography.GuestStayAccounts;

namespace BusinessProcesses.Version2_ImmutableEntities.Choreography.GroupCheckouts;

using static GuestStayAccountEvent;
using static GroupCheckoutCommand;
using static SagaResult;

public static class GroupCheckoutsConfig
{
    public static void ConfigureGroupCheckouts(
        EventBus eventBus,
        CommandBus commandBus,
        GroupCheckoutFacade groupCheckoutFacade
    )
    {
        eventBus
            .Subscribe<GroupCheckoutEvent.GroupCheckoutInitiated>((@event, ct) =>
                commandBus.Send(GroupCheckoutSaga.On(@event).Select(c => c.Message).ToArray(), ct)
            )
            .Subscribe<GuestCheckedOut>((@event, ct) =>
                GroupCheckoutSaga.On(@event) is Command<RecordGuestCheckoutCompletion>(var command)
                    ? commandBus.Send([command], ct)
                    : ValueTask.CompletedTask
            )
            .Subscribe<GuestCheckoutFailed>((@event, ct) =>
                GroupCheckoutSaga.On(@event) is Command<RecordGuestCheckoutFailure>(var command)
                    ? commandBus.Send([command], ct)
                    : ValueTask.CompletedTask
            );

        commandBus.Handle<RecordGuestCheckoutCompletion>(groupCheckoutFacade.RecordGuestCheckoutCompletion);
        commandBus.Handle<RecordGuestCheckoutFailure>(groupCheckoutFacade.RecordGuestCheckoutFailure);
    }
}

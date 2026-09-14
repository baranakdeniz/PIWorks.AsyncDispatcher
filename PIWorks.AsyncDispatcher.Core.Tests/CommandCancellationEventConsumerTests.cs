using AutoFixture.Xunit2;
using Moq;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using PIWorks.AsyncDispatcher.Core.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core.Tests
{
    public class CommandCancellationEventConsumerTests
    {
        //[Theory]
        //[AutoMoqData]
        //public async Task Handle_ShouldCancelCommand_WhenCalled(
        //    [Frozen] Mock<ICommandCancellationManager<Guid>> mockManager
        //    ,CancelCommandRequestedEventConsumer<Guid> sut,
        //    CancellationToken cancellationToken)

        //{
            
        //    var commandId = Guid.NewGuid();
        //     var notification = new CommandCancelledEvent<Guid>(commandId);
        //   await sut.Handle(notification,cancellationToken);
        //    mockManager.Verify(t=>t.Cancel(commandId), Times.Once);

        //}
    }
}

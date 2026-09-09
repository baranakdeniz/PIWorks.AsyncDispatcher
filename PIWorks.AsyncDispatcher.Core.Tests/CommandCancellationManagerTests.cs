using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core.Tests
{
    public  class CommandCancellationManagerTests
    {

        [Fact]
        public async Task RegisterCommand_ShouldRegisterWhenCalled( )
        {
            var commandId = Guid.NewGuid();
            var sut = new CommandCancellationManager<Guid>();
             var token= sut.RegisterCommand( commandId );

            Assert.NotEqual(CancellationToken.None, token);
            Assert.True(token.CanBeCanceled);
            Assert.False(token.IsCancellationRequested);
        }

        [Fact]
        public void RegisterCommand_ShouldReturnSameToken_WhenCalledMultipleTimeForSameKey()
        {
            var commandId= Guid.NewGuid();
            var sut = new CommandCancellationManager<Guid>();

            var token1= sut.RegisterCommand( commandId );
            var token2 = sut.RegisterCommand( commandId );

            Assert.Equal(token1, token2);
        }

        [Fact]
        public void Remove_ShouldRemoveToken_WhenCalled()
        {
            var commandId = Guid.NewGuid();
            var sut = new CommandCancellationManager<Guid>();
            var token = sut.RegisterCommand(commandId);
             sut.Remove(commandId);
            var newToken = sut.RegisterCommand(commandId);
            Assert.NotEqual(token, newToken);


        }
        [Fact]
        public void Cancel_ShouldCancelToken_WhenCalled()
        {
            var commandId= Guid.NewGuid();
            var sut = new CommandCancellationManager<Guid>();
            var token = sut.RegisterCommand(commandId);
            sut.Cancel(commandId);
            var flagCancel = token.IsCancellationRequested;
            //cancel olduğunda biizm token gitmiş olmalı 
            Assert.True(flagCancel);
        }

    }
}

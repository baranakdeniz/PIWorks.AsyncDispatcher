using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core.Tests
{
    public class InMemoryCommandTrackerTests
    {
 
        [Fact]
        public async Task GetStatusAsync_ShouldReturnNull_WhenCommandIdDoesNotExist()
        {//Arrange
            var sut = new InMemoryCommandTracker<Guid>();
            var commandId =  Guid.NewGuid();
            //Act
            var result = await sut.GetStatusAsync(commandId);
            //Assert
            Assert.Null(result);

        }
        [Fact]
        public async Task GetStatusAsync_ShouldReturnStatus_WhenCalled()
        {
            var commandId = Guid.NewGuid();
            var sut = new InMemoryCommandTracker<Guid>();
            var result= await sut.GetStatusAsync(commandId);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task InitializeAsync_ShouldAddCommandStateInfo_WhenCalled()
        {
            var sut = new InMemoryCommandTracker<Guid>();
            var commandId = Guid.NewGuid();

               await sut.InitializeAsync(commandId);
            Assert.NotNull(await sut.GetStatusAsync(commandId));//doğru mu ?
        }

        [Fact]
        public async Task UpdateStatusAsync_ShouldUpdateCommandStateInfo_WhenCalled()
        {
            var sut = new InMemoryCommandTracker<Guid>();
            var commandId= Guid.NewGuid();
            var expectedStatus = CommandStatus.Running;
            var expectedWorkerId = "Test-Worker-Instance";
            await sut.UpdateStatusAsync(commandId, expectedStatus, expectedWorkerId);
            var savedState = await sut.GetStatusAsync(commandId);
            Assert.NotNull(savedState);
            Assert.Equal(expectedStatus, savedState.Value.Status);
            Assert.Equal(expectedWorkerId, savedState.Value.WorkerId);


        }

        [Fact]
        public async Task GetRunningCommands_ShouldCountCorrectWhenCalled()
        {
            var tracker = new InMemoryCommandTracker<Guid>();
            var commandId = Guid.NewGuid();
            var commandId2 = Guid.NewGuid();
            var expectedWorkerId = "Test-Worker-Instance";
            await tracker.UpdateStatusAsync(commandId,CommandStatus.Running,expectedWorkerId);
            await tracker.UpdateStatusAsync(commandId2, CommandStatus.Running, expectedWorkerId);

            var count = await tracker.GetRunningCommandsCountAsync();
            Assert.Equal(2, count);

        }

    }
}

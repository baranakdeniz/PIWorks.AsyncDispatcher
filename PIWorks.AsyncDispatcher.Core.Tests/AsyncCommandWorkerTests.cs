using AutoFixture.Xunit2;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core.Tests
{
    public class AsyncCommandWorkerTests
    {
        public class DummyCommand : IAsyncCommand<Guid>
        {
            public Guid Key { get; set; } = Guid.NewGuid();
        }

   
            [Theory]
            [AutoMoqData]
            public async Task ExecuteAsync_ShouldDequeueAndProcessCommand_WhenCalled(
                [Frozen] Mock<ICommandTracker<Guid>> mockTracker,
                [Frozen] Mock<ICommandBus<Guid>> mockBus,
                [Frozen] Mock<IServiceScopeFactory> mockScopeFactory,
                [Frozen] Mock<ICommandCancellationManager<Guid>> mockCancellationManager,
                [Frozen] Mock<IServiceScope> mockServiceScope,
                [Frozen] Mock<IServiceProvider> mockProvider,
                TestableAsyncCommandWorker sut,
                DummyCommand command)
            {
                
                var envelope = new CommandEnvelope<DummyCommand, Guid>(command);
                var handlerCompleted = new TaskCompletionSource<bool>(); 

                var mockHandler = new Mock<IAsyncCommandHandler<DummyCommand, Guid>>();
                mockHandler
                    .Setup(h => h.ExecuteAsync(It.IsAny<DummyCommand>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask)
                    .Callback(() => handlerCompleted.TrySetResult(true)); 

                mockProvider
                    .Setup(p => p.GetService(typeof(IAsyncCommandHandler<DummyCommand, Guid>)))
                    .Returns(mockHandler.Object);
                mockServiceScope.Setup(s => s.ServiceProvider).Returns(mockProvider.Object);
                mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockServiceScope.Object);

             
                mockBus
                    .SetupSequence(b => b.DequeueAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(envelope)
                    .ThrowsAsync(new OperationCanceledException());

                // 2. Act
                try
                {
                    await sut.ExecuteForTest(CancellationToken.None);
                }
                catch (OperationCanceledException) { }

            
                await handlerCompleted.Task.WaitAsync(TimeSpan.FromSeconds(2));

                // 3. Assert
                mockBus.Verify(b => b.DequeueAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
                mockHandler.Verify(h => h.ExecuteAsync(It.IsAny<DummyCommand>(), It.IsAny<CancellationToken>()), Times.Once);
                mockTracker.Verify(t => t.UpdateStatusAsync(
                    envelope.CommandId,
                    CommandStatus.Running,
                    It.IsAny<string>()),
                    Times.Once);
            }
        [Theory]
        [AutoMoqData]
        public async Task ExecuteAsync_ShouldUpdateStatusToFinishedAndCleanup_WhenCommandSucceeds(
    [Frozen] Mock<ICommandTracker<Guid>> mockTracker,
    [Frozen] Mock<ICommandBus<Guid>> mockBus,
    [Frozen] Mock<IServiceScopeFactory> mockScopeFactory,
    [Frozen] Mock<ICommandCancellationManager<Guid>> mockCancellationManager,
    [Frozen] Mock<IServiceScope> mockServiceScope,
    [Frozen] Mock<IServiceProvider> mockProvider,
    TestableAsyncCommandWorker sut,
    DummyCommand command)
        {
            // Arrange
            var envelope = new CommandEnvelope<DummyCommand, Guid>(command);
            var handlerCompleted = new TaskCompletionSource<bool>();

            var mockHandler = new Mock<IAsyncCommandHandler<DummyCommand, Guid>>();
            mockHandler
                .Setup(h => h.ExecuteAsync(It.IsAny<DummyCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Callback(() => handlerCompleted.TrySetResult(true));

            mockProvider
                .Setup(p => p.GetService(typeof(IAsyncCommandHandler<DummyCommand, Guid>)))
                .Returns(mockHandler.Object);
            mockServiceScope.Setup(s => s.ServiceProvider).Returns(mockProvider.Object);
            mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockServiceScope.Object);

            mockBus
                .SetupSequence(b => b.DequeueAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(envelope)
                .ThrowsAsync(new OperationCanceledException());

            // Act
            try { await sut.ExecuteForTest(CancellationToken.None); }
            catch (OperationCanceledException) { }

            await handlerCompleted.Task.WaitAsync(TimeSpan.FromSeconds(2));

            // Assert
            mockTracker.Verify(t => t.UpdateStatusAsync(envelope.CommandId, CommandStatus.Finished, It.IsAny<string>()), Times.Once);
            mockCancellationManager.Verify(c => c.Remove(envelope.CommandId), Times.Once);
        }
        [Theory]
        [AutoMoqData]
        public async Task ExecuteAsync_ShouldUpdateStatusToError_WhenHandlerThrowsException(
    [Frozen] Mock<ICommandTracker<Guid>> mockTracker,
    [Frozen] Mock<ICommandBus<Guid>> mockBus,
    [Frozen] Mock<IServiceScopeFactory> mockScopeFactory,
    [Frozen] Mock<ICommandCancellationManager<Guid>> mockCancellationManager,
    [Frozen] Mock<IServiceScope> mockServiceScope,
    [Frozen] Mock<IServiceProvider> mockProvider,
    TestableAsyncCommandWorker sut,
    DummyCommand command)
        {
            // Arrange
            var envelope = new CommandEnvelope<DummyCommand, Guid>(command);
            var failureLogged = new TaskCompletionSource<bool>();
            var expectedException = new InvalidOperationException("Handler failed unexpectedly!");

            var mockHandler = new Mock<IAsyncCommandHandler<DummyCommand, Guid>>();
            mockHandler
                .Setup(h => h.ExecuteAsync(It.IsAny<DummyCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            mockProvider
                .Setup(p => p.GetService(typeof(IAsyncCommandHandler<DummyCommand, Guid>)))
                .Returns(mockHandler.Object);
            mockServiceScope.Setup(s => s.ServiceProvider).Returns(mockProvider.Object);
            mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockServiceScope.Object);

           
            mockTracker
                .Setup(t => t.UpdateStatusAsync(envelope.CommandId, CommandStatus.Error, It.IsAny<string>(), expectedException.Message))
                .Returns(Task.CompletedTask)
                .Callback(() => failureLogged.TrySetResult(true));

            mockBus
                .SetupSequence(b => b.DequeueAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(envelope)
                .ThrowsAsync(new OperationCanceledException());

            // Act
            try { await sut.ExecuteForTest(CancellationToken.None); }
            catch (OperationCanceledException) { }

            await failureLogged.Task.WaitAsync(TimeSpan.FromSeconds(2));

            // Assert
            mockTracker.Verify(t => t.UpdateStatusAsync(
                envelope.CommandId,
                CommandStatus.Error,
                It.IsAny<string>(),
                expectedException.Message),
                Times.Once);

            mockCancellationManager.Verify(c => c.Remove(envelope.CommandId), Times.Once);
        }

        [Theory]
        [AutoMoqData]
        public async Task ExecuteAsync_ShouldUpdateStatusToCancelled_WhenCommandIsCancelled(
    [Frozen] Mock<ICommandTracker<Guid>> mockTracker,
    [Frozen] Mock<ICommandBus<Guid>> mockBus,
    [Frozen] Mock<IServiceScopeFactory> mockScopeFactory,
    [Frozen] Mock<ICommandCancellationManager<Guid>> mockCancellationManager,
    [Frozen] Mock<IServiceScope> mockServiceScope,
    [Frozen] Mock<IServiceProvider> mockProvider,
    TestableAsyncCommandWorker sut,
    DummyCommand command)
        {
            // Arrange
            var envelope = new CommandEnvelope<DummyCommand, Guid>(command);
            var cancelLogged = new TaskCompletionSource<bool>();

            var mockHandler = new Mock<IAsyncCommandHandler<DummyCommand, Guid>>();
            mockHandler
                .Setup(h => h.ExecuteAsync(It.IsAny<DummyCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            mockProvider
                .Setup(p => p.GetService(typeof(IAsyncCommandHandler<DummyCommand, Guid>)))
                .Returns(mockHandler.Object);
            mockServiceScope.Setup(s => s.ServiceProvider).Returns(mockProvider.Object);
            mockScopeFactory.Setup(f => f.CreateScope()).Returns(mockServiceScope.Object);

        
            mockTracker
                .Setup(t => t.UpdateStatusAsync(envelope.CommandId, CommandStatus.Cancelled, It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .Callback(() => cancelLogged.TrySetResult(true));

            mockBus
                .SetupSequence(b => b.DequeueAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(envelope)
                .ThrowsAsync(new OperationCanceledException());

            // Act
            try { await sut.ExecuteForTest(CancellationToken.None); }
            catch (OperationCanceledException) { }

            await cancelLogged.Task.WaitAsync(TimeSpan.FromSeconds(2));

            // Assert
            mockTracker.Verify(t => t.UpdateStatusAsync(
                envelope.CommandId,
                CommandStatus.Cancelled,
                It.IsAny<string>()),
                Times.Once);

            mockCancellationManager.Verify(c => c.Remove(envelope.CommandId), Times.Once);
        }


    }


    }
    
//[Theory]
        //[AutoMoqData]
        //public async Task ExecuteAsync_ShouldDequeueAndProcessCommand_WhenCalled(
        //    [Frozen] Mock<ICommandBus<Guid>> mockBus,
        //    [Frozen] Mock<IServiceProvider> mockServiceProvider,
        //    [Frozen] Mock<ICommandTracker<Guid>> mockTracker,
        //    AsyncCommandWorker<Guid> sut,
        //    DummyCommand command)
        //{
        //    using var cts = new CancellationTokenSource();
        //    var envelope = new CommandEnvelope<DummyCommand, Guid>(command);

        //    var mockHandler = new Mock<IAsyncCommandHandler<DummyCommand, Guid>>();
        //    mockHandler
        //        .Setup(h => h.ExecuteAsync(command, It.IsAny<CancellationToken>()))
        //        .Returns(Task.CompletedTask);

        //    mockServiceProvider
        //        .Setup(sp => sp.GetService(typeof(IAsyncCommandHandler<DummyCommand, Guid>)))
        //        .Returns(mockHandler.Object);

        //    mockBus
        //        .Setup(b => b.DequeueAsync(It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(envelope)
        //        .Callback(() => cts.Cancel());

        //    await sut.StartAsync(cts.Token);
        //    if (sut.ExecuteTask != null)
        //    {
        //        await sut.ExecuteTask;
        //    }

        //    mockBus.Verify(b => b.DequeueAsync(It.IsAny<CancellationToken>()), Times.Once);
        //    mockHandler.Verify(h => h.ExecuteAsync(command, It.IsAny<CancellationToken>()), Times.Once);
        //    mockTracker.Verify(t => t.UpdateStatusAsync(command.Key, CommandStatus.Running, null), Times.Once);
        //    mockTracker.Verify(t => t.UpdateStatusAsync(command.Key, CommandStatus.Finished, null), Times.Once);
        //}

        //[Theory]
        //[AutoMoqData]
        //public async Task ExecuteAsync_ShouldUpdateStatusToRunningBeforeFinished(
        //    [Frozen] Mock<ICommandBus<Guid>> mockBus,
        //    [Frozen] Mock<IServiceProvider> mockServiceProvider,
        //    [Frozen] Mock<ICommandTracker<Guid>> mockTracker,
        //    AsyncCommandWorker<Guid> sut,
        //    DummyCommand command)
        //{
        //    using var cts = new CancellationTokenSource();
        //    var envelope = new CommandEnvelope<DummyCommand, Guid>(command);

        //    var mockHandler = new Mock<IAsyncCommandHandler<DummyCommand, Guid>>();
        //    mockHandler
        //        .Setup(h => h.ExecuteAsync(command, It.IsAny<CancellationToken>()))
        //        .Returns(Task.CompletedTask);

        //    mockServiceProvider
        //        .Setup(sp => sp.GetService(typeof(IAsyncCommandHandler<DummyCommand, Guid>)))
        //        .Returns(mockHandler.Object);

        //    mockBus
        //        .Setup(b => b.DequeueAsync(It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(envelope)
        //        .Callback(() => cts.Cancel());

        //    var sequence = new MockSequence();

        //    mockTracker
        //        .InSequence(sequence)
        //        .Setup(t => t.UpdateStatusAsync(command.Key, CommandStatus.Running, null))
        //        .Returns(Task.CompletedTask);

        //    mockTracker
        //        .InSequence(sequence)
        //        .Setup(t => t.UpdateStatusAsync(command.Key, CommandStatus.Finished, null))
        //        .Returns(Task.CompletedTask);

        //    await sut.StartAsync(cts.Token);
        //    if (sut.ExecuteTask != null)
        //    {
        //        await sut.ExecuteTask;
        //    }

        //    mockTracker.Verify(t => t.UpdateStatusAsync(command.Key, CommandStatus.Running, null), Times.Once);
        //    mockTracker.Verify(t => t.UpdateStatusAsync(command.Key, CommandStatus.Finished, null), Times.Once);
        //}

        //[Theory]
        //[AutoMoqData]
        //public async Task ExecuteAsync_ShouldUpdateStatusToFailed_WhenHandlerThrowsException(
        //    [Frozen] Mock<ICommandBus<Guid>> mockBus,
        //    [Frozen] Mock<IServiceProvider> mockServiceProvider,
        //    [Frozen] Mock<ICommandTracker<Guid>> mockTracker,
        //    AsyncCommandWorker<Guid> sut,
        //    DummyCommand command)
        //{
        //    using var cts = new CancellationTokenSource();
        //    var envelope = new CommandEnvelope<DummyCommand, Guid>(command);

        //    var mockHandler = new Mock<IAsyncCommandHandler<DummyCommand, Guid>>();
        //    mockHandler
        //        .Setup(h => h.ExecuteAsync(command, It.IsAny<CancellationToken>()))
        //        .ThrowsAsync(new InvalidOperationException("Beklenmeyen hata!"));

        //    mockServiceProvider
        //        .Setup(sp => sp.GetService(typeof(IAsyncCommandHandler<DummyCommand, Guid>)))
        //        .Returns(mockHandler.Object);

        //    mockBus
        //        .Setup(b => b.DequeueAsync(It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(envelope)
        //        .Callback(() => cts.Cancel());

        //    await sut.StartAsync(cts.Token);
        //    if (sut.ExecuteTask != null)
        //    {
        //        await sut.ExecuteTask;
        //    }

        //    mockTracker.Verify(t => t.UpdateStatusAsync(command.Key, CommandStatus.Finished, null), Times.Never);
        //    mockTracker.Verify(t => t.UpdateStatusAsync(
        //        command.Key,
        //        CommandStatus.Error,
        //        It.Is<string>(msg => msg.Contains("Beklenmeyen hata!"))),
        //        Times.Once);
        //}

        //[Theory]
        //[AutoMoqData]
        //public async Task ExecuteAsync_ShouldUpdateStatusToCancelled_WhenOperationIsCanceled(
        //    [Frozen] Mock<ICommandBus<Guid>> mockBus,
        //    [Frozen] Mock<IServiceProvider> mockServiceProvider,
        //    [Frozen] Mock<ICommandTracker<Guid>> mockTracker,
        //    [Frozen] Mock<ICommandCancellationManager<Guid>> mockCancellationManager,
        //    AsyncCommandWorker<Guid> sut,
        //    DummyCommand command)
        //{
        //    using var cts = new CancellationTokenSource();
        //    var envelope = new CommandEnvelope<DummyCommand, Guid>(command);

        //    var mockHandler = new Mock<IAsyncCommandHandler<DummyCommand, Guid>>();
        //    mockHandler
        //        .Setup(h => h.ExecuteAsync(command, It.IsAny<CancellationToken>()))
        //        .ThrowsAsync(new OperationCanceledException());

        //    mockServiceProvider
        //        .Setup(sp => sp.GetService(typeof(IAsyncCommandHandler<DummyCommand, Guid>)))
        //        .Returns(mockHandler.Object);

        //    mockBus
        //        .Setup(b => b.DequeueAsync(It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(envelope)
        //        .Callback(() => cts.Cancel());

        //    await sut.StartAsync(cts.Token);
        //    if (sut.ExecuteTask != null)
        //    {
        //        await sut.ExecuteTask;
        //    }

        //    mockTracker.Verify(t => t.UpdateStatusAsync(command.Key, CommandStatus.Finished, null), Times.Never);
        //    mockTracker.Verify(t => t.UpdateStatusAsync(command.Key, CommandStatus.Cancelled, It.IsAny<string>()), Times.Once);
        //    mockCancellationManager.Verify(m => m.Remove(command.Key), Times.Once);
        //}
using AutoFixture.Xunit2;
using Moq;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace PIWorks.AsyncDispatcher.Core.Tests
{
    public class AsyncCommandWorkerTests
    {
        [Theory]
        [AutoMoqData]
        public async Task ExecuteAsync_ShouldDequeueAndProcessCommand_WhenCalled(
            [Frozen] Mock<ICommandBus<Guid>> mockBus,
            [Frozen] Mock<IServiceProvider> mockServiceProvider,
            [Frozen] Mock<ICommandTracker<Guid>> mockTracker,
            AsyncCommandWorker<Guid> sut,
            DummyCommand command)
        {
            using var cts = new CancellationTokenSource();
            var envelope = new CommandEnvelope<DummyCommand, Guid>(command);

            var mockHandler = new Mock<IAsyncCommandHandler<DummyCommand, Guid>>();
            mockHandler
                .Setup(h => h.ExecuteAsync(command, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IAsyncCommandHandler<DummyCommand, Guid>)))
                .Returns(mockHandler.Object);

            mockBus
                .Setup(b => b.DequeueAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(envelope)
                .Callback(() => cts.Cancel());

            await sut.StartAsync(cts.Token);
            if (sut.ExecuteTask != null)
            {
                await sut.ExecuteTask;
            }

            mockBus.Verify(b => b.DequeueAsync(It.IsAny<CancellationToken>()), Times.Once);
            mockHandler.Verify(h => h.ExecuteAsync(command, It.IsAny<CancellationToken>()), Times.Once);
            mockTracker.Verify(t => t.UpdateStatusAsync(command.Key, CommandStatus.Running, null), Times.Once);
            mockTracker.Verify(t => t.UpdateStatusAsync(command.Key, CommandStatus.Finished, null), Times.Once);
        }

        [Theory]
        [AutoMoqData]
        public async Task ExecuteAsync_ShouldUpdateStatusToRunningBeforeFinished(
            [Frozen] Mock<ICommandBus<Guid>> mockBus,
            [Frozen] Mock<IServiceProvider> mockServiceProvider,
            [Frozen] Mock<ICommandTracker<Guid>> mockTracker,
            AsyncCommandWorker<Guid> sut,
            DummyCommand command)
        {
            using var cts = new CancellationTokenSource();
            var envelope = new CommandEnvelope<DummyCommand, Guid>(command);

            var mockHandler = new Mock<IAsyncCommandHandler<DummyCommand, Guid>>();
            mockHandler
                .Setup(h => h.ExecuteAsync(command, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IAsyncCommandHandler<DummyCommand, Guid>)))
                .Returns(mockHandler.Object);

            mockBus
                .Setup(b => b.DequeueAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(envelope)
                .Callback(() => cts.Cancel());

            var sequence = new MockSequence();

            mockTracker
                .InSequence(sequence)
                .Setup(t => t.UpdateStatusAsync(command.Key, CommandStatus.Running, null))
                .Returns(Task.CompletedTask);

            mockTracker
                .InSequence(sequence)
                .Setup(t => t.UpdateStatusAsync(command.Key, CommandStatus.Finished, null))
                .Returns(Task.CompletedTask);

            await sut.StartAsync(cts.Token);
            if (sut.ExecuteTask != null)
            {
                await sut.ExecuteTask;
            }

            mockTracker.Verify(t => t.UpdateStatusAsync(command.Key, CommandStatus.Running, null), Times.Once);
            mockTracker.Verify(t => t.UpdateStatusAsync(command.Key, CommandStatus.Finished, null), Times.Once);
        }

        [Theory]
        [AutoMoqData]
        public async Task ExecuteAsync_ShouldUpdateStatusToFailed_WhenHandlerThrowsException(
            [Frozen] Mock<ICommandBus<Guid>> mockBus,
            [Frozen] Mock<IServiceProvider> mockServiceProvider,
            [Frozen] Mock<ICommandTracker<Guid>> mockTracker,
            AsyncCommandWorker<Guid> sut,
            DummyCommand command)
        {
            using var cts = new CancellationTokenSource();
            var envelope = new CommandEnvelope<DummyCommand, Guid>(command);

            var mockHandler = new Mock<IAsyncCommandHandler<DummyCommand, Guid>>();
            mockHandler
                .Setup(h => h.ExecuteAsync(command, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Beklenmeyen hata!"));

            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IAsyncCommandHandler<DummyCommand, Guid>)))
                .Returns(mockHandler.Object);

            mockBus
                .Setup(b => b.DequeueAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(envelope)
                .Callback(() => cts.Cancel());

            await sut.StartAsync(cts.Token);
            if (sut.ExecuteTask != null)
            {
                await sut.ExecuteTask;
            }

            mockTracker.Verify(t => t.UpdateStatusAsync(command.Key, CommandStatus.Finished, null), Times.Never);
            mockTracker.Verify(t => t.UpdateStatusAsync(
                command.Key,
                CommandStatus.Error,
                It.Is<string>(msg => msg.Contains("Beklenmeyen hata!"))),
                Times.Once);
        }

        [Theory]
        [AutoMoqData]
        public async Task ExecuteAsync_ShouldUpdateStatusToCancelled_WhenOperationIsCanceled(
            [Frozen] Mock<ICommandBus<Guid>> mockBus,
            [Frozen] Mock<IServiceProvider> mockServiceProvider,
            [Frozen] Mock<ICommandTracker<Guid>> mockTracker,
            [Frozen] Mock<ICommandCancellationManager<Guid>> mockCancellationManager,
            AsyncCommandWorker<Guid> sut,
            DummyCommand command)
        {
            using var cts = new CancellationTokenSource();
            var envelope = new CommandEnvelope<DummyCommand, Guid>(command);

            var mockHandler = new Mock<IAsyncCommandHandler<DummyCommand, Guid>>();
            mockHandler
                .Setup(h => h.ExecuteAsync(command, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            mockServiceProvider
                .Setup(sp => sp.GetService(typeof(IAsyncCommandHandler<DummyCommand, Guid>)))
                .Returns(mockHandler.Object);

            mockBus
                .Setup(b => b.DequeueAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(envelope)
                .Callback(() => cts.Cancel());

            await sut.StartAsync(cts.Token);
            if (sut.ExecuteTask != null)
            {
                await sut.ExecuteTask;
            }

            mockTracker.Verify(t => t.UpdateStatusAsync(command.Key, CommandStatus.Finished, null), Times.Never);
            mockTracker.Verify(t => t.UpdateStatusAsync(command.Key, CommandStatus.Cancelled, It.IsAny<string>()), Times.Once);
            mockCancellationManager.Verify(m => m.Remove(command.Key), Times.Once);
        }
    }
}
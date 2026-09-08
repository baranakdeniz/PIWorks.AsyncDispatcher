using AutoFixture.Xunit2;
using Microsoft.Extensions.Logging;
using Moq;
using PIWorks.AsyncDispatcher.Core.Abstracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace PIWorks.AsyncDispatcher.Core.Tests
{
    public class DummyCommand : IAsyncCommand<Guid>
    {
        public Guid Key { get; set; }
    }

    public class DefaultAsyncCommandDispatcherTests
    {

        // --- ENQUEUEASYNC TESTLERİ ---

        [Theory]
        [AutoMoqData]
        public async Task EnqueueAsync_ShouldInitializeAndEnqueueCommand(
            [Frozen] Mock<ICommandTracker<Guid>> mockTracker,
            [Frozen] Mock<ICommandBus<Guid>> mockBus,
            DefaultAsyncCommandDispatcher<Guid> sut,
            DummyCommand command,
            CancellationToken cancellationToken)
        {
            await sut.EnqueueAsync(command, cancellationToken);

            mockTracker.Verify(t => t.InitializeAsync(command.Key), Times.Once);
            mockBus.Verify(b => b.EnqueueAsync(It.IsAny<CommandEnvelope<DummyCommand, Guid>>(), cancellationToken), Times.Once);
        }

        [Theory]
        [AutoMoqData]
        public async Task EnqueueAsync_ShouldCallInitializeBeforeEnqueue(
            [Frozen] Mock<ICommandTracker<Guid>> mockTracker,
            [Frozen] Mock<ICommandBus<Guid>> mockBus,
            DefaultAsyncCommandDispatcher<Guid> sut,
            DummyCommand command,
            CancellationToken cancellationToken)
        {
            var sequence = new MockSequence();

            mockTracker.InSequence(sequence)
                        .Setup(t => t.InitializeAsync(command.Key))
                        .Returns(Task.CompletedTask);

            mockBus.InSequence(sequence)
                    .Setup(b => b.EnqueueAsync(It.IsAny<CommandEnvelope<DummyCommand, Guid>>(), cancellationToken))
                    .Returns(Task.CompletedTask);

            await sut.EnqueueAsync(command, cancellationToken);

            mockTracker.Verify(t => t.InitializeAsync(command.Key), Times.Once);
            mockBus.Verify(b => b.EnqueueAsync(It.IsAny<CommandEnvelope<DummyCommand, Guid>>(), cancellationToken), Times.Once);
        }

        [Theory]
        [AutoMoqData]
        public async Task EnqueueAsync_ShouldNotEnqueueToBus_WhenInitializeThrowsException(
            [Frozen] Mock<ICommandTracker<Guid>> mockTracker,
            [Frozen] Mock<ICommandBus<Guid>> mockBus,
            DefaultAsyncCommandDispatcher<Guid> sut,
            DummyCommand command)
        {
            mockTracker.Setup(t => t.InitializeAsync(command.Key))
                        .ThrowsAsync(new InvalidOperationException("Database connection failed"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.EnqueueAsync(command));

            mockBus.Verify(b => b.EnqueueAsync(It.IsAny<CommandEnvelope<DummyCommand, Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // --- CANCELASYNC TESTLERİ ---

        [Theory]
        [AutoMoqData]
        public async Task CancelAsync_ShouldThrowKeyNotFoundException_WhenStatusIsNull(
            [Frozen] Mock<ICommandTracker<Guid>> mockTracker,
            DefaultAsyncCommandDispatcher<Guid> sut,
            Guid commandId)
        {
            mockTracker.Setup(t => t.GetStatusAsync(commandId))
                       .ReturnsAsync((CommandStateInfo?)null);

            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.CancelAsync(commandId));
            Assert.Contains(commandId.ToString(), exception.Message);
        }

        [Theory]
        [InlineAutoMoqData(CommandStatus.Finished)]
        [InlineAutoMoqData(CommandStatus.Cancelled)]
        public async Task CancelAsync_ShouldReturnEarly_WhenStatusIsAlreadyFinishedOrCancelled(
            CommandStatus existingStatus,
            [Frozen] Mock<ICommandTracker<Guid>> mockTracker,
            [Frozen] Mock<ICommandEventPublisher<Guid>> mockPublisher,
            DefaultAsyncCommandDispatcher<Guid> sut,
            Guid commandId)
        {
            var existingState = new CommandStateInfo { Status = existingStatus };

            mockTracker.Setup(t => t.GetStatusAsync(commandId))
                       .ReturnsAsync(existingState);

            await sut.CancelAsync(commandId);

            mockTracker.Verify(t => t.UpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<CommandStatus>(), It.IsAny<string>()), Times.Never);
            mockPublisher.Verify(p => p.PublishCancelAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
        [AutoMoqData]
        public async Task CancelAsync_ShouldUpdateStatusAndPublish_WhenCommandIsPendingOrRunning(
            [Frozen] Mock<ICommandTracker<Guid>> mockTracker,
            [Frozen] Mock<ICommandEventPublisher<Guid>> mockPublisher,
            DefaultAsyncCommandDispatcher<Guid> sut,
            Guid commandId)
        {
            var existingState = new CommandStateInfo { Status = CommandStatus.Running };

            mockTracker.Setup(t => t.GetStatusAsync(commandId))
                       .ReturnsAsync(existingState);

            await sut.CancelAsync(commandId);

            mockTracker.Verify(t => t.UpdateStatusAsync(commandId, CommandStatus.Cancelling, null), Times.Once);
            mockPublisher.Verify(p => p.PublishCancelAsync(commandId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
    
//manuel olara kelle yazılmış hali mekaniğini anlamak için
 //public class DummyCommand : IAsyncCommand<Guid>
        //{
        //    public Guid Key { get; set; }
        //}

        //    private readonly Mock<ICommandBus<Guid>> _mockBus;
        //    private readonly Mock<ICommandTracker<Guid>> _mockTracker;
        //    private readonly Mock<ICommandEventPublisher<Guid>> _mockPublisher;
        //    private readonly Mock<ILogger<DefaultAsyncCommandDispatcher<Guid>>> _mockLogger;
        //    private readonly DefaultAsyncCommandDispatcher<Guid> _sut; // System Under Test

        //    public DefaultAsyncCommandDispatcherTests()
        //    {
        //        // Her testten önce dublörleri taptaze hazırlıyoruz
        //        _mockBus = new Mock<ICommandBus<Guid>>();
        //        _mockTracker = new Mock<ICommandTracker<Guid>>();
        //        _mockPublisher = new Mock<ICommandEventPublisher<Guid>>();
        //        _mockLogger = new Mock<ILogger<DefaultAsyncCommandDispatcher<Guid>>>();

        //        // Resepsiyonistimize (Dispatcher) sahte servisleri veriyoruz
        //        _sut = new DefaultAsyncCommandDispatcher<Guid>(
        //            _mockBus.Object,
        //            _mockTracker.Object,
        //            _mockPublisher.Object,
        //            _mockLogger.Object);
        //    }

        //    // --- ENQUEUEASYNC TESTLERİ ---

        //    [Fact]
        //    public async Task EnqueueAsync_ShouldInitializeAndEnqueueCommand()
        //    {
        //        var command = new DummyCommand { Key = Guid.NewGuid() };
        //        var cancellationToken = new CancellationToken();

        //        await _sut.EnqueueAsync(command, cancellationToken);

        //        _mockTracker.Verify(t => t.InitializeAsync(command.Key), Times.Once);
        //        _mockBus.Verify(b => b.EnqueueAsync(It.IsAny<CommandEnvelope<DummyCommand, Guid>>(), cancellationToken), Times.Once);
        //    }

        //    [Fact]
        //    public async Task EnqueueAsync_ShouldCallInitializeBeforeEnqueue()
        //    {
        //        // Sıralama Doğrulaması (Execution Order Test)
        //        var command = new DummyCommand { Key = Guid.NewGuid() };
        //        var cancellationToken = new CancellationToken();
        //        var sequence = new MockSequence();

        //        // 1. Önce Tracker'ın Initialize metodu çalışmalı
        //        _mockTracker.InSequence(sequence)
        //                    .Setup(t => t.InitializeAsync(command.Key))
        //                    .Returns(Task.CompletedTask);

        //        // 2. Sonra Bus'ın Enqueue metodu çalışmalı
        //        _mockBus.InSequence(sequence)
        //                .Setup(b => b.EnqueueAsync(It.IsAny<CommandEnvelope<DummyCommand, Guid>>(), cancellationToken))
        //                .Returns(Task.CompletedTask);

        //        await _sut.EnqueueAsync(command, cancellationToken);

        //        _mockTracker.Verify(t => t.InitializeAsync(command.Key), Times.Once);
        //        _mockBus.Verify(b => b.EnqueueAsync(It.IsAny<CommandEnvelope<DummyCommand, Guid>>(), cancellationToken), Times.Once);
        //    }

        //    [Fact]
        //    public async Task EnqueueAsync_ShouldNotEnqueueToBus_WhenInitializeThrowsException()
        //    {
        //        // Hata Akışı (Exception Path Test)
        //        var command = new DummyCommand { Key = Guid.NewGuid() };

        //        // Tracker'ı sabote edip hata fırlatmasını sağlıyoruz
        //        _mockTracker.Setup(t => t.InitializeAsync(command.Key))
        //                    .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        //        // Hatanın yukarı fırlatıldığını doğruluyoruz
        //        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.EnqueueAsync(command));

        //        // En kritik kontrol: Hata alındığı için Bus KESİNLİKLE çağrılmamış olmalı!
        //        _mockBus.Verify(b => b.EnqueueAsync(It.IsAny<CommandEnvelope<DummyCommand, Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
        //    }

        //    // --- CANCELASYNC TESTLERİ ---

        //    [Fact]
        //    public async Task CancelAsync_ShouldThrowKeyNotFoundException_WhenStatusIsNull()
        //    {
        //        var commandId = Guid.NewGuid();

        //        _mockTracker.Setup(t => t.GetStatusAsync(commandId))
        //                    .ReturnsAsync((CommandStateInfo?)null);

        //        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.CancelAsync(commandId));
        //        Assert.Contains(commandId.ToString(), exception.Message);
        //    }

        //    [Theory]
        //    [InlineData(CommandStatus.Finished)]
        //    [InlineData(CommandStatus.Cancelled)]
        //    public async Task CancelAsync_ShouldReturnEarly_WhenStatusIsAlreadyFinishedOrCancelled(CommandStatus existingStatus)
        //    {
        //        var commandId = Guid.NewGuid();
        //        var existingState = new CommandStateInfo { Status = existingStatus };

        //        _mockTracker.Setup(t => t.GetStatusAsync(commandId))
        //                    .ReturnsAsync(existingState);

        //        await _sut.CancelAsync(commandId);

        //        // Erken dönüş (return) çalıştığı için alttaki metotlar HİÇ ÇAĞRILMAMIŞ olmalı
        //        _mockTracker.Verify(t => t.UpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<CommandStatus>(), It.IsAny<string>()), Times.Never);
        //        _mockPublisher.Verify(p => p.PublishCancelAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        //    }

        //    [Fact]
        //    public async Task CancelAsync_ShouldUpdateStatusAndPublish_WhenCommandIsPendingOrRunning()
        //    {
        //        var commandId = Guid.NewGuid();
        //        var existingState = new CommandStateInfo { Status = CommandStatus.Running };

        //        _mockTracker.Setup(t => t.GetStatusAsync(commandId))
        //                    .ReturnsAsync(existingState);

        //        await _sut.CancelAsync(commandId);

        //        // UpdateStatusAsync doğru statüyle (Cancelling) çağrıldı mı?
        //        _mockTracker.Verify(t => t.UpdateStatusAsync(commandId, CommandStatus.Cancelling, null), Times.Once);

        //        // Event Publisher tetiklendi mi?
        //        _mockPublisher.Verify(p => p.PublishCancelAsync(commandId, It.IsAny<CancellationToken>()), Times.Once);
        //    }
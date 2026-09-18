//using System;
//using AutoFixture.Xunit2;
//using Xunit;
//using System.Collections.Generic;
//using System.Text;
//using PIWorks.AsyncDispatcher.WebApi.Tests;
//using Moq;
//using PIWorks.AsyncDispatcher.Core.Abstracts;
//using PIWorks.AsyncDispatcher.WebApi.Controllers;
//using PIWorks.AsyncDispatcher.WebApi.Commands;
//using Microsoft.AspNetCore.Mvc;
//using PIWorks.AsyncDispatcher.Core;

//namespace PIWorks.AsyncDispatcher.Api.Tests.Controllers
//{
//    public class ReportsControllerTests
//    {
     

//        [Theory]
//        [AutoMoqData]
//        public async Task GenerateReport_ShouldReturnAccepted_WhenDispatchedSuccessfully([Frozen] Mock<IAsyncCommandDispatcher<Guid>> mockDispatcher , [Frozen] Mock<ICommandTracker<Guid>> mockTracker)
//        {
//            var sut = new ReportsController(mockDispatcher.Object, mockTracker.Object);//neden object
//            var commandId = Guid.NewGuid();
//            mockDispatcher.Setup(t =>t.EnqueueAsync(It.IsAny<GenerateReportCommand>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

//            var result = await sut.Generate("somereport",CancellationToken.None);

//           var acceptedResult = Assert.IsType<AcceptedResult>(result);
            
//            mockDispatcher.Verify(d=>d.EnqueueAsync(It.IsAny<GenerateReportCommand>(), It.IsAny<CancellationToken>()),Times.Once);

//        }
//        [Theory]
//        [AutoMoqData]
//        public async Task GetStatus_ShouldReturnOk_WhenStatusExist([Frozen] Mock<IAsyncCommandDispatcher<Guid>> mockDispatcher, [Frozen] Mock<ICommandTracker<Guid>> mockTracker)
//        {
//            var sut = new ReportsController(mockDispatcher.Object, mockTracker.Object);
//            var commandId = Guid.NewGuid();
//            var expectedStateInfo = new CommandStateInfo
//            {
//                Status = CommandStatus.Running,
//                WorkerId = "Worker-1",
//                ErrorMessage = null
//            };
//            mockTracker.Setup(t => t.GetStatusAsync(commandId)).ReturnsAsync(expectedStateInfo);

//            var result= await sut.GetStatusId(commandId);

//            var okObjectResult = Assert.IsType<OkObjectResult>(result);
//            var statusProperty = okObjectResult.Value?
//                .GetType()
//                .GetProperty("Status")?
//                .GetValue(okObjectResult.Value, null);

//            Assert.NotNull(statusProperty);
//            Assert.Equal(expectedStateInfo.Status.ToString(), statusProperty.ToString());

//            mockTracker.Verify(v => v.GetStatusAsync(commandId), Times.Once);
//        }
//        [Theory]
//        [AutoMoqData]
//        public async Task Cancel_ShouldReturnNotFound_WhenCommandDoesNotExist([Frozen] Mock<IAsyncCommandDispatcher<Guid>> mockDispatcher, [Frozen] Mock<ICommandTracker<Guid>> mockTracker)
//        {
//            var sut = new ReportsController(mockDispatcher.Object , mockTracker.Object);
//            var commandId= Guid.NewGuid();
//            mockDispatcher
//        .Setup(d => d.CancelAsync(commandId, It.IsAny<CancellationToken>()))
//        .Throws(new KeyNotFoundException());

//            var result = await sut.Cancel(commandId, CancellationToken.None);

//            mockDispatcher.Verify(v =>v.CancelAsync(commandId,It.IsAny<CancellationToken>()), Times.Once);
//            var cancelObject = Assert.IsType<NotFoundObjectResult>(result);
//        }

//    }
//}

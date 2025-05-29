using System;
using System.Threading.Tasks;
using ContactManagement.Cadastro.API.Controllers;
using ContactManagement.Cadastro.API.Models;
using ContactManagement.Messages.Events;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ContactManagement.Cadastro.Tests.Controllers
{
    public class ContactsControllerTests
    {
        [Fact]
        public async Task Create_ValidRequest_PublishesContactCreatedAndReturnsAccepted()
        {
            // Arrange
            var mockPublish = new Mock<IPublishEndpoint>();
            ContactCreated? publishedEvent = null;
            mockPublish
                .Setup(x => x.Publish(It.IsAny<ContactCreated>(), default))
                .Callback<ContactCreated, System.Threading.CancellationToken>((evt, _) =>
                {
                    publishedEvent = evt;
                })
                .Returns(Task.CompletedTask);

            var controller = new ContactsController(mockPublish.Object);
            var req = new CreateContactRequest("Ana", "ana@ex.com", "999888777", "11");

            // Act
            var result = await controller.Create(req);

            // Assert
            var accepted = Assert.IsType<AcceptedAtActionResult>(result);
            Assert.Equal(nameof(ContactsController.GetById), accepted.ActionName);
            Assert.NotNull(publishedEvent);
            Assert.Equal("Ana", publishedEvent!.Name);
            Assert.Equal("ana@ex.com", publishedEvent.Email);
        }

        [Fact]
        public async Task Create_NullRequest_ReturnsBadRequest_AndDoesNotPublish()
        {
            // Arrange
            var mockPublish = new Mock<IPublishEndpoint>();
            var controller = new ContactsController(mockPublish.Object);

            // Act
            var result = await controller.Create(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            mockPublish.Verify(x => x.Publish(It.IsAny<ContactCreated>(), default), Times.Never);
        }
    }
}

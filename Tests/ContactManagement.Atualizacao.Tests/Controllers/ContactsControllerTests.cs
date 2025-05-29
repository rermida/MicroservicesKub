using System;
using System.Threading.Tasks;
using ContactManagement.Atualizacao.API.Controllers;
using ContactManagement.Atualizacao.API.Models;
using ContactManagement.Messages.Events;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ContactManagement.Atualizacao.Tests.Controllers
{
    public class ContactsControllerTests
    {
        [Fact]
        public async Task Update_ValidRequest_PublishesContactUpdated_AndReturnsNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            var mockPublish = new Mock<IPublishEndpoint>();
            ContactUpdated? publishedEvent = null;
            mockPublish
                .Setup(x => x.Publish(It.IsAny<ContactUpdated>(), default))
                .Callback<ContactUpdated, System.Threading.CancellationToken>((evt, _) =>
                {
                    publishedEvent = evt;
                })
                .Returns(Task.CompletedTask);

            var controller = new ContactManagement.Atualizacao.API.Controllers.ContactsController(mockPublish.Object);
            var req = new UpdateContactRequest("Ana", "ana2@ex.com", "777666555", "11");

            // Act
            var result = await controller.Update(id, req);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.NotNull(publishedEvent);
            Assert.Equal(id, publishedEvent!.Id);
            Assert.Equal("ana2@ex.com", publishedEvent.Email);
        }
    }
}

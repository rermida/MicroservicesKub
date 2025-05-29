using System;
using System.Threading.Tasks;
using ContactManagement.Messages.Events;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using ExclusaoController = ContactManagement.Exclusao.API.Controllers.ContactsController;

namespace ContactManagement.Exclusao.Tests.Controllers
{
    public class ContactsControllerTests
    {
        [Fact]
        public async Task Delete_ValidId_PublishesContactDeleted_AndReturnsNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            var mockPublish = new Mock<IPublishEndpoint>();
            ContactDeleted? publishedEvent = null;
            mockPublish
                .Setup(x => x.Publish(It.IsAny<ContactDeleted>(), default))
                .Callback<ContactDeleted, System.Threading.CancellationToken>((evt, _) =>
                {
                    publishedEvent = evt;
                })
                .Returns(Task.CompletedTask);

            var controller = new ExclusaoController(mockPublish.Object);

            // Act
            var result = await controller.Delete(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            Assert.NotNull(publishedEvent);
            Assert.Equal(id, publishedEvent!.Id);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContactManagement.Consulta.API.Controllers;
using ContactManagement.Consulta.API.Models;
using ContactManagement.Domain.Entities;
using ContactManagement.Domain.Repositories;
using ContactManagement.Messages.Events;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ContactManagement.Consulta.Tests.Controllers
{
    public class ContactsQueryControllerTests
    {
        [Fact]
        public async Task GetAll_ReturnsOk_WithDtos()
        {
            // Arrange
            var contacts = new List<Contact>
            {
                new Contact(Guid.NewGuid(), "X", "x@ex.com", "11111111", "21"),
                new Contact(Guid.NewGuid(), "Y", "y@ex.com", "22222222", "31")
            };
            var mockRepo = new Mock<IContactRepository>();
            mockRepo.Setup(r => r.GetAllContactsAsync()).ReturnsAsync(contacts);

            var mockPublish = new Mock<IPublishEndpoint>();
            var controller = new ContactsQueryController(mockRepo.Object, mockPublish.Object);

            // Act
            var result = await controller.GetAll();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var dtos = Assert.IsAssignableFrom<List<ContactDto>>(ok.Value);
            Assert.Equal(2, dtos.Count);
            Assert.All(dtos, d => Assert.NotEqual(Guid.Empty, d.Id));
            mockPublish.Verify(x => x.Publish(It.IsAny<object>(), default), Times.Never);
        }

        [Fact]
        public async Task GetById_ExistingId_PublishesReadEvent_AndReturnsOk()
        {
            // Arrange
            var id = Guid.NewGuid();
            var contact = new Contact(id, "Z", "z@ex.com", "33333333", "41");
            var mockRepo = new Mock<IContactRepository>();
            mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(contact);

            var readPublished = false;
            var mockPublish = new Mock<IPublishEndpoint>();
            mockPublish
                .Setup(x => x.Publish(It.IsAny<ContactRead>(), default))
                .Callback<ContactRead, System.Threading.CancellationToken>((evt, _) =>
                {
                    readPublished = true;
                    Assert.Equal(id, evt.Id);
                })
                .Returns(Task.CompletedTask);

            var controller = new ContactsQueryController(mockRepo.Object, mockPublish.Object);

            // Act
            var result = await controller.GetById(id);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<ContactDto>(ok.Value);
            Assert.Equal("Z", dto.Name);
            Assert.True(readPublished);
        }

        [Fact]
        public async Task GetById_NonexistentId_ReturnsNotFound_AndDoesNotPublish()
        {
            // Arrange
            var id = Guid.NewGuid();
            var mockRepo = new Mock<IContactRepository>();
            mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Contact?)null);

            var mockPublish = new Mock<IPublishEndpoint>();
            var controller = new ContactsQueryController(mockRepo.Object, mockPublish.Object);

            // Act
            var result = await controller.GetById(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            mockPublish.Verify(x => x.Publish(It.IsAny<ContactRead>(), default), Times.Never);
        }
    }
}

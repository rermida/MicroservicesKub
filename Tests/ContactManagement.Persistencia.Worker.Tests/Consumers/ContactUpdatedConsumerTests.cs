using System;
using System.Threading.Tasks;
using ContactManagement.Domain.Entities;
using ContactManagement.Domain.Repositories;
using ContactManagement.Messages.Events;
using ContactManagement.Persistencia.Worker.Consumers;
using MassTransit;
using Moq;
using Xunit;

namespace ContactManagement.Persistencia.Worker.Tests.Consumers
{
    public class ContactUpdatedConsumerTests
    {
        [Fact]
        public async Task Consume_ExistingContact_CallsUpdateAsync()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existing = new Contact(id, "Old", "old@ex.com", "11111111", "21");
            var @event = new ContactUpdated(id, "New", "new@ex.com", "22222222", "31");

            var mockRepo = new Mock<IContactRepository>();
            mockRepo.Setup(r => r.GetByIdAsync(id))
                    .ReturnsAsync(existing);

            var updateCalled = false;
            mockRepo.Setup(r => r.UpdateAsync(It.Is<Contact>(c => 
                        c.Id == id && c.Name == "New" && c.Email == "new@ex.com")))
                    .Callback(() => updateCalled = true)
                    .Returns(Task.CompletedTask);

            var consumer = new ContactUpdatedConsumer(mockRepo.Object);
            var mockContext = new Mock<ConsumeContext<ContactUpdated>>();
            mockContext.Setup(c => c.Message).Returns(@event);

            // Act
            await consumer.Consume(mockContext.Object);

            // Assert
            Assert.True(updateCalled, "Deveria chamar IContactRepository.UpdateAsync");
        }

        [Fact]
        public async Task Consume_NonexistentContact_DoesNotCallUpdate()
        {
            // Arrange
            var id = Guid.NewGuid();
            var @event = new ContactUpdated(id, "X", "x@ex.com", "33333333", "41");

            var mockRepo = new Mock<IContactRepository>();
            mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Contact?)null);

            var consumer = new ContactUpdatedConsumer(mockRepo.Object);
            var mockContext = new Mock<ConsumeContext<ContactUpdated>>();
            mockContext.Setup(c => c.Message).Returns(@event);

            // Act
            await consumer.Consume(mockContext.Object);

            // Assert
            mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Contact>()), Times.Never);
        }
    }
}

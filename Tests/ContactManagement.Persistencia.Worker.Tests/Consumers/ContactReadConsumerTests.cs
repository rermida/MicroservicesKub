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
    public class ContactReadConsumerTests
    {
        [Fact]
        public async Task Consume_ExistingContact_UpdatesLastReadAtAndCallsUpdate()
        {
            // Arrange
            var id = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;
            var existing = new Contact(id, "N", "n@ex.com", "44444444", "51");
            var @event = new ContactRead(id, now);

            var mockRepo = new Mock<IContactRepository>();
            mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(existing);

            var updateCalled = false;
            mockRepo.Setup(r => r.UpdateAsync(It.Is<Contact>(c => 
                        c.Id == id && c.LastReadAt == now)))
                    .Callback(() => updateCalled = true)
                    .Returns(Task.CompletedTask);

            var consumer = new ContactReadConsumer(mockRepo.Object);
            var mockContext = new Mock<ConsumeContext<ContactRead>>();
            mockContext.Setup(c => c.Message).Returns(@event);

            // Act
            await consumer.Consume(mockContext.Object);

            // Assert
            Assert.True(updateCalled, "Deveria chamar IContactRepository.UpdateAsync com LastReadAt");
        }

        [Fact]
        public async Task Consume_NonexistentContact_DoesNotCallUpdate()
        {
            // Arrange
            var id = Guid.NewGuid();
            var @event = new ContactRead(id);

            var mockRepo = new Mock<IContactRepository>();
            mockRepo.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Contact?)null);

            var consumer = new ContactReadConsumer(mockRepo.Object);
            var mockContext = new Mock<ConsumeContext<ContactRead>>();
            mockContext.Setup(c => c.Message).Returns(@event);

            // Act
            await consumer.Consume(mockContext.Object);

            // Assert
            mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Contact>()), Times.Never);
        }
    }
}

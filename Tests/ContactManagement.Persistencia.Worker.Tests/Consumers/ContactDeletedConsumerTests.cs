using System;
using System.Threading.Tasks;
using ContactManagement.Messages.Events;
using ContactManagement.Persistencia.Worker.Consumers;
using ContactManagement.Domain.Repositories;
using MassTransit;
using Moq;
using Xunit;

namespace ContactManagement.Persistencia.Worker.Tests.Consumers
{
    public class ContactDeletedConsumerTests
    {
        [Fact]
        public async Task Consume_ValidEvent_CallsDeleteAsync()
        {
            // Arrange
            var id = Guid.NewGuid();
            var @event = new ContactDeleted(id);

            var mockRepo = new Mock<IContactRepository>();
            var deleteCalled = false;
            mockRepo.Setup(r => r.DeleteAsync(id))
                    .Callback(() => deleteCalled = true)
                    .Returns(Task.CompletedTask);

            var consumer = new ContactDeletedConsumer(mockRepo.Object);
            var mockContext = new Mock<ConsumeContext<ContactDeleted>>();
            mockContext.Setup(c => c.Message).Returns(@event);

            // Act
            await consumer.Consume(mockContext.Object);

            // Assert
            Assert.True(deleteCalled, "Deveria chamar IContactRepository.DeleteAsync");
        }
    }
}

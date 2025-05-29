using System;
using System.Threading;
using System.Threading.Tasks;
using ContactManagement.Infrastructure.Data;
using ContactManagement.Domain.Entities;
using ContactManagement.Domain.Repositories;
using ContactManagement.Messages.Events;
using ContactManagement.Persistencia.Worker.Consumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ContactManagement.Persistencia.Worker.Tests.Consumers
{
    public class ContactCreatedConsumerTests
    {
        [Fact]
        public async Task Consume_ValidEvent_CallsAddAndSaveChanges()
        {
            // Arrange
            var id = Guid.NewGuid();
            var @event = new ContactCreated(id, "Nome", "email@teste.com", "12345678", "11");

            var mockRepo = new Mock<IContactRepository>();
            var options = new DbContextOptionsBuilder<ContactDbContext>()
                .UseInMemoryDatabase("TestDb1")
                .Options;
            var mockDb = new Mock<ContactDbContext>(options);

            var addCalled = false;
            var saveCalled = false;

            mockRepo.Setup(r => r.AddAsync(It.Is<Contact>(c => c.Id == id)))
                    .Callback(() => addCalled = true)
                    .Returns(Task.CompletedTask);

            mockDb.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()))
                  .Callback(() => saveCalled = true)
                  .ReturnsAsync(1);

            var consumer = new ContactCreatedConsumer(mockDb.Object, mockRepo.Object);
            var mockContext = new Mock<ConsumeContext<ContactCreated>>();
            mockContext.Setup(c => c.Message).Returns(@event);

            // Act
            await consumer.Consume(mockContext.Object);

            // Assert
            Assert.True(addCalled, "Deveria chamar IContactRepository.AddAsync");
            Assert.True(saveCalled, "Deveria chamar DbContext.SaveChangesAsync");
        }
    }
}

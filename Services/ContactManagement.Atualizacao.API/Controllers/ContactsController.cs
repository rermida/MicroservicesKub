using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using ContactManagement.Atualizacao.API.Models;
using ContactManagement.Messages.Events;

namespace ContactManagement.Atualizacao.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactsController : ControllerBase
    {
        private readonly IPublishEndpoint _publish;

        public ContactsController(IPublishEndpoint publish) =>
            _publish = publish;

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateContactRequest req)
        {
            // 1) Construir e publicar evento
            var @event = new ContactUpdated(
                id,
                req.Name,
                req.Email,
                req.Phone,
                req.Ddd
            );
            await _publish.Publish(@event);

            // 2) Retornar 204 NoContent
            return NoContent();
        }
    }
}

using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using ContactManagement.Messages.Events;

namespace ContactManagement.Exclusao.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactsController : ControllerBase
    {
        private readonly IPublishEndpoint _publish;

        public ContactsController(IPublishEndpoint publish) =>
            _publish = publish;

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            // Publicar evento de deleção
            var @event = new ContactDeleted(id);
            await _publish.Publish(@event);

            // Retornar 204 NoContent
            return NoContent();
        }
    }
}

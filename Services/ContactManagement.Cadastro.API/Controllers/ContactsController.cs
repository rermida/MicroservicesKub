using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using ContactManagement.Cadastro.API.Models;
using ContactManagement.Messages.Events;

namespace ContactManagement.Cadastro.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactsController : ControllerBase
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public ContactsController(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        // Cria um novo contato e publica evento ContactCreated no broker.
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateContactRequest request)
        {
            if (request is null)
                return BadRequest("Request body is null.");

            // 1) Gerar novo Id
            var id = Guid.NewGuid();

            // 2) Construir e publicar evento
            var @event = new ContactCreated(
                id,
                request.Name,
                request.Email,
                request.Phone,
                request.Ddd
            );
            await _publishEndpoint.Publish(@event);

            return CreatedAtAction(nameof(GetById), new { id = id }, new { id = id });
        }

        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            return Ok(); // ou NotFound();
        }
    }
}

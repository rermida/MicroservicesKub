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

        /// <summary>
        /// Cria um novo contato e publica evento ContactCreated no broker.
        /// </summary>
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

            // 3) Retornar 202 Accepted com link para GET futuro
            return AcceptedAtAction(
                nameof(GetById),
                new { id },
                new { id }
            );
        }

        /// <summary>
        /// Stub para futuro endpoint de consulta (pode chamar o microsserviço de Consulta).
        /// </summary>
        [HttpGet("{id:guid}")]
        public IActionResult GetById(Guid id)
        {
            // ainda sem implementação de leitura; apenas retorna 202 para demonstração
            return Accepted();
        }
    }
}

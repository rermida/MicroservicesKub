using System;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using ContactManagement.Domain.Repositories;
using ContactManagement.Messages.Events;
using ContactManagement.Consulta.API.Models;

namespace ContactManagement.Consulta.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactsQueryController : ControllerBase
    {
        private readonly IContactRepository _repo;
        private readonly IPublishEndpoint    _publish;

        public ContactsQueryController(IContactRepository repo, IPublishEndpoint publish)
        {
            _repo    = repo;
            _publish = publish;
        }

        /// <summary>
        /// Retorna todos os contatos.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var contacts = await _repo.GetAllContactsAsync();
            var dtos = contacts
                .Select(c => new ContactDto(c.Id, c.Name, c.Email, c.Phone, c.Ddd))
                .ToList();

            return Ok(dtos);
        }

        /// <summary>
        /// Retorna um contato pelo ID e publica evento ContactRead.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var contact = await _repo.GetByIdAsync(id);
            if (contact is null)
                return NotFound();

            // Publicar evento de leitura
            var readEvent = new ContactRead(id);
            await _publish.Publish(readEvent);

            var dto = new ContactDto(
                contact.Id,
                contact.Name,
                contact.Email,
                contact.Phone,
                contact.Ddd
            );
            return Ok(dto);
        }
    }
}

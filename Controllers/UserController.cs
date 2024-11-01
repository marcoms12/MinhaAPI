using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinhaAPI.Data;
using MinhaAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace MinhaAPI.Controllers
{   //ROTAS
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound( new {message = "Usu�rio n�o encontrado!" });
            }

            return user;
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(UserDto userDto)
        {
            if (_context.Users.Any(u => u.Email == userDto.Email))
            {
                return BadRequest(new { message = "E-mail j� est� em uso." });
            }

            using var hmac = new HMACSHA512();

            var user = new User
            {
                Name = userDto.Name,
                Email = userDto.Email,
                Password = userDto.Password,
                PasswordSalt = Convert.ToBase64String(hmac.Key),  // Gerando o salt
                PasswordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(userDto.Password)))  // Gerando o hash da senha
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }


        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, UserDto userDto)
        {
            // Procura o usu�rio pelo ID fornecido na URL
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new {message = "Este usu�rio n�o existe!"});
            }

            // Atualiza as propriedades do usu�rio com os dados do DTO
            user.Name = userDto.Name;
            user.Email = userDto.Email;
            user.Password = userDto.Password;

            // Marca a entidade como modificada
            _context.Entry(user).State = EntityState.Modified;

            try
            {
                // Salva as altera��es no banco de dados
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            // Retorna uma resposta sem conte�do ao final da opera��o
            return NoContent();
        }


        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound("Usu�rio n�o encontrado para a exclus�o!");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new {message = "Usu�rio deletado!"});
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}

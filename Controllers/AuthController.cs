using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using MinhaAPI.Models;
using MinhaAPI.Data;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using MailKit.Net.Smtp;
using MimeKit;

//Rotas
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserDto request)
    {
        // Gerar hash da senha
        using var hmac = new HMACSHA512();
        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(request.Password)))
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "User registered successfully" });
    }

    // Validação de Nome e Email
    private void ValidateUserClaims(User user)
    {
        if (string.IsNullOrWhiteSpace(user.Name))
        {
            throw new ArgumentException("O nome do usuário não pode estar vazio.");
        }

        if (!IsValidEmail(user.Email))
        {
            throw new ArgumentException("O e-mail fornecido não é válido.");
        }
    }

    private bool IsValidEmail(string email)
    {
        var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, emailPattern);
    }

    //Geração de Token JWT
    private string CreateToken(User user)
    {
        // Validações antes de criar claims
        ValidateUserClaims(user);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your_jwt_secret_key_here"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Método de Login
    [HttpPost("login")]
    public async Task<IActionResult> Login(UserDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email); // Alterado para Email

        if (user == null) return BadRequest("User not found");

        using var hmac = new HMACSHA512();
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(request.Password));

        if (Convert.ToBase64String(computedHash) != user.PasswordHash)
            return BadRequest("Incorrect password");

        var token = CreateToken(user);

        return Ok(new { token });
    }

    // Método de Recuperação de Senha
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(string email)
    {   // Verifica se o usuário existe e valida a requisição de acordo com a resposta
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return BadRequest("Usuário não encontrado!");
        // Gera um token de redefinição de senha
        user.ResetPasswordToken = Guid.NewGuid().ToString();
        user.ResetPasswordTokenExpires = DateTime.Now.AddHours(1);
        await _context.SaveChangesAsync();

        // Enviar email com token (usando MailKit, por exemplo)
        await SendEmail(user.Email, user.ResetPasswordToken);

        return Ok("Verifique o seu Email e acesse o link de reset.");
    }

    private async Task SendEmail(string toEmail, string resetToken)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("No Reply", "noreply@suadomain.com"));
        message.To.Add(new MailboxAddress("", toEmail));
        message.Subject = "Reset Your Password";

        message.Body = new TextPart("plain")
        {
            Text = $"Use o seguinte token para redefinir sua senha: {resetToken}"
        };

        using (var client = new SmtpClient())
        {
            // Conecte-se ao servidor SMTP
            await client.ConnectAsync("sandbox.smtp.mailtrap.io", 587, false);

            // Se o servidor SMTP requer autenticação, use suas credenciais
            await client.AuthenticateAsync("73ae80b0f893d1", "03121acaf77695");

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }

}

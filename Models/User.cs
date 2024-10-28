using System;
using System.ComponentModel.DataAnnotations;


namespace MinhaAPI.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(255)]
        public string Email { get; set; }

        [Required]
        [MinLength(8)]
        [MaxLength(255)]
        public string Password { get; set; }

        public DateTime? CreateTime { get; set; } = DateTime.Now;

        public string? PasswordHash {get; set; }

        public string? ResetPasswordToken { get; set; }

        public DateTime? ResetPasswordTokenExpires { get; set; }



        public User()
        {
        Name = string.Empty;
        Email = string.Empty;
        Password = string.Empty;
        PasswordHash = string.Empty;
        ResetPasswordToken = string.Empty;

            
        }
    }
}



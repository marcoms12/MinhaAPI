public class UserDto
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }

    public UserDto()
    {
        Name = string.Empty;
        Email = string.Empty;
        Password = string.Empty;
    }
}

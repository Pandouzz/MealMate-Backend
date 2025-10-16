public class User
{
    public int UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PwHash { get; set; }

    public string? ImageUrl { get; set; }

    public string Salt { get; set; }
}
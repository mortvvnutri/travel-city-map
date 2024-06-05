namespace TSM.Models
{
    public class RegisterUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty; // Убедитесь, что это свойство существует
        public List<int>? PreferredCats { get; set; } // Убедитесь, что это свойство существует
    }
}

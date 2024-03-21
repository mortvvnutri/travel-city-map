namespace backend.Utils
{
    public class RegisterUserRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirebaseToken { get; set; } = string.Empty;
    }
}
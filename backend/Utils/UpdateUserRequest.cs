namespace backend.Utils
{
    public class UpdateUserRequest
    {
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public List<int>? PreferredCats { get; set; }
        public int? DefCustomPlace { get; set; }
        public string Meta { get; set; }
    }
}

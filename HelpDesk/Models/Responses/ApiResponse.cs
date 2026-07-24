namespace HelpDesk.Models.Responses
{
    public class ApiResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
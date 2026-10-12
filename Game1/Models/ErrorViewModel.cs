namespace Game1.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        /// <summary>HTTP status shown on the page (404, 403, 500, …).</summary>
        public int StatusCode { get; set; } = 500;

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId) && StatusCode >= 500;
    }
}

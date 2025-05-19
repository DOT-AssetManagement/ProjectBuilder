namespace ProjectBuilder.Web.MVC.Models
{
    public class ErrorViewModel
    {
        public string Title { get; set; }
        public ErrorType Type { get; set; }
        public string SubTitle { get; set; }
        public string? RequestId { get; set; }
        public bool HasBackBtn { get; set; }
        public string BtnUrl { get; set; }
        public string BtnContent { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string ErrorMessage { get; set; }
    }
    public enum ErrorType
    {
        Unknown = 0,
        BadRequest,
        Info,
        Error
    }
}
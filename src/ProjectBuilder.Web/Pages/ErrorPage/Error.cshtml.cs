using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ProjectBuilder.Helpers.Exceptions;

namespace ProjectBuilder.Web.Pages.ErrorPage
{
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        public string? ErrorMessage { get; set; }
        public string? ErrorDetails { get; set; }

        public void OnGet() => ReadError();
        public void OnPost() => ReadError();
        private void ReadError()
        {
            ErrorMessage = HttpContext.Session.GetString("ErrorMessage");
            ErrorDetails = HttpContext.Session.GetString("ErrorDetails");

            if (string.IsNullOrWhiteSpace(ErrorMessage))
            {
                ErrorMessage = "Something went wrong.";
            }

            HttpContext.Session.Remove("ErrorMessage");
            HttpContext.Session.Remove("ErrorDetails");
        }
    }
}
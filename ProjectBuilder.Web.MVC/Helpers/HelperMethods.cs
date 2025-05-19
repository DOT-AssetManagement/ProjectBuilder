using Microsoft.Identity.Web;
using ProjectBuilder.Core;
using System.Security.Claims;

namespace ProjectBuilder.Web.MVC
{

    internal static class HelperMethods
    {
        internal static UserModel ToUserModel(this ClaimsPrincipal userClaims,bool isUserActive = true)
        {
            var userEmail = userClaims.FindFirstValue("emails");
            var userId = userClaims.GetObjectId();
            var userName = userClaims.FindFirstValue("name");
            return new UserModel { Email = userEmail, B2CUserId = Guid.Parse(userId), Name = userName, IsActive = isUserActive };
        }
        internal static async Task<string> SaveUploadedFile(this IFormFile formFile)
        {
            using MemoryStream stream = new MemoryStream();
            await formFile.CopyToAsync(stream);
            var filename = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            using var excelfile = File.Create(filename);
            await excelfile.WriteAsync(stream.ToArray());
            return filename;
        }
    }
}

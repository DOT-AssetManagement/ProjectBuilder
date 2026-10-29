using Microsoft.AspNetCore.Http;
using ProjectBuilder.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.Helpers
{
    public static class HelperMethods
    {
        //internal static UserModel ToUserModel(this ClaimsPrincipal userClaims, bool isUserActive = true)
        //{
        //    var userEmail = userClaims.FindFirstValue("emails");
        //    var userId = userClaims.GetObjectId();
        //    var userName = userClaims.FindFirstValue("name");
        //    return new UserModel { Email = userEmail, B2CUserId = Guid.Parse(userId), Name = userName, IsActive = isUserActive };
        //}
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

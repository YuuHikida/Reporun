using Microsoft.AspNetCore.Html;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
//using Microsoft.DotNet.Scaffolding.Shared.Project;
using ReportSystem.ViewModels;

namespace ReportSystem.Chartlogic
{
    public static class HtmlExtensions
    {
        public static string GetUserHtml(ApplicationUserChart auc, string RoleName)
        {
            var user = auc.Users.FirstOrDefault(x => x.NewRole == RoleName);
            if (user != null)
            {
                return ($"{RoleName}:{user.LastName} {user.FirstName}");
                //return new HtmlString($"{RoleName}:{user.LastName} {user.FirstName}</dt>").ToString();
            }
            return $"<p>No Date</p>";
            //return null;
        }

    }
}

using ReportSystem.Models;

namespace ReportSystem.ViewModels
{
    public class ApplicationUserChart
    {
        /*↓こっちが今閲覧している人のroleを読み込む用(ページごとで表示させる内容を変える為
        */
        public ApplicationUser? User { get; set; }

        public List<ApplicationUser>? Users { get; set; }

    }
}

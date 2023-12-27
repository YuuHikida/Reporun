using ReportSystem.Models;

namespace ReportSystem.ViewModels
{
    public class UserIndex
    {
        public ApplicationUser User { get; set; }

        public List<ApplicationUser> Users { get; set; }

        public List<Project> Projects { get; set; }

        ////以下スライド未読記事表示の為に追加
       // public List<Feedback>? feedbacks { get; set; }

        public List<Report>? report { get; set; }

    }

}

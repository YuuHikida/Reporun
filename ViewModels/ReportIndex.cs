using ReportSystem.Models;

namespace ReportSystem.ViewModels
{
    public class ReportIndex
    {
        // 単数==========================================
        public ApplicationUser User { get; set; }

        // 復数==========================================
        public List<Report> Reports { get; set;} 

        public List<Attendance> Attendances { get; set;}

        public List<Feedback>? Feedbacks {  get; set;}

        public List<Notification> Notifications { get; set; }
    }
}

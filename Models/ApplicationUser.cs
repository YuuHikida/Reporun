using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace ReportSystem.Models
{
    public class ApplicationUser : IdentityUser
    {

        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        public string? Role {  get; set; }
        //Icon画像埋め込み用の新たな変数
        //public string? icon { get; set; }
        [Column(TypeName = "varbinary(MAX)")]
        public byte[]? Icon { get; set; }
        //新たに追加の役割
        //public string NewRole { get; set; }

        //public List<Project> Projects { get; set; } = new List<Project>();

        //public List<Project>? Projects { get; set; }

        public ICollection<UserProject>? UserProjects { get; set; }

        public ICollection<Attendance> Attendances {  get; set; }
        
        public ICollection<Report> Reports {  get; set; }

        public ICollection<Todo> Todos { get; set; }

        public ICollection<Feedback> Feedbacks { get; set; }

        }
}

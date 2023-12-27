
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReportSystem.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("UesrId")]
        public string UserId { get; set; }

        public string DateReport { get; set; }

        public bool IsSent { get; set; }

        public bool IsDisplayed { get; set; }
    }

}

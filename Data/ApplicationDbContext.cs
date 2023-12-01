using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ReportSystem.Models;

namespace ReportSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ReportSystem.Models.Report> report { get; set; } = default;

        public DbSet<ReportSystem.Models.ApplicationUser>? user { get; set; }
        public DbSet<ReportSystem.Models.Attendance>? attendance { get; set; }
        public DbSet<ReportSystem.Models.Todo>? todo { get; set; }
        public DbSet<ReportSystem.Models.Feedback>? feedback { get; set; }

        public DbSet<ReportSystem.Models.Project>? project { get; set; }

        public DbSet<ReportSystem.Models.UserProject>? userproject { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //中間テーブルを用意した場合の設定

            builder.Entity<UserProject>()
                .HasKey(up => new { up.UserId, up.ProjectId });

            builder.Entity<UserProject>()
                .HasOne(up => up.User)
                .WithMany(u => u.UserProjects)
                .HasForeignKey(up => up.UserId);

            builder.Entity<UserProject>()
                .HasOne(up => up.Project)
                .WithMany(p => p.UserProjects)
                .HasForeignKey(up => up.ProjectId);

            //中間テーブルがない場合の設定

            //builder.Entity<Project>()
            //    .HasMany(x => x.Users)
            //    .WithMany(x => x.Projects);

        }
    }
}
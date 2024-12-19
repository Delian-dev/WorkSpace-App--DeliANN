using Proiect_DAW_DeliANN.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using static Proiect_DAW_DeliANN.Models.ApplicationUserWorkspaces;

namespace Proiect_DAW_DeliANN.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Reaction> Reactions { get; set; }
        public DbSet<ApplicationUserWorkspace> ApplicationUserWorkspaces { get; set; }
        public DbSet<Profile> Profiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // definirea relatiei many-to-many dintre ApplicationUser si Workspace

            base.OnModelCreating(modelBuilder);

            // definire primary key compus
            modelBuilder.Entity<ApplicationUserWorkspace>()
                .HasKey(ab => new { ab.Id, ab.UserId, ab.WorkspaceId });


            // definire relatii cu modelele Workspaces si ApplicationUser (FK)

            modelBuilder.Entity<ApplicationUserWorkspace>()
                .HasOne(ab => ab.User)
                .WithMany(ab => ab.ApplicationUserWorkspaces)
                .HasForeignKey(ab => ab.UserId);

            modelBuilder.Entity<ApplicationUserWorkspace>()
                .HasOne(ab => ab.Workspace)
                .WithMany(ab => ab.ApplicationUserWorkspaces)
                .HasForeignKey(ab => ab.WorkspaceId);


            //definirea relatiei 1-1 dintr user si profil
            modelBuilder.Entity<ApplicationUser>()
               .HasOne(a => a.Profile)
               .WithOne(p => p.User)
               .HasForeignKey<Profile>(p => p.UserId);
        }
    }
}

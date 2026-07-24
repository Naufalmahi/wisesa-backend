using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HelpDesk.Models.Entities;
using HelpDesk.Models.Enums;

namespace HelpDesk.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Ticket> Tickets { get; set; }

        public DbSet<SlaPolicy> SlaPolicies { get; set; }

        public DbSet<TicketSla> TicketSlas { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<ActivityLog> ActivityLogs { get; set; }

        public DbSet<Escalation> Escalations { get; set; }
        public DbSet<Attachment> Attachments { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(b =>
            {
                b.ToTable("Users");
                b.Property(u => u.Name).HasMaxLength(100).IsRequired();
                b.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
                b.HasIndex(u => u.Email).IsUnique();
            });

            builder.Entity<IdentityRole<Guid>>(b => b.ToTable("Roles"));
            builder.Entity<IdentityUserRole<Guid>>(b => b.ToTable("UserRoles"));
            builder.Entity<IdentityUserClaim<Guid>>(b => b.ToTable("UserClaims"));
            builder.Entity<IdentityUserLogin<Guid>>(b => b.ToTable("UserLogins"));
            builder.Entity<IdentityUserToken<Guid>>(b => b.ToTable("UserTokens"));
            builder.Entity<IdentityRoleClaim<Guid>>(b => b.ToTable("RoleClaims"));

            builder.Entity<Category>(b =>
            {
                b.ToTable("Categories");
                b.HasKey(c => c.Id);
                b.Property(c => c.Name).HasMaxLength(100).IsRequired();
                b.Property(c => c.Description).HasMaxLength(500);
                b.HasIndex(c => c.Name).IsUnique();
            });

            builder.Entity<Ticket>(b =>
            {
                b.ToTable("Tickets");
                b.HasKey(t => t.Id);

                b.Property(t => t.TicketNumber).HasMaxLength(20).IsRequired(false);
                b.HasIndex(t => t.TicketNumber).IsUnique();

                b.Property(t => t.Title).HasMaxLength(200).IsRequired();
                b.Property(t => t.Description).IsRequired();
                b.Property(t => t.AffectedUser).HasMaxLength(150);
                b.Property(t => t.RelatedTicketId).IsRequired(false);

                b.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
                b.Property(t => t.Priority).HasConversion<string>().HasMaxLength(20);

                b.HasIndex(t => t.Status);
                b.HasIndex(t => t.Priority);
                b.HasIndex(t => t.CreatedAt);

                b.HasOne(t => t.Creator)
                    .WithMany(u => u.CreatedTickets)
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(t => t.Assignee)
                    .WithMany(u => u.AssignedTickets)
                    .HasForeignKey(t => t.AssignedToId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(t => t.Category)
                    .WithMany(c => c.Tickets)
                    .HasForeignKey(t => t.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<SlaPolicy>(b =>
            {
                b.ToTable("SlaPolicies");
                b.HasKey(s => s.Id);
                b.Property(s => s.Priority).HasConversion<string>().HasMaxLength(20);

                b.HasIndex(s => s.Priority).IsUnique();
            });

            builder.Entity<TicketSla>(b =>
            {
                b.ToTable("TicketSlas");
                b.HasKey(ts => ts.Id);

                b.HasOne(ts => ts.Ticket)
                    .WithMany(t => t.TicketSlas)
                    .HasForeignKey(ts => ts.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(ts => ts.Technician)
                    .WithMany(u => u.TicketSlas)
                    .HasForeignKey(ts => ts.TechnicianId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(ts => ts.SlaPolicy)
                    .WithMany(sp => sp.TicketSlas)
                    .HasForeignKey(ts => ts.SlaPolicyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Comment>(b =>
            {
                b.ToTable("Comments");
                b.HasKey(c => c.Id);
                b.Property(c => c.Message).IsRequired();
                b.Property(c => c.Type).HasConversion<string>().HasMaxLength(20);

                b.HasQueryFilter(c => c.DeletedAt == null);

                b.HasOne(c => c.Ticket)
                    .WithMany(t => t.Comments)
                    .HasForeignKey(c => c.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(c => c.User)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Notification>(b =>
            {
                b.ToTable("Notifications");
                b.HasKey(n => n.Id);
                b.Property(n => n.Message).HasMaxLength(500).IsRequired();
                b.Property(n => n.Type).HasConversion<string>().HasMaxLength(30);

                b.HasIndex(n => new { n.UserId, n.IsRead });

                b.HasOne(n => n.User)
                    .WithMany(u => u.Notifications)
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(n => n.Ticket)
                    .WithMany(t => t.Notifications)
                    .HasForeignKey(n => n.TicketId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ActivityLog>(b =>
            {
                b.ToTable("ActivityLogs");
                b.HasKey(a => a.Id);
                b.Property(a => a.Action).HasMaxLength(100).IsRequired();
                b.Property(a => a.OldValue).HasMaxLength(500);
                b.Property(a => a.NewValue).HasMaxLength(500);

                b.HasIndex(a => a.TicketId);
                b.HasIndex(a => a.CreatedAt);

                b.HasOne(a => a.Ticket)
                    .WithMany(t => t.ActivityLogs)
                    .HasForeignKey(a => a.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(a => a.User)
                    .WithMany(u => u.ActivityLogs)
                    .HasForeignKey(a => a.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            var lowPolicyId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567801");
            var medPolicyId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567802");
            var highPolicyId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567803");
            var critPolicyId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567804");

            builder.Entity<SlaPolicy>().HasData(
                new SlaPolicy { Id = lowPolicyId, Priority = TicketPriority.Low, ResponseMinutes = 180, ResolveMinutes = 1440 },
                new SlaPolicy { Id = medPolicyId, Priority = TicketPriority.Medium, ResponseMinutes = 120, ResolveMinutes = 480 },
                new SlaPolicy { Id = highPolicyId, Priority = TicketPriority.High, ResponseMinutes = 60, ResolveMinutes = 240 },
                new SlaPolicy { Id = critPolicyId, Priority = TicketPriority.Critical, ResponseMinutes = 30, ResolveMinutes = 120 }
            );

            builder.Entity<Category>().HasData(
                new Category { Id = Guid.Parse("b1b2c3d4-e5f6-7890-abcd-ef1234567801"), Name = "Network", Description = "Masalah jaringan, konektivitas, VPN, dan internet" },
                new Category { Id = Guid.Parse("b1b2c3d4-e5f6-7890-abcd-ef1234567802"), Name = "Hardware", Description = "Masalah perangkat keras, printer, monitor, dan komputer" },
                new Category { Id = Guid.Parse("b1b2c3d4-e5f6-7890-abcd-ef1234567803"), Name = "Software", Description = "Masalah aplikasi, instalasi, lisensi, dan update" },
                new Category { Id = Guid.Parse("b1b2c3d4-e5f6-7890-abcd-ef1234567804"), Name = "Security", Description = "Masalah keamanan, akses, password, dan virus" },
                new Category { Id = Guid.Parse("b1b2c3d4-e5f6-7890-abcd-ef1234567805"), Name = "General", Description = "Permintaan umum dan pertanyaan lainnya" }
            );

            builder.Entity<Escalation>(b =>
            {
                b.ToTable("Escalations");
                b.HasKey(e => e.Id);
                b.Property(e => e.Reason).HasMaxLength(500).IsRequired();
                b.Property(e => e.FromLevel).HasConversion<string>().HasMaxLength(10);
                b.Property(e => e.ToLevel).HasConversion<string>().HasMaxLength(10);
                b.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);

                b.HasOne(e => e.Ticket)
                    .WithMany(t => t.Escalations)
                    .HasForeignKey(e => e.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(e => e.FromUser)
                    .WithMany()
                    .HasForeignKey(e => e.FromUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(e => e.ToUser)
                    .WithMany()
                    .HasForeignKey(e => e.ToUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Attachment>(b =>
            {
                b.ToTable("Attachments");
                b.HasKey(a => a.Id);
                b.Property(a => a.FileName).HasMaxLength(255).IsRequired();
                b.Property(a => a.StoredFileName).HasMaxLength(255).IsRequired();
                b.Property(a => a.ContentType).HasMaxLength(100).IsRequired();

                b.HasOne(a => a.Ticket)
                    .WithMany(t => t.Attachments)
                    .HasForeignKey(a => a.TicketId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(a => a.UploadedBy)
                    .WithMany()
                    .HasForeignKey(a => a.UploadedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ApplyTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                var now = DateTime.UtcNow;

                var updatedAtProp = entry.Entity.GetType().GetProperty("UpdatedAt");
                if (updatedAtProp != null && updatedAtProp.PropertyType == typeof(DateTime))
                {
                    updatedAtProp.SetValue(entry.Entity, now);
                }

                if (entry.State == EntityState.Added)
                {
                    var createdAtProp = entry.Entity.GetType().GetProperty("CreatedAt");
                    if (createdAtProp != null && createdAtProp.PropertyType == typeof(DateTime))
                    {
                        createdAtProp.SetValue(entry.Entity, now);
                    }
                }
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            ApplyTimestamps();
            return base.SaveChanges();
        }
    }
}
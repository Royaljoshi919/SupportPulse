using Microsoft.EntityFrameworkCore;
using SupportPulse.Api.Models;

namespace SupportPulse.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext() { }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketFile> TicketFiles => Set<TicketFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Role)
                  .HasConversion<string>();
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
           modelBuilder.Entity<Ticket>(entity =>
{
    entity.ToTable("tickets");

    entity.HasKey(e => e.Id);

    entity.Property(e => e.UserId)
          .HasColumnName("user_id")
          .IsRequired();

    entity.Property(e => e.Subject)
          .HasColumnName("subject")
          .IsRequired();

    entity.Property(e => e.Description)
          .HasColumnName("description")
          .IsRequired();

    entity.Property(e => e.Status)
          .HasColumnName("status")
          .HasConversion<string>();

    entity.Property(e => e.Priority)
          .HasColumnName("priority")
          .HasConversion<string>();

    entity.Property(e => e.CreatedAt)
          .HasColumnName("created_at");

    entity.HasMany(e => e.Files)
          .WithOne(e => e.Ticket)
          .HasForeignKey(e => e.TicketId);
});
        });

      modelBuilder.Entity<TicketFile>(entity =>
{
    entity.ToTable("ticket_files");

    entity.HasKey(e => e.Id);

    entity.Property(e => e.Id)
        .HasColumnName("id");

    entity.Property(e => e.TicketId)
        .HasColumnName("ticket_id");

    entity.Property(e => e.FilePath)
        .HasColumnName("file_path");

    entity.Property(e => e.FileType)
        .HasColumnName("file_type")
        .HasConversion<string>();

    entity.HasOne(e => e.Ticket)
        .WithMany(e => e.Files)
        .HasForeignKey(e => e.TicketId);
});
    }
}
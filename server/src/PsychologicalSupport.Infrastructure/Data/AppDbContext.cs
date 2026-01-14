using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PsychologicalSupport.Domain.Entities;

namespace PsychologicalSupport.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public DbSet<Psychologist> Psychologists => Set<Psychologist>();
    public DbSet<Specialization> Specializations => Set<Specialization>();
    public DbSet<PsychologistSpecialization> PsychologistSpecializations => Set<PsychologistSpecialization>();
    public DbSet<Availability> Availabilities => Set<Availability>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<QuestionnaireResponse> QuestionnaireResponses => Set<QuestionnaireResponse>();
    public DbSet<Article> Articles => Set<Article>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ApplicationUser
        builder.Entity<ApplicationUser>(e =>
        {
            e.Property(u => u.Language).HasMaxLength(10).HasDefaultValue("ru");
            e.Property(u => u.FirstName).HasMaxLength(100);
            e.Property(u => u.LastName).HasMaxLength(100);
        });

        // Psychologist
        builder.Entity<Psychologist>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasOne(p => p.User)
                .WithOne(u => u.Psychologist)
                .HasForeignKey<Psychologist>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.Property(p => p.Languages).HasMaxLength(50).HasDefaultValue("ru");
            e.Property(p => p.WorkFormats).HasMaxLength(50).HasDefaultValue("online");
            e.Property(p => p.PricePerSession).HasPrecision(10, 2);
            e.Property(p => p.Education).HasMaxLength(500);
            e.Property(p => p.ApproachDescription).HasMaxLength(2000);
            e.Property(p => p.MeetingLink).HasMaxLength(500);
            e.Property(p => p.PhotoPath).HasMaxLength(500);
        });

        // Specialization
        builder.Entity<Specialization>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Key).HasMaxLength(50).IsRequired();
            e.Property(s => s.NameRu).HasMaxLength(100).IsRequired();
            e.Property(s => s.NameTj).HasMaxLength(100).IsRequired();
            e.HasIndex(s => s.Key).IsUnique();
        });

        // PsychologistSpecialization (junction)
        builder.Entity<PsychologistSpecialization>(e =>
        {
            e.HasKey(ps => new { ps.PsychologistId, ps.SpecializationId });

            e.HasOne(ps => ps.Psychologist)
                .WithMany(p => p.Specializations)
                .HasForeignKey(ps => ps.PsychologistId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(ps => ps.Specialization)
                .WithMany(s => s.Psychologists)
                .HasForeignKey(ps => ps.SpecializationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Availability
        builder.Entity<Availability>(e =>
        {
            e.HasKey(a => a.Id);
            e.HasOne(a => a.Psychologist)
                .WithMany(p => p.Availabilities)
                .HasForeignKey(a => a.PsychologistId)
                .OnDelete(DeleteBehavior.Cascade);

            e.Property(a => a.SlotDurationMinutes).HasDefaultValue(60);
        });

        // Session
        builder.Entity<Session>(e =>
        {
            e.HasKey(s => s.Id);

            e.HasOne(s => s.Client)
                .WithMany(u => u.ClientSessions)
                .HasForeignKey(s => s.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(s => s.Psychologist)
                .WithMany(p => p.Sessions)
                .HasForeignKey(s => s.PsychologistId)
                .OnDelete(DeleteBehavior.Restrict);

            e.Property(s => s.MeetingLink).HasMaxLength(500);
            e.Property(s => s.Notes).HasMaxLength(2000);
        });

        // QuestionnaireResponse
        builder.Entity<QuestionnaireResponse>(e =>
        {
            e.HasKey(q => q.Id);

            e.HasOne(q => q.User)
                .WithMany(u => u.QuestionnaireResponses)
                .HasForeignKey(q => q.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            e.Property(q => q.Gender).HasMaxLength(20).IsRequired();
            e.Property(q => q.PreferredLanguage).HasMaxLength(10).IsRequired();
            e.Property(q => q.MainIssue).HasMaxLength(500).IsRequired();
            e.Property(q => q.UrgencyLevel).HasMaxLength(20).IsRequired();
            e.Property(q => q.FormatPreference).HasMaxLength(20).IsRequired();
            e.Property(q => q.AdditionalInfo).HasMaxLength(2000);
            e.Property(q => q.GuestSessionId).HasMaxLength(100);
        });

        // Article
        builder.Entity<Article>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.TitleRu).HasMaxLength(200).IsRequired();
            e.Property(a => a.TitleTj).HasMaxLength(200).IsRequired();
            e.Property(a => a.ImagePath).HasMaxLength(500);
        });

        // Seed specializations
        builder.Entity<Specialization>().HasData(
            new Specialization { Id = 1, Key = "anxiety", NameRu = "Тревожность", NameTj = "Изтироб" },
            new Specialization { Id = 2, Key = "depression", NameRu = "Депрессия", NameTj = "Афсурдагӣ" },
            new Specialization { Id = 3, Key = "burnout", NameRu = "Выгорание", NameTj = "Хастагӣ" },
            new Specialization { Id = 4, Key = "relationships", NameRu = "Отношения", NameTj = "Муносибатҳо" },
            new Specialization { Id = 5, Key = "postpartum", NameRu = "Послеродовая депрессия", NameTj = "Афсурдагии баъд аз таваллуд" },
            new Specialization { Id = 6, Key = "stress", NameRu = "Стресс", NameTj = "Стресс" },
            new Specialization { Id = 7, Key = "self_esteem", NameRu = "Самооценка", NameTj = "Худбаҳодиҳӣ" },
            new Specialization { Id = 8, Key = "grief", NameRu = "Горе и утрата", NameTj = "Ғам ва талафот" },
            new Specialization { Id = 9, Key = "trauma", NameRu = "Травма", NameTj = "Садама" },
            new Specialization { Id = 10, Key = "family", NameRu = "Семейные проблемы", NameTj = "Мушкилоти оилавӣ" }
        );

        // Seed roles
        builder.Entity<IdentityRole<Guid>>().HasData(
            new IdentityRole<Guid> { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Admin", NormalizedName = "ADMIN" },
            new IdentityRole<Guid> { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Psychologist", NormalizedName = "PSYCHOLOGIST" },
            new IdentityRole<Guid> { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Client", NormalizedName = "CLIENT" }
        );
    }
}

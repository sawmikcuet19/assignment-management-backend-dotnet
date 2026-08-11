using AssignmentManagement.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ClassCourse> ClassCourses => Set<ClassCourse>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<ClassSubject> ClassSubjects => Set<ClassSubject>();
    public DbSet<TeacherClassSubject> TeacherClassSubjects => Set<TeacherClassSubject>();
    public DbSet<StudentClass> StudentClasses => Set<StudentClass>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<SubmissionAttachment> SubmissionAttachments => Set<SubmissionAttachment>();
    public DbSet<AppSetting> ApplicationSettings => Set<AppSetting>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Role>(entity =>
        {
            entity.HasIndex(x => x.Name).IsUnique();
        });

        builder.Entity<User>(entity =>
        {
            entity.HasIndex(x => x.Email).IsUnique();
        });

        builder.Entity<ClassSubject>(entity =>
        {
            entity.HasIndex(x => new
            {
                x.ClassId,
                x.SubjectId,
                x.AcademicYear
            }).IsUnique();
        });

        builder.Entity<TeacherClassSubject>(entity =>
        {
            entity.HasIndex(x => new
            {
                x.ClassSubjectId,
                x.TeacherUserId
            }).IsUnique();
        });

        builder.Entity<StudentClass>(entity =>
        {
            entity.HasIndex(x => new
            {
                x.ClassId,
                x.StudentUserId,
                x.AcademicYear
            }).IsUnique();
        });

        builder.Entity<Submission>(entity =>
        {
            entity.HasIndex(x => new
            {
                x.AssignmentId,
                x.StudentUserId
            }).IsUnique();
        });

        builder.Entity<AppSetting>(entity =>
        {
            entity.HasKey(x => x.Key);
        });

        builder.Entity<User>()
            .HasOne(x => x.Role)
            .WithMany(x => x.Users)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ClassSubject>()
            .HasOne(x => x.Class)
            .WithMany(x => x.ClassSubjects)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ClassSubject>()
            .HasOne(x => x.Subject)
            .WithMany(x => x.ClassSubjects)
            .HasForeignKey(x => x.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TeacherClassSubject>()
            .HasOne(x => x.ClassSubject)
            .WithMany(x => x.TeacherAssignments)
            .HasForeignKey(x => x.ClassSubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<TeacherClassSubject>()
            .HasOne(x => x.Teacher)
            .WithMany()
            .HasForeignKey(x => x.TeacherUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentClass>()
            .HasOne(x => x.Class)
            .WithMany(x => x.StudentEnrollments)
            .HasForeignKey(x => x.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<StudentClass>()
            .HasOne(x => x.Student)
            .WithMany()
            .HasForeignKey(x => x.StudentUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Assignment>()
            .HasOne(x => x.ClassSubject)
            .WithMany(x => x.Assignments)
            .HasForeignKey(x => x.ClassSubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Assignment>()
            .HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Submission>()
            .HasOne(x => x.Assignment)
            .WithMany(x => x.Submissions)
            .HasForeignKey(x => x.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Submission>()
            .HasOne(x => x.Student)
            .WithMany()
            .HasForeignKey(x => x.StudentUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Submission>()
            .HasOne(x => x.ReviewedByUser)
            .WithMany()
            .HasForeignKey(x => x.ReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SubmissionAttachment>()
            .HasOne(x => x.Submission)
            .WithMany(x => x.Attachments)
            .HasForeignKey(x => x.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
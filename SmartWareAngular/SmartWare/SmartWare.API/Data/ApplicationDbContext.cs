using Microsoft.EntityFrameworkCore;
using SmartWare.API.Core.Entities;

namespace SmartWare.API.Data
{
    /// <summary>
    /// Ana veritabanı context sınıfı
    /// EF Core'un veritabanıyla iletişim kurduğu yerdir
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        // Constructor - Dependency Injection için
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)   

        {
        }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<PostTag> PostTags { get; set; }
        public DbSet<PostView> PostViews { get; set; }
        public DbSet<User> Users { get; set; }
        // ============================================
        // OnModelCreating - Fluent API ile tablo yapılandırması
        // ============================================

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========== AUTHOR CONFIGURATION ==========
            modelBuilder.Entity<Author>(entity =>
            {
                // Tablo adı
                entity.ToTable("Authors");

                // Primary Key
                entity.HasKey(e => e.Id);

                // Email unique olmalı
                entity.HasIndex(e => e.Email)
                    .IsUnique();

                // Property constraints
                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Bio)
                    .HasMaxLength(1000);

                entity.Property(e => e.AvatarUrl)
                    .HasMaxLength(500);

                // Soft delete için global query filter
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // ========== POST CONFIGURATION ==========
            modelBuilder.Entity<Post>(entity =>
            {
                entity.ToTable("Posts");
                entity.HasKey(e => e.Id);

                // Slug unique olmalı
                entity.HasIndex(e => e.Slug)
                    .IsUnique();

                // İndeksler - Sık arama yapılan alanlar
                entity.HasIndex(e => e.IsPublished);
                entity.HasIndex(e => e.PublishedAt);
                entity.HasIndex(e => e.AuthorId);

                // Property constraints
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Slug)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.Content)
                    .IsRequired()
                    .HasColumnType("nvarchar(max)"); // Uzun içerik

                entity.Property(e => e.Summary)
                    .HasMaxLength(500);

                entity.Property(e => e.FeaturedImageUrl)
                    .HasMaxLength(500);

                // Foreign Key - Author ilişkisi
                entity.HasOne(e => e.Author)
                    .WithMany(a => a.Posts)
                    .HasForeignKey(e => e.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict); // Cascade delete YAPMA

                // Soft delete filter
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // ========== TAG CONFIGURATION ==========
            modelBuilder.Entity<Tag>(entity =>
            {
                entity.ToTable("Tags");
                entity.HasKey(e => e.Id);

                // Name ve Slug unique
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.Slug).IsUnique();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Slug)
                    .IsRequired()
                    .HasMaxLength(60);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // ========== POSTTAG CONFIGURATION (Many-to-Many) ==========
            modelBuilder.Entity<PostTag>(entity =>
            {
                entity.ToTable("PostTags");
                entity.HasKey(e => e.Id);

                // Composite unique index - Aynı post-tag çifti tekrar eklenmemeli
                entity.HasIndex(e => new { e.PostId, e.TagId })
                    .IsUnique();

                // Post ilişkisi
                entity.HasOne(e => e.Post)
                    .WithMany(p => p.PostTags)
                    .HasForeignKey(e => e.PostId)
                    .OnDelete(DeleteBehavior.Cascade); // Post silinirse tags de silinir

                // Tag ilişkisi
                entity.HasOne(e => e.Tag)
                    .WithMany(t => t.PostTags)
                    .HasForeignKey(e => e.TagId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // ========== POSTVIEW CONFIGURATION ==========
            modelBuilder.Entity<PostView>(entity =>
            {
                entity.ToTable("PostViews");
                entity.HasKey(e => e.Id);

                // İndeks - Analytics sorguları için
                entity.HasIndex(e => e.PostId);
                entity.HasIndex(e => e.ViewedAt);

                entity.Property(e => e.IpAddress)
                    .HasMaxLength(45); // IPv6 için

                entity.Property(e => e.UserAgent)
                    .HasMaxLength(500);

                // Post ilişkisi
                entity.HasOne(e => e.Post)
                    .WithMany(p => p.PostViews)
                    .HasForeignKey(e => e.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // ========== USER CONFIGURATION ==========
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);

                // Username ve Email unique
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.FirstName)
                    .HasMaxLength(100);

                entity.Property(e => e.LastName)
                    .HasMaxLength(100);

                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasDefaultValue("Reader");

                entity.HasQueryFilter(e => !e.IsDeleted);
            });
        }

        // ============================================
        // SaveChangesAsync Override - Otomatik UpdatedAt
        // ============================================

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Değişen entity'leri bul
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Core.Common.BaseEntity &&
                           (e.State == EntityState.Modified));

            // UpdatedAt'i otomatik güncelle
            foreach (var entry in entries)
            {
                ((Core.Common.BaseEntity)entry.Entity).UpdatedAt = DateTime.UtcNow;
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
// ============================================
// AÇIKLAMALAR
// ============================================

// 1. DbSet<T>:
//    - Her DbSet bir veritabanı tablosunu temsil eder
//    - LINQ sorguları için kullanılır
//    - Örnek: context.Posts.Where(p => p.IsPublished)

// 2. OnModelCreating:
//    - Fluent API ile tablo yapılandırması
//    - Attributes yerine kod ile yapılandırma (daha esnek)
//    - İndeksler, constraints, relationships burada tanımlanır

// 3. HasQueryFilter:
//    - Global query filter - her sorguda otomatik uygulanır
//    - Soft delete için: !e.IsDeleted
//    - Tüm sorgularda otomatik olarak IsDeleted=false filtresi eklenir

// 4. OnDelete(DeleteBehavior):
//    - Cascade: Parent silinince child da silinir
//    - Restrict: Parent silinmek istenirse hata verir (child varsa)
//    - SetNull: Parent silinince child'ın FK'si null olur

// 5. HasIndex:
//    - Veritabanı indeksi oluşturur
//    - Sorgu performansını artırır
//    - IsUnique() ile unique constraint

// 6. SaveChangesAsync Override:
//    - Her kayıt güncellendiğinde UpdatedAt otomatik set edilir
//    - ChangeTracker ile değişen entity'ler izlenir



using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProManagerOnline.Site.Domain.Documentation;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;

namespace ProManagerOnline.Site.Infrastructure.Persistence.Configurations;

/// <summary>EF Core mapping for the <see cref="DocArticle"/> aggregate.</summary>
internal sealed class DocArticleConfiguration : IEntityTypeConfiguration<DocArticle>
{
    public void Configure(EntityTypeBuilder<DocArticle> builder)
    {
        builder.ToTable("DocArticles");

        builder.HasKey(article => article.Id);
        builder.Property(article => article.Id)
            .HasConversion(id => id.Value, value => new DocArticleId(value))
            .ValueGeneratedNever();

        builder.Property(article => article.ProductId)
            .HasConversion(id => id.Value, value => new ProductId(value))
            .IsRequired();

        builder.Property(article => article.Slug)
            .HasConversion(slug => slug.Value, value => Slug.Create(value))
            .HasMaxLength(Slug.MaxLength)
            .IsRequired();

        builder.Property(article => article.Title).HasMaxLength(200).IsRequired();
        builder.Property(article => article.Section).HasMaxLength(100).IsRequired();
        builder.Property(article => article.Body).IsRequired();
        builder.Property(article => article.Position).IsRequired();
        builder.Property(article => article.Status).HasConversion<string>().HasMaxLength(20).IsRequired();

        // A documentation article's slug is unique within its product.
        builder.HasIndex(article => new { article.ProductId, article.Slug }).IsUnique();
    }
}

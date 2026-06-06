using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProManagerOnline.Site.Domain.Content;

namespace ProManagerOnline.Site.Infrastructure.Persistence.Configurations;

/// <summary>EF Core mapping for the singleton <see cref="AboutPage"/> aggregate.</summary>
internal sealed class AboutPageConfiguration : IEntityTypeConfiguration<AboutPage>
{
    public void Configure(EntityTypeBuilder<AboutPage> builder)
    {
        builder.ToTable("AboutPages");

        builder.HasKey(page => page.Id);
        builder.Property(page => page.Id)
            .HasConversion(id => id.Value, value => new AboutPageId(value))
            .ValueGeneratedNever();

        builder.Property(page => page.Title).HasMaxLength(200).IsRequired();
        builder.Property(page => page.Body).IsRequired();
    }
}

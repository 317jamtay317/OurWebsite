using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProManagerOnline.Site.Domain.Products;
using ProManagerOnline.Site.Domain.ValueObjects;

namespace ProManagerOnline.Site.Infrastructure.Persistence.Configurations;

/// <summary>EF Core mapping for the <see cref="Product"/> aggregate and its owned plans.</summary>
internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(product => product.Id);
        builder.Property(product => product.Id)
            .HasConversion(id => id.Value, value => new ProductId(value))
            .ValueGeneratedNever();

        builder.Property(product => product.Slug)
            .HasConversion(slug => slug.Value, value => Slug.Create(value))
            .HasMaxLength(Slug.MaxLength)
            .IsRequired();
        builder.HasIndex(product => product.Slug).IsUnique();

        builder.Property(product => product.Name).HasMaxLength(200).IsRequired();
        builder.Property(product => product.Category).HasMaxLength(200).IsRequired();
        builder.Property(product => product.Summary).HasMaxLength(1000).IsRequired();
        builder.Property(product => product.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(product => product.PricingKind).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.OwnsMany(product => product.Plans, plan =>
        {
            plan.ToTable("ProductPlans");
            plan.WithOwner().HasForeignKey("ProductId");
            plan.HasKey(p => p.Id);
            plan.Property(p => p.Id)
                .HasConversion(id => id.Value, value => new PlanId(value))
                .ValueGeneratedNever();
            plan.Property(p => p.Name).HasMaxLength(100).IsRequired();
            plan.Property(p => p.Description).HasMaxLength(500);
            plan.Property(p => p.BillingPeriod).HasConversion<string>().HasMaxLength(20).IsRequired();
            plan.Property(p => p.IsFeatured);

            plan.OwnsOne(p => p.Price, price =>
            {
                price.Property(money => money.Amount).HasColumnName("PriceAmount").HasPrecision(18, 2);
                price.Property(money => money.Currency).HasColumnName("PriceCurrency").HasConversion<string>().HasMaxLength(3);
            });
            plan.Navigation(p => p.Price).IsRequired();
        });

        builder.Navigation(product => product.Plans)
            .HasField("_plans")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

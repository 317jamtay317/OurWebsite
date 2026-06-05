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

        // Optional one-time price, present only for fixed-price products.
        builder.OwnsOne(product => product.FixedPrice, price =>
        {
            price.Property(money => money.Amount).HasColumnName("FixedPriceAmount").HasPrecision(18, 2);
            price.Property(money => money.Currency).HasColumnName("FixedPriceCurrency").HasConversion<string>().HasMaxLength(3);
        });
        builder.Navigation(product => product.FixedPrice).IsRequired(false);

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

            // Feature lines, ordered by Position, stored in a child table. The public Plan.Features
            // read model projects from the mapped _features backing field, so it is ignored here.
            plan.Ignore(p => p.Features);
            plan.OwnsMany<PlanFeature>("_features", feature =>
            {
                feature.ToTable("ProductPlanFeatures");
                feature.WithOwner().HasForeignKey("PlanId");
                feature.Property<int>("Id");
                feature.HasKey("Id");
                feature.Property(f => f.Position);
                feature.Property(f => f.Text).HasMaxLength(200).IsRequired();
            });
            plan.Navigation("_features").UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.Navigation(product => product.Plans)
            .HasField("_plans")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

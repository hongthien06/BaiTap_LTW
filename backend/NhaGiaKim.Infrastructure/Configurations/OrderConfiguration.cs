using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NhaGiaKim.Domain.Entities;

namespace NhaGiaKim.Infrastructure.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.ToTable("Orders");
        b.HasKey(x => x.Id);
        b.Property(x => x.OrderCode).HasMaxLength(30).IsRequired();
        b.HasIndex(x => x.OrderCode).IsUnique();   // chan trung ma don khi chay dong thoi
        b.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Phone).HasMaxLength(20).IsRequired();
        b.Property(x => x.Address).HasMaxLength(500).IsRequired();
        b.Property(x => x.UnitPrice).HasPrecision(18, 2);
        b.Property(x => x.TotalPrice).HasPrecision(18, 2);
        b.Property(x => x.PaymentMethod).HasConversion<int>();
        b.Property(x => x.Status).HasConversion<int>();
        b.Property(x => x.Note).HasMaxLength(1000);

        b.HasIndex(x => x.Phone);
        b.HasIndex(x => x.Status);
        b.HasIndex(x => x.CreatedAt);
    }
}

public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
{
    public void Configure(EntityTypeBuilder<Feedback> b)
    {
        b.ToTable("Feedbacks");
        b.HasKey(x => x.Id);
        b.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Content).HasMaxLength(2000).IsRequired();
        b.HasIndex(x => x.IsApproved);
        b.HasIndex(x => x.CreatedAt);
    }
}

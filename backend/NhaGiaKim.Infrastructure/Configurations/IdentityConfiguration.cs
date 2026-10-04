using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NhaGiaKim.Domain.Entities;
using NhaGiaKim.Domain.Enums;

namespace NhaGiaKim.Infrastructure.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> b)
    {
        b.ToTable("Roles");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.Name).IsUnique();

        b.HasData(
            new Role { Id = 1, Name = RoleName.Admin },
            new Role { Id = 2, Name = RoleName.Staff });
    }
}

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> b)
    {
        b.ToTable("AppUsers");
        b.HasKey(x => x.Id);
        b.Property(x => x.Email).HasMaxLength(256).IsRequired();
        b.HasIndex(x => x.Email).IsUnique();
        b.Property(x => x.PasswordHash).HasMaxLength(200).IsRequired();
        b.Property(x => x.FullName).HasMaxLength(200).IsRequired();

        b.HasOne(x => x.Role).WithMany(x => x.Users)
            .HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class SiteSettingConfiguration : IEntityTypeConfiguration<SiteSetting>
{
    public void Configure(EntityTypeBuilder<SiteSetting> b)
    {
        b.ToTable("SiteSettings");
        b.HasKey(x => x.Id);
        b.Property(x => x.Key).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.Key).IsUnique();
        b.Property(x => x.Value).HasMaxLength(1000).IsRequired();
        b.Property(x => x.Description).HasMaxLength(300);
    }
}

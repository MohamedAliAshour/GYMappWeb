using GYMappWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GYMappWeb.Areas.Identity.Data;

public class GYMappWebContext : IdentityDbContext<ApplicationUser>
{
    public GYMappWebContext(DbContextOptions<GYMappWebContext> options)
        : base(options)
    {
    }

    public DbSet<TblMemberShipFreeze> TblMemberShipFreezes { get; set; }
    public DbSet<TblMembershipType> TblMembershipTypes { get; set; }
    public DbSet<TblOffer> TblOffers { get; set; }
    public DbSet<TblUser> TblUsers { get; set; }
    public DbSet<TblUserMemberShip> TblUserMemberShips { get; set; }
    public DbSet<GymBranch> GymBranches { get; set; }
    public DbSet<Checkin> Checkins { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure GymBranch
        builder.Entity<GymBranch>(entity =>
        {
            entity.HasKey(e => e.GymBranchId).HasName("PK_GymBranches");
            entity.ToTable("GymBranches");
            entity.Property(e => e.GymBranchId).HasColumnName("GymBranch_ID").ValueGeneratedOnAdd();
            entity.Property(e => e.GymName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Location).IsRequired().HasMaxLength(200);
            entity.Property(e => e.CreateDate)
                .HasColumnType("datetime2(0)")
                .IsRequired(true);
            entity.Property(e => e.CreatedBy)
                .HasColumnType("nvarchar(100)")
                .IsRequired(true);
        });

        // Configure Checkin
        builder.Entity<Checkin>(entity =>
        {
            entity.HasKey(e => e.CheckinId).HasName("PK_Checkins");
            entity.ToTable("Checkins");
            entity.Property(e => e.CheckinId).HasColumnName("Checkin_ID").ValueGeneratedOnAdd();
            entity.Property(e => e.CheckinDate)
                .HasColumnType("datetime2(0)")
                .IsRequired(true);
            entity.Property(e => e.UserId).HasColumnName("User_ID");
            entity.Property(e => e.GymBranchId).HasColumnName("GymBranch_ID");
            entity.Property(e => e.CreatedBy)
                .HasColumnType("nvarchar(100)")
                .IsRequired(true);

            entity.HasOne(d => d.User)
                .WithMany(p => p.Checkins)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Checkins_tbl_Users");

            entity.HasOne(d => d.GymBranch)
                .WithMany(p => p.Checkins)
                .HasForeignKey(d => d.GymBranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Checkins_GymBranches");
        });

        // Configure TblUser
        builder.Entity<TblUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK_Users");
            entity.ToTable("tbl_Users");
            entity.HasIndex(e => e.UserCode, "IX_tbl_UserCode").IsUnique();
            entity.HasIndex(e => e.UserName, "IX_tbl_UserName").IsUnique();
            entity.Property(e => e.UserId).HasColumnName("User_ID").ValueGeneratedOnAdd();
            entity.Property(e => e.UserCode).IsRequired();
            entity.Property(e => e.UserName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.UserPhone).IsRequired().HasMaxLength(50);
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(50);
            entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime2(0)")
                    .IsRequired(true);
            entity.Property(e => e.CreatedBy)
                    .HasColumnType("nvarchar(100)")
                    .IsRequired(true);
            entity.Property(e => e.GymBranchId)
                .HasColumnName("GymBranch_ID")
                .IsRequired(false); // Make it nullable

            entity.HasOne(d => d.GymBranch)
                .WithMany(p => p.Users)
                .HasForeignKey(d => d.GymBranchId)
                .OnDelete(DeleteBehavior.SetNull) // Set null on delete
                .HasConstraintName("FK_tbl_Users_GymBranches");
        });

        // Configure ApplicationUser - Make GymBranchId nullable
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.GymBranchId)
                .HasColumnName("GymBranch_ID")
                .IsRequired(false); // Make it nullable

            entity.HasOne<GymBranch>()
                .WithMany(p => p.ApplicationUsers)
                .HasForeignKey(d => d.GymBranchId)
                .OnDelete(DeleteBehavior.SetNull) // Set null on delete
                .HasConstraintName("FK_AspNetUsers_GymBranches");
        });

        // Configure TblUserMemberShip
        builder.Entity<TblUserMemberShip>(entity =>
        {
            entity.HasKey(e => e.UserMemberShipId).HasName("PK_UserMemberShip");
            entity.ToTable("tbl_UserMemberShip");
            entity.Property(e => e.UserMemberShipId).HasColumnName("UserMemberShip_ID").ValueGeneratedOnAdd();
            entity.Property(e => e.StartDate).IsRequired();
            entity.Property(e => e.EndDate).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.invitationUsed);
            entity.Property(e => e.TotalFreezedDays);
            entity.Property(e => e.OffId).HasColumnName("Off_ID");
            entity.Property(e => e.UserId).HasColumnName("User_ID");
            entity.Property(e => e.MemberShipTypesId).HasColumnName("MemberShipTypes_ID");
            entity.Property(e => e.CreatedBy)
                   .HasColumnType("nvarchar(100)")
                   .IsRequired(true);
            entity.Property(e => e.CreatedDate)
                     .HasColumnType("datetime2(0)")
                     .IsRequired(true);
            entity.HasOne(d => d.User)
                .WithMany(p => p.TblUserMemberShips)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_UserMemberShip_tbl_Users");
            entity.HasOne(d => d.MemberShipTypes)
                .WithMany(p => p.TblUserMemberShips)
                .HasForeignKey(d => d.MemberShipTypesId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_UserMemberShip_tbl_MembershipTypes");
            entity.HasOne(d => d.Off)
                .WithMany(p => p.TblUserMemberShips)
                .HasForeignKey(d => d.OffId)
                .HasConstraintName("FK_tbl_UserMemberShip_tbl_Offers");
        });

        // Configure TblOffer
        builder.Entity<TblOffer>(entity =>
        {
            entity.HasKey(e => e.OffId).HasName("PK_Offers");
            entity.ToTable("tbl_Offers");
            entity.Property(e => e.OffId).HasColumnName("Off_ID").ValueGeneratedOnAdd();
            entity.Property(e => e.OfferName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DiscountPrecentage).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.MemberShipTypesId).HasColumnName("MemberShipTypes_ID");
            entity.Property(e => e.CreatedBy).HasColumnType("nvarchar(100)").IsRequired(true);
            entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime2(0)")
                    .IsRequired(true);
            entity.HasOne(d => d.MemberShipTypes)
                .WithMany(p => p.TblOffers)
                .HasForeignKey(d => d.MemberShipTypesId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_Offers_tbl_MembershipTypes");
        });

        // Configure TblMembershipType
        builder.Entity<TblMembershipType>(entity =>
        {
            entity.HasKey(e => e.MemberShipTypesId).HasName("PK_MembershipTypes");
            entity.ToTable("tbl_MembershipTypes");
            entity.Property(e => e.MemberShipTypesId).HasColumnName("MemberShipTypes_ID").ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.MembershipDuration).IsRequired();
            entity.Property(e => e.Price).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(50);
            entity.Property(e => e.FreezeCount).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.TotalFreezeDays).IsRequired();
            entity.Property(e => e.CreatedBy).HasColumnType("nvarchar(100)").IsRequired(true);
            entity.Property(e => e.CreatedDate)
                     .HasColumnType("datetime2(0)")
                     .IsRequired(true);
            entity.Property(e => e.invitationCount).IsRequired();
        });

        // Configure TblMemberShipFreeze
        builder.Entity<TblMemberShipFreeze>(entity =>
        {
            entity.HasKey(e => e.MemberShipFreezeId).HasName("PK_MemberShipFreeze");
            entity.ToTable("tbl_MemberShipFreeze");
            entity.Property(e => e.MemberShipFreezeId).HasColumnName("MemberShipFreeze_ID").ValueGeneratedOnAdd();
            entity.Property(e => e.UserMemberShipId).HasColumnName("UserMemberShip_ID");
            entity.Property(e => e.FreezeStartDate).IsRequired();
            entity.Property(e => e.FreezeEndDate).IsRequired();
            entity.Property(e => e.Reason).HasMaxLength(50);
            entity.Property(e => e.CreatedBy).HasColumnType("nvarchar(100)").IsRequired(true);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime2(0)").IsRequired(true);
            entity.HasOne(d => d.UserMemberShip)
                .WithMany(p => p.TblMemberShipFreezes)
                .HasForeignKey(d => d.UserMemberShipId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_MemberShipFreeze_tbl_UserMemberShip");
        });
    }
}
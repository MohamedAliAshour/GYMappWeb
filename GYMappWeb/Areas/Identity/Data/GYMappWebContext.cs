using GYMappWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

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
    public DbSet<LogEntry> Logs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure LogEntry table
        builder.Entity<LogEntry>(entity =>
        {
            entity.ToTable("Logs");

            // Indexes for better query performance
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.Level);
            entity.HasIndex(e => e.Controller);
            entity.HasIndex(e => e.User);
            entity.HasIndex(e => e.StatusCode);

            // Configure string lengths
            entity.Property(e => e.Level).HasMaxLength(50);
            entity.Property(e => e.Logger).HasMaxLength(255);
            entity.Property(e => e.Controller).HasMaxLength(100);
            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.User).HasMaxLength(100);
            entity.Property(e => e.Url).HasMaxLength(500);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.HttpMethod).HasMaxLength(10);
            entity.Property(e => e.RequestPath).HasMaxLength(1000);
        });

        // Configure ApplicationUser - Add IsActive property and GymBranchId foreign key
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true); // Default to active

            entity.Property(e => e.GymBranchId)
                .HasColumnName("GymBranch_ID")
                .IsRequired(false); // Make it nullable

            entity.HasOne<GymBranch>()
                .WithMany(p => p.ApplicationUsers)
                .HasForeignKey(d => d.GymBranchId)
                .OnDelete(DeleteBehavior.SetNull) // Set null on delete
                .HasConstraintName("FK_AspNetUsers_GymBranches");
        });

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
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true); // Default to active
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
        // IsActive property removed from here

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

        // Configure TblUserMemberShip - Add GymBranchId foreign key
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
            entity.Property(e => e.GymBranchId)
                .HasColumnName("GymBranch_ID")
                .IsRequired(false); // Make it nullable
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

            entity.HasOne(d => d.GymBranch)
                .WithMany(p => p.UserMemberships)
                .HasForeignKey(d => d.GymBranchId)
                .OnDelete(DeleteBehavior.SetNull) // Set null on delete
                .HasConstraintName("FK_tbl_UserMemberShip_GymBranches");
        });

        // Configure TblOffer - Add GymBranchId foreign key
        builder.Entity<TblOffer>(entity =>
        {
            entity.HasKey(e => e.OffId).HasName("PK_Offers");
            entity.ToTable("tbl_Offers");
            entity.Property(e => e.OffId).HasColumnName("Off_ID").ValueGeneratedOnAdd();
            entity.Property(e => e.OfferName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DiscountPrecentage).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.MemberShipTypesId).HasColumnName("MemberShipTypes_ID");
            entity.Property(e => e.GymBranchId)
                .HasColumnName("GymBranch_ID")
                .IsRequired(false); // Make it nullable
            entity.Property(e => e.CreatedBy).HasColumnType("nvarchar(100)").IsRequired(true);
            entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime2(0)")
                    .IsRequired(true);

            entity.HasOne(d => d.MemberShipTypes)
                .WithMany(p => p.TblOffers)
                .HasForeignKey(d => d.MemberShipTypesId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_Offers_tbl_MembershipTypes");

            entity.HasOne(d => d.GymBranch)
                .WithMany(p => p.Offers)
                .HasForeignKey(d => d.GymBranchId)
                .OnDelete(DeleteBehavior.SetNull) // Set null on delete
                .HasConstraintName("FK_tbl_Offers_GymBranches");
        });

        // Configure TblMembershipType - Add GymBranchId foreign key
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
            entity.Property(e => e.GymBranchId)
                .HasColumnName("GymBranch_ID")
                .IsRequired(false); // Make it nullable
            entity.Property(e => e.CreatedBy).HasColumnType("nvarchar(100)").IsRequired(true);
            entity.Property(e => e.CreatedDate)
                     .HasColumnType("datetime2(0)")
                     .IsRequired(true);
            entity.Property(e => e.invitationCount).IsRequired();

            entity.HasOne(d => d.GymBranch)
                .WithMany(p => p.MembershipTypes)
                .HasForeignKey(d => d.GymBranchId)
                .OnDelete(DeleteBehavior.SetNull) // Set null on delete
                .HasConstraintName("FK_tbl_MembershipTypes_GymBranches");
        });

        // Configure TblMemberShipFreeze - Add GymBranchId foreign key
        builder.Entity<TblMemberShipFreeze>(entity =>
        {
            entity.HasKey(e => e.MemberShipFreezeId).HasName("PK_MemberShipFreeze");
            entity.ToTable("tbl_MemberShipFreeze");
            entity.Property(e => e.MemberShipFreezeId).HasColumnName("MemberShipFreeze_ID").ValueGeneratedOnAdd();
            entity.Property(e => e.UserMemberShipId).HasColumnName("UserMemberShip_ID");
            entity.Property(e => e.FreezeStartDate).IsRequired();
            entity.Property(e => e.FreezeEndDate).IsRequired();
            entity.Property(e => e.Reason).HasMaxLength(50);
            entity.Property(e => e.GymBranchId)
                .HasColumnName("GymBranch_ID")
                .IsRequired(false); // Make it nullable
            entity.Property(e => e.CreatedBy).HasColumnType("nvarchar(100)").IsRequired(true);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime2(0)").IsRequired(true);

            entity.HasOne(d => d.UserMemberShip)
                .WithMany(p => p.TblMemberShipFreezes)
                .HasForeignKey(d => d.UserMemberShipId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_tbl_MemberShipFreeze_tbl_UserMemberShip");

            entity.HasOne(d => d.GymBranch)
                .WithMany(p => p.MembershipFreezes)
                .HasForeignKey(d => d.GymBranchId)
                .OnDelete(DeleteBehavior.SetNull) // Set null on delete
                .HasConstraintName("FK_tbl_MemberShipFreeze_GymBranches");
        });
    }
}
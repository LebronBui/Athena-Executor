using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RealEstateAuction.Models;

public partial class RealEstateContext : DbContext
{
    public RealEstateContext()
    {
    }

    public RealEstateContext(DbContextOptions<RealEstateContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Auction> Auctions { get; set; }

    public virtual DbSet<AuctionBidding> AuctionBiddings { get; set; }

    public virtual DbSet<Banking> Bankings { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Image> Images { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<TicketComment> TicketComments { get; set; }

    public virtual DbSet<TicketImage> TicketImages { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        IConfigurationRoot configuration = builder.Build();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("connection"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Auction>(entity =>
        {
            entity.ToTable("Auction");

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CreatedTime).HasColumnType("datetime");
            entity.Property(e => e.Description).HasColumnType("ntext");
            entity.Property(e => e.Direction).HasMaxLength(255);
            entity.Property(e => e.EndPrice).HasColumnType("money");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.Reason).HasColumnType("ntext");
            entity.Property(e => e.StartPrice).HasColumnType("money");
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.UpdatedTime).HasColumnType("datetime");

            entity.HasOne(d => d.Approver).WithMany(p => p.AuctionApprovers)
                .HasForeignKey(d => d.ApproverId)
                .HasConstraintName("FK_Auction_User1");

            entity.HasOne(d => d.User).WithMany(p => p.AuctionUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Auction_User");

            entity.HasMany(d => d.Categories).WithMany(p => p.Auctions)
                .UsingEntity<Dictionary<string, object>>(
                    "AuctionCategory",
                    r => r.HasOne<Category>().WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Auction_Category_Category"),
                    l => l.HasOne<Auction>().WithMany()
                        .HasForeignKey("AuctionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Auction_Category_Auction"),
                    j =>
                    {
                        j.HasKey("AuctionId", "CategoryId");
                        j.ToTable("Auction_Category");
                    });
        });

        modelBuilder.Entity<AuctionBidding>(entity =>
        {
            entity.HasKey(e => e.BiddingId);

            entity.ToTable("Auction_Bidding");

            entity.Property(e => e.BiddingPrice).HasColumnType("money");
            entity.Property(e => e.TimeBidding).HasColumnType("datetime");

            entity.HasOne(d => d.Auction).WithMany(p => p.AuctionBiddings)
                .HasForeignKey(d => d.AuctionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Auction_Bidding_Auction");

            entity.HasOne(d => d.Member).WithMany(p => p.AuctionBiddings)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Auction_Bidding_User");
        });

        modelBuilder.Entity<Banking>(entity =>
        {
            entity.ToTable("Banking");

            entity.Property(e => e.AccountName).HasColumnType("ntext");
            entity.Property(e => e.BankAccount).HasColumnType("ntext");
            entity.Property(e => e.BankName).HasColumnType("ntext");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Category");

            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.Property(e => e.Url).HasColumnType("ntext");

            entity.HasMany(d => d.Auctions).WithMany(p => p.Images)
                .UsingEntity<Dictionary<string, object>>(
                    "AuctionImage",
                    r => r.HasOne<Auction>().WithMany()
                        .HasForeignKey("AuctionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_AuctionImage_Auction"),
                    l => l.HasOne<Image>().WithMany()
                        .HasForeignKey("ImageId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Auction_Image_Images"),
                    j =>
                    {
                        j.HasKey("ImageId", "AuctionId");
                        j.ToTable("Auction_Image");
                    });
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notification");

            entity.Property(e => e.Description).HasColumnType("ntext");
            entity.Property(e => e.Link).HasColumnType("ntext");

            entity.HasOne(d => d.ToUserNavigation).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.ToUser)
                .HasConstraintName("FK_Notification_User1");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payment");

            entity.Property(e => e.Amount).HasColumnType("money");
            entity.Property(e => e.Code).HasColumnType("ntext");
            entity.Property(e => e.TransactionDate).HasColumnType("datetime");
            entity.Property(e => e.UserBankAccount).HasMaxLength(50);
            entity.Property(e => e.UserBankName).HasMaxLength(50);

            entity.HasOne(d => d.Bank).WithMany(p => p.Payments)
                .HasForeignKey(d => d.BankId)
                .HasConstraintName("FK_Payment_Banking");

            entity.HasOne(d => d.User).WithMany(p => p.Payments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Payment_User1");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role");

            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.ToTable("Ticket");

            entity.Property(e => e.Description).HasColumnType("ntext");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Staff).WithMany(p => p.TicketStaffs)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK_Ticket_User1");

            entity.HasOne(d => d.User).WithMany(p => p.TicketUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ticket_User");
        });

        modelBuilder.Entity<TicketComment>(entity =>
        {
            entity.ToTable("TicketComment");

            entity.Property(e => e.Comment).HasColumnType("ntext");

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketComments)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TicketComment_Ticket");

            entity.HasOne(d => d.User).WithMany(p => p.TicketComments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TicketComment_User");
        });

        modelBuilder.Entity<TicketImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Table_1");

            entity.ToTable("Ticket_Image");

            entity.Property(e => e.Url).HasColumnType("ntext");

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketImages)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ticket_Image_Ticket");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.Property(e => e.Address).HasColumnType("ntext");
            entity.Property(e => e.Avatar).HasColumnType("ntext");
            entity.Property(e => e.Description).HasColumnType("ntext");
            entity.Property(e => e.Dob)
                .HasColumnType("date")
                .HasColumnName("DOB");
            entity.Property(e => e.Email).IsUnicode(false);
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Wallet).HasColumnType("money");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_Role1");

            entity.HasMany(d => d.Auctions).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AuctionParticipant",
                    r => r.HasOne<Auction>().WithMany()
                        .HasForeignKey("AuctionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_AuctionParticipant_Auction"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_AuctionParticipant_User"),
                    j =>
                    {
                        j.HasKey("UserId", "AuctionId").HasName("PK_AuctionParticipant_1");
                        j.ToTable("AuctionParticipant");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

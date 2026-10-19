    using System;
    using System.Collections.Generic;
    using BeeMart.BackendApi.Models;
    using Microsoft.EntityFrameworkCore;

    namespace BeeMart.BackendApi.Data;

    public partial class BeeMartDbContext : DbContext
    {
   

        public BeeMartDbContext(DbContextOptions<BeeMartDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<LtPaymentMethod> LtPaymentMethods { get; set; }

        public virtual DbSet<MsProduct> MsProducts { get; set; }

        public virtual DbSet<MsUser> MsUsers { get; set; }

        public virtual DbSet<TrTransaction> TrTransactions { get; set; }

        public virtual DbSet<TrTransactionDetail> TrTransactionDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LtPaymentMethod>(entity =>
            {
                entity.HasKey(e => e.IdPaymentMethod);

                entity.ToTable("LtPaymentMethod");

                entity.Property(e => e.MethodName).HasMaxLength(50);
                entity.Property(e => e.Userin).HasMaxLength(36);
                entity.Property(e => e.Userup).HasMaxLength(36);
            });

            modelBuilder.Entity<MsProduct>(entity =>
            {
                entity.HasKey(e => e.IdProduct);

                entity.ToTable("MsProduct");

                entity.HasIndex(e => e.Name, "IX_MsProduct_Name");

                entity.Property(e => e.IdProduct).HasMaxLength(36);
                entity.Property(e => e.Discount).HasColumnType("decimal(5, 2)");
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.Userin).HasMaxLength(36);
                entity.Property(e => e.Userup).HasMaxLength(36);
                entity.Property(e => e.Weight).HasColumnType("decimal(10, 2)");
            });

            modelBuilder.Entity<MsUser>(entity =>
            {
                entity.HasKey(e => e.IdUser);

                entity.ToTable("MsUser");

                entity.HasIndex(e => e.Username, "IX_MsUser_Username").IsUnique();

                entity.Property(e => e.IdUser).HasMaxLength(5);
                entity.Property(e => e.FullName).HasMaxLength(300);
                entity.Property(e => e.PasswordHash).HasMaxLength(255);
                entity.Property(e => e.Role).HasMaxLength(20);
                entity.Property(e => e.Userin).HasMaxLength(36);
                entity.Property(e => e.Username).HasMaxLength(50);
                entity.Property(e => e.Userup).HasMaxLength(36);
            });

            modelBuilder.Entity<TrTransaction>(entity =>
            {
                entity.HasKey(e => e.IdTransaction);

                entity.ToTable("TrTransaction");

                entity.HasIndex(e => e.IdPaymentMethod, "IX_TrTransaction_IdPaymentMethod");

                entity.HasIndex(e => e.IdUser, "IX_TrTransaction_IdUser");

                entity.HasIndex(e => e.InvoiceNo, "IX_TrTransaction_InvoiceNo").IsUnique();

                entity.HasIndex(e => e.TransactionDate, "IX_TrTransaction_TransactionDate");

                entity.Property(e => e.IdTransaction).HasMaxLength(36);
                entity.Property(e => e.IdUser).HasMaxLength(5);
                entity.Property(e => e.InvoiceNo).HasMaxLength(20);
                entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.Userin).HasMaxLength(36);
                entity.Property(e => e.Userup).HasMaxLength(36);

                entity.HasOne(d => d.IdPaymentMethodNavigation).WithMany(p => p.TrTransactions)
                    .HasForeignKey(d => d.IdPaymentMethod)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.TrTransactions)
                    .HasForeignKey(d => d.IdUser)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<TrTransactionDetail>(entity =>
            {
                entity.HasKey(e => e.IdTransactionDetail);

                entity.ToTable("TrTransactionDetail");

                entity.HasIndex(e => e.IdProduct, "IX_TrTransactionDetail_IdProduct");

                entity.HasIndex(e => e.IdTransaction, "IX_TrTransactionDetail_IdTransaction");

                entity.Property(e => e.IdTransactionDetail).HasMaxLength(36);
                entity.Property(e => e.Discount).HasColumnType("decimal(5, 2)");
                entity.Property(e => e.IdProduct).HasMaxLength(36);
                entity.Property(e => e.IdTransaction).HasMaxLength(36);
                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.SubTotal).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.Userin).HasMaxLength(36);
                entity.Property(e => e.Userup).HasMaxLength(36);

                entity.HasOne(d => d.IdProductNavigation).WithMany(p => p.TrTransactionDetails)
                    .HasForeignKey(d => d.IdProduct)
                    .OnDelete(DeleteBehavior.ClientSetNull);

                entity.HasOne(d => d.IdTransactionNavigation).WithMany(p => p.TrTransactionDetails).HasForeignKey(d => d.IdTransaction);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }

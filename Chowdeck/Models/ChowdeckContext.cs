using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Chowdeck.Models;

public partial class ChowdeckContext : DbContext
{
    private readonly IConfiguration _config;
    public ChowdeckContext()
    {
    }

    public ChowdeckContext(DbContextOptions<ChowdeckContext> options, IConfiguration configuration)
        : base(options)
    {
        _config = configuration;
    }

    public virtual DbSet<Restaurant> Restaurants { get; set; }

    public virtual DbSet<RestaurantMenu> RestaurantMenus { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderTimeline> OrderTimelines { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(Environment.GetEnvironmentVariable("DB_CONNECTION_STRING"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<Restaurant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("restaurants_pkey");

            entity.ToTable("restaurants");

            entity.Property(e => e.Id)
                .HasMaxLength(255)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.AverageRating).HasColumnName("average_rating");
            entity.Property(e => e.Category).HasColumnName("category");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Image).HasColumnName("image");
            entity.Property(e => e.Lat)
                .HasMaxLength(255)
                .HasColumnName("lat");
            entity.Property(e => e.Lng)
                .HasMaxLength(255)
                .HasColumnName("lng");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Rank).HasColumnName("rank");
            entity.Property(e => e.Ratings).HasColumnName("ratings");
            entity.Ignore(m => m.RestaurantMenus);
        });

        modelBuilder.Entity<RestaurantMenu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("restaurant_menus_pkey");

            entity.ToTable("restaurant_menus");

            entity.Property(e => e.Id)
                .HasMaxLength(255)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Category).HasColumnName("category");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Image).HasColumnName("image");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Price).HasColumnName("price").HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RestaurantId)
                .HasMaxLength(255)
                .HasColumnName("restaurant_id");

            entity.HasOne(d => d.Restaurant).WithMany(p => p.RestaurantMenus)
                .HasForeignKey(d => d.RestaurantId)
                .HasConstraintName("restaurant_menus_restaurant_id_fkey");

            entity.Ignore(m => m.Restaurant);

        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Id)
                .HasMaxLength(255)
                .HasDefaultValueSql("uuid_generate_v4()");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(e => e.Id)
                .HasMaxLength(255)
                .HasDefaultValueSql("uuid_generate_v4()");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(e => e.Id)
                .HasMaxLength(255)
                .HasDefaultValueSql("uuid_generate_v4()");
        });

        modelBuilder.Entity<OrderTimeline>(entity =>
        {
            entity.Property(e => e.Id)
                .HasMaxLength(255)
                .HasDefaultValueSql("uuid_generate_v4()");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

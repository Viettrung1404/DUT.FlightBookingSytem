using System;
using System.Collections.Generic;
using FlightBookingWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace FlightBookingWeb.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Airplane> Airplanes { get; set; }

    public virtual DbSet<Airport> Airports { get; set; }

    public virtual DbSet<Baggage> Baggages { get; set; }

    public virtual DbSet<Flight> Flights { get; set; }

    public virtual DbSet<FlightRoute> FlightRoutes { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<MonthlyRevenue> MonthlyRevenues { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<TicketClass> TicketClasses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__Account__349DA586A6D1DBBF");

            entity.ToTable("Account");

            entity.Property(e => e.AccountId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("AccountID");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Airplane>(entity =>
        {
            entity.HasKey(e => e.AirplaneId).HasName("PK__Airplane__5ED76B855DCBD68E");

            entity.ToTable("Airplane");

            entity.Property(e => e.AirplaneId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("AirplaneID");
            entity.Property(e => e.AirplaneName).HasMaxLength(100);
            entity.Property(e => e.AirplaneType).HasMaxLength(50);
            entity.Property(e => e.Vipseats).HasColumnName("VIPSeats");
        });

        modelBuilder.Entity<Airport>(entity =>
        {
            entity.HasKey(e => e.AirportId).HasName("PK__Airport__E3DBE08AA32C5FA3");

            entity.ToTable("Airport");

            entity.Property(e => e.AirportId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("AirportID");
            entity.Property(e => e.AirportName).HasMaxLength(100);
            entity.Property(e => e.City).HasMaxLength(100);
        });

        modelBuilder.Entity<Baggage>(entity =>
        {
            entity.HasKey(e => e.BaggageId).HasName("PK__Baggage__01AFFC05C3619B0D");

            entity.ToTable("Baggage");

            entity.Property(e => e.BaggageId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("BaggageID");
            entity.Property(e => e.CustomerId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("CustomerID");
            entity.Property(e => e.TicketId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("TicketID");

            entity.HasOne(d => d.Customer).WithMany(p => p.Baggages)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__Baggage__Custome__4CA06362");

            entity.HasOne(d => d.Ticket).WithMany(p => p.Baggages)
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("FK__Baggage__TicketI__4D94879B");
        });

        modelBuilder.Entity<Flight>(entity =>
        {
            entity.HasKey(e => e.FlightId).HasName("PK__Flight__8A9E148E4510A9FB");

            entity.ToTable("Flight");

            entity.Property(e => e.FlightId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("FlightID");
            entity.Property(e => e.AirplaneId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("AirplaneID");
            entity.Property(e => e.RouteId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("RouteID");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Airplane).WithMany(p => p.Flights)
                .HasForeignKey(d => d.AirplaneId)
                .HasConstraintName("FK__Flight__Airplane__4222D4EF");

            entity.HasOne(d => d.Route).WithMany(p => p.Flights)
                .HasForeignKey(d => d.RouteId)
                .HasConstraintName("FK__Flight__RouteID__412EB0B6");
        });

        modelBuilder.Entity<FlightRoute>(entity =>
        {
            entity.HasKey(e => e.RouteId).HasName("PK__FlightRo__80979AAD7B5C1658");

            entity.ToTable("FlightRoute");

            entity.Property(e => e.RouteId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("RouteID");
            entity.Property(e => e.ArrivalAirportId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("ArrivalAirportID");
            entity.Property(e => e.DepartureAirportId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("DepartureAirportID");

            entity.HasOne(d => d.ArrivalAirport).WithMany(p => p.FlightRouteArrivalAirports)
                .HasForeignKey(d => d.ArrivalAirportId)
                .HasConstraintName("FK__FlightRou__Arriv__3C69FB99");

            entity.HasOne(d => d.DepartureAirport).WithMany(p => p.FlightRouteDepartureAirports)
                .HasForeignKey(d => d.DepartureAirportId)
                .HasConstraintName("FK__FlightRou__Depar__3B75D760");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PK__Invoice__D796AAD5B4BC4601");

            entity.ToTable("Invoice");

            entity.Property(e => e.InvoiceId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("InvoiceID");
            entity.Property(e => e.CustomerId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("CustomerID");
            entity.Property(e => e.IssueDate)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TicketId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("TicketID");

            entity.HasOne(d => d.Customer).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__Invoice__Custome__5070F446");

            entity.HasOne(d => d.Ticket).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("FK__Invoice__TicketI__5165187F");
        });

        modelBuilder.Entity<MonthlyRevenue>(entity =>
        {
            entity.HasKey(e => e.RevenueId).HasName("PK__MonthlyR__275F173D5FA14C62");

            entity.ToTable("MonthlyRevenue");

            entity.Property(e => e.RevenueId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("RevenueID");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("PK__Ticket__712CC6278E67B49A");

            entity.ToTable("Ticket");

            entity.Property(e => e.TicketId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("TicketID");
            entity.Property(e => e.ClassId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("ClassID");
            entity.Property(e => e.CustomerId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("CustomerID");
            entity.Property(e => e.FlightId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("FlightID");
            entity.Property(e => e.SeatPosition).HasMaxLength(10);
            entity.Property(e => e.TransitAirportId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("TransitAirportID");

            entity.HasOne(d => d.Class).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("FK__Ticket__ClassID__48CFD27E");

            entity.HasOne(d => d.Customer).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__Ticket__Customer__47DBAE45");

            entity.HasOne(d => d.Flight).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.FlightId)
                .HasConstraintName("FK__Ticket__FlightID__46E78A0C");

            entity.HasOne(d => d.TransitAirport).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.TransitAirportId)
                .HasConstraintName("FK__Ticket__TransitA__49C3F6B7");
        });

        modelBuilder.Entity<TicketClass>(entity =>
        {
            entity.HasKey(e => e.ClassId).HasName("PK__TicketCl__CB1927A0D61DE022");

            entity.ToTable("TicketClass");

            entity.Property(e => e.ClassId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("ClassID");
            entity.Property(e => e.ClassName).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

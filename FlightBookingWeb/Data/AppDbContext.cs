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

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<TicketClass> TicketClasses { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=BARONGER23\\SQLEXPRESS;Database=AirlineTicketManagement;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__Account__349DA5865E6FF514");

            entity.ToTable("Account");

            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(300)
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
            entity.HasKey(e => e.AirplaneId).HasName("PK__Airplane__5ED76B85A31FF0F0");

            entity.ToTable("Airplane");

            entity.Property(e => e.AirplaneId).HasColumnName("AirplaneID");
            entity.Property(e => e.AirplaneName).HasMaxLength(100);
            entity.Property(e => e.AirplaneType).HasMaxLength(50);
            entity.Property(e => e.Vipseats).HasColumnName("VIPSeats");
        });

        modelBuilder.Entity<Airport>(entity =>
        {
            entity.HasKey(e => e.AirportId).HasName("PK__Airport__E3DBE08A9E00D483");

            entity.ToTable("Airport");

            entity.Property(e => e.AirportId).HasColumnName("AirportID");
            entity.Property(e => e.AirportName).HasMaxLength(100);
            entity.Property(e => e.City).HasMaxLength(100);
        });

        modelBuilder.Entity<Baggage>(entity =>
        {
            entity.HasKey(e => e.BaggageId).HasName("PK__Baggage__01AFFC056534C994");

            entity.ToTable("Baggage");

            entity.Property(e => e.BaggageId).HasColumnName("BaggageID");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.TicketId).HasColumnName("TicketID");

            entity.HasOne(d => d.Customer).WithMany(p => p.Baggages)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__Baggage__Custome__5EBF139D");

            entity.HasOne(d => d.Ticket).WithMany(p => p.Baggages)
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("FK__Baggage__TicketI__5FB337D6");
        });

        modelBuilder.Entity<Flight>(entity =>
        {
            entity.HasKey(e => e.FlightId).HasName("PK__Flight__8A9E148E8683ABDE");

            entity.ToTable("Flight");

            entity.Property(e => e.FlightId).HasColumnName("FlightID");
            entity.Property(e => e.AirplaneId).HasColumnName("AirplaneID");
            entity.Property(e => e.RouteId).HasColumnName("RouteID");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Airplane).WithMany(p => p.Flights)
                .HasForeignKey(d => d.AirplaneId)
                .HasConstraintName("FK__Flight__Airplane__5441852A");

            entity.HasOne(d => d.Route).WithMany(p => p.Flights)
                .HasForeignKey(d => d.RouteId)
                .HasConstraintName("FK__Flight__RouteID__534D60F1");
        });

        modelBuilder.Entity<FlightRoute>(entity =>
        {
            entity.HasKey(e => e.RouteId).HasName("PK__FlightRo__80979AADBFD2585F");

            entity.ToTable("FlightRoute");

            entity.Property(e => e.RouteId).HasColumnName("RouteID");
            entity.Property(e => e.ArrivalAirportId).HasColumnName("ArrivalAirportID");
            entity.Property(e => e.DepartureAirportId).HasColumnName("DepartureAirportID");

            entity.HasOne(d => d.ArrivalAirport).WithMany(p => p.FlightRouteArrivalAirports)
                .HasForeignKey(d => d.ArrivalAirportId)
                .HasConstraintName("FK__FlightRou__Arriv__4E88ABD4");

            entity.HasOne(d => d.DepartureAirport).WithMany(p => p.FlightRouteDepartureAirports)
                .HasForeignKey(d => d.DepartureAirportId)
                .HasConstraintName("FK__FlightRou__Depar__4D94879B");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PK__Invoice__D796AAD575D048B8");

            entity.ToTable("Invoice");

            entity.Property(e => e.InvoiceId).HasColumnName("InvoiceID");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.IssueDate)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TicketId).HasColumnName("TicketID");

            entity.HasOne(d => d.Customer).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__Invoice__Custome__628FA481");

            entity.HasOne(d => d.Ticket).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("FK__Invoice__TicketI__6383C8BA");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("PK__Ticket__712CC627E19D390C");

            entity.ToTable("Ticket");

            entity.Property(e => e.TicketId).HasColumnName("TicketID");
            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.FlightId).HasColumnName("FlightID");
            entity.Property(e => e.SeatPosition).HasMaxLength(10);
            entity.Property(e => e.TransitAirportId).HasColumnName("TransitAirportID");

            entity.HasOne(d => d.Class).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("FK__Ticket__ClassID__5AEE82B9");

            entity.HasOne(d => d.Customer).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK__Ticket__Customer__59FA5E80");

            entity.HasOne(d => d.Flight).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.FlightId)
                .HasConstraintName("FK__Ticket__FlightID__59063A47");

            entity.HasOne(d => d.TransitAirport).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.TransitAirportId)
                .HasConstraintName("FK__Ticket__TransitA__5BE2A6F2");
        });

        modelBuilder.Entity<TicketClass>(entity =>
        {
            entity.HasKey(e => e.ClassId).HasName("PK__TicketCl__CB1927A05758410A");

            entity.ToTable("TicketClass");

            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.ClassName).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

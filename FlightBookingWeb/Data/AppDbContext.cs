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

    public virtual DbSet<FlightSchedule> FlightSchedules { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Models.Route> Routes { get; set; }

    public virtual DbSet<Seat> Seats { get; set; }

    public virtual DbSet<SeatBooking> SeatBookings { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<VwAvailableSeat> VwAvailableSeats { get; set; }

    public virtual DbSet<VwFlightDetail> VwFlightDetails { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=BARONGER23\\SQLEXPRESS;Database=OnlineBookingAirLine;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__Account__349DA5868F8E6AD6");

            entity.ToTable("Account");

            entity.HasIndex(e => e.Username, "UQ__Account__536C85E470B2C223").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Account__A9D105341B7701DA").IsUnique();

            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Customer");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Airplane>(entity =>
        {
            entity.HasKey(e => e.AirplaneId).HasName("PK__Airplane__5ED76B85699F44F1");

            entity.ToTable("Airplane", tb => tb.HasTrigger("tr_Airplane_AfterInsert"));

            entity.Property(e => e.AirplaneId).HasColumnName("AirplaneID");
            entity.Property(e => e.AirplaneName).HasMaxLength(50);
            entity.Property(e => e.AirplaneType).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
        });

        modelBuilder.Entity<Airport>(entity =>
        {
            entity.HasKey(e => e.AirportId).HasName("PK__Airport__E3DBE08A0AF80F82");

            entity.ToTable("Airport");

            entity.HasIndex(e => e.AirportCode, "UQ__Airport__4B67735328AEB255").IsUnique();

            entity.Property(e => e.AirportId).HasColumnName("AirportID");
            entity.Property(e => e.AirportCode)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.AirportName).HasMaxLength(100);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.Country).HasMaxLength(50);
        });

        modelBuilder.Entity<Baggage>(entity =>
        {
            entity.HasKey(e => e.BaggageId).HasName("PK__Baggage__01AFFC05FF73FD83");

            entity.ToTable("Baggage");

            entity.Property(e => e.BaggageId).HasColumnName("BaggageID");
            entity.Property(e => e.Fee).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Checked");
            entity.Property(e => e.TicketId).HasColumnName("TicketID");
            entity.Property(e => e.Weight).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Ticket).WithMany(p => p.Baggages)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Baggage__TicketI__60A75C0F");
        });

        modelBuilder.Entity<Flight>(entity =>
        {
            entity.HasKey(e => e.FlightId).HasName("PK__Flight__8A9E148EA34D2DB5");

            entity.ToTable("Flight");

            entity.Property(e => e.FlightId).HasColumnName("FlightID");
            entity.Property(e => e.ArrivalDateTime).HasColumnType("datetime");
            entity.Property(e => e.DepartureDateTime).HasColumnType("datetime");
            entity.Property(e => e.ScheduleId).HasColumnName("ScheduleID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Scheduled");

            entity.HasOne(d => d.Schedule).WithMany(p => p.Flights)
                .HasForeignKey(d => d.ScheduleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Flight__Schedule__4CA06362");
        });

        modelBuilder.Entity<FlightSchedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("PK__FlightSc__9C8A5B69F3B1E376");

            entity.ToTable("FlightSchedule");

            entity.Property(e => e.ScheduleId).HasColumnName("ScheduleID");
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.AirplaneId).HasColumnName("AirplaneID");
            entity.Property(e => e.Frequency)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RouteId).HasColumnName("RouteID");

            entity.HasOne(d => d.Airplane).WithMany(p => p.FlightSchedules)
                .HasForeignKey(d => d.AirplaneId)
                .HasConstraintName("FK_FlightSchedule_Airplane");

            entity.HasOne(d => d.Route).WithMany(p => p.FlightSchedules)
                .HasForeignKey(d => d.RouteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FlightSch__Route__48CFD27E");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PK__Invoice__D796AAD5AE3F557D");

            entity.ToTable("Invoice");

            entity.HasIndex(e => e.InvoiceNumber, "UQ__Invoice__D776E9816F72F383").IsUnique();

            entity.Property(e => e.InvoiceId).HasColumnName("InvoiceID");
            entity.Property(e => e.InvoiceNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.IssueDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaymentId).HasColumnName("PaymentID");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.Payment).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.PaymentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invoice__Payment__6A30C649");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payment__9B556A584924B919");

            entity.ToTable("Payment");

            entity.Property(e => e.PaymentId).HasColumnName("PaymentID");
            entity.Property(e => e.Amount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Completed");
            entity.Property(e => e.TicketId).HasColumnName("TicketID");
            entity.Property(e => e.TransactionId)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("TransactionID");

            entity.HasOne(d => d.Ticket).WithMany(p => p.Payments)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payment__TicketI__656C112C");
        });

        modelBuilder.Entity<Models.Route>(entity =>
        {
            entity.HasKey(e => e.RouteId).HasName("PK__Route__80979AADDABEAFAF");

            entity.ToTable("Route");

            entity.Property(e => e.RouteId).HasColumnName("RouteID");
            entity.Property(e => e.ArrivalAirportId).HasColumnName("ArrivalAirportID");
            entity.Property(e => e.DepartureAirportId).HasColumnName("DepartureAirportID");
            entity.Property(e => e.BasePrice).HasColumnType("decimal(18, 0)");

            entity.HasOne(d => d.ArrivalAirport).WithMany(p => p.RouteArrivalAirports)
                .HasForeignKey(d => d.ArrivalAirportId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Route__ArrivalAi__44FF419A");

            entity.HasOne(d => d.DepartureAirport).WithMany(p => p.RouteDepartureAirports)
                .HasForeignKey(d => d.DepartureAirportId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Route__Departure__440B1D61");
        });

        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasKey(e => e.SeatId).HasName("PK__Seat__311713D320E3B016");

            entity.ToTable("Seat");

            entity.HasIndex(e => new { e.AirplaneId, e.SeatNumber }, "UQ_Seat_Airplane").IsUnique();

            entity.Property(e => e.SeatId).HasColumnName("SeatID");
            entity.Property(e => e.AirplaneId).HasColumnName("AirplaneID");
            entity.Property(e => e.IsEmergencyExit).HasDefaultValue(false);
            entity.Property(e => e.SeatClass)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SeatNumber)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.SeatType)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Airplane).WithMany(p => p.Seats)
                .HasForeignKey(d => d.AirplaneId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Seat__AirplaneID__412EB0B6");
        });

        modelBuilder.Entity<SeatBooking>(entity =>
        {
            entity.HasKey(e => e.SeatBookingId).HasName("PK__SeatBook__7FC8032BE18403B7");

            entity.ToTable("SeatBooking");

            entity.Property(e => e.BookingDate).HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.SeatBookings)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("FK__SeatBooki__Accou__151B244E");

            entity.HasOne(d => d.Flight).WithMany(p => p.SeatBookings)
                .HasForeignKey(d => d.FlightId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SeatBooki__Fligh__123EB7A3");

            entity.HasOne(d => d.Seat).WithMany(p => p.SeatBookings)
                .HasForeignKey(d => d.SeatId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SeatBooki__SeatI__1332DBDC");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("PK__Ticket__712CC62757798DCE");

            entity.ToTable("Ticket", tb => tb.HasTrigger("tr_Ticket_BeforeInsert"));

            entity.HasIndex(e => e.RelatedTicketId, "IX_Ticket_RelatedTicketID");

            entity.Property(e => e.TicketId).HasColumnName("TicketID");
            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.BookingDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FlightId).HasColumnName("FlightID");
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.RelatedTicketId).HasColumnName("RelatedTicketID");
            entity.Property(e => e.SeatId).HasColumnName("SeatID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Confirmed");
            entity.Property(e => e.TicketType)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("OneWay");

            entity.HasOne(d => d.Account).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ticket__AccountI__5AEE82B9");

            entity.HasOne(d => d.Flight).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.FlightId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ticket__FlightID__59FA5E80");

            entity.HasOne(d => d.RelatedTicket).WithMany(p => p.InverseRelatedTicket)
                .HasForeignKey(d => d.RelatedTicketId)
                .HasConstraintName("FK_Ticket_RelatedTicket");

            entity.HasOne(d => d.Seat).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.SeatId)
                .HasConstraintName("FK__Ticket__SeatID__5CD6CB2B");
        });

        modelBuilder.Entity<VwAvailableSeat>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_AvailableSeats");

            entity.Property(e => e.FlightId).HasColumnName("FlightID");
            entity.Property(e => e.Price).HasColumnType("numeric(13, 3)");
            entity.Property(e => e.SeatClass)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SeatId).HasColumnName("SeatID");
            entity.Property(e => e.SeatNumber)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.SeatType)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<VwFlightDetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_FlightDetails");

            entity.Property(e => e.AirplaneName).HasMaxLength(50);
            entity.Property(e => e.AirplaneType).HasMaxLength(50);
            entity.Property(e => e.ArrivalAirport)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.ArrivalAirportName).HasMaxLength(100);
            entity.Property(e => e.ArrivalCity).HasMaxLength(50);
            entity.Property(e => e.ArrivalDateTime).HasColumnType("datetime");
            entity.Property(e => e.DepartureAirport)
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.DepartureAirportName).HasMaxLength(100);
            entity.Property(e => e.DepartureCity).HasMaxLength(50);
            entity.Property(e => e.DepartureDateTime).HasColumnType("datetime");
            entity.Property(e => e.FlightId).HasColumnName("FlightID");
            entity.Property(e => e.RouteId).HasColumnName("RouteID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

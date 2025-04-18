using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlightBookingWeb.Migrations
{
    /// <inheritdoc />
    public partial class CreateDataBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Account",
                columns: table => new
                {
                    AccountID = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Username = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Password = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    PhoneNumber = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Gender = table.Column<bool>(type: "bit", nullable: true),
                    Role = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Account__349DA586A6D1DBBF", x => x.AccountID);
                });

            migrationBuilder.CreateTable(
                name: "Airplane",
                columns: table => new
                {
                    AirplaneID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    AirplaneName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AirplaneType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EconomySeats = table.Column<int>(type: "int", nullable: true),
                    VIPSeats = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Airplane__5ED76B855DCBD68E", x => x.AirplaneID);
                });

            migrationBuilder.CreateTable(
                name: "Airport",
                columns: table => new
                {
                    AirportID = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    AirportName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Airport__E3DBE08AA32C5FA3", x => x.AirportID);
                });

            migrationBuilder.CreateTable(
                name: "MonthlyRevenue",
                columns: table => new
                {
                    RevenueID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Revenue = table.Column<int>(type: "int", nullable: true),
                    TicketsSold = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MonthlyR__275F173D5FA14C62", x => x.RevenueID);
                });

            migrationBuilder.CreateTable(
                name: "TicketClass",
                columns: table => new
                {
                    ClassID = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    ClassName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TicketCl__CB1927A0D61DE022", x => x.ClassID);
                });

            migrationBuilder.CreateTable(
                name: "FlightRoute",
                columns: table => new
                {
                    RouteID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    DepartureAirportID = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    ArrivalAirportID = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__FlightRo__80979AAD7B5C1658", x => x.RouteID);
                    table.ForeignKey(
                        name: "FK__FlightRou__Arriv__3C69FB99",
                        column: x => x.ArrivalAirportID,
                        principalTable: "Airport",
                        principalColumn: "AirportID");
                    table.ForeignKey(
                        name: "FK__FlightRou__Depar__3B75D760",
                        column: x => x.DepartureAirportID,
                        principalTable: "Airport",
                        principalColumn: "AirportID");
                });

            migrationBuilder.CreateTable(
                name: "Flight",
                columns: table => new
                {
                    FlightID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    RouteID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    AirplaneID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DepartureTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    ArrivalTime = table.Column<TimeOnly>(type: "time", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Flight__8A9E148E4510A9FB", x => x.FlightID);
                    table.ForeignKey(
                        name: "FK__Flight__Airplane__4222D4EF",
                        column: x => x.AirplaneID,
                        principalTable: "Airplane",
                        principalColumn: "AirplaneID");
                    table.ForeignKey(
                        name: "FK__Flight__RouteID__412EB0B6",
                        column: x => x.RouteID,
                        principalTable: "FlightRoute",
                        principalColumn: "RouteID");
                });

            migrationBuilder.CreateTable(
                name: "Ticket",
                columns: table => new
                {
                    TicketID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    SeatPosition = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Price = table.Column<int>(type: "int", nullable: true),
                    FlightID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    CustomerID = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    ClassID = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    TransitAirportID = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Ticket__712CC6278E67B49A", x => x.TicketID);
                    table.ForeignKey(
                        name: "FK__Ticket__ClassID__48CFD27E",
                        column: x => x.ClassID,
                        principalTable: "TicketClass",
                        principalColumn: "ClassID");
                    table.ForeignKey(
                        name: "FK__Ticket__Customer__47DBAE45",
                        column: x => x.CustomerID,
                        principalTable: "Account",
                        principalColumn: "AccountID");
                    table.ForeignKey(
                        name: "FK__Ticket__FlightID__46E78A0C",
                        column: x => x.FlightID,
                        principalTable: "Flight",
                        principalColumn: "FlightID");
                    table.ForeignKey(
                        name: "FK__Ticket__TransitA__49C3F6B7",
                        column: x => x.TransitAirportID,
                        principalTable: "Airport",
                        principalColumn: "AirportID");
                });

            migrationBuilder.CreateTable(
                name: "Baggage",
                columns: table => new
                {
                    BaggageID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    CustomerID = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    TicketID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    ExtraFee = table.Column<int>(type: "int", nullable: true),
                    Weight = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Baggage__01AFFC05C3619B0D", x => x.BaggageID);
                    table.ForeignKey(
                        name: "FK__Baggage__Custome__4CA06362",
                        column: x => x.CustomerID,
                        principalTable: "Account",
                        principalColumn: "AccountID");
                    table.ForeignKey(
                        name: "FK__Baggage__TicketI__4D94879B",
                        column: x => x.TicketID,
                        principalTable: "Ticket",
                        principalColumn: "TicketID");
                });

            migrationBuilder.CreateTable(
                name: "Invoice",
                columns: table => new
                {
                    InvoiceID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    CustomerID = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    IssueDate = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    TicketID = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Invoice__D796AAD5B4BC4601", x => x.InvoiceID);
                    table.ForeignKey(
                        name: "FK__Invoice__Custome__5070F446",
                        column: x => x.CustomerID,
                        principalTable: "Account",
                        principalColumn: "AccountID");
                    table.ForeignKey(
                        name: "FK__Invoice__TicketI__5165187F",
                        column: x => x.TicketID,
                        principalTable: "Ticket",
                        principalColumn: "TicketID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Baggage_CustomerID",
                table: "Baggage",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_Baggage_TicketID",
                table: "Baggage",
                column: "TicketID");

            migrationBuilder.CreateIndex(
                name: "IX_Flight_AirplaneID",
                table: "Flight",
                column: "AirplaneID");

            migrationBuilder.CreateIndex(
                name: "IX_Flight_RouteID",
                table: "Flight",
                column: "RouteID");

            migrationBuilder.CreateIndex(
                name: "IX_FlightRoute_ArrivalAirportID",
                table: "FlightRoute",
                column: "ArrivalAirportID");

            migrationBuilder.CreateIndex(
                name: "IX_FlightRoute_DepartureAirportID",
                table: "FlightRoute",
                column: "DepartureAirportID");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_CustomerID",
                table: "Invoice",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_TicketID",
                table: "Invoice",
                column: "TicketID");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_ClassID",
                table: "Ticket",
                column: "ClassID");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_CustomerID",
                table: "Ticket",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_FlightID",
                table: "Ticket",
                column: "FlightID");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_TransitAirportID",
                table: "Ticket",
                column: "TransitAirportID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Baggage");

            migrationBuilder.DropTable(
                name: "Invoice");

            migrationBuilder.DropTable(
                name: "MonthlyRevenue");

            migrationBuilder.DropTable(
                name: "Ticket");

            migrationBuilder.DropTable(
                name: "TicketClass");

            migrationBuilder.DropTable(
                name: "Account");

            migrationBuilder.DropTable(
                name: "Flight");

            migrationBuilder.DropTable(
                name: "Airplane");

            migrationBuilder.DropTable(
                name: "FlightRoute");

            migrationBuilder.DropTable(
                name: "Airport");
        }
    }
}

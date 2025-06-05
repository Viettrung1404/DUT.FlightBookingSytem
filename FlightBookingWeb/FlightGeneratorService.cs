using FlightBookingWeb.Data;
using FlightBookingWeb.Models;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
public class FlightStatusUpdater : BackgroundService
{
    private readonly IServiceProvider _services;

    public FlightStatusUpdater(IServiceProvider services)
    {
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var now = DateTime.Now;

            var flights = context.Flights
                .Where(f => f.Status != "Đã hoàn thành" && f.Status != "Đã hủy")
                .ToList();

            foreach (var flight in flights)
            {
                if (now >= flight.ArrivalDateTime)
                {
                    flight.Status = "Đã hoàn thành";
                }
                else if (now >= flight.DepartureDateTime)
                {
                    flight.Status = "Đang bay";
                }
                else
                {
                    flight.Status = "Chưa cất cánh";
                }
            }

            context.SaveChanges();

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}

public class FlightGeneratorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public FlightGeneratorService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var today = DateTime.Now;
                var endDate = today.AddDays(60);
                var schedules = context.FlightSchedules
                                       .Where(s => s.Status == true && s.Active == true)
                                       .ToList();

                foreach (var schedule in schedules)
                {
                    // Bỏ qua nếu tần suất không hợp lệ
                    if (schedule.Frequency < 0)
                        continue;

                    var departure = schedule.DepartureTime;
                    var arrivalTime = departure + schedule.ArrivalTime.ToTimeSpan();

                    // 👉 Nếu Frequency = 0: chỉ tạo chuyến bay một lần duy nhất
                    if (schedule.Frequency == 0)
                    {
                        bool existed = context.Flights.Any(f =>
                            f.ScheduleId == schedule.ScheduleId &&
                            f.DepartureDateTime == departure);

                        if (!existed)
                        {
                            context.Flights.Add(new Flight
                            {
                                ScheduleId = schedule.ScheduleId,
                                DepartureDateTime = departure,
                                ArrivalDateTime = arrivalTime,
                                Status = "Chưa cất cánh"
                            });

                            context.SaveChanges();
                        }

                        continue; // ❗ Quan trọng: bỏ qua xử lý bên dưới
                    }

                    // 👉 Nếu Frequency > 0: tạo các chuyến bay lặp trong 60 ngày tới
                    // Đẩy ngày khởi hành về sau nếu quá khứ
                    while (departure < today)
                    {
                        departure = departure.AddDays(schedule.Frequency);
                    }

                    for (var ngay = departure; ngay <= endDate; ngay = ngay.AddDays(schedule.Frequency))
                    {
                        var arrival = ngay + schedule.ArrivalTime.ToTimeSpan();

                        bool existed = context.Flights.Any(f =>
                            f.ScheduleId == schedule.ScheduleId &&
                            f.DepartureDateTime == ngay);

                        if (!existed)
                        {
                            context.Flights.Add(new Flight
                            {
                                ScheduleId = schedule.ScheduleId,
                                DepartureDateTime = ngay,
                                ArrivalDateTime = arrival,
                                Status = "Chưa cất cánh"
                            });

                            context.SaveChanges();
                        }
                    }
                }
            }

            await Task.Delay(1000, stoppingToken); // đợi 1 giây rồi kiểm tra tiếp
        }
    }
}
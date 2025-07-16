
using Microsoft.AspNetCore.Cors.Infrastructure;
using Serilog;
using TaskControl.Core.AppSettings;
using TaskControl.Core.SharedInterfaces;
using TaskControl.InformationModule.Application.DTOs;
using TaskControl.InformationModule.DataAccess.Infrastructure;
using TaskControl.InformationModule.DataAccess.Interface;
using TaskControl.InformationModule.DataAccess.Repositories;
using TaskControl.InformationModule.Services;
using TaskControl.InventoryModule.DataAccess.Infrastructure;
using TaskControl.InventoryModule.DataAccess.Interface;

namespace TaskControl.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

            builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection(AppSettings.SectionName));

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();



            //builder.Services.AddScoped<IInventoryDataConnection, InventoryDataConnection>();
            builder.Services.AddScoped<IInformationDataConnection, InformationDataConnection>();
            builder.Services.AddScoped<IBranchRepository, BranchRepository>();
            builder.Services.AddScoped<IService<BranchDto>, BranchService>();



            var app = builder.Build();
            try
            {
                Log.Information("������ ���������� {AppName} ������ {AppVersion}",
                    builder.Configuration["AppSettings:AppName"],
                    builder.Configuration["AppSettings:AppVersion"]);

                app.UseSwagger();
                app.UseSwaggerUI();


                app.UseHttpsRedirection();

                app.UseAuthorization();


                app.MapControllers();
                Log.Information("���������� ��������� � ������ � ������ �� ����� {Port}",
    builder.Configuration["urls"] ?? "default");
                Console.WriteLine("����");
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "����������� ������ ��� ������� ����������");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}

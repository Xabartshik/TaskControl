using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskControl.Core.Shared.SharedInterfaces;
using TaskControl.InformationModule.Application.DTOs;
using TaskControl.InformationModule.Application.Services;
using TaskControl.InformationModule.DAL.Repositories;
using TaskControl.InformationModule.DataAccess.Infrastructure;
using TaskControl.InformationModule.DataAccess.Interface;
using TaskControl.InformationModule.DataAccess.Repositories;
using TaskControl.InformationModule.Services;
using TaskControl.InventoryModule.Application.DTOs;
using TaskControl.InventoryModule.Application.Services;
using TaskControl.InventoryModule.DAL.Repositories;
using TaskControl.InventoryModule.DataAccess.Infrastructure;
using TaskControl.InventoryModule.DataAccess.Interface;
using TaskControl.OrderModule.Application.DTOs;
using TaskControl.OrderModule.Application.Services;
using TaskControl.OrderModule.DAL.Repositories;
using TaskControl.OrderModule.DataAccess.Infrastructure;
using TaskControl.OrderModule.DataAccess.Interface;
using TaskControl.OrderModule.Services;
using TaskControl.ReportsModule.Application.DTOs;
using TaskControl.ReportsModule.Application.Services;
using TaskControl.ReportsModule.DataAccess.Infrastructure;
using TaskControl.ReportsModule.DataAccess.Interface;
using TaskControl.ReportsModule.DataAccess.Repositories;
using TaskControl.TaskModule.Application.DTOs;
using TaskControl.TaskModule.Application.Interface;
using TaskControl.TaskModule.Application.Services;
using TaskControl.TaskModule.DAL.Repositories;
using TaskControl.TaskModule.DataAccess.Infrastructure;
using TaskControl.TaskModule.DataAccess.Interface;
using TaskControl.TaskModule.DataAccess.Repositories;

namespace TaskControl.Core.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServicesGroup(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddServicesInfoModule();
            services.AddServicesInvModule();
            services.AddServicesOrderModule();
            services.AddServicesReportsModule();
            services.AddServicesTaskModule();

            services.AddAuthenticationServices(configuration);

            return services;
        }


        private static IServiceCollection AddServicesInfoModule(this IServiceCollection services)
        {
            services.AddScoped<IInformationDataConnection, InformationDataConnection>();
            services.AddScoped<IBranchRepository, BranchRepository>();
            services.AddScoped<IService<BranchDto>, BranchService>();

            services.AddScoped<ICheckIOEmployeeRepository, CheckIOEmployeeRepository>();
            services.AddScoped<IService<CheckIOEmployeeDto>, CheckIOEmployeeService>();

            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IService<EmployeeDto>, EmployeeService>();

            services.AddScoped<IItemRepository, ItemRepository>();
            services.AddScoped<IService<ItemDto>, ItemService>();

            services.AddScoped<ActiveEmployeeService>();

            return services;
        }

        private static IServiceCollection AddServicesInvModule(this IServiceCollection services)
        {
            services.AddScoped<IInventoryDataConnection, InventoryDataConnection>();
            services.AddScoped<IItemMovementRepository, ItemMovementRepository>();
            services.AddScoped<IService<ItemMovementDto>, ItemMovementService>();

            services.AddScoped<IItemPositionRepository, ItemPositionRepository>();
            services.AddScoped<IService<ItemPositionDto>, ItemPositionService>();

            services.AddScoped<IItemStatusRepository, ItemStatusRepository>();
            services.AddScoped<IService<ItemStatusDto>, ItemStatusService>();

            services.AddScoped<IOrderPositionRepository, OrderPositionRepository>();
            services.AddScoped<IService<OrderPositionDto>, OrderPositionService>();

            services.AddScoped<IOrderPositionRepository, OrderPositionRepository>();
            services.AddScoped<IService<OrderPositionDto>, OrderPositionService>();

            return services;
        }

        private static IServiceCollection AddServicesReportsModule(this IServiceCollection services)
        {
            services.AddScoped<IReportDataConnection, ReportDataConnection>();
            services.AddScoped<IRawEventRepository, RawEventRepository>();
            services.AddScoped<IService<RawEventDto>, RawEventService>();

            return services;
        }

        private static IServiceCollection AddServicesOrderModule(this IServiceCollection services)
        {
            services.AddScoped<IOrderDataConnection, OrderDataConnection>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IService<OrderDto>, OrderService>();

            services.AddScoped<IOrderPositionRepository, OrderPositionRepository>();
            services.AddScoped<IService<OrderPositionDto>, OrderPositionService>();

            return services;
        }

        private static IServiceCollection AddServicesTaskModule(this IServiceCollection services)
        {
            services.AddScoped<ITaskDataConnection, TaskDataConnection>();
            services.AddScoped<IActiveTaskRepository, ActiveTaskRepository>();
            services.AddScoped<IService<BaseTaskDto>, BaseTaskService>();

            services.AddScoped<ITaskAssignationRepository, TaskAssignationRepository>();
            services.AddScoped<IService<TaskAssignationDto>, TaskAssignationService>();

            services.AddScoped<IInventoryProcessService, InventoryProcessService>();
            services.AddScoped<IDiscrepancyManagementService, DiscrepancyManagementService>();
            services.AddScoped<IInventoryReportService, InventoryReportService>();

            services.AddScoped<IInventoryAssignmentRepository, InventoryAssignmentRepository>();
            services.AddScoped<IInventoryAssignmentLineRepository, InventoryAssignmentLineRepository>();
            services.AddScoped<IInventoryDiscrepancyRepository, InventoryDiscrepancyRepository>();
            services.AddScoped<IInventoryStatisticsRepository, InventoryStatisticsRepository>();

            services.AddScoped<IPositionCellRepository, PositionCellRepository>();
            services.AddScoped<PositionDetailsService>();

            services.AddScoped<IMobileAppUserRepository, MobileAppUserRepository>();
            services.AddScoped<IMobileAppUserService, MobileAppUserService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();

            return services;
        }

        private static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration cfg)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var jwt = cfg.GetSection("Jwt");
                    var key = Encoding.UTF8.GetBytes(jwt["Key"]!);

                    options.TokenValidationParameters = new()
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwt["Issuer"],
                        ValidateAudience = true,
                        ValidAudience = jwt["Audience"],
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(30)
                    };
                });

            services.AddAuthorization();
            return services;
        }

    }
}

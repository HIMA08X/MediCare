using MediCare.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using MediCare.Data;
using MediCare.Services;
namespace MediCare.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // el connection string yafnan
            /*var connectionString = builder.Configuration.GetConnectionString("MDCDefaultConnection") ?? throw new InvalidOperationException("Connection string 'MDCDefaultConnection' was not found.");
            builder.Services.AddDbContext<MediCareDbContext>(options => options.UseSqlServer(connectionString));*/
            builder.Services.AddDataServices(builder.Configuration);
            builder.Services.AddApplicationServices();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}

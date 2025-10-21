
using MathSlidesBe.BaseRepo;
using Microsoft.EntityFrameworkCore;

namespace MathSlidesBe
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();
            //);
            builder.Services.AddDbContext<MathSlidesDbContext>();
            builder.Services.AddScoped(typeof(IRepository<>),typeof(Repository<>));
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddAuthentication("MyCookieAuth")
                .AddCookie("MyCookieAuth",options =>
                {
                    options.Cookie.Name = "myapp_auth";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.None;
                    options.LoginPath = "/auth/login";
                    options.AccessDeniedPath = "/auth/denied";
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                });
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

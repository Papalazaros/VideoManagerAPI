using AutoMapper;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text.Json.Serialization;
using VideoManager.Middleware;
using VideoManager.Services;

namespace VideoManager
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            services.AddDbContext<VideoManagerDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
            });

            services.AddMemoryCache();

            services.AddHttpClient<IAuth0Service, Auth0Service>();

            services.AddScoped<IUserService, UserService>();

            services.AddScoped<IEncoder, FfmpegService>();

            services.AddScoped<IFileService, FileService>();

            services.AddScoped<IUserVideoService, UserVideoService>();

            services.AddScoped<IVideoManagerService, VideoManagerService>();

            services.AddScoped<IRoomService, RoomService>();

            services.AddHostedService<EncodingService>();

            services.AddHostedService<VideoBackgroundService>();

            services.AddAutoMapper(typeof(Startup));

            services.AddSignalR().AddJsonProtocol(opts => opts.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            services.AddCors(o => o.AddPolicy("CorsPolicy", builder => {
                builder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithOrigins("http://localhost:8080", "http://localhost");
            }));

            services.Configure<FormOptions>(options =>
            {
                options.ValueCountLimit = 50;
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = long.MaxValue;
            });

            services.AddControllers()
                .AddFluentValidation(opt => opt.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly()))
                .AddJsonOptions(opts => opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            services.AddSwaggerGen(x => x.SwaggerDoc("v1", new OpenApiInfo { Title = "VideoManager" }));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();

            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "VideoManager"));

            app.UseCors("CorsPolicy");

            app.UseResponseCompression();

            //app.UseHttpsRedirection();

            app.UseWhen(context => !context.Request.Path.StartsWithSegments("/videoHub"),
                app => app.UseMiddleware(typeof(AuthenticationMiddleware)));

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<VideoHub>("/videoHub");
                endpoints.MapControllers();
            });
        }
    }
}

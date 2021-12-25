using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using CloudinaryDotNet;
using api.Models;
using api.Services;
using api.Interfaces;
using System.IO;
using System.Reflection;

#pragma warning disable 1591 

namespace api
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

            services.Configure<BlogDatabaseSettings>(
            Configuration.GetSection(nameof(BlogDatabaseSettings)));
            services.Configure<ApiSettings>(Configuration.GetSection(nameof(ApiSettings)));

            services.AddSingleton<IBlogDatabaseSettings>(sp =>
                sp.GetRequiredService<IOptions<BlogDatabaseSettings>>().Value);
            services.AddSingleton<IApiSettings>(sp =>
                sp.GetRequiredService<IOptions<ApiSettings>>().Value);
            services.PostConfigure<BlogDatabaseSettings>(opts =>
            {
                opts.ConnectionString = Environment.GetEnvironmentVariable("API_DB_URL");
            });

            services.PostConfigure<ApiSettings>(opts =>
            {
                opts.ApiKey = Environment.GetEnvironmentVariable("API_KEY");
                opts.CloudinaryName = Environment.GetEnvironmentVariable("CLOUDINARY_NAME");
                opts.CloudinaryKey = Environment.GetEnvironmentVariable("CLOUDINARY_KEY");
                opts.CloudinarySecret = Environment.GetEnvironmentVariable("CLOUDINARY_SECRET");
                opts.CloudinaryUrl = Environment.GetEnvironmentVariable("CLOUDINARY_URL");
            });

            var cloudinaryName = Environment.GetEnvironmentVariable("CLOUDINARY_NAME");
            var cloudinaryKey = Environment.GetEnvironmentVariable("CLOUDINARY_KEY");
            var cloudinarySecret = Environment.GetEnvironmentVariable("CLOUDINARY_SECRET");


            if (new[] { cloudinaryName, cloudinaryKey, cloudinarySecret }.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("Please specify Cloudinary account details!");
            }

            services.AddSingleton<IMongoService, MongoService>();
            services.AddSingleton<DbCrudService<IEntityBase>>();
            services.AddSingleton<PostService>();
            services.AddSingleton<CategoryService>();
            services.AddSingleton<SubscriberService>();
            services.AddSingleton<ImageService>();
            services.AddSingleton(new Cloudinary(new Account(cloudinaryName, cloudinaryKey, cloudinarySecret)));
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "api", Version = "v1" });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blog Api v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

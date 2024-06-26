
using MemberWebServer.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

namespace MemberWebServer
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);
            

            var configuration = new ConfigurationBuilder()
				.SetBasePath(builder.Environment.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.Build();

			builder.Services.AddControllers();
			builder.Services.AddDbContext<LoginContext>(opt =>
				opt.UseInMemoryDatabase("LoginList"));
			builder.Services.AddDbContext<RegistrationContext>(opt =>
				opt.UseInMemoryDatabase("RegistrationList"));
            builder.Services.AddDbContext<MemberContext>(opt =>
                opt.UseInMemoryDatabase("MemberList"));

            builder.Services.AddAuthentication(
				opt =>
				{
					opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
					opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

                })
				.AddJwtBearer(options =>
				{
					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuer = true,
						ValidateAudience = true,
						ValidateLifetime = true,
						ValidateIssuerSigningKey = true,
						ValidIssuer = configuration["JwtSettings:Issuer"],
						ValidAudience = configuration["JwtSettings:Audience"],
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"]))
					};
					options.Events = new JwtBearerEvents
					{
						OnChallenge = context =>
						{
							context.HandleResponse();
							context.Response.StatusCode = 401;
							context.Response.ContentType = "application/json";
							return context.Response.WriteAsync(new
							{
								context.Response.StatusCode,
								Message = "로그인을 해주세요."
							}.ToString());
						}
					};
				});

			// Add services to the container.

			builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(options =>
			{
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer"
                });

				options.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{
						new OpenApiSecurityScheme
						{
							Reference = new OpenApiReference
							{
								Type = ReferenceType.SecurityScheme,
								Id = "Bearer"
							 }
						},
						Array.Empty< string >()
					}
				});

                options.SwaggerDoc("v1", new OpenApiInfo
				{
					Version = "v1",
					Title ="Member API"
				});
				
				// using System.Reflection;
				var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
			});

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();


            app.UseAuthentication();
            app.UseAuthorization();


			app.MapControllers();
            app.MapSwagger().RequireAuthorization();

            app.Run();
		}
	}
}

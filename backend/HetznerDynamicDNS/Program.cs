// (c) 2023 Dan Saul
using DanSaul.SharedCode.Hetzner;
using DanSaul.SharedCode.StandardizedEnvironmentVariables;
using RestSharp;
using Serilog;
using Serilog.Events;

namespace HetznerDynamicDNS
{
	public class Program
	{
		public static WebApplication? Application { get; private set; }

		public static void Main(string[] args)
		{
			Log.Logger = new LoggerConfiguration()
				//.Enrich.WithMachineName()
				//.Enrich.FromLogContext()
				//.Enrich.WithProcessId()
				//.Enrich.WithThreadId()
				//.Enrich.WithMachineName()
				.MinimumLevel.Debug()
				.MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
				.WriteTo.Console()
				//.WriteTo.File(new Serilog.Formatting.Json.JsonFormatter(), SERILOG_LOG_FILE)
				.CreateLogger();


			var builder = WebApplication.CreateBuilder(args);

			builder.Host.UseSerilog((HostBuilderContext ctx, LoggerConfiguration lc) =>
			{
				lc.WriteTo.Console();
			});


			builder.Services.AddSingleton<HttpClient>((IServiceProvider serviceProvider) =>
			{
				return new HttpClient();
			});
			builder.Services.AddSingleton<RestClient>((IServiceProvider serviceProvider) =>
			{
				RestClientOptions options = new("https://dns.hetzner.com/api/v1/");
				RestClient client = new(options);
				client.AddDefaultHeader("Auth-API-Token", EnvHetzner.HETZNER_API_KEY);
				return client;
			});

			builder.Services.AddSingleton<DynamicDNSManager>();


			builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			Application = builder.Build();

			// Configure the HTTP request pipeline.
			if (Application.Environment.IsDevelopment())
			{
				Application.UseSwagger();
				Application.UseSwaggerUI();
			}

			Application.UseHttpsRedirection();

			Application.UseAuthorization();


			Application.MapControllers();

			DynamicDNSManager dns = Application.Services.GetRequiredService<DynamicDNSManager>();
			Task.Run(dns.Run);

			Application.Run();
		}
	}
}
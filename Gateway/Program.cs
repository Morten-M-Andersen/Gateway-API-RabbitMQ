using Serilog;
using DotNetEnv;
using Gateway.Messages;

var builder = WebApplication.CreateBuilder(args);

// Indlæs .env-filen tidligt i programmet
Env.Load();

// Konfiguration af Serilog
var seqUrl = builder.Configuration.GetValue<string>("Seq:Url") ?? "http://localhost:5341";

Log.Logger = new LoggerConfiguration()
    //.MinimumLevel.Debug() //logger ALT
    .Enrich.WithCorrelationId() // specifickt fra Serilog.Enricher.CorrelationId (ekstra NuGet)
    .WriteTo.Console()  // log skrives til konsol
    .WriteTo.Seq(seqUrl)   // URL til Seq
    .Enrich.FromLogContext()    // tilføjer Serilog kontekst
    .CreateLogger();

// Serilog som default logger
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// *** Registrering af services ***
builder.Services.AddSingleton<IMessageProducer, RabbitMqProducer>();

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
Log.CloseAndFlush(); // korrekt nedlukning af loggeren

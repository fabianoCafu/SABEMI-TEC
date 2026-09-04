using AutoMapper;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SABEMITEC.ContratoAPI.ConfigMapper;
using SABEMITEC.ContratoAPI.Consumers;
using SABEMITEC.ContratoAPI.Context;
using SABEMITEC.ContratoAPI.Repository;
using SABEMITEC.ContratoAPI.Service;
using SABEMITEC.ContratoAPI.SignalR;

var builder = WebApplication.CreateBuilder(args);
var connection = builder.Configuration.GetConnectionString("DBConnectionString");

if (string.IsNullOrWhiteSpace(connection))
{
    throw new InvalidOperationException("ConnectionString 'DBConnectionString' não encontrada.");
}

builder.Services.AddDbContext<SqlSeverContextContrato>(options =>
{
    options.UseSqlServer(connection, sql => sql.MigrationsAssembly("SABEMITEC.ContratoAPI"));
});

IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<IContratoRepository, ContratoRepository>();
builder.Services.AddScoped<IContratoService, ContratoService>();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Serviço Contratos", Version = "v1" });
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<EventoStatusContratoConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h => { h.Username("guest"); h.Password("guest");});
        cfg.ReceiveEndpoint("status-contrato", e => { e.ConfigureConsumer<EventoStatusContratoConsumer>(context);});
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

builder.Services.AddSignalR();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AngularPolicy");
app.UseAuthorization();
app.MapControllers();
app.MapHub<PagamentoHub>("/pagamentos-hub");
app.Run();

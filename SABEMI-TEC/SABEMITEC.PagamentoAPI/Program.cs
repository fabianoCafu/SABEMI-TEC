using AutoMapper;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SABEMITEC.PagamentoAPI.ConfigMapper;
using SABEMITEC.PagamentoAPI.Context;
using SABEMITEC.PagamentoAPI.Middleware;
using SABEMITEC.PagamentoAPI.Repository;
using SABEMITEC.PagamentoAPI.Service;

var builder = WebApplication.CreateBuilder(args);
var connection = builder.Configuration.GetConnectionString("DBConnectionString");

if (string.IsNullOrWhiteSpace(connection))
{
    throw new InvalidOperationException("ConnectionString 'DBConnectionString' não encontrada.");
}

builder.Services.AddDbContext<SQLSeverContext>(options =>
{
    options.UseSqlServer(connection, sql => sql.MigrationsAssembly("SABEMITEC.PagamentoAPI"));
});

IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<IEventoBrutoService, EventoBrutoService>();
builder.Services.AddScoped<IEventoBrutoRepository, EventoBrutoRepository>();

builder.Services.AddControllers()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
});

builder.Services.AddSwaggerGen(options =>
{ 
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Serviço Pagamentos", Version = "v1" });
});

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h => { h.Username("guest"); h.Password("guest"); });
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var app = builder.Build();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pagamento API");
    c.InjectJavascript("/swagger-ui/swagger-hmac.js");
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseMiddleware<SignatureMiddleware>();
app.MapControllers();
app.Run();


using Blazored.SessionStorage;
using ICWebApp.Application.Interface.Provider;
using ICWebApp.Application.Interface.Services;
using ICWebApp.Application.Interface.Sessionless;
using ICWebApp.Application.Provider;
using ICWebApp.Application.Services;
using ICWebApp.Application.Sessionless;
using ICWebApp.DataStore;
using ICWebApp.DataStore.MSSQL;
using ICWebApp.DataStore.MSSQL.Interfaces;
using ICWebApp.DataStore.MSSQL.Interfaces.UnitOfWork;
using ICWebApp.DataStore.MSSQL.Repositories;
using ICWebApp.DataStore.MSSQL.UnitOfWork;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//DBConnection
builder.Services.AddSingleton<DBContext>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

builder.Services.AddScoped<ICANTEENProvider, CANTEENProvider>();
builder.Services.AddScoped<IMSGProvider, MSGProvider>();
builder.Services.AddScoped<ICONFProviderSessionless, CONFProviderSessionless>();

builder.Services.AddScoped<IMailerService, MailerService>();

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


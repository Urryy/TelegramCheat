using Microsoft.EntityFrameworkCore;
using TelegramCheat;
using TelegramCheat.Implementations;
using TelegramCheat.Interfaces;

var builder = WebApplication.CreateBuilder(args);

string connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDatabaseContext>(opt => opt.UseSqlServer(connection));

builder.Services.AddTransient<TelegramBot>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IOwnProxyRepository, OwnProxyRepository>();
builder.Services.AddTransient<ITelegramBotService, TelegramBotService>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.Services.GetRequiredService<TelegramBot>().StartRecieveBot().Wait();

app.UseRouting();

app.MapControllers();

app.Run();

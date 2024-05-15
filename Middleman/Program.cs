var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(s => s.DisplayRequestDuration());

app.UseHttpsRedirection();

app.MapControllers();

app.Run();



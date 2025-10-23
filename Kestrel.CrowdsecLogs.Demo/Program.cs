var builder = WebApplication.CreateBuilder(args);

builder.AddCrowdSecLogsExporter();

var app = builder.Build();

app.UseCrowdSecLogsExporter();
app.MapGet("/", async () =>
{
   await Task.Delay(TimeSpan.FromSeconds(1));
   return "Hello World";
});


app.Run();

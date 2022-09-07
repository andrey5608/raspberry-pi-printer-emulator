using EscPosDecoderApi;
using EscPosDecoderApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var settings = app.Configuration.GetSection("Settings").Get<Settings>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/receipt/create/{merchantId}", async (string merchantId, HttpRequest request) =>
    {
        //Task.Run(() => { });
        //return "File was processed successfully!"; // TODO run async

        string decodingResult;
        try
        {
            using var ms = new MemoryStream();
            await request.Body.CopyToAsync(ms);
            var fileBytes = ms.ToArray();

            decodingResult = Decoder.DecodeByteArrayToText(fileBytes, merchantId, settings);
        }
        catch (Exception e)
        {
            return Results.Problem(e.Message);
        }


        return Results.Ok(decodingResult);
    }).Accepts<IFormFile>("application/octet-stream")
.WithName("Create a receipt");

app.Run();
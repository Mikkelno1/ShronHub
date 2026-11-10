using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using ShronHub.Endpoints;
using ShronHub.Repository;
using ShronHub.Settings;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection("DatabaseSettings"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;

    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;

    var client = sp.GetRequiredService<IMongoClient>();

    return client.GetDatabase(settings.DatabaseName);
});

builder.Services.AddSingleton<IGridFSBucket>(sp =>
{
    var database = sp.GetRequiredService<IMongoDatabase>();
    
    var settings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;

    var option = new GridFSBucketOptions { BucketName = settings.VideoBucketName };
    return new GridFSBucket(database,  option);
});

builder.Services.AddSingleton<IVideoRepository, VideoRepository>();
builder.Services.AddAntiforgery();

//Franks fix
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
});


builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize =
        500 * 1024 * 1024;
});

//multipart/form-data that our videeEndpoint uses, has a default limit of 28MB 
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit =
        500 * 1024 * 1024;
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build(); //Build WebApplication----------------------------------------------------------

app.UseAntiforgery();
app.UseHttpsRedirection();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => { options.SwaggerEndpoint("/openapi/v1.json", "StudentStore API"); });
}

app.MapVideoEndpoint();

//For our frontend
app.UseStaticFiles();
app.UseDefaultFiles();

app.Run();

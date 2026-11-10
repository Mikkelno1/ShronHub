using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MongoDB.Driver.GridFS;
using ShronHub.Model;
using ShronHub.Model.DTO_s;
using ShronHub.Repository;


namespace ShronHub.Endpoints;

//https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/parameter-binding?view=aspnetcore-10.0
//https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.iformfile.filename?view=aspnetcore-10.0#microsoft-aspnetcore-http-iformfile-filename
//https://learn.microsoft.com/da-dk/aspnet/core/breaking-changes/8/antiforgery-checks?source=recommendations&view=aspnetcore-10.0 <--Forgery
public static class VideoEndpoint
{
    public static IEndpointRouteBuilder MapVideoEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/videos", async (IFormFile videoFile, [FromForm] UploadVideoMetaDataDTO uploadMetadata, IVideoRepository vidRepo) =>
        {
            if (videoFile.Length == 0)
            {
                return Results.BadRequest("File is empty.");
            }

            if (string.IsNullOrWhiteSpace(uploadMetadata.Genre))
            {
                return Results.BadRequest("Genre is required.");
            }

            if (string.IsNullOrWhiteSpace(uploadMetadata.Title))
            {
                return Results.BadRequest("Title is required.");
            }


            //removing potential dangerous file name as seen in reference at top
            var safeFileName = Path.GetFileName(videoFile.FileName);

            //Title and FileName is different as the Video could be 234325.mp4 but the title : "Lets Play Dog"

            var metadata = new VideoMetaData
            {
                Title = uploadMetadata.Title,
                ContentType = videoFile.ContentType,
                Genre = uploadMetadata.Genre
            };

            await using var stream = videoFile.OpenReadStream();

            var id = await vidRepo.UploadAsync(safeFileName, metadata, stream);
            
            var video = new Video
            {
                Id = id,
                FileName = safeFileName,
                Length = videoFile.Length,
                UploadedAt = DateTime.UtcNow,
                VideoMetaData = metadata
            };

            var videoDto = ToVideoDto(video);
            
            return Results.Created($"/videos/{id}", videoDto);
            
        }).DisableAntiforgery();
        //Necessary for IFormFile


        app.MapGet("/videos/{id}", async (string id, IVideoRepository videoRepo) =>
        {
            var video = await videoRepo.GetByIdAsync(id);

            if (video == null)
            {
                return Results.NotFound();
            }
            var videoDto = ToVideoDto(video);
            return Results.Ok(videoDto);
        });

        //get all, for backend searching replaced with search one down under
        //this one can be used with frontend searching
        app.MapGet("/videos", async (IVideoRepository vidRepo) =>
        {
            var videos = await vidRepo.GetAllAsync();

            var videoDtos = videos.Select(ToVideoDto).ToList();

            return Results.Ok(videoDtos);
        });
        
        /*
         //for backend searching
        app.MapGet("/videos", async (string? search, IVideoRepository vidRepo) =>
        {
            List<Video> videos;

            if (string.IsNullOrWhiteSpace(search))
            {
                videos = await vidRepo.GetAllAsync(); //the get all now, if no search is provided
            }
            else
            {
                videos = await vidRepo.SearchAsync(search);
            }
            var videoDtos = videos.Select(ToVideoDto).ToList();

            return Results.Ok(videoDtos);
        });
        */
        
        
        app.MapGet("/videos/{id}/stream", async (string id, IVideoRepository vidRepo) =>
        {
            var video = await vidRepo.GetByIdAsync(id);

            if (video is null)
            {
                return Results.NotFound();
            }

            var stream = await vidRepo.OpenStreamASync(id);

            if (stream is null)
            {
                return Results.NotFound();
            }
            //enableRangeProcessing: true --> only downloads part of the full file, instead of the entire file
            return Results.Stream(stream, contentType: video.VideoMetaData.ContentType, enableRangeProcessing: true);
        });
        
        
        app.MapDelete("/videos/{id}", async (string id, IVideoRepository vidRepo) =>
        {
            var deleted = await vidRepo.DeleteAsync(id);

            if (!deleted)
            {
                return Results.NotFound();
            }
            return Results.NoContent();
        });
        return app;
    }

    
    private static VideoDTO ToVideoDto(Video video)
    {
        var videoDto = new VideoDTO
        {
            Id = video.Id,
            Title = video.VideoMetaData.Title,
            Genre = video.VideoMetaData.Genre,
            ContentType = video.VideoMetaData.ContentType
        };

        return videoDto;
    }
}
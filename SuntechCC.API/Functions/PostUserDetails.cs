using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using SuntechCC.API.Models;
using SuntechCC.API.Services;

namespace SuntechCC.API.Functions;

public class PostUserDetails
{
    private readonly ICosmosService _cosmosService;

    public PostUserDetails(ICosmosService cosmosService)
    {
        _cosmosService = cosmosService;
    }

    /// <summary>
    /// Function that accepts POST requests and saves them to Cosmos db
    /// </summary>
    /// <param name="req">POST request; contains user details</param>
    /// <returns>204 - Accepted, 400 - bad request, 500 - internal error</returns>
    [FunctionName("PostUserDetails")]
    public async Task<HttpResponseMessage> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "PostUserDetails")] HttpRequest req)
    {
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrEmpty(requestBody))
                return new HttpResponseMessage(HttpStatusCode.BadRequest);

            // deserialize request
            var userDetails = DeserializeIntoUserDetails(requestBody);

            // initialize change feed and insert into cosmos
            await _cosmosService.InitializeChangeFeed();
            await _cosmosService.InsertIntoCosmos(userDetails);
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new HttpResponseMessage(HttpStatusCode.InternalServerError);
        }
        finally
        {
            await _cosmosService.DisposeChangeFeed();
        }
    }

    private UserModel DeserializeIntoUserDetails(string requestBody)
    {
        var serializationOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        };
        var userDetailsRequest = JsonSerializer.Deserialize<UserModelRequest>(requestBody, serializationOptions);

        // parse birthday to datetime
        if (!DateTime.TryParse(userDetailsRequest.Birthday, out var birthday))
        {
            throw new ArgumentException("Incorrect birthday format");
        }

        // convert birthday to epoch
        var birthdayToEpoc = DateTime.UtcNow - birthday;

        var userDetails = new UserModel()
        {
            Id = Guid.NewGuid().ToString(),
            FirstName = userDetailsRequest.FirstName,
            LastName = userDetailsRequest.LastName,
            Email = userDetailsRequest.Email,
            BirthdayInEpoch = (int) birthdayToEpoc.TotalSeconds
        };
        return userDetails;
    }
}
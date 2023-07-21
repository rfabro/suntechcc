using System;
using Newtonsoft.Json;

namespace SuntechCC.API.Models;

public class UserModel
{
    [JsonProperty("id")]
    public string Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int BirthdayInEpoch { get; set; }
    public string Email { get; set; }
}
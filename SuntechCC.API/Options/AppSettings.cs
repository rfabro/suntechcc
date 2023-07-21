namespace SuntechCC.API.Options;

public class AppSettings
{
    public Azure Azure { get; set; }
}

public class Azure
{
    public Cosmos Cosmos { get; set; }
}

public class Cosmos
{
    public string ConnectionString { get; set; }
    public string Container { get; set; }
    public string DatabaseId { get; set; }
}
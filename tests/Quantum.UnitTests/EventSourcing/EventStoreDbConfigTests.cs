using Quantum.EventSourcing.EventStoreDB;

namespace Quantum.UnitTests.EventSourcing;

public class EventStoreDbConfigTests
{
    private EventStoreDbConfig _eventStoreDbConfig = new EventStoreDbConfig();

    [Fact]
    public void defaultConnectionStringTests()
    {
        _eventStoreDbConfig.GetConnectionString()
            .Should()
            .BeEquivalentTo(GetDefaultConnectionString());
    }

    [Fact]
    public void changeefaultPort()
    {
        var port = "8080";
        _eventStoreDbConfig.Port = port;
        _eventStoreDbConfig.GetConnectionString()
            .Should()
            .BeEquivalentTo(GetDefaultConnectionString(port: port));
    }

    [Fact]
    public void changeUrl()
    {
        var url = "192.168.2.165";
        _eventStoreDbConfig.Url = url;
        _eventStoreDbConfig.GetConnectionString()
            .Should()
            .BeEquivalentTo(GetDefaultConnectionString(url: url));
    }

    [Fact]
    public void changeProtocl()
    {
        var protocol = "https";
        _eventStoreDbConfig.Protocol = "https";
        _eventStoreDbConfig.GetConnectionString()
            .Should()
            .BeEquivalentTo(GetDefaultConnectionString(protocol: protocol));
    }

    [Fact]
    public void changeKeepAliveTimeout()
    {
        var keepAliveTimeout = 15200;
        _eventStoreDbConfig.KeepAliveTimeout = keepAliveTimeout;
        _eventStoreDbConfig.GetConnectionString()
            .Should()
            .BeEquivalentTo(GetDefaultConnectionString(keepAliveTimeout: keepAliveTimeout));
    }

    [Fact]
    public void changeKeepAliveInterval()
    {
        var keepAliveInterval = 15200;
        _eventStoreDbConfig.KeepAliveInterval = keepAliveInterval;
        _eventStoreDbConfig.GetConnectionString()
            .Should()
            .BeEquivalentTo(GetDefaultConnectionString(keepAliveInterval: keepAliveInterval));
    }

    private static string GetDefaultConnectionString(string protocol = "esdb+discover",
        string url = "localhost",
        string port = "2113",
        bool tls = false,
        int keepAliveTimeout = 10000,
        int keepAliveInterval = 10000)
    {

        return $"{protocol}://{url}:{port}?tls={tls.ToString().ToLower()}&keepAliveTimeout={keepAliveTimeout}&keepAliveInterval={keepAliveInterval}";
    }
}
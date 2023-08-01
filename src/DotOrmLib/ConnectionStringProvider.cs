using Microsoft.Extensions.Configuration;

namespace DotOrmLib
{
    public interface IConnectionStringProvider
    {
        string ConnectionString { get; set; }
    }
    public class ConnectionStringProvider : IConnectionStringProvider
    {
        static ConnectionStringProvider()
        {
            config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            DefaultServer = config[$"{nameof(DotOrmLib)}:{nameof(DefaultServer)}"] ?? DefaultServer;
            DefaultDatabase = config[$"{nameof(DotOrmLib)}:{nameof(DefaultDatabase)}"] ?? DefaultDatabase;
            DefaultSqlSaCredentials = config[$"{nameof(DotOrmLib)}:{nameof(DefaultSqlSaCredentials)}"] ?? DefaultSqlSaCredentials;
            DefaulSqlCredentials = DefaultSqlSaCredentials ?? config[$"{nameof(DotOrmLib)}:{nameof(DefaulSqlCredentials)}"] ?? DefaulSqlCredentials;
            ConnectionStringFormat = config[$"{nameof(DotOrmLib)}:{nameof(ConnectionStringFormat)}"] ?? ConnectionStringFormat;
        }
        public ConnectionStringProvider(string connectionString)
        {
            ConnectionString = connectionString;
        }
        public string ConnectionString { get; set; } = null!;

        public static string DefaultServer = "localhost";
        public static string DefaultDatabase = "Scc1";
        public static string DefaultSqlSaCredentials = $"User Id=sa;Password=Pass@word";
        public static string DefaulSqlCredentials = "Trusted_Connection=True;";
        public static string ConnectionStringFormat = $"Server={{Server}};Database={{Database}};Persist Security Info=True;TrustServerCertificate=True;{{DefaultSqlCredentials}}";
        private static IConfigurationRoot config;

        public static ConnectionStringProvider Create()
            => Create(DefaultServer, DefaultDatabase, DefaulSqlCredentials);

        public static ConnectionStringProvider Create(string dbName)
            => Create(DefaultServer, dbName, DefaulSqlCredentials);

        public static ConnectionStringProvider Create(string server, string dbName, string credentials)
        {
            return new ConnectionStringProvider(ConnectionStringFormat.Replace("{{Server}}", server).Replace("{{Database}}", dbName).Replace("{{DefaultSqlCredentials}}", credentials));
        }
    }

}

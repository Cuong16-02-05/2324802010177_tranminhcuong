namespace ASC.Web.Services
{
    public interface ILoggerService
    {
        string GetOperationId();
        string GetLifetime();
    }

    public class TransientLoggerService : ILoggerService
    {
        private readonly string _id = Guid.NewGuid().ToString("N")[..8];
        public string GetOperationId() => _id;
        public string GetLifetime() => "Transient";
    }

    public class ScopedLoggerService : ILoggerService
    {
        private readonly string _id = Guid.NewGuid().ToString("N")[..8];
        public string GetOperationId() => _id;
        public string GetLifetime() => "Scoped";
    }

    public class SingletonLoggerService : ILoggerService
    {
        private readonly string _id = Guid.NewGuid().ToString("N")[..8];
        public string GetOperationId() => _id;
        public string GetLifetime() => "Singleton";
    }
}

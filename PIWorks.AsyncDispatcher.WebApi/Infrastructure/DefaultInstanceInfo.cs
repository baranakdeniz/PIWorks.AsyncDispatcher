namespace PIWorks.AsyncDispatcher.WebApi.Infrastructure
{
    public class DefaultInstanceInfo : IInstanceInfo
    {// Uygulama her başladığında kendine "Pod-12345678" gibi benzersiz bir isim almasını sağlarızz!!
        public string WorkerId { get; } = "Pod-" + Guid.NewGuid().ToString("N")[..8];
    }
}

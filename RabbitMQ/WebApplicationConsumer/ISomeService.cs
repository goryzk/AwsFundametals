namespace WebApplicationConsumer
{
    public interface ISomeService
    {
        Task<bool> CheckData(int transactionId);
    }
}

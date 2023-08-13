namespace WebApplicationConsumer
{
    public class SomeService : ISomeService
    {
        public async Task<bool> CheckData(int transactionId)
        {
            await Task.Delay(5000);
            return transactionId % 2 == 0;
        }
    }
}

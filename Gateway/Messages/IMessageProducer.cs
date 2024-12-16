namespace Gateway.Messages
{
    public interface IMessageProducer
    {
        Task SendMessage<T>(T message);
    }
}

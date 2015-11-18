namespace Fixie.Execution
{
    public interface IHandler<in TMessage> where TMessage : IMessage
    {
        void Handle(TMessage message);
    }
}
namespace Fixie.Execution
{
    public interface Handler<in TMessage> where TMessage : Message
    {
        void Handle(TMessage message);
    }
}
namespace Fixie.Internal
{
    public interface Handler<in TMessage> : Listener where TMessage : Message
    {
        void Handle(TMessage message);
    }
}
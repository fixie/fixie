namespace Fixie.Reports
{
    using System.Threading.Tasks;

    public interface Handler<in TMessage> : Report where TMessage : Message
    {
        void Handle(TMessage message);
    }

    public interface AsyncHandler<in TMessage> : Report where TMessage : Message
    {
        Task HandleAsync(TMessage message);
    }
}
namespace Fixie.Reports
{
    using System.Threading.Tasks;

    public interface IHandler<in TMessage> : IReport where TMessage : IMessage
    {
        Task Handle(TMessage message);
    }
}
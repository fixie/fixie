namespace Fixie.Reports
{
    using System.Threading.Tasks;

    public interface IHandler<in TMessage> : IReport where TMessage : Message
    {
        Task Handle(TMessage message);
    }
}
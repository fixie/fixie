namespace Fixie.Reports;

public interface IHandler<in TMessage> : IReport where TMessage : IMessage
{
    Task Handle(TMessage message);
}
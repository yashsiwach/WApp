namespace SupportTicketService.Application.Interfaces.Repositories;

public interface IUnitOfWork
{
    ISupportTicketRepository Tickets { get; }
    ITicketReplyRepository   Replies { get; }
    Task SaveAsync();
}

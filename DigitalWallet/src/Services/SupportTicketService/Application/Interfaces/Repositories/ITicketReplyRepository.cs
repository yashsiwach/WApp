using SupportTicketService.Domain.Entities;

namespace SupportTicketService.Application.Interfaces.Repositories;

public interface ITicketReplyRepository
{
    Task AddAsync(TicketReply reply);
    Task SaveAsync();
}

using System.Collections.Generic;
using System.Threading.Tasks;
using WEB_CV.Models;

namespace WEB_CV.Services
{
    public interface IMessagingService
    {
        Task<List<Conversation>> GetConversationsAsync(int currentUserId);
        Task<List<Message>> GetMessagesAsync(int userAId, int userBId, int take = 100, int skip = 0);
        Task<Message> SendMessageAsync(int fromUserId, int toUserId, string content);
        Task MarkAsReadAsync(int readerUserId, int otherUserId);
    }
}



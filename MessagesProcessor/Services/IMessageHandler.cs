using System.Threading.Tasks;

namespace MessagesProcessor.Services
{
    public interface IMessageHandler
    {
        public string DataType { get; }

        Task HandleAsync(string body);
    }
}

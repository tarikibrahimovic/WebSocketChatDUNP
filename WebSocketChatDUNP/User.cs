using System.Net.WebSockets;

namespace WebSocketChatDUNP
{
    public class User
    {
        public string Name { get; set; }
        public WebSocket WebSocket { get; set; }
    }
}

using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;
using WebSocketChatDUNP;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5000");

var app = builder.Build();

app.UseWebSockets();

var connections = new List<User>();

app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var name = context.Request.Query["name"];
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        if (webSocket != null)
            connections.Add(new User
            {
                Name = name,
                WebSocket = webSocket
            });
        Console.WriteLine("Novi korisnik se konektovao na web socket!");
        foreach(var connection in connections)
        {
            if (connection.WebSocket != webSocket)
                await SendMessageToClient(connection.WebSocket, $"Korisnik {name} se konetkovao na websocket!");
        }
        await SendMessageToClient(webSocket, "Dobrodosli na WebSocket chat!");
        await ReceiveMessage(webSocket, async (result, buffer) =>
        {
            if(result.MessageType == WebSocketMessageType.Text)
            {
                var messageString = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var message = JsonConvert.DeserializeObject<dynamic>(messageString);
                Console.WriteLine("Poruka od klijenta je: " + message.Message);
                if (message.To == "All")
                {
                    foreach (var connection in connections)
                    {
                        if (connection.WebSocket != webSocket) 
                            await SendMessageToClient(connection.WebSocket, name + ": " + message.Message);
                    }
                }
                else
                {
                    var user = connections.FirstOrDefault(x => x.Name == name);
                    if (user != null)
                    {
                        await SendMessageToClient(user.WebSocket, name + ": " + message.Message);
                    }
                }
                foreach (var connection in connections)
                {
                    if (connection.WebSocket != webSocket)
                    {
                        await SendMessageToClient(connection.WebSocket, name + ": " + message);
                    }
                }
            }
            else if(result.MessageType == WebSocketMessageType.Close || webSocket.State == WebSocketState.Aborted)
            {
                Console.WriteLine("Korisnik je napustio chat!");
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Korisnik je napustio chat!", CancellationToken.None);
            }
            else
            {
                Console.WriteLine("Nepoznat tip poruke!");
            }
        });
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});


async Task SendMessageToClient(WebSocket webSocket, string message)
{
    var buffer = Encoding.UTF8.GetBytes(message);
    var arraySegment = new ArraySegment<byte>(buffer);
    await webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
}


async Task ReceiveMessage(WebSocket webSocket, Action<WebSocketReceiveResult, byte[]> callback)
{
    var buffer = new byte[1024 * 4];
    while(webSocket.State == WebSocketState.Open)
    {
        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        callback(result, buffer);
    }
}


await app.RunAsync();

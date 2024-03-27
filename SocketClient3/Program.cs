using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;

var connection = new ClientWebSocket();
Console.WriteLine("Unesite vase ime: ");
var name = Console.ReadLine();
Console.WriteLine("Konektujem se na websocket...");
await connection.ConnectAsync(new Uri("ws://localhost:5000/ws?name=" + name), CancellationToken.None);
Console.WriteLine("Uspesno ste se konektovali na websocket!");

var buffer = new byte[1024 * 4];
var result = await connection.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
Console.WriteLine("Poruka od servera je: " + message);


var ReceiveTask = Task.Run(() =>
{
    while (true)
    {
        var buffer = new byte[1024 * 4];
        var result = connection.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        var message = Encoding.UTF8.GetString(buffer, 0, result.Result.Count);
        Console.WriteLine("Poruka od servera je: " + message);
    }
});

var SendTask = Task.Run(async () =>
{
    while (true)
    {
        var To = Console.ReadLine();
        var messageString = Console.ReadLine();
        if (messageString == "exit")
        {
            break;
        }
        if (String.IsNullOrEmpty(To))
        {
            To = "All";
        }
        var message = JsonConvert.SerializeObject(new
        {
            To,
            Message = messageString
        });
        var bytes = Encoding.UTF8.GetBytes(message);
        var arraySegment = new ArraySegment<byte>(bytes);
        await connection.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
    }
});

await Task.WhenAny(ReceiveTask, SendTask);


await connection.CloseAsync(WebSocketCloseStatus.NormalClosure, "Korisnik je napustio chat!", CancellationToken.None);
Console.ReadLine();

await Task.WhenAll(ReceiveTask, SendTask);
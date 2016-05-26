# MyWebSocket
## WebSocket Server написанный на языке c#.
[ссылка на сайт][http://jackyt1988t.github.io/MyWebSocket]
Поддержка .NET Framework 4.5, Mono 4.2 <br>
Пример простейшего WebSocket echo сервера

```C#
using System;
using System.Text;

using ChatConnect.Tcp;
using ChatConnect.Tcp.Protocol;
using ChatConnect.Tcp.Protocol.WS;


namespace ChatConnect
{
    class Program
    {
        static void Main(string[] args)
        {
        		// Включить вывод отладочной информации
        		WS.Debug = true; 
			WS.EventConnect += (object obj, PEventArgs e) =>
			{
				// Объект WebSocket
				WS WebSocket = obj as WS;
				// Событие наступает когда приходят новые данные
				WebSocket.EventData += (object sender, PEventArgs ev) =>
				{
					WSBinnary binnary = ev.sender as WSBinnary;
					if (binnary.Opcod == WSOpcod.Text)
					{
						string text = Encoding.UTF8.GetString(binnary.Data);
						Console.WriteLine(text);
						// Отправляем текстовый фрейм
						WebSocket.Message(text);
					}
				};
				// Событие наступает если произошла ошибка данных
				WebSocket.EventError += (object sender, PEventArgs ev) =>
				{
					Console.WriteLine(ev.sender.ToString());
				};
				// Событие наступает если соединение было закрыто
				WebSocket.EventClose += (object sender, PEventArgs ev) =>
				{
					Console.WriteLine(ev.sender.ToString());
				};
			};
			WServer Server = new WServer("0.0.0.0", 8081, 2);
			
        }
    }
}

// See https://aka.ms/new-console-template for more information

using System.Collections.Concurrent;

ConcurrentDictionary<int, int> dictionary = [];

for (int i = 0; i < 100; i++)
{
    Task.Run(() =>
    {
        while (true)
        {
            var key = Random.Shared.Next(0, 10000);
            dictionary[key] = Random.Shared.Next();
        }
    });
}

//await Task.Delay(1000);
Thread.Sleep(TimeSpan.FromSeconds(1));

for (int i = 0; i < 100; i++)
{
    var list = dictionary.Values.ToList();
}

Console.WriteLine("Hello, World!");

using Discord;
using Discord.WebSocket;

namespace Spellcard;

public static class Program
{
    public static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();
    public static DiscordSocketClient Client = null!;
    public static Dictionary<string, string> TextCommands = new();
    private const string CommandsTextPath = "commands.txt";
    public static async Task MainAsync()
    {
        Client = new DiscordSocketClient(new DiscordSocketConfig());
        Client.Log += Log;
        Client.Ready += Initialize;
        Client.MessageReceived += OnMessageRecieved;

        /* Initialize */
        {
            if (!File.Exists(CommandsTextPath))
            {
                File.Create(CommandsTextPath);
                Thread.Sleep(1000);
            }
            foreach (var item in await File.ReadAllLinesAsync(CommandsTextPath))
            {
                if (item == "") continue;
                
                var split = item.Split(':');
                TextCommands.Add(split[0], split[1].Replace("\\n", "\n"));
            }
        }


        const string path = "token.txt";
        string token;
        if (File.Exists(path))
            token = (await File.ReadAllTextAsync(path)).Replace(" ", "");
        else
        {
            Console.WriteLine("No token has been found. Shutting down...");
            return;
        }

        await Client.LoginAsync(TokenType.Bot, token);
        await Client.StartAsync();
            
        // Block this task until the program is closed.
        await Task.Delay(-1);
    }
    private static Task Log(LogMessage msg)
    {
        Console.WriteLine(msg);
        return Task.CompletedTask;
    }

    private static Task OnMessageRecieved(SocketMessage message)
    {
        const string prefix = "!";
        if (message.Author.IsBot) return Task.CompletedTask;

        if (message.Content.StartsWith($"{prefix}addcmd"))
        {
            var split = message.Content.Split(' ').Where((_, i) => i != 0).ToArray();

            string input = split[0];
            string output = string.Join(" ", split.Where((_, i) => i != 0));
            
            AddCommandToFile(input, output);
            message.Channel.SendMessageAsync($"Added command `{input}` with output: `{output.Replace("\\n", "\n")}`");
        }
        else switch (message.Content)
        {
            case $"{prefix}clrcmds":
                ClearCommands();
                message.Channel.SendMessageAsync("Cleared all commands.");
                break;
            case $"{prefix}help":
                message.Channel.SendMessageAsync(null, false, new EmbedBuilder()
                    .WithTitle("All commands")
                    .WithDescription(string.Join("\n", TextCommands.Select(x => $"{x.Key}:{x.Value.Replace("\n", "\\n")}")) + "\n" + string.Join("\n", "clrcmds:Clears all user-set commands.", "addcmd:Adds a new command. (`!addcmd example-cmd this is an example`)"))
                    .WithColor(Color.Blue)
                    .Build());
                break;
            default:
            {
                if (message.Content.StartsWith(prefix) && message.Content.Length > 1)
                {
                    var cmdName = message.Content.Remove(0, 1);
                    message.Channel.SendMessageAsync(TextCommands.TryGetValue(cmdName, out var output)
                        ? output
                        : $"No command named `{cmdName}` found.");
                }
                break;
            }
        }
        
        return Task.CompletedTask;
    }

    private static void AddCommandToFile(string command, string output)
    {
        TextCommands.Add(command, output.Replace("\\n", "\n"));
        var lines = File.ReadAllText(CommandsTextPath);
        File.WriteAllText(CommandsTextPath, lines + $"\n{command}:{output.Replace("\n", "\\n")}");
    }

    private static void ClearCommands()
    {
        TextCommands.Clear();
        File.WriteAllText(CommandsTextPath, "");
    }
    private static Task Initialize()
    {
        
        return Task.CompletedTask;
    }
}
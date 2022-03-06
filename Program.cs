using Discord;
using Discord.WebSocket;

namespace Spellcard;

public static class Program
{
    public static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();
    public static DiscordSocketClient Client = null!;
    public static async Task MainAsync()
    {
        Client = new DiscordSocketClient(new DiscordSocketConfig());
        Client.Log += Log;
        Client.Ready += Initialize;
        Client.MessageReceived += OnMessageRecieved;
        Client.JoinedGuild += OnGuildJoined;

        foreach (var guild in Client.Guilds)
        {
            CreateGuildCommandsFile(guild);
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

    private static Task OnGuildJoined(SocketGuild guild)
    {
        CreateGuildCommandsFile(guild);
        return Task.CompletedTask;
    }

    private static void CreateGuildCommandsFile(SocketGuild guild)
    {
        if (!File.Exists($"{guild.Id}"))
            File.WriteAllText($"{guild.Id}", "");
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

            message.Channel.SendMessageAsync(
                AddCommandToFile(message.Channel.GetGuild(), input, output)
                    ? $"Added command `{input}` with output: `{output.Replace("\\n", "\n")}`"
                    : $"Command `{input}` is already present.");
        }
        else if (message.Content.StartsWith($"{prefix}rmvcmd"))
        {
            var input = message.Content.Split(' ').Where((_, i) => i != 0).ToArray()[0];
            var guild = message.Channel.GetGuild();

            if (input is "addcmd" or "clrcmds" or "rmvcmd")
                message.Channel.SendMessageAsync("Cannot remove this command.");
            else
                message.Channel.SendMessageAsync(RemoveCommandToFile(guild, input)
                    ? $"Removed command `{input}`."
                    : $"No command named `{input}` found.");
        }
        else switch (message.Content)
        {
            case $"{prefix}clrcmds":
                ClearCommands(message.Channel.GetGuild());
                message.Channel.SendMessageAsync("Cleared all commands.");
                break;
            case $"{prefix}help":
                message.Channel.SendMessageAsync(null, false, new EmbedBuilder()
                    .WithTitle("All commands")
                    .WithDescription(string.Join("\n", File.ReadAllLines($"{message.Channel.GetGuild().Id}")) + "\n" + string.Join("\n", "clrcmds:Clears all user-set commands.", "addcmd:Adds a new command. (`!addcmd example-cmd this is an example`)", "rmvcmd:Removes a specified command. (`!rmvcmd example-cmd`)"))
                    .WithColor(Color.Blue)
                    .Build());
                break;
            default:
            {
                if (message.Content.StartsWith(prefix) && message.Content.Length > 1)
                {
                    var cmdName = message.Content.Remove(0, 1);
                    message.Channel.SendMessageAsync(GetCommandFromFile(message.Channel.GetGuild(), cmdName, out var output)
                        ? output
                        : $"No command named `{cmdName}` found.");
                }
                break;
            }
        }
        
        return Task.CompletedTask;
    }

    private static bool AddCommandToFile(SocketGuild guild, string command, string output)
    {
        CreateGuildCommandsFile(guild);
        
        var lines = File.ReadAllText($"{guild.Id}");
        var lines2 = lines.Split('\n');
        
        if (lines2.Any(x => x.Split(':')[0] is "addcmd" or "clrcmds" or "rmvcmd" || x.Split(':')[0] == command)) return false;
        
        File.WriteAllText($"{guild.Id}", lines + $"\n{command}:{output.Replace("\n", "\\n")}");
        return true;
    }
    private static bool RemoveCommandToFile(SocketGuild guild, string command)
    {
        CreateGuildCommandsFile(guild);
        
        var lines = File.ReadAllLines($"{guild.Id}");
        
        var condition = new Func<string, bool>(x => x.Split(':')[0] != command);
        
        if (lines.All(condition)) return false;
        
        File.WriteAllLines($"{guild.Id}", lines.Where(condition));
        return true;
    }

    private static bool GetCommandFromFile(SocketGuild guild, string command, out string response)
    {
        CreateGuildCommandsFile(guild);
        
        var lines = File.ReadAllLines(guild.Id.ToString());
        var condition = new Func<string, bool>(x => x.Split(':')[0] == command);
        if (lines.Any(condition))
        {
            response = lines.Single(condition).Split(':')[1];
            return true;
        }
        response = "null";
        return false;
    }
    private static void ClearCommands(SocketGuild guild)
    {
        File.WriteAllText(guild.Id.ToString(), "");
    }
    private static Task Initialize()
    {
        
        return Task.CompletedTask;
    }
}
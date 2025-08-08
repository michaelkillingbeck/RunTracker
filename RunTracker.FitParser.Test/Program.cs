using Dynastream.Fit;

var decoder = new Decode();
var messages = new List<Mesg>();

int messagesCount = 0;

decoder.MesgEvent += (sender, e) =>
{
    messagesCount++;
    messages.Add(e.mesg);
};

using var fitStream = new FileStream("FITFiles\\sample.fit", FileMode.Open, FileAccess.Read);
if (decoder.IsFIT(fitStream) == false)
{
    throw new InvalidOperationException("Not a valid FIT file");
}

decoder.Read(fitStream);

Console.WriteLine($"{messagesCount} messages received");
Console.WriteLine();

var messageTypes = messages
    .GroupBy(message => message.Name)
    .OrderByDescending(group => group.Count())
    .ToList();

foreach (var messageGroup in messageTypes)
{
    Console.WriteLine($"{messageGroup.Key}: {messageGroup.Count()}");
}

Console.ReadKey();
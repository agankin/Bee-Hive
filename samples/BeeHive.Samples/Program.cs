using BeeHive.Samples;

Console.WriteLine("Running Hive Queues sample...");
BeeHiveSamples.HiveQueuesSample();

Console.WriteLine("Running Hive Tasks sample...");
await BeeHiveSamples.HiveTasksSample();

Console.WriteLine("Running Queue Result Bag sample...");
await BeeHiveSamples.QueueResultBagSample();

Console.Write("To exit press any key...");
Console.ReadKey(true);
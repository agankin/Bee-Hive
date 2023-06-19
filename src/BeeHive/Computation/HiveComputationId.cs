namespace BeeHive
{
    internal record HiveComputationId(Guid Id)
    {
        public static HiveComputationId Create() => new HiveComputationId(Guid.NewGuid());
    }
}
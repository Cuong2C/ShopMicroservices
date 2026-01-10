namespace BuildingBlocks.Messaging.Events;

public record IntegrationEvent
{
    public Guid Guid => Guid.NewGuid();
    public DateTime OccuredOn => DateTime.Now;
    public string EventName => GetType().AssemblyQualifiedName;
}

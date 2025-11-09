namespace HappyFactory.Models;

public interface IEvent
{
    /// <summary>
    /// UTC timestamp when the event occurred.
    /// </summary>
    DateTime Timestamp { get; }
}
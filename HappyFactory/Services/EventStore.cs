using HappyFactory.Models;

namespace HappyFactory.Services;

/// <summary>
/// Very small in-memory event store.
/// - Stores events in an in-memory list.
/// - Not durable; intended for examples and tests.
/// - Notifies subscribers synchronously when events are appended.
/// </summary>
public class EventStore
{
    private readonly List<IEvent> _events = new();
    private readonly object _lock = new();

    /// <summary>
    /// Raised whenever an event is appended.
    /// Subscribers should be resilient to exceptions — exceptions thrown by a subscriber will not prevent other subscribers from being invoked.
    /// </summary>
    public event Action<IEvent>? EventAppended;

    /// <summary>
    /// Append an event to the store and notify subscribers.
    /// </summary>
    public void Append(IEvent @event)
    {
        if (@event == null) throw new ArgumentNullException(nameof(@event));

        lock (_lock)
        {
            _events.Add(@event);
        }

        // Notify subscribers — do so outside the lock.
        var handlers = EventAppended;
        if (handlers == null) return;

        foreach (var handler in handlers.GetInvocationList().Cast<Action<IEvent>>())
        {
            try
            {
                handler(@event);
            }
            catch
            {
                // Subscribers are responsible for handling errors.
                // Swallow here to ensure one faulty subscriber doesn't break others.
            }
        }
    }

    /// <summary>
    /// Append multiple events atomically from the perspective of the in-memory list.
    /// </summary>
    public void Append(params IEvent[] events)
    {
        ArgumentNullException.ThrowIfNull(events);

        // Add all events under lock once
        lock (_lock)
        {
            _events.AddRange(events);
        }

        // Notify subscribers for each event without re-adding to the store.
        var handlers = EventAppended;
        if (handlers == null) return;

        foreach (var ev in events)
        {
            foreach (Action<IEvent> handler in handlers.GetInvocationList().Cast<Action<IEvent>>())
            {
                try
                {
                    handler(ev);
                }
                catch
                {
                    // Swallow subscriber exceptions so one faulty subscriber doesn't prevent others from running.
                }
            }
        }
    }

    /// <summary>
    /// Returns a snapshot of all events stored so far.
    /// </summary>
    public IReadOnlyList<IEvent> GetAll()
    {
        lock (_lock)
        {
            return _events.ToList().AsReadOnly();
        }
    }
}
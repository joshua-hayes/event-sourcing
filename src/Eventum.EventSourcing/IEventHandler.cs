namespace Eventum.EventSourcing
{
    /// <summary>
    /// Provides an abstraction for something that can handle an event.
    /// </summary>
    /// <typeparam name="TEvent">The type of event being handled by the handler implementation.</typeparam>
    public interface IEventHandler<in TEvent>
    {
        /// <summary>
        ///     Handles the specified event.
        /// </summary>
        /// <param name="event">The event being handled.</param>
        void Handle(TEvent @event);
    }
}
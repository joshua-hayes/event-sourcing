using EventSourcing.Events;

namespace EventSourcing.Projection
{
    /// <summary>
    /// Marker interface for a view projection.
    /// </summary>
    public interface IEventProjection
    {
        /// <summary>
        /// Projects an event stream event onto the managed view and computes an updated changeset.
        /// </summary>
        /// <param name="event">The event to apply.</param>
        void ApplyChange(IEventStreamEvent @event);

        /// <summary>
        /// The materialised view the projection will update when applying event stream changes.
        /// </summary>
        public MaterialisedView View { get; }
    }


    /// <summary>
    /// Marker interface for a view projection.
    /// </summary>
    public interface IEventProjection<TMaterialisedView> : IEventProjection
    {
        /// <summary>
        /// The materialised view the projection will update when applying event stream changes.
        /// </summary>
        public new TMaterialisedView View { get; }
    }
}

using EventSourcing.Events;

namespace EventSourcing.Projections
{
    /// <summary>
    /// Base class for a projection that projects event changes onto a materialised view.
    /// </summary>
    /// <typeparam name="TMaterialisedView">The type of view.</typeparam>
    public abstract class EventProjection<TMaterialisedView> : IEventProjection where TMaterialisedView : MaterialisedView, new()
    {
        protected EventProjection(TMaterialisedView view)
        {
            View = view;
        }

        /// <summary>
        /// Gets the name of the view.
        /// </summary>
        /// <param name="event">The event being handled that contains information that can be used
        /// to resolve the view name.</param>
        /// <returns>The name of the view.</returns>
        public static string GetViewName(IEventStreamEvent @event) => typeof(TMaterialisedView).Name;

        /// <summary>
        /// The materialised view the projection will update when applying event stream changes.
        /// </summary>
        public TMaterialisedView View { get; }

        /// <summary>
        /// <see cref="IEventProjection.View"/>
        /// </summary>
        MaterialisedView IEventProjection.View => this.View;

        /// <summary>
        /// <see cref="IEventProjection.ApplyChange(IEventStreamEvent)"/>
        /// </summary>
        public void ApplyChange(IEventStreamEvent @event)
        {
            try
            {
                ((dynamic)this).Handle((dynamic)@event);

            }
            catch
            {
                throw new EventProjectionException(this.GetType().Name, View, @event);
            }
        }
    }
}

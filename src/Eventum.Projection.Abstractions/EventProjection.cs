using Eventum.EventSourcing;
using Eventum.Persistence;
using Eventum.Serialisation;
using System.Security.Cryptography;
using System.Text;

namespace Eventum.Projection
{
    /// <summary>
    /// Base class for a projection that projects event changes onto a materialised view.
    /// </summary>
    /// <typeparam name="TMaterialisedView">The type of view.</typeparam>
    /// <typeparam name="IEventSerialiser">The serialiser to user when serialising the view.</typeparam>
    /// <typeparam name="maxChangesetSize">The maximum number of changes that can be tracked.</typeparam>
    public abstract class EventProjection<TMaterialisedView> : IEventProjection where TMaterialisedView : MaterialisedView, new()
    {
        private int _maxChangesetSize;
        private readonly IEventSerialiser _serialiser;

        protected EventProjection(TMaterialisedView view, IEventSerialiser serialiser, int maxChangesetSize = 10)
        {
            _maxChangesetSize = maxChangesetSize;
            _serialiser = serialiser;
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
                // Apply change

                ((dynamic)this).Handle((dynamic)@event);

                // Serialise view

                View.View = _serialiser.Serialise(View);

                // Compute and update the hash of the materialized view
                var viewHash = ComputeHash(View);
                var change = $"{@event.Version},{viewHash}";

                if (!View.Changeset.Contains(change)) {
                    View.Changeset.Add(change);
                    if (View.Changeset.Count > _maxChangesetSize)
                    {
                        // Maintain bounded size to prevent the changeset from bloating
                        // the view size.

                        View.Changeset.RemoveAt(0);
                    }
                }
            }
            catch
            {
                throw new EventProjectionException(this.GetType().Name, View, @event);
            }
        }


        /// <summary>
        /// Computes a hash for the current state of the view.
        /// </summary>
        /// <param name="view">The view state from which to compute the hash.</param>
        /// <returns>A hash of the current view state.</returns>
        private string ComputeHash(TMaterialisedView view)
        {
            using var sha256 = SHA256.Create();
            var state = View.View;
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(state));

            return Convert.ToBase64String(hashBytes);
        }
    }
}

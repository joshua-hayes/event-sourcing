using EventSourcing.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EventSourcing.Projections
{
    public class MaterialisedViewProjectionEngine
    {
        private readonly IMaterialisedViewRepository _materialisedViewRepository;
        private List<Type> _projections;

        public MaterialisedViewProjectionEngine(Assembly assembly, IMaterialisedViewRepository materialisedViewRepository)
        {
            _materialisedViewRepository = materialisedViewRepository;

            LoadAssembly(assembly);
        }

        private void LoadAssembly(Assembly assembly)
        {
            _projections = assembly.GetTypes()
                                   .Where(p => typeof(IEventProjection).IsAssignableFrom(p))
                                   .Where(p => p.GetInterfaces()
                                                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>)))
                                   .ToList();
        }

        /// <summary>
        /// Projects an event through one or more event projections to update a materialised view.
        /// </summary>
        /// <param name="event">The event change to project.</param>
        /// <returns>The task.</returns>
        public async Task ProjectAsync(IEventStreamEvent @event)
        {
            if (@event is null)
                throw new ArgumentNullException(nameof(@event));

            var projections = _projections.Where(p => p.GetInterfaces()
                                                       .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>)
                                                                                 && i.GetGenericArguments().First() == @event.GetType()));
            foreach (var p in projections)
            {
                try
                {
                    var viewNameMethod = p.GetMethod("GetViewName", BindingFlags.Static
                                                     | BindingFlags.Instance
                                                     | BindingFlags.Public
                                                     | BindingFlags.NonPublic);
                    var viewName = (string)viewNameMethod.Invoke(null, new object[] { @event });
                    var viewType = p.GetConstructors().First().GetParameters().First()?.ParameterType;

                    var view = await _materialisedViewRepository.LoadViewAsync(viewName, viewType);

                    var projection = (IEventProjection)Activator.CreateInstance(p, view);
                    projection.ApplyChange(@event);

                    // Update view changeset

                    var eventType = @event.EventType ?? @event.GetType().Name;
                    var change = $"{eventType}:{@event.Version}";
                    if (!view.Changeset.Contains(change))
                        view.Changeset.Add(change);

                    bool saved = await _materialisedViewRepository.SaveViewAsync(viewName, projection.View);
                    if (!saved)
                    {
                        throw new EventProjectionException(p.Name, @event, null);
                    }
                }
                catch (Exception e) when (e is not EventProjectionException)
                {
                    throw new EventProjectionException(p.Name, @event, e);
                }
            }
        }
    }
}
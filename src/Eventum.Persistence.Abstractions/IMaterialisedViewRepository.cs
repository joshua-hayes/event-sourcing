namespace Eventum.Persistence
{
    /// <summary>
    /// Repository for saving and loading a <see cref="MaterialisedView"/>.
    /// </summary>
    public interface IMaterialisedViewRepository
    {
        /// <summary>
        /// Saves a materialised view and its current state.
        /// </summary>
        /// <param name="name">The name of the materialised view.</param>
        /// <param name="view">The materialised view to save.</param>
        /// <returns>True, if the view was sucessfully saved.</returns>
        Task<bool> SaveViewAsync(string name, MaterialisedView view);

        /// <summary>
        /// Hydrates a previously saved <see cref="MaterialisedView"/>.
        /// </summary>
        /// <typeparam name="TView">The type of view to load.</typeparam>
        /// <param name="name">The name of the materialised view.</param>
        /// <returns>The materialised view.</returns>
        Task<TView> LoadViewAsync<TView>(string name) where TView : MaterialisedView, new();

        /// <summary>
        /// Hydrates a previously saved <see cref="MaterialisedView"/>.
        /// </summary>
        /// <param name="name">The name of the materialised view.</param>
        /// <param name="type">The type of view to load.</typeparam>
        /// <returns>The materialised view.</returns>
        Task<MaterialisedView> LoadViewAsync(string name, Type type);
    }
}

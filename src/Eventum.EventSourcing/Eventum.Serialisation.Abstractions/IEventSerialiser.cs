namespace Eventum.Serialisation
{
    /// <summary>
    /// Provides an abstraction for serialising an object to / from a string.
    /// </summary>
    public interface IEventSerialiser
    {
        /// <summary>
        /// Serialises the given object to a string.
        /// </summary>
        /// <typeparam name="T">The type of object to serialise.</typeparam>
        /// <param name="obj">The object to serialise.</param>
        /// <returns>The serialised represenation of the object.</returns>
        string Serialise<T>(T obj);

        /// <summary>
        /// De-serialises the provided data into an instance of the type specified.
        /// </summary>
        /// <typeparam name="T">The type of object being de-serialised.</typeparam>
        /// <param name="data">The data from which to de-serialise the object.</param>
        /// <returns>The de-serialised object.</returns>
        T Deserialise<T>(string data);
    }
}

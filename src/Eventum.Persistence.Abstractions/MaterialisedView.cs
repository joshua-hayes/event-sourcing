namespace Eventum.Persistence;

/// <summary>
/// Maintains an up to date serialised version of a view that can be updated
/// as <see cref="EventStreamChange"/>'s occur.
/// </summary>
public class MaterialisedView : IMaterialisedView
{
    public MaterialisedView() : this(string.Empty, null, new List<string>())
    {
    }

    public MaterialisedView(string view, string etag, IList<string> changeset)
    {
        Etag = etag;
        Changeset = changeset;
        View = view;
    }

    /// <summary>
    /// <see cref="IMaterialisedView.Etag"/>
    /// </summary>
    public string Etag { get; set; }

    /// <summary>
    /// <see cref="IMaterialisedView.Changeset"/>
    /// </summary>
    public IList<string> Changeset { get; set; }

    /// <summary>
    /// <see cref="IMaterialisedView.View"/>
    /// </summary>
    public string View { get; set; }
}

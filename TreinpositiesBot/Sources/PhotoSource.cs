namespace TreinpositiesBot; 

public abstract class PhotoSource {
	public abstract IReadOnlyCollection<string> ExtractIds(string message);
	public abstract Task<IReadOnlyList<Photobox>?> GetPhotos(IReadOnlyCollection<string> ids);
}

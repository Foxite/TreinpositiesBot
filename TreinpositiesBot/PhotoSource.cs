namespace TreinpositiesBot; 

public abstract class PhotoSource {
	public abstract IReadOnlyCollection<string> ExtractIds(string message);
	public abstract Task<Photobox?> GetPhoto(IReadOnlyCollection<string> ids);
}
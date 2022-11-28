using System.Text;

public record Photobox(
	string PageUrl,
	string ImageUrl,
	string? Owner,
	string? VehicleType,
	string VehicleNumber,
	PhotoType PhotoType,
	string Taken,
	string Photographer,
	string PhotographerUrl
) {
	public string Identity {
		get {
			var ret = new StringBuilder();

			void AppendStringAndSpace(string? str) {
				if (!string.IsNullOrWhiteSpace(str)) {
					ret.Append(str);
					ret.Append(' ');
				}
			}
			
			AppendStringAndSpace(Owner);
			AppendStringAndSpace(VehicleType);
			ret.Append(VehicleNumber);

			return ret.ToString();
		}
	}
};

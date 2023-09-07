namespace WebAdmin.Backend.Controllers;

public static class StringUtil {
	public static IEnumerable<int> IndicesOf(this string haystack, string needle, StringComparison stringComparison = StringComparison.Ordinal) {
		int lastPosition = 0;
		while (true) {
			lastPosition = haystack.IndexOf(needle, lastPosition, stringComparison);
			if (lastPosition == -1) {
				yield break;
			}
			yield return lastPosition;
		}
	}
}

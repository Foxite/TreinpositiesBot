namespace WebAdmin.Backend.Controllers;

public static class StringUtil {
	public static IEnumerable<int> IndicesOf(this string haystack, string needle, StringComparison stringComparison = StringComparison.Ordinal) {
		int lastPosition = 0;
		while (true) {
			if (lastPosition + 1 >= haystack.Length) {
				yield break;
			}
			
			lastPosition = haystack.IndexOf(needle, lastPosition + 1, stringComparison);
			if (lastPosition == -1) {
				yield break;
			}
			
			yield return lastPosition;
		}
	}
}

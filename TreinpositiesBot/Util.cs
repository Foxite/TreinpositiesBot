using System.Diagnostics.CodeAnalysis;
using HtmlAgilityPack;

namespace TreinpositiesBot; 

public class Util {
	[return: NotNullIfNotNull("node")]
	public static string? GetText(HtmlNode? node) {
		if (node == null) {
			return null;
		}

		if (node.NodeType == HtmlNodeType.Text) {
			return HtmlEntity.DeEntitize(node.InnerText).Trim();
		}

		string ret = "";
		foreach (HtmlNode childNode in node.ChildNodes) {
			ret += GetText(childNode);
		}

		return ret;
	}
}

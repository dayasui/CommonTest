using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ParallelCommon {
	public static class StringUtil {
		private static readonly Dictionary<char, char> _convLetterTable = new Dictionary<char, char>() {
			{'ぁ', 'あ'},
			{'ぃ', 'い'},
			{'ぅ', 'う'},
			{'ぇ', 'え'},
			{'ぉ', 'お'},
			{'っ', 'つ'},
			{'ゃ', 'や'},
			{'ゅ', 'ゆ'},
			{'ょ', 'よ'},
			{'ゎ', 'わ'},
		};

		public static string GetLastWord(string str) {
			if (string.IsNullOrEmpty(str)) {
				return "";
			}

			string pattern = "ー|ん";
			for (int i = str.Length - 1; i >= 0; --i) {
				char c = str[i];
				if (_convLetterTable.ContainsKey(c)) {
					c = _convLetterTable[c];
				}

				string s = c.ToString();
				if (!Regex.IsMatch(s, pattern)) {
					return s;
				}
			}

			return "";
		}

		public static string GetFirstWord(string str) {
			if (string.IsNullOrEmpty(str)) {
				return "";
			}

			return str[0].ToString();

		}

		public static string ReplaceEmoji(string src, string replace) {
			string dst = "";
			foreach (char c in src) {
				if (char.IsHighSurrogate(c)) {
					dst += replace;
				} else if (char.IsLowSurrogate(c)) {
					// 何もしない
				} else {
					dst += c;
				}
			}

			return dst;
		}
	}
}

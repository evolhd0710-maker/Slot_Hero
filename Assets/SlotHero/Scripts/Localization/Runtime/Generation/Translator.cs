using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SlotHero.Localization
{
    /// <summary>
    /// 한국어 화면 글자를 번역표로 영어로 바꾼다(2026년 11월 8일 원재가 영어 설정을 더하게 했다).
    ///
    /// 코드와 설정 에셋은 한국어 글자를 그대로 쓰고, 화면에 그리기 직전에만 이것을 거친다.
    /// 그래서 글자로 무엇을 가리는 코드는 언어와 상관없이 같은 글자를 본다.
    ///
    /// 찾는 차례.
    /// ① 원문 그대로 있는 줄
    /// ② 앞뒤 빈칸을 뗀 원문
    /// ③ 자리({0})가 있는 틀. 자리에 든 글자도 다시 번역한다. 끝까지 다 번역된 틀 가운데 자리에 한글이 가장 적게 든 틀을 쓴다
    /// ④ 여러 줄이면 빈 줄로 나뉜 문단마다, 그다음 줄마다
    /// ⑤ 글자 꾸밈 표시(&lt;color&gt; 같은 것)가 있으면 표시 사이의 글자마다
    /// ⑥ 마침표 뒤에 이어 쓴 문장이면 문장마다
    /// ⑦ 줄표( — ), 가운뎃점( · ), 쉼표로 늘어놓은 목록이면 낱낱이
    /// ⑧ 끝에 숫자가 붙은 이름("불꽃 8")이면 이름만 번역하고 숫자는 그대로
    /// 앞 방법이 일부만 번역하면 다음 방법도 해 보고, 끝까지 번역한 첫 방법을 쓴다.
    /// 어디에도 없으면 처음 일부라도 번역한 결과를 돌려주고 못 찾은 글자로 적어 둔다.
    ///
    /// 같은 한국어가 화면마다 다른 뜻이면(설정 탭의 "일반" 과 아이템 등급의 "일반") 원문 앞에 맥락을 붙여 따로 적는다.
    /// 표에는 "맥락|원문" 으로 적고, 글상자는 맥락을 들고 있다가 그 줄을 먼저 찾는다.
    ///
    /// 번역이 = 하나인 줄은 번역하지 않는 글자(언어 이름 "한국어" 같은 것)다. 그대로 두고 못 찾은 글자로도 적지 않는다.
    /// </summary>
    public class Translator
    {
        private sealed class Template
        {
            public Regex Pattern;
            public int[] Slots;
            public string English;
            public int LiteralLength;
        }

        /// <summary>번역한 글과, 남김없이 번역했는지(번역하지 않기로 한 글자는 번역한 것으로 친다).</summary>
        private struct Result
        {
            public string Text;
            public bool Complete;

            public Result(string text, bool complete)
            {
                Text = text;
                Complete = complete;
            }
        }

        private static readonly Regex SlotToken = new Regex(@"\{(\d+)\}");
        private static readonly Regex EnglishSlot = new Regex(@"\{(\d+)(?:\|([^|}]*)\|([^}]*))?\}");
        private static readonly Regex RichTag = new Regex(@"<[^<>]+>");
        private static readonly Regex TrailingNumber = new Regex(@"^(.*\S)\s+([+\-]?\d+(?:\.\d+)?%?)$", RegexOptions.Singleline);
        private static readonly Regex SentenceGap = new Regex(@"(?<=[.!?])[ ]+(?=\S)");
        private static readonly string[] ListSeparators = { " — ", " · ", ", " };
        private const string LeadingPunctuation = ".,!?:";

        private readonly Dictionary<string, string> _exact = new Dictionary<string, string>();
        private readonly HashSet<string> _ignored = new HashSet<string>();
        private readonly List<Template> _templates = new List<Template>();
        private readonly Dictionary<string, Result> _cache = new Dictionary<string, Result>();
        private readonly HashSet<string> _missing = new HashSet<string>();

        /// <summary>번역표의 줄 수. 틀도 센다.</summary>
        public int Count
        {
            get { return _exact.Count + _templates.Count; }
        }

        /// <summary>번역하지 못한 화면 글자. 검사가 이것으로 빠진 번역을 찾는다.</summary>
        public ICollection<string> Missing
        {
            get { return _missing; }
        }

        /// <summary>번역표를 새로 읽는다. 원문이나 번역이 빈 줄은 건너뛴다. 같은 원문은 뒤의 줄이 이긴다.</summary>
        public void Load(IList<TranslationEntry> entries)
        {
            _exact.Clear();
            _ignored.Clear();
            _templates.Clear();
            _cache.Clear();
            _missing.Clear();

            if (entries == null)
            {
                return;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                TranslationEntry entry = entries[i];
                if (entry == null || string.IsNullOrEmpty(entry.Korean) || string.IsNullOrEmpty(entry.English))
                {
                    continue;
                }

                if (entry.English == "=")
                {
                    _ignored.Add(entry.Korean);
                    continue;
                }

                if (SlotToken.IsMatch(entry.Korean))
                {
                    _templates.Add(MakeTemplate(entry.Korean, entry.English));
                }
                else
                {
                    _exact[entry.Korean] = entry.English;
                }
            }

            // 글자가 많이 박힌 틀이 더 좁게 맞으므로 먼저 본다.
            _templates.Sort((a, b) => b.LiteralLength.CompareTo(a.LiteralLength));
        }

        /// <summary>못 찾은 글자 기록만 비운다.</summary>
        public void ClearMissing()
        {
            _missing.Clear();
        }

        /// <summary>한글이 섞였는지. 한글이 없는 글자는 번역하지 않는다.</summary>
        public static bool HasHangul(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if ((c >= '가' && c <= '힣') || (c >= 'ㄱ' && c <= 'ㆎ'))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>맥락이 붙은 글자를 영어로 바꾼다. 그 맥락의 줄이 없으면 맥락 없이 찾는다.</summary>
        public string Translate(string source, string context)
        {
            string english;
            if (!string.IsNullOrEmpty(context) && source != null && _exact.TryGetValue(context + "|" + source, out english))
            {
                return english;
            }

            return Translate(source);
        }

        /// <summary>화면 글자 하나를 영어로 바꾼다. 다 바꾸지 못하면 못 찾은 글자로 적는다.</summary>
        public string Translate(string source)
        {
            if (!HasHangul(source))
            {
                return source;
            }

            Result result = Resolve(source);
            if (!result.Complete)
            {
                _missing.Add(source);
            }

            return result.Text;
        }

        private Result Resolve(string source)
        {
            Result cached;
            if (_cache.TryGetValue(source, out cached))
            {
                return cached;
            }

            Result result = ResolveFresh(source);
            _cache[source] = result;
            return result;
        }

        private Result ResolveFresh(string source)
        {
            if (!HasHangul(source) || _ignored.Contains(source))
            {
                return new Result(source, true);
            }

            string english;
            if (_exact.TryGetValue(source, out english))
            {
                return new Result(english, true);
            }

            // 앞뒤 빈칸은 떼어 찾고 다시 붙인다.
            string core = source.Trim();
            if (core.Length != source.Length && core.Length > 0)
            {
                int start = source.IndexOf(core, System.StringComparison.Ordinal);
                string head = source.Substring(0, start);
                string tail = source.Substring(start + core.Length);
                Result inner = Resolve(core);

                // 영어가 문장부호로 시작하면(받침 뒤 조사를 "." 로 옮긴 것 같은 때) 앞 빈칸을 붙이지 않는다.
                if (inner.Text.Length > 0 && LeadingPunctuation.IndexOf(inner.Text[0]) >= 0)
                {
                    head = head.TrimEnd(' ');
                }

                return new Result(head + inner.Text + tail, inner.Complete);
            }

            // 방법마다 해 보고 끝까지 번역한 첫 결과를 쓴다. 없으면 처음 나온 결과를 쓴다.
            Result fallback = new Result(source, false);
            bool hasFallback = false;

            Result templated;
            if (TryTemplates(source, out templated) && Take(templated, ref fallback, ref hasFallback))
            {
                return templated;
            }

            if (source.Contains("\n\n")
                && Take(Join(source.Split(new[] { "\n\n" }, System.StringSplitOptions.None), "\n\n"), ref fallback, ref hasFallback))
            {
                return fallback;
            }

            if (source.IndexOf('\n') >= 0 && Take(Join(source.Split('\n'), "\n"), ref fallback, ref hasFallback))
            {
                return fallback;
            }

            if (SentenceGap.IsMatch(source) && Take(Join(SentenceGap.Split(source), " "), ref fallback, ref hasFallback))
            {
                return fallback;
            }

            for (int s = 0; s < ListSeparators.Length; s++)
            {
                string separator = ListSeparators[s];
                if (source.Contains(separator)
                    && Take(Join(source.Split(new[] { separator }, System.StringSplitOptions.None), separator), ref fallback, ref hasFallback))
                {
                    return fallback;
                }
            }

            if (source.IndexOf('<') >= 0 && RichTag.IsMatch(source) && Take(ResolveAroundTags(source), ref fallback, ref hasFallback))
            {
                return fallback;
            }

            Match numbered = TrailingNumber.Match(source);
            if (numbered.Success)
            {
                Result name = Resolve(numbered.Groups[1].Value);
                if (name.Complete)
                {
                    return new Result(name.Text + " " + numbered.Groups[2].Value, true);
                }
            }

            return fallback;
        }

        /// <summary>
        /// 한 방법의 결과를 받는다. 끝까지 번역했으면 그것을 fallback 에 담고 참을 돌려준다.
        /// 일부만 번역했으면 처음 것만 fallback 에 남기고 거짓을 돌려준다.
        /// </summary>
        private static bool Take(Result result, ref Result fallback, ref bool hasFallback)
        {
            if (result.Complete)
            {
                fallback = result;
                return true;
            }

            if (!hasFallback)
            {
                fallback = result;
                hasFallback = true;
            }

            return false;
        }

        /// <summary>조각마다 번역해 다시 잇는다. 조각이 모두 번역됐을 때만 다 번역한 것이다.</summary>
        private Result Join(string[] parts, string separator)
        {
            bool complete = true;
            for (int i = 0; i < parts.Length; i++)
            {
                Result part = Resolve(parts[i]);
                parts[i] = part.Text;
                complete &= part.Complete;
            }

            return new Result(string.Join(separator, parts), complete);
        }

        /// <summary>
        /// 틀에 맞춰 본다. 다 번역된 틀 가운데 자리에 든 한글이 가장 적은 틀을 고른다.
        /// 자리에 숫자만 든 틀이 가장 곧게 맞은 것이다. 다 번역된 틀이 없으면 처음 맞은 틀을 돌려준다.
        /// </summary>
        private bool TryTemplates(string source, out Result result)
        {
            result = new Result(source, false);
            bool found = false;
            int bestCost = int.MaxValue;

            for (int i = 0; i < _templates.Count; i++)
            {
                Template template = _templates[i];
                Match match = template.Pattern.Match(source);
                if (!match.Success)
                {
                    continue;
                }

                // 자리 하나가 목록이나 여러 줄, 여러 문장을 통째로 삼키면 틀이 아니라 나눠 번역할 글이다.
                // "스테이지 1 · 9번째 방 · 엘리트에게 쓰러짐" 을 "{0}에게 쓰러짐" 으로 받으면 순서가 틀어진다.
                if (SpansPieces(match))
                {
                    continue;
                }

                string[] values = new string[template.Slots.Length == 0 ? 0 : Max(template.Slots) + 1];
                bool complete = true;
                int cost = 0;
                for (int s = 0; s < template.Slots.Length; s++)
                {
                    string captured = match.Groups[s + 1].Value;
                    if (HasHangul(captured))
                    {
                        cost++;
                    }

                    Result value = Resolve(captured);
                    values[template.Slots[s]] = value.Text;
                    complete &= value.Complete;
                }

                Result filled = new Result(Fill(template.English, values), complete);
                if (complete)
                {
                    if (cost == 0)
                    {
                        result = filled;
                        return true;
                    }

                    if (cost < bestCost)
                    {
                        result = filled;
                        bestCost = cost;
                        found = true;
                    }

                    continue;
                }

                if (!found)
                {
                    result = filled;
                    found = true;
                }
            }

            return found;
        }

        /// <summary>틀의 자리에 든 글자에 줄바꿈, 목록 구분, 문장 사이가 있는지.</summary>
        private static bool SpansPieces(Match match)
        {
            for (int g = 1; g < match.Groups.Count; g++)
            {
                string captured = match.Groups[g].Value;
                if (captured.IndexOf('\n') >= 0 || SentenceGap.IsMatch(captured))
                {
                    return true;
                }

                for (int s = 0; s < ListSeparators.Length; s++)
                {
                    if (ListSeparators[s] != ", " && captured.Contains(ListSeparators[s]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>꾸밈 표시는 그대로 두고 표시 사이의 글자만 번역한다.</summary>
        private Result ResolveAroundTags(string source)
        {
            StringBuilder builder = new StringBuilder(source.Length);
            bool complete = true;
            int last = 0;
            foreach (Match tag in RichTag.Matches(source))
            {
                if (tag.Index > last)
                {
                    Result piece = Resolve(source.Substring(last, tag.Index - last));
                    builder.Append(piece.Text);
                    complete &= piece.Complete;
                }

                builder.Append(tag.Value);
                last = tag.Index + tag.Length;
            }

            if (last < source.Length)
            {
                Result piece = Resolve(source.Substring(last));
                builder.Append(piece.Text);
                complete &= piece.Complete;
            }

            return new Result(builder.ToString(), complete);
        }

        private static Template MakeTemplate(string korean, string english)
        {
            StringBuilder pattern = new StringBuilder("^");
            List<int> slots = new List<int>();
            int literal = 0;
            int last = 0;
            foreach (Match token in SlotToken.Matches(korean))
            {
                string text = korean.Substring(last, token.Index - last);
                pattern.Append(Regex.Escape(text));
                literal += text.Length;
                pattern.Append("(.+?)");
                slots.Add(int.Parse(token.Groups[1].Value));
                last = token.Index + token.Length;
            }

            string rest = korean.Substring(last);
            pattern.Append(Regex.Escape(rest));
            literal += rest.Length;
            pattern.Append("$");

            Template template = new Template();
            template.Pattern = new Regex(pattern.ToString(), RegexOptions.Singleline | RegexOptions.CultureInvariant);
            template.Slots = slots.ToArray();
            template.English = english;
            template.LiteralLength = literal;
            return template;
        }

        /// <summary>영어 틀의 자리를 채운다. {0|card|cards} 는 값이 1 이면 "1 card", 아니면 "3 cards" 가 된다.</summary>
        public static string Fill(string english, string[] values)
        {
            return EnglishSlot.Replace(english, match =>
            {
                int index = int.Parse(match.Groups[1].Value);
                string value = index < values.Length && values[index] != null ? values[index] : string.Empty;
                if (!match.Groups[2].Success)
                {
                    return value;
                }

                string word = value.Trim() == "1" ? match.Groups[2].Value : match.Groups[3].Value;
                return value + " " + word;
            });
        }

        private static int Max(int[] values)
        {
            int max = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] > max)
                {
                    max = values[i];
                }
            }

            return max;
        }
    }
}

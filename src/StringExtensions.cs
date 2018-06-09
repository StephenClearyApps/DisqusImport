using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DisqusImport
{
    public static class StringExtensions
    {
        public static TextElementEnumerable TextElements(this string source) => new TextElementEnumerable(source);

        public struct TextElementEnumerable: IEnumerable<string>
        {
            private readonly string _source;

            public TextElementEnumerable(string source)
            {
                _source = source;
            }

            public TextElementEnumerator GetEnumerator() => new TextElementEnumerator(StringInfo.GetTextElementEnumerator(_source));

            IEnumerator<string> IEnumerable<string>.GetEnumerator() => GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public struct TextElementEnumerator: IEnumerator<string>
        {
            private readonly System.Globalization.TextElementEnumerator _source;

            public TextElementEnumerator(System.Globalization.TextElementEnumerator source)
            {
                _source = source;
            }

            public bool MoveNext() => _source.MoveNext();

            public void Reset() => _source.Reset();

            public string Current => (string)_source.Current;

            object IEnumerator.Current => _source.Current;

            public void Dispose() { }
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using Biz.Morsink.DataConvert;
using Biz.Morsink.DataConvert.Converters;
using Pidgin;
namespace FfAdmin.Common
{
    using static Parser;
    using static Parser<char>;
    public static class Csv
    {
        public static readonly Parser<char, char> InnerChar = OneOf(Try(AnyCharExcept('"')),
            Try(Char('"').Then(Char('"'))));
        public static readonly Parser<char, IEnumerable<char>> Whitespace = OneOf('\t', '\r', ' ').Many();
        public static readonly Parser<char, string> Cell =
            OneOf(
                Try(InnerChar.ManyString().Between(Char('"')).Between(Whitespace)),
                Try(AnyCharExcept(',', '\n', '\r').ManyString()));
        public static readonly Parser<char, string[]> Row =
            Cell.Separated(Char(',')).Select(vals => vals.Select(val => val.Trim()).ToArray());
        public static readonly Parser<char, IEnumerable<string[]>> All =
            Row.Separated(Char('\n').Between(Whitespace)).Before(End);

        public static IEnumerable<Dictionary<string, string>> ParseCsv(this string str, IEqualityComparer<string>? equalityComparer = null)
        {
            equalityComparer ??= EqualityComparer<string>.Default;
            var result = All.ParseOrThrow(str).ToList();
            if (result.Any())
            {
                var headers = result.First();
                return result.Skip(1).Select(row => row.Zip(headers, (v, n) => (n, v)).ToDictionary(x => x.n.Replace(' ', '_'), x => x.v, equalityComparer))
                    .Where(d => d.Count > 1);
            }
            else
                return Enumerable.Empty<Dictionary<string, string>>();
        }
        public static IEnumerable<T> ParseCsv<T>(this string str, IEqualityComparer<string>? equalityComparer = null, Func<Dictionary<string,string>, bool>? filter = null)
            => str.ParseCsv(equalityComparer)
                .Where(d => filter is null || filter(d))
                .Select(x => DataConverter.Convert(x).To<T>());

        private static readonly DataConverter DataConverter = new (
                IdentityConverter.Instance,
                RecordConverter.ForDictionaries(),
                IsoDateTimeConverter.Instance,
                Base64Converter.Instance,
                new ToStringConverter(true),
                new TryParseConverter().Restrict((_, to) => to != typeof(bool)), // bool parsing has a custom converter in pipeline
                EnumToNumericConverter.Instance,
                SimpleNumericConverter.Instance,
                BooleanConverter.Instance,
                new NumericToEnumConverter(),
                EnumParseConverter.CaseInsensitive,
                new ToNullableConverter(),
                TupleConverter.Instance,
                ToObjectConverter.Instance,
                new FromStringRepresentationConverter().Restrict((from, _) => from != typeof(Version)), // Version could conflict with numeric types' syntaxes.
                new DynamicConverter()
            );
        public static IEnumerable<Dictionary<string, string>> ConvertAllToDictionary<T>(this IEnumerable<T> records)
            => records.Select(rec =>
                    DataConverter.Convert(rec)
                        .To(new Dictionary<string, string>()))
                .Where(d => d.Count > 0);
        public static string ToCsv<T>(this IEnumerable<T> records)
        {
            var data = records.ConvertAllToDictionary().ToList();
            if (!data.Any())
                return "No data";
            var columns = data.First().Keys;
            return string.Join("\r\n",
                new[]
                {
                    string.Join(",", columns)
                }.Concat(data.Select(dict => dict.ToLine(columns))));
        }
        private static string ToLine(this IReadOnlyDictionary<string, string> dict, IEnumerable<string> columns)
            => string.Join(",", columns
                .Select(c => dict.TryGetValue(c, out var x) ? x : "")
                .Select(x => $"\"{x.Replace("\"", "\"\"")}\""));
    }
}

﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace FfAdminWeb.Utils
{
    public class DataSheetWriter
    {
        private readonly DataSheet _dataSheet;
        private int _x;
        private int _y;

        private DataSheetWriter(DataSheet dataSheet, int x, int y)
        {
            _dataSheet = dataSheet;
            _x = x;
            _y = y;
        }
        public static DataSheetWriter New(string name)
            => new (new DataSheet (name), 1, 1);
        public DataSheetWriter Write(object? value)
        {
            if (value != null)
                _dataSheet[_x, _y] = value;
            _x++;
            return this;
        }
        public DataSheetWriter RowFeed()
        {
            _x = 1;
            _y++;
            return this;
        }
        public DataSheetWriter Empty()
        {
            _x++;
            return this;
        }
        public DataSheet GetSheet()
            => _dataSheet;
    }
    public class DataSheet
    {
        public static DataSheet Build(string name, Action<DataSheetWriter> builder)
        {
            var writer = DataSheetWriter.New(name);
            builder(writer);
            return writer.GetSheet();
        }

        private readonly SortedDictionary<int, SortedDictionary<int, object>> _values = new ();
        public string Name { get; }
        public DataSheet(string name)
        {
            Name = name;
        }
        public object? this[int x, int y]
        {
            get => _values.TryGetValue(y, out var row) && row.TryGetValue(x, out var fld) ? fld : null;
            set
            {
                if (!_values.TryGetValue(y, out var row))
                    _values[y] = row = new SortedDictionary<int, object> ();
                if (value == null)
                    row.Remove(x);
                else
                    row[x] = value;
            }
        }
        public IEnumerable<(int Number, SortedDictionary<int, object> Data)> Rows => _values.Select(v => (v.Key, v.Value));
    }
    public static class ExcelExport
    {
        public static byte[] ToExcel(this IEnumerable<DataSheet> sheeets)
        {
            using var ms = new MemoryStream();
            sheeets.ToExcel(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return ms.ToArray();
        }
        public static void ToExcel(this IEnumerable<DataSheet> sheeets, Stream stream)
        {
            using var doc = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, true);
            var wbPart = doc.AddWorkbookPart();
            wbPart.Workbook = new Workbook
            {
                MCAttributes = new MarkupCompatibilityAttributes
                {
                    Ignorable = "x15 xr xr6 xr10 xr2"
                }
            };
            wbPart.Workbook.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
            wbPart.Workbook.AddNamespaceDeclaration("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006");
            wbPart.Workbook.AddNamespaceDeclaration("x15", "http://schemas.microsoft.com/office/spreadsheetml/2010/11/main");
            wbPart.Workbook.AddNamespaceDeclaration("xr", "http://schemas.microsoft.com/office/spreadsheetml/2014/revision");
            wbPart.Workbook.AddNamespaceDeclaration("xr6", "http://schemas.microsoft.com/office/spreadsheetml/2016/revision6");
            wbPart.Workbook.AddNamespaceDeclaration("xr10", "http://schemas.microsoft.com/office/spreadsheetml/2016/revision10");
            wbPart.Workbook.AddNamespaceDeclaration("xr2", "http://schemas.microsoft.com/office/spreadsheetml/2015/revision2");

            wbPart.AddStyleSheet();
            var sheets = new Sheets();
            wbPart.Workbook.Append(sheets);
            var idx = 0;
            foreach (var sheet in sheeets)
            {
                sheets.Append(new Sheet
                {
                    Name = sheet.Name, SheetId = (uint)idx + 1, Id = $"rId{idx}"
                });
                wbPart.AddSheet(sheet, $"rId{idx}");
                idx++;
            }
            doc.Save();
        }
        public static void AddStyleSheet(this WorkbookPart wbPart)
        {
            var stylePart = wbPart.AddNewPart<WorkbookStylesPart>();
            var ss = new Stylesheet();

            ss.CreateFonts();
            ss.CreateFills();
            ss.CreateBorders();

            stylePart.Stylesheet = ss;
        }
        // We may need this in the future:
        //
        // private static void CreateCellFormats(this Stylesheet ss)
        // {
        //     ss.CellFormats = new ();
        //     ss.CellFormats.Append(new CellFormat());
        //     ss.CellFormats.Count = (uint)ss.CellFormats.ChildElements.Count;
        // }
        private static void CreateBorders(this Stylesheet ss)
        {
            ss.Borders = new Borders ();
            ss.Borders.Append(new Border());
            ss.Borders.Count = (uint)ss.Borders.ChildElements.Count;
        }
        private static void CreateFills(this Stylesheet ss)
        {
            ss.Fills = new Fills ();
            var f = new Fill
            {
                PatternFill = new PatternFill
                {
                    PatternType = PatternValues.None
                }
            };
            ss.Fills.Append(f);
            ss.Fills.Count = (uint)ss.Fills.ChildElements.Count;

        }
        private static void CreateFonts(this Stylesheet ss)
        {
            ss.Fonts = new Fonts ();
            var f = new Font();
            ss.Fonts.Append(f);
            ss.Fonts.Count = (uint)ss.Fonts.ChildElements.Count;
        }

        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        public static void AddSheet(this WorkbookPart wbPart, DataSheet sheet, string id)
        {
            var wsPart = wbPart.AddNewPart<WorksheetPart>(id);
            var data = new SheetData();
            wsPart.Worksheet = new Worksheet (data);

            foreach (var (number, rowData) in sheet.Rows)
            {
                var r = new Row
                {
                    RowIndex = (uint)number
                };
                foreach (var (key, value) in rowData)
                {
                    if (!ReferenceEquals(value, null))
                        r.Append(CreateCell(number, key, value));
                }
                data.Append(r);
            }
        }

        private static Cell CreateCell(int row, int col, object value)
        {
            var c = new Cell
            {
                CellReference = $"{ColumnName(col)}{row}",
                DataType = value switch
                {
                    string => CellValues.String,
                    long => CellValues.Number,
                    decimal => CellValues.Number,
                    double => CellValues.Number,
                    int => CellValues.Number,
                    float => CellValues.Number,
                    DateTime => CellValues.Date,
                    bool => CellValues.Boolean,
                    _ => CellValues.String
                },
                CellValue = new CellValue (value switch
                {
                    string str => str,
                    long l => l.ToString(),
                    double d => d.ToString(CultureInfo.InvariantCulture),
                    decimal d => d.ToString(CultureInfo.InvariantCulture),
                    _ => value.ToString()
                } ?? "")
            };

            return c;
        }
        private static string ColumnName(int c)
        {
            var col = new Stack<char>();

            while (c > 0)
            {
                var m = (c - 1) % 26;
                col.Push((char)(m + 'A'));
                c = (c - m) / 26;
            }

            return new string (col.ToArray());
        }
    }

}

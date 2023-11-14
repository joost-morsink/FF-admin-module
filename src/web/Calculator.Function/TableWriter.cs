using System.Text;

namespace FfAdmin.Calculator.Function;

public class TableWriter
{
    private readonly StringBuilder _builder;

    private TableWriter()
    {
        _builder = new StringBuilder();
    }

    public static string Build(Action<TableWriter> writer)
    {
        var tableWriter = new TableWriter();
        tableWriter._builder.Append("<table>");
        writer(tableWriter);
        tableWriter._builder.Append("</table>");
        return tableWriter._builder.ToString();
    }

    public TableWriter Row(Action<RowWriter> writer)
    {
        RowWriter.Build(this, writer);
        return this;
    }

    public TableWriter HeaderRow(params string[] content)
        => Row(wri =>
        {
            foreach (var header in content)
                wri.Header(header);
        });

    public TableWriter FirstColumnHeaderRow(string header, params string[] content)
        => Row(wri =>
        {
            wri.Header(header);
            foreach (var cell in content)
                wri.Cell(cell);
        });
    public TableWriter ContentRow(params string[] content)
        => Row(wri =>
        {
            foreach (var cell in content)
                wri.Cell(cell);
        });
    public class RowWriter
    {
        private readonly StringBuilder _builder;

        private RowWriter(StringBuilder builder)
        {
            _builder = builder;
        }
        public static void Build(TableWriter table, Action<RowWriter> writer)
        {
            var rowWriter = new RowWriter(table._builder);
            rowWriter._builder.Append("<tr>");
            writer(rowWriter);
            rowWriter._builder.Append("</tr>");
        }

        public RowWriter Header(string content)
        {
            _builder.Append($"<th>{content}</th>");
            return this;
        }

        public RowWriter Cell(string content)
        {
            _builder.Append($"<td>{content}</td>");
            return this;
        }
    }
}

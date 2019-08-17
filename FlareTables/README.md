# FlareTables
High-performance, paginated, filterable table component for Blazor. Inspired by jQuery Datatables, but much faster. No default CSS is included; if it was it probably wouldn't fit with the rest of your site's design, so you would have to override it anyway.

[View on NuGet.org](https://www.nuget.org/stats/packages/FlareTables)

### Contents
* [Filtering example](#filtering-demo)
* [Example usage](#example-usage)

### Filtering demo
Filtering and sorting 100,000 rows of data in <1 second

![Filtering example](https://i.imgur.com/C47CiEk.gif)

![Sorting example](https://i.imgur.com/xxTubzx.gif)


### Example usage

```razor

@functions {
    [Parameter]
    private IEnumerable<Contact> Data { get; set; }

    private TableStateHandler _flareTable;

    protected override void OnInit()
    {
        _flareTable = new TableStateHandler(Data, StateHasChanged, 3, 25);
    }

    <FlareTablePaginationButtons TableState="@_flareTable"></FlareTablePaginationButtons>
    <FlareTablePaginationSize TableState="@_flareTable"></FlareTablePaginationSize>
    <FlareTableColumnReset TableState="@_flareTable"></FlareTableColumnReset>
}

<table>
    <thead>
    <tr>
        @foreach (PropertyInfo field in new Contact().GetType().GetProperties().ToList())
        {
            _flareTable.InitColumn(field.Name);
            <th>@field.Name</th>
        }
    </tr>
    <tr>
        @foreach (PropertyInfo field in new Contact().GetType().GetProperties().ToList())
        {
            <th>
                <FlareTableColumnFilter TableState="@_flareTable" FieldName="@field.Name"></FlareTableColumnFilter>
            </th>
        }
    </tr>
    </thead>
    <tbody>
    @foreach (Contact row in _flareTable.Data())
    {
        <tr>
            @foreach (var field in row.GetType().GetProperties().ToList())
            {
                <td>@field.GetValue(row)</td>
            }
        </tr>
    }
    </tbody>
</table>

<FlareTableInfo TableState="@_flareTable"></FlareTableInfo>
```

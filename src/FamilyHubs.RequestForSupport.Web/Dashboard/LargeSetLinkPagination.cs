using FamilyHubs.SharedKernel.Razor.Dashboard;
using FamilyHubs.SharedKernel.Razor.Pagination;

namespace FamilyHubs.RequestForSupport.Web.Dashboard;

//todo: move to shared
public class LargeSetLinkPagination<TColumn> : LargeSetPagination, ILinkPagination
    where TColumn : struct, Enum
{
    private readonly string _dashboardPath;
    private readonly TColumn _column;
    private readonly SortOrder _sort;

    public LargeSetLinkPagination(string dashboardPath, int totalPages, int currentPage, TColumn column, SortOrder sort)
        : base(totalPages, currentPage)
    {
        _dashboardPath = dashboardPath;
        _column = column;
        _sort = sort;
    }

    public string GetUrl(int page)
    {
        return $"{_dashboardPath}?columnName={_column}&sort={_sort}&currentPage={page}";
    }
}
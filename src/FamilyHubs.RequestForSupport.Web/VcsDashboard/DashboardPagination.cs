using FamilyHubs.SharedKernel.Razor.Dashboard;
using FamilyHubs.SharedKernel.Razor.Pagination;

namespace FamilyHubs.RequestForSupport.Web.VcsDashboard;

//todo: make more generic
public class DashboardPagination : LargeSetPagination, ILinkPagination
{
    private readonly Column _column;
    private readonly SortOrder _sort;

    public DashboardPagination(int totalPages, int currentPage, Column column, SortOrder sort)
        : base(totalPages, currentPage)
    {
        _column = column;
        _sort = sort;
    }

    public string GetUrl(int page)
    {
        return $"/VcsRequestForSupport/Dashboard?columnName={_column}&sort={_sort}&currentPage={page}";
    }
}
using FamilyHubs.RequestForSupport.Core.Models;
using FamilyHubs.RequestForSupport.Web.Pages.VcsRequestForSupport;

namespace FamilyHubs.RequestForSupport.Web.Dashboard;

public class DashboardPagination : LargeSetPagination, ILinkPagination
{
    private readonly Column _column;
    private readonly Sort _sort;

    public DashboardPagination(int totalPages, int currentPage, Column column, Sort sort)
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
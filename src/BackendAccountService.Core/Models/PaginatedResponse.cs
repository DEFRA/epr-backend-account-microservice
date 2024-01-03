using BackendAccountService;
using BackendAccountService.Core;
using BackendAccountService.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendAccountService.Core.Models;

public class PaginatedResponse<T>
{
    public List<T> Items { get; private set; }
    public int CurrentPage { get; private set; }
    public int TotalItems { get; private set; }
    public int PageSize { get; private set; }

    public PaginatedResponse(List<T> items, int count, int pageIndex, int pageSize)
    {
        CurrentPage = pageIndex;
        TotalItems = count;
        PageSize = pageSize;
        Items = items;
    }

    public static async Task<PaginatedResponse<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PaginatedResponse<T>(items, count, pageIndex, pageSize);
    }
}
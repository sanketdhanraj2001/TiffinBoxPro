using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiffinBox.Application.Common.Models
{
    public class PaginatedList<T>
    {
        public List<T> Items { get; }
        public int PageNumber { get; }
        public int TotalPages { get; }
        public int TotalCount { get; }
        public int PageSize { get; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public PaginatedList(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        }

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var totalCount = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, totalCount, pageNumber, pageSize);
        }
    }
}

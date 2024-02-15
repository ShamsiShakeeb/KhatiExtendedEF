using KhatiExtendedEF.Model;
using Microsoft.EntityFrameworkCore;

namespace KhatiExtendedEF.Extensions
{
    public static class KhatiExtensions
    {
        public static async Task<PaginationResponseModel<T>> PaginationAsync<T>(this IQueryable<T> list, int pageSize = 10, int pageIndex = 1) where T : class
        {
            var model = new PaginationResponseModel<T>()
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalData = await list.CountAsync(),
                Data = await list.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(),
            };

            model.HasNextPage = Math.Ceiling(Convert.ToDecimal(model.TotalData) / Convert.ToDecimal(pageSize)) > model.PageIndex;
            model.HasPreviousPage = model.TotalData > pageSize && model.PageIndex > 1;

            return model;
        }
    }
}

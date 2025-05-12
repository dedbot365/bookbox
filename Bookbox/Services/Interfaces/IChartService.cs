using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookbox.Services.Interfaces
{
    public interface IChartService
    {
        Task<Dictionary<string, object>> GetBookStatisticsAsync();
        Task<Dictionary<string, object>> GetOrderStatisticsAsync();
        Task<Dictionary<string, object>> GetReviewStatisticsAsync();
        Task<Dictionary<string, object>> GetTimeBasedOrderStatisticsAsync();
        Task<object> GetRecentCompletedOrdersAsync(int count = 5);
        Task<decimal> GetMonthlyRevenueAsync();
        Task<decimal> GetTotalRevenueAsync();
        Task<decimal> GetWeeklyRevenueAsync();
        Task<decimal> GetDailyRevenueAsync();
    }
}
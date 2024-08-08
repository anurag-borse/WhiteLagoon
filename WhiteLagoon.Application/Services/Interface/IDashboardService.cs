using WhiteLagoon.Web.ViewModels;

namespace WhiteLagoon.Application.Services.Interface
{
    public interface IDashboardService
    {
        Task<RadialBarChartDTO> GetTotalBookingRadialChartData();

        Task<RadialBarChartDTO> GetRegisterUserChartData();

        Task<RadialBarChartDTO> GetRevenueChartData();

        Task<PieChartDTO> GetBookingPieChartData();

        Task<LineChartDTO> GetMemberAndBookingLineChartData();
    }
}
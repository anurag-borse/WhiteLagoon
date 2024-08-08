using Microsoft.AspNetCore.Mvc;
using WhiteLagoon.Application.Services.Interface;

namespace WhiteLagoon.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<JsonResult> GetTotalBookingRadialChartData()
        {
            return Json(await _dashboardService.GetTotalBookingRadialChartData());
        }

        public async Task<JsonResult> GetRegisterUserChartData()
        {
            return Json(await _dashboardService.GetRegisterUserChartData());
        }

        public async Task<JsonResult> GetRevenueChartData()
        {
            return Json(await _dashboardService.GetRevenueChartData());
        }

        public async Task<JsonResult> GetBookingPieChartData()
        {
            return Json(await _dashboardService.GetBookingPieChartData());
        }

        public async Task<JsonResult> GetMemberAndBookingLineChartData()
        {
            return Json(await _dashboardService.GetMemberAndBookingLineChartData());
        }
    }
}
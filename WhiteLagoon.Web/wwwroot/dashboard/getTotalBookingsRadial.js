$(document).ready(function () {
    loadTotalBookingRadialChart();
});

function loadTotalBookingRadialChart() {
    $(".chart-spinner").show();
    $.ajax({
        url: '/dashboard/GetTotalBookingRadialChartData',
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            document.querySelector("#spanTotalBookingCount").innerHTML = data.totalCount;
            var sectionCurrentCount = document.createElement("span");
            if (data.hasRatioIncreased) {
                sectionCurrentCount.className = "text-success me-1";
                sectionCurrentCount.innerHTML = '<i class="bi bi-arrow-up-circle me-1"></i><span> ' + data.countInCurrentMonth + '<span>';
            }
            else {
                sectionCurrentCount.className = "text-danger me-1";
                sectionCurrentCount.innerHTML = '<i class="bi bi-arrow-down-circle me-1"></i><span> ' + data.countInCurrentMonth + '<span>';
            }

            document.querySelector("#sectionBookingCount").append(sectionCurrentCount);
            document.querySelector("#sectionBookingCount").append(" Since Last Month");

            loadRadialBarChart("totalBookingsRadialChart", data.series);

            $(".chart-spinner").hide();
        },
        error: function (xhr, status, error) {
            console.error("Error loading chart data: ", error);
            $(".chart-spinner").hide();
        }
    });
}

function loadRadialBarChart(id, seriesData) {
    var options = {
        chart: {
            height: 90,
            width: 90,
            type: "radialBar",
            sparkline: {
                enabled: true
            },
            offsetY: -10
        },
        series: seriesData,
        plotOptions: {
            radialBar: {
                dataLabels: {
                    value: {
                        offsetY: -10,
                    }
                }
            }
        },
        labels: [""]
    };

    var chart = new ApexCharts(document.querySelector("#" + id), options);
    chart.render();
}
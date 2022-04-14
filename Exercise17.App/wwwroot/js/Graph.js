function graph(xValues, yValues) {
    var ctx = document.getElementById("tempValues").getContext("2d");
    new Chart(ctx, {
        type: "line",
        data: {
            labels: xValues,
            datasets: [{
                fill: false,
                lineTension: 0,
                backgroundColor: "rgba(0,0,255,1.0)",
                borderColor: "rgba(179,67,236, 0.5)",
                data: yValues
            }]
        },
        options: {
            legend: { display: false },
            scales: {
                y: {
                    ticks: {
                        callback: function (value, index, ticks) {
                            return value + "&#176;C";
                        }
                    }

                }
            }
        }
    }
    )
}
// Staff Dashboard Charts
document.addEventListener('DOMContentLoaded', function() {
    // Initialize Bootstrap 5 dropdowns
    const dropdownElementList = [].slice.call(document.querySelectorAll('.dropdown-toggle'));
    dropdownElementList.map(function (dropdownToggleEl) {
        return new bootstrap.Dropdown(dropdownToggleEl);
    });
    
    // ==================== REVENUE CHART WITH TIME PERIOD SWITCHING ====================
    const revenueCtx = document.getElementById('revenueLineChart').getContext('2d');
    
    // Get revenue data from the data attributes
    const revenueData = dashboardData.revenueData || {
        daily: Array(7).fill(0),
        weekly: Array(4).fill(0),
        monthly: Array(12).fill(0),
        yearly: Array(5).fill(0)
    };

    // Labels for different time periods
    const timeframeLabels = {
        daily: Array.from({length: 7}, (_, i) => {
            const d = new Date(); 
            d.setDate(d.getDate() - (6-i));
            return d.toLocaleDateString('en-US', {month: 'short', day: 'numeric'});
        }),
        weekly: ['Week 1 (Most Recent)', 'Week 2', 'Week 3', 'Week 4 (Oldest)'],
        monthly: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
        yearly: Array.from({length: 5}, (_, i) => `${new Date().getFullYear() - 4 + i}`)
    };

    // Create the revenue chart (initially with monthly data)
    const revenueLineChart = new Chart(revenueCtx, {
        type: 'line',
        data: {
            labels: timeframeLabels.monthly,
            datasets: [{
                label: 'Revenue (NPR)',
                lineTension: 0.3,
                backgroundColor: 'rgba(111, 66, 193, 0.05)',
                borderColor: 'rgba(111, 66, 193, 1)',
                pointRadius: 3,
                pointBackgroundColor: 'rgba(111, 66, 193, 1)',
                pointBorderColor: 'rgba(111, 66, 193, 1)',
                pointHoverRadius: 3,
                pointHoverBackgroundColor: 'rgba(111, 66, 193, 1)',
                pointHoverBorderColor: 'rgba(111, 66, 193, 1)',
                pointHitRadius: 10,
                pointBorderWidth: 2,
                data: revenueData.monthly,
            }],
        },
        options: {
            maintainAspectRatio: false,
            layout: {
                padding: {
                    left: 10,
                    right: 25,
                    top: 25,
                    bottom: 0
                }
            },
            scales: {
                x: {
                    grid: {
                        display: false,
                        drawBorder: false
                    }
                },
                y: {
                    ticks: {
                        maxTicksLimit: 5,
                        padding: 10,
                        callback: function(value) {
                            return 'NPR ' + value.toLocaleString();
                        }
                    },
                    grid: {
                        color: 'rgb(234, 236, 244)',
                        drawBorder: false
                    }
                }
            },
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            const label = context.dataset.label || '';
                            const value = context.raw;
                            return label + ': NPR ' + value.toLocaleString();
                        }
                    }
                }
            }
        }
    });

    // Function to update revenue chart based on selected timeframe
    function updateRevenueChart(timeframe) {
        const title = document.getElementById('revenue-overview-title');
        if (title) {
            title.textContent = `Revenue Trends (${timeframe.charAt(0).toUpperCase() + timeframe.slice(1)})`;
        }
        
        revenueLineChart.data.labels = timeframeLabels[timeframe];
        revenueLineChart.data.datasets[0].data = revenueData[timeframe];
        revenueLineChart.update();
    }
    
    // Add click handlers to revenue timeframe options
    document.querySelectorAll('.revenue-timeframe-option').forEach(option => {
        option.addEventListener('click', function(e) {
            e.preventDefault();
            const timeframe = this.getAttribute('data-timeframe');
            document.querySelectorAll('.revenue-timeframe-option').forEach(o => o.classList.remove('active'));
            this.classList.add('active');
            updateRevenueChart(timeframe);
        });
    });

    // ==================== ORDERS CHART WITH TIME PERIOD SWITCHING ====================
    const ordersCtx = document.getElementById('ordersLineChart').getContext('2d');
    
    // Get chart data from the data attributes
    const orderData = dashboardData.orderData;

    // Create the orders chart (initially with monthly data)
    const ordersLineChart = new Chart(ordersCtx, {
        type: 'line',
        data: {
            labels: timeframeLabels.monthly,
            datasets: [{
                label: 'Orders',
                lineTension: 0.3,
                backgroundColor: 'rgba(78, 115, 223, 0.05)',
                borderColor: 'rgba(78, 115, 223, 1)',
                pointRadius: 3,
                pointBackgroundColor: 'rgba(78, 115, 223, 1)',
                pointBorderColor: 'rgba(78, 115, 223, 1)',
                pointHoverRadius: 3,
                pointHoverBackgroundColor: 'rgba(78, 115, 223, 1)',
                pointHoverBorderColor: 'rgba(78, 115, 223, 1)',
                pointHitRadius: 10,
                pointBorderWidth: 2,
                data: orderData.monthly,
            }],
        },
        options: {
            maintainAspectRatio: false,
            layout: {
                padding: {
                    left: 10,
                    right: 25,
                    top: 25,
                    bottom: 0
                }
            },
            scales: {
                x: {
                    grid: {
                        display: false,
                        drawBorder: false
                    }
                },
                y: {
                    ticks: {
                        maxTicksLimit: 5,
                        padding: 10
                    },
                    grid: {
                        color: 'rgb(234, 236, 244)',
                        drawBorder: false
                    }
                }
            },
            plugins: {
                legend: {
                    display: false
                }
            }
        }
    });
    
    // Function to update chart based on selected timeframe
    function updateOrderChart(timeframe) {
        const title = document.getElementById('orders-overview-title');
        if (title) {
            title.textContent = `Orders Overview (${timeframe.charAt(0).toUpperCase() + timeframe.slice(1)})`;
        }
        
        ordersLineChart.data.labels = timeframeLabels[timeframe];
        ordersLineChart.data.datasets[0].data = orderData[timeframe];
        ordersLineChart.update();
    }
    
    // Add click handlers to timeframe options
    document.querySelectorAll('.timeframe-option').forEach(option => {
        option.addEventListener('click', function(e) {
            e.preventDefault();
            const timeframe = this.getAttribute('data-timeframe');
            document.querySelectorAll('.timeframe-option').forEach(o => o.classList.remove('active'));
            this.classList.add('active');
            updateOrderChart(timeframe);
        });
    });
    
    // Initialize with monthly view for orders
    document.querySelector('.timeframe-option[data-timeframe="monthly"]')?.classList.add('active');
    updateOrderChart('monthly');
    
    // Initialize with monthly view for revenue
    document.querySelector('.revenue-timeframe-option[data-timeframe="monthly"]')?.classList.add('active');
    updateRevenueChart('monthly');
    
    // Genre, order status, top books, and format charts are already implemented in the view
});

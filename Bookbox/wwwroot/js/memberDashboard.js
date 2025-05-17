// Member Dashboard JavaScript

document.addEventListener('DOMContentLoaded', function() {
    // Create a global object to store chart data
    const dashboardData = {
        orderData: {
            daily: window.orderDataDaily || [],
            weekly: window.orderDataWeekly || [],
            monthly: window.orderDataMonthly || [],
            yearly: window.orderDataYearly || []
        },
        expenseData: {
            daily: window.expenseDataDaily || [],
            weekly: window.expenseDataWeekly || [],
            monthly: window.expenseDataMonthly || [],
            yearly: window.expenseDataYearly || []
        }
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

    // Orders Line Chart
    const ordersCtx = document.getElementById('ordersLineChart').getContext('2d');
    const ordersLineChart = new Chart(ordersCtx, {
        type: 'line',
        data: {
            labels: timeframeLabels.daily,
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
                data: dashboardData.orderData.daily,
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
                            return value.toFixed(0);
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
                }
            }
        }
    });

    // Function to update order chart based on selected timeframe
    function updateOrderChart(timeframe) {
        if (!dashboardData.orderData[timeframe]) {
            console.error("No order data found for timeframe:", timeframe);
            return;
        }
        
        ordersLineChart.data.labels = timeframeLabels[timeframe];
        ordersLineChart.data.datasets[0].data = dashboardData.orderData[timeframe];
        
        // Update chart title
        const cardHeader = document.getElementById('orders-overview-title');
        if (cardHeader) {
            const viewNames = {
                'daily': 'Your Daily Orders (Last 7 Days)',
                'weekly': 'Your Weekly Orders (Last 4 Weeks)',
                'monthly': 'Your Monthly Orders (This Year)',
                'yearly': 'Your Yearly Orders (Last 5 Years)'
            };
            cardHeader.textContent = viewNames[timeframe] || 'Your Order History';
        }
        
        ordersLineChart.update();
    }

    // Expense Line Chart
    const expenseCtx = document.getElementById('expenseLineChart').getContext('2d');
    const expenseLineChart = new Chart(expenseCtx, {
        type: 'line',
        data: {
            labels: timeframeLabels.daily,
            datasets: [{
                label: 'Expense (NPR)',
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
                data: dashboardData.expenseData.daily,
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
                            return 'NPR ' + context.raw.toLocaleString();
                        }
                    }
                }
            }
        }
    });

    // Function to update expense chart based on selected timeframe
    function updateExpenseChart(timeframe) {
        if (!dashboardData.expenseData[timeframe]) {
            console.error("No expense data found for timeframe:", timeframe);
            return;
        }
        
        expenseLineChart.data.labels = timeframeLabels[timeframe];
        expenseLineChart.data.datasets[0].data = dashboardData.expenseData[timeframe];
        
        // Update chart title
        const cardHeader = document.getElementById('expense-overview-title');
        if (cardHeader) {
            const viewNames = {
                'daily': 'Your Daily Expenses (Last 7 Days)',
                'weekly': 'Your Weekly Expenses (Last 4 Weeks)',
                'monthly': 'Your Monthly Expenses (This Year)',
                'yearly': 'Your Yearly Expenses (Last 5 Years)'
            };
            cardHeader.textContent = viewNames[timeframe] || 'Your Expense Overview';
        }
        
        expenseLineChart.update();
    }

    // Add click handlers to order timeframe options
    document.querySelectorAll('.order-timeframe-option').forEach(option => {
        option.addEventListener('click', function(e) {
            e.preventDefault();
            // Update the active class
            document.querySelectorAll('.order-timeframe-option').forEach(opt => 
                opt.classList.remove('active'));
            this.classList.add('active');
            
            // Update the chart with the selected timeframe
            updateOrderChart(this.getAttribute('data-timeframe'));
        });
    });

    // Add click handlers to expense timeframe options
    document.querySelectorAll('.expense-timeframe-option').forEach(option => {
        option.addEventListener('click', function(e) {
            e.preventDefault();
            // Update the active class
            document.querySelectorAll('.expense-timeframe-option').forEach(opt => 
                opt.classList.remove('active'));
            this.classList.add('active');
            
            // Update the chart with the selected timeframe
            updateExpenseChart(this.getAttribute('data-timeframe'));
        });
    });

    // Initialize with daily view for both charts
    updateOrderChart('daily');
    updateExpenseChart('daily');

    // Set the default active class on the daily options
    document.querySelector('.order-timeframe-option[data-timeframe="daily"]')?.classList.add('active');
    document.querySelector('.expense-timeframe-option[data-timeframe="daily"]')?.classList.add('active');
    document.querySelector('.order-timeframe-option[data-timeframe="monthly"]')?.classList.remove('active');
    document.querySelector('.expense-timeframe-option[data-timeframe="monthly"]')?.classList.remove('active');

    // Create pie and donut charts
    createGenrePieChart();
    createOrderStatusChart();
    createFormatDonutChart();

    // Genre Pie Chart function
    function createGenrePieChart() {
        const genreCtx = document.getElementById('genrePieChart').getContext('2d');
        if (!genreCtx) return;
        
        const genreData = window.genreData || [];
        const genreLabels = genreData.map(item => item.genre);
        const genreCounts = genreData.map(item => item.count);
        
        const backgroundColors = ['#4e73df', '#1cc88a', '#36b9cc', '#f6c23e', '#e74a3b', '#858796', 
                                '#2e59d9', '#17a673', '#2c9faf', '#dda20a', '#be2617', '#60616f'];
        const hoverBackgroundColors = ['#2e59d9', '#17a673', '#2c9faf', '#dda20a', '#be2617', '#60616f',
                                    '#4e73df', '#1cc88a', '#36b9cc', '#f6c23e', '#e74a3b', '#858796'];
        
        const genrePieChart = new Chart(genreCtx, {
            type: 'doughnut',
            data: {
                labels: genreLabels,
                datasets: [{
                    data: genreCounts,
                    backgroundColor: backgroundColors.slice(0, genreLabels.length),
                    hoverBackgroundColor: hoverBackgroundColors.slice(0, genreLabels.length),
                    hoverBorderColor: 'rgba(234, 236, 244, 1)',
                }],
            },
            options: {
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom',
                        display: genreLabels.length <= 8
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                const label = context.label || '';
                                const value = context.raw;
                                const total = context.dataset.data.reduce((acc, val) => acc + val, 0);
                                const percentage = Math.round((value / total) * 100);
                                return `${label}: ${value} books (${percentage}%)`;
                            }
                        }
                    }
                },
                cutout: '60%',
            }
        });
    }

    // Order Status Doughnut Chart function
    function createOrderStatusChart() {
        const orderStatusCtx = document.getElementById('orderStatusChart').getContext('2d');
        if (!orderStatusCtx) return;
        
        const orderStatusData = window.orderStatusData || [];
        const orderStatusLabels = orderStatusData.map(item => item.label);
        const orderStatusCounts = orderStatusData.map(item => item.count);
        const orderStatusColors = orderStatusData.map(item => item.color);

        const orderStatusChart = new Chart(orderStatusCtx, {
            type: 'doughnut',
            data: {
                labels: orderStatusLabels,
                datasets: [{
                    data: orderStatusCounts,
                    backgroundColor: orderStatusColors,
                    hoverBackgroundColor: ['#17a673', '#dda20a', '#be2617'],
                    hoverBorderColor: 'rgba(234, 236, 244, 1)',
                }],
            },
            options: {
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom',
                        display: true
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                const label = context.label || '';
                                const value = context.raw;
                                const total = context.dataset.data.reduce((acc, val) => acc + val, 0);
                                const percentage = Math.round((value / total) * 100);
                                return `${label}: ${value} (${percentage}%)`;
                            }
                        }
                    }
                },
                cutout: '60%',
            }
        });
    }

    // Format Donut Chart function
    function createFormatDonutChart() {
        const formatCtx = document.getElementById('formatDonutChart').getContext('2d');
        if (!formatCtx) return;
        
        const formatData = window.formatData || [];
        const formatLabels = formatData.map(item => item.format);
        const formatCounts = formatData.map(item => item.count);

        const backgroundColors = ['#4e73df', '#1cc88a', '#36b9cc', '#f6c23e', '#e74a3b', '#858796', 
                                '#2e59d9', '#17a673', '#2c9faf', '#dda20a', '#be2617', '#60616f'];
        const hoverBackgroundColors = ['#2e59d9', '#17a673', '#2c9faf', '#dda20a', '#be2617', '#60616f',
                                    '#4e73df', '#1cc88a', '#36b9cc', '#f6c23e', '#e74a3b', '#858796'];

        const formatDonutChart = new Chart(formatCtx, {
            type: 'doughnut',
            data: {
                labels: formatLabels,
                datasets: [{
                    data: formatCounts,
                    backgroundColor: backgroundColors.slice(0, formatLabels.length),
                    hoverBackgroundColor: hoverBackgroundColors.slice(0, formatLabels.length),
                    hoverBorderColor: 'rgba(234, 236, 244, 1)',
                }],
            },
            options: {
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom',
                        display: formatLabels.length <= 8
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                const label = context.label || '';
                                const value = context.raw;
                                const total = context.dataset.data.reduce((acc, val) => acc + val, 0);
                                const percentage = Math.round((value / total) * 100);
                                return `${label}: ${value} books (${percentage}%)`;
                            }
                        }
                    }
                },
                cutout: '60%',
            }
        });
    }
});

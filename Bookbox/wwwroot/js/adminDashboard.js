// Admin Dashboard Charts
document.addEventListener('DOMContentLoaded', function() {
    // ==================== ORDERS CHART WITH TIME PERIOD SWITCHING ====================
    const ordersCtx = document.getElementById('ordersLineChart').getContext('2d');
    
    // Get chart data from the data attributes
    const orderData = dashboardData.orderData;
    
    // Debug data
    console.log("Debug - All order data:", orderData);

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

    // Create the orders chart (initially with daily data)
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
                data: orderData.daily,
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
        console.log("Updating chart to:", timeframe);
        
        if (!orderData[timeframe]) {
            console.error("No data found for timeframe:", timeframe);
            return;
        }
        
        ordersLineChart.data.labels = timeframeLabels[timeframe];
        ordersLineChart.data.datasets[0].data = orderData[timeframe];
        
        // Update chart title to reflect the current view
        const cardHeader = document.querySelector('.card-header h6');
        if (cardHeader) {
            const viewNames = {
                'daily': 'Daily (Last 7 Days)',
                'weekly': 'Weekly (Last 4 Weeks)',
                'monthly': 'Monthly (This Year)',
                'yearly': 'Yearly (Last 5 Years)'
            };
            cardHeader.textContent = `Orders Overview - ${viewNames[timeframe] || timeframe}`;
        }
        
        ordersLineChart.update();
    }
    
    // Add click handlers to timeframe options
    document.querySelectorAll('.timeframe-option').forEach(option => {
        option.addEventListener('click', function(e) {
            e.preventDefault();
            // Update the active class
            document.querySelectorAll('.timeframe-option').forEach(opt => 
                opt.classList.remove('active'));
            this.classList.add('active');
            
            // Update the chart with the selected timeframe
            updateOrderChart(this.getAttribute('data-timeframe'));
        });
    });
    
    // Initialize with daily view
    document.querySelector('.timeframe-option[data-timeframe="daily"]').classList.add('active');
    updateOrderChart('daily');

    // ==================== GENRE PIE CHART ====================
    const genreCtx = document.getElementById('genrePieChart').getContext('2d');
    
    // Extract genre data
    const genreData = dashboardData.genreData;
    const genreLabels = genreData.map(item => item.genre);
    const genreCounts = genreData.map(item => item.count);

    // Define colors for the chart
    const backgroundColors = ['#4e73df', '#1cc88a', '#36b9cc', '#f6c23e', '#e74a3b', '#858796', 
                             '#2e59d9', '#17a673', '#2c9faf', '#dda20a', '#be2617', '#60616f'];
    const hoverBackgroundColors = ['#2e59d9', '#17a673', '#2c9faf', '#dda20a', '#be2617', '#60616f',
                                  '#4e73df', '#1cc88a', '#36b9cc', '#f6c23e', '#e74a3b', '#858796'];

    // Create the genre pie chart
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
                            return `${label}: ${value} (${percentage}%)`;
                        }
                    }
                }
            },
            cutout: '60%',
        }
    });

    // Update the small legend below the chart
    const legendContainer = document.getElementById('genre-legend');
    if (legendContainer) {
        legendContainer.innerHTML = genreLabels.slice(0, 5).map((label, index) => {
            return `<span class="mr-2"><i class="fas fa-circle" style="color: ${backgroundColors[index]}"></i> ${label}</span>`;
        }).join('');
    }

    // ==================== TOP SELLING BOOKS CHART ====================
    const topBooksCtx = document.getElementById('topSellingBooksChart').getContext('2d');
    const topSellingData = dashboardData.topSellingData;
    const bookLabels = topSellingData.map(item => item.title);
    const salesCounts = topSellingData.map(item => item.salesCount);

    const topBooksChart = new Chart(topBooksCtx, {
        type: 'bar',
        data: {
            labels: bookLabels,
            datasets: [{
                label: 'Sales',
                backgroundColor: '#4e73df',
                hoverBackgroundColor: '#2e59d9',
                data: salesCounts,
            }],
        },
        options: {
            maintainAspectRatio: false,
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

    // ==================== BOOKS BY FORMAT CHART ====================
    const formatCtx = document.getElementById('formatDonutChart').getContext('2d');

    // Extract format data
    const formatData = dashboardData.formatData;
    const formatLabels = formatData.map(item => item.format);
    const formatCounts = formatData.map(item => item.count);

    // Create the format donut chart
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
                            return `${label}: ${value} (${percentage}%)`;
                        }
                    }
                }
            },
            cutout: '60%',
        }
    });
});
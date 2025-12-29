// Transaction Analysis Results - Interactive Charts and Table Functionality
// This script creates all the interactive charts for the financial analysis
// Each chart is designed to provide specific insights into transaction patterns

document.addEventListener('DOMContentLoaded', function() {
    // Configure Chart.js defaults for consistent styling
    Chart.defaults.font.family = 'system-ui, -apple-system, "Segoe UI", Roboto, sans-serif';
    Chart.defaults.color = '#6c757d';
    Chart.defaults.backgroundColor = 'rgba(13, 110, 253, 0.1)';
    Chart.defaults.borderColor = 'rgba(13, 110, 253, 0.8)';

    // Currency navigation functionality
    function initializeCurrencyNavigation() {
        document.querySelectorAll('.currency-nav .nav-link').forEach(link => {
            link.addEventListener('click', function(e) {
                e.preventDefault();

                const currency = this.getAttribute('data-currency');
                const targetId = this.getAttribute('href').substring(1); // Remove #

                // Update active nav item for this currency
                const currencyNav = this.closest('.currency-nav');
                currencyNav.querySelectorAll('.nav-link').forEach(navLink => {
                    navLink.classList.remove('active');
                });
                this.classList.add('active');

                // Hide all sections for this currency
                const currencySection = document.getElementById(`currency_${currency}`);
                currencySection.querySelectorAll('.currency-content-section').forEach(contentSection => {
                    contentSection.style.display = 'none';
                });

                // Show target section
                const targetSection = document.getElementById(targetId);
                if (targetSection) {
                    targetSection.style.display = 'block';

                    // Smooth scroll to section
                    targetSection.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            });
        });
    }

    // Function to toggle counterparties view between limited and full
    window.toggleCounterpartiesView = function(currency) {
        const tableBody = document.getElementById(`counterpartiesTableBody_${currency}`);
        const toggleButton = document.getElementById(`toggleCounterparties_${currency}`);
        const hiddenRows = tableBody.querySelectorAll('.counterparty-row-hidden');
        const totalRows = tableBody.querySelectorAll('tr').length;
        const isExpanded = hiddenRows.length > 0 && hiddenRows[0].style.display !== 'none';

        if (isExpanded) {
            // Collapse - hide additional rows
            hiddenRows.forEach(row => {
                row.style.display = 'none';
            });
            toggleButton.innerHTML = '<i class="fas fa-chevron-down me-1"></i>Show All (' + totalRows + ' total)';
        } else {
            // Expand - show all rows
            hiddenRows.forEach(row => {
                row.style.display = '';
            });
            toggleButton.innerHTML = '<i class="fas fa-chevron-up me-1"></i>Show Less (20 only)';
        }
    };

    // Table sorting functionality for counterparties tables
    function initializeTableSorting() {
        document.querySelectorAll('[id^="counterpartiesTable_"]').forEach(table => {
            const headers = table.querySelectorAll('th.sortable');

            // Initialize sort state
            table.sortState = { column: 'net', direction: 'desc' };

            // Update sort icons and header highlighting
            function updateSortIcons(activeHeader, direction) {
                headers.forEach(header => {
                    const icon = header.querySelector('i');
                    header.classList.remove('active-sort');

                    if (header === activeHeader) {
                        header.classList.add('active-sort');
                        icon.className = direction === 'asc' ? 'fas fa-sort-up ms-1' : 'fas fa-sort-down ms-1';
                    } else {
                        icon.className = 'fas fa-sort ms-1';
                    }
                });
            }

            // Apply sort to table function - FIXED to sort all data and limit visibility
            function applySortToTable(targetTable, column, direction) {
                const tbody = targetTable.querySelector('tbody');
                const allRows = Array.from(tbody.querySelectorAll('tr'));

                // Check if currently expanded
                const isExpanded = allRows.some(row =>
                    row.classList.contains('counterparty-row-hidden') &&
                    row.style.display !== 'none'
                );

                // Sort ALL rows (both visible and hidden)
                allRows.sort((a, b) => {
                    let aValue, bValue;

                    if (column === 'counterparty') {
                        aValue = a.querySelector('td:first-child').getAttribute('data-value');
                        bValue = b.querySelector('td:first-child').getAttribute('data-value');
                    } else {
                        const columnIndex = column === 'sent' ? 1 : column === 'received' ? 2 : 3;
                        aValue = parseFloat(a.querySelector(`td:nth-child(${columnIndex + 1})`).getAttribute('data-value'));
                        bValue = parseFloat(b.querySelector(`td:nth-child(${columnIndex + 1})`).getAttribute('data-value'));
                    }

                    if (direction === 'asc') {
                        return column === 'counterparty' ?
                            aValue.localeCompare(bValue) : aValue - bValue;
                    } else {
                        return column === 'counterparty' ?
                            bValue.localeCompare(aValue) : bValue - aValue;
                    }
                });

                // Clear tbody and re-append all sorted rows
                tbody.innerHTML = '';

                // Add sorted rows and apply correct visibility classes
                allRows.forEach((row, index) => {
                    if (index < 20) {
                        // First 20 rows are always visible
                        row.className = 'counterparty-row-visible';
                        row.style.display = '';
                    } else {
                        // Rows beyond 20 are hidden unless expanded
                        row.className = 'counterparty-row-hidden';
                        row.style.display = isExpanded ? '' : 'none';
                    }
                    tbody.appendChild(row);
                });
            }

            // Make applySortToTable available globally for toggle function
            window.applySortToTable = applySortToTable;

            // Sort table rows
            function sortTable(column, direction) {
                applySortToTable(table, column, direction);
                table.sortState = { column, direction };
            }

            // Add click event listeners to sortable headers
            headers.forEach(header => {
                header.addEventListener('click', () => {
                    const column = header.getAttribute('data-sort');
                    let direction = 'desc';

                    // Toggle direction if clicking the same column
                    if (table.sortState.column === column) {
                        direction = table.sortState.direction === 'desc' ? 'asc' : 'desc';
                    }

                    sortTable(column, direction);
                    updateSortIcons(header, direction);
                });
            });

            // Set initial sort state - Net Balance column starts highlighted with down arrow
            const netHeader = table.querySelector('[data-sort="net"]');
            if (netHeader) {
                updateSortIcons(netHeader, 'desc');
            }
        });
    }

    // Initialize all functionality
    initializeCurrencyNavigation();
    initializeTableSorting();
});

// Function to create charts for a specific currency
// This is called from the Razor view with the currency-specific data
window.createCurrencyCharts = function(currency, analysisData) {
    // Balance Tracking Chart - Shows how account balance changes over time
    // This is crucial for understanding financial health trends
    if (document.getElementById(`balanceChart_${currency}`)) {
        const balanceCtx = document.getElementById(`balanceChart_${currency}`).getContext('2d');
        new Chart(balanceCtx, {
            type: 'line',
            data: {
                labels: analysisData.balanceHistory.labels,
                datasets: [{
                    label: 'Account Balance',
                    data: analysisData.balanceHistory.data,
                    borderColor: 'rgb(13, 110, 253)',
                    backgroundColor: 'rgba(13, 110, 253, 0.1)',
                    borderWidth: 3,
                    fill: true,
                    tension: 0.4,
                    pointBackgroundColor: 'rgb(13, 110, 253)',
                    pointBorderColor: 'white',
                    pointBorderWidth: 2,
                    pointRadius: 6
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    }
                },
                scales: {
                    y: {
                        beginAtZero: false,
                        grid: {
                            color: 'rgba(0,0,0,0.1)'
                        },
                        ticks: {
                            callback: function(value) {
                                return new Intl.NumberFormat('en-US', {
                                    style: 'currency',
                                    currency: currency
                                }).format(value);
                            }
                        }
                    },
                    x: {
                        grid: {
                            display: false
                        }
                    }
                }
            }
        });
    }

    // Monthly Cash Flow Chart - Income vs Expenses comparison
    // This helps identify spending patterns and income trends
    if (document.getElementById(`cashFlowChart_${currency}`)) {
        const cashFlowCtx = document.getElementById(`cashFlowChart_${currency}`).getContext('2d');
        new Chart(cashFlowCtx, {
            type: 'bar',
            data: {
                labels: analysisData.monthlyAnalyses.labels,
                datasets: [{
                    label: 'Income',
                    data: analysisData.monthlyAnalyses.income,
                    backgroundColor: 'rgba(25, 135, 84, 0.8)',
                    borderColor: 'rgb(25, 135, 84)',
                    borderWidth: 1
                }, {
                    label: 'Expenses',
                    data: analysisData.monthlyAnalyses.expenses,
                    backgroundColor: 'rgba(220, 53, 69, 0.8)',
                    borderColor: 'rgb(220, 53, 69)',
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: {
                            color: 'rgba(0,0,0,0.1)'
                        },
                        ticks: {
                            callback: function(value) {
                                return new Intl.NumberFormat('en-US', {
                                    style: 'currency',
                                    currency: currency
                                }).format(value);
                            }
                        }
                    },
                    x: {
                        grid: {
                            display: false
                        }
                    }
                }
            }
        });
    }

    // Transaction Types Distribution - Shows transaction pattern breakdown
    // Helps understand what types of transactions are most common
    if (document.getElementById(`transactionTypesChart_${currency}`)) {
        const typeCtx = document.getElementById(`transactionTypesChart_${currency}`).getContext('2d');
        const isMobile = window.innerWidth < 768;

        new Chart(typeCtx, {
            type: 'doughnut',
            data: {
                labels: analysisData.transactionTypes.labels,
                datasets: [{
                    data: analysisData.transactionTypes.data,
                    backgroundColor: [
                        'rgba(13, 110, 253, 0.8)',
                        'rgba(25, 135, 84, 0.8)',
                        'rgba(255, 193, 7, 0.8)',
                        'rgba(220, 53, 69, 0.8)',
                        'rgba(111, 66, 193, 0.8)',
                        'rgba(13, 202, 240, 0.8)',
                        'rgba(108, 117, 125, 0.8)',
                        'rgba(253, 126, 20, 0.8)',
                        'rgba(214, 51, 132, 0.8)',
                        'rgba(32, 201, 151, 0.8)'
                    ],
                    borderWidth: 2,
                    borderColor: 'white'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            padding: isMobile ? 8 : 12,
                            usePointStyle: true,
                            font: {
                                size: isMobile ? 10 : 11
                            },
                            boxWidth: isMobile ? 12 : 15,
                            boxHeight: isMobile ? 12 : 15
                        },
                        maxHeight: 200,
                        display: true
                    }
                },
                layout: {
                    padding: {
                        bottom: 20
                    }
                }
            }
        });
    }

    // Yearly Analysis Chart - Long-term trend visualization
    // Essential for understanding annual financial performance
    if (document.getElementById(`yearlyChart_${currency}`)) {
        const yearlyCtx = document.getElementById(`yearlyChart_${currency}`).getContext('2d');
        new Chart(yearlyCtx, {
            type: 'bar',
            data: {
                labels: analysisData.yearlyAnalyses.labels,
                datasets: [{
                    label: 'Net Income',
                    data: analysisData.yearlyAnalyses.data,
                    backgroundColor: function(context) {
                        const value = context.parsed.y;
                        return value >= 0 ? 'rgba(25, 135, 84, 0.8)' : 'rgba(220, 53, 69, 0.8)';
                    },
                    borderColor: function(context) {
                        const value = context.parsed.y;
                        return value >= 0 ? 'rgb(25, 135, 84)' : 'rgb(220, 53, 69)';
                    },
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: {
                            color: 'rgba(0,0,0,0.1)'
                        },
                        ticks: {
                            callback: function(value) {
                                return new Intl.NumberFormat('en-US', {
                                    style: 'currency',
                                    currency: currency
                                }).format(value);
                            }
                        }
                    },
                    x: {
                        grid: {
                            display: false
                        }
                    }
                }
            }
        });
    }
};

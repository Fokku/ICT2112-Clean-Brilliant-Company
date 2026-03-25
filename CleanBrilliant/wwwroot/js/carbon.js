// Carbon Footprint Module — Frontend AJAX Logic
var Carbon = (function ($) {
    'use strict';

    // Enum display mappings (backend serializes enums as integers)
    var WorkMode = { 0: 'Remote', 1: 'Onsite' };
    var TransportMode = { 0: 'Car', 1: 'Bus', 2: 'Train' };

    // CO2 badge helper
    function co2Badge(value, thresholds) {
        // thresholds: { green: maxGreen, yellow: maxYellow }
        var t = thresholds || { green: 1.0, yellow: 5.0 };
        var color, bg;
        if (value < t.green) {
            bg = '#dcfce7'; color = '#16a34a';
        } else if (value <= t.yellow) {
            bg = '#fef3c7'; color = '#d97706';
        } else {
            bg = '#fee2e2'; color = '#ef4444';
        }
        return '<span style="background:' + bg + ';color:' + color +
            ';padding:2px 8px;border-radius:8px;font-size:.82rem;font-weight:600;">' +
            value.toFixed(1) + 't</span>';
    }

    // Show error alert above a container
    function showError(container, message) {
        $(container).find('.alert-danger').remove();
        $(container).prepend(
            '<div class="alert alert-danger alert-dismissible fade show" role="alert">' +
            message +
            '<button type="button" class="btn-close" data-bs-dismiss="alert"></button></div>'
        );
    }

    // Set button loading state
    function setLoading(btn, loading, originalText) {
        if (loading) {
            btn.data('original-text', btn.text());
            btn.prop('disabled', true).text('Loading...');
        } else {
            btn.prop('disabled', false).text(originalText || btn.data('original-text') || 'Submit');
        }
    }

    // ========== DASHBOARD ==========
    function initDashboard() {
        $.get('/api/corporate-footprint/employees', function (data) {
            var count = Array.isArray(data) ? data.length : 0;
            $('#employee-count').text(count);
            $('#employee-label').text(count === 1 ? 'employee' : 'employees');
        }).fail(function () {
            $('#employee-count').text('—');
        });

        $.get('/api/corporate-footprint/buildings', function (data) {
            var count = Array.isArray(data) ? data.length : 0;
            $('#building-count').text(count);
            $('#building-label').text(count === 1 ? 'facility' : 'facilities');
        }).fail(function () {
            $('#building-count').text('—');
        });
    }

    // ========== EMPLOYEES ==========
    function initEmployees() {
        loadEmployees();

        // Toggle transport/distance fields based on work mode
        $('#emp-work-mode').on('change', function () {
            var isRemote = $(this).val() === '0';
            $('#emp-transport-mode, #emp-travel-distance').prop('disabled', isRemote);
            if (isRemote) {
                $('#emp-transport-mode').val('0');
                $('#emp-travel-distance').val('');
                $('#emp-days-in-office').val('0');
            }
        });

        // Add employee form submit
        $('#add-employee-form').on('submit', function (e) {
            e.preventDefault();
            var btn = $(this).find('button[type="submit"]');
            setLoading(btn, true);

            var payload = {
                name: '',
                workMode: parseInt($('#emp-work-mode').val()),
                transportMode: parseInt($('#emp-transport-mode').val()),
                travelDistance: parseFloat($('#emp-travel-distance').val()) || 0,
                daysInOffice: parseInt($('#emp-days-in-office').val()) || 0
            };

            $.ajax({
                url: '/api/corporate-footprint/employees',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(payload),
                success: function () {
                    $('#add-employee-form')[0].reset();
                    loadEmployees();
                    setLoading(btn, false, 'Add Employee');
                },
                error: function (xhr) {
                    showError('#employee-table-container', 'Failed to add employee: ' + (xhr.responseJSON?.message || xhr.statusText));
                    setLoading(btn, false, 'Add Employee');
                }
            });
        });
    }

    function loadEmployees() {
        $.get('/api/corporate-footprint/employees', function (data) {
            var tbody = $('#employee-table tbody');
            tbody.empty();
            if (!data || data.length === 0) {
                tbody.append('<tr><td colspan="7" class="text-center text-muted py-4">No employees added yet.</td></tr>');
                return;
            }
            data.forEach(function (emp) {
                tbody.append(
                    '<tr>' +
                    '<td>' + emp.employeeId + '</td>' +
                    '<td>' + (WorkMode[emp.employeeWorkmode] || emp.employeeWorkmode) + '</td>' +
                    '<td>' + (TransportMode[emp.transportMode] || '—') + '</td>' +
                    '<td>' + (emp.travelDistance || '—') + '</td>' +
                    '<td>' + emp.daysInOffice + '</td>' +
                    '<td class="co2-cell" data-id="' + emp.employeeId + '">—</td>' +
                    '<td>' +
                    '<a href="#" class="calc-emp text-primary me-2" data-id="' + emp.employeeId + '">Calculate</a>' +
                    '<a href="#" class="delete-emp text-danger" data-id="' + emp.employeeId + '">Delete</a>' +
                    '</td></tr>'
                );
            });

            // Bind calculate
            tbody.find('.calc-emp').on('click', function (e) {
                e.preventDefault();
                var id = $(this).data('id');
                $.get('/api/corporate-footprint/employees/' + id + '/footprint', function (result) {
                    var cell = tbody.find('.co2-cell[data-id="' + id + '"]');
                    cell.html(co2Badge(result.footprint, { green: 1.0, yellow: 5.0 }));
                }).fail(function () {
                    showError('#employee-table-container', 'Failed to calculate footprint for employee ' + id);
                });
            });

            // Bind delete
            tbody.find('.delete-emp').on('click', function (e) {
                e.preventDefault();
                var id = $(this).data('id');
                if (!confirm('Delete employee ' + id + '?')) return;
                $.ajax({
                    url: '/api/corporate-footprint/employees/' + id,
                    method: 'DELETE',
                    success: function () { loadEmployees(); },
                    error: function () { showError('#employee-table-container', 'Failed to delete employee ' + id); }
                });
            });
        }).fail(function () {
            showError('#employee-table-container', 'Failed to load employees.');
        });
    }

    // ========== BUILDINGS ==========
    function initBuildings() {
        loadBuildings();

        $('#add-building-form').on('submit', function (e) {
            e.preventDefault();
            var btn = $(this).find('button[type="submit"]');
            setLoading(btn, true);

            var payload = {
                address: $('#bld-address').val(),
                squareFoot: parseFloat($('#bld-sqft').val()) || 0,
                electricity: parseFloat($('#bld-electricity').val()) || 0,
                gas: parseFloat($('#bld-gas').val()) || 0
            };

            $.ajax({
                url: '/api/corporate-footprint/buildings',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(payload),
                success: function () {
                    $('#add-building-form')[0].reset();
                    loadBuildings();
                    setLoading(btn, false, 'Add Building');
                },
                error: function (xhr) {
                    showError('#building-table-container', 'Failed to add building: ' + (xhr.responseJSON?.message || xhr.statusText));
                    setLoading(btn, false, 'Add Building');
                }
            });
        });
    }

    function loadBuildings() {
        $.get('/api/corporate-footprint/buildings', function (data) {
            var tbody = $('#building-table tbody');
            tbody.empty();
            if (!data || data.length === 0) {
                tbody.append('<tr><td colspan="7" class="text-center text-muted py-4">No buildings added yet.</td></tr>');
                return;
            }
            data.forEach(function (bld) {
                tbody.append(
                    '<tr>' +
                    '<td>' + bld.buildingId + '</td>' +
                    '<td>' + bld.address + '</td>' +
                    '<td>' + bld.squareFoot + '</td>' +
                    '<td>' + bld.electricityKWH + ' kWh</td>' +
                    '<td>' + bld.gasVolume + '</td>' +
                    '<td class="co2-cell" data-id="' + bld.buildingId + '">—</td>' +
                    '<td>' +
                    '<a href="#" class="calc-bld text-primary me-2" data-id="' + bld.buildingId + '">Calculate</a>' +
                    '<a href="#" class="delete-bld text-danger" data-id="' + bld.buildingId + '">Delete</a>' +
                    '</td></tr>'
                );
            });

            tbody.find('.calc-bld').on('click', function (e) {
                e.preventDefault();
                var id = $(this).data('id');
                $.get('/api/corporate-footprint/buildings/' + id + '/footprint', function (result) {
                    var cell = tbody.find('.co2-cell[data-id="' + id + '"]');
                    cell.html(co2Badge(result.footprint, { green: 50, yellow: 150 }));
                }).fail(function () {
                    showError('#building-table-container', 'Failed to calculate footprint for building ' + id);
                });
            });

            tbody.find('.delete-bld').on('click', function (e) {
                e.preventDefault();
                var id = $(this).data('id');
                if (!confirm('Delete building ' + id + '?')) return;
                $.ajax({
                    url: '/api/corporate-footprint/buildings/' + id,
                    method: 'DELETE',
                    success: function () { loadBuildings(); },
                    error: function () { showError('#building-table-container', 'Failed to delete building ' + id); }
                });
            });
        }).fail(function () {
            showError('#building-table-container', 'Failed to load buildings.');
        });
    }

    // ========== SHIPPING ==========
    function initShipping() {
        // Order lookup
        $('#order-lookup-form').on('submit', function (e) {
            e.preventDefault();
            var btn = $(this).find('button[type="submit"]');
            setLoading(btn, true);
            var orderId = $('#ship-order-id').val();
            var country = $('#ship-order-country').val();
            var postal = $('#ship-order-postal').val();

            $.when(
                $.get('/api/carbon/order/' + orderId + '/shipping'),
                $.get('/api/carbon/order/' + orderId + '/total?countryCode=' + encodeURIComponent(country) + '&postalCode=' + encodeURIComponent(postal))
            ).done(function (shippingRes, totalRes) {
                var shipping = shippingRes[0];
                var total = totalRes[0];
                $('#order-result').html(
                    '<div class="p-3 rounded" style="background:#f8fafc;border:1px solid #e2e8f0;">' +
                    '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Shipping Carbon</span><span class="fw-semibold">' + shipping.shippingCarbon + ' tonnes CO\u2082</span></div>' +
                    '<div class="d-flex justify-content-between"><span class="text-muted">Total (incl. pre-shipment)</span><span class="fw-bold fs-5">' + total.totalCarbon + ' tonnes CO\u2082</span></div>' +
                    '</div>'
                ).show();
                setLoading(btn, false, 'Calculate');
            }).fail(function () {
                showError('#order-lookup-form', 'Failed to look up order carbon.');
                setLoading(btn, false, 'Calculate');
                $('#order-result').hide();
            });
        });

        // Restock lookup
        $('#restock-lookup-form').on('submit', function (e) {
            e.preventDefault();
            var btn = $(this).find('button[type="submit"]');
            setLoading(btn, true);
            var restockId = $('#ship-restock-id').val();
            var country = $('#ship-restock-country').val();
            var postal = $('#ship-restock-postal').val();

            $.when(
                $.get('/api/carbon/restock/' + restockId + '/shipping'),
                $.get('/api/carbon/restock/' + restockId + '/total?countryCode=' + encodeURIComponent(country) + '&postalCode=' + encodeURIComponent(postal))
            ).done(function (shippingRes, totalRes) {
                var shipping = shippingRes[0];
                var total = totalRes[0];
                $('#restock-result').html(
                    '<div class="p-3 rounded" style="background:#f8fafc;border:1px solid #e2e8f0;">' +
                    '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Shipping Carbon</span><span class="fw-semibold">' + shipping.shippingCarbon + ' tonnes CO\u2082</span></div>' +
                    '<div class="d-flex justify-content-between"><span class="text-muted">Total (incl. pre-shipment)</span><span class="fw-bold fs-5">' + total.totalCarbon + ' tonnes CO\u2082</span></div>' +
                    '</div>'
                ).show();
                setLoading(btn, false, 'Calculate');
            }).fail(function () {
                showError('#restock-lookup-form', 'Failed to look up restock carbon.');
                setLoading(btn, false, 'Calculate');
                $('#restock-result').hide();
            });
        });

        // Carbon logs toggle
        $('#toggle-logs').on('click', function (e) {
            e.preventDefault();
            var logsDiv = $('#carbon-logs');
            if (logsDiv.is(':visible')) {
                logsDiv.hide();
                $(this).text('View Carbon Calculation Logs');
                return;
            }
            $(this).text('Loading...');
            $.get('/api/carbon/logs', function (data) {
                var html = '<table class="table table-hover"><thead><tr>';
                if (data && data.length > 0) {
                    // Dynamic columns from first entry
                    var keys = Object.keys(data[0]);
                    keys.forEach(function (k) { html += '<th>' + k + '</th>'; });
                    html += '</tr></thead><tbody>';
                    data.forEach(function (row) {
                        html += '<tr>';
                        keys.forEach(function (k) { html += '<td>' + (row[k] != null ? row[k] : '—') + '</td>'; });
                        html += '</tr>';
                    });
                    html += '</tbody></table>';
                } else {
                    html = '<p class="text-muted text-center py-3">No logs available.</p>';
                }
                logsDiv.html(html).show();
                $('#toggle-logs').text('Hide Carbon Calculation Logs');
            }).fail(function () {
                showError('#shipping-container', 'Failed to load carbon logs.');
                $('#toggle-logs').text('View Carbon Calculation Logs');
            });
        });
    }

    // ========== ANALYSIS ==========
    function initAnalysis() {
        // Shipping recommendation
        $('#recommend-form').on('submit', function (e) {
            e.preventDefault();
            var btn = $(this).find('button[type="submit"]');
            setLoading(btn, true);
            var postal = $('#analysis-postal').val();
            var delivery = $('#analysis-delivery').val();
            var country = $('#analysis-country').val();

            $.get('/api/carbon-analysis/recommend?postalCode=' + encodeURIComponent(postal) +
                '&deliveryType=' + encodeURIComponent(delivery) +
                '&countryCode=' + encodeURIComponent(country),
                function (data) {
                    $('#recommend-result').html(
                        '<div class="p-3 rounded" style="background:#f0fdf4;border:1px solid #bbf7d0;">' +
                        '<strong>Recommended:</strong> ' + data.recommendation +
                        '</div>'
                    ).show();
                    setLoading(btn, false, 'Get Recommendation');
                }).fail(function () {
                    showError('#recommend-form', 'Failed to get recommendation.');
                    setLoading(btn, false, 'Get Recommendation');
                });
        });

        // Carbon level analysis
        $('#analyze-form').on('submit', function (e) {
            e.preventDefault();
            var btn = $(this).find('button[type="submit"]');
            setLoading(btn, true);
            var carbon = parseFloat($('#analyze-carbon').val());

            $.get('/api/carbon-analysis/analyze?totalCarbon=' + carbon, function (data) {
                var levelColor = { 'Low': '#16a34a', 'Medium': '#d97706', 'High': '#ef4444' };
                var c = levelColor[data.level] || '#64748b';
                $('#analyze-result').html(
                    '<div class="p-3 rounded" style="background:#f8fafc;border:1px solid #e2e8f0;">' +
                    '<span class="text-muted">Carbon Level:</span> <strong style="color:' + c + ';">' + data.level + '</strong>' +
                    ' <span class="text-muted">(' + data.totalCarbon + ' tonnes CO\u2082)</span></div>'
                ).show();
                setLoading(btn, false, 'Analyze');
            }).fail(function () {
                showError('#analyze-form', 'Failed to analyze carbon level.');
                setLoading(btn, false, 'Analyze');
            });
        });

        // Shipping estimate
        $('#estimate-form').on('submit', function (e) {
            e.preventDefault();
            var btn = $(this).find('button[type="submit"]');
            setLoading(btn, true);
            var method = $('#estimate-method').val();
            var distance = parseFloat($('#estimate-distance').val());

            $.get('/api/carbon-analysis/estimate?method=' + encodeURIComponent(method) + '&distanceKm=' + distance, function (data) {
                $('#estimate-result').html(
                    '<div class="p-3 rounded" style="background:#f8fafc;border:1px solid #e2e8f0;">' +
                    '<span class="text-muted">Estimated Carbon:</span> <strong>' + data.estimatedCarbon + ' tonnes CO\u2082</strong>' +
                    ' <span class="text-muted">(' + data.method + ', ' + data.distanceKm + ' km)</span></div>'
                ).show();
                setLoading(btn, false, 'Estimate');
            }).fail(function () {
                showError('#estimate-form', 'Failed to estimate carbon.');
                setLoading(btn, false, 'Estimate');
            });
        });
    }

    return {
        initDashboard: initDashboard,
        initEmployees: initEmployees,
        initBuildings: initBuildings,
        initShipping: initShipping,
        initAnalysis: initAnalysis
    };
})(jQuery);

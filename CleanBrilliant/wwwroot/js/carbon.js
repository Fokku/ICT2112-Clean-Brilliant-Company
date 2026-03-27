// Carbon Footprint Module — Frontend AJAX Logic
var Carbon = (function ($) {
    'use strict';
    var shippingLogsState = { page: 1, pageSize: 10 };

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

    function clearError(container) {
        $(container).find('.alert-danger').remove();
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

    function formatTimestamp(unixSeconds) {
        if (unixSeconds === null || unixSeconds === undefined || unixSeconds === '') {
            return 'N/A';
        }

        var date = new Date(parseFloat(unixSeconds) * 1000);
        if (isNaN(date.getTime())) {
            return 'N/A';
        }

        return date.toLocaleString();
    }

    function escapeHtml(value) {
        return $('<div>').text(value == null ? '' : value).html();
    }

    function renderShippingBreakdown(result) {
        var distanceText = result.distanceKm != null ? result.distanceKm + ' km' : 'N/A';
        var coefficientText = result.coefficient != null ? result.coefficient : 'N/A';
        var methodText = result.shippingMethod || 'N/A';

        return '' +
            '<div class="p-3 rounded" style="background:#f8fafc;border:1px solid #e2e8f0;">' +
            '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Shipping Carbon</span><span class="fw-semibold">' + result.shippingCarbon + ' tonnes CO\u2082</span></div>' +
            '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Shipping Method</span><span class="fw-semibold">' + methodText + '</span></div>' +
            '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Distance</span><span class="fw-semibold">' + distanceText + '</span></div>' +
            '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Coefficient</span><span class="fw-semibold">' + coefficientText + '</span></div>' +
            '<div class="mb-2"><span class="text-muted d-block">Calculation</span><span class="fw-semibold">' + result.formula + '</span></div>' +
            '<div class="d-flex justify-content-between"><span class="text-muted">Timestamp</span><span class="fw-semibold">' + formatTimestamp(result.timestamp) + '</span></div>' +
            '</div>';
    }

    function renderTotalSummary(result) {
        return '' +
            '<div class="p-3 rounded" style="background:#f8fafc;border:1px solid #e2e8f0;">' +
            '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Pre-Order Carbon</span><span class="fw-semibold">' + result.preOrderCarbon + ' tonnes CO\u2082</span></div>' +
            '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Shipping Carbon</span><span class="fw-semibold">' + result.shippingCarbon + ' tonnes CO\u2082</span></div>' +
            '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Total Carbon</span><span class="fw-semibold">' + result.totalCarbon + ' tonnes CO\u2082</span></div>' +
            '<div class="mb-2"><span class="text-muted d-block">Calculation</span><span class="fw-semibold">' + result.formula + '</span></div>' +
            '<div class="d-flex justify-content-between"><span class="text-muted">Timestamp</span><span class="fw-semibold">' + formatTimestamp(result.timestamp) + '</span></div>' +
            '</div>';
    }

    function renderRouteSummary(result, type) {
        var idLabel = type === 'outbound' ? 'Order ID' : 'Restock ID';
        var idValue = type === 'outbound' ? result.orderID : result.restockID;
        var routeIdLine = type === 'outbound' && result.customerRouteDistID
            ? '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Route Dist ID</span><span class="fw-semibold">' + result.customerRouteDistID + '</span></div>'
            : '';

        return '' +
            '<div class="p-3 rounded" style="background:#f8fafc;border:1px solid #e2e8f0;">' +
            '<div class="d-flex justify-content-between mb-2"><span class="text-muted">' + idLabel + '</span><span class="fw-semibold">' + idValue + '</span></div>' +
            routeIdLine +
            '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Distance</span><span class="fw-semibold">' + (result.distanceKm != null ? result.distanceKm + ' km' : 'N/A') + '</span></div>' +
            '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Duration</span><span class="fw-semibold">' + (result.durationMin != null ? result.durationMin + ' min' : 'N/A') + '</span></div>' +
            '<div class="d-flex justify-content-between"><span class="text-muted">Timestamp</span><span class="fw-semibold">' + formatTimestamp(result.timestamp) + '</span></div>' +
            '</div>';
    }

    function renderRouteLoggingTable(result, type) {
        var isOutbound = type === 'outbound';
        var idLabel = isOutbound ? 'Order ID' : 'Restock ID';
        var idValue = isOutbound ? result.orderID : result.restockID;
        var routeDistId = isOutbound ? result.customerRouteDistID : result.supplierRouteDistID;
        var rows = [
            { label: idLabel, value: idValue || 'N/A' },
            { label: 'Route Dist ID', value: routeDistId || 'N/A' },
            { label: 'Logged Distance', value: result.distanceKm != null ? result.distanceKm + ' km' : 'N/A' },
            { label: 'Logged Duration', value: result.durationMin != null ? result.durationMin + ' min' : 'N/A' },
            { label: 'Timestamp', value: formatTimestamp(result.timestamp) }
        ];

        return renderLoggingTable('Logged Route Record', rows);
    }

    function renderHubLookup(result) {
        var routeChain = result.routeChain || result.formula;
        var loggingHtml = result.orderID || result.loggedTransportCarbon != null
            ? '<div class="p-2 rounded mb-2" style="background:#eff6ff;border:1px solid #bfdbfe;">' +
                '<div><strong>Operational logging completed</strong></div>' +
                (result.orderID ? '<div class="text-muted" style="font-size:.9rem;">Order ID: ' + result.orderID + '</div>' : '') +
                (result.loggedTransportCarbon != null ? '<div class="text-muted" style="font-size:.9rem;">Logged transport carbon: ' + result.loggedTransportCarbon + ' tonnes CO\u2082</div>' : '') +
              '</div>'
            : '';
        var legsHtml = '';
        if (Array.isArray(result.legs)) {
            result.legs.forEach(function (leg, index) {
                legsHtml += '' +
                    '<div class="p-3 rounded mt-2" style="background:#ffffff;border:1px solid #dbe4ee;">' +
                    '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Leg ' + (index + 1) + '</span><span class="fw-semibold text-uppercase">' + leg.method + '</span></div>' +
                    '<div class="d-flex justify-content-between mb-2"><span class="text-muted">From</span><span class="fw-semibold">' + leg.source + '</span></div>' +
                    '<div class="d-flex justify-content-between mb-2"><span class="text-muted">To</span><span class="fw-semibold">' + leg.destination + '</span></div>' +
                    '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Distance</span><span class="fw-semibold">' + leg.distanceKm + ' km</span></div>' +
                    '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Duration</span><span class="fw-semibold">' + leg.durationMin + ' min</span></div>' +
                    '<div class="d-flex justify-content-between"><span class="text-muted">Calculation</span><span class="fw-semibold">' + leg.calculationType + '</span></div>' +
                    '</div>';
            });
        }

        return '' +
            '<div class="p-3 rounded" style="background:#f8fafc;border:1px solid #e2e8f0;">' +
            loggingHtml +
            '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Company</span><span class="fw-semibold">' + result.companyName + '</span></div>' +
            '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Customer Location</span><span class="fw-semibold">' + result.customerPostalCode + '</span></div>' +
            '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Country</span><span class="fw-semibold">' + result.countryCode + '</span></div>' +
            '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Selected Method</span><span class="fw-semibold">' + result.shippingMethod + '</span></div>' +
            '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Total Distance</span><span class="fw-semibold">' + result.distanceKm + ' km</span></div>' +
            '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Total Duration</span><span class="fw-semibold">' + result.durationMin + ' min</span></div>' +
            '<div class="mb-2"><span class="text-muted d-block">Route Chain</span><span class="fw-semibold">' + routeChain + '</span></div>' +
            legsHtml +
            '</div>';
    }

    function renderTotalCalculation(result) {
        return '' +
            '<div class="p-3 rounded" style="background:#f8fafc;border:1px solid #e2e8f0;">' +
            '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Pre-Shipment Carbon</span><span class="fw-semibold">' + result.preShipmentCarbon + ' tonnes CO\u2082</span></div>' +
            '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Shipping Carbon</span><span class="fw-semibold">' + result.shippingCarbon + ' tonnes CO\u2082</span></div>' +
            '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Total Carbon</span><span class="fw-semibold">' + result.totalCarbon + ' tonnes CO\u2082</span></div>' +
            '<div><span class="text-muted d-block">Calculation</span><span class="fw-semibold">' + escapeHtml(result.formula) + '</span></div>' +
            '</div>';
    }

    function renderLiveShippingTest(result, heading) {
        var routeChain = result.routeChain || result.formula || 'N/A';
        var carbonHtml = result.loggedTransportCarbon != null
            ? '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Calculated Shipping Carbon</span><span class="fw-semibold">' + result.loggedTransportCarbon + ' tonnes CO\u2082</span></div>'
            : '';
        var legsHtml = '';

        if (Array.isArray(result.legs) && result.legs.length > 0) {
            legsHtml = '<div class="mt-3"><div class="text-muted mb-2">' + heading + ' Legs</div>';
            result.legs.forEach(function (leg, index) {
                legsHtml += '' +
                    '<div class="p-2 rounded mb-2" style="background:#ffffff;border:1px solid #dbe4ee;">' +
                    '<div><strong>Leg ' + (index + 1) + ' (' + escapeHtml(leg.method) + ')</strong></div>' +
                    '<div class="text-muted" style="font-size:.9rem;">' + escapeHtml(leg.source) + ' -> ' + escapeHtml(leg.destination) + '</div>' +
                    '<div class="text-muted" style="font-size:.9rem;">' + leg.distanceKm + ' km | ' + leg.durationMin + ' min | ' + escapeHtml(leg.calculationType || 'N/A') + '</div>' +
                    '</div>';
            });
            legsHtml += '</div>';
        }

        return '' +
            '<div class="p-3 rounded" style="background:#f8fafc;border:1px solid #e2e8f0;">' +
            '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Shipping Method</span><span class="fw-semibold">' + escapeHtml(result.shippingMethod || 'truck') + '</span></div>' +
            '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Total Distance</span><span class="fw-semibold">' + (result.distanceKm != null ? result.distanceKm + ' km' : 'N/A') + '</span></div>' +
            '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Total Duration</span><span class="fw-semibold">' + (result.durationMin != null ? result.durationMin + ' min' : 'N/A') + '</span></div>' +
            carbonHtml +
            '<div class="mb-2"><span class="text-muted d-block">Route Chain</span><span class="fw-semibold">' + escapeHtml(routeChain) + '</span></div>' +
            legsHtml +
            '</div>';
    }

    function renderLoggingTable(title, rows) {
        var body = rows.map(function (row) {
            return '<tr><th style="width:45%;">' + escapeHtml(row.label) + '</th><td>' + escapeHtml(row.value) + '</td></tr>';
        }).join('');

        return '' +
            '<div class="p-3 rounded" style="background:#fff;border:1px solid #dbe4ee;">' +
            '<div class="fw-semibold mb-2">' + escapeHtml(title) + '</div>' +
            '<div class="table-responsive">' +
            '<table class="table table-sm mb-0">' +
            '<tbody>' + body + '</tbody>' +
            '</table>' +
            '</div>' +
            '</div>';
    }

    function renderDistanceCarbonLogs(rows) {
        if (!rows || rows.length === 0) {
            return '<p class="text-muted text-center py-3 mb-0">No distance carbon logs available.</p>';
        }

        var html = '' +
            '<div class="table-responsive">' +
            '<table class="table table-hover align-middle">' +
            '<thead><tr>' +
            '<th>Reference</th>' +
            '<th>Type</th>' +
            '<th>Method</th>' +
            '<th>Carbon</th>' +
            '<th>Timestamp</th>' +
            '</tr></thead><tbody>';

        rows.forEach(function (row) {
            html += '' +
                '<tr>' +
                '<td>' + escapeHtml(row.referenceId) + '</td>' +
                '<td class="text-capitalize">' + escapeHtml(row.recordType) + '</td>' +
                '<td>' + escapeHtml(row.shippingMethod) + '</td>' +
                '<td>' + escapeHtml(row.carbonAmount) + '</td>' +
                '<td>' + escapeHtml(formatTimestamp(row.timestamp)) + '</td>' +
                '</tr>';
        });

        html += '</tbody></table></div>';
        return html;
    }

    function renderRouteLogs(rows) {
        if (!rows || rows.length === 0) {
            return '<p class="text-muted text-center py-3 mb-0">No route logs available.</p>';
        }

        var html = '' +
            '<div class="table-responsive">' +
            '<table class="table table-hover align-middle">' +
            '<thead><tr>' +
            '<th>Reference</th>' +
            '<th>Type</th>' +
            '<th>Route Dist ID</th>' +
            '<th>Distance</th>' +
            '<th>Duration</th>' +
            '<th>Timestamp</th>' +
            '</tr></thead><tbody>';

        rows.forEach(function (row) {
            html += '' +
                '<tr>' +
                '<td>' + escapeHtml(row.referenceId) + '</td>' +
                '<td class="text-capitalize">' + escapeHtml(row.recordType) + '</td>' +
                '<td>' + escapeHtml(row.routeDistId) + '</td>' +
                '<td>' + (row.distanceKm != null ? escapeHtml(row.distanceKm) + ' km' : 'N/A') + '</td>' +
                '<td>' + (row.durationMin != null ? escapeHtml(row.durationMin) + ' min' : 'N/A') + '</td>' +
                '<td>' + escapeHtml(formatTimestamp(row.timestamp)) + '</td>' +
                '</tr>';
        });

        html += '</tbody></table></div>';
        return html;
    }

    function showOutboundLoggingProof(orderId) {
        var target = $('#live-outbound-logging-result');
        if (!orderId) {
            target.hide().empty();
            return;
        }

        $.when(
            $.get('/api/carbon/order/' + orderId + '/shipping'),
            $.get('/api/carbon/order/' + orderId + '/outbound-summary')
        ).done(function (shippingResponse, summaryResponse) {
            var shipping = shippingResponse[0];
            var summary = summaryResponse[0];
            target.html(renderLoggingTable('Logging Proof', [
                { label: 'Order ID', value: orderId },
                { label: 'Shipping Method', value: shipping.shippingMethod || 'N/A' },
                { label: 'Logged Shipping Carbon', value: shipping.shippingCarbon + ' tonnes CO2' },
                { label: 'Logged Distance', value: summary.distanceKm != null ? summary.distanceKm + ' km' : 'N/A' },
                { label: 'Logged Duration', value: summary.durationMin != null ? summary.durationMin + ' min' : 'N/A' },
                { label: 'Timestamp', value: formatTimestamp(shipping.timestamp || summary.timestamp) }
            ])).show();
        }).fail(function () {
            target.html(renderLoggingTable('Logging Proof', [
                { label: 'Order ID', value: orderId },
                { label: 'Status', value: 'Logging lookup failed or no stored record found.' }
            ])).show();
        });
    }

    function showInboundLoggingProof(restockId) {
        var target = $('#live-inbound-logging-result');
        if (!restockId) {
            target.hide().empty();
            return;
        }

        $.when(
            $.get('/api/carbon/restock/' + restockId + '/shipping'),
            $.get('/api/carbon/restock/' + restockId + '/inbound-summary')
        ).done(function (shippingResponse, summaryResponse) {
            var shipping = shippingResponse[0];
            var summary = summaryResponse[0];
            target.html(renderLoggingTable('Logging Proof', [
                { label: 'Restock ID', value: restockId },
                { label: 'Shipping Method', value: shipping.shippingMethod || 'N/A' },
                { label: 'Logged Shipping Carbon', value: shipping.shippingCarbon + ' tonnes CO2' },
                { label: 'Logged Distance', value: summary.distanceKm != null ? summary.distanceKm + ' km' : 'N/A' },
                { label: 'Logged Duration', value: summary.durationMin != null ? summary.durationMin + ' min' : 'N/A' },
                { label: 'Timestamp', value: formatTimestamp(shipping.timestamp || summary.timestamp) }
            ])).show();
        }).fail(function () {
            target.html(renderLoggingTable('Logging Proof', [
                { label: 'Restock ID', value: restockId },
                { label: 'Status', value: 'Logging lookup failed or no stored record found.' }
            ])).show();
        });
    }

    function renderRecommendation(result) {
        var strategyLabel = result.strategy === 'duration' ? 'Route Duration' : 'Current Rules';
        var html = '' +
            '<div><strong>Recommended:</strong> ' + result.recommendation + '</div>' +
            '<div class="text-muted mt-1">Mode: ' + strategyLabel + '</div>';

        if (result.strategy === 'duration' && Array.isArray(result.candidates) && result.candidates.length > 0) {
            html += '<div class="mt-3">';
            result.candidates.forEach(function (candidate) {
                html += '' +
                    '<div class="p-2 rounded mb-2" style="background:#ffffff;border:1px solid #dbe4ee;">' +
                    '<div><strong>' + candidate.method + '</strong></div>' +
                    '<div class="text-muted" style="font-size:.9rem;">Duration: ' + candidate.durationMin.toFixed(2) + ' min | Speed: ' + candidate.averageSpeedKmPerHour.toFixed(2) + ' km/h | Carbon: ' + candidate.estimatedCarbon.toFixed(2) + ' tonnes CO\u2082</div>' +
                    '<div class="text-muted" style="font-size:.9rem;">Route: ' + candidate.routeChain + '</div>' +
                    '</div>';
            });
            html += '</div>';
        }

        return html;
    }

    function renderInboundLookup(result) {
        var routeChain = result.routeChain || (result.warehouseName + ' -> ' + result.companyName);
        var loggingHtml = result.restockID || result.loggedTransportCarbon != null
            ? '<div class="p-2 rounded mb-2" style="background:#eff6ff;border:1px solid #bfdbfe;">' +
                '<div><strong>Operational logging completed</strong></div>' +
                (result.restockID ? '<div class="text-muted" style="font-size:.9rem;">Restock ID: ' + result.restockID + '</div>' : '') +
                (result.loggedTransportCarbon != null ? '<div class="text-muted" style="font-size:.9rem;">Logged transport carbon: ' + result.loggedTransportCarbon + ' tonnes CO\u2082</div>' : '') +
              '</div>'
            : '';
        return '' +
            '<div class="p-3 rounded" style="background:#f8fafc;border:1px solid #e2e8f0;">' +
            loggingHtml +
            '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Company</span><span class="fw-semibold">' + result.companyName + '</span></div>' +
            '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Warehouse</span><span class="fw-semibold">' + result.warehouseName + '</span></div>' +
            '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Method</span><span class="fw-semibold">' + result.shippingMethod + '</span></div>' +
            '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Distance</span><span class="fw-semibold">' + result.distanceKm + ' km</span></div>' +
            '<div class="d-flex justify-content-between mb-2"><span class="text-muted">Duration</span><span class="fw-semibold">' + result.durationMin + ' min</span></div>' +
            '<div class="mb-2"><span class="text-muted d-block">Route</span><span class="fw-semibold">' + routeChain + '</span></div>' +
            '<div class="mb-2"><span class="text-muted d-block">Formula</span><span class="fw-semibold">' + result.formula + '</span></div>' +
            '</div>';
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

    function initRouteSummary() {
        $('#dashboard-outbound-form').on('submit', function (e) {
            e.preventDefault();
            var btn = $(this).find('button[type="submit"]');
            setLoading(btn, true);
            var orderId = $('#dashboard-order-id').val();

            $.get('/api/carbon/order/' + orderId + '/outbound-summary', function (summary) {
                clearError('#dashboard-outbound-form');
                $('#dashboard-outbound-result').html(renderRouteSummary(summary, 'outbound')).show();
                $('#dashboard-outbound-logging-result').html(renderRouteLoggingTable(summary, 'outbound')).show();
                setLoading(btn, false, 'Search');
            }).fail(function (xhr) {
                var message = xhr.responseJSON?.message || 'Invalid order ID.';
                showError('#dashboard-outbound-form', message);
                setLoading(btn, false, 'Search');
                $('#dashboard-outbound-result').hide();
                $('#dashboard-outbound-logging-result').hide().empty();
            });
        });

        $('#dashboard-inbound-form').on('submit', function (e) {
            e.preventDefault();
            var btn = $(this).find('button[type="submit"]');
            setLoading(btn, true);
            var restockId = $('#dashboard-restock-id').val();

            $.get('/api/carbon/restock/' + restockId + '/inbound-summary', function (summary) {
                clearError('#dashboard-inbound-form');
                $('#dashboard-inbound-result').html(renderRouteSummary(summary, 'inbound')).show();
                $('#dashboard-inbound-logging-result').html(renderRouteLoggingTable(summary, 'inbound')).show();
                setLoading(btn, false, 'Search');
            }).fail(function (xhr) {
                var message = xhr.responseJSON?.message || 'Invalid restock ID.';
                showError('#dashboard-inbound-form', message);
                setLoading(btn, false, 'Search');
                $('#dashboard-inbound-result').hide();
                $('#dashboard-inbound-logging-result').hide().empty();
            });
        });

        $('#toggle-route-logs').on('click', function (e) {
            e.preventDefault();
            var logsDiv = $('#route-logs');
            if (logsDiv.is(':visible')) {
                logsDiv.hide();
                $(this).text('View Route Logs');
                return;
            }

            $(this).text('Loading...');
            $.get('/api/carbon/route-logs', function (data) {
                logsDiv.html(renderRouteLogs(data)).show();
                $('#toggle-route-logs').text('Hide Route Logs');
            }).fail(function () {
                showError('#route-summary-container', 'Failed to load route logs.');
                $('#toggle-route-logs').text('View Route Logs');
                logsDiv.hide();
            });
        });
    }

    function initHubLookup() {
        $.get('/api/carbon/inbound-warehouses', function (data) {
            bindWarehouseOptions($('#warehouse-select'), data, 'Select a warehouse');
            bindWarehouseOptions($('#inbound-warehouse-select'), data, 'Select a warehouse');
        }).fail(function () {
            showError('#hub-lookup-container', 'Failed to load warehouse list.');
            $('#warehouse-select').html('<option value="">Failed to load warehouses</option>');
            $('#inbound-warehouse-select').html('<option value="">Failed to load warehouses</option>');
        });

        $('#country-code').on('change', function () {
            var isSingapore = $(this).val() === 'SG';
            var methodSelect = $('#transport-method');
            methodSelect.val('truck');
            methodSelect.find('option').prop('disabled', false);
            if (isSingapore) {
                methodSelect.find('option').not('[value="truck"]').prop('disabled', true);
            }
        }).trigger('change');

        $('#hub-lookup-form').on('submit', function (e) {
            e.preventDefault();
            var btn = $(this).find('button[type="submit"]');
            setLoading(btn, true);

            var customerPostalCode = $('#customer-postal-code').val();
            var countryCode = $('#country-code').val();
            var method = $('#transport-method').val();
            var orderId = $('#outbound-order-id').val();

            $.get('/api/carbon/outbound-route', {
                customerPostalCode: customerPostalCode,
                method: method,
                countryCode: countryCode,
                orderID: orderId
            }, function (result) {
                clearError('#hub-lookup-container');
                $('#hub-lookup-result').html(renderHubLookup(result)).show();
                setLoading(btn, false, 'Calculate Distance');
            }).fail(function (xhr) {
                var message = xhr.responseJSON?.message || 'Failed to calculate outbound route.';
                showError('#hub-lookup-container', message);
                setLoading(btn, false, 'Calculate Distance');
                $('#hub-lookup-result').hide();
            });
        });

        $('#inbound-route-form').on('submit', function (e) {
            e.preventDefault();
            var btn = $(this).find('button[type="submit"]');
            setLoading(btn, true);

            var warehouseId = $('#inbound-warehouse-select').val();
            var restockId = $('#inbound-restock-id').val();
            if (!warehouseId) {
                showError('#hub-lookup-container', 'Select a warehouse for inbound testing.');
                setLoading(btn, false, 'Test Inbound Route');
                $('#inbound-route-result').hide();
                return;
            }

            $.get('/api/carbon/inbound-route', {
                warehouseId: warehouseId,
                restockID: restockId
            }, function (result) {
                clearError('#hub-lookup-container');
                $('#inbound-route-result').html(renderInboundLookup(result)).show();
                setLoading(btn, false, 'Test Inbound Route');
            }).fail(function (xhr) {
                var message = xhr.responseJSON?.message || 'Failed to calculate inbound route.';
                showError('#hub-lookup-container', message);
                setLoading(btn, false, 'Test Inbound Route');
                $('#inbound-route-result').hide();
            });
        });
    }

    function bindWarehouseOptions(select, data, placeholder) {
        select.empty();
        if (!data || data.length === 0) {
            select.append('<option value="">No warehouses available</option>');
            return;
        }

        select.append('<option value="">' + placeholder + '</option>');
        data.forEach(function (warehouse) {
            var label = warehouse.name + ' (' + warehouse.postalCode + ')';
            select.append(
                $('<option></option>')
                    .val(warehouse.id)
                    .text(label)
                    .attr('data-warehouse-name', warehouse.name)
                    .attr('data-postal-code', warehouse.postalCode)
            );
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
        $.get('/api/carbon/inbound-warehouses', function (data) {
            bindWarehouseOptions($('#live-inbound-warehouse'), data, 'Select a warehouse');
        }).fail(function () {
            $('#live-inbound-warehouse').html('<option value="">Failed to load warehouses</option>');
        });

        $('#live-outbound-country-code').on('change', function () {
            var isSingapore = $(this).val() === 'SG';
            var methodSelect = $('#live-outbound-method');
            methodSelect.val('truck');
            methodSelect.find('option').prop('disabled', false);
            if (isSingapore) {
                methodSelect.find('option').not('[value="truck"]').prop('disabled', true);
            }
        }).trigger('change');

        // Order lookup
        $('#order-lookup-form').on('submit', function (e) {
            e.preventDefault();
            var btn = $(this).find('button[type="submit"]');
            setLoading(btn, true);
            var orderId = $('#ship-order-id').val();

            $.get('/api/carbon/order/' + orderId + '/shipping', function (shipping) {
                clearError('#order-lookup-form');
                $('#order-result').html(renderShippingBreakdown(shipping)).show();
                setLoading(btn, false, 'Lookup');
            }).fail(function (xhr) {
                var message = xhr.responseJSON?.message || 'Invalid order ID.';
                showError('#order-lookup-form', message);
                setLoading(btn, false, 'Lookup');
                $('#order-result').hide();
            });
        });

        // Restock lookup
        $('#restock-lookup-form').on('submit', function (e) {
            e.preventDefault();
            var btn = $(this).find('button[type="submit"]');
            setLoading(btn, true);
            var restockId = $('#ship-restock-id').val();

            $.get('/api/carbon/restock/' + restockId + '/shipping', function (shipping) {
                clearError('#restock-lookup-form');
                $('#restock-result').html(renderShippingBreakdown(shipping)).show();
                setLoading(btn, false, 'Lookup');
            }).fail(function (xhr) {
                var message = xhr.responseJSON?.message || 'Invalid restock ID.';
                showError('#restock-lookup-form', message);
                setLoading(btn, false, 'Lookup');
                $('#restock-result').hide();
            });
        });

        $('#live-outbound-shipping-form').on('submit', function (e) {
            e.preventDefault();
            var btn = $(this).find('button[type="submit"]');
            setLoading(btn, true);

            var orderId = $('#live-outbound-order-id').val();
            var customerPostalCode = $('#live-outbound-customer-postal').val();
            var countryCode = $('#live-outbound-country-code').val();
            var method = $('#live-outbound-method').val();

            $.get('/api/carbon/outbound-route', {
                orderID: orderId,
                customerPostalCode: customerPostalCode,
                countryCode: countryCode,
                method: method
            }, function (result) {
                clearError('#live-outbound-shipping-form');
                $('#live-outbound-shipping-result').html(renderLiveShippingTest(result, 'Outbound')).show();
                showOutboundLoggingProof(orderId);
                setLoading(btn, false, 'Calculate');
            }).fail(function (xhr) {
                var message = xhr.responseJSON?.message || 'Failed to run live outbound test.';
                showError('#live-outbound-shipping-form', message);
                $('#live-outbound-shipping-result').hide();
                $('#live-outbound-logging-result').hide().empty();
                setLoading(btn, false, 'Calculate');
            });
        });

        $('#live-inbound-shipping-form').on('submit', function (e) {
            e.preventDefault();
            var btn = $(this).find('button[type="submit"]');
            setLoading(btn, true);

            var restockId = $('#live-inbound-restock-id').val();
            var warehouseId = $('#live-inbound-warehouse').val();

            $.get('/api/carbon/inbound-route', {
                restockID: restockId,
                warehouseId: warehouseId
            }, function (result) {
                clearError('#live-inbound-shipping-form');
                $('#live-inbound-shipping-result').html(renderLiveShippingTest(result, 'Inbound')).show();
                showInboundLoggingProof(restockId);
                setLoading(btn, false, 'Calculate');
            }).fail(function (xhr) {
                var message = xhr.responseJSON?.message || 'Failed to run live inbound test.';
                showError('#live-inbound-shipping-form', message);
                $('#live-inbound-shipping-result').hide();
                $('#live-inbound-logging-result').hide().empty();
                setLoading(btn, false, 'Calculate');
            });
        });

        $('#order-summary-form').on('submit', function (e) {
            e.preventDefault();
            var btn = $(this).find('button[type="submit"]');
            setLoading(btn, true);
            var orderId = $('#summary-order-id').val();

            $.get('/api/carbon/order/' + orderId + '/summary', function (summary) {
                clearError('#order-summary-form');
                $('#order-summary-result').html(renderTotalSummary(summary)).show();
                setLoading(btn, false, 'Lookup');
            }).fail(function (xhr) {
                var message = xhr.responseJSON?.message || 'Invalid order ID.';
                showError('#order-summary-form', message);
                setLoading(btn, false, 'Lookup');
                $('#order-summary-result').hide();
            });
        });

        $('#order-total-calc-form').on('submit', function (e) {
            e.preventDefault();
            var btn = $(this).find('button[type="submit"]');
            setLoading(btn, true);
            var preShipmentCarbon = parseFloat($('#calc-pre-shipment-carbon').val());
            var shippingCarbon = parseFloat($('#calc-shipping-carbon').val());

            if (isNaN(preShipmentCarbon) || isNaN(shippingCarbon)) {
                showError('#order-total-calc-form', 'Both pre-shipment carbon and shipping carbon are required.');
                setLoading(btn, false, 'Calculate');
                $('#order-total-calc-result').hide();
                return;
            }

            clearError('#order-total-calc-form');
            $('#order-total-calc-result').html(renderTotalCalculation({
                preShipmentCarbon: preShipmentCarbon.toFixed(4),
                shippingCarbon: shippingCarbon.toFixed(4),
                totalCarbon: (preShipmentCarbon + shippingCarbon).toFixed(4),
                formula: preShipmentCarbon.toFixed(4) + ' + ' + shippingCarbon.toFixed(4) + ' = ' + (preShipmentCarbon + shippingCarbon).toFixed(4) + ' tonnes CO2'
            })).show();
            setLoading(btn, false, 'Calculate');
        });

        $('#restock-summary-form').on('submit', function (e) {
            e.preventDefault();
            var btn = $(this).find('button[type="submit"]');
            setLoading(btn, true);
            var restockId = $('#summary-restock-id').val();

            $.get('/api/carbon/restock/' + restockId + '/summary', function (summary) {
                clearError('#restock-summary-form');
                $('#restock-summary-result').html(renderTotalSummary(summary)).show();
                setLoading(btn, false, 'Lookup');
            }).fail(function (xhr) {
                var message = xhr.responseJSON?.message || 'Invalid restock ID.';
                showError('#restock-summary-form', message);
                setLoading(btn, false, 'Lookup');
                $('#restock-summary-result').hide();
            });
        });

        $('#restock-total-calc-form').on('submit', function (e) {
            e.preventDefault();
            var btn = $(this).find('button[type="submit"]');
            setLoading(btn, true);
            var preShipmentCarbon = parseFloat($('#calc-restock-pre-shipment-carbon').val());
            var shippingCarbon = parseFloat($('#calc-restock-shipping-carbon').val());

            if (isNaN(preShipmentCarbon) || isNaN(shippingCarbon)) {
                showError('#restock-total-calc-form', 'Both pre-shipment carbon and shipping carbon are required.');
                setLoading(btn, false, 'Calculate');
                $('#restock-total-calc-result').hide();
                return;
            }

            clearError('#restock-total-calc-form');
            $('#restock-total-calc-result').html(renderTotalCalculation({
                preShipmentCarbon: preShipmentCarbon.toFixed(4),
                shippingCarbon: shippingCarbon.toFixed(4),
                totalCarbon: (preShipmentCarbon + shippingCarbon).toFixed(4),
                formula: preShipmentCarbon.toFixed(4) + ' + ' + shippingCarbon.toFixed(4) + ' = ' + (preShipmentCarbon + shippingCarbon).toFixed(4) + ' tonnes CO2'
            })).show();
            setLoading(btn, false, 'Calculate');
        });

        $('#toggle-distance-logs').on('click', function (e) {
            e.preventDefault();
            var logsDiv = $('#distance-carbon-logs');
            if (logsDiv.is(':visible')) {
                logsDiv.hide();
                $(this).text('View Distance Carbon Logs');
                return;
            }

            $(this).text('Loading...');
            $.get('/api/carbon/distance-logs', function (data) {
                logsDiv.html(renderDistanceCarbonLogs(data)).show();
                $('#toggle-distance-logs').text('Hide Distance Carbon Logs');
            }).fail(function () {
                showError('#shipping-container', 'Failed to load distance carbon logs.');
                $('#toggle-distance-logs').text('View Distance Carbon Logs');
                logsDiv.hide();
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
            var strategy = $('#analysis-strategy').val();

            $.get('/api/carbon-analysis/recommend?postalCode=' + encodeURIComponent(postal) +
                '&deliveryType=' + encodeURIComponent(delivery) +
                '&countryCode=' + encodeURIComponent(country) +
                '&strategy=' + encodeURIComponent(strategy),
                function (data) {
                    $('#recommend-result').html(
                        '<div class="p-3 rounded" style="background:#f0fdf4;border:1px solid #bbf7d0;">' +
                        renderRecommendation(data) +
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
        initRouteSummary: initRouteSummary,
        initHubLookup: initHubLookup,
        initEmployees: initEmployees,
        initBuildings: initBuildings,
        initShipping: initShipping,
        initAnalysis: initAnalysis
    };
})(jQuery);

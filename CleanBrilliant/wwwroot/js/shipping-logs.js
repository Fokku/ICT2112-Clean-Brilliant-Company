(function ($) {
    'use strict';

    var state = { page: 1, pageSize: 10 };

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

    function renderLogsPage(data) {
        if (!data || !data.items || data.items.length === 0) {
            return '<p class="text-muted text-center py-3 mb-0">No logs available.</p>';
        }

        var html = '' +
            '<div class="table-responsive">' +
            '<table class="table table-hover align-middle">' +
            '<thead><tr>' +
            '<th>Reference</th>' +
            '<th>Type</th>' +
            '<th>Total</th>' +
            '<th>Pre-Shipment</th>' +
            '<th>Shipping</th>' +
            '<th>Timestamp</th>' +
            '<th>Formula</th>' +
            '</tr></thead><tbody>';

        data.items.forEach(function (row) {
            html += '' +
                '<tr>' +
                '<td>' + escapeHtml(row.referenceId) + '</td>' +
                '<td class="text-capitalize">' + escapeHtml(row.recordType) + '</td>' +
                '<td>' + escapeHtml(row.totalCarbon) + '</td>' +
                '<td>' + (row.preShipmentCarbon != null ? escapeHtml(row.preShipmentCarbon) : '—') + '</td>' +
                '<td>' + (row.shippingCarbon != null ? escapeHtml(row.shippingCarbon) : '—') + '</td>' +
                '<td>' + escapeHtml(formatTimestamp(row.timestamp)) + '</td>' +
                '<td>' + escapeHtml(row.formula) + '</td>' +
                '</tr>';
        });

        html += '</tbody></table></div>';
        html += '' +
            '<div class="d-flex justify-content-between align-items-center mt-3">' +
            '<div class="text-muted" style="font-size:.9rem;">Page ' + data.page + ' of ' + Math.max(data.totalPages, 1) + ' • ' + data.totalCount + ' record(s)</div>' +
            '<div class="d-flex gap-2">' +
            '<button type="button" class="btn btn-outline-secondary btn-sm" data-log-page="' + (data.page - 1) + '"' + (data.page <= 1 ? ' disabled' : '') + '>Previous</button>' +
            '<button type="button" class="btn btn-outline-secondary btn-sm" data-log-page="' + (data.page + 1) + '"' + (data.page >= data.totalPages ? ' disabled' : '') + '>Next</button>' +
            '</div>' +
            '</div>';

        return html;
    }

    function showError(message) {
        $('#shipping-container').find('.alert-danger').remove();
        $('#shipping-container').prepend(
            '<div class="alert alert-danger alert-dismissible fade show" role="alert">' +
            escapeHtml(message) +
            '<button type="button" class="btn-close" data-bs-dismiss="alert"></button></div>'
        );
    }

    function loadLogs(page) {
        var logsDiv = $('#carbon-logs');
        state.page = page;
        logsDiv.html('<p class="text-muted text-center py-3 mb-0">Loading logs...</p>').show();

        $.get('/api/carbon/logs', { page: page, pageSize: state.pageSize }, function (data) {
            logsDiv.html(renderLogsPage(data)).show();
            $('#toggle-logs').text('Hide Carbon Calculation Logs');
        }).fail(function () {
            showError('Failed to load carbon logs.');
            $('#toggle-logs').text('View Carbon Calculation Logs');
            logsDiv.hide();
        });
    }

    $(function () {
        if (!$('#shipping-container').length) {
            return;
        }

        $('#toggle-logs').off('click').on('click', function (e) {
            e.preventDefault();
            var logsDiv = $('#carbon-logs');
            if (logsDiv.is(':visible')) {
                logsDiv.hide();
                $(this).text('View Carbon Calculation Logs');
                return;
            }

            $(this).text('Loading...');
            state.page = 1;
            loadLogs(state.page);
        });

        $('#carbon-logs').off('click', '[data-log-page]').on('click', '[data-log-page]', function (e) {
            e.preventDefault();
            var page = parseInt($(this).attr('data-log-page'), 10);
            if (!page || $(this).prop('disabled')) {
                return;
            }

            loadLogs(page);
        });
    });
})(jQuery);

/* ═══════════════════════════════════════════════════════════════
   Agile Task Manager — site.js
   Global JS: sidebar, SignalR notifications, utilities
═══════════════════════════════════════════════════════════════ */

document.addEventListener('DOMContentLoaded', () => {

    // ── Universal Modal Form-Reset (Phase 5 fix) ───────────────
    // Every Bootstrap modal that closes automatically resets its
    // forms to blank — prevents stale data on re-open.
    document.querySelectorAll('.modal').forEach(modalEl => {
        modalEl.addEventListener('hidden.bs.modal', () => {
            // Reset every <form> inside this modal
            modalEl.querySelectorAll('form').forEach(form => form.reset());

            // Deselect all <select multiple> options explicitly
            // (form.reset() alone sometimes leaves multi-select highlighted)
            modalEl.querySelectorAll('select[multiple]').forEach(sel => {
                Array.from(sel.options).forEach(opt => { opt.selected = false; });
            });

            // Remove any inline validation error elements injected by JS
            modalEl.querySelectorAll('#taskDateError, .inline-form-error').forEach(el => el.remove());

            // Reset modal title back to "Create Task" if it was switched to "Edit Task"
            const title = modalEl.querySelector('#taskModalLabel');
            if (title && title.textContent.trim() === 'Edit Task') {
                title.textContent = 'Create Task';
            }

            // Re-enable any fields that may have been disabled (e.g. timer running)
            modalEl.querySelectorAll('[disabled]').forEach(el => el.removeAttribute('disabled'));

            // Reset timer-specific state (Start/Stop button visibility)
            const startBtn = modalEl.querySelector('#startTimerBtn');
            const stopBtn  = modalEl.querySelector('#stopTimerBtn');
            if (startBtn) startBtn.classList.remove('d-none');
            if (stopBtn)  stopBtn.classList.add('d-none');

            // Reset timerDisplay back to 00:00:00
            const timerDisplay = modalEl.querySelector('#timerDisplay');
            if (timerDisplay) timerDisplay.textContent = '00:00:00';

            // Clear taskId hidden field so edits never bleed into new-task creates
            const taskId = modalEl.querySelector('#taskId');
            if (taskId) taskId.value = '';
        });
    });

    // ── Sidebar Toggle (mobile) ────────────────────────────────
    const toggleBtn = document.getElementById('sidebarToggle');
    const sidebar   = document.getElementById('sidebar');

    if (toggleBtn && sidebar) {
        toggleBtn.addEventListener('click', () => {
            sidebar.classList.toggle('open');
        });

        // Close sidebar when clicking outside on mobile
        document.addEventListener('click', e => {
            if (window.innerWidth <= 768
                && sidebar.classList.contains('open')
                && !sidebar.contains(e.target)
                && !toggleBtn.contains(e.target)) {
                sidebar.classList.remove('open');
            }
        });
    }

    // ── Auto-dismiss alerts after 4s ──────────────────────────
    document.querySelectorAll('.alert.alert-success').forEach(el => {
        setTimeout(() => {
            el.style.transition = 'opacity 0.4s';
            el.style.opacity    = '0';
            setTimeout(() => el.remove(), 400);
        }, 4000);
    });

    // ── Highlight active nav link (fallback) ──────────────────
    const path = window.location.pathname.toLowerCase();
    document.querySelectorAll('.atm-nav-link').forEach(link => {
        const href = link.getAttribute('href')?.toLowerCase() ?? '';
        if (href && href !== '/' && path.startsWith(href)) {
            link.classList.add('active');
        }
    });

    // ── Global SignalR for notifications ──────────────────────
    // Only connect if SignalR is available and user appears authenticated
    if (typeof signalR !== 'undefined' && document.getElementById('sidebar')) {
        const notifConnection = new signalR.HubConnectionBuilder()
            .withUrl('/taskHub')
            .withAutomaticReconnect([0, 2000, 5000, 10000])
            .build();

        notifConnection.start()
            .then(() => {
                updateSignalRStatus(true);
                console.log('[SignalR] Connected — notification hub active');
            })
            .catch(err => {
                updateSignalRStatus(false);
                console.warn('[SignalR] Connection failed:', err);
            });

        notifConnection.onreconnecting(() => updateSignalRStatus(false));
        notifConnection.onreconnected(() => updateSignalRStatus(true));

        // Receive global notification pushed to this user
        notifConnection.on('ReceiveNotification', (message) => {
            showGlobalToast(message, 'notification');
            bumpNotifBadge();
        });
    }

    // ── SignalR status indicator ───────────────────────────────
    function updateSignalRStatus(connected) {
        const el = document.getElementById('signalrStatus');
        if (!el) return;
        if (connected) {
            el.classList.add('connected');
        } else {
            el.classList.remove('connected');
        }
    }

    // ── Notification badge bump ────────────────────────────────
    function bumpNotifBadge() {
        const link  = document.querySelector('.atm-nav-link[href*="Notification"]');
        if (!link) return;
        let badge = link.querySelector('.atm-badge');
        if (!badge) {
            badge = document.createElement('span');
            badge.className = 'atm-badge';
            badge.textContent = '0';
            link.appendChild(badge);
        }
        const current = parseInt(badge.textContent) || 0;
        badge.textContent = current + 1;
        badge.style.animation = 'none';
        requestAnimationFrame(() => {
            badge.style.animation = '';
        });
    }

    // ── Global Toast ───────────────────────────────────────────
    window.showGlobalToast = function(msg, type = 'info') {
        const toast = document.createElement('div');
        toast.className = 'atm-toast';

        const iconMap = {
            notification : 'bi-bell-fill',
            success      : 'bi-check-circle-fill',
            error        : 'bi-exclamation-triangle-fill',
            warning      : 'bi-exclamation-circle-fill',
            info         : 'bi-info-circle-fill'
        };

        toast.innerHTML = `<i class="bi ${iconMap[type] ?? iconMap.info} me-2"></i>${msg}`;
        document.body.appendChild(toast);

        setTimeout(() => {
            toast.style.transition = 'opacity 0.4s, transform 0.4s';
            toast.style.opacity    = '0';
            toast.style.transform  = 'translateY(10px)';
            setTimeout(() => toast.remove(), 400);
        }, 3500);
    };

    // ── Confirm forms (data-confirm attribute) ─────────────────
    document.querySelectorAll('form[data-confirm]').forEach(form => {
        form.addEventListener('submit', e => {
            const msg = form.dataset.confirm || 'Are you sure?';
            if (!confirm(msg)) e.preventDefault();
        });
    });

    // ── Task status badge live update helper ───────────────────
    // Called from Task/Details.cshtml SignalR handler
    window.updateTaskStatusBadge = function(newStatus) {
        const badge = document.getElementById('liveStatus');
        if (!badge) return;
        const classMap = {
            'ToDo'       : 'todo',
            'InProgress' : 'inprogress',
            'Done'       : 'done'
        };
        badge.textContent = newStatus;
        badge.className   = `atm-status ${classMap[newStatus] ?? 'todo'}`;
    };

    // ── Table row hover effect helper ──────────────────────────
    document.querySelectorAll('.atm-table tbody tr').forEach(row => {
        row.style.transition = 'background 0.12s';
    });

    // ── Responsive: close sidebar on nav link click (mobile) ──
    document.querySelectorAll('.atm-nav-link').forEach(link => {
        link.addEventListener('click', () => {
            if (window.innerWidth <= 768 && sidebar) {
                sidebar.classList.remove('open');
            }
        });
    });

});

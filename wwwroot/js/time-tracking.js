/* ═══════════════════════════════════════════════════════════════
   Agile Task Manager — time-tracking.js
   Time tracking with timers, manual entries, and analytics
═══════════════════════════════════════════════════════════════ */

window.timeTracking = {
    dashboardData: null,
    activeTimer: null,
    timerInterval: null,
    currentChart: null,
    currentChartView: 'daily',
    isLoading: false,

    init: async function() {
        await this.loadDashboard();
        this.setupEventListeners();
        this.startAutoRefresh();
    },

    setupEventListeners: function() {
        // Keyboard shortcuts
        document.addEventListener('keydown', (e) => {
            if (e.ctrlKey || e.metaKey) {
                switch (e.key) {
                    case 't':
                        e.preventDefault();
                        this.showTimer();
                        break;
                    case 'r':
                        e.preventDefault();
                        this.refresh();
                        break;
                    case 'e':
                        e.preventDefault();
                        this.showAddEntryModal();
                        break;
                }
            }
        });

        // Timer form changes
        document.getElementById('timerTask')?.addEventListener('change', (e) => {
            this.updateTimerTaskName();
        });

        // Entry form time calculation
        document.getElementById('entryStartTime')?.addEventListener('change', () => {
            this.calculateEntryDuration();
        });
        
        document.getElementById('entryEndTime')?.addEventListener('change', () => {
            this.calculateEntryDuration();
        });
    },

    loadDashboard: async function() {
        try {
            this.setLoading(true);
            this.hideError();

            const response = await fetch('/api/time-tracking/dashboard');
            if (!response.ok) throw new Error('Failed to load dashboard');

            this.dashboardData = await response.json();
            this.renderDashboard();
            this.setLoading(false);

        } catch (error) {
            console.error('Error loading dashboard:', error);
            this.showError(error.message);
            this.setLoading(false);
        }
    },

    renderDashboard: function() {
        if (!this.dashboardData) return;

        this.renderTodayStats();
        this.renderActiveTimer();
        this.renderTimeChart();
        this.renderTaskBreakdown();
        this.renderRecentEntries();
        
        document.getElementById('timeTrackingContent').classList.remove('d-none');
    },

    renderTodayStats: function() {
        const container = document.getElementById('todayStats');
        
        const stats = [
            {
                title: 'Today\'s Total',
                value: this.formatDuration(this.dashboardData.TodayTotalTime),
                icon: 'bi-clock',
                color: 'primary',
                subtitle: `${this.dashboardData.TodayEntryCount} entries`
            },
            {
                title: 'Week Total',
                value: this.formatDuration(this.dashboardData.WeekTotalTime),
                icon: 'bi-calendar-week',
                color: 'info',
                subtitle: `${this.dashboardData.WeekEntryCount} entries`
            },
            {
                title: 'Month Total',
                value: this.formatDuration(this.dashboardData.MonthTotalTime),
                icon: 'bi-calendar-month',
                color: 'success',
                subtitle: `${this.dashboardData.MonthEntryCount} entries`
            },
            {
                title: 'Active Timer',
                value: this.dashboardData.HasActiveTimer ? 'Running' : 'None',
                icon: this.dashboardData.HasActiveTimer ? 'bi-play-circle' : 'bi-stop-circle',
                color: this.dashboardData.HasActiveTimer ? 'success' : 'secondary',
                subtitle: this.dashboardData.HasActiveTimer ? '⏱️ Active' : '⏸️ Inactive'
            }
        ];

        container.innerHTML = stats.map(stat => `
            <div class="col-md-3">
                <div class="card text-center">
                    <div class="card-body">
                        <i class="bi ${stat.icon}" style="font-size: 2rem; color: var(--bs-${stat.color});"></i>
                        <h5 class="mt-2">${stat.value}</h5>
                        <p class="text-muted mb-0">${stat.title}</p>
                        <small class="text-muted">${stat.subtitle}</small>
                    </div>
                </div>
            </div>
        `).join('');
    },

    renderActiveTimer: function() {
        const container = document.getElementById('activeTimerSection');
        
        if (!this.dashboardData.HasActiveTimer || !this.dashboardData.ActiveTimer) {
            container.innerHTML = '';
            return;
        }

        const timer = this.dashboardData.ActiveTimer;
        container.innerHTML = `
            <div class="col-12">
                <div class="card border-success">
                    <div class="card-header bg-success text-white">
                        <h5 class="mb-0">
                            <span class="active-timer-indicator"></span>
                            Active Timer Running
                        </h5>
                    </div>
                    <div class="card-body">
                        <div class="row align-items-center">
                            <div class="col-md-8">
                                <h6>${this.escapeHtml(timer.TaskTitle)}</h6>
                                <p class="text-muted mb-0">
                                    Started: ${new Date(timer.StartTime).toLocaleString()}
                                    ${timer.Description ? `• ${this.escapeHtml(timer.Description)}` : ''}
                                </p>
                            </div>
                            <div class="col-md-4 text-center">
                                <div class="display-4 text-success" id="activeTimerDisplay">
                                    ${this.formatDuration(timer.Duration)}
                                </div>
                                <button class="btn btn-danger mt-2" onclick="timeTracking.stopActiveTimer()">
                                    <i class="bi bi-stop-fill"></i> Stop Timer
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;

        // Start updating the active timer display
        this.startActiveTimerUpdate();
    },

    renderTimeChart: function() {
        const container = document.getElementById('timeChart');
        if (!container || !this.dashboardData) return;

        // Destroy existing chart
        if (this.currentChart) {
            this.currentChart.destroy();
        }

        const ctx = document.createElement('canvas');
        container.innerHTML = '';
        container.appendChild(ctx);

        const chartData = this.getChartData();
        
        this.currentChart = new Chart(ctx, {
            type: 'bar',
            data: chartData,
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    },
                    title: {
                        display: true,
                        text: this.getChartTitle()
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: 'Hours'
                        }
                    }
                }
            }
        });
    },

    getChartData: function() {
        let labels, data;
        
        switch (this.currentChartView) {
            case 'daily':
                labels = this.dashboardData.DailyBreakdown.map(d => new Date(d.Date).toLocaleDateString());
                data = this.dashboardData.DailyBreakdown.map(d => this.parseDurationHours(d.TotalTime));
                break;
            case 'weekly':
                labels = ['Week 1', 'Week 2', 'Week 3', 'Week 4'];
                data = [8, 12, 6, 9]; // Sample data - would come from API
                break;
            case 'monthly':
                labels = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'];
                data = [40, 35, 45, 38, 42, 48]; // Sample data - would come from API
                break;
            default:
                labels = [];
                data = [];
        }

        return {
            labels: labels,
            datasets: [{
                label: 'Hours',
                data: data,
                backgroundColor: 'rgba(13, 110, 253, 0.5)',
                borderColor: 'rgba(13, 110, 253, 1)',
                borderWidth: 1
            }]
        };
    },

    getChartTitle: function() {
        switch (this.currentChartView) {
            case 'daily': return 'Daily Time Tracking (Last 7 Days)';
            case 'weekly': return 'Weekly Time Tracking';
            case 'monthly': return 'Monthly Time Tracking';
            default: return 'Time Analytics';
        }
    },

    renderTaskBreakdown: function() {
        const container = document.getElementById('taskBreakdown');
        if (!container || !this.dashboardData) return;

        const taskBreakdown = this.dashboardData.TaskBreakdown || [];
        
        if (taskBreakdown.length === 0) {
            container.innerHTML = '<p class="text-muted text-center">No task data available</p>';
            return;
        }

        const totalTime = taskBreakdown.reduce((sum, task) => sum + this.parseDurationHours(task.TotalTime), 0);

        container.innerHTML = taskBreakdown.map(task => {
            const percentage = totalTime > 0 ? (this.parseDurationHours(task.TotalTime) / totalTime * 100).toFixed(1) : 0;
            
            return `
                <div class="task-breakdown-item">
                    <div>
                        <div class="task-name">${this.escapeHtml(task.TaskTitle)}</div>
                        <small class="text-muted">${task.ProjectName || 'No project'}</small>
                        <div class="task-progress">
                            <div class="task-progress-bar" style="width: ${percentage}%"></div>
                        </div>
                    </div>
                    <div class="text-end">
                        <div class="task-time">${this.formatDuration(task.TotalTime)}</div>
                        <small class="text-muted">${percentage}%</small>
                    </div>
                </div>
            `;
        }).join('');
    },

    renderRecentEntries: function() {
        const tbody = document.getElementById('timeEntriesBody');
        if (!tbody) return;

        const entries = this.dashboardData.RecentEntries || [];
        
        if (entries.length === 0) {
            tbody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">No time entries found</td></tr>';
            return;
        }

        tbody.innerHTML = entries.map(entry => {
            const durationClass = this.getDurationClass(entry.Duration);
            const statusClass = entry.IsActive ? 'status-active' : 'status-completed';
            
            return `
                <tr class="time-entry-row">
                    <td>${new Date(entry.StartTime).toLocaleDateString()}</td>
                    <td>
                        <a href="/Task/Details/${entry.TaskId}" target="_blank">
                            ${this.escapeHtml(entry.TaskTitle)}
                        </a>
                    </td>
                    <td>
                        <span class="duration-badge ${durationClass}">
                            ${this.formatDuration(entry.Duration)}
                        </span>
                    </td>
                    <td>
                        ${entry.Description ? this.escapeHtml(entry.Description) : '<em class="text-muted">No description</em>'}
                    </td>
                    <td>
                        <span class="badge ${statusClass}">
                            ${entry.IsActive ? 'Active' : 'Completed'}
                        </span>
                    </td>
                    <td>
                        <div class="btn-group btn-group-sm">
                            <button class="btn btn-outline-primary" onclick="timeTracking.editEntry(${entry.Id})">
                                <i class="bi bi-pencil"></i>
                            </button>
                            ${!entry.IsActive ? `
                                <button class="btn btn-outline-danger" onclick="timeTracking.deleteEntry(${entry.Id})">
                                    <i class="bi bi-trash"></i>
                                </button>
                            ` : ''}
                        </div>
                    </td>
                </tr>
            `;
        }).join('');
    },

    // Timer Functions
    showTimer: function() {
        this.loadTasksForTimer();

        // If there's an active timer, restore its running state in the modal
        if (this.dashboardData && this.dashboardData.HasActiveTimer && this.dashboardData.ActiveTimer) {
            const timer = this.dashboardData.ActiveTimer;

            // Wait for loadTasksForTimer to populate the select, then set the value
            // Use a small delay to ensure the DOM is ready
            setTimeout(() => {
                const taskSelect = document.getElementById('timerTask');
                if (taskSelect && timer.TaskId) {
                    taskSelect.value = timer.TaskId;
                    taskSelect.disabled = true;
                }

                const descField = document.getElementById('timerDescription');
                if (descField) {
                    descField.value = timer.Description || '';
                    descField.disabled = true;
                }

                // Show Stop, hide Start
                document.getElementById('startTimerBtn').classList.add('d-none');
                document.getElementById('stopTimerBtn').classList.remove('d-none');

                // Restart the live ticker from the persisted server StartTime
                // This is the core fix: elapsed = now - serverStartTime (UTC)
                this.startTimerDisplay(timer);
                this.updateTimerDisplay(); // immediate update so display isn't blank
            }, 100);
        }

        const modal = new bootstrap.Modal(document.getElementById('timerModal'));
        modal.show();
    },

    loadTasksForTimer: async function() {
        try {
            const response = await fetch('/api/tasks');
            if (!response.ok) throw new Error('Failed to load tasks');

            const tasks = await response.json();
            const select = document.getElementById('timerTask');
            
            select.innerHTML = '<option value="">Choose a task...</option>';
            tasks.forEach(task => {
                select.innerHTML += `<option value="${task.Id}">${this.escapeHtml(task.Title)}</option>`;
            });

        } catch (error) {
            console.error('Error loading tasks:', error);
            showGlobalToast(error.message, 'error');
        }
    },

    updateTimerTaskName: function() {
        const select = document.getElementById('timerTask');
        const display = document.getElementById('timerTaskName');
        
        if (select.value) {
            const option = select.options[select.selectedIndex];
            display.textContent = option.text;
        } else {
            display.textContent = '';
        }
    },

    startTimer: async function() {
        const taskId = document.getElementById('timerTask').value;
        const description = document.getElementById('timerDescription').value.trim();
        
        if (!taskId) {
            showGlobalToast('Please select a task', 'warning');
            return;
        }

        try {
            const response = await fetch('/api/time-tracking/timer/start', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify({
                    taskId: parseInt(taskId),
                    description: description || null
                })
            });

            if (!response.ok) throw new Error('Failed to start timer');

            const timeEntry = await response.json();
            
            // Update UI
            document.getElementById('startTimerBtn').classList.add('d-none');
            document.getElementById('stopTimerBtn').classList.remove('d-none');
            document.getElementById('timerTask').disabled = true;
            document.getElementById('timerDescription').disabled = true;
            
            // Start timer display
            this.startTimerDisplay(timeEntry);
            
            showGlobalToast('Timer started successfully', 'success');

        } catch (error) {
            console.error('Error starting timer:', error);
            showGlobalToast(error.message, 'error');
        }
    },

    stopTimer: async function() {
        try {
            // Use the select value, or fall back to the in-memory activeTimer.taskId
            // (fallback needed if the modal was restored after page navigation before
            //  the async loadTasksForTimer had set the <select> value)
            const taskId = document.getElementById('timerTask').value
                        || (this.activeTimer && this.activeTimer.taskId);

            if (!taskId) {
                showGlobalToast('No active task timer to stop', 'warning');
                return;
            }
            
            const response = await fetch(`/api/time-tracking/timer/stop/${taskId}`, {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                }
            });

            if (!response.ok) throw new Error('Failed to stop timer');

            // Stop timer display
            this.stopTimerDisplay();
            
            // Reset UI (the hidden.bs.modal in site.js also handles this, but we do it
            // explicitly here so the user sees feedback immediately without waiting)
            document.getElementById('startTimerBtn').classList.remove('d-none');
            document.getElementById('stopTimerBtn').classList.add('d-none');
            document.getElementById('timerTask').disabled = false;
            document.getElementById('timerDescription').disabled = false;
            
            showGlobalToast('Timer stopped — time saved', 'success');
            
            // Close modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('timerModal'));
            if (modal) modal.hide();
            
            // Refresh dashboard to show updated totals
            await this.loadDashboard();

        } catch (error) {
            console.error('Error stopping timer:', error);
            showGlobalToast(error.message, 'error');
        }
    },

    stopActiveTimer: async function() {
        if (!this.dashboardData.ActiveTimer) return;
        
        try {
            const response = await fetch(`/api/time-tracking/timer/stop/${this.dashboardData.ActiveTimer.TaskId}`, {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                }
            });

            if (!response.ok) throw new Error('Failed to stop timer');

            showGlobalToast('Timer stopped successfully', 'success');
            await this.loadDashboard();

        } catch (error) {
            console.error('Error stopping active timer:', error);
            showGlobalToast(error.message, 'error');
        }
    },

    startTimerDisplay: function(timeEntry) {
        if (!timeEntry || !timeEntry.StartTime) return;
        const startTime = new Date(timeEntry.StartTime);
        if (isNaN(startTime.getTime())) return;

        // Clear any existing interval before starting a new one (prevents double-counting)
        if (this.timerInterval) {
            clearInterval(this.timerInterval);
            this.timerInterval = null;
        }

        this.activeTimer = {
            startTime: startTime,
            taskId: timeEntry.TaskId
        };

        this.timerInterval = setInterval(() => {
            this.updateTimerDisplay();
        }, 1000);

        this.updateTimerDisplay();
    },

    stopTimerDisplay: function() {
        if (this.timerInterval) {
            clearInterval(this.timerInterval);
            this.timerInterval = null;
        }
        
        this.activeTimer = null;
        document.getElementById('timerDisplay').textContent = '00:00:00';
    },

    updateTimerDisplay: function() {
        if (!this.activeTimer) return;

        const now = new Date();
        const elapsed = now - this.activeTimer.startTime;
        const hours = Math.floor(elapsed / 3600000);
        const minutes = Math.floor((elapsed % 3600000) / 60000);
        const seconds = Math.floor((elapsed % 60000) / 1000);

        const display = `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
        document.getElementById('timerDisplay').textContent = display;
    },

    startActiveTimerUpdate: function() {
        if (this.activeTimerUpdateInterval) {
            clearInterval(this.activeTimerUpdateInterval);
        }

        this.activeTimerUpdateInterval = setInterval(() => {
            this.updateActiveTimerDisplay();
        }, 1000);
    },

    updateActiveTimerDisplay: function() {
        if (!this.dashboardData || !this.dashboardData.ActiveTimer) return;
        if (!this.dashboardData.ActiveTimer.StartTime) return;
        const display = document.getElementById('activeTimerDisplay');
        if (!display) return;

        const now = new Date();
        const startTime = new Date(this.dashboardData.ActiveTimer.StartTime);
        if (isNaN(startTime.getTime())) return; // Invalid date from API
        const elapsed = now - startTime;
        
        display.textContent = this.formatDuration({ totalMilliseconds: elapsed });
    },

    // Manual Entry Functions
    showAddEntryModal: function() {
        // Reset the form before opening to clear any previous values
        const form = document.getElementById('addEntryForm');
        if (form) form.reset();

        this.loadTasksForEntry();
        
        // Set default start time to now
        const now = new Date();
        const localDateTime = new Date(now.getTime() - now.getTimezoneOffset() * 60000).toISOString().slice(0, 16);
        document.getElementById('entryStartTime').value = localDateTime;
        
        const modal = new bootstrap.Modal(document.getElementById('addEntryModal'));
        modal.show();
    },

    loadTasksForEntry: async function() {
        try {
            const response = await fetch('/api/tasks');
            if (!response.ok) throw new Error('Failed to load tasks');

            const tasks = await response.json();
            const select = document.getElementById('entryTask');
            
            select.innerHTML = '<option value="">Choose a task...</option>';
            tasks.forEach(task => {
                select.innerHTML += `<option value="${task.Id}">${this.escapeHtml(task.Title)}</option>`;
            });

        } catch (error) {
            console.error('Error loading tasks:', error);
            showGlobalToast(error.message, 'error');
        }
    },

    calculateEntryDuration: function() {
        const startTime = document.getElementById('entryStartTime').value;
        const endTime = document.getElementById('entryEndTime').value;
        
        if (startTime && endTime) {
            const start = new Date(startTime);
            const end = new Date(endTime);
            
            if (end > start) {
                const duration = end - start;
                const hours = Math.floor(duration / 3600000);
                const minutes = Math.floor((duration % 3600000) / 60000);
                
                // Could display this somewhere if needed
                console.log(`Duration: ${hours}h ${minutes}m`);
            }
        }
    },

    saveEntry: async function() {
        const taskId = document.getElementById('entryTask').value;
        const startTime = document.getElementById('entryStartTime').value;
        const endTime = document.getElementById('entryEndTime').value;
        const description = document.getElementById('entryDescription').value.trim();
        
        if (!taskId || !startTime) {
            showGlobalToast('Please fill in required fields', 'warning');
            return;
        }

        try {
            const entryData = {
                taskId: parseInt(taskId),
                startTime: new Date(startTime).toISOString(),
                endTime: endTime ? new Date(endTime).toISOString() : null,
                description: description || null
            };

            const response = await fetch('/api/time-tracking/entries', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify(entryData)
            });

            if (!response.ok) throw new Error('Failed to save entry');

            showGlobalToast('Time entry saved successfully', 'success');
            
            // Close modal and refresh
            const modal = bootstrap.Modal.getInstance(document.getElementById('addEntryModal'));
            if (modal) modal.hide();
            
            await this.loadDashboard();

        } catch (error) {
            console.error('Error saving entry:', error);
            showGlobalToast(error.message, 'error');
        }
    },

    editEntry: function(entryId) {
        showGlobalToast('Edit entry functionality coming soon', 'info');
    },

    deleteEntry: async function(entryId) {
        if (!confirm('Are you sure you want to delete this time entry?')) return;
        
        try {
            const response = await fetch(`/api/time-tracking/entries/${entryId}`, {
                method: 'DELETE',
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                }
            });

            if (!response.ok) throw new Error('Failed to delete entry');

            showGlobalToast('Time entry deleted successfully', 'success');
            await this.loadDashboard();

        } catch (error) {
            console.error('Error deleting entry:', error);
            showGlobalToast(error.message, 'error');
        }
    },

    // Chart Functions
    changeChartView: function(view, btnElement) {
        this.currentChartView = view;
        
        document.querySelectorAll('.btn-group .btn').forEach(btn => {
            btn.classList.remove('active');
        });
        if (btnElement) {
            btnElement.classList.add('active');
        }
        
        this.renderTimeChart();
    },

    // Utility Functions
    refresh: function() {
        this.loadDashboard();
    },

    startAutoRefresh: function() {
        // Auto-refresh every 60 seconds
        setInterval(() => {
            this.loadDashboard();
        }, 60000);
    },

    setLoading: function(loading) {
        this.isLoading = loading;
        const loadingElement = document.getElementById('timeTrackingLoading');
        const contentElement = document.getElementById('timeTrackingContent');
        
        if (loading) {
            loadingElement.classList.remove('d-none');
            contentElement.classList.add('d-none');
        } else {
            loadingElement.classList.add('d-none');
            contentElement.classList.remove('d-none');
        }
    },

    showError: function(message) {
        const errorElement = document.getElementById('timeTrackingError');
        const errorMessage = document.getElementById('errorMessage');
        
        errorMessage.textContent = message;
        errorElement.classList.remove('d-none');
    },

    hideError: function() {
        const errorElement = document.getElementById('timeTrackingError');
        errorElement.classList.add('d-none');
    },

    parseDurationHours: function(duration) {
        if (!duration) return 0;
        if (typeof duration === 'number') return duration / 3600000;
        if (typeof duration === 'object' && duration.totalHours != null) return duration.totalHours;
        if (typeof duration === 'string') {
            const parts = duration.split(':');
            if (parts.length === 3) {
                const p = parts[0].split('.');
                let h = 0;
                if (p.length === 2) { h += parseInt(p[0]) * 24; h += parseInt(p[1]); }
                else { h += parseInt(p[0]); }
                h += parseInt(parts[1]) / 60;
                h += parseInt(parts[2]) / 3600;
                return h;
            }
        }
        return 0;
    },

    formatDuration: function(duration) {
        if (!duration) return '00:00:00';

        let totalMs;
        
        if (typeof duration.totalMilliseconds === 'number') {
            totalMs = duration.totalMilliseconds;
        } else if (typeof duration.totalHours === 'number') {
            totalMs = duration.totalHours * 3600000;
        } else if (typeof duration === 'string') {
            return duration;
        } else {
            return '00:00:00';
        }

        const hours = Math.floor(totalMs / 3600000);
        const minutes = Math.floor((totalMs % 3600000) / 60000);
        const seconds = Math.floor((totalMs % 60000) / 1000);

        return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
    },

    getDurationClass: function(duration) {
        if (!duration) return 'duration-short';
        const hours = typeof duration.totalHours === 'number' ? duration.totalHours : 0;
        
        if (hours < 1) return 'duration-short';
        if (hours < 4) return 'duration-medium';
        return 'duration-long';
    },

    escapeHtml: function(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
};

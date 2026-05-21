/* ═══════════════════════════════════════════════════════════════
   Agile Task Manager — analytics.js
   Sprint and Project Analytics with charts and metrics
═══════════════════════════════════════════════════════════════ */

window.analytics = {
    sprintId: null,
    projectId: null,
    type: null, // 'sprint' or 'project'
    data: null,
    charts: {},

    init: async function(id, type = 'sprint') {
        this.type = type;
        if (type === 'sprint') {
            this.sprintId = id;
        } else {
            this.projectId = id;
        }
        
        await this.loadData();
        this.renderContent();
    },

    // Called by the sprint dropdown (and project filter) to swap the active sprint
    // without a full page reload. Destroys stale charts and re-fetches data.
    changeSprint: async function(id) {
        this.sprintId = Number(id);
        this.type = 'sprint';
        // Destroy all existing charts so Chart.js doesn't throw on canvas reuse
        this.destroyAllCharts();
        await this.loadData();
        this.renderContent();
    },

    loadData: async function() {
        try {
            this.showLoading(true);
            this.hideError();
            
            let url;
            if (this.type === 'sprint') {
                url = `/api/analytics/sprint/${this.sprintId}`;
            } else {
                url = `/api/analytics/project/${this.projectId}`;
            }
            
            const response = await fetch(url, {
                credentials: 'same-origin',
                cache: 'no-cache'   // prevent browser returning stale sprint data
            });
            if (!response.ok) {
                const errText = await response.text().catch(() => '');
                let msg = `Failed to load analytics data (${response.status})`;
                try {
                    const j = JSON.parse(errText);
                    if (j && typeof j.error === 'string') msg = j.error;
                } catch {
                    if (errText && errText.length < 240) msg = errText;
                }
                throw new Error(msg);
            }

            this.data = await response.json();
            this.showLoading(false);
        } catch (error) {
            console.error('Error loading analytics:', error);
            this.showError(error.message);
            this.showLoading(false);
        }
    },

    renderContent: function() {
        if (!this.data) return;
        
        this.renderOverviewCards();
        this.renderProgressSection();
        this.renderTaskCharts();
        this.renderTeamPerformance();
        this.renderRecentActivity();
        
        document.getElementById('analyticsContent').classList.remove('d-none');

        // Keep sprint dropdown in sync when loaded via changeSprint()
        const sprintFilter = document.getElementById('sprintFilter');
        if (sprintFilter && this.sprintId) {
            sprintFilter.value = this.sprintId;
        }

        // Update the sprint name badge so users can confirm which sprint is active
        const badge = document.getElementById('sprintNameBadge');
        if (badge && this.data.SprintName) {
            badge.textContent = `${this.data.SprintName}  ·  ${this.data.Status}`;
            badge.classList.remove('d-none');
        }
    },

    renderOverviewCards: function() {
        const container = document.getElementById('overviewCards');
        container.innerHTML = '';
        
        const cards = [
            {
                title: 'Total Tasks',
                value: this.data.TotalTasks,
                icon: 'bi-list-task',
                class: 'info'
            },
            {
                title: 'Completed Tasks',
                value: this.data.CompletedTasks,
                icon: 'bi-check-circle',
                class: 'success'
            },
            {
                title: 'In Progress',
                value: this.data.InProgressTasks,
                icon: 'bi-arrow-repeat',
                class: 'warning'
            },
            {
                title: 'Overdue Tasks',
                value: this.data.OverdueTasks,
                icon: 'bi-exclamation-triangle',
                class: this.data.OverdueTasks > 0 ? 'danger' : 'success'
            }
        ];
        
        if (this.type === 'sprint') {
            cards.push({
                title: 'Days Remaining',
                value: this.data.RemainingDays,
                icon: 'bi-calendar-days',
                class: this.data.RemainingDays <= 3 ? 'danger' : 'info'
            });
        }
        
        cards.forEach(card => {
            const cardElement = document.createElement('div');
            cardElement.className = 'col-md-6 col-lg-3';
            cardElement.innerHTML = `
                <div class="metric-card ${card.class}">
                    <div class="d-flex justify-content-between align-items-start">
                        <div>
                            <div class="metric-value">${card.value}</div>
                            <div class="metric-label">${card.title}</div>
                        </div>
                        <i class="bi ${card.icon}" style="font-size: 2rem; opacity: 0.8;"></i>
                    </div>
                </div>
            `;
            container.appendChild(cardElement);
        });
    },

    renderProgressSection: function() {
        // Update progress bars
        const completionPercentage = Math.round(this.data.CompletionPercentage);
        const sprintProgressPercentage = Math.round(this.data.SprintProgressPercentage);
        
        document.getElementById('completionPercentage').textContent = `${completionPercentage}%`;
        document.getElementById('completionProgressBar').style.width = `${completionPercentage}%`;
        
        document.getElementById('sprintProgressPercentage').textContent = `${sprintProgressPercentage}%`;
        document.getElementById('sprintProgressBar').style.width = `${sprintProgressPercentage}%`;
        
        // Render sprint metrics
        this.renderSprintMetrics();
        
        // Render burndown chart
        this.renderBurndownChart();
    },

    renderSprintMetrics: function() {
        const container = document.getElementById('sprintMetrics');
        const metrics = [
            {
                label: 'Sprint Duration',
                value: `${this.data.TotalDays} days`,
                icon: 'bi-calendar-week'
            },
            {
                label: 'Average Daily Velocity',
                value: `${Math.round(this.data.CompletedTasks / Math.max(1, this.data.TotalDays - this.data.RemainingDays))} tasks/day`,
                icon: 'bi-graph-up'
            },
            {
                label: 'Testing Tasks',
                value: this.data.TestingTasks,
                icon: 'bi-clipboard-check'
            },
            {
                label: 'To Do Tasks',
                value: this.data.ToDoTasks,
                icon: 'bi-list-check'
            }
        ];
        
        container.innerHTML = metrics.map(metric => `
            <div class="d-flex justify-content-between align-items-center mb-3">
                <div class="d-flex align-items-center">
                    <i class="bi ${metric.icon} me-2"></i>
                    <span>${metric.label}</span>
                </div>
                <strong>${metric.value}</strong>
            </div>
        `).join('');
    },

    renderBurndownChart: function() {
        const container = document.getElementById('burndownChart');
        
        if (!this.data.BurndownData || this.data.BurndownData.length === 0) {
            container.innerHTML = '<p class="text-muted text-center">No burndown data available</p>';
            return;
        }
        
        // Destroy existing chart if it exists
        if (this.charts.burndown) {
            this.charts.burndown.destroy();
        }
        
        const ctx = document.createElement('canvas');
        container.innerHTML = '';
        container.appendChild(ctx);
        
        this.charts.burndown = new Chart(ctx, {
            type: 'line',
            data: {
                labels: this.data.BurndownData.map(d => new Date(d.Date).toLocaleDateString()),
                datasets: [
                    {
                        label: 'Ideal Burndown',
                        data: this.data.BurndownData.map(d => d.IdealRemaining),
                        borderColor: '#6c757d',
                        backgroundColor: 'rgba(108, 117, 125, 0.1)',
                        borderDash: [5, 5],
                        fill: false,
                        tension: 0.1
                    },
                    {
                        label: 'Actual Burndown',
                        data: this.data.BurndownData.map(d => d.ActualRemaining),
                        borderColor: '#0d6efd',
                        backgroundColor: 'rgba(13, 110, 253, 0.1)',
                        fill: false,
                        tension: 0.1
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    title: {
                        display: true,
                        text: 'Sprint Burndown Chart'
                    },
                    legend: {
                        display: true,
                        position: 'bottom'
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: 'Tasks Remaining'
                        }
                    },
                    x: {
                        title: {
                            display: true,
                            text: 'Date'
                        }
                    }
                }
            }
        });
    },

    renderTaskCharts: function() {
        this.renderTaskStatusChart();
        this.renderTaskPriorityChart();
    },

    renderTaskStatusChart: function() {
        const container = document.getElementById('taskStatusChart');
        
        if (!this.data.TasksByStatus || Object.keys(this.data.TasksByStatus).length === 0) {
            container.innerHTML = '<p class="text-muted text-center">No task status data available</p>';
            return;
        }
        
        // Destroy existing chart if it exists
        if (this.charts.status) {
            this.charts.status.destroy();
        }
        
        const ctx = document.createElement('canvas');
        container.innerHTML = '';
        container.appendChild(ctx);
        
        const labels = Object.keys(this.data.TasksByStatus);
        const data = Object.values(this.data.TasksByStatus);
        const colors = labels.map(status => this.getStatusColor(status));
        
        this.charts.status = new Chart(ctx, {
            type: 'pie',
            data: {
                labels: labels,
                datasets: [{
                    data: data,
                    backgroundColor: colors,
                    borderWidth: 2,
                    borderColor: '#fff'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom'
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                const percentage = ((context.parsed / total) * 100).toFixed(1);
                                return `${context.label}: ${context.parsed} (${percentage}%)`;
                            }
                        }
                    }
                }
            }
        });
    },

    renderTaskPriorityChart: function() {
        const container = document.getElementById('taskPriorityChart');
        
        if (!this.data.TasksByPriority || Object.keys(this.data.TasksByPriority).length === 0) {
            container.innerHTML = '<p class="text-muted text-center">No task priority data available</p>';
            return;
        }
        
        // Destroy existing chart if it exists
        if (this.charts.priority) {
            this.charts.priority.destroy();
        }
        
        const ctx = document.createElement('canvas');
        container.innerHTML = '';
        container.appendChild(ctx);
        
        const labels = Object.keys(this.data.TasksByPriority);
        const data = Object.values(this.data.TasksByPriority);
        const colors = labels.map(priority => this.getPriorityColor(priority));
        
        this.charts.priority = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Tasks',
                    data: data,
                    backgroundColor: colors,
                    borderWidth: 0
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
                        beginAtZero: true,
                        ticks: {
                            stepSize: 1
                        }
                    }
                }
            }
        });
    },

    renderTeamPerformance: function() {
        const tbody = document.getElementById('teamPerformanceBody');
        
        if (!this.data.TeamPerformance || this.data.TeamPerformance.length === 0) {
            tbody.innerHTML = '<tr><td colspan="6" class="text-center text-muted">No team performance data available</td></tr>';
            return;
        }
        
        tbody.innerHTML = this.data.TeamPerformance.map(member => {
            const completionRate = Math.round(member.CompletionRate);
            const performanceClass = this.getPerformanceClass(completionRate);
            
            return `
                <tr>
                    <td>
                        <div class="d-flex align-items-center">
                            <div class="avatar-sm bg-primary text-white rounded-circle d-flex align-items-center justify-content-center me-2" style="width: 32px; height: 32px;">
                                ${member.UserName.charAt(0).toUpperCase()}
                            </div>
                            ${this.escapeHtml(member.UserName)}
                        </div>
                    </td>
                    <td>${member.AssignedTasks}</td>
                    <td>${member.CompletedTasks}</td>
                    <td>
                        <div class="d-flex align-items-center">
                            <div class="progress me-2" style="width: 100px; height: 8px;">
                                <div class="progress-bar" style="width: ${completionRate}%"></div>
                            </div>
                            <span>${completionRate}%</span>
                        </div>
                    </td>
                    <td>
                        ${member.OverdueTasks > 0 ? 
                            `<span class="badge bg-danger">${member.OverdueTasks}</span>` : 
                            '<span class="text-muted">None</span>'}
                    </td>
                    <td>
                        <span class="performance-badge ${performanceClass}">${this.getPerformanceLabel(completionRate)}</span>
                    </td>
                </tr>
            `;
        }).join('');
    },

    renderRecentActivity: function() {
        const container = document.getElementById('recentActivity');
        
        if (!this.data.RecentActivity || this.data.RecentActivity.length === 0) {
            container.innerHTML = '<p class="text-muted text-center">No recent activity</p>';
            return;
        }
        
        container.innerHTML = this.data.RecentActivity.map(activity => {
            const icon = this.getActivityIcon(activity.Action);
            const iconColor = this.getActivityColor(activity.Action);
            
            return `
                <div class="activity-item">
                    <div class="activity-icon ${iconColor}">
                        <i class="bi ${icon}"></i>
                    </div>
                    <div class="activity-content">
                        <div class="activity-title">${this.escapeHtml(activity.ActorName)}</div>
                        <div class="activity-description">
                            ${this.getActivityDescription(activity)}
                        </div>
                        <div class="activity-time">${this.formatTime(activity.Timestamp)}</div>
                    </div>
                </div>
            `;
        }).join('');
    },

    // Utility Methods
    refresh: function() {
        this.loadData().then(() => {
            this.renderContent();
            if (typeof showGlobalToast === 'function') {
                showGlobalToast('Analytics refreshed successfully', 'success');
            }
        });
    },

    destroyAllCharts: function() {
        Object.values(this.charts).forEach(chart => {
            if (chart && typeof chart.destroy === 'function') {
                chart.destroy();
            }
        });
        this.charts = {};
    },

    exportReport: function() {
        if (!this.data) {
            showGlobalToast('No data available to export', 'warning');
            return;
        }
        
        // Create CSV content
        let csvContent = '';
        
        if (this.type === 'sprint') {
            csvContent = 'Sprint Analytics Report\n\n';
            csvContent += `Sprint: ${this.data.SprintName}\n`;
            csvContent += `Status: ${this.data.Status}\n`;
            csvContent += `Start Date: ${new Date(this.data.StartDate).toLocaleDateString()}\n`;
            csvContent += `End Date: ${new Date(this.data.EndDate).toLocaleDateString()}\n\n`;
        }
        
        csvContent += 'Summary\n';
        csvContent += `Total Tasks,${this.data.TotalTasks}\n`;
        csvContent += `Completed Tasks,${this.data.CompletedTasks}\n`;
        csvContent += `In Progress,${this.data.InProgressTasks}\n`;
        csvContent += `Overdue Tasks,${this.data.OverdueTasks}\n\n`;
        
        if (this.data.TeamPerformance) {
            csvContent += 'Team Performance\n';
            csvContent += 'Team Member,Assigned Tasks,Completed Tasks,Completion Rate,Overdue Tasks\n';
            this.data.TeamPerformance.forEach(member => {
                csvContent += `"${member.UserName}",${member.AssignedTasks},${member.CompletedTasks},${member.CompletionRate}%,${member.OverdueTasks}\n`;
            });
        }
        
        // Create download link
        const blob = new Blob([csvContent], { type: 'text/csv' });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `${this.type}-analytics-${new Date().toISOString().split('T')[0]}.csv`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
        
        showGlobalToast('Report exported successfully', 'success');
    },

    showLoading: function(show) {
        const loadingElement = document.getElementById('analyticsLoading');
        const contentElement = document.getElementById('analyticsContent');
        
        if (show) {
            loadingElement.classList.remove('d-none');
            contentElement.classList.add('d-none');
        } else {
            loadingElement.classList.add('d-none');
        }
    },

    showError: function(message) {
        const errorElement = document.getElementById('analyticsError');
        const errorMessage = document.getElementById('errorMessage');
        
        errorMessage.textContent = message;
        errorElement.classList.remove('d-none');
    },

    hideError: function() {
        const errorElement = document.getElementById('analyticsError');
        errorElement.classList.add('d-none');
    },

    escapeHtml: function(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    },

    getStatusColor: function(status) {
        switch (status.toLowerCase()) {
            case 'todo': return '#6c757d';
            case 'inprogress': return '#0d6efd';
            case 'testing': return '#ffc107';
            case 'done': return '#28a745';
            default: return '#6c757d';
        }
    },

    getPriorityColor: function(priority) {
        switch (priority.toLowerCase()) {
            case 'high': return '#dc3545';
            case 'medium': return '#ffc107';
            case 'low': return '#28a745';
            default: return '#6c757d';
        }
    },

    getPerformanceClass: function(rate) {
        if (rate >= 90) return 'performance-excellent';
        if (rate >= 70) return 'performance-good';
        if (rate >= 50) return 'performance-average';
        return 'performance-poor';
    },

    getPerformanceLabel: function(rate) {
        if (rate >= 90) return 'Excellent';
        if (rate >= 70) return 'Good';
        if (rate >= 50) return 'Average';
        return 'Poor';
    },

    getActivityIcon: function(action) {
        switch (action.toLowerCase()) {
            case 'statuschanged': return 'bi-arrow-left-right';
            case 'created': return 'bi-plus-circle';
            case 'updated': return 'bi-pencil';
            case 'deleted': return 'bi-trash';
            case 'assigned': return 'bi-person-plus';
            case 'commented': return 'bi-chat-dots';
            default: return 'bi-activity';
        }
    },

    getActivityColor: function(action) {
        switch (action.toLowerCase()) {
            case 'statuschanged': return 'bg-info';
            case 'created': return 'bg-success';
            case 'updated': return 'bg-warning';
            case 'deleted': return 'bg-danger';
            case 'assigned': return 'bg-primary';
            case 'commented': return 'bg-secondary';
            default: return 'bg-secondary';
        }
    },

    getActivityDescription: function(activity) {
        switch (activity.Action.toLowerCase()) {
            case 'statuschanged':
                return `Changed status from ${activity.OldValue} to ${activity.NewValue}`;
            case 'created':
                return 'Created this task';
            case 'updated':
                return 'Updated this task';
            case 'deleted':
                return 'Deleted this task';
            case 'assigned':
                return 'Assigned this task';
            case 'commented':
                return 'Added a comment';
            default:
                return `Performed ${activity.Action} action`;
        }
    },

    formatTime: function(timestamp) {
        const date = new Date(timestamp);
        const now = new Date();
        const diffMs = now - date;
        const diffMins = Math.floor(diffMs / 60000);
        const diffHours = Math.floor(diffMs / 3600000);
        const diffDays = Math.floor(diffMs / 86400000);
        
        if (diffMins < 1) return 'Just now';
        if (diffMins < 60) return `${diffMins} minute${diffMins > 1 ? 's' : ''} ago`;
        if (diffHours < 24) return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;
        if (diffDays < 7) return `${diffDays} day${diffDays > 1 ? 's' : ''} ago`;
        
        return date.toLocaleDateString();
    }
};

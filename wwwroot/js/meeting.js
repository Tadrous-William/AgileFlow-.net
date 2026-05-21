/* ═══════════════════════════════════════════════════════════════
   Agile Task Manager — meeting.js
   Meeting management with standups, retrospectives, and scheduling
═══════════════════════════════════════════════════════════════ */

window.meeting = {
    dashboardData: null,
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
                    case 'm':
                        e.preventDefault();
                        this.showCreateMeetingModal();
                        break;
                    case 's':
                        e.preventDefault();
                        this.submitStandup();
                        break;
                    case 'r':
                        e.preventDefault();
                        this.refresh();
                        break;
                }
            }
        });

        // Meeting form changes
        document.getElementById('meetingProject')?.addEventListener('change', (e) => {
            this.loadProjectParticipants(e.target.value);
        });
    },

    loadDashboard: async function() {
        try {
            this.setLoading(true);
            this.hideError();

            const response = await fetch('/api/meetings/dashboard');
            if (!response.ok) throw new Error('Failed to load dashboard');

            const rawData = await response.json();
            this.dashboardData = this.normalizeData(rawData);
            
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

        this.renderTodayMeetings();
        this.renderMeetingStats();
        this.renderUpcomingMeetings();
        this.renderRecentStandups();
        
        document.getElementById('meetingContent').classList.remove('d-none');
    },

    renderTodayMeetings: function() {
        const container = document.getElementById('todayMeetings');
        if (!container) return;
        
        const stats = [
            {
                title: 'Today\'s Meetings',
                value: this.dashboardData?.todayMeetingCount ?? 0,
                icon: 'bi-calendar-day',
                color: 'primary',
                subtitle: 'Scheduled for today'
            },
            {
                title: 'Upcoming',
                value: this.dashboardData?.upcomingMeetingCount ?? 0,
                icon: 'bi-calendar-week',
                color: 'info',
                subtitle: 'Next 7 days'
            },
            {
                title: 'Pending Action Items',
                value: this.dashboardData?.pendingActionItemCount ?? 0,
                icon: 'bi-list-check',
                color: 'warning',
                subtitle: 'Need attention'
            },
            {
                title: 'Overdue Action Items',
                value: this.dashboardData?.overdueActionItemCount ?? 0,
                icon: 'bi-exclamation-triangle',
                color: (this.dashboardData?.overdueActionItemCount ?? 0) > 0 ? 'danger' : 'success',
                subtitle: 'Past due'
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

    renderMeetingStats: function() {
        const container = document.getElementById('meetingStats');
        
        if (!container) return;
        
        const totalMeetings = (this.dashboardData?.todayMeetings?.length || 0) + (this.dashboardData?.upcomingMeetings?.length || 0);
        const weeklyMeetings = this.dashboardData?.upcomingMeetings?.length || 0;
        
        container.innerHTML = `
            <div class="meeting-stat">
                <div class="stat-label">Total Meetings</div>
                <div class="stat-value">${totalMeetings}</div>
            </div>
            <div class="meeting-stat">
                <div class="stat-label">This Week</div>
                <div class="stat-value">${weeklyMeetings}</div>
            </div>
            <div class="meeting-stat">
                <div class="stat-label">Has Standup Today</div>
                <div class="stat-value ${this.dashboardData?.hasStandupToday ? 'text-success' : 'text-warning'}">
                    ${this.dashboardData?.hasStandupToday ? '✓ Yes' : '⚠️ No'}
                </div>
            </div>
            <div class="meeting-stat">
                <div class="stat-label">Action Items</div>
                <div class="stat-value">${this.dashboardData?.pendingActionItemCount || 0}</div>
            </div>
        `;
    },

    renderUpcomingMeetings: function() {
        const container = document.getElementById('upcomingMeetings');
        const meetings = this.dashboardData.upcomingMeetings || [];
        
        if (meetings.length === 0) {
            container.innerHTML = '<p class="text-muted text-center">No upcoming meetings scheduled</p>';
            return;
        }

        container.innerHTML = meetings.map(meeting => {
            const meetingTypeClass = `meeting-type-${meeting.type.toLowerCase().replace(' ', '')}`;
            const statusClass = `meeting-status-${meeting.status.toLowerCase()}`;
            
            return `
                <div class="card mb-3 meeting-card ${meetingTypeClass}">
                    <div class="card-body">
                        <div class="d-flex justify-content-between align-items-start">
                            <div class="flex-grow-1">
                                <h6 class="mb-1">
                                    ${this.escapeHtml(meeting.title)}
                                    <span class="badge ${statusClass} ms-2">${meeting.status}</span>
                                </h6>
                                <p class="text-muted mb-2">
                                    <i class="bi bi-calendar"></i> ${new Date(meeting.scheduledAt).toLocaleString()}
                                    <br>
                                    <i class="bi bi-geo-alt"></i> ${this.escapeHtml(meeting.projectName)}
                                    <br>
                                    ${meeting.facilitatorName ? `<i class="bi bi-person"></i> Facilitator: ${this.escapeHtml(meeting.facilitatorName)}` : ''}
                                </p>
                                ${meeting.description ? `<p class="mb-2">${this.escapeHtml(meeting.description)}</p>` : ''}
                            </div>
                            <div class="text-end">
                                <div class="btn-group btn-group-sm">
                                    <button class="btn btn-outline-primary" onclick="meeting.viewMeeting(${meeting.id})">
                                        <i class="bi bi-eye"></i> View Details
                                    </button>
                                </div>
                                <div class="mt-2">
                                    ${this.renderParticipantAvatars(meeting.participants)}
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            `;
        }).join('');
    },

    renderParticipantAvatars: function(participants) {
        if (!participants || participants.length === 0) {
            return '<small class="text-muted">No participants</small>';
        }

        const maxVisible = 4;
        const visibleParticipants = participants.slice(0, maxVisible);
        const remainingCount = participants.length - maxVisible;

        return `
            <div class="d-flex align-items-center">
                ${visibleParticipants.map(p => `
                    <div class="participant-avatar me-1" title="${p.userName}">
                        ${p.userName.charAt(0).toUpperCase()}
                    </div>
                `).join('')}
                ${remainingCount > 0 ? `
                    <div class="participant-avatar more me-1" title="${remainingCount} more participants">
                        +${remainingCount}
                    </div>
                ` : ''}
            </div>
        `;
    },

    renderRecentStandups: function() {
        const container = document.getElementById('recentStandups');
        const standups = this.dashboardData.recentStandups || [];
        
        if (standups.length === 0) {
            container.innerHTML = '<p class="text-muted text-center">No recent standups found</p>';
            return;
        }

        container.innerHTML = standups.map(standup => {
            const hasBlockers = standup.hasBlockers;
            
            return `
                <div class="standup-item ${hasBlockers ? 'has-blockers' : ''}">
                    <div class="d-flex justify-content-between align-items-start mb-2">
                        <div>
                            <h6 class="mb-1">${this.escapeHtml(standup.userName)}</h6>
                            <small class="text-muted">
                                ${standup.projectName} • ${new Date(standup.date).toLocaleDateString()}
                                ${standup.isToday ? '<span class="badge bg-success ms-1">Today</span>' : ''}
                            </small>
                        </div>
                        ${hasBlockers ? '<i class="bi bi-exclamation-triangle text-warning"></i>' : ''}
                    </div>
                    
                    ${standup.yesterdayAccomplishments ? `
                        <div class="mb-2">
                            <strong>Yesterday:</strong> ${this.escapeHtml(standup.yesterdayAccomplishments)}
                        </div>
                    ` : ''}
                    
                    ${standup.todayGoals ? `
                        <div class="mb-2">
                            <strong>Today:</strong> ${this.escapeHtml(standup.todayGoals)}
                        </div>
                    ` : ''}
                    
                    ${standup.blockers ? `
                        <div class="mb-2">
                            <strong>Blockers:</strong> <span class="text-warning">${this.escapeHtml(standup.blockers)}</span>
                        </div>
                    ` : ''}
                    
                    ${standup.notes ? `
                        <div>
                            <strong>Notes:</strong> ${this.escapeHtml(standup.notes)}
                        </div>
                    ` : ''}
                </div>
            `;
        }).join('');
    },

    // Meeting Creation Functions
    showCreateMeetingModal: async function() {
        // Explicitly reset the form first (defensive — site.js hidden.bs.modal also handles this)
        const form = document.getElementById('createMeetingForm');
        if (form) form.reset();

        await this.loadProjects();
        await this.loadUsers();
        
        // Set default date and time
        const now = new Date();
        const localDate = new Date(now.getTime() - now.getTimezoneOffset() * 60000).toISOString().split('T')[0];
        const localTime = new Date(now.getTime() - now.getTimezoneOffset() * 60000).toISOString().slice(11, 16);
        
        document.getElementById('meetingDate').value = localDate;
        document.getElementById('meetingTime').value = localTime;
        
        const modal = new bootstrap.Modal(document.getElementById('createMeetingModal'));
        modal.show();
    },

    loadProjects: async function() {
        try {
            // Use user-projects endpoint which returns just the user's projects
            const response = await fetch('/api/projects/user-projects');
            if (!response.ok) throw new Error('Failed to load projects');

            const projects = this.normalizeData(await response.json());
            const select = document.getElementById('meetingProject');
            
            if (!select) return;
            
            select.innerHTML = '<option value="">Select project...</option>';
            
            // Handle both array response and object response
            const projectList = Array.isArray(projects) ? projects : (projects.items || []);
            
            projectList.forEach(project => {
                select.innerHTML += `<option value="${project.id}">${this.escapeHtml(project.name)}</option>`;
            });

        } catch (error) {
            console.error('Error loading projects:', error);
            showGlobalToast(error.message, 'error');
        }
    },

    loadUsers: async function() {
        try {
            const response = await fetch('/api/users');
            if (!response.ok) throw new Error('Failed to load users');

            const users = this.normalizeData(await response.json());
            const participantsSelect = document.getElementById('meetingParticipants');
            const facilitatorSelect = document.getElementById('meetingFacilitator');
            
            participantsSelect.innerHTML = '';
            facilitatorSelect.innerHTML = '<option value="">Select facilitator...</option>';
            
            users.forEach(user => {
                participantsSelect.innerHTML += `<option value="${user.id}">${this.escapeHtml(user.fullName)}</option>`;
                facilitatorSelect.innerHTML += `<option value="${user.id}">${this.escapeHtml(user.fullName)}</option>`;
            });

        } catch (error) {
            console.error('Error loading users:', error);
            showGlobalToast(error.message, 'error');
        }
    },

    loadProjectParticipants: async function(projectId) {
        if (!projectId) return;
        
        try {
            const response = await fetch(`/api/projects/${projectId}/members`);
            if (!response.ok) throw new Error('Failed to load project members');

            const members = this.normalizeData(await response.json());
            const select = document.getElementById('meetingParticipants');
            
            select.innerHTML = '';
            members.forEach(member => {
                select.innerHTML += `<option value="${member.userId}">${this.escapeHtml(member.userName)}</option>`;
            });

        } catch (error) {
            console.error('Error loading project members:', error);
            showGlobalToast(error.message, 'error');
        }
    },

    createMeeting: async function() {
        const title = document.getElementById('meetingTitle').value.trim();
        const type = document.getElementById('meetingType').value;
        const description = document.getElementById('meetingDescription').value.trim();
        const date = document.getElementById('meetingDate').value;
        const time = document.getElementById('meetingTime').value;
        const projectId = document.getElementById('meetingProject').value;
        const facilitatorId = document.getElementById('meetingFacilitator').value;
        
        const selectedParticipants = Array.from(document.getElementById('meetingParticipants').selectedOptions)
            .map(option => option.value);
        
        if (!title || !type || !date || !time || !projectId) {
            showGlobalToast('Please fill in all required fields', 'warning');
            return;
        }

        try {
            const scheduledAt = new Date(`${date}T${time}`);
            
            const meetingData = {
                title: title,
                description: description,
                type: type,
                scheduledAt: scheduledAt.toISOString(),
                projectId: parseInt(projectId),
                participantIds: selectedParticipants,
                facilitatedBy: facilitatorId || null
            };

            const response = await fetch('/api/meetings', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify(meetingData)
            });

            if (!response.ok) throw new Error('Failed to create meeting');

            showGlobalToast('Meeting created successfully', 'success');
            
            // Close modal and refresh
            const modal = bootstrap.Modal.getInstance(document.getElementById('createMeetingModal'));
            modal.hide();
            
            await this.loadDashboard();

        } catch (error) {
            console.error('Error creating meeting:', error);
            showGlobalToast(error.message, 'error');
        }
    },

    // Standup Functions
    submitStandup: function() {
        // Reset the standup form before opening
        const form = document.getElementById('standupForm');
        if (form) form.reset();

        this.loadStandupProjects();
        
        const modal = new bootstrap.Modal(document.getElementById('submitStandupModal'));
        modal.show();
    },

    loadStandupProjects: async function() {
        try {
            const response = await fetch('/api/projects/user-projects');
            if (!response.ok) throw new Error('Failed to load projects');

            const projects = this.normalizeData(await response.json());
            const select = document.getElementById('standupProject');
            
            select.innerHTML = '<option value="">Select project...</option>';
            
            const projectList = Array.isArray(projects) ? projects : (projects.items || []);
            
            projectList.forEach(project => {
                select.innerHTML += `<option value="${project.id}">${this.escapeHtml(project.name)}</option>`;
            });

        } catch (error) {
            console.error('Error loading projects:', error);
            showGlobalToast(error.message, 'error');
        }
    },

    saveStandup: async function() {
        const projectId = document.getElementById('standupProject').value;
        const yesterdayAccomplishments = document.getElementById('yesterdayAccomplishments').value.trim();
        const todayGoals = document.getElementById('todayGoals').value.trim();
        const blockers = document.getElementById('blockers').value.trim();
        const notes = document.getElementById('standupNotes').value.trim();
        
        if (!projectId) {
            showGlobalToast('Please select a project', 'warning');
            return;
        }

        try {
            const standupData = {
                projectId: parseInt(projectId),
                yesterdayAccomplishments: yesterdayAccomplishments || null,
                todayGoals: todayGoals || null,
                blockers: blockers || null,
                notes: notes || null
            };

            const response = await fetch('/api/meetings/standup', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify(standupData)
            });

            if (!response.ok) throw new Error('Failed to submit standup');

            showGlobalToast('Standup submitted successfully', 'success');
            
            // Close modal and refresh
            const modal = bootstrap.Modal.getInstance(document.getElementById('submitStandupModal'));
            modal.hide();
            
            await this.loadDashboard();

        } catch (error) {
            console.error('Error submitting standup:', error);
            showGlobalToast(error.message, 'error');
        }
    },

    // Quick Start Functions
    quickStartMeeting: function() {
        this.showCreateMeetingModal();
    },

    startDailyStandup: function() {
        this.showCreateMeetingModal();
        document.getElementById('meetingType').value = 'DailyStandup';
        document.getElementById('meetingTitle').value = 'Daily Standup';
    },

    startSprintRetro: function() {
        this.showCreateMeetingModal();
        document.getElementById('meetingType').value = 'SprintRetrospective';
        document.getElementById('meetingTitle').value = 'Sprint Retrospective';
    },

    startSprintPlanning: function() {
        this.showCreateMeetingModal();
        document.getElementById('meetingType').value = 'SprintPlanning';
        document.getElementById('meetingTitle').value = 'Sprint Planning';
    },

    startOneOnOne: function() {
        this.showCreateMeetingModal();
        document.getElementById('meetingType').value = 'OneOnOne';
        document.getElementById('meetingTitle').value = '1-on-1 Meeting';
    },

    // Meeting Actions
    viewMeeting: async function(meetingId) {
        try {
            const modalElement = document.getElementById('viewMeetingModal');
            const contentElement = document.getElementById('meetingDetailsContent');
            const modal = new bootstrap.Modal(modalElement);
            
            // Show loading state
            contentElement.innerHTML = `
                <div class="text-center py-4">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Loading details...</span>
                    </div>
                </div>
            `;
            modal.show();

            const response = await fetch(`/api/meetings/${meetingId}`);
            if (!response.ok) throw new Error('Failed to load meeting details');

            const meeting = this.normalizeData(await response.json());
            
            contentElement.innerHTML = `
                <div class="meeting-details">
                    <div class="row mb-4">
                        <div class="col-md-8">
                            <h4 class="mb-1">${this.escapeHtml(meeting.title)}</h4>
                            <span class="badge meeting-status-${meeting.status.toLowerCase()}">${meeting.status}</span>
                            <p class="text-muted mt-2">${this.escapeHtml(meeting.description || 'No description provided')}</p>
                        </div>
                        <div class="col-md-4 border-start">
                            <div class="mb-2">
                                <i class="bi bi-calendar-event text-primary me-2"></i>
                                <strong>Date:</strong> ${new Date(meeting.scheduledAt).toLocaleDateString()}
                            </div>
                            <div class="mb-2">
                                <i class="bi bi-clock text-primary me-2"></i>
                                <strong>Time:</strong> ${new Date(meeting.scheduledAt).toLocaleTimeString()}
                            </div>
                            <div class="mb-2">
                                <i class="bi bi-geo-alt text-primary me-2"></i>
                                <strong>Project:</strong> ${this.escapeHtml(meeting.projectName)}
                            </div>
                            <div class="mb-2">
                                <i class="bi bi-person-badge text-primary me-2"></i>
                                <strong>Facilitator:</strong> ${this.escapeHtml(meeting.facilitatorName || 'None')}
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <h6 class="border-bottom pb-2 mb-3">👥 Participants (${meeting.participants?.length || 0})</h6>
                            <div class="list-group list-group-flush">
                                ${meeting.participants?.map(p => `
                                    <div class="list-group-item px-0 d-flex align-items-center">
                                        <div class="participant-avatar me-2">${p.userName.charAt(0).toUpperCase()}</div>
                                        <div>
                                            <div class="fw-bold">${this.escapeHtml(p.userName)}</div>
                                            <small class="text-muted">${p.isFacilitator ? 'Facilitator' : 'Participant'}</small>
                                        </div>
                                    </div>
                                `).join('') || '<p class="text-muted">No participants listed</p>'}
                            </div>
                        </div>
                        <div class="col-md-6">
                            <h6 class="border-bottom pb-2 mb-3">📝 Notes & Decisions</h6>
                            <div class="meeting-notes-list">
                                ${meeting.notes?.map(n => `
                                    <div class="mb-3 p-2 border-start border-4 border-info bg-light">
                                        <div class="small fw-bold">${n.type}</div>
                                        <div>${this.escapeHtml(n.content)}</div>
                                    </div>
                                `).join('') || '<p class="text-muted italic">No notes captured yet</p>'}
                            </div>

                            <h6 class="border-bottom pb-2 mt-4 mb-3">✅ Action Items</h6>
                            <div class="action-items-list">
                                ${meeting.actionItems?.map(ai => `
                                    <div class="form-check mb-2">
                                        <input class="form-check-input" type="checkbox" ${ai.status === 'Completed' ? 'checked' : ''} disabled>
                                        <label class="form-check-label ${ai.status === 'Completed' ? 'text-decoration-line-through' : ''}">
                                            ${this.escapeHtml(ai.title)}
                                            <br><small class="text-muted">Assigned to: ${this.escapeHtml(ai.assignedToName || 'Unassigned')}</small>
                                        </label>
                                    </div>
                                `).join('') || '<p class="text-muted italic">No action items defined</p>'}
                            </div>
                        </div>
                    </div>
                </div>
            `;
        } catch (error) {
            console.error('Error loading meeting details:', error);
            showGlobalToast(error.message, 'error');
        }
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
        const loadingElement = document.getElementById('meetingLoading');
        const contentElement = document.getElementById('meetingContent');
        
        if (loading) {
            loadingElement.classList.remove('d-none');
            contentElement.classList.add('d-none');
        } else {
            loadingElement.classList.add('d-none');
            contentElement.classList.remove('d-none');
        }
    },

    showError: function(message) {
        const errorElement = document.getElementById('meetingError');
        const errorMessage = document.getElementById('errorMessage');
        
        errorMessage.textContent = message;
        errorElement.classList.remove('d-none');
    },

    hideError: function() {
        const errorElement = document.getElementById('meetingError');
        errorElement.classList.add('d-none');
    },

    escapeHtml: function(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    },

    normalizeData: function(data) {
        if (!data || typeof data !== 'object') return data;
        if (Array.isArray(data)) return data.map(item => this.normalizeData(item));
        
        const result = {};
        for (const key in data) {
            const camelKey = key.charAt(0).toLowerCase() + key.slice(1);
            result[camelKey] = this.normalizeData(data[key]);
        }
        return result;
    }
};

/* ═══════════════════════════════════════════════════════════════
   Agile Task Manager — kanban.js
   Kanban board functionality with drag & drop, SignalR integration
═══════════════════════════════════════════════════════════════ */

window.kanban = {
    projectId: null,
    sprintId: null,
    signalRConnection: null,
    boardData: null,
    draggedTask: null,

    init: async function (projectId, sprintId = null) {
        this.projectId = projectId;
        this.sprintId = sprintId;

        if (projectId) {
            await this.loadBoard();
            await this.initializeSignalR();
        } else {
            await this.loadProjects();
        }
        this.setupEventListeners();
    },

    loadBoard: async function () {
        try {
            this.showLoading(true);
            this.hideError();

            const response = await fetch(`/api/kanban/board?projectId=${this.projectId}${this.sprintId ? `&sprintId=${this.sprintId}` : ''}`, {
                credentials: 'include'
            });
            if (!response.ok) throw new Error('Failed to load board');

            const responseText = await response.text();

            // Debug: Log what we actually received
            console.log('Response status:', response.status);
            console.log('Response headers:', response.headers);
            console.log('First 500 chars of response:', responseText.substring(0, 500));

            // Check if response is HTML (error page) instead of JSON
            if (responseText.trim().startsWith('<')) {
                throw new Error('Server returned HTML instead of JSON. Authentication may be required.');
            }

            this.boardData = JSON.parse(responseText);
            this.renderBoard();
            this.showLoading(false);
        } catch (error) {
            console.error('Error loading board:', error);
            this.showError(error.message);
            this.showLoading(false);
        }
    },

    renderBoard: function () {
        const boardContainer = document.getElementById('kanbanBoard');
        const columnsContainer = document.getElementById('kanbanColumns');

        const columns = this.boardData?.columns ?? this.boardData?.Columns;
        if (!this.boardData || !columns) return;

        columnsContainer.innerHTML = '';

        columns.forEach(column => {
            const columnElement = this.createColumnElement(column);
            columnsContainer.appendChild(columnElement);
        });

        boardContainer.classList.remove('d-none');
    },

    createColumnElement: function (column) {
        const status = (column.status ?? column.Status ?? '').toString();
        const title = (column.title ?? column.Title ?? status).toString();
        const tasks = column.tasks ?? column.Tasks ?? [];

        const colDiv = document.createElement('div');
        colDiv.className = 'col';
        colDiv.innerHTML = `
            <div class="kanban-column" data-status="${status}">
                <div class="kanban-column-header status-${status.toLowerCase()}">
                    <div class="d-flex justify-content-between align-items-center">
                        <div>
                            <h5 class="mb-0">${title}</h5>
                            <small class="text-muted">${tasks.length} tasks</small>
                        </div>
                        <div class="dropdown">
                            <button class="btn btn-sm btn-outline-secondary" data-bs-toggle="dropdown">
                                <i class="bi bi-three-dots"></i>
                            </button>
                            <ul class="dropdown-menu">
                                <li><a class="dropdown-item" href="#" onclick="kanban.showCreateTaskModal('${status}')">
                                    <i class="bi bi-plus"></i> Add Task
                                </a></li>
                                <li><a class="dropdown-item" href="#" onclick="kanban.filterColumn('${status}')">
                                    <i class="bi bi-funnel"></i> Filter
                                </a></li>
                            </ul>
                        </div>
                    </div>
                </div>
                <div class="kanban-column-body" data-status="${status}" 
                     ondrop="kanban.handleDrop(event)" 
                     ondragover="kanban.handleDragOver(event)"
                     ondragleave="kanban.handleDragLeave(event)">
                    ${tasks.map(task => this.createTaskElement(task)).join('')}
                </div>
            </div>
        `;

        return colDiv;
    },

    createTaskElement: function (task) {
        const priorityValue = (task.priority ?? task.Priority ?? '').toString();
        const priorityClass = `priority-${priorityValue.toLowerCase()}`;
        const blockedClass = task.isBlocked ? 'blocked' : '';
        const overdueClass = task.isOverdue ? 'overdue' : '';

        return `
            <div class="kanban-task ${priorityClass} ${blockedClass} ${overdueClass}" 
                 data-task-id="${task.id ?? task.Id}" 
                 data-status="${task.status ?? task.Status}"
                 draggable="true"
                 ondragstart="kanban.handleDragStart(event)"
                 ondragend="kanban.handleDragEnd(event)"
                 onclick="kanban.showTaskDetails(${task.id ?? task.Id})">
                
                <div class="task-title">${this.escapeHtml(task.title ?? task.Title)}</div>
                
                <div class="task-meta">
                    <div class="task-assignee">
                        ${task.assignedToName || task.AssignedToName ?
                `<i class="bi bi-person-fill"></i> ${this.escapeHtml(task.assignedToName ?? task.AssignedToName)}` :
                '<i class="bi bi-person"></i> Unassigned'}
                    </div>
                    <div class="task-deadline">
                        ${task.deadline || task.Deadline ?
                `<i class="bi bi-calendar"></i> ${new Date(task.deadline ?? task.Deadline).toLocaleDateString()}` :
                ''}
                    </div>
                </div>
                
                ${(task.isBlocked ?? task.IsBlocked) ? `
                    <div class="task-dependency">
                        <i class="bi bi-exclamation-triangle"></i> Blocked by dependency
                    </div>
                ` : ''}
                
                ${(task.dependsOnTitle ?? task.DependsOnTitle) ? `
                    <div class="task-dependency">
                        <i class="bi bi-link"></i> Depends on: ${this.escapeHtml(task.dependsOnTitle ?? task.DependsOnTitle)}
                    </div>
                ` : ''}
                
                <div class="d-flex justify-content-between align-items-center mt-2">
                    <div class="btn-group btn-group-sm">
                        <button class="btn btn-outline-primary btn-sm" onclick="event.stopPropagation(); kanban.editTask(${task.id ?? task.Id})">
                            <i class="bi bi-pencil"></i>
                        </button>
                        <button class="btn btn-outline-danger btn-sm" onclick="event.stopPropagation(); kanban.deleteTask(${task.id ?? task.Id})">
                            <i class="bi bi-trash"></i>
                        </button>
                    </div>
                    <div class="task-actions">
                        ${(task.status ?? task.Status) !== 'Done' ? `
                            <button class="btn btn-sm btn-success" onclick="event.stopPropagation(); kanban.startTimer(${task.id ?? task.Id})">
                                <i class="bi bi-play-fill"></i>
                            </button>
                        ` : ''}
                    </div>
                </div>
            </div>
        `;
    },

    // Drag and Drop Handlers
    handleDragStart: function (event) {
        const taskElement = event.target;
        const taskId = taskElement.dataset.taskId;

        const cols = this.boardData.columns || this.boardData.Columns;
        this.draggedTask = cols
            .flatMap(col => col.tasks || col.Tasks)
            .find(task => (task.id ?? task.Id) == taskId);

        taskElement.classList.add('dragging');
        event.dataTransfer.effectAllowed = 'move';
        event.dataTransfer.setData('text/html', taskElement.innerHTML);
    },

    handleDragEnd: function (event) {
        event.target.classList.remove('dragging');
        document.querySelectorAll('.kanban-column').forEach(col => {
            col.classList.remove('drag-over');
        });
    },

    handleDragOver: function (event) {
        event.preventDefault();
        event.dataTransfer.dropEffect = 'move';

        const column = event.currentTarget.closest('.kanban-column');
        if (column) {
            column.classList.add('drag-over');
        }
    },

    handleDragLeave: function (event) {
        const column = event.currentTarget.closest('.kanban-column');
        if (column && !column.contains(event.relatedTarget)) {
            column.classList.remove('drag-over');
        }
    },

    handleDrop: async function (event) {
        event.preventDefault();

        const column = event.currentTarget.closest('.kanban-column');
        column.classList.remove('drag-over');

        if (!this.draggedTask) return;

        const newStatus = column.dataset.status;
        const currentStatus = (this.draggedTask.status ?? this.draggedTask.Status);
        if (newStatus === currentStatus) return;

        try {
            this.showLoading(true);

            const response = await fetch('/api/kanban/move', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify({
                    taskId: (this.draggedTask.id ?? this.draggedTask.Id),
                    newStatus: newStatus
                })
            });

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.error || 'Failed to move task');
            }

            // Update local data
            this.draggedTask.status = newStatus;

            // Show success message
            showGlobalToast(`Task "${this.draggedTask.title ?? this.draggedTask.Title}" moved to ${newStatus}`, 'success');

            // Refresh board
            await this.loadBoard();

        } catch (error) {
            console.error('Error moving task:', error);
            showGlobalToast(error.message, 'error');
        } finally {
            this.showLoading(false);
            this.draggedTask = null;
        }
    },

    // Task Management
    showCreateTaskModal: function (status = 'ToDo') {
        document.getElementById('taskModalLabel').textContent = 'Create Task';
        document.getElementById('taskForm').reset();
        document.getElementById('taskId').value = '';

        // Set default status
        const statusSelect = document.getElementById('taskStatus');
        if (statusSelect) {
            statusSelect.value = status;
        }

        // Load users and dependencies
        this.loadTaskFormOptions();

        const modal = new bootstrap.Modal(document.getElementById('taskModal'));
        modal.show();
    },

    editTask: async function (taskId) {
        try {
            const response = await fetch(`/api/tasks/${taskId}`);
            if (!response.ok) throw new Error('Failed to load task');

            const task = await response.json();

            // Reset form first to clear any previous edit's values
            document.getElementById('taskForm').reset();

            document.getElementById('taskModalLabel').textContent = 'Edit Task';
            document.getElementById('taskId').value = task.id;
            document.getElementById('taskTitle').value = task.title;
            document.getElementById('taskDescription').value = task.description || '';
            document.getElementById('taskPriority').value = task.priority;
            document.getElementById('taskStartDate').value = task.startDate ?
                new Date(task.startDate).toISOString().slice(0, 16) : '';
            document.getElementById('taskDeadline').value = task.deadline ?
                new Date(task.deadline).toISOString().slice(0, 16) : '';

            // Load options first, THEN restore selected values (ensures options exist in DOM)
            await this.loadTaskFormOptions();

            // Restore assigned-to and depends-on after options are loaded
            document.getElementById('taskAssignedTo').value = task.assignedToId || '';
            document.getElementById('taskDependsOn').value = task.dependsOnId || '';

            const modal = new bootstrap.Modal(document.getElementById('taskModal'));
            modal.show();

        } catch (error) {
            console.error('Error loading task:', error);
            showGlobalToast(error.message, 'error');
        }
    },

    deleteTask: async function (taskId) {
        if (!confirm('Are you sure you want to delete this task?')) return;

        try {
            const response = await fetch(`/api/tasks/${taskId}`, {
                method: 'DELETE',
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                }
            });

            if (!response.ok) throw new Error('Failed to delete task');

            showGlobalToast('Task deleted successfully', 'success');
            await this.loadBoard();

        } catch (error) {
            console.error('Error deleting task:', error);
            showGlobalToast(error.message, 'error');
        }
    },

    saveTask: async function () {
        const form = document.getElementById('taskForm');
        const formData = new FormData(form);
        const taskId = document.getElementById('taskId').value;

        const title = formData.get('title')?.trim();
        const startDate = formData.get('startDate') || null;
        const deadline = formData.get('deadline') || null;

        // ── Validation ──────────────────────────────────────────────
        if (!title) {
            showGlobalToast('Title is required.', 'error');
            return;
        }

        // Clear any previous date error
        document.getElementById('taskDateError')?.remove();

        if (startDate && deadline && new Date(startDate) > new Date(deadline)) {
            // Show inline error under the date fields
            const dateRow = document.getElementById('taskStartDate')?.closest('.row');
            let errEl = document.getElementById('taskDateError');
            if (!errEl) {
                errEl = document.createElement('div');
                errEl.id = 'taskDateError';
                errEl.className = 'col-12 text-danger small mt-1';
                errEl.innerHTML = '<i class="bi bi-exclamation-circle"></i> Start date/time must not be after end date/time.';
                dateRow?.appendChild(errEl);
            }
            showGlobalToast('Start date/time must not be after end date/time.', 'error');
            return;
        }
        if (deadline && new Date(deadline) < new Date()) {
            // warn only — don't block
            const confirmed = confirm('The deadline is in the past. Do you still want to save?');
            if (!confirmed) return;
        }

        const taskData = {
            title: title,
            description: formData.get('description'),
            priority: parseInt(formData.get('priority')),
            assignedToId: formData.get('assignedToId') || null,
            startDate: startDate,
            deadline: deadline,
            estimatedHours: parseFloat(formData.get('estimatedHours')) || 2.0,
            dependsOnId: formData.get('dependsOnId') ? parseInt(formData.get('dependsOnId')) : null,
            projectId: parseInt(formData.get('projectId')),
            sprintId: formData.get('sprintId') ? parseInt(formData.get('sprintId')) : null,
            status: 0  // ToDo by default for new tasks
        };

        if (taskId) {
            // For edits include the current status from the board
            const cols = this.boardData?.columns ?? this.boardData?.Columns ?? [];
            const existing = cols.flatMap(c => c.tasks ?? c.Tasks ?? []).find(t => (t.id ?? t.Id) == taskId);
            if (existing) {
                const statusMap = { 'ToDo': 0, 'InProgress': 1, 'Testing': 2, 'Done': 3 };
                taskData.status = statusMap[existing.status ?? existing.Status] ?? 0;
            }
            taskData.id = parseInt(taskId);
        }

        try {
            const url = taskId ? `/api/tasks/${taskId}` : '/api/tasks';
            const method = taskId ? 'PUT' : 'POST';

            const response = await fetch(url, {
                method: method,
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify(taskData)
            });

            if (!response.ok) {
                const err = await response.json().catch(() => ({ error: 'Unknown error' }));
                throw new Error(err.error || 'Failed to save task');
            }

            showGlobalToast(`Task ${taskId ? 'updated' : 'created'} successfully`, 'success');

            const modal = bootstrap.Modal.getInstance(document.getElementById('taskModal'));
            modal.hide();

            await this.loadBoard();

        } catch (error) {
            console.error('Error saving task:', error);
            showGlobalToast(error.message, 'error');
        }
    },

    loadTaskFormOptions: async function () {
        try {
            // Load all assignable users (Developer, TeamLead, Admin)
            const usersResponse = await fetch('/api/users');
            if (usersResponse.ok) {
                const users = await usersResponse.json();
                const assignedToSelect = document.getElementById('taskAssignedTo');
                const currentVal = assignedToSelect.value;
                assignedToSelect.innerHTML = '<option value="">Unassigned</option>';
                const assignableRoles = ['Developer', 'TeamLead', 'Admin'];
                users
                    .filter(u => assignableRoles.includes(u.role))
                    .forEach(user => {
                        const opt = document.createElement('option');
                        opt.value = user.id;
                        opt.textContent = `${user.fullName} (${user.role})`;
                        assignedToSelect.appendChild(opt);
                    });
                // Restore previous selection if editing
                if (currentVal) assignedToSelect.value = currentVal;
            }

            // Load tasks from THIS project for dependency selection
            const tasksResponse = await fetch(`/api/tasks/by-project/${this.projectId}`);
            if (tasksResponse.ok) {
                const tasks = await tasksResponse.json();
                const currentTaskId = document.getElementById('taskId').value;

                const dependsOnSelect = document.getElementById('taskDependsOn');
                const currentDepVal = dependsOnSelect.value;
                dependsOnSelect.innerHTML = '<option value="">No Dependency</option>';
                tasks.forEach(task => {
                    // API returns lowercase id and title
                    const id = task.id;
                    const title = task.title;
                    if (String(id) !== String(currentTaskId)) {
                        const opt = document.createElement('option');
                        opt.value = id;
                        opt.textContent = title;
                        dependsOnSelect.appendChild(opt);
                    }
                });
                // Restore previous selection if editing
                if (currentDepVal) dependsOnSelect.value = currentDepVal;
            }

        } catch (error) {
            console.error('Error loading form options:', error);
        }
    },

    // AI Assistant
    generateDescription: async function () {
        const title = document.getElementById('taskTitle').value.trim();
        if (!title) {
            showGlobalToast('Please enter a title first', 'warning');
            return;
        }

        try {
            const response = await fetch('/api/ai-assistant/generate-description', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify({
                    title: title,
                    projectContext: this.boardData?.projectName
                })
            });

            if (!response.ok) throw new Error('Failed to generate description');

            const result = await response.json();
            document.getElementById('taskDescription').value = result.description;

            showGlobalToast('Description generated successfully', 'success');

        } catch (error) {
            console.error('Error generating description:', error);
            showGlobalToast(error.message, 'error');
        }
    },

    // Task Details
    showTaskDetails: async function (taskId) {
        try {
            const response = await fetch(`/api/tasks/${taskId}`);
            if (!response.ok) throw new Error('Failed to load task details');

            const task = await response.json();

            const content = document.getElementById('taskDetailsContent');
            content.innerHTML = this.renderTaskDetails(task);

            const modal = new bootstrap.Modal(document.getElementById('taskDetailsModal'));
            modal.show();
        } catch (error) {
            console.error('Error loading task details:', error);
            showGlobalToast(error.message, 'error');
        }
    },

    renderTaskDetails: function (task) {
        return `
            <div class="row">
                <div class="col-md-8">
                    <h4>${this.escapeHtml(task.title)}</h4>
                    <div class="mb-3">
                        <span class="badge bg-${this.getPriorityColor(task.priority)}">${task.priority}</span>
                        <span class="badge bg-${this.getStatusColor(task.status)} ms-2">${task.status}</span>
                        ${task.isBlocked ? '<span class="badge bg-warning ms-2">Blocked</span>' : ''}
                        ${task.isOverdue ? '<span class="badge bg-danger ms-2">Overdue</span>' : ''}
                    </div>
                    
                    <div class="mb-3">
                        <h6>Description</h6>
                        <p>${task.description || 'No description provided'}</p>
                    </div>
                    
                    <div class="row mb-3">
                        <div class="col-md-4">
                            <h6>Assigned To</h6>
                            <p>${task.assignedToName || 'Unassigned'}</p>
                        </div>
                        <div class="col-md-4">
                            <h6>Start Date</h6>
                            <p>${task.startDate ? new Date(task.startDate).toLocaleString() : 'No start date'}</p>
                        </div>
                        <div class="col-md-4">
                            <h6>Deadline</h6>
                            <p>${task.deadline ? new Date(task.deadline).toLocaleString() : 'No deadline'}</p>
                        </div>
                    </div>
                    
                    ${task.dependsOnTitle ? `
                        <div class="mb-3">
                            <h6>Dependency</h6>
                            <p>This task depends on: <strong>${this.escapeHtml(task.dependsOnTitle)}</strong></p>
                        </div>
                    ` : ''}
                    
                    <div class="mb-3">
                        <h6>Created</h6>
                        <p>${new Date(task.createdAt).toLocaleString()}</p>
                    </div>
                </div>
                
                <div class="col-md-4">
                    <div class="card">
                        <div class="card-header">
                            <h6 class="mb-0">Actions</h6>
                        </div>
                        <div class="card-body">
                            <div class="d-grid gap-2">
                                <button class="btn btn-primary" onclick="kanban.editTask(${task.id})">
                                    <i class="bi bi-pencil"></i> Edit Task
                                </button>
                                ${task.status !== 'Done' ? `
                                    <button class="btn btn-success" onclick="kanban.startTimer(${task.id})">
                                        <i class="bi bi-play-fill"></i> Start Timer
                                    </button>
                                ` : ''}
                                <button class="btn btn-outline-info" onclick="kanban.viewDependencies(${task.id})">
                                    <i class="bi bi-diagram-3"></i> Dependencies
                                </button>
                                <button class="btn btn-outline-warning" onclick="kanban.viewTimeTracking(${task.id})">
                                    <i class="bi bi-clock"></i> Time Tracking
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;
    },

    // Time Tracking Integration
    startTimer: async function (taskId) {
        try {
            const response = await fetch('/api/time-tracking/timer/start', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify({ taskId: taskId })
            });

            if (!response.ok) throw new Error('Failed to start timer');

            showGlobalToast('Timer started successfully', 'success');

        } catch (error) {
            console.error('Error starting timer:', error);
            showGlobalToast(error.message, 'error');
        }
    },

    // SignalR Integration
    initializeSignalR: async function () {
        if (typeof signalR === 'undefined') return;

        this.signalRConnection = new signalR.HubConnectionBuilder()
            .withUrl('/taskHub')
            .withAutomaticReconnect([0, 2000, 5000, 10000])
            .build();

        // Listen for task moves
        this.signalRConnection.on('ReceiveTaskMove', (data) => {
            if (data.taskId && data.newStatus) {
                this.handleRealTimeTaskMove(data);
            }
        });

        // Listen for task updates
        this.signalRConnection.on('ReceiveKanbanTaskUpdate', (data) => {
            if (data.action === 'created' || data.action === 'updated') {
                this.loadBoard();
            }
        });

        try {
            await this.signalRConnection.start();
            console.log('[Kanban] SignalR connected');

            // Join Kanban board group AFTER connecting
            await this.signalRConnection.invoke('JoinKanbanBoard', this.projectId, this.sprintId);
        } catch (error) {
            console.error('[Kanban] SignalR connection failed:', error);
        }
    },

    handleRealTimeTaskMove: function (data) {
        const taskElement = document.querySelector(`[data-task-id="${data.taskId}"]`);
        if (taskElement) {
            const newColumn = document.querySelector(`[data-status="${data.newStatus}"] .kanban-column-body`);
            if (newColumn) {
                newColumn.appendChild(taskElement);
                showGlobalToast(`${data.movedBy} moved task to ${data.newStatus}`, 'info');
            }
        }
    },

    // Utility Methods
    setupEventListeners: function () {
        // Add any additional event listeners here
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                // Close any open modals
                const modals = document.querySelectorAll('.modal.show');
                modals.forEach(modal => {
                    const modalInstance = bootstrap.Modal.getInstance(modal);
                    if (modalInstance) modalInstance.hide();
                });
            }
        });
    },  // ← FIX: comma was missing here in the original file

    refreshBoard: function () {
        this.loadBoard();
    },

    // Called when the sprint selector dropdown in the board header changes.
    // Reloads the board for the selected sprint and updates the hidden
    // taskSprintId field so new tasks go into the right sprint.
    switchSprint: function (newSprintId) {
        this.sprintId = newSprintId ? parseInt(newSprintId) : null;
        // Update the hidden field so saveTask() sends the correct sprintId
        const hiddenSprintId = document.getElementById('taskSprintId');
        if (hiddenSprintId) hiddenSprintId.value = newSprintId || '';
        // Update the URL so the page can be bookmarked / refreshed
        const url = new URL(window.location.href);
        if (newSprintId) {
            url.searchParams.set('sprintId', newSprintId);
        } else {
            url.searchParams.delete('sprintId');
        }
        history.replaceState(null, '', url.toString());
        this.loadBoard();
    },

    showLoading: function (show) {
        const loadingElement = document.getElementById('kanbanLoading');
        const boardElement = document.getElementById('kanbanBoard');

        if (show) {
            loadingElement.classList.remove('d-none');
            boardElement.classList.add('d-none');
        } else {
            loadingElement.classList.add('d-none');
            boardElement.classList.remove('d-none');
        }
    },

    showError: function (message) {
        const errorElement = document.getElementById('kanbanError');
        const errorMessage = document.getElementById('errorMessage');

        errorMessage.textContent = message;
        errorElement.classList.remove('d-none');
    },

    hideError: function () {
        const errorElement = document.getElementById('kanbanError');
        errorElement.classList.add('d-none');
    },

    escapeHtml: function (text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    },

    getPriorityColor: function (priority) {
        switch (priority.toLowerCase()) {
            case 'high': return 'danger';
            case 'medium': return 'warning';
            case 'low': return 'success';
            default: return 'secondary';
        }
    },

    getStatusColor: function (status) {
        switch (status.toLowerCase()) {
            case 'todo': return 'secondary';
            case 'inprogress': return 'primary';
            case 'testing': return 'warning';
            case 'done': return 'success';
            default: return 'secondary';
        }
    },

    // Placeholder methods for future features
    viewDependencies: function (taskId) {
        showGlobalToast('Dependency view coming soon', 'info');
    },

    viewTimeTracking: function (taskId) {
        showGlobalToast('Time tracking view coming soon', 'info');
    },

    filterColumn: function (status) {
        showGlobalToast(`Filter ${status} column coming soon`, 'info');
    },

    loadProjects: async function () {
        try {
            this.showLoading(true);
            this.hideError();

            const response = await fetch('/api/projects/user-projects', {
                credentials: 'include'
            });
            if (!response.ok) throw new Error('Failed to load projects');

            const projects = await response.json();
            const selector = document.getElementById('projectSelector');

            selector.innerHTML = '<option value="">Select a project...</option>';
            projects.forEach(project => {
                const option = document.createElement('option');
                option.value = (project.id ?? project.Id);
                option.textContent = (project.name ?? project.Name);
                selector.appendChild(option);
            });

            this.showLoading(false);
        } catch (error) {
            console.error('Error loading projects:', error);
            this.showError('Failed to load projects');
            this.showLoading(false);
        }
    },

    loadSelectedProject: function () {
        const selector = document.getElementById('projectSelector');
        const projectId = parseInt(selector.value);

        if (projectId) {
            window.location.href = `/Kanban/Index?projectId=${projectId}`;
        }
    },

    showCreateProjectModal: function () {
        console.log('showCreateProjectModal called');
        const modal = document.getElementById('createProjectModal');
        console.log('Modal element:', modal);

        if (modal) {
            console.log('Creating bootstrap modal');
            const bsModal = new bootstrap.Modal(modal);
            console.log('Bootstrap modal created:', bsModal);
            bsModal.show();
            console.log('Modal should be visible now');
        } else {
            console.error('Modal element not found!');
        }
    },

    createProject: async function (event) {
        event.preventDefault();

        const form = document.getElementById('createProjectForm');
        const formData = new FormData(form);

        const projectData = {
            Name: formData.get('projectName')?.trim() || '',
            Description: formData.get('projectDescription')?.trim() || '',
            StartDate: formData.get('projectStartDate') || null,
            EndDate: formData.get('projectEndDate') || null
        };

        console.log('Project data being sent:', projectData);

        let token;
        try {
            token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
            console.log('Token found:', token ? 'Yes' : 'No');

            const response = await fetch('/api/projects', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify(projectData)
            });

            if (!response.ok) throw new Error('Failed to create project');

            const result = await response.json();

            // Close modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('createProjectModal'));
            if (modal) modal.hide();

            // Show success message
            showGlobalToast('Project created successfully!', 'success');

            // Reload projects list
            await this.loadProjects();

            // Auto-select the new project
            const selector = document.getElementById('projectSelector');
            selector.value = (result.id ?? result.Id);

        } catch (error) {
            console.error('Error creating project:', error);
            console.error('Project data being sent:', projectData);
            console.error('Token:', token);

            // Try to get more detailed error info
            let errorMessage = 'Failed to create project';
            if (error instanceof Response) {
                try {
                    const errorData = await error.json();
                    errorMessage = errorData.message || errorData.title || errorMessage;
                } catch (e) {
                    console.error('Could not parse error response:', e);
                }
            }

            showGlobalToast(errorMessage, 'error');
        }
    }

};
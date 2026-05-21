/* ═══════════════════════════════════════════════════════════════
   Agile Task Manager — dependency.js
   Task dependency management with graph visualization
═══════════════════════════════════════════════════════════════ */

window.dependency = {
    taskId: null,
    dependencyGraph: null,
    graphData: null,
    isLoading: false,

    init: async function(taskId) {
        this.taskId = parseInt(taskId, 10) || 0;
        if (this.taskId <= 0) {
            this.showError('Invalid task ID. Cannot load dependency graph.');
            return;
        }
        await this.loadDependencyData();
        this.setupEventListeners();
    },

    setupEventListeners: function() {
        // Auto-refresh every 30 seconds
        setInterval(() => {
            this.refresh();
        }, 30000);

        // Keyboard shortcuts
        document.addEventListener('keydown', (e) => {
            if (e.ctrlKey || e.metaKey) {
                switch (e.key) {
                    case 'r':
                        e.preventDefault();
                        this.refresh();
                        break;
                    case 'm':
                        e.preventDefault();
                        this.showManageModal();
                        break;
                }
            }
        });
    },

    loadDependencyData: async function() {
        try {
            if (!this.taskId || this.taskId <= 0) {
                throw new Error('Invalid task ID');
            }
            this.setLoading(true);
            this.hideError();

            const response = await fetch(`/api/task-dependencies/graph/${this.taskId}`);
            if (!response.ok) throw new Error('Failed to load dependency data');

            this.graphData = await response.json();
            this.renderDependencyGraph();
            this.renderTaskOverview();
            this.renderDependencySummary();
            this.renderDependenciesLists();
            this.setLoading(false);

        } catch (error) {
            console.error('Error loading dependency data:', error);
            this.showError(error.message);
            this.setLoading(false);
        }
    },

    renderDependencyGraph: function() {
        const container = document.getElementById('dependencyGraph');
        if (!container || !this.graphData) return;

        // Clear previous graph
        container.innerHTML = '';

        // Create nodes and edges for vis-network
        const nodes = new vis.DataSet([
            {
                id: this.graphData.RootTaskId,
                label: this.graphData.RootTaskTitle,
                color: { background: '#0d6efd', border: '#0a58ca' },
                font: { color: 'white', bold: true },
                size: 30,
                shape: 'box'
            },
            ...this.graphData.Dependencies.map(dep => ({
                id: dep.TaskId,
                label: dep.TaskTitle,
                color: { 
                    background: dep.IsCompleted ? '#6c757d' : '#dc3545',
                    border: dep.IsCompleted ? '#5a6268' : '#c82333'
                },
                font: { color: 'white' },
                size: dep.IsCompleted ? 20 : 25,
                shape: 'box'
            })),
            ...this.graphData.Dependents.map(dep => ({
                id: dep.TaskId,
                label: dep.TaskTitle,
                color: { 
                    background: dep.IsCompleted ? '#6c757d' : '#28a745',
                    border: dep.IsCompleted ? '#5a6268' : '#1e7e34'
                },
                font: { color: 'white' },
                size: dep.IsCompleted ? 20 : 25,
                shape: 'box'
            }))
        ]);

        const edges = new vis.DataSet([
            ...this.graphData.Dependencies.map(dep => ({
                from: dep.TaskId,
                to: this.graphData.RootTaskId,
                arrows: 'to',
                color: { color: '#dc3545' },
                width: dep.IsCompleted ? 1 : 3,
                dashes: dep.IsCompleted ? [5, 5] : false
            })),
            ...this.graphData.Dependents.map(dep => ({
                from: this.graphData.RootTaskId,
                to: dep.TaskId,
                arrows: 'to',
                color: { color: '#28a745' },
                width: dep.IsCompleted ? 1 : 3,
                dashes: dep.IsCompleted ? [5, 5] : false
            }))
        ]);

        const data = { nodes, edges };
        const options = {
            layout: {
                hierarchical: {
                    direction: 'UD',
                    sortMethod: 'directed',
                    levelSeparation: 100,
                    nodeSpacing: 150
                }
            },
            nodes: {
                margin: 10,
                font: {
                    size: 12,
                    face: 'Arial'
                }
            },
            edges: {
                smooth: {
                    type: 'cubicBezier',
                    roundness: 0.4
                }
            },
            physics: {
                enabled: false
            },
            interaction: {
                hover: true,
                tooltipDelay: 200
            }
        };

        this.dependencyGraph = new vis.Network(container, data, options);

        // Add click handler for nodes
        this.dependencyGraph.on('click', (params) => {
            if (params.nodes.length > 0) {
                const nodeId = params.nodes[0];
                if (nodeId !== this.taskId) {
                    window.open(`/Task/Details/${nodeId}`, '_blank');
                }
            }
        });

        // Add hover tooltip
        this.dependencyGraph.on('hoverNode', (params) => {
            const nodeId = params.node;
            const node = nodes.get(nodeId);
            // Custom tooltip logic could be added here
        });
    },

    renderTaskOverview: function() {
        const container = document.getElementById('taskOverview');
        if (!container || !this.graphData) return;

        const canStart = this.graphData.CanStart;
        const blockedCount = this.graphData.BlockedCount;
        const dependentCount = this.graphData.DependentCount;

        container.innerHTML = `
            <div class="row align-items-center">
                <div class="col-md-8">
                    <h5>${this.escapeHtml(this.graphData.RootTaskTitle)}</h5>
                    <div class="d-flex gap-3 mb-2">
                        <span class="badge bg-${this.getStatusColor(this.graphData.RootTaskStatus.toString())}">
                            ${this.graphData.RootTaskStatus}
                        </span>
                        <span class="badge bg-secondary">
                            Priority: ${this.graphData.RootTaskPriority}
                        </span>
                        <span class="can-start-indicator ${canStart ? 'can-start-yes' : 'can-start-no'}">
                            ${canStart ? '✓ Can Start' : '⚠️ Blocked'}
                        </span>
                    </div>
                    ${blockedCount > 0 ? `
                        <div class="alert alert-warning mb-0">
                            <i class="bi bi-exclamation-triangle me-2"></i>
                            This task is blocked by ${blockedCount} unfinished ${blockedCount === 1 ? 'dependency' : 'dependencies'}
                        </div>
                    ` : canStart ? `
                        <div class="alert alert-success mb-0">
                            <i class="bi bi-check-circle me-2"></i>
                            All dependencies are completed. This task can be started.
                        </div>
                    ` : ''}
                </div>
                <div class="col-md-4 text-center">
                    <div class="dependency-stats">
                        <div class="stat-label">Dependencies</div>
                        <div class="stat-value">${blockedCount}</div>
                    </div>
                    <div class="dependency-stats">
                        <div class="stat-label">Dependents</div>
                        <div class="stat-value">${dependentCount}</div>
                    </div>
                </div>
            </div>
        `;
    },

    renderDependencySummary: function() {
        const container = document.getElementById('dependencySummary');
        if (!container || !this.graphData) return;

        const totalDependencies = this.graphData.Dependencies.length;
        const totalDependents = this.graphData.Dependents.length;
        const completedDependencies = this.graphData.Dependencies.filter(d => d.IsCompleted).length;
        const completedDependents = this.graphData.Dependents.filter(d => d.IsCompleted).length;

        container.innerHTML = `
            <div class="dependency-stats">
                <div>
                    <div class="stat-label">Total Dependencies</div>
                    <div class="stat-value">${totalDependencies}</div>
                </div>
                <div>
                    <div class="stat-label">Completed</div>
                    <div class="stat-value text-success">${completedDependencies}</div>
                </div>
            </div>
            <div class="dependency-stats">
                <div>
                    <div class="stat-label">Total Dependents</div>
                    <div class="stat-value">${totalDependents}</div>
                </div>
                <div>
                    <div class="stat-label">Completed</div>
                    <div class="stat-value text-success">${completedDependents}</div>
                </div>
            </div>
            <div class="dependency-stats">
                <div>
                    <div class="stat-label">Can Start</div>
                    <div class="stat-value ${this.graphData.CanStart ? 'text-success' : 'text-danger'}">
                        ${this.graphData.CanStart ? 'Yes' : 'No'}
                    </div>
                </div>
                <div>
                    <div class="stat-label">Is Blocking</div>
                    <div class="stat-value ${this.graphData.IsBlockingDependents ? 'text-warning' : 'text-muted'}">
                        ${this.graphData.IsBlockingDependents ? 'Yes' : 'No'}
                    </div>
                </div>
            </div>
        `;
    },

    renderDependenciesLists: function() {
        this.renderBlockingTasks();
        this.renderDependentTasks();
    },

    renderBlockingTasks: function() {
        const container = document.getElementById('blockingTasks');
        if (!container) return;

        const dependencies = this.graphData?.Dependencies || [];
        
        if (dependencies.length === 0) {
            container.innerHTML = '<p class="text-muted text-center">No blocking dependencies</p>';
            return;
        }

        container.innerHTML = dependencies.map(dep => `
            <div class="dependency-item ${dep.IsCompleted ? 'completed' : ''} ${dep.IsOverdue ? 'overdue' : ''}">
                <div class="dependency-info">
                    <div class="d-flex justify-content-between align-items-start">
                        <div>
                            <h6 class="mb-1">
                                <a href="/Task/Details/${dep.TaskId}" target="_blank">
                                    ${this.escapeHtml(dep.TaskTitle)}
                                </a>
                                ${dep.IsCompleted ? 
                                    '<span class="task-status-badge status-done">Completed</span>' : 
                                    `<span class="task-status-badge status-${dep.TaskStatus.toString().toLowerCase().replace(' ', '')}">${dep.TaskStatus}</span>`
                                }
                            </h6>
                            <small class="text-muted">
                                Assigned to: ${dep.AssignedToName || 'Unassigned'}
                                ${dep.Deadline ? `• Deadline: ${new Date(dep.Deadline).toLocaleDateString()}` : ''}
                            </small>
                        </div>
                        <div class="text-end">
                            ${dep.IsCompleted ? 
                                '<i class="bi bi-check-circle text-success"></i>' : 
                                '<i class="bi bi-clock text-warning"></i>'
                            }
                        </div>
                    </div>
                </div>
                <div class="dependency-actions">
                    <button class="btn btn-sm btn-outline-primary" onclick="dependency.viewTask(${dep.TaskId})">
                        <i class="bi bi-eye"></i> View
                    </button>
                </div>
            </div>
        `).join('');
    },

    renderDependentTasks: function() {
        const container = document.getElementById('dependentTasks');
        if (!container) return;

        const dependents = this.graphData?.Dependents || [];
        
        if (dependents.length === 0) {
            container.innerHTML = '<p class="text-muted text-center">No dependent tasks</p>';
            return;
        }

        container.innerHTML = dependents.map(dep => `
            <div class="dependency-item ${dep.IsCompleted ? 'completed' : ''} ${dep.IsOverdue ? 'overdue' : ''}">
                <div class="dependency-info">
                    <div class="d-flex justify-content-between align-items-start">
                        <div>
                            <h6 class="mb-1">
                                <a href="/Task/Details/${dep.TaskId}" target="_blank">
                                    ${this.escapeHtml(dep.TaskTitle)}
                                </a>
                                ${dep.IsCompleted ? 
                                    '<span class="task-status-badge status-done">Completed</span>' : 
                                    `<span class="task-status-badge status-${dep.TaskStatus.toString().toLowerCase().replace(' ', '')}">${dep.TaskStatus}</span>`
                                }
                            </h6>
                            <small class="text-muted">
                                Assigned to: ${dep.AssignedToName || 'Unassigned'}
                                ${dep.Deadline ? `• Deadline: ${new Date(dep.Deadline).toLocaleDateString()}` : ''}
                            </small>
                        </div>
                        <div class="text-end">
                            ${dep.IsCompleted ? 
                                '<i class="bi bi-check-circle text-success"></i>' : 
                                '<i class="bi bi-hourglass-split text-info"></i>'
                            }
                        </div>
                    </div>
                </div>
                <div class="dependency-actions">
                    <button class="btn btn-sm btn-outline-primary" onclick="dependency.viewTask(${dep.TaskId})">
                        <i class="bi bi-eye"></i> View
                    </button>
                </div>
            </div>
        `).join('');
    },

    showManageModal: async function() {
        const modal = document.getElementById('manageDependenciesModal');
        const select = document.getElementById('dependsOnTask');
        
        try {
            // Load available tasks for dependency selection
            const response = await fetch('/api/tasks');
            if (!response.ok) throw new Error('Failed to load tasks');
            
            const tasks = await response.json();
            
            // Filter out current task and existing dependencies
            const existingDeps = this.graphData?.Dependencies || [];
            const availableTasks = tasks.filter(task => 
                task.Id !== this.taskId && 
                !existingDeps.some(dep => dep.TaskId === task.Id)
            );
            
            select.innerHTML = '<option value="">Select a task to depend on...</option>';
            availableTasks.forEach(task => {
                select.innerHTML += `<option value="${task.Id}">${this.escapeHtml(task.Title)}</option>`;
            });
            
            // Show current dependencies
            this.renderCurrentDependencies();
            
            const modalInstance = new bootstrap.Modal(modal);
            modalInstance.show();
            
        } catch (error) {
            console.error('Error loading tasks for dependency management:', error);
            showGlobalToast(error.message, 'error');
        }
    },

    renderCurrentDependencies: function() {
        const container = document.getElementById('currentDependencies');
        if (!container) return;
        
        const dependencies = this.graphData?.Dependencies || [];
        
        if (dependencies.length === 0) {
            container.innerHTML = '<p class="text-muted">No current dependencies</p>';
            return;
        }
        
        container.innerHTML = dependencies.map(dep => `
            <div class="d-flex justify-content-between align-items-center mb-2 p-2 border rounded">
                <div>
                    <strong>${this.escapeHtml(dep.TaskTitle)}</strong>
                    <span class="badge bg-${dep.IsCompleted ? 'success' : 'warning'} ms-2">
                        ${dep.IsCompleted ? 'Completed' : 'Pending'}
                    </span>
                </div>
                <button type="button" class="btn btn-sm btn-outline-danger" onclick="dependency.removeDependency(${dep.TaskId})">
                    <i class="bi bi-trash"></i> Remove
                </button>
            </div>
        `).join('');
    },

    saveDependencies: async function() {
        const selectedTaskId = document.getElementById('dependsOnTask').value;
        
        if (!selectedTaskId) {
            showGlobalToast('Please select a task to add as dependency', 'warning');
            return;
        }
        
        try {
            const response = await fetch('/api/task-dependencies/create', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify({
                    taskId: this.taskId,
                    dependsOnTaskId: parseInt(selectedTaskId)
                })
            });
            
            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.error || 'Failed to add dependency');
            }
            
            showGlobalToast('Dependency added successfully', 'success');
            
            // Close modal and refresh
            const modal = bootstrap.Modal.getInstance(document.getElementById('manageDependenciesModal'));
            modal.hide();
            
            await this.loadDependencyData();
            
        } catch (error) {
            console.error('Error adding dependency:', error);
            showGlobalToast(error.message, 'error');
        }
    },

    removeDependency: async function(dependsOnTaskId) {
        if (!confirm('Are you sure you want to remove this dependency?')) return;
        
        try {
            const response = await fetch('/api/task-dependencies/remove', {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify({
                    taskId: this.taskId,
                    dependsOnTaskId: dependsOnTaskId
                })
            });
            
            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.error || 'Failed to remove dependency');
            }
            
            showGlobalToast('Dependency removed successfully', 'success');
            
            // Refresh the modal content
            await this.showManageModal();
            
        } catch (error) {
            console.error('Error removing dependency:', error);
            showGlobalToast(error.message, 'error');
        }
    },

    viewTask: function(taskId) {
        window.open(`/Task/Details/${taskId}`, '_blank');
    },

    refresh: function() {
        this.loadDependencyData();
    },

    // Utility Methods
    setLoading: function(loading) {
        this.isLoading = loading;
        const loadingElement = document.getElementById('dependencyLoading');
        const contentElement = document.getElementById('dependencyContent');
        
        if (loading) {
            loadingElement.classList.remove('d-none');
            contentElement.classList.add('d-none');
        } else {
            loadingElement.classList.add('d-none');
            contentElement.classList.remove('d-none');
        }
    },

    showError: function(message) {
        const errorElement = document.getElementById('dependencyError');
        const errorMessage = document.getElementById('errorMessage');
        
        errorMessage.textContent = message;
        errorElement.classList.remove('d-none');
    },

    hideError: function() {
        const errorElement = document.getElementById('dependencyError');
        errorElement.classList.add('d-none');
    },

    getStatusColor: function(status) {
        switch (status.toLowerCase()) {
            case 'todo': return 'secondary';
            case 'inprogress': return 'primary';
            case 'testing': return 'warning';
            case 'done': return 'success';
            default: return 'secondary';
        }
    },

    escapeHtml: function(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
};

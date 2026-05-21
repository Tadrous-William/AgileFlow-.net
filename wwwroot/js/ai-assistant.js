/* ═══════════════════════════════════════════════════════════════
   Agile Task Manager — ai-assistant.js
   AI Assistant functionality for task management
═══════════════════════════════════════════════════════════════ */

window.aiAssistant = {
    recentActivities: [],
    isLoading: false,

    init: async function() {
        this.setupEventListeners();
        await this.loadTasks();
        this.updateRecentActivities();
    },

    setupEventListeners: function() {
        // Auto-generate suggestions toggle
        document.getElementById('autoGenerate')?.addEventListener('change', (e) => {
            if (e.target.checked) {
                this.enableAutoGeneration();
            } else {
                this.disableAutoGeneration();
            }
        });

        // Keyboard shortcuts
        document.addEventListener('keydown', (e) => {
            if (e.ctrlKey || e.metaKey) {
                switch (e.key) {
                    case 'g':
                        e.preventDefault();
                        this.showDescriptionGenerator();
                        break;
                    case 't':
                        e.preventDefault();
                        this.showTitleSuggester();
                        break;
                    case 'l':
                        e.preventDefault();
                        this.showTagGenerator();
                        break;
                    case 'a':
                        e.preventDefault();
                        this.showTaskAnalyzer();
                        break;
                }
            }
        });
    },

    // Tool Display Methods
    showDescriptionGenerator: function() {
        this.hideAllTools();
        document.getElementById('descriptionGenerator').classList.remove('d-none');
        document.getElementById('welcomeScreen').classList.add('d-none');
        document.getElementById('taskTitle')?.focus();
    },

    showTitleSuggester: function() {
        this.hideAllTools();
        document.getElementById('titleSuggester').classList.remove('d-none');
        document.getElementById('welcomeScreen').classList.add('d-none');
        document.getElementById('taskDescription')?.focus();
    },

    showTagGenerator: function() {
        this.hideAllTools();
        document.getElementById('tagGenerator').classList.remove('d-none');
        document.getElementById('welcomeScreen').classList.add('d-none');
        document.getElementById('tagTaskTitle')?.focus();
    },

    showTaskAnalyzer: function() {
        this.hideAllTools();
        document.getElementById('taskAnalyzer').classList.remove('d-none');
        document.getElementById('welcomeScreen').classList.add('d-none');
    },

    hideAllTools: function() {
        const tools = ['descriptionGenerator', 'titleSuggester', 'tagGenerator', 'taskAnalyzer'];
        tools.forEach(tool => {
            document.getElementById(tool)?.classList.add('d-none');
        });
        document.getElementById('welcomeScreen')?.classList.remove('d-none');
    },

    // Description Generator
    generateDescription: async function() {
        const title = document.getElementById('taskTitle').value.trim();
        const projectContext = document.getElementById('projectContext').value.trim();
        
        if (!title) {
            showGlobalToast('Please enter a task title', 'warning');
            return;
        }

        try {
            this.setLoading(true);
            
            const response = await fetch('/api/ai-assistant/generate-description', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify({
                    title: title,
                    projectContext: projectContext || null
                })
            });

            if (!response.ok) throw new Error('Failed to generate description');

            const result = await response.json();
            
            this.displayGeneratedDescription(result.description);
            this.addActivity('Generated description', title);
            showGlobalToast('Description generated successfully', 'success');
            
        } catch (error) {
            console.error('Error generating description:', error);
            showGlobalToast(error.message, 'error');
        } finally {
            this.setLoading(false);
        }
    },

    displayGeneratedDescription: function(description) {
        const resultDiv = document.getElementById('descriptionResult');
        const descriptionDiv = document.getElementById('generatedDescription');
        
        descriptionDiv.textContent = description;
        resultDiv.classList.remove('d-none');
        
        // Add confidence badge if enabled
        if (document.getElementById('showConfidence')?.checked) {
            const confidence = this.calculateConfidence(description);
            const confidenceBadge = document.createElement('span');
            confidenceBadge.className = `confidence-badge confidence-${confidence.level}`;
            confidenceBadge.textContent = `${confidence.score}% confidence`;
            descriptionDiv.appendChild(confidenceBadge);
        }
    },

    copyDescription: function() {
        const description = document.getElementById('generatedDescription').textContent.trim();
        if (!description) return;

        navigator.clipboard.writeText(description).then(() => {
            showGlobalToast('Description copied to clipboard', 'success');
        }).catch(err => {
            console.error('Failed to copy:', err);
            showGlobalToast('Failed to copy to clipboard', 'error');
        });
    },

    useDescription: function() {
        const description = document.getElementById('generatedDescription').textContent.trim();
        if (!description) return;

        // Store in localStorage for use in task creation
        localStorage.setItem('aiGeneratedDescription', description);
        showGlobalToast('Description ready for use in task creation', 'success');
        
        // Optionally redirect to task creation
        if (confirm('Description saved! Would you like to create a new task with this description?')) {
            window.location.href = '/Task/Create';
        }
    },

    clearDescriptionForm: function() {
        document.getElementById('descriptionForm').reset();
        document.getElementById('descriptionResult').classList.add('d-none');
    },

    // Title Suggester
    suggestTitle: async function() {
        const description = document.getElementById('taskDescription').value.trim();
        
        if (!description) {
            showGlobalToast('Please enter a task description', 'warning');
            return;
        }

        try {
            this.setLoading(true);
            
            const response = await fetch('/api/ai-assistant/suggest-title', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify({
                    description: description
                })
            });

            if (!response.ok) throw new Error('Failed to suggest titles');

            const result = await response.json();
            
            this.displaySuggestedTitles(result.title);
            this.addActivity('Suggested titles', description.substring(0, 50) + '...');
            showGlobalToast('Titles suggested successfully', 'success');
            
        } catch (error) {
            console.error('Error suggesting titles:', error);
            showGlobalToast(error.message, 'error');
        } finally {
            this.setLoading(false);
        }
    },

    displaySuggestedTitles: function(primaryTitle) {
        const resultsDiv = document.getElementById('titleResults');
        const titlesDiv = document.getElementById('suggestedTitles');
        
        // Generate variations of the primary title
        const titles = [
            primaryTitle,
            this.generateTitleVariation(primaryTitle, 'action'),
            this.generateTitleVariation(primaryTitle, 'technical'),
            this.generateTitleVariation(primaryTitle, 'user-focused'),
            this.generateTitleVariation(primaryTitle, 'concise')
        ].filter((title, index, arr) => arr.indexOf(title) === index); // Remove duplicates

        titlesDiv.innerHTML = titles.map((title, index) => `
            <div class="list-group-item suggestion-item" onclick="aiAssistant.selectTitle('${this.escapeHtml(title)}')">
                <div class="d-flex justify-content-between align-items-center">
                    <span>${this.escapeHtml(title)}</span>
                    <div>
                        ${index === 0 ? '<span class="badge bg-primary">Recommended</span>' : ''}
                        ${document.getElementById('showConfidence')?.checked ? 
                            `<span class="confidence-badge confidence-high">${Math.max(95 - index * 10, 60)}%</span>` : ''}
                    </div>
                </div>
            </div>
        `).join('');

        resultsDiv.classList.remove('d-none');
    },

    generateTitleVariation: function(original, type) {
        const words = original.toLowerCase().split(' ');
        
        switch (type) {
            case 'action':
                return words.map(word => {
                    const actionWords = ['implement', 'create', 'develop', 'build', 'design', 'fix', 'update', 'optimize'];
                    return actionWords.includes(word) ? word : word;
                }).join(' ');
            
            case 'technical':
                return words.map(word => {
                    const techWords = ['api', 'database', 'ui', 'frontend', 'backend', 'system', 'module', 'component'];
                    return techWords.some(tech => word.includes(tech)) ? word : word;
                }).join(' ');
            
            case 'user-focused':
                return words.map(word => {
                    const userWords = ['user', 'customer', 'client', 'experience', 'interface', 'journey'];
                    return userWords.includes(word) ? word : word;
                }).join(' ');
            
            case 'concise':
                return words.slice(0, 4).join(' ');
            
            default:
                return original;
        }
    },

    selectTitle: function(title) {
        localStorage.setItem('aiSuggestedTitle', title);
        showGlobalToast('Title selected! You can use it when creating a task.', 'success');
        
        if (confirm('Title selected! Would you like to create a new task with this title?')) {
            window.location.href = '/Task/Create';
        }
    },

    clearTitleForm: function() {
        document.getElementById('titleForm').reset();
        document.getElementById('titleResults').classList.add('d-none');
    },

    // Tag Generator
    generateTags: async function() {
        const title = document.getElementById('tagTaskTitle').value.trim();
        const description = document.getElementById('tagTaskDescription').value.trim();
        
        if (!title && !description) {
            showGlobalToast('Please enter a title or description', 'warning');
            return;
        }

        try {
            this.setLoading(true);
            
            const response = await fetch('/api/ai-assistant/suggest-tags', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify({
                    title: title,
                    description: description
                })
            });

            if (!response.ok) throw new Error('Failed to generate tags');

            const result = await response.json();
            
            this.displayGeneratedTags(result.tags);
            this.addActivity('Generated tags', title || description.substring(0, 50) + '...');
            showGlobalToast('Tags generated successfully', 'success');
            
        } catch (error) {
            console.error('Error generating tags:', error);
            showGlobalToast(error.message, 'error');
        } finally {
            this.setLoading(false);
        }
    },

    displayGeneratedTags: function(tags) {
        const resultsDiv = document.getElementById('tagResults');
        const tagsDiv = document.getElementById('generatedTags');
        
        tagsDiv.innerHTML = tags.map(tag => `
            <span class="tag-item" onclick="aiAssistant.selectTag('${this.escapeHtml(tag)}')">
                ${this.escapeHtml(tag)}
                <i class="bi bi-plus-circle ms-1"></i>
            </span>
        `).join('');

        resultsDiv.classList.remove('d-none');
    },

    selectTag: function(tag) {
        // Store selected tags in localStorage
        let selectedTags = JSON.parse(localStorage.getItem('aiSelectedTags') || '[]');
        if (!selectedTags.includes(tag)) {
            selectedTags.push(tag);
            localStorage.setItem('aiSelectedTags', JSON.stringify(selectedTags));
            showGlobalToast(`Tag "${tag}" selected!`, 'success');
        }
    },

    clearTagForm: function() {
        document.getElementById('tagForm').reset();
        document.getElementById('tagResults').classList.add('d-none');
    },

    // Task Analyzer
    analyzeTask: async function() {
        const taskId = document.getElementById('analyzerTaskId').value;
        
        if (!taskId) {
            showGlobalToast('Please select a task to analyze', 'warning');
            return;
        }

        try {
            this.setLoading(true);
            
            // Get task details
            const taskResponse = await fetch(`/api/tasks/${taskId}`);
            if (!taskResponse.ok) throw new Error('Failed to load task');
            
            const task = await taskResponse.json();
            
            // Perform AI analysis (simulate for now)
            const analysis = this.performTaskAnalysis(task);
            
            this.displayAnalysisResults(analysis);
            this.addActivity('Analyzed task', task.title);
            showGlobalToast('Task analysis completed', 'success');
            
        } catch (error) {
            console.error('Error analyzing task:', error);
            showGlobalToast(error.message, 'error');
        } finally {
            this.setLoading(false);
        }
    },

    performTaskAnalysis: function(task) {
        // Simulated AI analysis
        const complexity = this.calculateComplexity(task);
        const estimatedHours = this.estimateHours(task, complexity);
        const riskLevel = this.assessRisk(task);
        const suggestions = this.generateSuggestions(task);
        
        return {
            complexity,
            estimatedHours,
            riskLevel,
            suggestions,
            completeness: this.assessCompleteness(task),
            priorityAlignment: this.assessPriorityAlignment(task)
        };
    },

    calculateComplexity: function(task) {
        let score = 0;
        
        // Title complexity
        if (task.title) {
            score += task.title.split(' ').length * 2;
        }
        
        // Description complexity
        if (task.description) {
            score += task.description.length / 10;
        }
        
        // Priority factor
        if (task.priority === 'High') score += 20;
        if (task.priority === 'Medium') score += 10;
        
        // Dependency factor
        if (task.dependsOnId) score += 15;
        
        if (score < 20) return 'Low';
        if (score < 40) return 'Medium';
        if (score < 60) return 'High';
        return 'Very High';
    },

    estimateHours: function(task, complexity) {
        const baseHours = {
            'Low': 2,
            'Medium': 4,
            'High': 8,
            'Very High': 16
        };
        
        let hours = baseHours[complexity] || 4;
        
        // Adjust based on priority
        if (task.priority === 'High') hours *= 1.5;
        if (task.priority === 'Low') hours *= 0.8;
        
        return Math.round(hours);
    },

    assessRisk: function(task) {
        let riskScore = 0;
        
        // Overdue risk
        if (task.deadline && new Date(task.deadline) < new Date()) {
            riskScore += 30;
        }
        
        // Dependency risk
        if (task.dependsOnId) riskScore += 20;
        
        // No assignee risk
        if (!task.assignedToId) riskScore += 15;
        
        // High priority risk
        if (task.priority === 'High') riskScore += 10;
        
        if (riskScore < 15) return 'Low';
        if (riskScore < 30) return 'Medium';
        return 'High';
    },

    generateSuggestions: function(task) {
        const suggestions = [];
        
        if (!task.description) {
            suggestions.push('Add a detailed description to clarify requirements');
        }
        
        if (!task.assignedToId) {
            suggestions.push('Assign this task to a team member');
        }
        
        if (!task.deadline) {
            suggestions.push('Set a deadline to track progress');
        }
        
        if (task.priority === 'High' && !task.dependsOnId) {
            suggestions.push('Consider breaking down this high-priority task into smaller subtasks');
        }
        
        if (task.dependsOnId) {
            suggestions.push('Ensure the dependency task is completed before starting this task');
        }
        
        if (suggestions.length === 0) {
            suggestions.push('Task appears well-structured and ready for execution');
        }
        
        return suggestions;
    },

    assessCompleteness: function(task) {
        let score = 0;
        
        if (task.title && task.title.length > 5) score += 25;
        if (task.description && task.description.length > 20) score += 25;
        if (task.assignedToId) score += 25;
        if (task.deadline) score += 25;
        
        return score;
    },

    assessPriorityAlignment: function(task) {
        // Simple priority alignment assessment
        if (task.priority === 'High' && task.deadline && new Date(task.deadline) < new Date(Date.now() + 7 * 24 * 60 * 60 * 1000)) {
            return 'Well Aligned';
        }
        
        if (task.priority === 'Low' && (!task.deadline || new Date(task.deadline) > new Date(Date.now() + 30 * 24 * 60 * 60 * 1000))) {
            return 'Well Aligned';
        }
        
        return 'Review Recommended';
    },

    displayAnalysisResults: function(analysis) {
        const resultsDiv = document.getElementById('analysisResults');
        const contentDiv = document.getElementById('analysisContent');
        
        contentDiv.innerHTML = `
            <div class="analysis-metric">
                <span>Complexity:</span>
                <span class="badge bg-${this.getComplexityColor(analysis.complexity)}">${analysis.complexity}</span>
            </div>
            <div class="analysis-metric">
                <span>Estimated Hours:</span>
                <span><strong>${analysis.estimatedHours}h</strong></span>
            </div>
            <div class="analysis-metric">
                <span>Risk Level:</span>
                <span class="badge bg-${this.getRiskColor(analysis.riskLevel)}">${analysis.riskLevel}</span>
            </div>
            <div class="analysis-metric">
                <span>Completeness:</span>
                <div class="progress" style="width: 200px; height: 8px;">
                    <div class="progress-bar" style="width: ${analysis.completeness}%"></div>
                </div>
                <span class="ms-2">${analysis.completeness}%</span>
            </div>
            <div class="analysis-metric">
                <span>Priority Alignment:</span>
                <span class="badge bg-${analysis.priorityAlignment === 'Well Aligned' ? 'success' : 'warning'}">${analysis.priorityAlignment}</span>
            </div>
            
            <div class="mt-3">
                <h6>Suggestions:</h6>
                <ul class="list-unstyled">
                    ${analysis.suggestions.map(suggestion => `
                        <li class="mb-2">
                            <i class="bi bi-lightbulb text-warning me-2"></i>
                            ${this.escapeHtml(suggestion)}
                        </li>
                    `).join('')}
                </ul>
            </div>
        `;

        resultsDiv.classList.remove('d-none');
    },

    // Utility Methods
    setLoading: function(loading) {
        this.isLoading = loading;
        const loadingDiv = document.getElementById('aiLoading');
        
        if (loading) {
            loadingDiv.classList.remove('d-none');
        } else {
            loadingDiv.classList.add('d-none');
        }
    },

    loadTasks: async function() {
        try {
            const response = await fetch('/api/tasks');
            if (response.ok) {
                const tasks = await response.json();
                const select = document.getElementById('analyzerTaskId');
                if (select) {
                    select.innerHTML = '<option value="">Choose a task to analyze...</option>';
                    tasks.forEach(task => {
                        select.innerHTML += `<option value="${task.id}">${this.escapeHtml(task.title)}</option>`;
                    });
                }
            }
        } catch (error) {
            console.error('Error loading tasks:', error);
        }
    },

    addActivity: function(action, details) {
        const activity = {
            action: action,
            details: details,
            timestamp: new Date(),
            icon: this.getActivityIcon(action)
        };
        
        this.recentActivities.unshift(activity);
        if (this.recentActivities.length > 10) {
            this.recentActivities.pop();
        }
        
        this.updateRecentActivities();
        
        // Send notification if enabled
        if (document.getElementById('enableNotifications')?.checked) {
            showGlobalToast(`AI ${action}: ${details}`, 'info');
        }
    },

    updateRecentActivities: function() {
        const container = document.getElementById('recentActivities');
        if (!container) return;
        
        if (this.recentActivities.length === 0) {
            container.innerHTML = '<p class="text-muted">No recent AI activities</p>';
            return;
        }
        
        container.innerHTML = this.recentActivities.map(activity => `
            <div class="activity-item">
                <div class="activity-icon bg-primary text-white">
                    <i class="bi ${activity.icon}"></i>
                </div>
                <div class="activity-content">
                    <div>${activity.action}</div>
                    <div class="text-muted">${this.escapeHtml(activity.details)}</div>
                    <div class="activity-time">${this.formatTime(activity.timestamp)}</div>
                </div>
            </div>
        `).join('');
    },

    enableAutoGeneration: function() {
        // Enable auto-generation features
        console.log('Auto-generation enabled');
    },

    disableAutoGeneration: function() {
        // Disable auto-generation features
        console.log('Auto-generation disabled');
    },

    calculateConfidence: function(text) {
        // Simple confidence calculation based on text length and structure
        const length = text.length;
        const sentences = text.split(/[.!?]+/).length;
        
        let score = 70; // Base score
        
        if (length > 100) score += 10;
        if (length > 200) score += 10;
        if (sentences > 2) score += 5;
        if (text.includes(':') || text.includes(';')) score += 5;
        
        score = Math.min(score, 95);
        
        return {
            score: score,
            level: score >= 80 ? 'high' : score >= 60 ? 'medium' : 'low'
        };
    },

    getActivityIcon: function(action) {
        switch (action.toLowerCase()) {
            case 'generated description': return 'bi-magic';
            case 'suggested titles': return 'bi-lightbulb';
            case 'generated tags': return 'bi-tags';
            case 'analyzed task': return 'bi-graph-up';
            default: return 'bi-robot';
        }
    },

    getComplexityColor: function(complexity) {
        switch (complexity) {
            case 'Low': return 'success';
            case 'Medium': return 'info';
            case 'High': return 'warning';
            case 'Very High': return 'danger';
            default: return 'secondary';
        }
    },

    getRiskColor: function(risk) {
        switch (risk) {
            case 'Low': return 'success';
            case 'Medium': return 'warning';
            case 'High': return 'danger';
            default: return 'secondary';
        }
    },

    formatTime: function(timestamp) {
        const date = new Date(timestamp);
        const now = new Date();
        const diffMs = now - date;
        const diffMins = Math.floor(diffMs / 60000);
        
        if (diffMins < 1) return 'Just now';
        if (diffMins < 60) return `${diffMins}m ago`;
        
        const diffHours = Math.floor(diffMs / 3600000);
        if (diffHours < 24) return `${diffHours}h ago`;
        
        return date.toLocaleDateString();
    },

    escapeHtml: function(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
};

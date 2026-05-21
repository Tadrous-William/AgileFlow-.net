class AchievementToast {
    constructor() {
        this.connection = null;
        this.userId = null;
        this.currentProjectId = null;
        this.init();
    }

    async init() {
        // Get current user ID from page or make an API call
        this.userId = await this.getCurrentUserId();
        
        // Initialize SignalR connection
        await this.initializeSignalR();
        
        // Setup toast container
        this.setupToastContainer();
    }

    async getCurrentUserId() {
        // Try to get from page data first
        const userIdElement = document.querySelector('[data-user-id]');
        if (userIdElement) {
            return userIdElement.getAttribute('data-user-id');
        }
        
        // Fallback to API call
        try {
            const response = await fetch('/api/user/current-id');
            if (response.ok) {
                const data = await response.json();
                return data.userId;
            }
        } catch (error) {
            console.error('Failed to get current user ID:', error);
        }
        
        return null;
    }

    async initializeSignalR() {
        try {
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl('/achievementHub')
                .withAutomaticReconnect()
                .configureLogging(signalR.LogLevel.Information)
                .build();

            // Set up the achievement notification handler
            this.connection.on('AchievementNotification', (notification) => {
                this.showAchievementToast(notification);
            });

            // Start the connection
            await this.connection.start();
            console.log('Achievement Hub connected');

            // Join user group
            if (this.userId) {
                await this.connection.invoke('JoinUserGroup', this.userId);
            }

            // Join project groups for current projects
            await this.joinProjectGroups();

        } catch (error) {
            console.error('Failed to initialize SignalR:', error);
        }
    }

    async joinProjectGroups() {
        // Get current project IDs from page data
        const projectElements = document.querySelectorAll('[data-project-id]');
        const projectIds = Array.from(projectElements).map(el => el.getAttribute('data-project-id'));
        
        for (const projectId of projectIds) {
            if (projectId && this.connection && this.connection.state === signalR.HubConnectionState.Connected) {
                try {
                    await this.connection.invoke('JoinProjectGroup', parseInt(projectId));
                } catch (error) {
                    console.error(`Failed to join project group ${projectId}:`, error);
                }
            }
        }
    }

    setupToastContainer() {
        // Create toast container if it doesn't exist
        let container = document.getElementById('achievement-toast-container');
        if (!container) {
            container = document.createElement('div');
            container.id = 'achievement-toast-container';
            container.className = 'achievement-toast-container';
            document.body.appendChild(container);
        }
    }

    showAchievementToast(notification) {
        const toast = this.createToastElement(notification);
        const container = document.getElementById('achievement-toast-container');
        
        container.appendChild(toast);
        
        // Trigger animation
        setTimeout(() => {
            toast.classList.add('show');
        }, 100);

        // Auto remove after 5 seconds
        setTimeout(() => {
            toast.classList.remove('show');
            setTimeout(() => {
                if (toast.parentNode) {
                    toast.parentNode.removeChild(toast);
                }
            }, 300);
        }, 5000);

        // Add click to dismiss
        toast.addEventListener('click', () => {
            toast.classList.remove('show');
            setTimeout(() => {
                if (toast.parentNode) {
                    toast.parentNode.removeChild(toast);
                }
            }, 300);
        });
    }

    createToastElement(notification) {
        const toast = document.createElement('div');
        toast.className = `achievement-toast achievement-toast-${notification.Type.toLowerCase()}`;
        
        let icon = '';
        let titleClass = '';
        
        switch (notification.Type) {
            case 'BadgeEarned':
                icon = '🏆';
                titleClass = 'achievement-toast-badge';
                break;
            case 'LevelUp':
                icon = '⬆️';
                titleClass = 'achievement-toast-level';
                break;
            case 'StreakAchieved':
                icon = '🔥';
                titleClass = 'achievement-toast-streak';
                break;
            case 'XPAwarded':
                icon = '✨';
                titleClass = 'achievement-toast-xp';
                break;
            case 'LeaderboardUpdate':
                icon = '📊';
                titleClass = 'achievement-toast-leaderboard';
                break;
            default:
                icon = '🎉';
                titleClass = 'achievement-toast-default';
        }

        toast.innerHTML = `
            <div class="achievement-toast-icon">${icon}</div>
            <div class="achievement-toast-content">
                <div class="achievement-toast-title ${titleClass}">${notification.Title}</div>
                <div class="achievement-toast-message">${notification.Message}</div>
                ${notification.Data ? this.getAdditionalInfo(notification) : ''}
            </div>
            <div class="achievement-toast-close">×</div>
        `;

        // Add close button functionality
        const closeBtn = toast.querySelector('.achievement-toast-close');
        if (closeBtn) {
            closeBtn.addEventListener('click', (e) => {
                e.stopPropagation();
                toast.classList.remove('show');
                setTimeout(() => {
                    if (toast.parentNode) {
                        toast.parentNode.removeChild(toast);
                    }
                }, 300);
            });
        }

        return toast;
    }

    getAdditionalInfo(notification) {
        let info = '';
        
        if (notification.Type === 'BadgeEarned' && notification.Data.badge) {
            const badge = notification.Data.badge;
            info = `
                <div class="achievement-toast-badge-info">
                    <span class="achievement-toast-badge-icon">${badge.Icon || '🏅'}</span>
                    <span class="achievement-toast-badge-name">${badge.Name}</span>
                </div>
            `;
        } else if (notification.Type === 'LevelUp' && notification.Data.newLevel) {
            info = `
                <div class="achievement-toast-level-info">
                    <span class="achievement-toast-level-number">Level ${notification.Data.newLevel}</span>
                    <span class="achievement-toast-level-title">${notification.Data.levelTitle}</span>
                </div>
            `;
        } else if (notification.Type === 'StreakAchieved' && notification.Data.streakDays) {
            info = `
                <div class="achievement-toast-streak-info">
                    <span class="achievement-toast-streak-days">${notification.Data.streakDays} days</span>
                </div>
            `;
        } else if (notification.Type === 'XPAwarded' && notification.Data.xpAmount) {
            info = `
                <div class="achievement-toast-xp-info">
                    <span class="achievement-toast-xp-amount">+${notification.Data.xpAmount} XP</span>
                </div>
            `;
        }
        
        return info;
    }

    // Public method to manually show a toast (for testing)
    showManualToast(type, title, message, data = null) {
        const notification = {
            Type: type,
            Title: title,
            Message: message,
            Data: data,
            Timestamp: new Date()
        };
        this.showAchievementToast(notification);
    }
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    window.achievementToast = new AchievementToast();
});

// Export for use in other scripts
if (typeof module !== 'undefined' && module.exports) {
    module.exports = AchievementToast;
}

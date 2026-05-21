// Mobile-specific JavaScript for AgileTaskManager
class MobileApp {
    constructor() {
        this.isOnline = navigator.onLine;
        this.isPWA = 'serviceWorker' in navigator;
        this.init();
    }

    init() {
        this.setupOfflineSupport();
        this.setupPushNotifications();
        this.setupTouchGestures();
        this.setupMobileNavigation();
        this.setupPullToRefresh();
        this.setupMobileSpecificAnalytics();
        this.optimizeForMobile();
    }

    setupOfflineSupport() {
        // Service Worker registration for offline support
        if ('serviceWorker' in navigator) {
            navigator.serviceWorker.register('/sw.js')
                .then(registration => {
                    console.log('Service Worker registered:', registration);
                })
                .catch(error => {
                    console.error('Service Worker registration failed:', error);
                });
        }

        // Online/Offline status handling
        window.addEventListener('online', () => {
            this.isOnline = true;
            this.showNotification('You are back online', 'success');
            this.syncOfflineData();
        });

        window.addEventListener('offline', () => {
            this.isOnline = false;
            this.showNotification('You are offline', 'warning');
        });
    }

    setupPushNotifications() {
        if ('Notification' in window && 'PushManager' in window) {
            // Request notification permission
            Notification.requestPermission().then(permission => {
                if (permission === 'granted') {
                    this.setupPushSubscription();
                }
            });
        }
    }

    setupPushSubscription() {
        navigator.serviceWorker.ready.then(registration => {
            return registration.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: 'YOUR_VAPID_PUBLIC_KEY'
            });
        }).then(subscription => {
            // Send subscription to server
            this.sendPushSubscriptionToServer(subscription);
        });
    }

    setupTouchGestures() {
        // Touch gesture handling
        let touchStartX = 0;
        let touchStartY = 0;
        let touchEndX = 0;
        let touchEndY = 0;

        document.addEventListener('touchstart', (e) => {
            touchStartX = e.touches[0].clientX;
            touchStartY = e.touches[0].clientY;
        });

        document.addEventListener('touchend', (e) => {
            touchEndX = e.changedTouches[0].clientX;
            touchEndY = e.changedTouches[0].clientY;

            const deltaX = touchEndX - touchStartX;
            const deltaY = touchEndY - touchStartY;

            // Handle swipe gestures
            if (Math.abs(deltaX) > Math.abs(deltaY)) {
                if (deltaX > 50) {
                    this.handleSwipeRight();
                } else if (deltaX < -50) {
                    this.handleSwipeLeft();
                }
            } else {
                if (deltaY > 50) {
                    this.handleSwipeDown();
                } else if (deltaY < -50) {
                    this.handleSwipeUp();
                }
            }
        });

        // Prevent default touch behaviors
        document.addEventListener('touchmove', (e) => {
            e.preventDefault();
        }, { passive: false });
    }

    handleSwipeRight() {
        // Handle right swipe (e.g., navigate to next)
        console.log('Swipe right detected');
        this.triggerCustomEvent('swipeRight');
    }

    handleSwipeLeft() {
        // Handle left swipe (e.g., navigate to previous)
        console.log('Swipe left detected');
        this.triggerCustomEvent('swipeLeft');
    }

    handleSwipeUp() {
        // Handle up swipe (e.g., refresh or go up)
        console.log('Swipe up detected');
        this.triggerCustomEvent('swipeUp');
    }

    handleSwipeDown() {
        // Handle down swipe (e.g., open menu or go down)
        console.log('Swipe down detected');
        this.triggerCustomEvent('swipeDown');
    }

    setupMobileNavigation() {
        // Mobile-specific navigation enhancements
        this.setupHamburgerMenu();
        this.setupMobileTabs();
        this.setupMobileSearch();
    }

    setupHamburgerMenu() {
        const hamburger = document.querySelector('.mobile-menu-toggle');
        const nav = document.querySelector('.mobile-nav');

        if (hamburger && nav) {
            hamburger.addEventListener('click', () => {
                nav.classList.toggle('active');
                hamburger.classList.toggle('active');
            });
        }
    }

    setupMobileTabs() {
        const tabs = document.querySelectorAll('.mobile-tab');
        const tabContents = document.querySelectorAll('.mobile-tab-content');

        tabs.forEach((tab, index) => {
            tab.addEventListener('click', () => {
                // Remove active class from all tabs
                tabs.forEach(t => t.classList.remove('active'));
                tabContents.forEach(content => content.classList.remove('active'));

                // Add active class to clicked tab
                tab.classList.add('active');
                if (tabContents[index]) {
                    tabContents[index].classList.add('active');
                }
            });
        });
    }

    setupMobileSearch() {
        const searchInput = document.querySelector('.mobile-search-input');
        const searchResults = document.querySelector('.mobile-search-results');

        if (searchInput && searchResults) {
            let searchTimeout;

            searchInput.addEventListener('input', (e) => {
                clearTimeout(searchTimeout);
                searchTimeout = setTimeout(() => {
                    this.performMobileSearch(e.target.value);
                }, 300); // Debounce search
            });
        }
    }

    performMobileSearch(query) {
        // Mobile search implementation
        const searchResults = document.querySelector('.mobile-search-results');
        
        if (query.length < 2) {
            searchResults.innerHTML = '';
            return;
        }

        // Simulate search results (replace with actual API call)
        const results = [
            { id: 1, title: 'Task Management', type: 'task' },
            { id: 2, title: 'Project Dashboard', type: 'project' },
            { id: 3, title: 'Analytics', type: 'analytics' }
        ];

        const filteredResults = results.filter(result => 
            result.title.toLowerCase().includes(query.toLowerCase())
        );

        this.displayMobileSearchResults(filteredResults);
    }

    displayMobileSearchResults(results) {
        const searchResults = document.querySelector('.mobile-search-results');
        
        if (results.length === 0) {
            searchResults.innerHTML = '<div class="no-results">No results found</div>';
            return;
        }

        const resultsHtml = results.map(result => `
            <div class="search-result-item" data-type="${result.type}" data-id="${result.id}">
                <div class="search-result-icon">
                    <i class="fas fa-${this.getIconForType(result.type)}"></i>
                </div>
                <div class="search-result-title">${result.title}</div>
            </div>
        `).join('');

        searchResults.innerHTML = resultsHtml;
    }

    getIconForType(type) {
        const icons = {
            task: 'tasks',
            project: 'project-diagram',
            analytics: 'chart-line'
        };
        return icons[type] || 'file';
    }

    setupPullToRefresh() {
        let isPulling = false;
        let startY = 0;
        let currentY = 0;

        const container = document.querySelector('.mobile-content');
        
        if (container) {
            container.addEventListener('touchstart', (e) => {
                isPulling = true;
                startY = e.touches[0].clientY;
                currentY = startY;
            });

            container.addEventListener('touchmove', (e) => {
                if (isPulling) {
                    currentY = e.touches[0].clientY;
                    const distance = currentY - startY;
                    
                    if (distance > 0) {
                        const pullIndicator = document.querySelector('.mobile-pull-indicator');
                        if (pullIndicator) {
                            const opacity = Math.min(1, distance / 100);
                            pullIndicator.style.opacity = opacity;
                        }
                    }
                }
            });

            container.addEventListener('touchend', () => {
                if (isPulling) {
                    const distance = currentY - startY;
                    if (distance > 80) {
                        this.triggerRefresh();
                    }
                    this.resetPullIndicator();
                }
                isPulling = false;
            });
        }
    }

    triggerRefresh() {
        const pullIndicator = document.querySelector('.mobile-pull-indicator');
        if (pullIndicator) {
            pullIndicator.classList.add('visible');
            pullIndicator.innerHTML = '<i class="fas fa-sync fa-spin"></i>';
        }

        // Trigger refresh
        this.showNotification('Refreshing...', 'info');
        setTimeout(() => {
            window.location.reload();
        }, 1000);
    }

    resetPullIndicator() {
        const pullIndicator = document.querySelector('.mobile-pull-indicator');
        if (pullIndicator) {
            pullIndicator.classList.remove('visible');
            pullIndicator.innerHTML = '';
            pullIndicator.style.opacity = '0';
        }
    }

    setupMobileSpecificAnalytics() {
        // Mobile-specific analytics tracking
        this.trackMobileUsage();
        this.trackDevicePerformance();
        this.trackUserEngagement();
    }

    trackMobileUsage() {
        // Track mobile-specific usage patterns
        const usageData = {
            deviceType: this.getDeviceType(),
            screenResolution: `${screen.width}x${screen.height}`,
            userAgent: navigator.userAgent,
            connectionType: navigator.connection ? navigator.connection.effectiveType : 'unknown',
            timestamp: new Date().toISOString()
        };

        // Send to analytics service
        this.sendAnalyticsData('mobile_usage', usageData);
    }

    trackDevicePerformance() {
        // Monitor device performance metrics
        const performanceMetrics = {
            memoryUsage: this.getMemoryUsage(),
            cpuUsage: this.getCPUUsage(),
            batteryLevel: this.getBatteryLevel(),
            networkSpeed: this.getNetworkSpeed(),
            timestamp: new Date().toISOString()
        };

        this.sendAnalyticsData('device_performance', performanceMetrics);
    }

    getDeviceType() {
        const width = window.innerWidth;
        if (width <= 480) return 'mobile-phone';
        if (width <= 768) return 'mobile-tablet';
        if (width <= 1024) return 'mobile-desktop';
        return 'mobile-large';
    }

    getMemoryUsage() {
        if (performance.memory) {
            return {
                used: performance.memory.usedJSHeapSize,
                total: performance.memory.totalJSHeapSize,
                limit: performance.memory.jsHeapSizeLimit
            };
        }
        return null;
    }

    getCPUUsage() {
        // CPU usage would require additional APIs
        return { usage: 0, cores: navigator.hardwareConcurrency || 1 };
    }

    getBatteryLevel() {
        if ('getBattery' in navigator) {
            return navigator.getBattery().then(battery => ({
                level: battery.level,
                charging: battery.charging
            }));
        }
        return Promise.resolve({ level: 1, charging: false });
    }

    getNetworkSpeed() {
        if (navigator.connection) {
            return {
                downlink: navigator.connection.downlink,
                effectiveType: navigator.connection.effectiveType,
                rtt: navigator.connection.rtt
            };
        }
        return { downlink: 0, effectiveType: 'unknown', rtt: 0 };
    }

    optimizeForMobile() {
        // Mobile performance optimizations
        this.optimizeImages();
        this.optimizeAnimations();
        this.optimizeScrolling();
        this.optimizeForms();
    }

    optimizeImages() {
        // Lazy loading for images
        const images = document.querySelectorAll('img[data-src]');
        
        if ('IntersectionObserver' in window) {
            const imageObserver = new IntersectionObserver((entries, observer) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        const img = entry.target;
                        img.src = img.dataset.src;
                        img.removeAttribute('data-src');
                        observer.unobserve(img);
                    }
                });
            });

            images.forEach(img => imageObserver.observe(img));
        }
    }

    optimizeAnimations() {
        // Reduce animations on mobile for better performance
        const mediaQuery = window.matchMedia('(prefers-reduced-motion: reduce)');
        
        if (mediaQuery.matches || this.isLowEndDevice()) {
            document.documentElement.classList.add('reduce-motion');
        }
    }

    optimizeScrolling() {
        // Smooth scrolling for mobile
        const scrollContainer = document.querySelector('.mobile-content');
        
        if (scrollContainer) {
            scrollContainer.style.scrollBehavior = 'smooth';
            scrollContainer.style.webkitOverflowScrolling = 'touch';
        }
    }

    optimizeForms() {
        // Mobile form optimizations
        const forms = document.querySelectorAll('form');
        
        forms.forEach(form => {
            // Add mobile-specific form enhancements
            form.addEventListener('submit', (e) => {
                this.handleMobileFormSubmit(e);
            });
        });
    }

    handleMobileFormSubmit(event) {
        // Mobile form submission handling
        const submitButton = event.target.querySelector('button[type="submit"]');
        
        if (submitButton) {
            submitButton.disabled = true;
            submitButton.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Processing...';
            
            setTimeout(() => {
                submitButton.disabled = false;
                submitButton.innerHTML = 'Submit';
            }, 2000);
        }
    }

    sendAnalyticsData(event, data) {
        // Send analytics data to server
        fetch('/api/analytics/mobile', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: JSON.stringify({
                event: event,
                data: data,
                timestamp: new Date().toISOString()
            })
        }).catch(error => {
            console.error('Analytics tracking failed:', error);
        });
    }

    sendPushSubscriptionToServer(subscription) {
        // Send push subscription to server
        fetch('/api/push/subscribe', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(subscription)
        }).catch(error => {
            console.error('Push subscription failed:', error);
        });
    }

    showNotification(message, type = 'info') {
        if ('Notification' in window && Notification.permission === 'granted') {
            const notification = new Notification(message, {
                icon: '/favicon.ico',
                badge: '/favicon.ico',
                tag: 'mobile-notification'
            });

            notification.onclick = () => {
                window.focus();
                notification.close();
            };

            setTimeout(() => {
                notification.close();
            }, 5000);
        }
    }

    triggerCustomEvent(eventName) {
        const event = new CustomEvent(eventName, {
            bubbles: true,
            cancelable: true
        });
        document.dispatchEvent(event);
    }

    isLowEndDevice() {
        // Detect low-end devices
        const connection = navigator.connection || navigator.mozConnection || navigator.webkitConnection;
        return connection && connection.effectiveType === 'slow-2g' || connection.effectiveType === '2g';
    }

    syncOfflineData() {
        // Sync data when coming back online
        const offlineData = localStorage.getItem('offlineData');
        
        if (offlineData) {
            try {
                const data = JSON.parse(offlineData);
                
                // Send offline data to server
                fetch('/api/sync/offline', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(data)
                }).then(response => {
                    if (response.ok) {
                        localStorage.removeItem('offlineData');
                        this.showNotification('Data synced successfully', 'success');
                    }
                });
            } catch (error) {
                console.error('Failed to sync offline data:', error);
            }
        }
    }

    // Public API for external use
    showMobileMenu() {
        const nav = document.querySelector('.mobile-nav');
        const toggle = document.querySelector('.mobile-menu-toggle');
        
        if (nav && toggle) {
            nav.classList.add('active');
            toggle.classList.add('active');
        }
    }

    hideMobileMenu() {
        const nav = document.querySelector('.mobile-nav');
        const toggle = document.querySelector('.mobile-menu-toggle');
        
        if (nav && toggle) {
            nav.classList.remove('active');
            toggle.classList.remove('active');
        }
    }

    getCurrentDevice() {
        return {
            type: this.getDeviceType(),
            isMobile: this.getDeviceType() !== 'mobile-desktop' && this.getDeviceType() !== 'mobile-large',
            isTablet: this.getDeviceType() === 'mobile-tablet',
            isPhone: this.getDeviceType() === 'mobile-phone',
            screen: {
                width: screen.width,
                height: screen.height,
                orientation: screen.orientation.type
            },
            capabilities: {
                touch: 'ontouchstart' in window,
                geolocation: 'geolocation' in navigator,
                camera: 'mediaDevices' in navigator,
                notification: 'Notification' in window
            }
        };
    }
}

// Initialize mobile app when DOM is ready
document.addEventListener('DOMContentLoaded', () => {
    window.mobileApp = new MobileApp();
});

// Service Worker for AgileTaskManager PWA
const CACHE_NAME = 'agile-task-manager-v1';
const STATIC_CACHE = 'static-cache-v1';
const DYNAMIC_CACHE = 'dynamic-cache-v1';

// Files to cache on install
const STATIC_FILES = [
    '/',
    '/css/mobile.css',
    '/css/site.css',
    '/js/mobile.js',
    '/js/site.js',
    '/manifest.json',
    '/icons/icon-192x192.png',
    '/icons/icon-512x512.png'
];

// Install event - cache static files
self.addEventListener('install', (event) => {
    console.log('Service Worker installing...');
    
    event.waitUntil(
        caches.open(STATIC_CACHE)
            .then((cache) => {
                console.log('Caching static files');
                return cache.addAll(STATIC_FILES);
            })
            .then(() => {
                console.log('Service Worker installed successfully');
                self.skipWaiting();
            })
    );
});

// Activate event - clean up old caches
self.addEventListener('activate', (event) => {
    console.log('Service Worker activating...');
    
    event.waitUntil(
        caches.keys()
            .then((cacheNames) => {
                return Promise.all(
                    cacheNames.map((cacheName) => {
                        if (cacheName !== STATIC_CACHE && cacheName !== DYNAMIC_CACHE) {
                            console.log('Deleting old cache:', cacheName);
                            return caches.delete(cacheName);
                        }
                    })
                );
            })
            .then(() => {
                console.log('Service Worker activated successfully');
                return self.clients.claim();
            })
    );
});

// Fetch event - serve cached content when offline
self.addEventListener('fetch', (event) => {
    const request = event.request;
    const url = new URL(request.url);
    
    // Skip non-GET requests and external requests
    if (request.method !== 'GET' || url.origin !== location.origin) {
        return;
    }
    
    // Handle API requests differently
    if (url.pathname.startsWith('/api/')) {
        event.respondWith(handleApiRequest(request));
        return;
    }
    
    // Handle static files
    event.respondWith(
        caches.match(request)
            .then((response) => {
                if (response) {
                    // Serve from cache
                    return response;
                }
                
                // Try network, then cache
                return fetch(request)
                    .then((response) => {
                        // Cache successful responses
                        if (response.ok) {
                            const responseClone = response.clone();
                            caches.open(DYNAMIC_CACHE)
                                .then((cache) => {
                                    cache.put(request, responseClone);
                                });
                        }
                        return response;
                    })
                    .catch(() => {
                        // Serve offline page for failed requests
                        return caches.match('/offline.html');
                    });
            })
    );
});

// Handle API requests with offline support
function handleApiRequest(request) {
    return caches.match(request)
        .then((cachedResponse) => {
            if (cachedResponse) {
                return cachedResponse;
            }
            
            // Try network first for API requests
            return fetch(request)
                .then((response) => {
                    if (response.ok) {
                        const responseClone = response.clone();
                        caches.open(DYNAMIC_CACHE)
                            .then((cache) => {
                                cache.put(request, responseClone);
                            });
                    }
                    return response;
                })
                .catch(() => {
                    // Return offline response for API requests
                    return new Response(
                        JSON.stringify({
                            error: 'Offline',
                            message: 'No internet connection',
                            timestamp: new Date().toISOString()
                        }),
                        {
                            status: 503,
                            statusText: 'Service Unavailable',
                            headers: {
                                'Content-Type': 'application/json'
                            }
                        }
                    );
                });
        });
}

// Background sync for offline actions
self.addEventListener('sync', (event) => {
    console.log('Background sync triggered:', event.tag);
    
    if (event.tag === 'sync-tasks') {
        event.waitUntil(syncTasks());
    } else if (event.tag === 'sync-notifications') {
        event.waitUntil(syncNotifications());
    }
});

// Push notification handler
self.addEventListener('push', (event) => {
    console.log('Push notification received:', event);
    
    const options = {
        body: event.data ? event.data.text() : 'New notification',
        icon: '/icons/icon-192x192.png',
        badge: '/icons/icon-72x72.png',
        vibrate: [200, 100, 200],
        data: event.data ? event.data.json() : {},
        actions: [
            {
                action: 'view',
                title: 'View'
            },
            {
                action: 'dismiss',
                title: 'Dismiss'
            }
        ]
    };
    
    event.waitUntil(
        self.registration.showNotification('AgileTaskManager', options)
    );
});

// Notification click handler
self.addEventListener('notificationclick', (event) => {
    console.log('Notification clicked:', event);
    
    event.notification.close();
    
    if (event.action === 'view') {
        event.waitUntil(
            clients.openWindow('/')
        );
    } else if (event.action === 'dismiss') {
        // Just close the notification
    } else {
        // Default action - open the app
        event.waitUntil(
            clients.openWindow('/')
        );
    }
});

// Sync tasks when online
async function syncTasks() {
    try {
        const offlineTasks = await getOfflineTasks();
        
        if (offlineTasks.length > 0) {
            for (const task of offlineTasks) {
                try {
                    await fetch('/api/mobile/sync', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify(task)
                    });
                    
                    // Remove from offline storage after successful sync
                    await removeOfflineTask(task.id);
                } catch (error) {
                    console.error('Failed to sync task:', task, error);
                }
            }
        }
    } catch (error) {
        console.error('Sync failed:', error);
    }
}

// Sync notifications when online
async function syncNotifications() {
    try {
        const offlineNotifications = await getOfflineNotifications();
        
        if (offlineNotifications.length > 0) {
            for (const notification of offlineNotifications) {
                try {
                    await fetch('/api/mobile/notifications/sync', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify(notification)
                    });
                    
                    await removeOfflineNotification(notification.id);
                } catch (error) {
                    console.error('Failed to sync notification:', notification, error);
                }
            }
        }
    } catch (error) {
        console.error('Notification sync failed:', error);
    }
}

// Helper functions for offline storage
async function getOfflineTasks() {
    const offlineTasks = localStorage.getItem('offlineTasks');
    return offlineTasks ? JSON.parse(offlineTasks) : [];
}

async function removeOfflineTask(taskId) {
    const offlineTasks = await getOfflineTasks();
    const updatedTasks = offlineTasks.filter(task => task.id !== taskId);
    localStorage.setItem('offlineTasks', JSON.stringify(updatedTasks));
}

async function getOfflineNotifications() {
    const offlineNotifications = localStorage.getItem('offlineNotifications');
    return offlineNotifications ? JSON.parse(offlineNotifications) : [];
}

async function removeOfflineNotification(notificationId) {
    const offlineNotifications = await getOfflineNotifications();
    const updatedNotifications = offlineNotifications.filter(notification => notification.id !== notificationId);
    localStorage.setItem('offlineNotifications', JSON.stringify(updatedNotifications));
}

// Periodic cache cleanup
self.addEventListener('message', (event) => {
    if (event.data && event.data.type === 'CACHE_UPDATED') {
        // Clear dynamic cache when app updates
        caches.delete(DYNAMIC_CACHE);
    }
});

// Performance monitoring
self.addEventListener('fetch', (event) => {
    const start = performance.now();
    
    event.waitUntil(
        fetch(event.request).then(response => {
            const end = performance.now();
            const duration = end - start;
            
            // Log slow requests
            if (duration > 1000) {
                console.log('Slow request detected:', {
                    url: event.request.url,
                    duration: duration,
                    status: response.status
                });
            }
            
            return response;
        })
    );
});

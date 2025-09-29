import React, { useState, useRef, useEffect } from 'react';
import { Bell, Check, X, Circle } from 'lucide-react';
import { useNotifications } from '../lib/notificationContext';
import { formatDistanceToNow } from 'date-fns';

export const NotificationBell: React.FC = () => {
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const {
    notifications,
    unreadCount,
    isSignalRConnected,
    markAsRead,
    markAllAsRead,
    removeNotification,
    clearAll
  } = useNotifications();

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const getPriorityColor = (priority?: string) => {
    switch (priority) {
      case 'urgent': return 'text-red-600 bg-red-50';
      case 'high': return 'text-orange-600 bg-orange-50';
      case 'medium': return 'text-blue-600 bg-blue-50';
      case 'low': return 'text-gray-600 bg-gray-50';
      default: return 'text-gray-600 bg-gray-50';
    }
  };

  const getTypeIcon = (type: string) => {
    switch (type) {
      case 'incident-created': return '🚨';
      case 'incident-status-changed': return '🔄';
      case 'resource-assigned': return '🚛';
      default: return '📢';
    }
  };

  return (
    <div className="relative" ref={dropdownRef}>
      {/* Bell Icon */}
      <button
        onClick={() => setIsOpen(!isOpen)}
        className={`relative p-2 rounded-full transition-colors ${
          isSignalRConnected
            ? 'text-gray-600 hover:text-gray-900 hover:bg-gray-100'
            : 'text-red-500 hover:text-red-700'
        }`}
        title={isSignalRConnected ? "Notifications" : "Disconnected from real-time updates"}
      >
        <Bell size={20} />

        {/* Connection Status Indicator */}
        <div className={`absolute -top-1 -left-1 w-3 h-3 rounded-full ${
          isSignalRConnected ? 'bg-green-500' : 'bg-red-500'
        }`} />

        {/* Unread Count Badge */}
        {unreadCount > 0 && (
          <div className="absolute -top-1 -right-1 bg-red-500 text-white text-xs rounded-full min-w-[20px] h-5 flex items-center justify-center">
            {unreadCount > 99 ? '99+' : unreadCount}
          </div>
        )}
      </button>

      {/* Dropdown Menu */}
      {isOpen && (
        <div className="absolute right-0 mt-2 w-80 bg-white rounded-lg shadow-lg ring-1 ring-black ring-opacity-5 z-50">
          {/* Header */}
          <div className="px-4 py-3 border-b border-gray-200">
            <div className="flex items-center justify-between">
              <h3 className="text-lg font-medium text-gray-900">Notifications</h3>
              <div className="flex items-center space-x-2">
                <div className={`flex items-center space-x-1 text-xs ${
                  isSignalRConnected ? 'text-green-600' : 'text-red-600'
                }`}>
                  <Circle size={8} className={isSignalRConnected ? 'fill-green-500' : 'fill-red-500'} />
                  <span>{isSignalRConnected ? 'Connected' : 'Disconnected'}</span>
                </div>
                {unreadCount > 0 && (
                  <button
                    onClick={markAllAsRead}
                    className="text-xs text-blue-600 hover:text-blue-800"
                  >
                    Mark all read
                  </button>
                )}
              </div>
            </div>
          </div>

          {/* Notifications List */}
          <div className="max-h-96 overflow-y-auto">
            {notifications.length === 0 ? (
              <div className="px-4 py-8 text-center text-gray-500">
                <Bell size={48} className="mx-auto mb-2 text-gray-300" />
                <p>No notifications yet</p>
              </div>
            ) : (
              <div className="divide-y divide-gray-100">
                {notifications.slice(0, 20).map((notification) => (
                  <div
                    key={notification.id}
                    className={`px-4 py-3 hover:bg-gray-50 transition-colors ${
                      !notification.isRead ? 'bg-blue-50' : ''
                    }`}
                  >
                    <div className="flex items-start space-x-3">
                      {/* Type Icon */}
                      <div className="flex-shrink-0 text-lg">
                        {getTypeIcon(notification.type)}
                      </div>

                      {/* Content */}
                      <div className="flex-1 min-w-0">
                        <div className="flex items-start justify-between">
                          <div className="flex-1">
                            <p className={`text-sm font-medium ${
                              !notification.isRead ? 'text-gray-900' : 'text-gray-700'
                            }`}>
                              {notification.title}
                            </p>
                            <p className="text-sm text-gray-600 mt-1 line-clamp-2">
                              {notification.message}
                            </p>
                            <div className="flex items-center space-x-2 mt-2">
                              <span className="text-xs text-gray-400">
                                {formatDistanceToNow(notification.timestamp, { addSuffix: true })}
                              </span>
                              {notification.priority && (
                                <span className={`px-2 py-1 text-xs rounded-full ${getPriorityColor(notification.priority)}`}>
                                  {notification.priority}
                                </span>
                              )}
                            </div>
                          </div>

                          {/* Actions */}
                          <div className="flex items-center space-x-1 ml-2">
                            {!notification.isRead && (
                              <button
                                onClick={() => markAsRead(notification.id)}
                                className="p-1 text-gray-400 hover:text-green-600"
                                title="Mark as read"
                              >
                                <Check size={14} />
                              </button>
                            )}
                            <button
                              onClick={() => removeNotification(notification.id)}
                              className="p-1 text-gray-400 hover:text-red-600"
                              title="Remove"
                            >
                              <X size={14} />
                            </button>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Footer */}
          {notifications.length > 0 && (
            <div className="px-4 py-3 border-t border-gray-200">
              <button
                onClick={clearAll}
                className="w-full text-sm text-red-600 hover:text-red-800 text-center"
              >
                Clear all notifications
              </button>
            </div>
          )}
        </div>
      )}
    </div>
  );
};
import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { useSignalR, IncidentNotification, IncidentCreated, IncidentStatusChanged, ResourceAssigned } from '../hooks/useSignalR';
import { useUserStore } from './userStore';

export interface NotificationItem {
  id: string;
  type: 'incident-created' | 'incident-status-changed' | 'resource-assigned' | 'general';
  title: string;
  message: string;
  timestamp: Date;
  incidentId?: number;
  isRead: boolean;
  priority?: 'low' | 'medium' | 'high' | 'urgent';
}

interface NotificationContextType {
  notifications: NotificationItem[];
  unreadCount: number;
  isSignalRConnected: boolean;
  markAsRead: (id: string) => void;
  markAllAsRead: () => void;
  removeNotification: (id: string) => void;
  clearAll: () => void;
}

const NotificationContext = createContext<NotificationContextType | undefined>(undefined);

export const useNotifications = () => {
  const context = useContext(NotificationContext);
  if (context === undefined) {
    throw new Error('useNotifications must be used within a NotificationProvider');
  }
  return context;
};

interface NotificationProviderProps {
  children: ReactNode;
}

export const NotificationProvider: React.FC<NotificationProviderProps> = ({ children }) => {
  const [notifications, setNotifications] = useState<NotificationItem[]>([]);
  const { isConnected } = useSignalR();
  const { isAuthenticated } = useUserStore();

  const signalR = useSignalR();

  const addNotification = (notification: Omit<NotificationItem, 'id' | 'isRead'>) => {
    const newNotification: NotificationItem = {
      ...notification,
      id: `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`,
      isRead: false,
    };

    setNotifications(prev => [newNotification, ...prev.slice(0, 99)]); // Keep only latest 100 notifications

    // Show browser notification if permission granted
    if ('Notification' in window && Notification.permission === 'granted') {
      new Notification(notification.title, {
        body: notification.message,
        icon: '/favicon.ico',
        tag: `incident-${notification.incidentId}`,
      });
    }
  };

  const markAsRead = (id: string) => {
    setNotifications(prev =>
      prev.map(notification =>
        notification.id === id ? { ...notification, isRead: true } : notification
      )
    );
  };

  const markAllAsRead = () => {
    setNotifications(prev =>
      prev.map(notification => ({ ...notification, isRead: true }))
    );
  };

  const removeNotification = (id: string) => {
    setNotifications(prev => prev.filter(notification => notification.id !== id));
  };

  const clearAll = () => {
    setNotifications([]);
  };

  // Set up SignalR event handlers
  useEffect(() => {
    if (!isAuthenticated || !signalR) return;

    const cleanupNotification = signalR.addIncidentNotificationHandler((notification: IncidentNotification) => {
      if (notification && notification.title && notification.message) {
        addNotification({
          type: 'general',
          title: notification.title,
          message: notification.message,
          timestamp: new Date(notification.timestamp),
          incidentId: notification.incidentId,
          priority: 'medium',
        });
      }
    });

    const cleanupCreated = signalR.addIncidentCreatedHandler((incident: IncidentCreated) => {
      if (incident && incident.incidentData) {
        const priority = incident.incidentData.priority?.toLowerCase() === 'high' ? 'high' :
                        incident.incidentData.priority?.toLowerCase() === 'urgent' ? 'urgent' : 'medium';

        addNotification({
          type: 'incident-created',
          title: 'New Incident Assigned',
          message: `${incident.incidentData.type} incident at ${incident.incidentData.address}`,
          timestamp: new Date(incident.timestamp),
          incidentId: incident.incidentId,
          priority,
        });
      }
    });

    const cleanupStatusChanged = signalR.addIncidentStatusChangedHandler((statusChange: IncidentStatusChanged) => {
      if (statusChange && statusChange.incidentId && statusChange.newStatus) {
        addNotification({
          type: 'incident-status-changed',
          title: 'Incident Status Updated',
          message: `Incident #${statusChange.incidentId} status changed to ${statusChange.newStatus}`,
          timestamp: new Date(statusChange.timestamp),
          incidentId: statusChange.incidentId,
          priority: 'low',
        });
      }
    });

    const cleanupResourceAssigned = signalR.addResourceAssignedHandler((assignment: ResourceAssigned) => {
      if (assignment && assignment.resourceType && assignment.resourceId) {
        addNotification({
          type: 'resource-assigned',
          title: 'Resource Assignment',
          message: `${assignment.resourceType} #${assignment.resourceId} assigned to incident #${assignment.incidentId}`,
          timestamp: new Date(assignment.timestamp),
          incidentId: assignment.incidentId,
          priority: 'medium',
        });
      }
    });

    return () => {
       cleanupNotification();
       cleanupCreated();
       cleanupStatusChanged();
       cleanupResourceAssigned();
     };
   }, [isAuthenticated, signalR, addNotification]);

  // Request notification permission on mount
  useEffect(() => {
    if (isAuthenticated && 'Notification' in window && Notification.permission === 'default') {
      Notification.requestPermission();
    }
  }, [isAuthenticated]);

  const unreadCount = notifications.filter(n => !n.isRead).length;

  const value = {
    notifications,
    unreadCount,
    isSignalRConnected: isConnected,
    markAsRead,
    markAllAsRead,
    removeNotification,
    clearAll,
  };

  return (
    <NotificationContext.Provider value={value}>
      {children}
    </NotificationContext.Provider>
  );
};
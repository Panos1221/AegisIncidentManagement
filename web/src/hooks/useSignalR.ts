import { useEffect, useRef, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import { useUserStore } from '../lib/userStore';

export interface IncidentNotification {
  incidentId: number;
  title: string;
  message: string;
  timestamp: string;
}

export interface IncidentUpdate {
  incidentId: number;
  updateData: any;
  timestamp: string;
}

export interface IncidentCreated {
  incidentId: number;
  incidentData: {
    id: number;
    type: string;
    description: string;
    address: string;
    priority: string;
    status: string;
    createdAt: string;
  };
  timestamp: string;
}

export interface IncidentStatusChanged {
  incidentId: number;
  newStatus: string;
  timestamp: string;
}

export interface ResourceAssigned {
  incidentId: number;
  resourceType: string;
  resourceId: number;
  timestamp: string;
}

export interface AssignmentStatusChanged {
  incidentId: number;
  assignmentId: number;
  newStatus: string;
  oldStatus: string;
  timestamp: string;
}

export interface IncidentLogAdded {
  incidentId: number;
  message: string;
  at: string;
  by?: string;
  timestamp: string;
}

export interface VehicleAssignmentChanged {
  vehicleId: number;
  personnelId?: number;
  action: string; // "assigned" or "unassigned"
  timestamp: string;
}

export interface PersonnelStatusChanged {
  personnelId: number;
  newStatus: string;
  oldStatus: string;
  timestamp: string;
}

export interface PersonnelCreated {
  personnelData: any;
  timestamp: string;
}

export interface PersonnelUpdated {
  personnelId: number;
  updateData: any;
  timestamp: string;
}

export interface PersonnelDeleted {
  personnelId: number;
  timestamp: string;
}

export interface VehicleStatusChanged {
  vehicleId: number;
  newStatus: string;
  oldStatus: string;
  timestamp: string;
}

export interface VehicleCreated {
  vehicleData: any;
  timestamp: string;
}

export interface VehicleUpdated {
  vehicleId: number;
  updateData: any;
  timestamp: string;
}

export interface VehicleDeleted {
  vehicleId: number;
  timestamp: string;
}

export const useSignalR = () => {
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const [isConnected, setIsConnected] = useState(false);
  const [connectionState, setConnectionState] = useState<signalR.HubConnectionState>(signalR.HubConnectionState.Disconnected);
  const { isAuthenticated } = useUserStore();

  // Event handlers arrays to support multiple subscribers
  const incidentNotificationHandlers = useRef<((notification: IncidentNotification) => void)[]>([]);
  const incidentUpdateHandlers = useRef<((update: IncidentUpdate) => void)[]>([]);
  const incidentCreatedHandlers = useRef<((incident: IncidentCreated) => void)[]>([]);
  const incidentStatusChangedHandlers = useRef<((statusChange: IncidentStatusChanged) => void)[]>([]);
  const resourceAssignedHandlers = useRef<((assignment: ResourceAssigned) => void)[]>([]);
  const assignmentStatusChangedHandlers = useRef<((statusChange: AssignmentStatusChanged) => void)[]>([]);
  const incidentLogAddedHandlers = useRef<((log: IncidentLogAdded) => void)[]>([]);
  const vehicleAssignmentChangedHandlers = useRef<((change: VehicleAssignmentChanged) => void)[]>([]);
  const personnelStatusChangedHandlers = useRef<((statusChange: PersonnelStatusChanged) => void)[]>([]);
  const personnelCreatedHandlers = useRef<((personnel: PersonnelCreated) => void)[]>([]);
  const personnelUpdatedHandlers = useRef<((personnel: PersonnelUpdated) => void)[]>([]);
  const personnelDeletedHandlers = useRef<((personnel: PersonnelDeleted) => void)[]>([]);
  const vehicleStatusChangedHandlers = useRef<((statusChange: VehicleStatusChanged) => void)[]>([]);
  const vehicleCreatedHandlers = useRef<((vehicle: VehicleCreated) => void)[]>([]);
  const vehicleUpdatedHandlers = useRef<((vehicle: VehicleUpdated) => void)[]>([]);
  const vehicleDeletedHandlers = useRef<((vehicle: VehicleDeleted) => void)[]>([]);

  const connect = async () => {
    if (connectionRef.current) return;

    const token = localStorage.getItem('authToken');
    if (!token) {
      console.log('No auth token available, skipping SignalR connection');
      return;
    }

    try {
      const connection = new signalR.HubConnectionBuilder()
        .withUrl('http://localhost:5000/api/hubs/incidents', {
          accessTokenFactory: token ? () => token : undefined,
          skipNegotiation: false,
          transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents | signalR.HttpTransportType.LongPolling
        })
        .withAutomaticReconnect([0, 2000, 10000, 30000])
        .configureLogging(signalR.LogLevel.Information)
        .build();

      // Set up event handlers
      connection.on('IncidentNotification', (notification: IncidentNotification) => {
        console.log('Received incident notification:', notification);
        incidentNotificationHandlers.current.forEach(handler => handler(notification));
      });

      connection.on('IncidentUpdate', (update: IncidentUpdate) => {
        console.log('Received incident update:', update);
        incidentUpdateHandlers.current.forEach(handler => handler(update));
      });

      connection.on('IncidentCreated', (incident: IncidentCreated) => {
        console.log('Received incident created:', incident);
        incidentCreatedHandlers.current.forEach(handler => handler(incident));
      });

      connection.on('IncidentStatusChanged', (statusChange: IncidentStatusChanged) => {
        console.log('Received incident status change:', statusChange);
        incidentStatusChangedHandlers.current.forEach(handler => handler(statusChange));
      });

      connection.on('ResourceAssigned', (assignment: ResourceAssigned) => {
        console.log('Received resource assignment:', assignment);
        resourceAssignedHandlers.current.forEach(handler => handler(assignment));
      });

      connection.on('AssignmentStatusChanged', (statusChange: AssignmentStatusChanged) => {
        console.log('Received assignment status change:', statusChange);
        assignmentStatusChangedHandlers.current.forEach(handler => handler(statusChange));
      });

      connection.on('IncidentLogAdded', (log: IncidentLogAdded) => {
        console.log('Received incident log added:', log);
        incidentLogAddedHandlers.current.forEach(handler => handler(log));
      });

      connection.on('VehicleAssignmentChanged', (change: VehicleAssignmentChanged) => {
        console.log('Received vehicle assignment change:', change);
        vehicleAssignmentChangedHandlers.current.forEach(handler => handler(change));
      });

      connection.on('PersonnelStatusChanged', (statusChange: PersonnelStatusChanged) => {
        console.log('Received personnel status change:', statusChange);
        personnelStatusChangedHandlers.current.forEach(handler => handler(statusChange));
      });

      connection.on('PersonnelCreated', (personnel: PersonnelCreated) => {
        console.log('Received personnel created:', personnel);
        personnelCreatedHandlers.current.forEach(handler => handler(personnel));
      });

      connection.on('PersonnelUpdated', (personnel: PersonnelUpdated) => {
        console.log('Received personnel updated:', personnel);
        personnelUpdatedHandlers.current.forEach(handler => handler(personnel));
      });

      connection.on('PersonnelDeleted', (personnel: PersonnelDeleted) => {
        console.log('Received personnel deleted:', personnel);
        personnelDeletedHandlers.current.forEach(handler => handler(personnel));
      });

      connection.on('VehicleStatusChanged', (statusChange: VehicleStatusChanged) => {
        console.log('Received vehicle status change:', statusChange);
        vehicleStatusChangedHandlers.current.forEach(handler => handler(statusChange));
      });

      connection.on('VehicleCreated', (vehicle: VehicleCreated) => {
        console.log('Received vehicle created:', vehicle);
        vehicleCreatedHandlers.current.forEach(handler => handler(vehicle));
      });

      connection.on('VehicleUpdated', (vehicle: VehicleUpdated) => {
        console.log('Received vehicle updated:', vehicle);
        vehicleUpdatedHandlers.current.forEach(handler => handler(vehicle));
      });

      connection.on('VehicleDeleted', (vehicle: VehicleDeleted) => {
        console.log('Received vehicle deleted:', vehicle);
        vehicleDeletedHandlers.current.forEach(handler => handler(vehicle));
      });

      // Handle connection state changes
      connection.onreconnecting(() => {
        console.log('SignalR reconnecting...');
        setIsConnected(false);
        setConnectionState(signalR.HubConnectionState.Reconnecting);
      });

      connection.onreconnected(() => {
        console.log('SignalR reconnected');
        setIsConnected(true);
        setConnectionState(signalR.HubConnectionState.Connected);
      });

      connection.onclose(() => {
        console.log('SignalR connection closed');
        setIsConnected(false);
        setConnectionState(signalR.HubConnectionState.Disconnected);
      });

      await connection.start();
      connectionRef.current = connection;
      setIsConnected(true);
      setConnectionState(signalR.HubConnectionState.Connected);
      console.log('SignalR connected successfully');
    } catch (error) {
      console.error('SignalR connection failed:', error);
      setIsConnected(false);
      setConnectionState(signalR.HubConnectionState.Disconnected);

      // Don't retry immediately if authentication failed
      const errorMessage = error instanceof Error ? error.message : String(error);
      if (errorMessage?.includes('Unauthorized') || errorMessage?.includes('401')) {
        console.log('SignalR authentication failed - token may be invalid or expired');
        return;
      }
    }
  };

  const disconnect = async () => {
    if (connectionRef.current) {
      try {
        await connectionRef.current.stop();
        console.log('SignalR disconnected');
      } catch (error) {
        console.error('Error disconnecting SignalR:', error);
      } finally {
        connectionRef.current = null;
        setIsConnected(false);
        setConnectionState(signalR.HubConnectionState.Disconnected);
      }
    }
  };

  // Auto connect when authenticated
  useEffect(() => {
    if (isAuthenticated) {
      connect();
    } else {
      disconnect();
    }

    return () => {
      disconnect();
    };
  }, [isAuthenticated]);

  // Functions to add event handlers
  const addIncidentNotificationHandler = (handler: (notification: IncidentNotification) => void) => {
    incidentNotificationHandlers.current.push(handler);
    return () => {
      const index = incidentNotificationHandlers.current.indexOf(handler);
      if (index > -1) incidentNotificationHandlers.current.splice(index, 1);
    };
  };

  const addIncidentUpdateHandler = (handler: (update: IncidentUpdate) => void) => {
    incidentUpdateHandlers.current.push(handler);
    return () => {
      const index = incidentUpdateHandlers.current.indexOf(handler);
      if (index > -1) incidentUpdateHandlers.current.splice(index, 1);
    };
  };

  const addIncidentCreatedHandler = (handler: (incident: IncidentCreated) => void) => {
    incidentCreatedHandlers.current.push(handler);
    return () => {
      const index = incidentCreatedHandlers.current.indexOf(handler);
      if (index > -1) incidentCreatedHandlers.current.splice(index, 1);
    };
  };

  const addIncidentStatusChangedHandler = (handler: (statusChange: IncidentStatusChanged) => void) => {
    incidentStatusChangedHandlers.current.push(handler);
    return () => {
      const index = incidentStatusChangedHandlers.current.indexOf(handler);
      if (index > -1) incidentStatusChangedHandlers.current.splice(index, 1);
    };
  };

  const addResourceAssignedHandler = (handler: (assignment: ResourceAssigned) => void) => {
    resourceAssignedHandlers.current.push(handler);
    return () => {
      const index = resourceAssignedHandlers.current.indexOf(handler);
      if (index > -1) resourceAssignedHandlers.current.splice(index, 1);
    };
  };

  const addAssignmentStatusChangedHandler = (handler: (statusChange: AssignmentStatusChanged) => void) => {
    assignmentStatusChangedHandlers.current.push(handler);
    return () => {
      const index = assignmentStatusChangedHandlers.current.indexOf(handler);
      if (index > -1) assignmentStatusChangedHandlers.current.splice(index, 1);
    };
  };

  const addIncidentLogAddedHandler = (handler: (log: IncidentLogAdded) => void) => {
    incidentLogAddedHandlers.current.push(handler);
    return () => {
      const index = incidentLogAddedHandlers.current.indexOf(handler);
      if (index > -1) incidentLogAddedHandlers.current.splice(index, 1);
    };
  };

  const addVehicleAssignmentChangedHandler = (handler: (change: VehicleAssignmentChanged) => void) => {
    vehicleAssignmentChangedHandlers.current.push(handler);
    return () => {
      const index = vehicleAssignmentChangedHandlers.current.indexOf(handler);
      if (index > -1) vehicleAssignmentChangedHandlers.current.splice(index, 1);
    };
  };

  const addPersonnelStatusChangedHandler = (handler: (statusChange: PersonnelStatusChanged) => void) => {
    personnelStatusChangedHandlers.current.push(handler);
    return () => {
      const index = personnelStatusChangedHandlers.current.indexOf(handler);
      if (index > -1) personnelStatusChangedHandlers.current.splice(index, 1);
    };
  };

  const addPersonnelCreatedHandler = (handler: (personnel: PersonnelCreated) => void) => {
    personnelCreatedHandlers.current.push(handler);
    return () => {
      const index = personnelCreatedHandlers.current.indexOf(handler);
      if (index > -1) personnelCreatedHandlers.current.splice(index, 1);
    };
  };

  const addPersonnelUpdatedHandler = (handler: (personnel: PersonnelUpdated) => void) => {
    personnelUpdatedHandlers.current.push(handler);
    return () => {
      const index = personnelUpdatedHandlers.current.indexOf(handler);
      if (index > -1) personnelUpdatedHandlers.current.splice(index, 1);
    };
  };

  const addPersonnelDeletedHandler = (handler: (personnel: PersonnelDeleted) => void) => {
    personnelDeletedHandlers.current.push(handler);
    return () => {
      const index = personnelDeletedHandlers.current.indexOf(handler);
      if (index > -1) personnelDeletedHandlers.current.splice(index, 1);
    };
  };

  const addVehicleStatusChangedHandler = (handler: (statusChange: VehicleStatusChanged) => void) => {
    vehicleStatusChangedHandlers.current.push(handler);
    return () => {
      const index = vehicleStatusChangedHandlers.current.indexOf(handler);
      if (index > -1) vehicleStatusChangedHandlers.current.splice(index, 1);
    };
  };

  const addVehicleCreatedHandler = (handler: (vehicle: VehicleCreated) => void) => {
    vehicleCreatedHandlers.current.push(handler);
    return () => {
      const index = vehicleCreatedHandlers.current.indexOf(handler);
      if (index > -1) vehicleCreatedHandlers.current.splice(index, 1);
    };
  };

  const addVehicleUpdatedHandler = (handler: (vehicle: VehicleUpdated) => void) => {
    vehicleUpdatedHandlers.current.push(handler);
    return () => {
      const index = vehicleUpdatedHandlers.current.indexOf(handler);
      if (index > -1) vehicleUpdatedHandlers.current.splice(index, 1);
    };
  };

  const addVehicleDeletedHandler = (handler: (vehicle: VehicleDeleted) => void) => {
    vehicleDeletedHandlers.current.push(handler);
    return () => {
      const index = vehicleDeletedHandlers.current.indexOf(handler);
      if (index > -1) vehicleDeletedHandlers.current.splice(index, 1);
    };
  };

  return {
    isConnected,
    connectionState,
    connect,
    disconnect,
    // Legacy setters for backward compatibility
    setOnIncidentNotification: (handler: ((notification: IncidentNotification) => void) | null) => {
      incidentNotificationHandlers.current = handler ? [handler] : [];
    },
    setOnIncidentUpdate: (handler: ((update: IncidentUpdate) => void) | null) => {
      incidentUpdateHandlers.current = handler ? [handler] : [];
    },
    setOnIncidentCreated: (handler: ((incident: IncidentCreated) => void) | null) => {
      incidentCreatedHandlers.current = handler ? [handler] : [];
    },
    setOnIncidentStatusChanged: (handler: ((statusChange: IncidentStatusChanged) => void) | null) => {
      incidentStatusChangedHandlers.current = handler ? [handler] : [];
    },
    setOnResourceAssigned: (handler: ((assignment: ResourceAssigned) => void) | null) => {
      resourceAssignedHandlers.current = handler ? [handler] : [];
    },
    // New add handler functions
     addIncidentNotificationHandler,
     addIncidentUpdateHandler,
     addIncidentCreatedHandler,
     addIncidentStatusChangedHandler,
     addResourceAssignedHandler,
     addAssignmentStatusChangedHandler,
     addIncidentLogAddedHandler,
     addVehicleAssignmentChangedHandler,
     addPersonnelStatusChangedHandler,
     addPersonnelCreatedHandler,
     addPersonnelUpdatedHandler,
     addPersonnelDeletedHandler,
     addVehicleStatusChangedHandler,
     addVehicleCreatedHandler,
     addVehicleUpdatedHandler,
     addVehicleDeletedHandler,
   };
};
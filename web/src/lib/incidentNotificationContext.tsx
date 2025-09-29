import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { useSignalR, IncidentCreated } from '../hooks/useSignalR';
import { useUserStore } from './userStore';
import { useAudio } from '../hooks/useAudio';

interface IncidentNotificationContextType {
  flashingIncidents: Set<number>;
  isIncidentFlashing: (incidentId: number) => boolean;
}

const IncidentNotificationContext = createContext<IncidentNotificationContextType | undefined>(undefined);

export const useIncidentNotification = () => {
  const context = useContext(IncidentNotificationContext);
  if (!context) {
    throw new Error('useIncidentNotification must be used within an IncidentNotificationProvider');
  }
  return context;
};

interface IncidentNotificationProviderProps {
  children: ReactNode;
}

export const IncidentNotificationProvider: React.FC<IncidentNotificationProviderProps> = ({ children }) => {
  const [flashingIncidents, setFlashingIncidents] = useState<Set<number>>(new Set());
  const { isAuthenticated } = useUserStore();
  const signalR = useSignalR();
  const { playAudio } = useAudio();
  const queryClient = useQueryClient();

  const addFlashingIncident = (incidentId: number) => {
    setFlashingIncidents(prev => new Set(prev).add(incidentId));
    
    // Remove flashing effect after 2 seconds
    setTimeout(() => {
      setFlashingIncidents(prev => {
        const newSet = new Set(prev);
        newSet.delete(incidentId);
        return newSet;
      });
    }, 2000);
  };

  const isIncidentFlashing = (incidentId: number): boolean => {
    return flashingIncidents.has(incidentId);
  };

  // Set up SignalR event handlers for new incidents
  useEffect(() => {
    if (!isAuthenticated || !signalR) return;

    const cleanupCreated = signalR.addIncidentCreatedHandler(async (incident: IncidentCreated) => {
      if (incident && incident.incidentData) {
        console.log('New incident received, triggering flash and audio:', incident.incidentId);

        // Add flashing effect
        addFlashingIncident(incident.incidentId);

        // Check if this is a reinforcement incident by fetching the current incidents
        // We'll check this after the queries are invalidated and fresh data is loaded
        queryClient.invalidateQueries({ queryKey: ['incidents'] });
        queryClient.invalidateQueries({ queryKey: ['dashboard-incidents'] });

        // Wait for query invalidation and then check participation type
        setTimeout(async () => {
          try {
            const incidentsData = queryClient.getQueryData(['incidents']) as any[];
            const currentIncident = incidentsData?.find(i => i.id === incident.incidentId);

            // Only play audio for primary incidents, not reinforcements
            if (!currentIncident || currentIncident.participationType !== 'Reinforcement') {
              playAudio('/src/audio/incident_alert.wav');
              setTimeout(() => {
                playAudio('/src/audio/incident_alert.wav');
              }, 2000);
            } else {
              console.log('Skipping audio for reinforcement incident:', incident.incidentId);
            }
          } catch (error) {
            console.error('Error checking incident participation type:', error);
            // Fallback: play audio if we can't determine the type
            playAudio('/src/audio/incident_alert.wav');
            setTimeout(() => {
              playAudio('/src/audio/incident_alert.wav');
            }, 2000);
          }
        }, 500); // Small delay to allow queries to update
      }
    });

    return () => {
       cleanupCreated();
     };
   }, [isAuthenticated, signalR, playAudio]);

  const value = {
    flashingIncidents,
    isIncidentFlashing
  };

  return (
    <IncidentNotificationContext.Provider value={value}>
      {children}
    </IncidentNotificationContext.Provider>
  );
};
import { Routes, Route } from 'react-router-dom'
import { ThemeProvider } from './lib/themeContext'
import { AuthProvider } from './lib/authContext'
import { NotificationProvider } from './lib/notificationContext'
import { IncidentNotificationProvider } from './lib/incidentNotificationContext'
import { ProtectedRoute, RoleProtectedRoute, ToastProvider } from './components'
import { useUserStore } from './lib/userStore'
import Layout from './components/Layout'
import Login from './pages/Login'
import Dashboard from './pages/Dashboard'
import IncidentsList from './pages/IncidentsList'
import IncidentDetail from './pages/IncidentDetail'
import NewIncident from './pages/NewIncident'
import VehiclesList from './pages/VehiclesList'
import MapView from './pages/MapView'
import Roster from './pages/Roster'
import StationManagement from './pages/StationManagement'
import WeatherForecast from './pages/WeatherForecast'
import CAD from './pages/CAD'

function App() {
  return (
    <ThemeProvider>
      <AuthProvider>
        <NotificationProvider>
          <IncidentNotificationProvider>
            <ToastProvider>
          <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/" element={
            <ProtectedRoute>
              <Layout>
                <Dashboard />
              </Layout>
            </ProtectedRoute>
          } />
          <Route path="/cad" element={
            <RoleProtectedRoute requiredPermission={() => useUserStore.getState().isFirefighter()}> {/* Firefighter, CoastGuardMember, EKABMember */}
              <Layout>
                <CAD />
              </Layout>
            </RoleProtectedRoute>
          } />
          <Route path="/dashboard" element={
            <ProtectedRoute>
              <Layout>
                <Dashboard />
              </Layout>
            </ProtectedRoute>
          } />
          <Route path="/incidents" element={
            <ProtectedRoute>
              <Layout>
                <IncidentsList />
              </Layout>
            </ProtectedRoute>
          } />
          <Route path="/incidents/new" element={
            <ProtectedRoute>
              <Layout>
                <NewIncident />
              </Layout>
            </ProtectedRoute>
          } />
          <Route path="/incidents/:id" element={
            <ProtectedRoute>
              <Layout>
                <IncidentDetail />
              </Layout>
            </ProtectedRoute>
          } />
          <Route path="/vehicles" element={
            <ProtectedRoute>
              <Layout>
                <VehiclesList />
              </Layout>
            </ProtectedRoute>
          } />
          <Route path="/map" element={
            <ProtectedRoute>
              <Layout>
                <MapView />
              </Layout>
            </ProtectedRoute>
          } />
          <Route path="/roster" element={
            <RoleProtectedRoute requiredPermission={() => useUserStore.getState().canViewPersonnelRoster()}>
              <Layout>
                <Roster />
              </Layout>
            </RoleProtectedRoute>
          } />
          <Route path="/station" element={
            <ProtectedRoute>
              <Layout>
                <StationManagement />
              </Layout>
            </ProtectedRoute>
          } />
          <Route path="/weather" element={
            <ProtectedRoute>
              <Layout>
                <WeatherForecast />
              </Layout>
            </ProtectedRoute>
          } />
        </Routes>
            </ToastProvider>
          </IncidentNotificationProvider>
        </NotificationProvider>
      </AuthProvider>
    </ThemeProvider>
  )
}

export default App
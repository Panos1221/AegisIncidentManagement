import { useUserStore } from '../lib/userStore';
import FireDepartmentDashboard from './dashboards/FireDepartmentDashboard';
import CoastGuardDashboard from './dashboards/CoastGuardDashboard';
import EKABDashboard from './dashboards/EKABDashboard';
import PoliceDashboard from './dashboards/PoliceDashboard';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { AlertTriangle } from 'lucide-react';

export default function Dashboard() {
  const { user } = useUserStore();

  if (!user) {
    return (
      <div className="p-6">
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <AlertTriangle className="h-5 w-5 text-yellow-500" />
              Authentication Required
            </CardTitle>
          </CardHeader>
          <CardContent>
            <p>Please log in to access your dashboard.</p>
          </CardContent>
        </Card>
      </div>
    );
  }

  // Route to agency-specific dashboard based on user's agency
  switch (user.agencyName) {
    case 'Hellenic Fire Service':
      return <FireDepartmentDashboard />;
    case 'Hellenic Coast Guard':
      return <CoastGuardDashboard />;
    case 'EKAB':
      return <EKABDashboard />;
     case 'Hellenic Police':
      return <PoliceDashboard />;     
    default:
      return (
        <div className="p-6">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <AlertTriangle className="h-5 w-5 text-yellow-500" />
                Unknown Agency
              </CardTitle>
            </CardHeader>
            <CardContent>
              <p>
                Your agency "{user.agencyName}" is not recognized. 
                Please contact your system administrator.
              </p>
              <div className="mt-4 p-3 bg-gray-100 dark:bg-gray-800 rounded">
                <p className="text-sm text-gray-600 dark:text-gray-400">
                  User: {user.name} ({user.email})<br />
                  Role: {user.role}<br />
                  Agency: {user.agencyName}
                </p>
              </div>
            </CardContent>
          </Card>
        </div>
      );
  }
}
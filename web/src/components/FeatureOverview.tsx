import { useUserStore } from '../lib/userStore'
import { 
  AlertTriangle, 
  Users, 
  Truck, 
  Bell, 
  Shield, 
  BarChart3,
  MapPin,
  Clock
} from 'lucide-react'

export default function FeatureOverview() {
  const { isDispatcher } = useUserStore()

  const dispatcherFeatures = [
    {
      icon: AlertTriangle,
      title: 'Create Incidents',
      description: 'Report new emergencies with priority levels and detailed information',
      color: 'text-red-600 bg-red-100'
    },
    {
      icon: Shield,
      title: 'Resource Assignment',
      description: 'Assign vehicles and personnel to incidents across all stations',
      color: 'text-blue-600 bg-blue-100'
    },
    {
      icon: BarChart3,
      title: 'System Analytics',
      description: 'View comprehensive statistics and response metrics',
      color: 'text-green-600 bg-green-100'
    },
    {
      icon: MapPin,
      title: 'Multi-Station View',
      description: 'Monitor incidents and resources across all fire stations',
      color: 'text-purple-600 bg-purple-100'
    }
  ]

  const firefighterFeatures = [
    {
      icon: Users,
      title: 'Station Management',
      description: 'Manage personnel roster and crew assignments for your station',
      color: 'text-blue-600 bg-blue-100'
    },
    {
      icon: Truck,
      title: 'Fleet Management',
      description: 'Add vehicles and assign personnel to apparatus',
      color: 'text-green-600 bg-green-100'
    },
    {
      icon: Bell,
      title: 'Incident Notifications',
      description: 'Receive real-time alerts for incidents assigned to your station',
      color: 'text-yellow-600 bg-yellow-100'
    },
    {
      icon: Clock,
      title: 'Station Analytics',
      description: 'View incident history and statistics for your station',
      color: 'text-purple-600 bg-purple-100'
    }
  ]

  const features = isDispatcher() ? dispatcherFeatures : firefighterFeatures

  return (
    <div className="card p-6 mb-8">
      <h2 className="text-lg font-semibold text-gray-900 dark:text-gray-100 mb-4">
        {isDispatcher() ? 'Dispatcher Capabilities' : 'Station Management Features'}
      </h2>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {features.map((feature, index) => (
          <div key={index} className="flex items-start space-x-3 p-3 rounded-lg hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors">
            <div className={`p-2 rounded-lg ${feature.color}`}>
              <feature.icon className="w-5 h-5" />
            </div>
            <div>
              <h3 className="font-medium text-gray-900">{feature.title}</h3>
              <p className="text-sm text-gray-600 mt-1">{feature.description}</p>
            </div>
          </div>
        ))}
      </div>
      
      <div className="mt-4 p-3 bg-blue-50 rounded-lg">
        <p className="text-sm text-blue-800">
          <strong>Demo Mode:</strong> Use the user switcher in the sidebar to test different role permissions. 
          {isDispatcher() 
            ? ' Try creating an incident and assigning resources!' 
            : ' Try managing your station personnel and vehicles!'
          }
        </p>
      </div>
    </div>
  )
}
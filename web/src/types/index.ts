export enum IncidentStatus {
  Created = 0,
  OnGoing = 1,
  PartialControl = 2,
  Controlled = 3,
  FullyControlled = 4,
  Closed = 5
}

export enum IncidentPriority {
  Critical = 1,
  High = 2,
  Normal = 3,
  Low = 4
}

export enum VehicleStatus {
  Available = 0,
  Notified = 1,
  EnRoute = 2,
  OnScene = 3,
  Busy = 4,
  Maintenance = 5,
  Offline = 6
}

export enum ResourceType {
  Vehicle = 0,
  Personnel = 1
}

export enum UserRole {
  Dispatcher = 0,
  FireDispatcher = 1,
  Firefighter = 2,
  CoastGuardDispatcher = 3,
  CoastGuardMember = 4,
  EKABDispatcher = 5,
  EKABMember = 6
}

export enum NotificationType {
  IncidentAssigned = 0,
  IncidentStatusUpdate = 1,
  VehicleAssigned = 2,
  PersonnelAssigned = 3
}

export enum AgencyType {
  FireDepartment = 0,
  CoastGuard = 1,
  EKAB = 2,
  Police = 3
}

export interface Agency {
  id: number
  type: AgencyType
  name: string
  code: string
  isActive: boolean
  createdAt: string
}

export interface CreateAgency {
  type: AgencyType
  name: string
  code: string
  isActive?: boolean
}

export interface Station {
  id: number
  name: string
  agencyId: number
  agencyType: AgencyType
  latitude: number
  longitude: number
}

export interface Vehicle {
  id: number
  stationId: number
  callsign: string
  type: string
  status: VehicleStatus
  plateNumber: string
  waterLevelLiters?: number
  waterCapacityLiters?: number
  foamLevelLiters?: number
  fuelLevelPercent?: number
  batteryVoltage?: number
  pumpPressureKPa?: number
  latitude?: number
  longitude?: number
  lastTelemetryAt?: string
}

export interface Personnel {
  id: number
  stationId: number
  name: string
  rank: string
  badgeNumber?: string
  isActive: boolean
  agencyId: number
  agencyName: string
  station?: {
    id: number
    name: string
    agencyId: number
    agencyName: string
    latitude: number
    longitude: number
  }
}

export interface CreatePersonnel {
  stationId: number
  agencyId: number
  name: string
  rank: string
  badgeNumber?: string
  isActive?: boolean
}

export interface Assignment {
  id: number
  incidentId: number
  resourceType: ResourceType
  resourceId: number
  status: string
  createdAt: string
  dispatchedAt?: string
  enRouteAt?: string
  onSceneAt?: string
  completedAt?: string
}

export interface IncidentLog {
  id: number
  incidentId: number
  at: string
  message: string
  by?: string
}

export interface Caller {
  id: number
  incidentId: number
  name?: string
  phoneNumber: string
  calledAt?: string
  notes?: string
}

export interface User {
  id: number
  supabaseUserId: string
  email: string
  name: string
  role: UserRole
  agencyId?: number
  agencyName?: string
  stationId?: number
  stationName?: string
  isActive: boolean
  createdAt: string
}

export interface CreateUser {
  supabaseUserId: string
  email: string
  name: string
  role: UserRole
  stationId?: number
}

export interface Notification {
  id: number
  userId: number
  type: NotificationType
  title: string
  message: string
  incidentId?: number
  isRead: boolean
  createdAt: string
}

export interface VehicleAssignment {
  id: number
  vehicleId: number
  vehicleCallsign: string
  personnelId: number
  personnelName: string
  personnelRank: string
  assignedAt: string
  unassignedAt?: string
  isActive: boolean
}

export interface CreateVehicleAssignment {
  vehicleId: number
  personnelId: number
}

export enum IncidentClosureReason {
  Action = 0,
  WithoutAction = 1,
  PreArrival = 2,
  Cancelled = 3,
  FalseAlarm = 4
}

export interface Incident {
  id: number
  stationId: number
  agencyId: number
  type?: string // For backwards compatibility
  mainCategory: string
  subCategory: string
  address?: string
  street?: string
  streetNumber?: string
  city?: string
  region?: string
  postalCode?: string
  country?: string
  latitude: number
  longitude: number
  status: IncidentStatus
  priority: IncidentPriority
  notes?: string
  createdByUserId: number
  createdByName: string
  createdAt: string
  // Close incident properties
  isClosed: boolean
  closureReason?: IncidentClosureReason
  closedAt?: string
  closedByUserId?: number
  closedByName?: string
  assignments: Assignment[]
  logs: IncidentLog[]
  callers?: Caller[]
  participationType?: string

  // New detailed incident information
  involvement?: IncidentInvolvement
  commanders: IncidentCommander[]
  injuries: Injury[]
  deaths: Death[]
  fire?: IncidentFire
  damage?: IncidentDamage
}

export interface CreateIncident {
  stationId: number
  type?: string // For backwards compatibility
  mainCategory: string
  subCategory: string
  address?: string
  street?: string
  streetNumber?: string
  city?: string
  region?: string
  postalCode?: string
  country?: string
  latitude: number
  longitude: number
  priority?: IncidentPriority
  notes?: string
  createdByUserId: number
  callers?: Omit<Caller, 'id' | 'incidentId'>[]
}

export interface CloseIncidentDto {
  closureReason: IncidentClosureReason
  closedByUserId: number
}

export interface FireStation {
  id: number
  name: string
  address: string
  city: string
  region: string
  area: number
  latitude: number
  longitude: number
  geometryJson: string
  createdAt: string
}

export interface FireStationBoundary {
  id: number
  fireStationId: number
  name: string
  coordinates: [number, number][][]
}

export interface LocationDto {
  latitude: number
  longitude: number
}

export interface UpdateVehicleDto {
  callsign?: string
  type?: string
  plateNumber?: string
  stationId?: number
  waterCapacityLiters?: number
}

export interface Ship {
  mmsi: string
  name?: string
  latitude: number
  longitude: number
  speed?: number // knots
  lastUpdate: string
}

export interface PatrolZone {
  id: number
  name: string
  description?: string
  agencyId: number
  agencyName: string
  stationId: number
  stationName: string
  boundaryCoordinates: string // GeoJSON polygon coordinates
  centerLatitude: number
  centerLongitude: number
  priority: number // 1 = High, 2 = Medium, 3 = Low
  isActive: boolean
  color?: string
  createdAt: string
  updatedAt?: string
  createdByUserId: number
  createdByUserName: string
  vehicleAssignments: PatrolZoneAssignment[]
}

export interface CreatePatrolZone {
  name: string
  description?: string
  stationId: number
  boundaryCoordinates: string
  centerLatitude: number
  centerLongitude: number
  priority?: number
  color?: string
}

export interface UpdatePatrolZone {
  name?: string
  description?: string
  stationId?: number
  boundaryCoordinates?: string
  centerLatitude?: number
  centerLongitude?: number
  priority?: number
  isActive?: boolean
  color?: string
}

export interface PatrolZoneAssignment {
  id: number
  patrolZoneId: number
  patrolZoneName: string
  vehicleId: number
  vehicleCallsign: string
  vehicleType: string
  assignedAt: string
  unassignedAt?: string
  isActive: boolean
  assignedByUserId: number
  assignedByUserName: string
  unassignedByUserId?: number
  unassignedByUserName?: string
  notes?: string
}

export interface CreatePatrolZoneAssignment {
  patrolZoneId: number
  vehicleId: number
  notes?: string
}

export interface IncidentTypeSubcategory {
  subcategoryNameEl: string
  subcategoryNameEn: string
}

export interface IncidentTypeCategory {
  categoryKey: string
  categoryNameEl: string
  categoryNameEn: string
  subcategories: IncidentTypeSubcategory[]
}

export interface IncidentTypesByAgency {
  agencyName: string
  categories: IncidentTypeCategory[]
}

// Incident detail interfaces
export interface IncidentInvolvement {
  id: number
  incidentId: number
  fireTrucksNumber?: number
  firePersonnel?: number
  otherAgencies?: string
  serviceActions?: string
  rescuedPeople?: number
  rescueInformation?: string
  createdAt: string
  updatedAt?: string
}

export interface CreateIncidentInvolvement {
  fireTrucksNumber?: number
  firePersonnel?: number
  otherAgencies?: string
  serviceActions?: string
  rescuedPeople?: number
  rescueInformation?: string
}

export interface IncidentCommander {
  id: number
  incidentId: number
  personnelId: number
  personnelName: string
  personnelBadgeNumber: string
  personnelRank: string
  observations?: string
  assignedAt: string
  assignedByUserId: number
  assignedByName: string
}

export interface CreateIncidentCommander {
  personnelId: number
  observations?: string
  assignedByUserId: number
}

export interface CreateIncidentCasualty {
  injuries: Injury[]
  deaths: Death[]
}

export interface Injury {
  id?: number
  name: string
  type: 'firefighter' | 'civilian'
  description?: string
}

export interface Death {
  id?: number
  name: string
  type: 'firefighter' | 'civilian'
  description?: string
}

export interface IncidentFire {
  id: number
  incidentId: number
  burnedArea?: string
  burnedItems?: string
  createdAt: string
  updatedAt?: string
}

export interface CreateIncidentFire {
  burnedArea?: string
  burnedItems?: string
}

export interface IncidentDamage {
  id: number
  incidentId: number
  ownerName?: string
  tenantName?: string
  damageAmount?: number
  savedProperty?: number
  incidentCause?: string
  createdAt: string
  updatedAt?: string
}

export interface CreateIncidentDamage {
  ownerName?: string
  tenantName?: string
  damageAmount?: number
  savedProperty?: number
  incidentCause?: string
}

export interface FireHydrant {
  id: number
  externalId: string
  latitude: number
  longitude: number
  position?: string
  type?: string
  additionalProperties?: Record<string, any>
  createdAt: string
  updatedAt: string
  isActive: boolean
}

export interface CoastGuardStation {
  id: number
  name: string
  nameGr: string
  address: string
  area: string
  type: string
  telephone?: string
  email?: string
  latitude: number
  longitude: number
  createdAt: string
}

export interface PoliceStation {
  id: number
  gid: number
  originalId: number
  name: string
  address: string
  sinoikia: string
  diam: string
  latitude: number
  longitude: number
  createdAt: string
}

export interface Hospital {
  id: number
  name: string
  address: string
  city: string
  region: string
  latitude: number
  longitude: number
  createdAt: string
}

export interface StationAssignmentRequestDto {
  latitude: number
  longitude: number
  agencyType: string // "fire", "coastguard", "police", "hospital"
}

export interface StationAssignmentResponseDto {
  stationId: number
  stationName: string
  assignmentMethod: string // "District", "Nearest"
  distance: number // Distance in meters (for nearest assignments)
  districtName: string // For district-based assignments
}


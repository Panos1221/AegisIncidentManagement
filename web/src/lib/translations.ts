export interface Translations {

  // Navigation
  dashboard: string
  cad: string
  incidents: string
  vehicles: string
  map: string
  stationNav: string
  roster: string
  settings: string
  logout: string
  systemOnline:string

  //Pages
  fireTrucks: string
  maritimeIncidents: string
  resourcesCapital: string
  maritimeMap: string
  medicalCalls: string
  incidentsPage:string
  coverageMap: string
  cadDescription: string
  fireStationsPlural:string
  Ports_Bases:string
  ekabStations:string
  managePersonnel:string

  //Agencies
  coastGuard:string
  fireService:string
  ekab:string
  police:string
  policeOfficer:string
  policeVehicles:string
  coastGuardVehicles:string
  coastGuardMembers:string
  EKABVehicles:string
  EKABMembers:string

  // Common
  from:string
  vehicleSingle:string
  to:string
  or: string
  save: string
  cancel: string
  delete: string
  edit: string
  create: string
  loading: string
  error: string
  success: string
  confirm: string
  yes: string
  no: string
  allStatuses: string
  allStations: string
  station: string
  welcomeBack: string
  fleet:string
  onDuty:string
  onAllStations:string
  vessels:string
  nauticalmiles:string
  outOfnumber: string
  total:string
  crew:string
  Unknown:string
  patrolCoverage: string
  maritimePicture: string
  recentActivity:string
  newIncidentReport:string
  hospital:string
  of:string
  hospitals:string
  hospitalsOff:string
  contactUnit:string
  category:string
  automatically:string
  changed:string
  theStatus:string
  injury: string
  death: string
  noInjuries: string
  noDeaths: string

  //Weather
  weatherConditions:string
  wind:string
  waveHeight:string
  temperature:string
  visibility:string
  seaState:string
  knots:string
  knot:string
  
  // Weather Forecast Page
  weatherForecast: string
  currentWeather: string
  hourlyForecast: string
  dailyForecast: string
  marineConditions: string
  feelsLike: string
  humidity: string
  pressure: string
  cloudCover: string
  uvIndex: string
  precipitation: string
  precipitationProbability: string
  gusts: string
  seaTemperature: string
  swellHeight: string
  waves: string
  filters: string
  viewMode: string
  current: string
  hourly: string
  
  // Map Filters
  mapFilters: string
  showFilters: string
  hideFilters: string
  resetFilters: string
  applyFilters: string
  filterByAgency: string
  showIncidents: string
  showVehicles: string
  showStations: string
  showBoundaries: string
  showHydrants: string
  showPatrolZones: string
  createPatrolZones: string
  showOtherAgencyStations: string
  filterSettings: string
  visibilityControls: string
  
  // Filter descriptions
  incidentsFilterDescription: string
  vehiclesFilterDescription: string
  fireStationsFilterDescription: string
  fireStationBoundariesFilterDescription: string
  fireHydrantsFilterDescription: string
  policeStations: string
  policeStationsFilterDescription: string
  coastGuardStations: string
  coastGuardStationsFilterDescription: string
  shipsFilterDescription: string
  ambulancesFilterDescription: string
  hospitalsFilterDescription: string
  availableFilters: string
  filtersDescription: string
  showAll: string
  hideAll: string
  apply: string
  daily: string
  timePeriod: string
  tomorrow: string
  next3Days: string
  next7Days: string
  customRange: string
  startDate: string
  endDate: string
  showMarineData: string
  locationRequired: string
  enableLocationForWeather: string
  loadingWeatherData: string
  weatherLoadError: string
  temperatureMax: string
  temperatureMin: string
  
  // Wind Direction translations
  windN: string
  windNNE: string
  windNE: string
  windENE: string
  windE: string
  windESE: string
  windSE: string
  windSSE: string
  windS: string
  windSSW: string
  windSW: string
  windWSW: string
  windW: string
  windWNW: string
  windNW: string
  windNNW: string
  
  // Sea State translations
  seaStateCalm:string
  seaStateCalmRippled:string
  seaStateSmooth:string
  seaStateModerate:string
  seaStateRough:string
  seaStateVeryRough:string
  seaStateHigh:string
  seaStateVeryHigh:string
  seaStatePhenomenal:string

  // Weather Descriptions
  weatherClear: string
  weatherMainlyClear: string
  weatherPartlyCloudy: string
  weatherOvercast: string
  weatherFog: string
  weatherDrizzleLight: string
  weatherDrizzleModerate: string
  weatherDrizzleDense: string
  weatherRainLight: string
  weatherRainModerate: string
  weatherRainHeavy: string
  weatherSnowLight: string
  weatherSnowModerate: string
  weatherSnowHeavy: string
  weatherShowers: string
  weatherThunderstorm: string

  // Incident Management
  incident:string
  newIncident: string
  incidentType: string
  incidentDescription: string
  incidentAddress: string
  incidentPriority: string
  incidentNotes: string
  incidentLocation: string
  createIncident: string
  creating: string
  assignToStation: string
  selectIncidentType: string
  selectStation: string
  briefDescription: string
  briefNotes: string
  streetAddress: string
  specialInstructions: string
  latestFive: string
  totalActiveIncidents: string

  // Detailed Address Fields
  fullAddress: string
  street: string
  streetNumber: string
  postalCode: string
  country: string
  optional: string

  // Incident Types
  fire: string
  medicalEmergency: string
  vehicleAccident: string
  hazmat: string
  rescue: string
  other: string

  // Priority Levels
  lowPriority: string
  normalPriority: string
  highPriority: string
  criticalPriority: string

  // Incident Status
  created: string
  createdAt: string
  onGoing: string
  partialControl: string
  controlled: string
  fullyControlled: string
  closed: string

  // Incident Closure
  cancelled: string
  duplicate: string
  falseAlarm: string
  closing: string
  closeIncident: string
  closureReason: string
  closedBy: string
  closedAt: string
  action: string
  withoutAction: string
  preArrival: string
  incidentClosedSuccessfully: string
  incidentReopenedSuccessfully: string
  reopenIncident: string
  continue: string
  confirmCloseIncident: string
  confirmCloseIncidentMessage: string
  goBack: string
  confirmReopenIncident: string
  confirmReopenIncidentMessage: string
  reopening: string

  // Vehicle Status
  notified: string
  enRoute: string
  onScene: string
  completed:string
  dispatched: string
  statusChanged: string
  priorityChanged: string
  incidentCreated: string
  incidentUpdated: string

  // User Interface
  darkMode: string
  lightMode: string
  language: string
  english: string
  greek: string

  // System
  incidentManagementSystem: string
  pleaseLogIn: string
  logInWithSupabase: string
  demoMode: string
  switchUser: string
  dispatcher: string
  firefighter: string

  // Location
  latitude: string
  longitude: string
  automaticallyAssigned: string
  noStationFound: string
  unableToDetermineStation: string
  stationAssignmentInfo: string
  addressDetails: string
  caller: string
  callerPhoneNumbers: string
  callerInformation: string
  more: string
  streetAndNumber: string
  zipCode:string
  addCaller: string
  phoneNumber: string
  callerName: string
  additionalNotes: string
  anonymous: string
  viewAll:string
  calledAt:string
  callers: string
  viewCallers: string
  unknownCaller: string
  noCallersRecorded: string

  // Validation Messages
  enterIncidentType: string
  setIncidentLocation: string
  selectFireStation: string

  // Page titles
  vehiclesPageTitle: string
  stationManagementTitle: string
  rosterPageTitle: string
  mapViewTitle: string
  incidentsPageTitle: string
  settingsPageTitle: string

  // Form elements
  name: string
  rank: string
  badgeNumber: string
  callsign: string
  plateNumber: string
  vehicleType: string
  personnel: string
  addPersonnel: string
  editPersonnel: string
  removePersonnel: string
  vehicleStatus: string
  available: string
  inUse: string
  outOfService: string
  maintenance: string
  add:string
  information:string

  // Status and messages
  noDataFound: string
  loadingData: string
  accessDenied: string
  permissionDenied: string
  noVehiclesFound: string
  noPersonnelFound: string
  noIncidentsFound: string
  dataLoadError: string
  updateVehicleStatus: string
  updatePersonnelStatus:string

  // Actions
  addVehicle: string
  editVehicle: string
  removeVehicle: string
  assignVehicle: string
  unassignVehicle: string
  assignedToPatrolZone: string
  viewDetails: string
  refresh: string
  search: string
  filter: string
  clear: string
  vehicleFilter: string
  showAllStations: string
  responsibleStationOnly: string
  showResponsibleStationOnly: string
  selectStations: string
  stationsSelected: string
  showingVehiclesFromAllStations: string
  showingVehiclesFromMyStation: string
  
  // Sorting and filtering
  filtersAndSorting: string
  sortBy: string
  sortOrder: string
  sortByStatus: string
  sortByResources: string
  sortByDate: string
  participationType: string
  allTypes: string
  primaryIncidents: string
  reinforcementIncidents: string
  reinforcement: string
  lowToHigh: string
  highToLow: string
  oldestFirst: string
  newestFirst: string

  // Table headers
  status: string
  lastUpdate: string
  telemetry: string
  fuelLevel: string
  waterLevel: string
  batteryVoltage: string
  pumpPressure: string
  location: string
  assignedPersonnel: string
  vehicleId: string
  type: string
  model: string
  year: string


  // Hellenic Coast Guard Vehicle / Vessel Types
  patrolBoat: string
  offshorePatrolVessel: string
  searchRescueBoat: string
  rigidInflatable: string
  pollutionControl: string
  cghelicopter: string
  cgairplane: string
  patrolVehicle: string
  cgbus: string

  // Vehicle types
  fireEngine: string
  ladder: string
  rescueVehicle: string
  ambulance: string
  command: string
  tanker: string
  fireBoat: string
  hazmatTruck: string
  support: string
  foodtruck: string
  fcbus: string
  petroltruck: string

  // Police Vehicle Types
  policePatrolCar: string
  policeMotorcycle: string
  policeVan: string
  policeBus: string
  policeHelicopter: string
  policeBoat: string
  policeCommandVehicle: string
  policeSpecialOperations: string
  policeTrafficEnforcement: string
  policeK9Unit: string
  policeBombSquad: string
  policeForensics: string

  // EKAB Vehicle Types
  basicAmbulance: string
  advancedAmbulance: string
  intensiveCareAmbulance: string
  neonatalAmbulance: string
  ekabMotorcycle: string
  ekabHelicopter: string
  ekabCommandVehicle: string
  ekabMobileICU: string
  ekabRescueVehicle: string
  ekabSupplyVehicle: string


  // Hellenic Fire Service Ranks

  lieutenantFireGeneral: string
  majorFireGeneral: string
  fireBrigadier: string,
  fireColonel: string
  lieutenantFireColonel: string
  fireMajor: string
  fireCaptain: string
  fireLieutenant: string
  fireSecondLieutenant: string
  fireWarrantOfficer: string
  fireSergeantAcademy: string
  fireSergeantNonAcademy: string
  seniorFirefighterAcademy: string
  seniorFirefighterNonAcademy: string
  firefighterRank: string

  // Hellenic Coast Guard Ranks
  viceAdmiral: string
  rearAdmiral: string
  commodore: string
  captain: string
  commander: string
  ltCommander: string
  lieutenant: string
  subLieutenant: string
  ensign: string
  warrantOfficer: string
  chiefPettyOfficer: string
  pettyOfficer1st: string
  pettyOfficer2nd: string
  coastGuardRank: string

  // Hellenic Police Ranks
  policeLieutenantGeneral: string
  policeMajorGeneral: string
  policeBrigadierGeneral: string
  policeDirector: string
  policeDeputyDirector: string
  policeCaptainI: string
  policeCaptainII: string
  policeLieutenantI: string
  policeLieutenantII: string
  policeDeputyLieutenant: string

  policeSergeantInvestigativeExam: string
  policeSergeantInvestigative: string
  policeSergeantNonInvestigative: string
  policeDeputySergeantInvestigative: string
  policeDeputySergeantNonInvestigative: string
  policeConstable: string


  // EKAB (National Center for Emergency Care) Ranks
  ekabPresident: string
  ekabVicePresident: string
  ekabRegionalDirector: string
  ekabDepartmentHead: string

  ekabEmergencyDoctor: string
  ekabNurse: string
  ekabParamedicSupervisor: string
  ekabParamedic: string
  ekabAmbulanceDriver: string

  ekabHelicopterDoctor: string
  ekabHelicopterParamedic: string
  ekabSpecialRescueTeam: string

  ekabCallCenterOperator: string
  ekabAdministrativeStaff: string
  ekabLogisticsSupport: string

  // Station management specific
  managePersonnelAndVehicles: string
  youDontHavePermission: string
  active: string
  inactive: string
  activeStatus: string
  inactiveStatus: string
  unknownPriority: string
  createdDate: string
  activeAssignments: string
  noPersonnelAssigned: string
  selectRank: string
  selectPersonnelToAssign: string
  alreadyAssignedToVehicle: string
  assignPersonnelToVehicle: string
  assign: string
  fireTruck: string
  engine: string
  unassign: string
  dismiss: string
  confirmUnassign: string

  // Map and location
  mapControls: string
  zoomIn: string
  zoomOut: string
  centerMap: string
  fullscreen: string
  coordinates: string
  address: string
  selectLocation: string
  currentLocation: string
  realTimeView: string
  legend: string
  activeIncidents: string
  fireStations: string
  fireStationsMap: string
  stationDistricts: string
  hide: string
  show: string
  fireStationDistricts: string
  dataLoadingIssues: string
  vehiclesOnMap: string
  fireStation: string
  region: string
  city: string
  area: string
  district: string
  department: string
  noDescription: string
  noNotes: string
  description: string
  notes: string
  noActiveIncidents: string
  selectIncidentToView: string
  incidentLogs: string
  selectIncidentToViewVehicles: string
  finished: string
  id: string
  hydrant:string
  hydrants:string

  // Notifications and alerts
  vehicleAdded: string
  vehicleUpdated: string
  vehicleRemoved: string
  personnelAdded: string
  personnelUpdated: string
  personnelRemoved: string
  settingsSaved: string
  operationFailed: string
  connectionLost: string
  reconnecting: string

  // Time and dates
  today: string
  yesterday: string
  thisWeek: string
  lastWeek: string
  thisMonth: string
  lastMonth: string
  never: string
  unknown: string

  // Settings page
  settingsDescription: string
  notifications: string
  system: string
  security: string
  emailAlerts: string
  emailAlertsDescription: string
  smsAlerts: string
  smsAlertsDescription: string
  pushNotifications: string
  pushNotificationsDescription: string
  defaultStation: string
  mapProvider: string
  refreshInterval: string
  refreshIntervalSeconds: string
  enableAutoAssignment: string
  sessionTimeout: string
  sessionTimeoutMinutes: string
  passwordExpiry: string
  passwordExpiryDays: string
  requireMFA: string
  selectDefaultStation: string
  googleMaps: string
  openStreetMap: string
  mapbox: string
  saveSettings: string

  // Empty states
  noVehiclesMessage: string
  noPersonnelMessage: string
  noIncidentsMessage: string
  noDataMessage: string
  addFirstVehicle: string
  addFirstPersonnel: string
  createFirstIncident: string

  // Vehicle-specific translations
  monitorVehicleStatus: string
  clearFilters: string
  vehicleCount: string
  vehiclesCount: string
  noVehiclesForStation: string
  tryAdjustingFilters: string
  noVehiclesRegistered: string
  failedToLoadVehicles: string
  vehicleCreatedSuccessfully: string
  vehicleUpdatedSuccessfully: string
  fillRequiredFields: string
  selectVehicleType: string
  licensePlateNumber: string
  waterCapacityLiters: string
  addingVehicle: string
  errorCreatingVehicle: string
  close: string
  retry: string
  editVehicleTitle: string
  fuel: string
  water: string
  battery: string
  pump: string
  offline: string
  busy: string

  // Incident-specific translations
  manageAndTrackIncidents: string
  newIncidentButton: string
  incidentNotFound: string
  incidentNotFoundDescription: string
  incidentDetails: string
  createdBy: string
  reported: string
  resolved: string
  assignedResources: string
  noResourcesAssigned: string
  activityLog: string
  noActivityLogged: string
  quickActions: string
  updateStatus: string
  assignResource: string
  addLogEntry: string
  viewOnMap: string
  statistics: string
  resourcesAssigned: string
  logEntries: string
  duration: string
  minutes: string
  updateStatusModal: string
  newStatus: string
  updating: string
  update: string
  assignResourceModal: string
  availableVehiclesModal: string
  availablePersonnel: string
  addLogModal: string
  message: string
  adding: string
  addLog: string
  priority: string
  incidentId: string
  resourceAssigned: string
  resources: string
  resource: string
  resourcesAvailable:string

  // Enhanced assignment UI
  vehiclesSection: string
  assigned: string
  assignedToThisIncident: string
  assignedToIncident: string
  unavailable: string
  outOfServiceStatus: string
  resourceAssignedSuccessfully: string
  resourceUnassignedSuccessfully: string
  confirmUnassignResource: string
  unassignResource: string

  // Patrol Zone Management
  createPatrolZone: string
  editPatrolZone: string
  patrolZone: string
  patrolZoneName: string
  patrolZoneDescription: string
  patrolZonePriority: string
  patrolZoneColor: string
  drawPolygon: string
  drawPolygonInstructions: string
  patrolZoneCreated: string
  patrolZoneUpdated: string
  patrolZoneDeleted: string
  deletePatrolZone: string
  confirmDeletePatrolZone: string
  patrolZoneNamePlaceholder: string
  patrolZoneDescriptionPlaceholder: string
  lowPriorityPatrol: string
  mediumPriorityPatrol: string
  highPriorityPatrol: string
  criticalPriorityPatrol: string
  selectColor: string
  assignedVehicles: string
  ourAssignedVehicles: string
  noVehiclesAssigned: string
  patrolZoneDetails: string
  coverage: string
  responsibleStation: string
  patrolZoneNameRequired: string
  patrolZoneDescriptionError: string
  createZone: string
  updateZone: string
  selectVehicleToAssign: string
  noAvailableVehicles: string
  vehicleAssignedSuccessfully: string
  errorAssigningVehicle: string
  assigning: string
  additionalResource: string
  telephone: string
  email: string

  // Login page
  loginTitle: string
  loginSubtitle: string
  loginWelcome: string
  loginDescription: string
  loginButton: string
  password: string
  signIn: string
  demoCredentials: string
  showDemoCredentials: string
  hideDemoCredentials: string
  showCredentials: string
  hideCredentials: string
  copyEmail: string
  emailCopied: string
  loginError: string
  loginSuccess: string
  invalidCredentials: string
  welcomeToSystem: string
  clickOnDemoToAutoFill:string
  member:string
  enterEmail:string
  enterPassword:string

  // New incident detail fields
  generalInfo: string
  subCategory: string
  incidentResponsibleStation: string
  involvement: string
  incidentPersonnel: string
  fireTrucksNumber: string
  firePersonnel: string
  otherAgencies: string
  serviceActions: string
  rescues: string
  rescuedPeople: string
  rescueInformation: string
  commanders: string
  searchForCommander: string
  observations: string
  noOfficersFound: string
  optionalObservations: string
  noCommandersAssigned: string
  select:string
  signal: string
  observationsForCommander: string
  describeOtherAgencies: string
  describeServiceActions: string
  rescueDetails: string
  casualtiesAccidentsBurns:string
  casualties: string
  accidents: string
  injuries: string
  firemen: string
  civilians: string
  nameAndCapacity: string
  deaths: string
  burned: string
  burnedArea: string
  burnedItems: string
  damages: string
  ownerName: string
  tenantName: string
  damageAmount: string
  savedProperty: string
  incidentCause: string
}

export const translations: Record<'en' | 'el', Translations> = {
  en: {

    //Agencies
    coastGuard:'Hellenic Coast Guard',
    fireService:'Hellenic Fire Service',
    ekab:'EKAB',
    police:'Hellenic Police',
    policeOfficer:'Police Officers',
    policeVehicles:'Police Vehicles',
    coastGuardVehicles:'Coast Guard Vehicles',
    coastGuardMembers:'Coast Guard Members',
    EKABVehicles:'EKAB Vehicles',
    EKABMembers:'EKAB Members',

    // Navigation
    dashboard: 'Dashboard',
    cad: 'Computer-aided dispatch (CAD)',
    incidents: 'Incidents',
    vehicles: 'Vehicles',
    map: 'Map',
    stationNav: 'Station',
    roster: 'Roster',
    settings: 'Settings',
    logout: 'Logout',
    systemOnline:'System Online',
    fireStationsPlural:'Fire Stations',
    Ports_Bases:'Ports & Bases',
    ekabStations:'EKAB Stations',
    managePersonnel: 'Personnel Management',   

    //Pages
    fireTrucks: 'Fleet',
    maritimeIncidents: 'Incidents',
    resourcesCapital: 'Fleet',
    maritimeMap: 'Stations & Incidents Map',
    medicalCalls: 'Medical Calls',
    incidentsPage:'Incidents Page',
    coverageMap: 'Coverage Map',
    cadDescription: 'View and manage active incidents assigned to your station',
    noActiveIncidents: 'No active incidents',
    selectIncidentToView: 'Select an incident to view details',
    description: 'Description',
    notes: 'Notes',
    incidentLogs: 'Incident Logs',
    selectIncidentToViewVehicles: 'Select an incident to view assigned vehicles',
    finished: 'Finished',

    // Common
    from:'from',
    vehicleSingle:'Vehicle',
    to:'to',
    or: 'or',
    save: 'Save',
    cancel: 'Cancel',
    delete: 'Delete',
    edit: 'Edit',
    create: 'Create',
    loading: 'Loading',
    error: 'Error',
    success: 'Success',
    confirm: 'Confirm',
    yes: 'Yes',
    no: 'No',
    allStatuses: 'All Statuses',
    allStations: 'All Stations',
    station: 'Station',
    welcomeBack: 'Welcome back',
    fleet: 'Fleet',
    onDuty: 'On Duty',
    vessels: 'Vessels',
    nauticalmiles:'nautical miles',
    outOfnumber: 'Out of',
    total:"total",
    crew: "Crew",
    Unknown:"Unknown",
    maritimePicture: 'Maritime Picture — Greece',
    onAllStations: 'On All Stations',
    patrolCoverage: 'Patrol Coverage',
    recentActivity: 'Recent Activity',
    newIncidentReport: 'New Incident Report',
    hospital: 'Hospital',
    of: 'of',
    hospitals: 'Hospitals',
    hospitalsOff: 'Hospitals',
    contactUnit: 'Contact Unit',
    category:'Category',
    automatically:'automatically',
    changed: 'changed',
    theStatus: 'Status',
    injury: 'Injury',
    death: 'Death',
    noInjuries: 'No injuries recorded',
    noDeaths: 'No Deaths recorded',

    // Weather
    weatherConditions: 'Weather Conditions',
    wind: 'Wind',
    waveHeight: 'Wave Height',
    temperature: 'Temperature',
    visibility: 'Visibility',
    seaState: 'Sea State',
    knots: 'knots',
    knot: 'knot',
    
    // Weather Forecast Page
    weatherForecast: 'Weather Forecast',
    currentWeather: 'Current Weather',
    hourlyForecast: 'Hourly Forecast',
    dailyForecast: 'Daily Forecast',
    marineConditions: 'Marine Conditions',
    feelsLike: 'Feels like',
    humidity: 'Humidity',
    pressure: 'Pressure',
    cloudCover: 'Cloud Cover',
    uvIndex: 'UV Index',
    precipitation: 'Precipitation',
    precipitationProbability: 'Rain Probability',
    gusts: 'Gusts',
    seaTemperature: 'Sea Temperature',
    swellHeight: 'Swell Height',
    waves: 'Waves',
    filters: 'Filters',
    viewMode: 'View Mode',
    current: 'Current',
    hourly: 'Hourly',
    
    // Map Filters
    mapFilters: 'Map Filters',
    showFilters: 'Show Filters',
    hideFilters: 'Hide Filters',
    resetFilters: 'Reset Filters',
    applyFilters: 'Apply Filters',
    filterByAgency: 'Filter by Agency',
    showIncidents: 'Show Incidents',
    showVehicles: 'Show Vehicles',
    showStations: 'Show Stations',
    showBoundaries: 'Show Boundaries',
    showHydrants: 'Show Hydrants',
    showPatrolZones: 'Show Patrol Zones',
    createPatrolZones: 'Create Patrol Zones',
    showOtherAgencyStations: 'Show Other Agency Stations',
    filterSettings: 'Filter Settings',
    visibilityControls: 'Visibility Controls',
    incidentsFilterDescription: 'Toggle visibility of active incidents on the map',
    vehiclesFilterDescription: 'Show or hide emergency vehicles and their current status',
    fireStationsFilterDescription: 'Display fire stations and their coverage areas',
    fireStationBoundariesFilterDescription: 'Show fire station jurisdiction boundaries',
    fireHydrantsFilterDescription: 'Display fire hydrant locations for emergency response',
    policeStations: 'Police Stations',
    policeStationsFilterDescription: 'Show police stations and their patrol areas',
    coastGuardStations: 'Coast Guard Stations',
    coastGuardStationsFilterDescription: 'Display coast guard stations and maritime coverage',
    shipsFilterDescription: 'Show or hide coast guard vessels and their current positions',
    ambulancesFilterDescription: 'Show or hide ambulances and their current status',
    hospitalsFilterDescription: 'Display hospitals and medical facilities',
    availableFilters: 'Available Filters',
    filtersDescription: 'Select which elements to display on the map',
    showAll: 'Show All',
    hideAll: 'Hide All',
    apply: 'Apply',
    add : 'Add',
    information:'Information',
    
    daily: 'Daily',
    timePeriod: 'Time Period',
    tomorrow: 'Tomorrow',
    next3Days: 'Next 3 Days',
    next7Days: 'Next 7 Days',
    customRange: 'Custom Range',
    startDate: 'Start Date',
    endDate: 'End Date',
    showMarineData: 'Show Marine Data',
    locationRequired: 'Location Required',
    enableLocationForWeather: 'Please enable location access to view weather data for your area.',
    loadingWeatherData: 'Loading weather data...',
    weatherLoadError: 'Failed to load weather data',
    temperatureMax: 'High',
    temperatureMin: 'Low',
    
    // Wind Directions
    windN: 'N',
    windNNE: 'NNE',
    windNE: 'NE',
    windENE: 'ENE',
    windE: 'E',
    windESE: 'ESE',
    windSE: 'SE',
    windSSE: 'SSE',
    windS: 'S',
    windSSW: 'SSW',
    windSW: 'SW',
    windWSW: 'WSW',
    windW: 'W',
    windWNW: 'WNW',
    windNW: 'NW',
    windNNW: 'NNW',
    
    // Sea State translations
    seaStateCalm: 'Calm (Glassy)',
    seaStateCalmRippled: 'Calm (Rippled)',
    seaStateSmooth: 'Smooth',
    seaStateModerate: 'Moderate',
    seaStateRough: 'Rough',
    seaStateVeryRough: 'Very Rough',
    seaStateHigh: 'High',
    seaStateVeryHigh: 'Very High',
    seaStatePhenomenal: 'Phenomenal',

    // Weather Descriptions
    weatherClear: 'Clear',
    weatherMainlyClear: 'Mainly clear',
    weatherPartlyCloudy: 'Partly cloudy',
    weatherOvercast: 'Overcast',
    weatherFog: 'Fog',
    weatherDrizzleLight: 'Light drizzle',
    weatherDrizzleModerate: 'Moderate drizzle',
    weatherDrizzleDense: 'Dense drizzle',
    weatherRainLight: 'Light rain',
    weatherRainModerate: 'Moderate rain',
    weatherRainHeavy: 'Heavy rain',
    weatherSnowLight: 'Light snow',
    weatherSnowModerate: 'Moderate snow',
    weatherSnowHeavy: 'Heavy snow',
    weatherShowers: 'Showers',
    weatherThunderstorm: 'Thunderstorm',

    // Incident Management
    incident:"Incident",
    newIncident: 'New Incident',
    incidentType: 'Incident Type',
    incidentDescription: 'Description',
    incidentAddress: 'Address',
    incidentPriority: 'Priority',
    incidentNotes: 'Notes',
    incidentLocation: 'Incident Address',
    createIncident: 'Create Incident',
    creating: 'Creating...',
    assignToStation: 'Assign to Station',
    selectIncidentType: 'Select incident type',
    selectStation: 'Select a station',
    briefDescription: 'Brief description of the incident...',
    briefNotes: 'Brief notes about the incident...',
    streetAddress: 'Street address or landmark',
    specialInstructions: 'Special instructions, hazards, or important information for responding firefighters...',

    // Detailed Address Fields
    fullAddress: 'Full Address',
    street: 'Street',
    streetNumber: 'Number',
    postalCode: 'Postal Code',
    country: 'Country',
    optional: 'optional',

    // Incident Types
    fire: 'Fire',
    medicalEmergency: 'Medical Emergency',
    vehicleAccident: 'Vehicle Accident',
    hazmat: 'Hazmat',
    rescue: 'Rescue',
    other: 'Other',

    // Priority Levels
    lowPriority: 'Low Priority',
    normalPriority: 'Normal Priority',
    highPriority: 'High Priority',
    criticalPriority: 'Critical Priority',

    // Incident Status
    created: 'Created',
    createdAt: 'Created At',
    onGoing: 'On Going',
    partialControl: 'Partial Control',
    controlled: 'Controlled',
    fullyControlled: 'Fully Controlled',
    closed: 'Closed',

    // Incident Closure
    cancelled: 'Cancelled',
    duplicate: 'Duplicate',
    falseAlarm: 'False Alarm',
    closing: 'Closing',
    closeIncident: 'Close Incident',
    closureReason: 'Closure Reason',
    closedBy: 'Closed By',
    closedAt: 'Closed At',
    action: 'Action',
    withoutAction: 'Without Action',
    preArrival: 'Pre-Arrival',
    incidentClosedSuccessfully: 'Incident closed successfully',
    incidentReopenedSuccessfully: 'Incident reopened successfully',
    reopenIncident: 'Reopen Incident',
    continue: 'Continue',
    confirmCloseIncident: 'Confirm Close Incident',
    confirmCloseIncidentMessage: 'Are you sure you want to close this incident? This action cannot be undone.',
    goBack: 'Go Back',
    confirmReopenIncident: 'Confirm Reopen Incident',
    confirmReopenIncidentMessage: 'Are you sure you want to reopen this incident? This will change its status back to active.',
    reopening: 'Reopening',

    // Vehicle Status
    notified: 'Notified',
    enRoute: 'En Route',
    onScene: 'On Scene',
    completed: 'Completed',
    dispatched: 'Dispatched',
    statusChanged: 'Status Changed',
    priorityChanged: 'Priority Changed',
    incidentCreated: 'Incident Created',
    incidentUpdated: 'Incident Updated',

    // User Interface
    darkMode: 'Dark Mode',
    lightMode: 'Light Mode',
    language: 'Language',
    english: 'English',
    greek: 'Ελληνικά',

    // System
    incidentManagementSystem: 'Incident Management System',
    pleaseLogIn: 'Please log in to continue',
    logInWithSupabase: 'Log In with Supabase',
    demoMode: 'Demo Mode - Switch User:',
    switchUser: 'Switch User',
    dispatcher: 'Dispatcher',
    firefighter: 'Firefighter',

    // Location
    latitude: 'Latitude',
    longitude: 'Longitude',
    automaticallyAssigned: 'Automatically assigned to',
    noStationFound: 'No fire station found for this location. Please select manually.',
    unableToDetermineStation: 'Unable to determine fire station automatically. Please select manually.',
    stationAssignmentInfo: 'Station assignment will be determined automatically when you set the incident location',
    addressDetails: 'Address Details',
    caller: 'Caller',
    callerPhoneNumbers: 'Caller Phone Numbers',
    callerInformation: 'Caller Information',
    addCaller: 'Add Caller',
    phoneNumber: 'Phone Number',
    callerName: 'Caller name',
    additionalNotes: 'Additional notes',
    anonymous: 'Anonymous',
    more: 'more',
    streetAndNumber: 'Street & Number',
    zipCode: 'Zip Code',
    viewAll:"View All",
    calledAt:"CALLED AT",
    callers: "Callers",
    viewCallers: "View Callers",
    unknownCaller: "Unknown Caller",
    noCallersRecorded: "No callers recorded for this incident",

    // Validation Messages
    enterIncidentType: 'Please enter an incident type',
    setIncidentLocation: 'Please set the incident location on the map',
    selectFireStation: 'Please select a fire station for this incident',

    // Page titles
    vehiclesPageTitle: 'Vehicles Management',
    stationManagementTitle: 'Station Management',
    rosterPageTitle: 'Personnel Roster',
    mapViewTitle: 'Map View',
    incidentsPageTitle: 'Incidents Management',
    settingsPageTitle: 'Settings',

    // Form elements
    name: 'Name',
    rank: 'Rank',
    badgeNumber: 'Badge Number',
    callsign: 'Callsign',
    plateNumber: 'Plate Number',
    vehicleType: 'Vehicle Type',
    personnel: 'Personnel',
    addPersonnel: 'Add Personnel',
    editPersonnel: 'Edit Personnel',
    removePersonnel: 'Remove Personnel',
    vehicleStatus: 'Vehicle Status',
    available: 'Available',
    inUse: 'In Use',
    outOfService: 'Out of Service',
    maintenance: 'Maintenance',

    // Status and messages
    noDataFound: 'No data found',
    loadingData: 'Loading data...',
    accessDenied: 'Access denied',
    permissionDenied: 'Permission denied',
    noVehiclesFound: 'No vehicles found',
    noPersonnelFound: 'No personnel found',
    noIncidentsFound: 'No incidents found',
    dataLoadError: 'Error loading data',

    // Actions
    addVehicle: 'Add Vehicle',
    editVehicle: 'Edit Vehicle',
    removeVehicle: 'Remove Vehicle',
    assignVehicle: 'Assign Vehicle',
    unassignVehicle: 'Unassign Vehicle',
    assignedToPatrolZone: 'Assigned to Patrol Zone',
    selectVehicleToAssign: 'Select a vehicle to assign to this patrol zone',
    noAvailableVehicles: 'No available vehicles',
    vehicleAssignedSuccessfully: 'Vehicle assigned successfully',
    errorAssigningVehicle: 'Error assigning vehicle',
    assigning: 'Assigning...',
    assign: 'Assign',
    viewDetails: 'View Details',
    refresh: 'Refresh',
    search: 'Search',
    filter: 'Filter',
    clear: 'Clear',
    vehicleFilter: 'Vehicle Filter',
    showAllStations: 'Show All Stations',
    responsibleStationOnly: 'Responsible Station',
    showResponsibleStationOnly: 'Show only responsible station',
    selectStations: 'Select stations...',
    stationsSelected: 'station(s) selected',
    showingVehiclesFromAllStations: 'Showing vehicles from all stations',
    showingVehiclesFromMyStation: 'Showing vehicles from my station only',
    additionalResource: 'Additional Resource',
    telephone: 'Telephone',
    email: 'Email',
    updateVehicleStatus: 'Update Vehicle Status',
    updatePersonnelStatus: 'Update Personnel Status',

    // Sorting and filtering
    filtersAndSorting: 'Filters & Sorting',
    sortBy: 'Sort By',
    sortOrder: 'Sort Order',
    sortByStatus: 'Status Priority',
    sortByResources: 'Resource Count',
    sortByDate: 'Date & Time',
    participationType: 'Participation Type',
    allTypes: 'All Types',
    primaryIncidents: 'Station Incidents',
    reinforcementIncidents: 'Reinforcement',
    reinforcement: 'Reinforcement',
    lowToHigh: 'Low to High',
    highToLow: 'High to Low',
    oldestFirst: 'Oldest First',
    newestFirst: 'Newest First',

    // Table headers
    status: 'Status',
    lastUpdate: 'Last Update',
    telemetry: 'Telemetry',
    fuelLevel: 'Fuel Level',
    waterLevel: 'Water Level',
    batteryVoltage: 'Battery Voltage',
    pumpPressure: 'Pump Pressure',
    location: 'Location',
    assignedPersonnel: 'Assigned Personnel',
    vehicleId: 'Vehicle ID',
    type: 'Type',
    model: 'Model',
    year: 'Year',

    // Hellenic Coast Guard Vehicle / Vessel Types
    patrolBoat: 'Patrol Boat',
    offshorePatrolVessel: 'Offshore Patrol Vessel',
    searchRescueBoat: 'Search Rescue Boat',
    rigidInflatable: 'Rigid Inflatable',
    pollutionControl: 'Pollution Control',
    cghelicopter: 'CG Helicopter',
    cgairplane: 'CG Airplane',
    patrolVehicle: 'Patrol Vehicle',
    cgbus: 'Bus',

    // Vehicle types
    fireEngine: 'Fire Engine',
    ladder: 'Ladder Truck',
    rescueVehicle: 'Rescue Vehicle',
    ambulance: 'Ambulance',
    command: 'Command Vehicle',
    tanker: 'Water Tanker',
    fireBoat: 'Fire Boat',
    hazmatTruck: 'Hazmat',
    support: 'Support Vehicle',
    foodtruck: 'Food Truck',
    fcbus: 'Fire Bus',
    petroltruck: 'Petrol Truck',

    // Police Vehicle Types
    policePatrolCar: 'Police Patrol Car',
    policeMotorcycle: 'Police Motorcycle',
    policeVan: 'Police Van',
    policeBus: 'Police Bus',
    policeHelicopter: 'Police Helicopter',
    policeBoat: 'Police Boat',
    policeCommandVehicle: 'Police Command Vehicle',
    policeSpecialOperations: 'Special Operations Vehicle',
    policeTrafficEnforcement: 'Traffic Enforcement Vehicle',
    policeK9Unit: 'K-9 Unit Vehicle',
    policeBombSquad: 'Bomb Squad Vehicle',
    policeForensics: 'Forensics Vehicle',

    // EKAB Vehicle Types
    basicAmbulance: 'Basic Ambulance',
    advancedAmbulance: 'Advanced Life Support Ambulance',
    intensiveCareAmbulance: 'Intensive Care Ambulance',
    neonatalAmbulance: 'Neonatal Ambulance',
    ekabMotorcycle: 'EKAB Motorcycle',
    ekabHelicopter: 'EKAB Helicopter',
    ekabCommandVehicle: 'EKAB Command Vehicle',
    ekabMobileICU: 'Mobile ICU',
    ekabRescueVehicle: 'EKAB Rescue Vehicle',
    ekabSupplyVehicle: 'EKAB Supply Vehicle',


    //Hellenic Fire Service Ranks

    lieutenantFireGeneral: 'Lieutenant Fire General',
    majorFireGeneral: 'Major Fire General',
    fireBrigadier: 'Fire Brigadier',
    fireColonel: 'Fire Colonel',
    lieutenantFireColonel: 'Lieutenant Fire Colonel',
    fireMajor: 'Fire Major',
    fireCaptain: 'Fire Captain',
    fireLieutenant: 'Fire Lieutenant',
    fireSecondLieutenant: 'Fire Second Lieutenant',

    fireWarrantOfficer: 'Fire Warrant Officer',

    fireSergeantAcademy: 'Fire Sergeant',
    fireSergeantNonAcademy: 'Non-Academy Fire Sergeant',

    seniorFirefighterAcademy: 'Senior Firefighter',
    seniorFirefighterNonAcademy: 'Non-Academy Senior Firefighter',

    firefighterRank: 'Firefighter',

    // Hellenic Coast Guard Ranks

    // Officer Grade Structure
    viceAdmiral: 'Vice Admiral',
    rearAdmiral: 'Rear Admiral',
    commodore: 'Commodore',
    captain: 'Captain',
    commander: 'Commander',
    ltCommander: 'Lieutenant Commander',
    lieutenant: 'Lieutenant',
    subLieutenant: 'Lieutenant Junior Grade',
    ensign: 'Ensign',

    // NCO / Enlisted Rank Structure
    warrantOfficer: 'Warrant Officer',
    chiefPettyOfficer: 'Chief Petty Officer',
    pettyOfficer1st: 'Petty Officer 1st Class',
    pettyOfficer2nd: 'Petty Officer 2nd Class',
    coastGuardRank: 'Coast Guardsman',

    // Hellenic Police Ranks
    policeLieutenantGeneral: 'Police Lieutenant General',
    policeMajorGeneral: 'Police Major General',
    policeBrigadierGeneral: 'Police Brigadier General',
    policeDirector: 'Police Director',
    policeDeputyDirector: 'Police Deputy Director',
    policeCaptainI: 'Police Captain I (or Police Major)',
    policeCaptainII: 'Police Captain II',
    policeLieutenantI: 'Police Lieutenant I',
    policeLieutenantII: 'Police Lieutenant II',
    policeDeputyLieutenant: 'Police Deputy Lieutenant (or Police Warrant Officer)',

    policeSergeantInvestigativeExam: 'Police Sergeant (Investigative Duty – with promotion exam)',
    policeSergeantInvestigative: 'Police Sergeant (Investigative Duty)',
    policeSergeantNonInvestigative: 'Police Sergeant (Non-Investigative Duty)',
    policeDeputySergeantInvestigative: 'Police Deputy Sergeant (Investigative Duty)',
    policeDeputySergeantNonInvestigative: 'Police Deputy Sergeant (Non-Investigative Duty)',
    policeConstable: 'Police Constable',

    // EKAB (National Center for Emergency Care) Ranks
    ekabPresident: 'President of EKAB',
    ekabVicePresident: 'Vice President of EKAB',
    ekabRegionalDirector: 'Regional Director',
    ekabDepartmentHead: 'Department Head',

    ekabEmergencyDoctor: 'Emergency Doctor',
    ekabNurse: 'Nurse',
    ekabParamedicSupervisor: 'Paramedic Supervisor',
    ekabParamedic: 'Paramedic (Ambulance Crew)',
    ekabAmbulanceDriver: 'Ambulance Driver',

    ekabHelicopterDoctor: 'Helicopter Doctor',
    ekabHelicopterParamedic: 'Helicopter Paramedic',
    ekabSpecialRescueTeam: 'Special Rescue Team (EKAB-SRT)',

    ekabCallCenterOperator: 'Call Center Operator (Dispatch)',
    ekabAdministrativeStaff: 'Administrative Staff',
    ekabLogisticsSupport: 'Logistics / Technical Support',

    // Station management specific
    managePersonnelAndVehicles: 'Manage personnel and vehicles for Station',
    youDontHavePermission: 'You don\'t have permission to manage station resources.',
    active: 'Active',
    inactive: 'Inactive',
    activeStatus: 'Active',
    inactiveStatus: 'Inactive',
    unknownPriority: 'Unknown Priority',
    createdDate: 'Created Date',
    activeAssignments: 'Active Assignments',
    noPersonnelAssigned: 'No personnel assigned',
    selectRank: 'Select rank',
    selectPersonnelToAssign: 'Select personnel to assign to',
    alreadyAssignedToVehicle: 'Already assigned to a vehicle',
    assignPersonnelToVehicle: 'Assign Personnel to Vehicle',
    fireTruck: 'Fire Truck',
    engine: 'Engine',
    unassign: 'Unassign',
    dismiss: 'Dismiss',
    confirmUnassign: 'Are you sure you want to unassign this resource?',

    // Map and location
    mapControls: 'Map Controls',
    zoomIn: 'Zoom In',
    zoomOut: 'Zoom Out',
    centerMap: 'Center Map',
    fullscreen: 'Fullscreen',
    coordinates: 'Coordinates',
    address: 'Address',
    selectLocation: 'Select Location',
    currentLocation: 'Current Location',
    realTimeView: 'Real-time view of incidents, vehicles, and stations',
    legend: 'Legend',
    activeIncidents: 'Active Incidents',
    fireStations: 'Fire Stations',
    fireStationsMap: 'Fire Stations',
    stationDistricts: 'Station Districts',
    hide: 'Hide',
    show: 'Show',
    fireStationDistricts: 'Fire Station Districts',
    dataLoadingIssues: 'Data Loading Issues',
    vehiclesOnMap: 'Vehicles on Map',
    fireStation: 'Fire Station',
    region: 'Region',
    city: 'City',
    area: 'Area',
    district: 'District',
    department: 'Department',
    noDescription: 'No description',
    noNotes: 'No notes',
    id: 'Incident ID',
    hydrant:'Fire Hydrant',
    hydrants:'Fire Hydrants',


    // Notifications and alerts
    vehicleAdded: 'Vehicle added successfully',
    vehicleUpdated: 'Vehicle updated successfully',
    vehicleRemoved: 'Vehicle removed successfully',
    personnelAdded: 'Personnel added successfully',
    personnelUpdated: 'Personnel updated successfully',
    personnelRemoved: 'Personnel removed successfully',
    settingsSaved: 'Settings saved successfully',
    operationFailed: 'Operation failed',
    connectionLost: 'Connection lost',
    reconnecting: 'Reconnecting...',

    // Time and dates
    today: 'Today',
    yesterday: 'Yesterday',
    thisWeek: 'This Week',
    lastWeek: 'Last Week',
    thisMonth: 'This Month',
    lastMonth: 'Last Month',
    never: 'Never',
    unknown: 'Unknown',

    // Settings page
    settingsDescription: 'Configure system preferences and user settings',
    notifications: 'Notifications',
    system: 'System',
    security: 'Security',
    emailAlerts: 'Email Alerts',
    emailAlertsDescription: 'Receive incident notifications via email',
    smsAlerts: 'SMS Alerts',
    smsAlertsDescription: 'Receive critical alerts via SMS',
    pushNotifications: 'Push Notifications',
    pushNotificationsDescription: 'Browser push notifications',
    defaultStation: 'Default Station',
    mapProvider: 'Map Provider',
    refreshInterval: 'Refresh Interval',
    refreshIntervalSeconds: 'Refresh Interval (seconds)',
    enableAutoAssignment: 'Enable Auto-Assignment',
    sessionTimeout: 'Session Timeout',
    sessionTimeoutMinutes: 'Session Timeout (minutes)',
    passwordExpiry: 'Password Expiry',
    passwordExpiryDays: 'Password Expiry (days)',
    requireMFA: 'Require Multi-Factor Authentication',
    selectDefaultStation: 'Select Default Station',
    googleMaps: 'Google Maps',
    openStreetMap: 'OpenStreetMap',
    mapbox: 'Mapbox',
    saveSettings: 'Save Settings',

    // Empty states
    noVehiclesMessage: 'No vehicles are currently registered in the system.',
    noPersonnelMessage: 'No personnel are currently assigned to this station.',
    noIncidentsMessage: 'No incidents have been reported.',
    noDataMessage: 'No data available to display.',
    addFirstVehicle: 'Add your first vehicle to get started.',
    addFirstPersonnel: 'Add your first personnel member to get started.',
    createFirstIncident: 'Create your first incident report to get started.',

    // Vehicle-specific translations
    monitorVehicleStatus: 'Monitor vehicle status and telemetry',
    clearFilters: 'Clear Filters',
    vehicleCount: 'vehicle',
    vehiclesCount: 'vehicles',
    noVehiclesForStation: 'No vehicles found for Station',
    tryAdjustingFilters: 'Try adjusting your filters',
    noVehiclesRegistered: 'No vehicles are currently registered',
    failedToLoadVehicles: 'Failed to load vehicles',
    vehicleCreatedSuccessfully: 'Vehicle created successfully',
    vehicleUpdatedSuccessfully: 'Vehicle updated successfully',
    fillRequiredFields: 'Please fill in all required fields',
    selectVehicleType: 'Select vehicle type',
    licensePlateNumber: 'License plate number',
    waterCapacityLiters: 'Water Capacity (Liters)',
    addingVehicle: 'Adding...',
    errorCreatingVehicle: 'Error Creating Vehicle',
    close: 'Close',
    retry: 'Retry',
    editVehicleTitle: 'Edit vehicle',
    fuel: 'Fuel',
    water: 'Water',
    battery: 'Battery',
    pump: 'Pump',
    offline: 'Offline',
    busy: 'Busy',

    // Incident-specific translations
    manageAndTrackIncidents: 'Manage and track emergency incidents',
    newIncidentButton: 'New Incident',
    incidentNotFound: 'Incident not found',
    incidentNotFoundDescription: 'The requested incident could not be found.',
    incidentDetails: 'Incident Details',
    createdBy: 'Created By',
    reported: 'Reported',
    resolved: 'Resolved',
    assignedResources: 'Assigned Resources',
    noResourcesAssigned: 'No resources assigned',
    activityLog: 'Activity Log',
    noActivityLogged: 'No activity logged',
    quickActions: 'Quick Actions',
    updateStatus: 'Update Status',
    assignResource: 'Assign Resource',
    addLogEntry: 'Add Log Entry',
    viewOnMap: 'View on Map',
    statistics: 'Statistics',
    resourcesAssigned: 'Resources Assigned',
    logEntries: 'Log Entries',
    duration: 'Duration',
    minutes: 'min',
    updateStatusModal: 'Update Status',
    newStatus: 'New Status',
    updating: 'Updating...',
    update: 'Update',
    assignResourceModal: 'Assign Resource',
    availableVehiclesModal: 'Available Vehicles',
    availablePersonnel: 'Available Personnel',
    addLogModal: 'Add Log Entry',
    message: 'Message',
    adding: 'Adding...',
    addLog: 'Add Log',
    priority: 'Priority',
    incidentId: 'Incident ID',
    resourceAssigned: 'resource assigned',
    resources: 'resources',
    resource: 'resource',
    resourcesAvailable: 'Available Resources',
    latestFive: 'Latest 5 Incidents',
    totalActiveIncidents: 'Total active incidents',

    // Enhanced assignment UI
    vehiclesSection: 'Vehicles',
    assigned: 'Assigned',
    assignedToThisIncident: 'Assigned to this incident',
    assignedToIncident: 'Assigned to incident',
    unavailable: 'Unavailable',
    outOfServiceStatus: 'Out of Service',
    resourceAssignedSuccessfully: 'Resource assigned successfully',
    resourceUnassignedSuccessfully: 'Resource unassigned successfully',
    confirmUnassignResource: 'Are you sure you want to unassign this resource?',
    unassignResource: 'Unassign Resource',

    // Patrol Zone Management
    createPatrolZone: 'Create Patrol Zone',
    editPatrolZone: 'Edit Patrol Zone',
    patrolZone: 'Patrol Zone',
    patrolZoneName: 'Patrol Zone Name',
    patrolZoneDescription: 'Description',
    patrolZonePriority: 'Priority',
    patrolZoneColor: 'Zone Color',
    drawPolygon: 'Draw Polygon',
    drawPolygonInstructions: 'Click on the map to start drawing the patrol zone boundary',
    patrolZoneCreated: 'Patrol zone created successfully',
    patrolZoneUpdated: 'Patrol zone updated successfully',
    patrolZoneDeleted: 'Patrol zone deleted successfully',
    deletePatrolZone: 'Delete Patrol Zone',
    confirmDeletePatrolZone: 'Are you sure you want to delete this patrol zone?',
    patrolZoneNamePlaceholder: 'Enter patrol zone name',
    patrolZoneDescriptionPlaceholder: 'Enter description for this patrol zone',
    lowPriorityPatrol: 'Low Priority',
    mediumPriorityPatrol: 'Medium Priority',
    highPriorityPatrol: 'High Priority',
    criticalPriorityPatrol: 'Critical Priority',
    selectColor: 'Select Color',
    assignedVehicles: 'Assigned Vehicles',
    ourAssignedVehicles: 'Our Assigned Vehicles',
    noVehiclesAssigned: 'No vehicles assigned to this patrol zone',
    patrolZoneDetails: 'Patrol Zone Details',
    coverage: 'Coverage Area',
    responsibleStation: 'Responsible Station',
    patrolZoneNameRequired: 'Patrol zone name is required',
    patrolZoneDescriptionError: 'Description error',
    createZone: 'Create Zone',
    updateZone: 'Update Zone',

    // Login page translations
    loginTitle: 'Incident Management System',
    loginSubtitle: 'Secure access to emergency response coordination',
    loginWelcome: 'Welcome Back',
    loginDescription: 'Sign in to access the incident management dashboard',
    loginButton: 'Sign In',
    demoCredentials: 'Demo Credentials',
    showDemoCredentials: 'Show Demo Credentials',
    hideDemoCredentials: 'Hide Demo Credentials',
    showCredentials: 'Show Credentials',
    hideCredentials: 'Hide Credentials',
    copyEmail: 'Copy Email',
    emailCopied: 'Email copied to clipboard',
    loginError: 'Login failed. Please check your credentials.',
    loginSuccess: 'Login successful! Redirecting...',
    welcomeToSystem:'Welcome to Aegis!',
    invalidCredentials: 'The credentials are wrong!',
    signIn:'Sign In',
    password:'Password',
    clickOnDemoToAutoFill:'Click on any credential to auto-fill the login form.',
    member:'Member',
    enterEmail:'Please enter your Email address',
    enterPassword:'Please enter your Password',

    // New incident detail fields
    generalInfo: 'General Info',
    subCategory: 'Sub Category',
    incidentResponsibleStation: 'Responsible Station',
    involvement: 'Involvement',
    incidentPersonnel: 'Personnel',
    fireTrucksNumber: 'Fire Trucks',
    firePersonnel: 'Fire Personnel',
    otherAgencies: 'Other Agencies',
    serviceActions: 'Service Actions',
    rescues: 'Rescues',
    rescuedPeople: 'Rescued People',
    rescueInformation: 'Rescue Information',
    commanders: 'Commanders',
    searchForCommander: 'Search for Commander',
    observations: 'Observations',
    noOfficersFound: 'No available officers found',
    optionalObservations: 'Optional observations...',
    noCommandersAssigned: 'No commanders assigned',
    select: 'Select',
    signal: 'Signal:',
    observationsForCommander: 'Observations for the commander...',
    describeOtherAgencies: 'Describe other agencies involved...',
    describeServiceActions: 'Describe actions performed...',
    rescueDetails: 'Rescue details...',
    casualtiesAccidentsBurns: 'Accidents, Damages, and Burns',
    casualties: 'Casualties',
    accidents: 'Accidents',
    injuries: 'Injuries',
    firemen: 'Firefighter',
    civilians: 'Civilian',
    nameAndCapacity: 'Name & Capacity',
    deaths: 'Deaths',
    burned: 'Burned',
    burnedArea: 'Burned Area',
    burnedItems: 'Burned Items',
    damages: 'Damages',
    ownerName: 'Owner Name',
    tenantName: 'Tenant Name',
    damageAmount: 'Damage Amount',
    savedProperty: 'Saved Property',
    incidentCause: 'Incident Cause',
  },
  el: {

    //Agencies
    coastGuard:'Λιμενικό Σώμα − Ελληνική Ακτοφυλακή',
    fireService:'Πυροσβεστικό Σώμα',
    ekab:'Ε.Κ.Α.Β.',
    police:'Ελληνική Αστυνομία',
    policeOfficer:'Αστυνομικοί',
    policeVehicles:'Οχήματα Αστυνομίας',
    coastGuardVehicles:'Οχήματα Λιμενικού Σώματος',
    coastGuardMembers:'Μέλη Λιμενικού Σώματος',
    EKABVehicles:'Οχήματα ΕΚΑΒ',
    EKABMembers:'Μέλη ΕΚΑΒ',

    // Navigation
    dashboard: 'Πίνακας Ελέγχου',
    cad: 'Computer-Aided Dispatch (CAD)',
    incidents: 'Συμβάντα',
    vehicles: 'Οχήματα',
    map: 'Χάρτης',
    stationNav: 'Σταθμός',
    roster: 'Βάρδιες Υπηρεσιών',
    settings: 'Ρυθμίσεις',
    logout: 'Αποσύνδεση',
    systemOnline:'Το API είναι ενεργό',
    fireStationsPlural:'Πυροσβεστικοί Σταθμοί',
    Ports_Bases:'Σταθμοί & Βάσεις',
    ekabStations:'Σταθμοί ΕΚΑΒ', 
    managePersonnel: 'Διαχείριση Προσωπικού',

    //Pages
    fireTrucks: 'Οχήματα',
    maritimeIncidents: 'Συμβάντα',
    resourcesCapital: 'Πόροι',
    maritimeMap: 'Χάρτης',
    medicalCalls: 'Περιστατικά',
    incidentsPage:'Σελίδα Συμβάντων',
    coverageMap: 'Χάρτης',
    cadDescription: 'Προβολή και διαχείριση ενεργών συμβάντων που έχουν ανατεθεί στον σταθμό σας',
    noActiveIncidents: 'Δεν υπάρχουν ενεργά συμβάντα',
    selectIncidentToView: 'Επιλέξτε ένα συμβάν για να δείτε λεπτομέρειες',
    description: 'Περιγραφή',
    notes: 'Σημειώσεις',
    incidentLogs: 'Αρχεία Συμβάντων',
    selectIncidentToViewVehicles: 'Επιλέξτε ένα συμβάν για να δείτε τα ανατεθειμένα οχήματα',
    finished: 'Ολοκληρώθηκε',

    // Common
    from:'από',
    to: 'σε',
    or: 'ή',
    save: 'Αποθήκευση',
    cancel: 'Ακύρωση',
    delete: 'Διαγραφή',
    edit: 'Επεξεργασία',
    create: 'Δημιουργία',
    loading: 'Φόρτωση',
    error: 'Σφάλμα',
    success: 'Επιτυχία',
    confirm: 'Επιβεβαίωση',
    yes: 'Ναι',
    no: 'Όχι',
    allStatuses: 'Όλες οι Καταστάσεις',
    allStations: 'Όλοι οι Σταθμοί',
    station: 'Σταθμός',
    welcomeBack: 'Καλώς ήρθατε',
    fleet:'Στόλου',
    onDuty:'Σε Υπηρεσία',
    vessels: 'Σκάφη',
    nauticalmiles:'ναυτικά μίλια',
    outOfnumber: 'Από',
    total:"συνολικά",
    crew: "Πλήρωμα",
    Unknown:"Άγνωστη",
    maritimePicture: 'Ναυτική Εικόνα — Ελλάδα',
    onAllStations: 'Σε Όλες τις Υπηρεσίες',
    recentActivity: 'Τελευταίες Ενεργειες',
    newIncidentReport: 'Νέα Αναφορά Συμβάντος',
    hospital: 'Νοσοκομείο',
    of: '',
    hospitals: 'Νοσοκομεία',
    hospitalsOff: 'Νοσοκομείων',
    contactUnit: 'Επικοινωνία με τη μονάδα',
    category:'Κατηγορία',
    add: 'Προσθήκη',
    information: 'Πληροφορίες',
    injury: 'Τραυματισμού',
    death: 'Θανάτου',
    noInjuries: 'Δεν έχουν καταγραφεί τραυματισμοί',
    noDeaths: 'Δεν έχουν καταγραφεί θάνατοι',

    //Weather
    weatherConditions: 'Καιρικές Συνθήκες',
    wind: 'Άνεμος',
    waveHeight: 'Ύψος Κύματος',
    temperature: 'Θερμοκρασία',
    visibility: 'Ορατότητα',
    seaState: 'Κατάσταση Θάλασσας',
    knots:'κόμβοι',
    knot:'κόμβος',
    
    // Weather Forecast Page
    weatherForecast: 'Πρόγνωση Καιρού',
    currentWeather: 'Τρέχων Καιρός',
    hourlyForecast: 'Ωριαία Πρόγνωση',
    dailyForecast: 'Ημερήσια Πρόγνωση',
    marineConditions: 'Ναυτικές Συνθήκες',
    feelsLike: 'Αίσθηση',
    humidity: 'Υγρασία',
    pressure: 'Πίεση',
    cloudCover: 'Νεφοκάλυψη',
    uvIndex: 'Δείκτης UV',
    precipitation: 'Κατακρημνίσματα',
    precipitationProbability: 'Πιθανότητα Βροχής',
    gusts: 'Ριπές Ανέμου',
    seaTemperature: 'Θερμοκρασία Θάλασσας',
    swellHeight: 'Ύψος Κυματισμού',
    waves: 'Κύματα',
    filters: 'Φίλτρα',
    viewMode: 'Λειτουργία Προβολής',
    current: 'Τρέχων',
    hourly: 'Ωριαία',
    
    // Map Filters
    mapFilters: 'Φίλτρα Χάρτη',
    showFilters: 'Εμφάνιση Φίλτρων',
    hideFilters: 'Απόκρυψη Φίλτρων',
    resetFilters: 'Επαναφορά Φίλτρων',
    applyFilters: 'Εφαρμογή Φίλτρων',
    filterByAgency: 'Φιλτράρισμα ανά Υπηρεσία',
    showIncidents: 'Εμφάνιση Συμβάντων',
    showVehicles: 'Εμφάνιση Οχημάτων',
    showStations: 'Εμφάνιση Σταθμών',
    showBoundaries: 'Εμφάνιση Ορίων',
    showHydrants: 'Εμφάνιση Κρουνών',
    showPatrolZones: 'Εμφάνιση Τομέων Περιπολίας',
    createPatrolZones: 'Δημιουργία Τομέων Περιπολίας',
    showOtherAgencyStations: 'Εμφάνιση Σταθμών Άλλων Υπηρεσιών',
    filterSettings: 'Ρυθμίσεις Φίλτρων',
    visibilityControls: 'Έλεγχοι Ορατότητας',
    incidentsFilterDescription: 'Εμφάνιση ή απόκρυψη συμβάντων στον χάρτη',
    vehiclesFilterDescription: 'Εμφάνιση ή απόκρυψη οχημάτων στον χάρτη',
    fireStationsFilterDescription: 'Εμφάνιση ή απόκρυψη πυροσβεστικών σταθμών στον χάρτη',
    fireStationBoundariesFilterDescription: 'Εμφάνιση ή απόκρυψη ορίων πυροσβεστικών σταθμών στον χάρτη',
    fireHydrantsFilterDescription: 'Εμφάνιση ή απόκρυψη πυροσβεστικών κρουνών στον χάρτη',
    policeStations: 'Αστυνομικοί Σταθμοί',
    policeStationsFilterDescription: 'Εμφάνιση ή απόκρυψη αστυνομικών σταθμών στον χάρτη',
    coastGuardStations: 'Σταθμοί Λιμενικού',
    coastGuardStationsFilterDescription: 'Εμφάνιση ή απόκρυψη σταθμών λιμενικού στον χάρτη',
    shipsFilterDescription: 'Εμφάνιση ή απόκρυψη πλοίων στον χάρτη',
    ambulancesFilterDescription: 'Εμφάνιση ή απόκρυψη ασθενοφόρων στον χάρτη',
    hospitalsFilterDescription: 'Εμφάνιση ή απόκρυψη νοσοκομείων στον χάρτη',
    availableFilters: 'Διαθέσιμα Φίλτρα',
    filtersDescription: 'Επιλέξτε ποια στοιχεία θέλετε να εμφανίζονται στον χάρτη',
    showAll: 'Εμφάνιση Όλων',
    hideAll: 'Απόκρυψη Όλων',
    apply: 'Εφαρμογή',
    daily: 'Ημερήσια',
    timePeriod: 'Χρονική Περίοδος',
    tomorrow: 'Αύριο',
    next3Days: 'Επόμενες 3 Ημέρες',
    next7Days: 'Επόμενες 7 Ημέρες',
    customRange: 'Προσαρμοσμένο Εύρος',
    startDate: 'Ημερομηνία Έναρξης',
    endDate: 'Ημερομηνία Λήξης',
    showMarineData: 'Εμφάνιση Ναυτικών Δεδομένων',
    locationRequired: 'Απαιτείται Τοποθεσία',
    enableLocationForWeather: 'Ενεργοποιήστε την τοποθεσία για δεδομένα καιρού',
    loadingWeatherData: 'Φόρτωση δεδομένων καιρού...',
    weatherLoadError: 'Σφάλμα φόρτωσης δεδομένων καιρού',
    temperatureMax: 'Μέγιστη Θερμοκρασία',
    temperatureMin: 'Ελάχιστη Θερμοκρασία',
    vehicleSingle: 'Όχημα',
    automatically: 'αυτόματα',
    changed: 'άλλαξε',
    theStatus: 'Η κατάσταση',
    
    // Wind Direction translations
    windN: 'Β',
    windNNE: 'ΒΒΑ',
    windNE: 'ΒΑ',
    windENE: 'ΑΒΑ',
    windE: 'Α',
    windESE: 'ΑΝΑ',
    windSE: 'ΝΑ',
    windSSE: 'ΝΝΑ',
    windS: 'Ν',
    windSSW: 'ΝΝΔ',
    windSW: 'ΝΔ',
    windWSW: 'ΔΝΔ',
    windW: 'Δ',
    windWNW: 'ΔΒΔ',
    windNW: 'ΒΔ',
    windNNW: 'ΒΒΔ',
    
    // Sea State translations
    seaStateCalm: 'Ήρεμη (Γαλήνια)',
    seaStateCalmRippled: 'Ήρεμη (Κυματάκια)',
    seaStateSmooth: 'Λεία',
    seaStateModerate: 'Μέτρια',
    seaStateRough: 'Τραχιά',
    seaStateVeryRough: 'Πολύ Τραχιά',
    seaStateHigh: 'Υψηλή',
    seaStateVeryHigh: 'Πολύ Υψηλή',
    seaStatePhenomenal: 'Φαινομενική',

    // Weather Descriptions
    weatherClear: 'Αίθριος',
    weatherMainlyClear: 'Κυρίως αίθριος',
    weatherPartlyCloudy: 'Μερικώς νεφελώδης',
    weatherOvercast: 'Συννεφιασμένος',
    weatherFog: 'Ομίχλη',
    weatherDrizzleLight: 'Ελαφρύ ψιλόβροχο',
    weatherDrizzleModerate: 'Μέτριο ψιλόβροχο',
    weatherDrizzleDense: 'Πυκνό ψιλόβροχο',
    weatherRainLight: 'Ελαφρά βροχή',
    weatherRainModerate: 'Μέτρια βροχή',
    weatherRainHeavy: 'Έντονη βροχή',
    weatherSnowLight: 'Ελαφρό χιόνι',
    weatherSnowModerate: 'Μέτριο χιόνι',
    weatherSnowHeavy: 'Έντονο χιόνι',
    weatherShowers: 'Μπόρες',
    weatherThunderstorm: 'Καταιγίδα',

    // Incident Management
    incident:'Συμβάν',
    newIncident: 'Νέο Συμβάν',
    incidentType: 'Τύπος Συμβάντος',
    incidentDescription: 'Περιγραφή',
    incidentAddress: 'Διεύθυνση',
    incidentPriority: 'Προτεραιότητα',
    incidentNotes: 'Σημειώσεις',
    incidentLocation: 'Διεύθυνση Συμβάντος',
    createIncident: 'Δημιουργία Συμβάντος',
    creating: 'Δημιουργία...',
    assignToStation: 'Ανάθεση σε Σταθμό',
    selectIncidentType: 'Επιλέξτε τύπο συμβάντος',
    selectStation: 'Επιλέξτε σταθμό',
    briefDescription: 'Σύντομη περιγραφή του συμβάντος...',
    briefNotes: 'Σύντομες σημειώσεις για το συμβάν...',
    streetAddress: 'Διεύθυνση ή σημείο αναφοράς',
    specialInstructions: 'Ειδικές οδηγίες, κίνδυνοι ή σημαντικές πληροφορίες για τους πυροσβέστες...',

    // Detailed Address Fields
    fullAddress: 'Πλήρης Διεύθυνση',
    street: 'Οδός',
    streetNumber: 'Αριθμός',
    postalCode: 'Ταχυδρομικός Κώδικας',
    country: 'Χώρα',
    optional: 'προαιρετικό',

    // Incident Types
    fire: 'Πυρκαγιά',
    medicalEmergency: 'Ιατρική Επείγουσα Κατάσταση',
    vehicleAccident: 'Τροχαίο Ατύχημα',
    hazmat: 'Επικίνδυνα Υλικά',
    rescue: 'Διάσωση',
    other: 'Άλλο',

    // Priority Levels
    lowPriority: 'Χαμηλή Προτεραιότητα',
    normalPriority: 'Κανονική Προτεραιότητα',
    highPriority: 'Υψηλή Προτεραιότητα',
    criticalPriority: 'Κρίσιμη Προτεραιότητα',

    // Incident Status
    created: 'ΔΗΜΙΟΥΡΓΗΘΗΚΕ',
    createdAt: 'Δημιουργήθηκε στις',
    onGoing: 'ΣΕ ΕΞΕΛΙΞΗ',
    partialControl: 'ΜΕΡΙΚΟΣ ΕΛΕΓΧΟΣ',
    controlled: 'ΥΠΟ ΕΛΕΓΧΟ',
    fullyControlled: 'ΠΛΗΡΗΣ ΕΛΕΓΧΟΣ',
    closed: 'ΚΛΕΙΣΤΟ',

    // Incident Closure
    cancelled: 'ΑΚΥΡΟ',
    duplicate: 'Διπλότυπο',
    falseAlarm: 'ΨΕΥΔΗΣ ΑΝΑΓΓΕΛΙΑ',
    closing: 'Κλείσιμο',
    closeIncident: 'Κλείσιμο Συμβάντος',
    closureReason: 'Λόγος Κλεισίματος',
    closedBy: 'Κλείστηκε από',
    closedAt: 'Έκλεισε στις',
    action: 'ΕΝΕΡΓΕΙΑ',
    withoutAction: 'ΑΝΕΥ ΕΝΕΡΓΕΙΑΣ',
    preArrival: 'ΠΡΟ ΑΦΙΞΕΩΣ',
    incidentClosedSuccessfully: 'Το συμβάν έκλεισε επιτυχώς',
    incidentReopenedSuccessfully: 'Το συμβάν άνοιξε ξανά επιτυχώς',
    reopenIncident: 'Επαναφορά Συμβάντος',
    continue: 'Συνέχεια',
    confirmCloseIncident: 'Επιβεβαίωση Κλεισίματος Συμβάντος',
    confirmCloseIncidentMessage: 'Είστε σίγουροι ότι θέλετε να κλείσετε αυτό το συμβάν;',
    goBack: 'Επιστροφή',
    confirmReopenIncident: 'Επιβεβαίωση Επαναφοράς Συμβάντος',
    confirmReopenIncidentMessage: 'Είστε σίγουροι ότι θέλετε να ανοίξετε ξανά αυτό το συμβάν;',
    reopening: 'Επαναφορά...',

    // Vehicle Status
    notified: 'Ειδοποίηση',
    enRoute: 'Καθοδόν',
    onScene: 'Άφιξη',
    completed: 'Ολοκληρώθηκε',
    dispatched: 'Αποστάλθηκε',
    statusChanged: 'Αλλαγή Κατάστασης',
    priorityChanged: 'Αλλαγή Προτεραιότητας',
    incidentCreated: 'Δημιουργία Συμβάντος',
    incidentUpdated: 'Ενημέρωση Συμβάντος',

    // User Interface
    darkMode: 'Σκοτεινό Θέμα',
    lightMode: 'Φωτεινό Θέμα',
    language: 'Γλώσσα',
    english: 'English',
    greek: 'Ελληνικά',

    // System
    incidentManagementSystem: 'Σύστημα Διαχείρισης Συμβάντων',
    pleaseLogIn: 'Παρακαλώ συνδεθείτε για να συνεχίσετε',
    logInWithSupabase: 'Σύνδεση με Supabase',
    demoMode: 'Λειτουργία Demo - Αλλαγή Χρήστη:',
    switchUser: 'Αλλαγή Χρήστη',
    dispatcher: 'Διαχειριστής',
    firefighter: 'Πυροσβέστης',

    // Location
    latitude: 'Γεωγραφικό Πλάτος',
    longitude: 'Γεωγραφικό Μήκος',
    automaticallyAssigned: 'Αυτόματη ανάθεση σε',
    noStationFound: 'Δεν βρέθηκε πυροσβεστικός σταθμός για αυτή τη θέση. Παρακαλώ επιλέξτε χειροκίνητα.',
    unableToDetermineStation: 'Αδυναμία αυτόματου προσδιορισμού σταθμού. Παρακαλώ επιλέξτε χειροκίνητα.',
    stationAssignmentInfo: 'Η ανάθεση σταθμού θα καθοριστεί αυτόματα όταν ορίσετε τη θέση του συμβάντος',
    addressDetails: 'Λεπτομέρειες Διεύθυνσης',
    caller: 'Τηλέφωνο Καλούντων',
    callerPhoneNumbers: 'Τηλέφωνα Καλούντων',
    callerInformation: 'Στοιχεία Καλούντος',
    addCaller: 'Προσθήκη Καλούντος',
    phoneNumber: 'Αριθμός Τηλεφώνου',
    callerName: 'Όνομα Καλούντος',
    additionalNotes: 'Επιπλέον Σημειώσεις',
    anonymous: 'Ανώνυμος',
    more: 'περισσότερα',
    streetAndNumber: 'Οδός & Αριθμός',
    zipCode: 'Ταχυδρομικός Αριθμός',
    viewAll:'Προβολή Όλων',
    calledAt:'ΏΡΑ ΤΗΛΕΦΏΝΟΥ',
    callers: "Καλούντες",
    viewCallers: "Προβολή Καλούντων",
    unknownCaller: "Άγνωστος Καλών",
    noCallersRecorded: "Δεν έχουν καταγραφεί καλούντες για αυτό το συμβάν",

    // Validation Messages
    enterIncidentType: 'Παρακαλώ εισάγετε τύπο συμβάντος',
    setIncidentLocation: 'Παρακαλώ ορίστε τη θέση του συμβάντος στο χάρτη',
    selectFireStation: 'Παρακαλώ επιλέξτε πυροσβεστικό σταθμό για αυτό το συμβάν',

    // Page titles
    vehiclesPageTitle: 'Διαχείριση Οχημάτων',
    stationManagementTitle: 'Διαχείριση Σταθμού',
    rosterPageTitle: 'Κατάλογος Προσωπικού',
    mapViewTitle: 'Προβολή Χάρτη',
    incidentsPageTitle: 'Διαχείριση Συμβάντων',
    settingsPageTitle: 'Ρυθμίσεις',

    // Form elements
    name: 'Όνομα',
    rank: 'Βαθμός',
    badgeNumber: 'Αριθμός Μητρώου',	
    callsign: 'Κωδικός Κλήσης',
    plateNumber: 'Αριθμός Πινακίδας',
    vehicleType: 'Τύπος Οχήματος',
    personnel: 'Άτομα',
    addPersonnel: 'Προσθήκη Προσωπικού',
    editPersonnel: 'Επεξεργασία Προσωπικού',
    removePersonnel: 'Αφαίρεση Προσωπικού',
    vehicleStatus: 'Κατάσταση Οχήματος',
    available: 'Διαθέσιμα',
    inUse: 'Σε Χρήση',
    outOfService: 'Εκτός Λειτουργίας',
    maintenance: 'Συντήρηση',

    // Status and messages
    noDataFound: 'Δεν βρέθηκαν δεδομένα',
    loadingData: 'Φόρτωση δεδομένων...',
    accessDenied: 'Άρνηση πρόσβασης',
    permissionDenied: 'Άρνηση άδειας',
    noVehiclesFound: 'Δεν βρέθηκαν οχήματα',
    noPersonnelFound: 'Δεν βρέθηκε προσωπικό',
    noIncidentsFound: 'Δεν βρέθηκαν συμβάντα',
    dataLoadError: 'Σφάλμα φόρτωσης δεδομένων',

    // Actions
    addVehicle: 'Προσθήκη Οχήματος',
    editVehicle: 'Επεξεργασία Οχήματος',
    removeVehicle: 'Αφαίρεση Οχήματος',
    assignVehicle: 'Ανάθεση Οχήματος',
    unassignVehicle: 'Αποδέσμευση Οχήματος',
    assignedToPatrolZone: 'Ανατεθειμένο σε Τομέα Περιπολίας',
    selectVehicleToAssign: 'Επιλέξτε όχημα για ανάθεση σε αυτή τη ζώνη περιπολίας',
    noAvailableVehicles: 'Δεν υπάρχουν διαθέσιμα οχήματα',
    vehicleAssignedSuccessfully: 'Το όχημα ανατέθηκε επιτυχώς',
    errorAssigningVehicle: 'Σφάλμα κατά την ανάθεση οχήματος',
    assigning: 'Ανάθεση...',
    viewDetails: 'Προβολή Λεπτομερειών',
    refresh: 'Ανανέωση',
    search: 'Αναζήτηση',
    filter: 'Φίλτρο',
    clear: 'Καθαρισμός',
    vehicleFilter: 'Φίλτρο Οχημάτων',
    showAllStations: 'Εμφάνιση Όλων των Σταθμών',
    responsibleStationOnly: 'Σταθμός Ευθύνης',
    showResponsibleStationOnly: 'Εμφάνιση μόνο σταθμού αρμοδιότητας',
    selectStations: 'Επιλογή σταθμών...',
    stationsSelected: 'σταθμός/οί επιλεγμένος/οι',
    showingVehiclesFromAllStations: 'Εμφάνιση οχημάτων από όλους τους σταθμούς',
    showingVehiclesFromMyStation: 'Εμφάνιση οχημάτων από τον σταθμό μου',
    
    // Sorting and filtering
    filtersAndSorting: 'Φίλτρα & Ταξινόμηση',
    sortBy: 'Ταξινόμηση Κατά',
    sortOrder: 'Σειρά Ταξινόμησης',
    sortByStatus: 'Προτεραιότητα Κατάστασης',
    sortByResources: 'Αριθμός Πόρων',
    sortByDate: 'Ημερομηνία & Ώρα',
    participationType: 'Τύπος Συμμετοχής',
    allTypes: 'Όλοι οι Τύποι',
    primaryIncidents: 'Συμβάντα Σταθμού',
    reinforcementIncidents: 'Ενίσχυση',
    reinforcement: 'Ενίσχυση',
    lowToHigh: 'Από Χαμηλό σε Υψηλό',
    highToLow: 'Από Υψηλό σε Χαμηλό',
    oldestFirst: 'Παλαιότερα Πρώτα',
    newestFirst: 'Νεότερα Πρώτα',

    // Table headers
    status: 'Κατάσταση',
    lastUpdate: 'Τελευταία Ενημέρωση',
    telemetry: 'Τηλεμετρία',
    fuelLevel: 'Στάθμη Καυσίμου',
    waterLevel: 'Στάθμη Νερού',
    batteryVoltage: 'Τάση Μπαταρίας',
    pumpPressure: 'Πίεση Αντλίας',
    location: 'Τοποθεσία',
    assignedPersonnel: 'Ανατεθειμένο Προσωπικό',
    vehicleId: 'Κωδικός Οχήματος',
    type: 'Τύπος',
    model: 'Μοντέλο',
    year: 'Έτος',


    // Hellenic Coast Guard Vehicle / Vessel Types
    patrolBoat: 'Περιπολικό Σκάφος',
    offshorePatrolVessel: 'Περιπολικό Ανοικτής Θαλάσσης',
    searchRescueBoat: 'Σκάφος Έρευνας και Διάσωσης',
    rigidInflatable: 'Φουσκωτό Σκάφος (RIB)',
    pollutionControl: 'Σκάφος Αντιμετώπισης Ρύπανσης',
    cghelicopter: 'Ελικόπτερο',
    cgairplane: 'Αεροσκάφος Επιτήρησης',
    patrolVehicle: 'Περιπολικό Όχημα Ξηράς',
    cgbus: 'Λεωφορείο Μεταφοράς Προσωπικού',

    // Vehicle types
    fireEngine: 'Πυροσβεστικό Όχημα',
    ladder: 'Κλιμακοφόρο',
    rescueVehicle: 'Διασωστικό',
    ambulance: 'Ασθενοφόρο',
    command: 'Όχημα Διοίκησης',
    tanker: 'Υδροφόρο',
    fireBoat: 'Πυροσβεστικό Σκάφος',
    hazmatTruck: 'Όχημα Αντιμετώπισης Επικίνδυνων Υλικών (HAZMAT)',
    support: 'Όχημα Υποστήριξης',
    foodtruck: 'Τροφός',
    fcbus: 'Λεωφορείο Μεταφοράς Προσωπικού',
    petroltruck: 'Όχημα Καυσίμων',

    // Police Vehicle Types
    policePatrolCar: 'Περιπολικό Αυτοκίνητο',
    policeMotorcycle: 'Αστυνομική Μοτοσικλέτα',
    policeVan: 'Αστυνομικό Φορτηγάκι',
    policeBus: 'Αστυνομικό Λεωφορείο',
    policeHelicopter: 'Αστυνομικό Ελικόπτερο',
    policeBoat: 'Αστυνομικό Σκάφος',
    policeCommandVehicle: 'Όχημα Διοίκησης Αστυνομίας',
    policeSpecialOperations: 'Όχημα Ειδικών Επιχειρήσεων',
    policeTrafficEnforcement: 'Όχημα Τροχαίας',
    policeK9Unit: 'Όχημα Κυνηγετικής Μονάδας',
    policeBombSquad: 'Όχημα Εξουδετέρωσης Εκρηκτικών',
    policeForensics: 'Όχημα Εγκληματολογικών Ερευνών',

    // EKAB Vehicle Types
    basicAmbulance: 'Βασικό Ασθενοφόρο',
    advancedAmbulance: 'Ασθενοφόρο Προηγμένης Υποστήριξης Ζωής',
    intensiveCareAmbulance: 'Ασθενοφόρο Εντατικής Θεραπείας',
    neonatalAmbulance: 'Νεογνικό Ασθενοφόρο',
    ekabMotorcycle: 'Μοτοσικλέτα ΕΚΑΒ',
    ekabHelicopter: 'Ελικόπτερο ΕΚΑΒ',
    ekabCommandVehicle: 'Όχημα Διοίκησης ΕΚΑΒ',
    ekabMobileICU: 'Κινητή Μονάδα Εντατικής Θεραπείας',
    ekabRescueVehicle: 'Διασωστικό Όχημα ΕΚΑΒ',
    ekabSupplyVehicle: 'Όχημα Εφοδιασμού ΕΚΑΒ',

    //Hellenic Fire Service Ranks
    lieutenantFireGeneral: 'Αντιστράτηγος',               // Lieutenant Fire General
    majorFireGeneral: 'Υποστράτηγος',                    // Major Fire General
    fireBrigadier: 'Αρχιπύραρχος',                       // Fire Brigadier
    fireColonel: 'Πύραρχος',                             // Fire Colonel
    lieutenantFireColonel: 'Αντιπύραρχος',               // Lieutenant Fire Colonel
    fireMajor: 'Επιπυραγός',                             // Fire Major
    fireCaptain: 'Πυραγός',                              // Fire Captain
    fireLieutenant: 'Υποπυραγός',                        // Fire Lieutenant
    fireSecondLieutenant: 'Ανθυποπυραγός',               // Fire Second Lieutenant
    fireWarrantOfficer: 'Πυρονόμος',                     // Fire Warrant Officer
    fireSergeantAcademy: 'Αρχιπυροσβέστης',       // Academy-trained Fire Sergeant
    fireSergeantNonAcademy: 'Αρχιπυροσβέστης Μη Παραγωγικής Σχολής', // Non-academy Fire Sergeant
    seniorFirefighterAcademy: 'Υπαρχιπυροσβέστης', // Non-academy Senior Firefighter
    seniorFirefighterNonAcademy: 'Υπαρχιπυροσβέστης Μη Παραγωγικής Σχολής', // Non-academy Senior Firefighter
    firefighterRank: "Πυροσβέστης",

    // Hellenic Coast Guard Ranks
    // Officer Grade Structure
    viceAdmiral: 'Αντιναύαρχος',            // Vice Admiral (OF-8)
    rearAdmiral: 'Υποναύαρχος',             // Rear Admiral (OF-7)
    commodore: 'Αρχιπλοίαρχος',             // Commodore (OF-6)
    captain: 'Πλοίαρχος',                   // Captain (OF-5)
    commander: 'Αντιπλοίαρχος',             // Commander (OF-4)
    ltCommander: 'Πλωτάρχης',               // Lieutenant Commander (OF-3)
    lieutenant: 'Υποπλοίαρχος',             // Lieutenant (OF-2)
    subLieutenant: 'Ανθυποπλοίαρχος',       // Lieutenant Junior Grade (OF-1)
    ensign: 'Σημαιοφόρος',                  // Ensign (OF-1)

    // NCO / Enlisted Rank Structure
    warrantOfficer: 'Ανθυπασπιστής',        // Warrant Officer (OR-9)
    chiefPettyOfficer: 'Αρχικελευστής',     // Chief Petty Officer (OR-8)
    pettyOfficer1st: 'Επικελευστής',        // Petty Officer 1st Class (OR-7)
    pettyOfficer2nd: 'Κελευστής',           // Petty Officer 2nd Class (OR-6)
    coastGuardRank: 'Λιμενοφύλακας',            // Coast Guardsman (OR-1/OR-5 gap filler)

    // Hellenic Police Ranks
    policeLieutenantGeneral: 'Αντιστράτηγος',
    policeMajorGeneral: 'Υποστράτηγος',
    policeBrigadierGeneral: 'Ταξίαρχος',
    policeDirector: 'Αστυνομικός Διευθυντής',
    policeDeputyDirector: 'Αστυνομικός Υποδιευθυντής',
    policeCaptainI: 'Αστυνόμος Α΄',           // (aka Police Major)
    policeCaptainII: 'Αστυνόμος Β΄',
    policeLieutenantI: 'Υπαστυνόμος Α΄',
    policeLieutenantII: 'Υπαστυνόμος Β΄',
    policeDeputyLieutenant: 'Ανθυπαστυνόμος', // (aka Police Warrant Officer)

    policeSergeantInvestigativeExam: 'Αρχιφύλακας (Ανακριτικός Υπάλληλος – Με εξετάσεις)',
    policeSergeantInvestigative: 'Αρχιφύλακας (Ανακριτικός Υπάλληλος)',
    policeSergeantNonInvestigative: 'Αρχιφύλακας (Μη ανακριτικός υπάλληλος)',
    policeDeputySergeantInvestigative: 'Υπαρχιφύλακας (Ανακριτικός Υπάλληλος)',
    policeDeputySergeantNonInvestigative: 'Υπαρχιφύλακας (Μη ανακριτικός υπάλληλος)',
    policeConstable: 'Αστυφύλακας',

    // EKAB Roles (Hellenic National Center for Emergency Care)

    // Senior Management / Administration
    ekabPresident: 'Πρόεδρος ΕΚΑΒ',
    ekabVicePresident: 'Αντιπρόεδρος ΕΚΑΒ',
    ekabRegionalDirector: 'Περιφερειακός Διευθυντής',
    ekabDepartmentHead: 'Προϊστάμενος Τμήματος',

    // Medical & Paramedical Staff
    ekabEmergencyDoctor: 'Ιατρός Επειγόντων',
    ekabNurse: 'Νοσηλευτής/Νοσηλεύτρια',
    ekabParamedicSupervisor: 'Επόπτης Διασωστών',
    ekabParamedic: 'Διασώστης – Πλήρωμα Ασθενοφόρου',
    ekabAmbulanceDriver: 'Οδηγός Ασθενοφόρου',

    // Specialized Units
    ekabHelicopterDoctor: 'Ιατρός Ελικοπτέρου',
    ekabHelicopterParamedic: 'Διασώστης Ελικοπτέρου',
    ekabSpecialRescueTeam: 'Ειδική Ομάδα Διάσωσης (ΕΚΑΒ-ΕΟΔ)',

    // Entry / Support Roles
    ekabCallCenterOperator: 'Τηλεφωνητής – Κέντρο Επιχειρήσεων',
    ekabAdministrativeStaff: 'Διοικητικός Υπάλληλος',
    ekabLogisticsSupport: 'Υποστηρικτικό Προσωπικό / Τεχνική Υπηρεσία',


    // Station management specific
    managePersonnelAndVehicles: 'Διαχείριση προσωπικού και οχημάτων για την Υπηρεσία:',
    youDontHavePermission: 'Δεν έχετε άδεια να διαχειριστείτε τους πόρους του σταθμού.',
    active: 'Ενεργός',
    inactive: 'Ανενεργός',
    activeStatus: 'Ενεργή Κατάσταση',
    inactiveStatus: 'Ανενεργή Κατάσταση',
    unknownPriority: 'Άγνωστη Προτεραιότητα',
    createdDate: 'Ημερομηνία Δημιουργίας',
    activeAssignments: 'Ενεργές Αναθέσεις',
    noPersonnelAssigned: 'Δεν έχει ανατεθεί προσωπικό',
    selectRank: 'Επιλέξτε βαθμό',
    selectPersonnelToAssign: 'Επιλέξτε προσωπικό για ανάθεση σε',
    alreadyAssignedToVehicle: 'Ήδη ανατεθειμένο σε όχημα',
    assignPersonnelToVehicle: 'Ανάθεση Προσωπικού σε Όχημα',
    assign: 'Ανάθεση',
    fireTruck: 'Πυροσβεστικό Όχημα',
    engine: 'Μηχανή',
    unassign: 'Αποδέσμευση',
    dismiss: 'Απόλυση',
    confirmUnassign: 'Είστε σίγουροι ότι θέλετε να αποδεσμεύσετε αυτόν τον πόρο;',

    // Map and location
    mapControls: 'Χειριστήρια Χάρτη',
    zoomIn: 'Μεγέθυνση',
    zoomOut: 'Σμίκρυνση',
    centerMap: 'Κεντράρισμα Χάρτη',
    fullscreen: 'Πλήρης Οθόνη',
    coordinates: 'Συντεταγμένες',
    address: 'Διεύθυνση',
    selectLocation: 'Επιλογή Τοποθεσίας',
    currentLocation: 'Τρέχουσα Τοποθεσία',
    realTimeView: 'Προβολή σε πραγματικό χρόνο συμβάντων, οχημάτων και σταθμών',
    legend: 'Υπόμνημα',
    activeIncidents: 'Ενεργά Συμβάντα',
    fireStations: 'Πυροσβεστικοί Σταθμοί',
    stationDistricts: 'Περιοχές Σταθμών',
    hide: 'Απόκρυψη',
    show: 'Εμφάνιση',
    fireStationsMap: "Πυροσβεστικών Υπηρεσιών Και Κλιμακίων",
    fireStationDistricts: 'Περιοχές Πυροσβεστικών Σταθμών',
    dataLoadingIssues: 'Προβλήματα Φόρτωσης Δεδομένων',
    vehiclesOnMap: 'Οχήματα στον Χάρτη',
    fireStation: 'Πυροσβεστικός Σταθμός',
    region: 'Περιοχή',
    city: 'Πόλη',
    area: 'Έκταση',
    district: 'Περιφέρεια',
    department: 'Τμήμα',
    noDescription: 'Χωρίς περιγραφή',
    noNotes: 'Χωρίς σημειώσεις',
    id: 'Κωδικός Συμβάντος',
    latestFive: 'Τελευταιά 5 Συμβάντα',
    totalActiveIncidents: 'Σύνολο Ενεργών Συμβάντων',
    hydrant: 'Πυροσβεστικός Κρουνός',
    hydrants: 'Πυροσβεστικών Κρουνών',

    // Notifications and alerts
    vehicleAdded: 'Το όχημα προστέθηκε επιτυχώς',
    vehicleUpdated: 'Το όχημα ενημερώθηκε επιτυχώς',
    vehicleRemoved: 'Το όχημα αφαιρέθηκε επιτυχώς',
    personnelAdded: 'Το προσωπικό προστέθηκε επιτυχώς',
    personnelUpdated: 'Το προσωπικό ενημερώθηκε επιτυχώς',
    personnelRemoved: 'Το προσωπικό αφαιρέθηκε επιτυχώς',
    settingsSaved: 'Οι ρυθμίσεις αποθηκεύτηκαν επιτυχώς',
    operationFailed: 'Η λειτουργία απέτυχε',
    connectionLost: 'Η σύνδεση χάθηκε',
    reconnecting: 'Επανασύνδεση...',

    // Time and dates
    today: 'Σήμερα',
    yesterday: 'Χθες',
    thisWeek: 'Αυτή την Εβδομάδα',
    lastWeek: 'Την Προηγούμενη Εβδομάδα',
    thisMonth: 'Αυτόν τον Μήνα',
    lastMonth: 'Τον Προηγούμενο Μήνα',
    never: 'Ποτέ',
    unknown: 'Άγνωστο',

    // Settings page
    settingsDescription: 'Διαμόρφωση προτιμήσεων συστήματος και ρυθμίσεων χρήστη',
    notifications: 'Ειδοποιήσεις',
    system: 'Σύστημα',
    security: 'Ασφάλεια',
    emailAlerts: 'Ειδοποιήσεις Email',
    emailAlertsDescription: 'Λήψη ειδοποιήσεων συμβάντων μέσω email',
    smsAlerts: 'Ειδοποιήσεις SMS',
    smsAlertsDescription: 'Λήψη κρίσιμων ειδοποιήσεων μέσω SMS',
    pushNotifications: 'Push Ειδοποιήσεις',
    pushNotificationsDescription: 'Ειδοποιήσεις προγράμματος περιήγησης',
    defaultStation: 'Προεπιλεγμένος Σταθμός',
    mapProvider: 'Πάροχος Χάρτη',
    refreshInterval: 'Διάστημα Ανανέωσης',
    refreshIntervalSeconds: 'Διάστημα Ανανέωσης (δευτερόλεπτα)',
    enableAutoAssignment: 'Ενεργοποίηση Αυτόματης Ανάθεσης',
    sessionTimeout: 'Λήξη Συνεδρίας',
    sessionTimeoutMinutes: 'Λήξη Συνεδρίας (λεπτά)',
    passwordExpiry: 'Λήξη Κωδικού',
    passwordExpiryDays: 'Λήξη Κωδικού (ημέρες)',
    requireMFA: 'Απαίτηση Πολυπαραγοντικής Ταυτοποίησης',
    selectDefaultStation: 'Επιλέξτε Προεπιλεγμένο Σταθμό',
    googleMaps: 'Google Maps',
    openStreetMap: 'OpenStreetMap',
    mapbox: 'Mapbox',
    saveSettings: 'Αποθήκευση Ρυθμίσεων',

    // Empty states
    noVehiclesMessage: 'Δεν υπάρχουν εγγεγραμμένα οχήματα στο σύστημα.',
    noPersonnelMessage: 'Δεν υπάρχει προσωπικό ανατεθειμένο σε αυτόν τον σταθμό.',
    noIncidentsMessage: 'Δεν έχουν αναφερθεί συμβάντα.',
    noDataMessage: 'Δεν υπάρχουν διαθέσιμα δεδομένα για εμφάνιση.',
    addFirstVehicle: 'Προσθέστε το πρώτο σας όχημα για να ξεκινήσετε.',
    addFirstPersonnel: 'Προσθέστε το πρώτο μέλος προσωπικού για να ξεκινήσετε.',
    createFirstIncident: 'Δημιουργήστε την πρώτη αναφορά συμβάντος για να ξεκινήσετε.',

    // Vehicle-specific translations
    monitorVehicleStatus: 'Παρακολούθηση κατάστασης και τηλεμετρίας οχημάτων',
    clearFilters: 'Καθαρισμός Φίλτρων',
    vehicleCount: 'όχημα',
    vehiclesCount: 'οχήματα',
    noVehiclesForStation: 'Δεν βρέθηκαν οχήματα για τον Σταθμό',
    tryAdjustingFilters: 'Δοκιμάστε να προσαρμόσετε τα φίλτρα σας',
    noVehiclesRegistered: 'Δεν υπάρχουν εγγεγραμμένα οχήματα',
    failedToLoadVehicles: 'Αποτυχία φόρτωσης οχημάτων',
    vehicleCreatedSuccessfully: 'Το όχημα δημιουργήθηκε επιτυχώς',
    vehicleUpdatedSuccessfully: 'Το όχημα ενημερώθηκε επιτυχώς',
    fillRequiredFields: 'Παρακαλώ συμπληρώστε όλα τα απαιτούμενα πεδία',
    selectVehicleType: 'Επιλέξτε τύπο οχήματος',
    licensePlateNumber: 'Αριθμός πινακίδας κυκλοφορίας',
    waterCapacityLiters: 'Χωρητικότητα Νερού (Λίτρα)',
    addingVehicle: 'Προσθήκη...',
    errorCreatingVehicle: 'Σφάλμα Δημιουργίας Οχήματος',
    close: 'Κλείσιμο',
    retry: 'Επανάληψη',
    editVehicleTitle: 'Επεξεργασία οχήματος',
    fuel: 'Καύσιμο',
    water: 'Νερό',
    battery: 'Μπαταρία',
    pump: 'Αντλία',
    offline: 'Εκτός Σύνδεσης',
    busy: 'Απασχολημένο',

    // Incident-specific translations
    manageAndTrackIncidents: 'Διαχείριση και παρακολούθηση επειγόντων συμβάντων',
    newIncidentButton: 'Νέο Συμβάν',
    incidentNotFound: 'Το συμβάν δεν βρέθηκε',
    incidentNotFoundDescription: 'Το ζητούμενο συμβάν δεν μπόρεσε να βρεθεί.',
    incidentDetails: 'Λεπτομέρειες Συμβάντος',
    createdBy: 'Δημιουργήθηκε από',
    reported: 'Ειδοποίηση',
    resolved: 'Επιλύθηκε',
    assignedResources: 'Ανατεθειμένοι Πόροι',
    noResourcesAssigned: 'Δεν έχουν ανατεθεί πόροι',
    activityLog: 'Αρχείο Δραστηριότητας',
    noActivityLogged: 'Δεν έχει καταγραφεί δραστηριότητα',
    quickActions: 'Γρήγορες Ενέργειες',
    updateStatus: 'Ενημέρωση Κατάστασης',
    assignResource: 'Ανάθεση Πόρου',
    addLogEntry: 'Προσθήκη Καταχώρησης',
    viewOnMap: 'Προβολή στον Χάρτη',
    statistics: 'Στατιστικά',
    resourcesAssigned: 'Ανατεθειμένοι Πόροι',
    logEntries: 'Καταχωρήσεις Αρχείου',
    duration: 'Διάρκεια',
    minutes: 'λεπτά',
    updateStatusModal: 'Ενημέρωση Κατάστασης',
    newStatus: 'Νέα Κατάσταση',
    updating: 'Ενημέρωση...',
    update: 'Ενημέρωση',
    assignResourceModal: 'Ανάθεση Πόρου',
    availableVehiclesModal: 'Διαθέσιμα Οχήματα',
    availablePersonnel: 'Διαθέσιμο Προσωπικό',
    addLogModal: 'Προσθήκη Καταχώρησης',
    message: 'Μήνυμα',
    adding: 'Προσθήκη...',
    addLog: 'Προσθήκη Καταχώρησης',
    priority: 'Προτεραιότητα',
    incidentId: 'Κωδικός Συμβάντος',
    resourceAssigned: 'πόρος ανατέθηκε',
    resources: 'πόροι',
    resourcesAvailable: 'Διαθέσιμοι Πόροι',
    resource: 'πόρος',
    patrolCoverage: 'Κάλυψη Τομέων',

    // Enhanced assignment UI
    vehiclesSection: 'Οχήματα',
    assigned: 'Ανατεθειμένο',
    assignedToThisIncident: 'Ανατεθειμένο σε αυτό το συμβάν',
    assignedToIncident: 'Ανατεθειμένο στο συμβάν',
    unavailable: 'Μη διαθέσιμο',
    outOfServiceStatus: 'Εκτός Λειτουργίας',
    resourceAssignedSuccessfully: 'Ο πόρος ανατέθηκε επιτυχώς',
    resourceUnassignedSuccessfully: 'Ο πόρος αποσύρθηκε επιτυχώς',
    confirmUnassignResource: 'Είστε σίγουροι ότι θέλετε να αποσύρετε αυτόν τον πόρο;',
    unassignResource: 'Απόσυρση Πόρου',

    // Patrol Zone Management
    createPatrolZone: 'Δημιουργία Τομέα Περιπολίας',
    editPatrolZone: 'Επεξεργασία Τομέα Περιπολίας',
    patrolZone: 'Τομέας Περιπολίας',
    patrolZoneName: 'Όνομα Τομέα Περιπολίας',
    patrolZoneDescription: 'Περιγραφή',
    patrolZonePriority: 'Προτεραιότητα',
    patrolZoneColor: 'Χρώμα Τομέα',
    drawPolygon: 'Σχεδίαση Πολυγώνου',
    drawPolygonInstructions: 'Κάντε κλικ στον χάρτη για να ξεκινήσετε τη σχεδίαση των ορίων του τομέα περιπολίας',
    patrolZoneCreated: 'Ο τομέας περιπολίας δημιουργήθηκε επιτυχώς',
    patrolZoneUpdated: 'Ο τομέας περιπολίας ενημερώθηκε επιτυχώς',
    patrolZoneDeleted: 'Ο τομέας περιπολίας διαγράφηκε επιτυχώς',
    deletePatrolZone: 'Διαγραφή Τομέα Περιπολίας',
    confirmDeletePatrolZone: 'Είστε σίγουροι ότι θέλετε να διαγράψετε αυτόν τον τομέα περιπολίας;',
    patrolZoneNamePlaceholder: 'Εισάγετε όνομα τομέα περιπολίας',
    patrolZoneDescriptionPlaceholder: 'Εισάγετε περιγραφή για αυτόν τον τομέα περιπολίας',
    lowPriorityPatrol: 'Χαμηλή Προτεραιότητα',
    mediumPriorityPatrol: 'Μέση Προτεραιότητα',
    highPriorityPatrol: 'Υψηλή Προτεραιότητα',
    criticalPriorityPatrol: 'Κρίσιμη Προτεραιότητα',
    selectColor: 'Επιλογή Χρώματος',
    assignedVehicles: 'Ανατεθειμένα Οχήματα',
    ourAssignedVehicles: 'Τα Δικά μας Οχήματα',
    noVehiclesAssigned: 'Δεν έχουν ανατεθεί οχήματα σε αυτόν τον τομέα περιπολίας',
    patrolZoneDetails: 'Λεπτομέρειες Τομέα Περιπολίας',
    coverage: 'Περιοχή Κάλυψης',
    responsibleStation: 'Υπεύθυνος Σταθμός',
    patrolZoneNameRequired: 'Το όνομα του τομέα περιπολίας είναι υποχρεωτικό',
    patrolZoneDescriptionError: 'Σφάλμα στην περιγραφή του τομέα περιπολίας',
    createZone: 'Δημιουργία Τομέα',
    updateZone: 'Ενημέρωση Τομέα',
    additionalResource: 'Πόρος από άλλο σταθμό',
    telephone: 'Τηλέφωνο',
    email: 'Email',
    updateVehicleStatus: 'Ενημέρωση Κατάστασης Οχήματος',
    updatePersonnelStatus: 'Ενημέρωση Κατάστασης Προσωπικού',

    // Login page translations
    loginTitle: 'Σύστημα Διαχείρισης Συμβάντων Aegis',
    loginSubtitle: 'Πρόσβαση στο διαχειριστικό σύστημα Aegis',
    loginWelcome: 'Καλώς Ήρθατε',
    loginDescription: 'Συνδεθείτε για πρόσβαση στον πίνακα διαχείρισης συμβάντων',
    loginButton: 'Σύνδεση',
    demoCredentials: 'Λογαριασμοί Demo',
    showDemoCredentials: 'Εμφάνιση Demo Kωδικών',
    hideDemoCredentials: 'Απόκρυψη Demo Kωδικών',
    showCredentials: 'Εμφάνιση Κωδικού',
    hideCredentials: 'Απόκρυψη Κωδικού',
    copyEmail: 'Αντιγραφή Email',
    emailCopied: 'Το email αντιγράφηκε στο πρόχειρο',
    loginError: 'Η σύνδεση απέτυχε. Παρακαλώ ελέγξτε τα διαπιστευτήριά σας.',
    loginSuccess: 'Επιτυχής σύνδεση! Ανακατεύθυνση...',
    welcomeToSystem:'Καλώς ήρθατε στο Αιγίς!',
    invalidCredentials: 'Τα στοιχεία σας είναι λανθασμένα',
    signIn:'Σύνδεση',
    password:'Κωδικός',
    clickOnDemoToAutoFill:'Κάντε κλικ σε οποιονδήποτε λογαριασμό για να συμπληρωθούν αυτόματα τα στοιχεία.',
    member:'Μέλος',
    enterEmail:'Παρακαλώ εισάγετε το Email σας',
    enterPassword:'Παρακαλώ εισάγετε τον Κωδικό σας',

    // New incident detail fields
    generalInfo: 'Γενικές Πληροφορίες',
    subCategory: 'Υποκατηγορία',
    incidentResponsibleStation: 'Υπεύθυνος Σταθμός',
    involvement: 'Εμπλεκόμενοι',
    incidentPersonnel: 'Προσωπικό',
    fireTrucksNumber: 'Πυροσβεστικά Οχήματα',
    firePersonnel: 'Πυροσβεστικό Προσωπικό',
    otherAgencies: 'Άλλες Υπηρεσίες',
    serviceActions: 'Ενέργειες Υπηρεσίας',
    rescues: 'Διασώσεις',
    rescuedPeople: 'Διασωθέντα Άτομα',
    rescueInformation: 'Πληροφορίες Διάσωσης',
    commanders: 'Επικεφαλής',
    searchForCommander: 'Αναζήτηση Επικεφαλή',
    observations: 'Παρατηρήσεις',
    noOfficersFound: 'Δεν βρέθηκαν διαθέσιμοι αξιωματικοί',
    optionalObservations: 'Προαιρετικές παρατηρήσεις...',
    noCommandersAssigned: 'Δεν έχει οριστεί επικεφαλής',
    select: 'Επιλογή',
    signal: 'Σήμα:',
    observationsForCommander: 'Παρατηρήσεις για τον επικεφαλή...',
    describeOtherAgencies: 'Περιγράψτε τις άλλες υπηρεσίες που εμπλέκονται...',
    describeServiceActions: 'Περιγράψτε τις ενέργειες που πραγματοποιήθηκαν...',
    rescueDetails: 'Λεπτομέρειες διάσωσης...',
    casualtiesAccidentsBurns: 'Ατυχήματα - Καιόμενα - Ζημιές',
    casualties: 'Ατυχήματα - Καιόμενα - Ζημιές',
    accidents: 'Ατυχήματα',
    injuries: 'Τραυματισμοί',
    firemen: 'Πυροσβέστης',
    civilians: 'Πολίτης',
    nameAndCapacity: 'Όνομα & Ιδιότητα',
    deaths: 'Θάνατοι',
    burned: 'Καιόμενα',
    burnedArea: 'Χώρος',
    burnedItems: 'Αντικείμενα',
    damages: 'Ζημιές',
    ownerName: 'Όνομα Ιδιοκτήτη',
    tenantName: 'Όνομα Ενοικιαστή',
    damageAmount: 'Ύψος Ζημιών',
    savedProperty: 'Διασωθείσα Περιουσία',
    incidentCause: 'Αιτία Συμβάντος',
  }
}
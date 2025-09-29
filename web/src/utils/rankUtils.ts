import { useTranslation } from '../hooks/useTranslation'

export interface RankOption {
  value: string
  label: string
}

export interface RankGroup {
  label: string
  options: RankOption[]
}

export const getRanksByAgency = (agencyName?: string, t?: any): RankGroup[] => {
  if (!t) {
    throw new Error('Translation function is required for getRanksByAgency')
  }

  const fireServiceRanks: RankOption[] = [
    { value: 'Lieutenant Fire General', label: t.lieutenantFireGeneral },
    { value: 'Major Fire General', label: t.majorFireGeneral },
    { value: 'Fire Brigadier', label: t.fireBrigadier },
    { value: 'Fire Colonel', label: t.fireColonel },
    { value: 'Lieutenant Fire Colonel', label: t.lieutenantFireColonel },
    { value: 'Fire Major', label: t.fireMajor },
    { value: 'Fire Captain', label: t.fireCaptain },
    { value: 'Fire Lieutenant', label: t.fireLieutenant },
    { value: 'Fire Second Lieutenant', label: t.fireSecondLieutenant },
    { value: 'Fire Warrant Officer', label: t.fireWarrantOfficer },
    { value: 'Fire Sergeant (Academy)', label: t.fireSergeantAcademy },
    { value: 'Fire Sergeant (Non-Academy)', label: t.fireSergeantNonAcademy },
    { value: 'Senior Firefighter (Academy)', label: t.seniorFirefighterAcademy },
    { value: 'Senior Firefighter (Non-Academy)', label: t.seniorFirefighterNonAcademy },
    { value: 'Firefighter', label: t.firefighterRank },
  ]

  const coastGuardRanks: RankOption[] = [
    { value: 'Vice Admiral', label: t.viceAdmiral },
    { value: 'Rear Admiral', label: t.rearAdmiral },
    { value: 'Commodore', label: t.commodore },
    { value: 'Captain', label: t.captain },
    { value: 'Commander', label: t.commander },
    { value: 'Lieutenant Commander', label: t.ltCommander },
    { value: 'Lieutenant', label: t.lieutenant },
    { value: 'Lieutenant Junior Grade', label: t.subLieutenant },
    { value: 'Ensign', label: t.ensign },
    { value: 'Warrant Officer', label: t.warrantOfficer },
    { value: 'Chief Petty Officer', label: t.chiefPettyOfficer },
    { value: 'Petty Officer 1st Class', label: t.pettyOfficer1st },
    { value: 'Petty Officer 2nd Class', label: t.pettyOfficer2nd },
    { value: 'Coast Guardsman', label: t.coastGuardRank },
  ]

  const policeRanks: RankOption[] = [
    //Officers
    { value: 'Police Lieutenant General', label: t.policeLieutenantGeneral },
    { value: 'Police Major General', label: t.policeMajorGeneral },
    { value: 'Police Brigadier General', label: t.policeBrigadierGeneral },
    { value: 'Police Director', label: t.policeDirector },
    { value: 'Police Deputy Director', label: t.policeDeputyDirector },
    { value: 'Police Captain I (or Police Major)', label: t.policeCaptainI },
    { value: 'Police Captain II', label: t.policeCaptainII },
    { value: 'Police Lieutenant I', label: t.policeLieutenantI },
    { value: 'Police Lieutenant II', label: t.policeLieutenantII },
    { value: 'Police Deputy Lieutenant', label: t.policeDeputyLieutenant },
    //// Sergeant / NCO & Enlisted 
    { value: 'Police Sergeant (Investigative Duty – with promotion exam)', label: t.policeSergeantInvestigativeExam },
    { value: 'Police Sergeant (Investigative Duty)', label: t.policeSergeantInvestigative },
    { value: 'Police Sergeant (Non-Investigative Duty)', label: t.policeSergeantNonInvestigative },
    { value: 'Police Deputy Sergeant (Investigative Duty)', label: t.policeDeputySergeantInvestigative },
    { value: 'Police Deputy Sergeant (Non-Investigative Duty)', label: t.policeDeputySergeantNonInvestigative },
    { value: 'Police Constable', label: t.policeConstable },
  ]



  const ekabRanks: RankOption[] = [
    // Senior Management / Administration
    { value: 'President of EKAB', label: t.ekabPresident },
    { value: 'Vice President of EKAB', label: t.ekabVicePresident },
    { value: 'Regional Director', label: t.ekabRegionalDirector },
    { value: 'Department Head', label: t.ekabDepartmentHead },

    // Medical & Paramedical Staff
    { value: 'Emergency Doctor', label: t.ekabEmergencyDoctor },
    { value: 'Nurse', label: t.ekabNurse },
    { value: 'Paramedic Supervisor', label: t.ekabParamedicSupervisor },
    { value: 'Paramedic (Ambulance Crew)', label: t.ekabParamedic },
    { value: 'Ambulance Driver', label: t.ekabAmbulanceDriver },

    // Specialized Units
    { value: 'Helicopter Doctor', label: t.ekabHelicopterDoctor },
    { value: 'Helicopter Paramedic', label: t.ekabHelicopterParamedic },
    { value: 'Special Rescue Team (EKAB-SRT)', label: t.ekabSpecialRescueTeam },

    // Entry / Support Roles
    { value: 'Call Center Operator (Dispatch)', label: t.ekabCallCenterOperator },
    { value: 'Administrative Staff', label: t.ekabAdministrativeStaff },
    { value: 'Logistics / Technical Support', label: t.ekabLogisticsSupport },
  ]


  // Determine which ranks to show based on agency
  if (agencyName === 'Hellenic Fire Service' || agencyName === 'Fire Department' || agencyName === 'FireDepartment' || agencyName?.toLowerCase().includes('fire')) {
    return [{ label: 'Fire Service', options: fireServiceRanks }]
  } else if (agencyName === 'Hellenic Coast Guard' || agencyName === 'Coast Guard' || agencyName === 'CoastGuard' || agencyName?.toLowerCase().includes('coast')) {
    return [{ label: 'Coast Guard', options: coastGuardRanks }]
  } else if (agencyName === 'Hellenic Police' || agencyName === 'Police' || agencyName?.toLowerCase().includes('police')) {
    return [{ label: 'Police', options: policeRanks }]
  } else if (agencyName === 'EKAB' || agencyName?.toLowerCase().includes('ekab')) {
    return [{ label: 'EKAB', options: ekabRanks }]
  }

  // Fallback - show all ranks if agency not recognized
  return [
    { label: 'Fire Service', options: fireServiceRanks },
    { label: 'Coast Guard', options: coastGuardRanks },
    { label: 'Police', options: policeRanks },
    { label: 'EKAB', options: ekabRanks },
  ]
}

export const useRanksByAgency = (agencyName?: string): RankGroup[] => {
  const t = useTranslation()
  return getRanksByAgency(agencyName, t)
}

export const translateRank = (rank: string, t: any): string => {
  // Create a mapping of rank values to translations
  const rankTranslations: Record<string, string> = {
    // Fire Service
    'Lieutenant Fire General': t.lieutenantFireGeneral,
    'Major Fire General': t.majorFireGeneral,
    'Fire Brigadier': t.fireBrigadier,
    'Fire Colonel': t.fireColonel,
    'Lieutenant Fire Colonel': t.lieutenantFireColonel,
    'Fire Major': t.fireMajor,
    'Fire Captain': t.fireCaptain,
    'Fire Lieutenant': t.fireLieutenant,
    'Fire Second Lieutenant': t.fireSecondLieutenant,
    'Fire Warrant Officer': t.fireWarrantOfficer,
    'Fire Sergeant (Academy)': t.fireSergeantAcademy,
    'Fire Sergeant (Non-Academy)': t.fireSergeantNonAcademy,
    'Senior Firefighter (Academy)': t.seniorFirefighterAcademy,
    'Senior Firefighter (Non-Academy)': t.seniorFirefighterNonAcademy,
    'Firefighter': t.firefighterRank,
    
    // Coast Guard
    'Vice Admiral': t.viceAdmiral,
    'Rear Admiral': t.rearAdmiral,
    'Commodore': t.commodore,
    'Captain': t.captain,
    'Commander': t.commander,
    'Lieutenant Commander': t.ltCommander,
    'Lieutenant': t.lieutenant,
    'Lieutenant Junior Grade': t.subLieutenant,
    'Ensign': t.ensign,
    'Warrant Officer': t.warrantOfficer,
    'Chief Petty Officer': t.chiefPettyOfficer,
    'Petty Officer 1st Class': t.pettyOfficer1st,
    'Petty Officer 2nd Class': t.pettyOfficer2nd,
    'Coast Guardsman': t.coastGuardRank,
    
    // Police
    'Police Lieutenant General': t.policeLieutenantGeneral,
    'Police Major General': t.policeMajorGeneral,
    'Police Brigadier General': t.policeBrigadierGeneral,
    'Police Director': t.policeDirector,
    'Police Deputy Director': t.policeDeputyDirector,
    'Police Captain I (or Police Major)': t.policeCaptainI,
    'Police Captain II': t.policeCaptainII,
    'Police Lieutenant I': t.policeLieutenantI,
    'Police Lieutenant II': t.policeLieutenantII,
    'Police Deputy Lieutenant': t.policeDeputyLieutenant,
    'Police Sergeant (Investigative Duty – with promotion exam)': t.policeSergeantInvestigativeExam,
    'Police Sergeant (Investigative Duty)': t.policeSergeantInvestigative,
    'Police Sergeant (Non-Investigative Duty)': t.policeSergeantNonInvestigative,
    'Police Deputy Sergeant (Investigative Duty)': t.policeDeputySergeantInvestigative,
    'Police Deputy Sergeant (Non-Investigative Duty)': t.policeDeputySergeantNonInvestigative,
    'Police Constable': t.policeConstable,
    
    // EKAB
    'President of EKAB': t.ekabPresident,
    'Vice President of EKAB': t.ekabVicePresident,
    'Regional Director': t.ekabRegionalDirector,
    'Department Head': t.ekabDepartmentHead,
    'Emergency Doctor': t.ekabEmergencyDoctor,
    'Nurse': t.ekabNurse,
    'Paramedic Supervisor': t.ekabParamedicSupervisor,
    'Paramedic (Ambulance Crew)': t.ekabParamedic,
    'Ambulance Driver': t.ekabAmbulanceDriver,
    'Helicopter Doctor': t.ekabHelicopterDoctor,
    'Helicopter Paramedic': t.ekabHelicopterParamedic,
    'Special Rescue Team (EKAB-SRT)': t.ekabSpecialRescueTeam,
    'Call Center Operator (Dispatch)': t.ekabCallCenterOperator,
    'Administrative Staff': t.ekabAdministrativeStaff,
    'Logistics / Technical Support': t.ekabLogisticsSupport,
  }
  
  return rankTranslations[rank] || rank
}
using IncidentManagement.Domain.Entities;
using IncidentManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace IncidentManagement.Infrastructure.Data;

public static class SeedData
{

    public static async Task SeedAgencies(IncidentManagementDbContext context)
    {
        if (await context.Agencies.AnyAsync())
            return;

        // Create agencies first
        var agencies = new[]
        {
             new Agency
            {
                Type = AgencyType.Police,
                Name = "Hellenic Police",
                Code = "POLICE",
                Description = "Law Enforcement and Public Safety",
                IsActive = true
            },           
            new Agency
            {
                Type = AgencyType.Fire,
                Name = "Hellenic Fire Service",
                Code = "FIRE",
                Description = "Fire and Rescue Services",
                IsActive = true
            },
            new Agency
            {
                Type = AgencyType.CoastGuard,
                Name = "Hellenic Coast Guard",
                Code = "HCG",
                Description = "Maritime Safety and Security",
                IsActive = true
            },
            new Agency
            {
                Type = AgencyType.EKAB,
                Name = "EKAB",
                Code = "EKAB",
                Description = "National Center for Emergency Care",
                IsActive = true
            }
        };

        context.Agencies.AddRange(agencies);
        await context.SaveChangesAsync();            
    }

    public static async Task SeedUsers(IncidentManagementDbContext context)
    {
        // Check if users are already seeded
        if (await context.Users.AnyAsync())
            return; // Already seeded

        // Get agencies from database
        var agencies = await context.Agencies.ToListAsync();
        var fireAgency = agencies.FirstOrDefault(a => a.Code == "FIRE");
        var coastGuardAgency = agencies.FirstOrDefault(a => a.Code == "HCG");
        var ekabAgency = agencies.FirstOrDefault(a => a.Code == "EKAB");
        var policeAgency = agencies.FirstOrDefault(a => a.Code == "POLICE");

        if (fireAgency == null || coastGuardAgency == null || ekabAgency == null || policeAgency == null)
        {
            throw new InvalidOperationException("Agencies must be seeded before users");
        }

        // Create demo users with proper credentials for all agencies
        var users = new[]
        {
            // Fire Service Users
            new User
            {
                SupabaseUserId = "fire-dispatcher-1",
                Email = "dispatcher@fireservice.gr",
                Password = "1", // In production, this should be hashed
                Name = "Dimitris Fire Dispatcher",
                Role = UserRole.Dispatcher,
                AgencyId = fireAgency.Id,
                IsActive = true
            },
            new User
            {
                SupabaseUserId = "fire-member-1",
                Email = "firefighter@fireservice.gr",
                Password = "1",
                Name = "Maria Firefighter",
                Role = UserRole.Member,
                AgencyId = fireAgency.Id,
                IsActive = true
            },
            
            // Coast Guard Users
            new User
            {
                SupabaseUserId = "cg-dispatcher-1",
                Email = "dispatcher@coastguard.gr",
                Password = "1",
                Name = "Nikos Coast Guard Dispatcher",
                Role = UserRole.Dispatcher,
                AgencyId = coastGuardAgency.Id,
                IsActive = true
            },
            new User
            {
                SupabaseUserId = "cg-member-1",
                Email = "member@coastguard.gr",
                Password = "1",
                Name = "Anna Coast Guard Member",
                Role = UserRole.Member,
                AgencyId = coastGuardAgency.Id,
                IsActive = true
            },
            
            // EKAB Users
            new User
            {
                SupabaseUserId = "ekab-dispatcher-1",
                Email = "dispatcher@ekab.gr",
                Password = "1",
                Name = "Kostas EKAB Dispatcher",
                Role = UserRole.Dispatcher,
                AgencyId = ekabAgency.Id,
                IsActive = true
            },
            new User
            {
                SupabaseUserId = "ekab-member-1",
                Email = "member@ekab.gr",
                Password = "1",
                Name = "Sofia EKAB Member",
                Role = UserRole.Member,
                AgencyId = ekabAgency.Id,
                IsActive = true
            },
            
            // Police Users
            new User
            {
                SupabaseUserId = "police-dispatcher-1",
                Email = "dispatcher@police.gr",
                Password = "1",
                Name = "Yannis Police Dispatcher",
                Role = UserRole.Dispatcher,
                AgencyId = policeAgency.Id,
                IsActive = true
            },
            new User
            {
                SupabaseUserId = "police-member-1",
                Email = "member@police.gr",
                Password = "1",
                Name = "Elena Soukalo",
                Role = UserRole.Member,
                AgencyId = policeAgency.Id,
                IsActive = true
            }
        };

        context.Users.AddRange(users);
        await context.SaveChangesAsync();
    }

    public static async Task SeedStations(IncidentManagementDbContext context)
    {
        // Check if stations are already seeded
        if (await context.Stations.AnyAsync())
            return; // Already seeded

        // Get agencies from database
        var agencies = await context.Agencies.ToListAsync();
        var fireAgency = agencies.FirstOrDefault(a => a.Code == "FIRE");
        var coastGuardAgency = agencies.FirstOrDefault(a => a.Code == "HCG");
        var ekabAgency = agencies.FirstOrDefault(a => a.Code == "EKAB");
        var policeAgency = agencies.FirstOrDefault(a => a.Code == "POLICE");

        if (fireAgency == null || coastGuardAgency == null || ekabAgency == null || policeAgency == null)
        {
            throw new InvalidOperationException("Agencies must be seeded before stations");
        }

        // Create stations for all agencies
        var stations = new[]
        {
            // // Fire Service Stations
            new Station
            {
                Name = "1ος Π. Σ. ΑΘΗΝΩΝ",
                AgencyId = fireAgency.Id,
                Latitude = 37.97474804070276,
                Longitude = 23.74144565231651
            },
            new Station
            {
                Name = "2ος Π. Σ. ΑΘΗΝΩΝ",
                AgencyId = fireAgency.Id,
                Latitude = 37.98874040718599,
                Longitude = 23.7093169684605
            },
            
            // Coast Guard Stations
            new Station
            {
                Name = "Piraeus Coast Guard Port",
                AgencyId = coastGuardAgency.Id,
                Latitude = 37.9364,
                Longitude = 23.6478
            },
            new Station
            {
                Name = "Thessaloniki Coast Guard Port",
                AgencyId = coastGuardAgency.Id,
                Latitude = 40.6401,
                Longitude = 22.9444
            },
            
            // EKAB Stations
            new Station
            {
                Name = "Athens EKAB Central",
                AgencyId = ekabAgency.Id,
                Latitude = 37.9838,
                Longitude = 23.7275
            },
            new Station
            {
                Name = "Thessaloniki EKAB Station",
                AgencyId = ekabAgency.Id,
                Latitude = 40.6401,
                Longitude = 22.9444
            },
            
            // // Police Stations
            new Station
            {
                Name = "Athens Central Police Station",
                AgencyId = policeAgency.Id,
                Latitude = 37.9755,
                Longitude = 23.7348
            },
            new Station
            {
                Name = "Thessaloniki Police Station",
                AgencyId = policeAgency.Id,
                Latitude = 40.6401,
                Longitude = 22.9444
            }
        };

        context.Stations.AddRange(stations);
        await context.SaveChangesAsync();
    }

    public static async Task SeedUserStationAssignments(IncidentManagementDbContext context)
    {
        // Get agencies from database
        var agencies = await context.Agencies.ToListAsync();
        var fireAgency = agencies.FirstOrDefault(a => a.Code == "FIRE");
        var coastGuardAgency = agencies.FirstOrDefault(a => a.Code == "HCG");
        var ekabAgency = agencies.FirstOrDefault(a => a.Code == "EKAB");
        var policeAgency = agencies.FirstOrDefault(a => a.Code == "POLICE");

        if (fireAgency == null || coastGuardAgency == null || ekabAgency == null || policeAgency == null)
        {
            throw new InvalidOperationException("Agencies must be seeded before user station assignments");
        }

        // Update users with station assignments (only members get station assignments)
        var allStations = await context.Stations.ToListAsync();
        var fireStations = allStations.Where(s => s.AgencyId == fireAgency.Id).ToList();
        var coastGuardStations = allStations.Where(s => s.AgencyId == coastGuardAgency.Id).ToList();
        var ekabStations = allStations.Where(s => s.AgencyId == ekabAgency.Id).ToList();
        var policeStations = allStations.Where(s => s.AgencyId == policeAgency.Id).ToList();

        // Assign stations to member users (not dispatchers)
        var fireMember = await context.Users.FirstOrDefaultAsync(u => u.SupabaseUserId == "fire-member-1");
        if (fireMember != null && fireStations.Any())
        {
            fireMember.StationId = fireStations[0].Id;
        }

        var coastGuardMember = await context.Users.FirstOrDefaultAsync(u => u.SupabaseUserId == "cg-member-1");
        if (coastGuardMember != null && coastGuardStations.Any())
        {
            coastGuardMember.StationId = coastGuardStations[0].Id;
        }

        var ekabMember = await context.Users.FirstOrDefaultAsync(u => u.SupabaseUserId == "ekab-member-1");
        if (ekabMember != null && ekabStations.Any())
        {
            ekabMember.StationId = ekabStations[0].Id;
        }

        var policeMember = await context.Users.FirstOrDefaultAsync(u => u.SupabaseUserId == "police-member-1");
        if (policeMember != null && policeStations.Any())
        {
            policeMember.StationId = policeStations[0].Id;
        }

        await context.SaveChangesAsync();
    }

    public static async Task SeedVehicles(IncidentManagementDbContext context)
    {
        // Check if vehicles are already seeded
        if (await context.Vehicles.AnyAsync())
            return; // Vehicles already seeded

        // Get agencies from database
        var agencies = await context.Agencies.ToListAsync();
        var fireAgency = agencies.FirstOrDefault(a => a.Code == "FIRE");
        var coastGuardAgency = agencies.FirstOrDefault(a => a.Code == "HCG");
        var ekabAgency = agencies.FirstOrDefault(a => a.Code == "EKAB");
        var policeAgency = agencies.FirstOrDefault(a => a.Code == "POLICE");

        if (fireAgency == null || coastGuardAgency == null || ekabAgency == null || policeAgency == null)
        {
            throw new InvalidOperationException("Agencies must be seeded before vehicles");
        }

        // Get stations from database
        var allStations = await context.Stations.ToListAsync();
        var fireStations = allStations.Where(s => s.AgencyId == fireAgency.Id).ToList();
        var coastGuardStations = allStations.Where(s => s.AgencyId == coastGuardAgency.Id).ToList();
        var ekabStations = allStations.Where(s => s.AgencyId == ekabAgency.Id).ToList();
        var policeStations = allStations.Where(s => s.AgencyId == policeAgency.Id).ToList();

        // Create vehicles for all agencies
        var vehicles = new List<Vehicle>();

        // Fire Service Vehicles
        if (fireStations.Count > 0)
        {
            vehicles.AddRange(new[]
            {
                new Vehicle
                {
                    StationId = fireStations[0].Id,
                    Callsign = "1-1-1 ΑΘΗΝΩΝ",
                    Type = "ΥΔΡΟΦΟΡΟ Α'ΤΥΠΟΥ",
                    PlateNumber = "ΠΣ-3148",
                    WaterCapacityLiters = 1000,
                    WaterLevelLiters = 1000,
                    FuelLevelPercent = 85,
                    Status = VehicleStatus.Available
                },
                new Vehicle
                {
                    StationId = fireStations[0].Id,
                    Callsign = "1-1-2 ΑΘΗΝΩΝ",
                    Type = "ΥΔΡΟΦΟΡΟ Β'ΤΥΠΟΥ",
                    PlateNumber = "ΠΣ-1357",
                    WaterCapacityLiters = 2500,
                    WaterLevelLiters = 2000,                    
                    FuelLevelPercent = 92,
                    Status = VehicleStatus.Available
                },
                new Vehicle
                {
                    StationId = fireStations[0].Id,
                    Callsign = "1-27",
                    Type = "ΥΔΡΟΦΟΡΟ Α'ΤΥΠΟΥ",
                    PlateNumber = "ΠΣ-3127",
                    FuelLevelPercent = 95,
                    WaterCapacityLiters = 1000,
                    WaterLevelLiters = 1000, 
                    Status = VehicleStatus.Available
                },
                new Vehicle
                {
                    StationId = fireStations[0].Id,
                    Callsign = "1-3 ΑΘΗΝΩΝ",
                    Type = "ΔΙΑΣΩΣΤΙΚΟ",
                    PlateNumber = "ΠΣ-3453",
                    FuelLevelPercent = 95,
                    Status = VehicleStatus.Available
                },
                new Vehicle
                {
                    StationId = fireStations[0].Id,
                    Callsign = "1-422 ΑΘΗΝΩΝ",
                    Type = "ΒΡΑΧΙΟΝΟΦΟΡΟ 22μ",
                    PlateNumber = "ΠΣ-2552",
                    FuelLevelPercent = 95,
                    Status = VehicleStatus.Available
                },
                new Vehicle
                {
                    StationId = fireStations[0].Id,
                    Callsign = "1-330 ΑΘΗΝΩΝ",
                    Type = "ΚΛΙΜΑΚΟΦΟΡΟ 30μ",
                    PlateNumber = "ΠΣ-2632",
                    FuelLevelPercent = 95,
                    Status = VehicleStatus.Available
                },
                new Vehicle
                {
                    StationId = fireStations[0].Id,
                    Callsign = "1-2 ΑΘΗΝΩΝ",
                    Type = "ΥΔΡΟΦΟΡΟ Δ'ΤΥΠΟΥ",
                    PlateNumber = "ΠΣ-2856",
                    FuelLevelPercent = 95,
                    WaterCapacityLiters = 10000,
                    WaterLevelLiters = 9850, 
                    Status = VehicleStatus.Available
                },
                new Vehicle
                {
                    StationId = fireStations[0].Id,
                    Callsign = "ΚΡΟΝΟΣ 15",
                    Type = "ΒΟΗΘΗΤΙΚΟ JEEP",
                    PlateNumber = "ΠΣ-5162",
                    FuelLevelPercent = 78,
                    Status = VehicleStatus.Available
                }                
            });

            if (fireStations.Count > 1)
            {
                vehicles.AddRange(new[]
                {
                    new Vehicle
                    {
                        StationId = fireStations[1].Id,
                        Callsign = "2-1-1 ΑΘΗΝΩΝ",
                        Type = "ΥΔΡΟΦΟΡΟ Α'ΤΥΠΟΥ",
                        PlateNumber = "ΠΣ-3142",
                        WaterCapacityLiters = 1000,
                        WaterLevelLiters = 980,
                        FuelLevelPercent = 88,
                        Status = VehicleStatus.Available
                    },
                    new Vehicle
                    {
                        StationId = fireStations[1].Id,
                        Callsign = "2-1-2 ΑΘΗΝΩΝ",
                        Type = "ΥΔΡΟΦΟΡΟ Β'ΤΥΠΟΥ",
                        PlateNumber = "ΠΣ-3202",
                        WaterCapacityLiters = 2500,
                        WaterLevelLiters = 2500,
                        FuelLevelPercent = 88,
                        Status = VehicleStatus.Available
                    },
                    new Vehicle
                    {
                        StationId = fireStations[1].Id,
                        Callsign = "2-320 ΑΘΗΝΩΝ",
                        Type = "ΚΛΙΜΑΚΟΦΟΡΟ 30μ",
                        PlateNumber = "ΠΣ-3832",
                        FuelLevelPercent = 95,
                        Status = VehicleStatus.Available
                    },
                    new Vehicle
                    {
                        StationId = fireStations[1].Id,
                        Callsign = "2-2 ΑΘΗΝΩΝ",
                        Type = "ΥΔΡΟΦΟΡΟ Δ'ΤΥΠΟΥ",
                        PlateNumber = "ΠΣ-2895",
                        FuelLevelPercent = 95,
                        WaterCapacityLiters = 10000,
                        WaterLevelLiters = 9850, 
                        Status = VehicleStatus.Available
                    },                    
                });
            }
        }

        // Coast Guard Vehicles
        if (coastGuardStations.Any())
        {
            vehicles.AddRange(new[]
            {
                new Vehicle
                {
                    StationId = coastGuardStations[0].Id,
                    Callsign = "HCG-101",
                    Type = "Patrol Boat",
                    PlateNumber = "ΛΣ-1001",
                    FuelLevelPercent = 90,
                    Status = VehicleStatus.Available
                },
                new Vehicle
                {
                    StationId = coastGuardStations[0].Id,
                    Callsign = "HCG-SAR-1",
                    Type = "Search and Rescue Vessel",
                    PlateNumber = "ΛΣ-1002",
                    FuelLevelPercent = 85,
                    Status = VehicleStatus.Available
                },
                new Vehicle
                {
                    StationId = coastGuardStations[0].Id,
                    Callsign = "HCG-CMD-1",
                    Type = "Command Vessel",
                    PlateNumber = "ΛΣ-2001",
                    FuelLevelPercent = 95,
                    Status = VehicleStatus.Available
                }
            });

            if (coastGuardStations.Count > 1)
            {
                vehicles.AddRange(new[]
                {
                    new Vehicle
                    {
                        StationId = coastGuardStations[1].Id,
                        Callsign = "HCG-201",
                        Type = "Patrol Boat",
                        PlateNumber = "ΛΣ-2101",
                        FuelLevelPercent = 88,
                        Status = VehicleStatus.Available
                    },
                    new Vehicle
                    {
                        StationId = coastGuardStations[1].Id,
                        Callsign = "HCG-RESCUE-2",
                        Type = "Rescue Vessel",
                        PlateNumber = "ΛΣ-2102",
                        FuelLevelPercent = 92,
                        Status = VehicleStatus.Available
                    }
                });
            }
        }

        // EKAB Vehicles
        if (ekabStations.Any())
        {
            vehicles.AddRange(new[]
            {
                new Vehicle
                {
                    StationId = ekabStations[0].Id,
                    Callsign = "EKAB-101",
                    Type = "Ambulance",
                    PlateNumber = "ΕΚ-3001",
                    FuelLevelPercent = 85,
                    Status = VehicleStatus.Available
                },
                new Vehicle
                {
                    StationId = ekabStations[0].Id,
                    Callsign = "EKAB-ICU-1",
                    Type = "Mobile ICU",
                    PlateNumber = "ΕΚ-3002",
                    FuelLevelPercent = 90,
                    Status = VehicleStatus.Available
                },
                new Vehicle
                {
                    StationId = ekabStations[0].Id,
                    Callsign = "EKAB-CMD-1",
                    Type = "Command Vehicle",
                    PlateNumber = "ΕΚ-4001",
                    FuelLevelPercent = 95,
                    Status = VehicleStatus.Available
                }
            });

            if (ekabStations.Count > 1)
            {
                vehicles.AddRange(new[]
                {
                    new Vehicle
                    {
                        StationId = ekabStations[1].Id,
                        Callsign = "EKAB-201",
                        Type = "Ambulance",
                        PlateNumber = "ΕΚ-4101",
                        FuelLevelPercent = 88,
                        Status = VehicleStatus.Available
                    },
                    new Vehicle
                    {
                        StationId = ekabStations[1].Id,
                        Callsign = "EKAB-RESCUE-2",
                        Type = "Rescue Ambulance",
                        PlateNumber = "ΕΚ-4102",
                        FuelLevelPercent = 92,
                        Status = VehicleStatus.Available
                    }
                });
            }
        }

        // Police Vehicles
        if (policeStations.Any())
        {
            vehicles.AddRange(new[]
            {
                new Vehicle
                {
                    StationId = policeStations[0].Id,
                    Callsign = "POLICE-101",
                    Type = "Patrol Car",
                    PlateNumber = "ΑΣ-1001",
                    FuelLevelPercent = 85,
                    Status = VehicleStatus.Available
                },
                new Vehicle
                {
                    StationId = policeStations[0].Id,
                    Callsign = "POLICE-SWAT-1",
                    Type = "SWAT Vehicle",
                    PlateNumber = "ΑΣ-1002",
                    FuelLevelPercent = 90,
                    Status = VehicleStatus.Available
                },
                new Vehicle
                {
                    StationId = policeStations[0].Id,
                    Callsign = "POLICE-CMD-1",
                    Type = "Command Vehicle",
                    PlateNumber = "ΑΣ-2001",
                    FuelLevelPercent = 95,
                    Status = VehicleStatus.Available
                }
            });

            if (policeStations.Count > 1)
            {
                vehicles.AddRange(new[]
                {
                    new Vehicle
                    {
                        StationId = policeStations[1].Id,
                        Callsign = "POLICE-201",
                        Type = "Patrol Car",
                        PlateNumber = "ΑΣ-2101",
                        FuelLevelPercent = 88,
                        Status = VehicleStatus.Available
                    },
                    new Vehicle
                    {
                        StationId = policeStations[1].Id,
                        Callsign = "POLICE-MOTOR-2",
                        Type = "Motorcycle",
                        PlateNumber = "ΑΣ-2102",
                        FuelLevelPercent = 92,
                        Status = VehicleStatus.Available
                    }
                });
            }
        }

        if (vehicles.Any())
        {
            context.Vehicles.AddRange(vehicles);
            await context.SaveChangesAsync();
        }
    }

    public static async Task SeedPersonnel(IncidentManagementDbContext context)
    {
        // Check if personnel are already seeded
        if (await context.Personnel.AnyAsync())
            return; // Already seeded

        // Get agencies from database
        var agencies = await context.Agencies.ToListAsync();
        var fireAgency = agencies.FirstOrDefault(a => a.Code == "FIRE");
        var coastGuardAgency = agencies.FirstOrDefault(a => a.Code == "HCG");
        var ekabAgency = agencies.FirstOrDefault(a => a.Code == "EKAB");
        var policeAgency = agencies.FirstOrDefault(a => a.Code == "POLICE");

        if (fireAgency == null || coastGuardAgency == null || ekabAgency == null || policeAgency == null)
        {
            throw new InvalidOperationException("Agencies must be seeded before personnel");
        }

        // Get stations from database
        var allStations = await context.Stations.ToListAsync();
        var fireStations = allStations.Where(s => s.AgencyId == fireAgency.Id).ToList();
        var coastGuardStations = allStations.Where(s => s.AgencyId == coastGuardAgency.Id).ToList();
        var ekabStations = allStations.Where(s => s.AgencyId == ekabAgency.Id).ToList();
        var policeStations = allStations.Where(s => s.AgencyId == policeAgency.Id).ToList();

        // Personnel for all agencies
        var personnel = new List<Personnel>();

        // Fire Service Personnel
        if (fireStations.Count > 0)
        {
            personnel.AddRange(new[]
            {
                new Personnel { StationId = fireStations[0].Id, AgencyId = fireAgency.Id, Name = "Dimitris Papadopoulos", Rank = "Fire Captain", BadgeNumber = "9885", IsActive = true },
                new Personnel { StationId = fireStations[0].Id, AgencyId = fireAgency.Id, Name = "Maria Konstantinou", Rank = "Fire Lieutenant", BadgeNumber = "19856", IsActive = true },
                new Personnel { StationId = fireStations[0].Id, AgencyId = fireAgency.Id, Name = "Nikos Georgiadis", Rank = "Firefighter", BadgeNumber = "22587", IsActive = true },
                new Personnel { StationId = fireStations[0].Id, AgencyId = fireAgency.Id, Name = "Anna Stavrou", Rank = "Firefighter", BadgeNumber = "22128", IsActive = true },
                new Personnel { StationId = fireStations[0].Id, AgencyId = fireAgency.Id, Name = "Kostas Michalopoulos", Rank = "Firefighter", BadgeNumber = "22578", IsActive = true }
            });

            if (fireStations.Count > 1)
            {
                personnel.AddRange(new[]
                {
                    new Personnel { StationId = fireStations[1].Id, AgencyId = fireAgency.Id, Name = "Yannis Petridis", Rank = "Fire Captain", BadgeNumber = "8874", IsActive = true },
                    new Personnel { StationId = fireStations[1].Id, AgencyId = fireAgency.Id, Name = "Sofia Nikolaou", Rank = "Fire Lieutenant", BadgeNumber = "16554", IsActive = true },
                    new Personnel { StationId = fireStations[1].Id, AgencyId = fireAgency.Id, Name = "Alexandros Katsaros", Rank = "Firefighter", BadgeNumber = "21488", IsActive = true },
                    new Personnel { StationId = fireStations[1].Id, AgencyId = fireAgency.Id, Name = "Elena Christou", Rank = "Firefighter", BadgeNumber = "23558", IsActive = true },
                    new Personnel { StationId = fireStations[1].Id, AgencyId = fireAgency.Id, Name = "Panagiotis Vlahos", Rank = "Firefighter", BadgeNumber = "23555", IsActive = true }
                });
            }
        }

        // Coast Guard Personnel
        if (coastGuardStations.Any())
        {
            personnel.AddRange(new[]
            {
                new Personnel { StationId = coastGuardStations[0].Id, AgencyId = coastGuardAgency.Id, Name = "Ioannis Marinou", Rank = "Captain", BadgeNumber = "78978987", IsActive = true },
                new Personnel { StationId = coastGuardStations[0].Id, AgencyId = coastGuardAgency.Id, Name = "Sofia Thalassia", Rank = "Lieutenant Commander", BadgeNumber = "12316548", IsActive = true },
                new Personnel { StationId = coastGuardStations[0].Id, AgencyId = coastGuardAgency.Id, Name = "Nikos Adaam", Rank = "Chief Petty Officer", BadgeNumber = "1231645", IsActive = true },
                new Personnel { StationId = coastGuardStations[0].Id, AgencyId = coastGuardAgency.Id, Name = "Maria Poseidoniou", Rank = "Petty Officer 1st Class", BadgeNumber = "213123", IsActive = true },
                new Personnel { StationId = coastGuardStations[0].Id, AgencyId = coastGuardAgency.Id, Name = "Dimitris Nautikos", Rank = "Coast Guardsman", BadgeNumber = "2163456", IsActive = true }
            });

            if (coastGuardStations.Count > 1)
            {
                personnel.AddRange(new[]
                {
                    new Personnel { StationId = coastGuardStations[1].Id, AgencyId = coastGuardAgency.Id, Name = "Andreas Pelagios", Rank = "Commodore", BadgeNumber = "2132154", IsActive = true },
                    new Personnel { StationId = coastGuardStations[1].Id, AgencyId = coastGuardAgency.Id, Name = "Elena Thalassinou", Rank = "Lieutenant Junior Grade", BadgeNumber = "21332132", IsActive = true },
                    new Personnel { StationId = coastGuardStations[1].Id, AgencyId = coastGuardAgency.Id, Name = "Kostas Fousekis", Rank = "Warrant Officer", BadgeNumber = "456489", IsActive = true },
                    new Personnel { StationId = coastGuardStations[1].Id, AgencyId = coastGuardAgency.Id, Name = "Anna Triton", Rank = "Petty Officer 1st Class", BadgeNumber = "2165456", IsActive = true },
                    new Personnel { StationId = coastGuardStations[1].Id, AgencyId = coastGuardAgency.Id, Name = "Petros Piksidas", Rank = "Coast Guardsman", BadgeNumber = "212246", IsActive = true }
                });
            }
        }

        // EKAB Personnel
        if (ekabStations.Any())
        {
            personnel.AddRange(new[]
            {
                new Personnel { StationId = ekabStations[0].Id, AgencyId = ekabAgency.Id, Name = "Alexandros Iatridis", Rank = "Emergency Doctor", BadgeNumber = "27Η25", IsActive = true },
                new Personnel { StationId = ekabStations[0].Id, AgencyId = ekabAgency.Id, Name = "Maria Therapousa", Rank = "Paramedic (Ambulance Crew)", BadgeNumber = "05Κ25", IsActive = true },
                new Personnel { StationId = ekabStations[0].Id, AgencyId = ekabAgency.Id, Name = "Nikos Soter", Rank = "Paramedic (Ambulance Crew)", BadgeNumber = "02Υ25", IsActive = true },
                new Personnel { StationId = ekabStations[0].Id, AgencyId = ekabAgency.Id, Name = "Sofia Fragkou", Rank = "Ambulance Driver", BadgeNumber = "01Β25", IsActive = true },
                new Personnel { StationId = ekabStations[0].Id, AgencyId = ekabAgency.Id, Name = "Dimitris Katsafoudis", Rank = "Ambulance Driver", BadgeNumber = "11Γ25", IsActive = true }
            });

            if (ekabStations.Count > 1)
            {
                personnel.AddRange(new[]
                {
                    new Personnel { StationId = ekabStations[1].Id, AgencyId = ekabAgency.Id, Name = "Elena Kwnsta", Rank = "Emergency Doctor", BadgeNumber = "07Η25", IsActive = true },
                    new Personnel { StationId = ekabStations[1].Id, AgencyId = ekabAgency.Id, Name = "Kostas Mitroglou", Rank = "Paramedic (Ambulance Crew)", BadgeNumber = "02Κ22", IsActive = true },
                    new Personnel { StationId = ekabStations[1].Id, AgencyId = ekabAgency.Id, Name = "Anna Titika", Rank = "Paramedic (Ambulance Crew)", BadgeNumber = "07Κ38", IsActive = true },
                    new Personnel { StationId = ekabStations[1].Id, AgencyId = ekabAgency.Id, Name = "Petros Mantalos", Rank = "Ambulance Driver", BadgeNumber = "01Κ77", IsActive = true },
                    new Personnel { StationId = ekabStations[1].Id, AgencyId = ekabAgency.Id, Name = "Yannis Daferis", Rank = "Ambulance Driver", BadgeNumber = "01Κ18", IsActive = true }
                });
            }
        }

        // Police Personnel
        if (policeStations.Any())
        {
            personnel.AddRange(new[]
            {
                new Personnel { StationId = policeStations[0].Id, AgencyId = policeAgency.Id, Name = "Georgios Karaiskakis", Rank = "Police Director", BadgeNumber = "2121578", IsActive = true },
                new Personnel { StationId = policeStations[0].Id, AgencyId = policeAgency.Id, Name = "Athina Stavrou", Rank = "Police Sergeant (Investigative Duty)", BadgeNumber = "212788", IsActive = true },
                new Personnel { StationId = policeStations[0].Id, AgencyId = policeAgency.Id, Name = "Michalis Nomikos", Rank = "Police Deputy Lieutenant", BadgeNumber = "21264564", IsActive = true },
                new Personnel { StationId = policeStations[0].Id, AgencyId = policeAgency.Id, Name = "Eleni Taxiarhou", Rank = "Police Constable", BadgeNumber = "877898", IsActive = true },
                new Personnel { StationId = policeStations[0].Id, AgencyId = policeAgency.Id, Name = "Petros Iwakim", Rank = "Police Constable", BadgeNumber = "241574", IsActive = true }
            });

            if (policeStations.Count > 1)
            {
                personnel.AddRange(new[]
                {
                    new Personnel { StationId = policeStations[1].Id, AgencyId = policeAgency.Id, Name = "Eleni Soukalo", Rank = "Police Major General", BadgeNumber = "115458", IsActive = true },
                    new Personnel { StationId = policeStations[1].Id, AgencyId = policeAgency.Id, Name = "Christina Fylaki", Rank = "Police Deputy Lieutenant", BadgeNumber = "213485", IsActive = true },
                    new Personnel { StationId = policeStations[1].Id, AgencyId = policeAgency.Id, Name = "Andreas Peripoliou", Rank = "Police Deputy Lieutenant", BadgeNumber = "22378", IsActive = true },
                    new Personnel { StationId = policeStations[1].Id, AgencyId = policeAgency.Id, Name = "Dimitra Matsouka", Rank = "Police Constable", BadgeNumber = "212123", IsActive = true },
                    new Personnel { StationId = policeStations[1].Id, AgencyId = policeAgency.Id, Name = "Nikos Kykloforias", Rank = "Police Constable", BadgeNumber = "211245", IsActive = true }
                });
            }
        }

        if (personnel.Any())
        {
            context.Personnel.AddRange(personnel);
            await context.SaveChangesAsync();
        }
    }

    public static async Task SeedIncidents(IncidentManagementDbContext context)
    {
        // Check if incidents are already seeded
        if (await context.Incidents.AnyAsync())
            return; // Already seeded

        // Get agencies from database
        var agencies = await context.Agencies.ToListAsync();
        var fireAgency = agencies.FirstOrDefault(a => a.Code == "FIRE");
        var coastGuardAgency = agencies.FirstOrDefault(a => a.Code == "HCG");
        var ekabAgency = agencies.FirstOrDefault(a => a.Code == "EKAB");
        var policeAgency = agencies.FirstOrDefault(a => a.Code == "POLICE");

        if (fireAgency == null || coastGuardAgency == null || ekabAgency == null || policeAgency == null)
        {
            throw new InvalidOperationException("Agencies must be seeded before incidents");
        }

        // Get stations and users from database
        var allStations = await context.Stations.ToListAsync();
        var fireStations = allStations.Where(s => s.AgencyId == fireAgency.Id).ToList();
        var coastGuardStations = allStations.Where(s => s.AgencyId == coastGuardAgency.Id).ToList();
        var ekabStations = allStations.Where(s => s.AgencyId == ekabAgency.Id).ToList();
        var policeStations = allStations.Where(s => s.AgencyId == policeAgency.Id).ToList();

        var users = await context.Users.ToListAsync();

        // Sample incidents for all agencies
        var incidents = new List<Incident>();

        // Fire Service Incidents
        if (fireStations.Count > 0)
        {
            var fireDispatcher = users.First(u => u.SupabaseUserId == "fire-dispatcher-1");
            incidents.AddRange(new[]
            {
                new Incident
                {
                    StationId = fireStations[0].Id,
                    AgencyId = fireAgency.Id,
                    Address = "Οδός Πατησίων 123, Αθήνα",
                    Street = "Πατησίων",
                    StreetNumber = "123",
                    City = "Αθήνα",
                    Region = "Αττική",
                    PostalCode = "11253",
                    Country = "Ελλάδα",
                    Latitude = 37.9908,
                    Longitude = 23.7383,
                    Status = IncidentStatus.OnGoing,
                    Priority = IncidentPriority.High,
                    Notes = "Φλόγα στον 2ο.",
                    CreatedByUserId = fireDispatcher.Id,
                    CreatedAt = DateTime.UtcNow.AddHours(-2),
                    MainCategory = "ΑΣΤΙΚΕΣ ΠΥΡΚΑΓΙΕΣ",
                    SubCategory = "ΚΤΙΡΙΟ ΚΑΤΟΙΚΙΑΣ"
                },
                new Incident
                {
                    StationId = fireStations[0].Id,
                    AgencyId = fireAgency.Id,
                    Address = "Λεωφόρος Κηφισίας, κοντά στο Μετρό",
                    Latitude = 38.0562,
                    Longitude = 23.7925,
                    Status = IncidentStatus.Closed,
                    Priority = IncidentPriority.Normal,
                    Notes = "Τρόλεϊ.",
                    CreatedByUserId = fireDispatcher.Id,
                    CreatedAt = DateTime.UtcNow.AddHours(-4),
                    IsClosed = true,
                    ClosedAt = DateTime.UtcNow.AddHours(-3),
                    MainCategory = "ΑΣΤΙΚΕΣ ΠΥΡΚΑΓΙΕΣ",
                    SubCategory = "ΜΕΤΑΦΟΡΙΚΑ ΜΕΣΑ"
                },
                new Incident
                {
                    StationId = fireStations[0].Id,
                    AgencyId = fireAgency.Id,
                    Address = "Λεωφόρος Βεΐκου 45, Γαλάτσι",
                    Street = "Λεωφόρος Βεΐκου",
                    StreetNumber = "45",
                    City = "Γαλάτσι",
                    Region = "Αττική",
                    PostalCode = "11147",
                    Country = "Ελλάδα",
                    Latitude = 38.0200,
                    Longitude = 23.7600,
                    Status = IncidentStatus.Closed,
                    Priority = IncidentPriority.High,
                    Notes = "Άτομα εντός",
                    CreatedByUserId = fireDispatcher.Id,
                    CreatedAt = DateTime.UtcNow.AddHours(-1.5),
                    MainCategory = "ΑΣΤΙΚΕΣ ΠΥΡΚΑΓΙΕΣ",
                    SubCategory = "ΚΤΙΡΙΟ ΚΑΤΟΙΚΙΑΣ"
                },
                new Incident
                {
                    StationId = fireStations[0].Id,
                    AgencyId = fireAgency.Id,
                    Address = "Άνω Λιόσια, Αθήνα",
                    Latitude = 38.0830,
                    Longitude = 23.6670,
                    Status = IncidentStatus.Controlled,
                    Priority = IncidentPriority.Normal,
                    Notes = "Κοντά στο καζίνο.",
                    CreatedByUserId = fireDispatcher.Id,
                    CreatedAt = DateTime.UtcNow.AddHours(-2.3),
                    MainCategory = "ΔΑΣΙΚΕΣ ΠΥΡΚΑΓΙΕΣ",
                    SubCategory = "ΓΕΩΡΓΙΚΗ ΕΚΤΑΣΗ"                  
                },
                new Incident
                {
                    StationId = fireStations[0].Id,
                    AgencyId = fireAgency.Id,
                    Address = "Οδός Αρριανού 10, Πετράλωνα, Αθήνα",
                    Street = "Αρριανού",
                    StreetNumber = "10",
                    City = "Αθήνα",
                    Region = "Αττική",
                    PostalCode = "11851",
                    Country = "Ελλάδα",
                    Latitude = 37.9700,
                    Longitude = 23.7150,
                    Status = IncidentStatus.OnGoing,
                    Priority = IncidentPriority.High,
                    Notes = "Σε προαύλιο χώρο εταιρίας.",
                    CreatedByUserId = fireDispatcher.Id,
                    CreatedAt = DateTime.UtcNow.AddHours(-0.8),
                    MainCategory = "ΑΣΤΙΚΕΣ ΠΥΡΚΑΓΙΕΣ",
                    SubCategory = "ΒΙΟΜΗΧΑΝΙΑ – ΒΙΟΤΕΧΝΙΑ"
                }
            });

            if (fireStations.Count > 1)
            {
                incidents.AddRange(new[]
                {
                    new Incident
                    {
                        StationId = fireStations[1].Id,
                        AgencyId = fireAgency.Id,
                        Address = "Λεωφόρος Ηρώων Πολυτεχνείου, Πειραιάς",
                        Latitude = 37.9470,
                        Longitude = 23.6390,
                        Status = IncidentStatus.Closed,
                        Priority = IncidentPriority.Normal,
                        Notes = "ΙΧ ολοσχερώς. Στο σημείο η ΕΛ.ΑΣ.",
                        CreatedByUserId = fireDispatcher.Id,
                        CreatedAt = DateTime.UtcNow.AddHours(-3.5),
                        IsClosed = true,
                        ClosedAt = DateTime.UtcNow.AddHours(-2.7),
                        MainCategory = "ΔΑΣΙΚΕΣ ΠΥΡΚΑΓΙΕΣ",
                        SubCategory = "ΓΕΩΡΓΙΚΗ ΕΚΤΑΣΗ"                       
                    },
                    new Incident
                    {
                        StationId = fireStations[1].Id,
                        AgencyId = fireAgency.Id,
                        Address = "Πάρνηθα, κοντά στην είσοδο Δροσιάς",
                        Latitude = 38.2000,
                        Longitude = 23.7500,
                        Status = IncidentStatus.Controlled,
                        Priority = IncidentPriority.High,
                        Notes = "Κοντά στο καταφύγιο. Δύσκολη πρόσβαση. Αφροδίτη Τατοΐου καθοδόν.",
                        CreatedByUserId = fireDispatcher.Id,
                        CreatedAt = DateTime.UtcNow.AddHours(-1.2),
                        MainCategory = "ΔΑΣΙΚΕΣ ΠΥΡΚΑΓΙΕΣ",
                        SubCategory = "ΧΟΡΤΟΛΙΒΑΔΙΚΗ ΕΚΤΑΣΗ"                      
                    },
                    new Incident
                    {
                        StationId = fireStations[1].Id,
                        AgencyId = fireAgency.Id,
                        Address = "Οδός Διονύσου 55, Κηφισιά",
                        Latitude = 38.1200,
                        Longitude = 23.8100,
                        Status = IncidentStatus.PartialControl,
                        Priority = IncidentPriority.High,
                        Notes = "Εργοαστίο MetalWorks. Πυροσβεστικά οχήματα και προσωπικό επί τόπου.",
                        CreatedByUserId = fireDispatcher.Id,
                        CreatedAt = DateTime.UtcNow.AddHours(-2),
                        MainCategory = "ΑΣΤΙΚΕΣ ΠΥΡΚΑΓΙΕΣ",
                        SubCategory = "ΒΙΟΜΗΧΑΝΙΑ – ΒΙΟΤΕΧΝΙΑ"
                    },
                    new Incident
                    {
                        StationId = fireStations[1].Id,
                        AgencyId = fireAgency.Id,
                        Address = "Πλατεία Ομονοίας, Αθήνα",
                        Latitude = 37.9890,
                        Longitude = 23.7320,
                        Status = IncidentStatus.OnGoing,
                        Priority = IncidentPriority.Normal,
                        Notes = "Σε ηλεκτρικό λεωφορείο στην πλατεία Ομόνοιας. Επιβάτες εκκενώθηκαν.",
                        CreatedByUserId = fireDispatcher.Id,
                        CreatedAt = DateTime.UtcNow.AddHours(-0.5),
                        MainCategory = "ΑΣΤΙΚΕΣ ΠΥΡΚΑΓΙΕΣ",
                        SubCategory = "ΜΕΤΑΦΟΡΙΚΑ ΜΕΣΑ"
                    }
                });
            }
        }

        // Coast Guard Incidents
        if (coastGuardStations.Any())
        {
            var cgDispatcher = users.First(u => u.SupabaseUserId == "cg-dispatcher-1");
            incidents.AddRange(new[]
            {
                new Incident
                {
                    StationId = coastGuardStations[0].Id,
                    AgencyId = coastGuardAgency.Id,
                    MainCategory = "SeaRescue",
                    SubCategory = "Missing Mariner / Passenger",
                    Notes = "Αγνοούμενο αλιευτικό σκάφος στον Σαρωνικό Κόλπο",
                    Address = "Σαρωνικός Κόλπος, 5 ναυτικά μίλια από την Αίγινα",
                    Latitude = 37.7500,
                    Longitude = 23.4333,
                    Status = IncidentStatus.OnGoing,
                    Priority = IncidentPriority.Critical,
                    CreatedByUserId = cgDispatcher.Id,
                    CreatedAt = DateTime.UtcNow.AddHours(-1)
                },
                new Incident
                {
                    StationId = coastGuardStations[0].Id,
                    AgencyId = coastGuardAgency.Id,
                    MainCategory = "EnvironmentalIncidents",
                    SubCategory = "Marine Pollution",
                    Notes = "Διαρροή πετρελαίου κοντά στο λιμάνι του Πειραιά",
                    Address = "Λιμάνι Πειραιά, Τερματικός Σταθμός Container 2",
                    Latitude = 37.937091037118606,
                    Longitude = 23.604789052875958,
                    Status = IncidentStatus.PartialControl,
                    Priority = IncidentPriority.High,
                    CreatedByUserId = cgDispatcher.Id,
                    CreatedAt = DateTime.UtcNow.AddHours(-3)
                },
                new Incident
                {
                    StationId = coastGuardStations[0].Id,
                    AgencyId = coastGuardAgency.Id,
                    MainCategory = "MedicalIncidents",
                    SubCategory = "Medical Emergency at Sea",
                    Notes = "Τραυματισμός πληρώματος σε εμπορικό πλοίο",
                    Address = "Λιμάνι Θεσσαλονίκης, Αποβάθρα 3",
                    Latitude = 40.547252955014486,
                    Longitude = 22.857356417273305,
                    Status = IncidentStatus.Controlled,
                    Priority = IncidentPriority.High,
                    CreatedByUserId = cgDispatcher.Id,
                    CreatedAt = DateTime.UtcNow.AddHours(-2)
                },
                new Incident
                {
                    StationId = coastGuardStations[0].Id,
                    AgencyId = coastGuardAgency.Id,
                    MainCategory = "SeaRescue",
                    SubCategory = "Man Overboard",
                    Notes = "Άνδρας έπεσε στη θάλασσα από φέριμποτ",
                    Address = "Στενό Σαντορίνης",
                    Latitude = 36.3932,
                    Longitude = 25.4615,
                    Status = IncidentStatus.OnGoing,
                    Priority = IncidentPriority.Critical,
                    CreatedByUserId = cgDispatcher.Id,
                    CreatedAt = DateTime.UtcNow.AddHours(-4)
                },
                new Incident
                {
                    StationId = coastGuardStations[0].Id,
                    AgencyId = coastGuardAgency.Id,
                    MainCategory = "Security",
                    SubCategory = "Vessel Inspection / Boarding",
                    Notes = "Πλεούμενο εμπόδιο λόγω επιπλέοντος κοντέινερ",
                    Address = "Κρήτη, κοντά στο λιμάνι Ηρακλείου",
                    Latitude = 35.3400,
                    Longitude = 25.1300,
                    Status = IncidentStatus.PartialControl,
                    Priority = IncidentPriority.Normal,
                    CreatedByUserId = cgDispatcher.Id,
                    CreatedAt = DateTime.UtcNow.AddHours(-6)
                },
                new Incident
                {
                    StationId = coastGuardStations[0].Id,
                    AgencyId = coastGuardAgency.Id,
                    MainCategory = "SeaRescue",
                    SubCategory = "Shipwreck / Capsize",
                    Notes = "Αλιευτικό σκάφος αναποδογύρισε κοντά στην Κέρκυρα",
                    Address = "Ακτή Κέρκυρας",
                    Latitude = 39.6240,
                    Longitude = 19.9217,
                    Status = IncidentStatus.OnGoing,
                    Priority = IncidentPriority.Critical,
                    CreatedByUserId = cgDispatcher.Id,
                    CreatedAt = DateTime.UtcNow.AddHours(-2.5)
                }
            });

            if (coastGuardStations.Count > 1)
            {
                incidents.AddRange(new[]
                {
                    new Incident
                    {
                        StationId = coastGuardStations[1].Id,
                        AgencyId = coastGuardAgency.Id,
                        MainCategory = "EnvironmentalIncidents",
                        SubCategory = "Marine Pollution",
                        Address = "Λιμάνι Ραφήνας, Βόρεια Αποβάθρα",
                        Latitude = 38.0167,
                        Longitude = 24.0250,
                        Status = IncidentStatus.PartialControl,
                        Priority = IncidentPriority.High,
                        Notes = "Λεκές πετρελαίου κοντά στη Ραφήνα. Καθαρισμός σε εξέλιξη. Ειδοποιήθηκαν οι τοπικές αρχές.",
                        CreatedByUserId = cgDispatcher.Id,
                        CreatedAt = DateTime.UtcNow.AddHours(-3.5)
                    },
                    new Incident
                    {
                        StationId = coastGuardStations[1].Id,
                        AgencyId = coastGuardAgency.Id,
                        MainCategory = "MedicalIncidents",
                        SubCategory = "Medical Emergency at Sea",
                        Address = "Κόλπος Ζακύνθου",
                        Latitude = 37.7870,
                        Longitude = 20.8980,
                        Status = IncidentStatus.Controlled,
                        Priority = IncidentPriority.High,
                        Notes = "Βαριά τραυματισμένος δύτης κοντά στη Ζάκυνθο. Απαιτείται άμεση εκκένωση.",
                        CreatedByUserId = cgDispatcher.Id,
                        CreatedAt = DateTime.UtcNow.AddHours(-1)
                    },
                    new Incident
                    {
                        StationId = coastGuardStations[1].Id,
                        AgencyId = coastGuardAgency.Id,
                        MainCategory = "SeaRescue",
                        SubCategory = "Missing Mariner / Passenger",
                        Address = "Νοτιοανατολικά των Αλοννήσου",
                        Latitude = 39.1460,
                        Longitude = 23.9090,
                        Status = IncidentStatus.OnGoing,
                        Priority = IncidentPriority.Critical,
                        Notes = "Αγνοούμενο ιστιοπλοϊκό στα Σποράδα. Αναφέρθηκε οπτική επαφή τελευταία φορά στις 10:45.",
                        CreatedByUserId = cgDispatcher.Id,
                        CreatedAt = DateTime.UtcNow.AddHours(-5)
                    }
                } );
            }
        }

        // EKAB Incidents
        if (ekabStations.Any())
        {
            var ekabDispatcher = users.First(u => u.SupabaseUserId == "ekab-dispatcher-1");
            incidents.AddRange(new[]
            {
                new Incident
                {
                    StationId = ekabStations[0].Id,
                    AgencyId = ekabAgency.Id,
                    MainCategory = "Emergencies",
                    SubCategory = "Cardiac Arrest / Cardiac Event",
                    Notes = "Πόνος στο στήθος, πιθανό έμφραγμα μυοκαρδίου. Ασθενής σε συνείδηση, ζωτικά σταθερά. Μεταφορά σε καρδιολογική μονάδα.",
                    Address = "Πλατεία Συντάγματος, Αθήνα",
                    City = "Αθήνα",
                    Region = "Αττική",
                    PostalCode = "10563",
                    Country = "Ελλάδα",
                    Latitude = 37.9755,
                    Longitude = 23.7348,
                    Status = IncidentStatus.OnGoing,
                    Priority = IncidentPriority.Critical,
                    CreatedByUserId = ekabDispatcher.Id,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-45)
                },
                new Incident
                {
                    StationId = ekabStations[0].Id,
                    AgencyId = ekabAgency.Id,
                    MainCategory = "TraumaIncidents",
                    SubCategory = "Road Traffic Accident",
                    Notes = "Σύγκρουση πολλών οχημάτων με τραυματισμούς. 3 οχήματα εμπλεκόμενα. 2 ελαφρά τραυματίες, 1 σοβαρός.",
                    Address = "Αττική Οδός, Έξοδος 7",
                    Latitude = 38.0667,
                    Longitude = 23.8000,
                    Status = IncidentStatus.PartialControl,
                    Priority = IncidentPriority.High,
                    CreatedByUserId = ekabDispatcher.Id,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-20)
                },
                new Incident
                {
                    StationId = ekabStations[0].Id,
                    AgencyId = ekabAgency.Id,
                    MainCategory = "TraumaIncidents",
                    SubCategory = "Fall from Height",
                    Notes = "Άτομο έπεσε από σκάλα, κάταγμα στο πόδι",
                    Address = "Οδός Ερμού 50, Αθήνα",
                    Latitude = 37.9838,
                    Longitude = 23.7275,
                    Status = IncidentStatus.OnGoing,
                    Priority = IncidentPriority.High,
                    CreatedByUserId = ekabDispatcher.Id,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-15)
                },
                new Incident
                {
                    StationId = ekabStations[0].Id,
                    AgencyId = ekabAgency.Id,
                    MainCategory = "Emergencies",
                    SubCategory = "Respiratory Distress / Arrest",
                    Notes = "Ασθενής με δύσπνοια και υποξία",
                    Address = "Κηφισίας 123, Μαρούσι",
                    Latitude = 38.0500,
                    Longitude = 23.7900,
                    Status = IncidentStatus.Controlled,
                    Priority = IncidentPriority.Critical,
                    CreatedByUserId = ekabDispatcher.Id,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-25)
                },
                new Incident
                {
                    StationId = ekabStations[0].Id,
                    AgencyId = ekabAgency.Id,
                    MainCategory = "TraumaIncidents",
                    SubCategory = "Road Traffic Accident",
                    Notes = "Μοτοσικλετιστής έπεσε, τραύματα στο κεφάλι",
                    Address = "Λεωφόρος Συγγρού 150, Αθήνα",
                    Latitude = 37.9400,
                    Longitude = 23.7160,
                    Status = IncidentStatus.OnGoing,
                    Priority = IncidentPriority.Critical,
                    CreatedByUserId = ekabDispatcher.Id,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-10)
                },
                new Incident
                {
                    StationId = ekabStations[0].Id,
                    AgencyId = ekabAgency.Id,
                    MainCategory = "Emergencies",
                    SubCategory = "Cardiac Arrest / Cardiac Event",
                    Notes = "Ασθενής με ακανόνιστο καρδιακό ρυθμό",
                    Address = "Πλατεία Ομονοίας, Αθήνα",
                    Latitude = 37.9890,
                    Longitude = 23.7320,
                    Status = IncidentStatus.PartialControl,
                    Priority = IncidentPriority.High,
                    CreatedByUserId = ekabDispatcher.Id,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-35)
                }
            });

            if (ekabStations.Count > 1)
            {
                incidents.AddRange(new[]
                {
                    new Incident
                    {
                        StationId = ekabStations[1].Id,
                        AgencyId = ekabAgency.Id,
                        MainCategory = "TraumaIncidents",
                        SubCategory = "Fall from Height",
                        Notes = "Πτώση ηλικιωμένου από πεζοδρόμιο, πιθανό κάταγμα.",                       
                        Address = "Οδός Σταδίου 22, Αθήνα",
                        Latitude = 37.9750,
                        Longitude = 23.7360,
                        Status = IncidentStatus.Controlled,
                        Priority = IncidentPriority.Normal,
                        CreatedByUserId = ekabDispatcher.Id,
                        CreatedAt = DateTime.UtcNow.AddMinutes(-18)
                    },
                    new Incident
                    {
                        StationId = ekabStations[1].Id,
                        AgencyId = ekabAgency.Id,
                        MainCategory = "Emergencies",
                        SubCategory = "Anaphylaxis (Severe Allergy)",
                        Notes = "Παιδί με σοβαρή αλλεργική αντίδραση. Ενέσιμη αδρεναλίνη σε χρήση. Παρακολούθηση ζωτικών.",                       
                        Address = "Οδός Πατησίων 88, Αθήνα",
                        Latitude = 37.9900,
                        Longitude = 23.7370,
                        Status = IncidentStatus.OnGoing,
                        Priority = IncidentPriority.Critical,
                        CreatedByUserId = ekabDispatcher.Id,
                        CreatedAt = DateTime.UtcNow.AddMinutes(-12)
                    },
                    new Incident
                    {
                        StationId = ekabStations[1].Id,
                        AgencyId = ekabAgency.Id,
                        MainCategory = "TraumaIncidents",
                        SubCategory = "Road Traffic Accident",
                        Notes = "Σύγκρουση αυτοκινήτου με πεζό. Πεζός με τραύματα στα κάτω άκρα, μεταφορά στο νοσοκομείο.",
                        Address = "Λεωφόρος Κηφισίας, Μαρούσι",
                        Latitude = 38.0500,
                        Longitude = 23.7900,
                        Status = IncidentStatus.PartialControl,
                        Priority = IncidentPriority.High,
                        CreatedByUserId = ekabDispatcher.Id,
                        CreatedAt = DateTime.UtcNow.AddMinutes(-40)
                    }
            });
            }
        }

        // Police Incidents
        if (policeStations.Any())
        {
            var policeDispatcher = users.First(u => u.SupabaseUserId == "police-dispatcher-1");
            incidents.AddRange(new[]
            {
                new Incident
                {
                    StationId = policeStations[0].Id,
                    AgencyId = policeAgency.Id,
                    MainCategory = "ViolentIncidents",
                    SubCategory = "Robbery / Armed Robbery",
                    Notes = "Ένοπλη ληστεία σε περίπτερο",
                    Address = "Οδός Ερμού 45, Αθήνα",
                    Street = "Ερμού",
                    StreetNumber = "45",
                    City = "Αθήνα",
                    Region = "Αττική",
                    PostalCode = "10563",
                    Country = "Ελλάδα",
                    Latitude = 37.976270,
                    Longitude = 23.729421,
                    Status = IncidentStatus.OnGoing,
                    Priority = IncidentPriority.High,
                    CreatedByUserId = policeDispatcher.Id,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-30)
                },
                new Incident
                {
                    StationId = policeStations[0].Id,
                    AgencyId = policeAgency.Id,
                    MainCategory = "TrafficIncidents",
                    SubCategory = "Road Blockage / Obstruction",
                    Notes = "Παράβαση υπερβολικής ταχύτητας σε αυτοκινητόδρομο",
                    Address = "Αττική Οδός, Χλμ 15",
                    Latitude = 38.0667,
                    Longitude = 23.8000,
                    Status = IncidentStatus.Closed,
                    Priority = IncidentPriority.Low,
                    CreatedByUserId = policeDispatcher.Id,
                    CreatedAt = DateTime.UtcNow.AddHours(-1),
                    IsClosed = true,
                    ClosedAt = DateTime.UtcNow.AddMinutes(-45)
                },
                new Incident
                {
                    StationId = policeStations[0].Id,
                    AgencyId = policeAgency.Id,
                    MainCategory = "TheftIncidents",
                    SubCategory = "Burglary / Home Break-in",
                    Notes = "Διάρρηξη διαμερίσματος στον Νέο Κόσμο",
                    Address = "Οδός Σολωμού 12, Νέος Κόσμος",
                    Latitude = 37.9600,
                    Longitude = 23.7300,
                    Status = IncidentStatus.OnGoing,
                    Priority = IncidentPriority.High,
                    CreatedByUserId = policeDispatcher.Id,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-50)
                },
                new Incident
                {
                    StationId = policeStations[0].Id,
                    AgencyId = policeAgency.Id,
                    MainCategory = "TrafficIncidents",
                    SubCategory = "Vehicle Collision",
                    Notes = "Σύγκρουση δύο αυτοκινήτων στο Χαλάνδρι",
                    Address = "Λεωφόρος Κηφισίας 200, Χαλάνδρι",
                    Latitude = 38.0450,
                    Longitude = 23.7990,
                    Status = IncidentStatus.PartialControl,
                    Priority = IncidentPriority.Normal,
                    CreatedByUserId = policeDispatcher.Id,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-25)
                },
                new Incident
                {
                    StationId = policeStations[0].Id,
                    AgencyId = policeAgency.Id,
                    MainCategory = "PublicGatherings",
                    SubCategory = "Protests / Demonstrations",
                    Notes = "Ομάδα διαδηλωτών στο Σύνταγμα",
                    Address = "Πλατεία Συντάγματος, Αθήνα",
                    Latitude = 37.975456,
                    Longitude = 23.735939,
                    Status = IncidentStatus.OnGoing,
                    Priority = IncidentPriority.Normal,
                    CreatedByUserId = policeDispatcher.Id,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-15)
                },
                new Incident
                {
                    StationId = policeStations[0].Id,
                    AgencyId = policeAgency.Id,
                    MainCategory = "TheftIncidents",
                    SubCategory = "Vehicle Theft",
                    Notes = "Κλοπή μοτοσικλέτας στη Νέα Σμύρνη",
                    Address = "Οδός Ελευθερίου Βενιζέλου 10, Νέα Σμύρνη",
                    Latitude = 37.9500,
                    Longitude = 23.7000,
                    Status = IncidentStatus.Closed,
                    Priority = IncidentPriority.High,
                    CreatedByUserId = policeDispatcher.Id,
                    CreatedAt = DateTime.UtcNow.AddMinutes(-40)
                }
            });

            if (policeStations.Count > 1)
            {
                incidents.AddRange(new[]
                {
                    new Incident
                    {
                        StationId = policeStations[1].Id,
                        AgencyId = policeAgency.Id,
                        MainCategory = "Narcotics",
                        SubCategory = "Drug Possession / Trafficking",
                        Notes = "Κατάσχεση ναρκωτικών στην Ομόνοια. Συνελήφθησαν δύο ύποπτοι. Ναρκωτικά κατασχέθηκαν.",                       
                        Address = "Πλατεία Ομονοίας, Αθήνα",
                        Latitude = 37.9890,
                        Longitude = 23.7320,
                        Status = IncidentStatus.Closed,
                        Priority = IncidentPriority.High,
                        CreatedByUserId = policeDispatcher.Id,
                        CreatedAt = DateTime.UtcNow.AddHours(-2),
                        IsClosed = true,
                        ClosedAt = DateTime.UtcNow.AddHours(-1)
                    },
                    new Incident
                    {
                        StationId = policeStations[1].Id,
                        AgencyId = policeAgency.Id,
                        MainCategory = "TrafficIncidents",
                        SubCategory = "Road Blockage / Obstruction",
                        Address = "Οδός Διονυσίου Αρεοπαγίτου, Αθήνα",
                        Latitude = 37.9715,
                        Longitude = 23.7250,
                        Status = IncidentStatus.Closed,
                        Priority = IncidentPriority.Low,
                        Notes = "Παράνομη στάθμευση οχήματος σε πεζόδρομο. Ο οδηγός εντοπίστηκε και βεβαιώθηκε.",
                        CreatedByUserId = policeDispatcher.Id,
                        CreatedAt = DateTime.UtcNow.AddMinutes(-55)
                    },
                    new Incident
                    {
                        StationId = policeStations[1].Id,
                        AgencyId = policeAgency.Id,
                        MainCategory = "TheftIncidents",
                        SubCategory = "Theft / Shoplifting",
                        Notes = "Βανδαλισμός σε δημόσια εγκατάσταση στο Κολωνάκι. Καταστροφές σε εγκαταστάσεις. Αναζητούνται ύποπτοι.",           
                        Address = "Οδός Μητροπόλεως 18, Κολωνάκι, Αθήνα",
                        Latitude = 37.9850,
                        Longitude = 23.7380,
                        Status = IncidentStatus.Closed,
                        Priority = IncidentPriority.Normal,
                        CreatedByUserId = policeDispatcher.Id,
                        CreatedAt = DateTime.UtcNow.AddMinutes(-20)
                    }
            });
            }
        }

        if (incidents.Any())
        {
            context.Incidents.AddRange(incidents);
            await context.SaveChangesAsync();
        }
    }

    public static async Task SeedAssignments(IncidentManagementDbContext context)
    {
        // Check if assignments are already seeded
        if (await context.Assignments.AnyAsync())
            return; // Already seeded

        // Get data from database
        var incidents = await context.Incidents.ToListAsync();
        var vehicles = await context.Vehicles.ToListAsync();
        var personnel = await context.Personnel.ToListAsync();

        // Sample assignments
        var assignments = new List<Assignment>();

        if (incidents.Any() && vehicles.Any() && personnel.Any())
        {
            assignments.AddRange(new[]
            {
                new Assignment
                {
                    IncidentId = incidents[0].Id,
                    ResourceType = ResourceType.Vehicle,
                    ResourceId = vehicles[0].Id,
                    Status = "OnScene",
                    CreatedAt = DateTime.UtcNow.AddHours(-2)
                },
                new Assignment
                {
                    IncidentId = incidents[0].Id,
                    ResourceType = ResourceType.Personnel,
                    ResourceId = personnel[0].Id,
                    Status = "OnScene",
                    CreatedAt = DateTime.UtcNow.AddHours(-2)
                }
            });
        }

        if (assignments.Any())
        {
            context.Assignments.AddRange(assignments);
            await context.SaveChangesAsync();
        }
    }

    public static async Task SeedShifts(IncidentManagementDbContext context)
    {
        // Check if shift templates are already seeded
        if (await context.ShiftTemplates.AnyAsync())
            return; // Already seeded

        // Get fire stations from database
        var fireAgency = await context.Agencies.FirstOrDefaultAsync(a => a.Code == "FIRE");
        if (fireAgency == null)
        {
            throw new InvalidOperationException("Fire agency must be seeded before shifts");
        }

        var fireStations = await context.Stations.Where(s => s.AgencyId == fireAgency.Id).ToListAsync();

        // Sample shift template
        if (fireStations.Any())
        {
            var shiftTemplate = new ShiftTemplate
            {
                StationId = fireStations[0].Id,
                Name = "24on/48off",
                Duration = TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59)),
                RRule = "FREQ=DAILY;INTERVAL=3"
            };

            context.ShiftTemplates.Add(shiftTemplate);
            await context.SaveChangesAsync();

            // Sample shift instances for next week
            var now = DateTime.UtcNow.Date;
            var shiftInstances = new List<ShiftInstance>();

            for (int i = 0; i < 7; i += 3)
            {
                shiftInstances.Add(new ShiftInstance
                {
                    StationId = fireStations[0].Id,
                    StartsAt = now.AddDays(i).AddHours(8),
                    EndsAt = now.AddDays(i + 1).AddHours(8),
                    SourceTemplateName = "24on/48off"
                });
            }

            context.ShiftInstances.AddRange(shiftInstances);
            await context.SaveChangesAsync();
        }
    }

    public static async Task SeedCallers(IncidentManagementDbContext context)
    {
        // Check if callers are already seeded
        if (await context.Callers.AnyAsync())
            return; // Already seeded

        // Get incidents from database
        var incidents = await context.Incidents.Take(10).ToListAsync();

        if (!incidents.Any())
            return;

        var callers = new List<Caller>();

        // Add sample callers for the first few incidents
        for (int i = 0; i < Math.Min(incidents.Count, 8); i++)
        {
            var incident = incidents[i];
            
            // Add 1-3 callers per incident
            var callerCount = (i % 3) + 1;
            
            for (int j = 0; j < callerCount; j++)
            {
                callers.Add(new Caller
                {
                    IncidentId = incident.Id,
                    Name = GetSampleCallerName(i, j),
                    PhoneNumber = GetSamplePhoneNumber(i, j),
                    CalledAt = incident.CreatedAt.AddMinutes(j * 2),
                    Notes = j == 0 ? "Πρώτος καλών" : $"Επιπλέον κλήση #{j + 1}"
                });
            }
        }

        if (callers.Any())
        {
            context.Callers.AddRange(callers);
            await context.SaveChangesAsync();
        }
    }

    private static string GetSampleCallerName(int incidentIndex, int callerIndex)
    {
        var names = new[]
        {
            "Γιάννης Παπαδόπουλος", "Μαρία Κωνσταντίνου", "Νίκος Αντωνίου",
            "Ελένη Γεωργίου", "Δημήτρης Νικολάου", "Κατερίνα Μιχαήλ",
            "Αντώνης Χριστοδούλου", "Σοφία Παναγιώτου", "Κώστας Στεφάνου",
            "Άννα Δημητρίου", "Πέτρος Ιωάννου", "Μαρίνα Αλεξάνδρου"
        };
        
        var index = (incidentIndex * 3 + callerIndex) % names.Length;
        return names[index];
    }

    private static string GetSamplePhoneNumber(int incidentIndex, int callerIndex)
    {
        var baseNumbers = new[]
        {
            "6944123456", "6955234567", "6966345678", "6977456789",
            "6988567890", "6999678901", "6900789012", "6911890123",
            "6922901234", "6933012345", "6944123456", "6955234567"
        };
        
        var index = (incidentIndex * 3 + callerIndex) % baseNumbers.Length;
        return baseNumbers[index];
    }
}
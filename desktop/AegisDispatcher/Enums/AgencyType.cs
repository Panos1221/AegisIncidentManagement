using System.ComponentModel;

namespace AegisDispatcher.Models
{
    public enum AgencyType
    {
        [Description("Fire Department")]
        FireDepartment = 0,
        [Description("Coast Guard")]
        CoastGuard = 1,
        [Description("EKAB")]
        EKAB = 2,
        [Description("Police")]
        Police = 3
    }
}

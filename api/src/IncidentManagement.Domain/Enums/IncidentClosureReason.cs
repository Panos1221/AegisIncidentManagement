namespace IncidentManagement.Domain.Enums;

public enum IncidentClosureReason
{
    Action,         // ΕΝΕΡΓΕΙΑ - Agency took action
    WithoutAction,  // ΑΝΕΥ ΕΝΕΡΓΕΙΑΣ - No action taken
    PreArrival,     // ΠΡΟ ΑΦΙΞΕΩΣ - Resolved before arrival
    Cancelled,       // ΑΚΥΡΟ - Cancelled incident
    FalseAlarm      // ΨΕΥΔΗΣ ΑΝΑΓΓΕΛΙΑ - False alarm
}
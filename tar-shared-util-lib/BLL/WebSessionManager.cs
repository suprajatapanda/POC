namespace TRS.IT.SI.BusinessFacadeLayer
{
    /// <summary>
    /// Simplified WebSessionManager for console applications only.
    /// The actual caching is handled by the database through ParticipantDC.
    /// </summary>
    public class WebSessionManager
    {
        public static void SetPPTObj(Model.ParticipantInfo oPPT, int ClientTypeID = 1)
        {
            // In console mode, this is a no-op since there's no session to store in
            // The database handles all caching through ParticipantDC
        }

        public static Model.ParticipantInfo GetPPTObj()
        {
            // Always return null in console mode
            return null;
        }

        public static bool IsPPTObjValid(string sessionID, int ClientTypeID)
        {
            // In console mode, always return false to force database retrieval
            // This ensures fresh data from ParticipantDC
            return false;
        }

        public static void ResetSession()
        {
            // No-op in console mode - no session to reset
        }

        public static bool IsPPTObjDirty(string sessionID)
        {
            // Always return true in console mode to force database updates
            return true;
        }

        public static void SetPPTObjDirty(bool value)
        {
            // No-op in console mode - no session state to maintain
        }
    }
}
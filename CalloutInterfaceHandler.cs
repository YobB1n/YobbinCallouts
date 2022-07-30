// This class provides compatibility with Opus' Callout Interface Plugin.

namespace YobbinCallouts
{
    /// <summary>
    /// Functions for CalloutInterface.
    /// </summary>
    internal static class CalloutInterfaceHandler
    {
        /// <summary>
        /// Send details about the callout to the MDT.  Call this in OnCalloutDisplayed.
        /// </summary>
        /// <param name="sender">The originating callout.</param>
        /// <param name="priority">The priority of the callout (e.g. CODE 2, CODE 3).</param>
        /// <param name="agency">The agency that should be handling the callout (e.g. LSPD, LSSD).</param>
        public static void SendCalloutDetails(LSPD_First_Response.Mod.Callouts.Callout sender, string priority, string agency = "")
        {
            try
            {
                CalloutInterface.API.Functions.SendCalloutDetails(sender, priority, agency);
            }
            catch (System.Exception ex)
            {
                // insert logging here
            }
        }

        /// <summary>
        /// Sends a message to the MDT.
        /// </summary>
        /// <param name="sender">The sending callout.</param>
        /// <param name="message">The message.</param>
        public static void SendMessage(LSPD_First_Response.Mod.Callouts.Callout sender, string message)
        {
            try
            {
                CalloutInterface.API.Functions.SendMessage(sender, message);
            }
            catch (System.Exception ex)
            {
                // insert logging here
            }
        }
    }
}
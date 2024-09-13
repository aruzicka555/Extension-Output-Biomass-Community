//  Authors:  Robert M. Scheller

using Landis.Library.UniversalCohorts;
using Landis.SpatialModeling;

namespace Landis.Extension.Output.BiomassCommunity
{
    /// <summary>
    /// The pools of dead biomass for the landscape's sites.
    /// </summary>
    public static class SiteVars
    {
        private static ISiteVar<ISiteCohorts> cohorts;
        public static ISiteVar<int> MapCode;


        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes the module.
        /// </summary>
        public static void Initialize()
        {

            cohorts = PlugIn.ModelCore.GetSiteVar<ISiteCohorts>("Succession.UniversalCohorts");
            MapCode = PlugIn.ModelCore.Landscape.NewSiteVar<int>();

            if (cohorts == null)
            {
                string mesg = string.Format("Cohorts are empty.  Please double-check that this extension is compatible with your chosen succession extension.");
                throw new System.ApplicationException(mesg);
            }
        }

        //---------------------------------------------------------------------
        public static ISiteVar<ISiteCohorts> Cohorts
        {
            get
            {
                return cohorts;
            }
        }
    }
}

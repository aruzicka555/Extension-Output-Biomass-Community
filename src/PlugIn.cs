//  Authors:  Robert M. Scheller

using Landis.Core;
using Landis.Library.UniversalCohorts;
using Landis.SpatialModeling;
using System;
using System.IO;

namespace Landis.Extension.Output.BiomassCommunity
{
    public class PlugIn
        : ExtensionMain
    {
        public static readonly ExtensionType ExtType = new ExtensionType("output");
        public static readonly string ExtensionName = "Output Biomass Community";

        private IInputParameters parameters;
        private static ICore modelCore;
        private string outputMapName = "output-community-{timestep}.tif";
        public static StreamWriter CommunityLog; 
        public static StreamWriter CommunityCsv;
        private bool mapCodeCreated = false;

        //---------------------------------------------------------------------

        public PlugIn()
            : base(ExtensionName, ExtType)
        {
        }

        //---------------------------------------------------------------------

        public static ICore ModelCore
        {
            get
            {
                return modelCore;
            }
        }
        public override void AddCohortData()
        {
            return;
        }

        //---------------------------------------------------------------------

        public override void LoadParameters(string dataFile, ICore mCore)
        {
            modelCore = mCore;
            InputParametersParser parser = new InputParametersParser();
            parameters = Landis.Data.Load<IInputParameters>(dataFile, parser);
        }

        //---------------------------------------------------------------------

        public override void Initialize()
        {

            Timestep = parameters.Timestep;
            MetadataHandler.Initialize(Timestep, outputMapName);
            SiteVars.Initialize();

        }

        //---------------------------------------------------------------------

        public override void Run()
        {
            // Create community Dictionary
            //      * First, summarize every community to nearest 25 g Biomass
            //      * Assign to a Dictionary
            //      * Each Dictionary entry has a unique ID
            //      * The cell is assigned that ID
            //      * If a community matches one from earlier in the list, give previous ID
            //      * Output text file matching input from Landis.Library.Succession-vAGBinput.dll (AGB input branch in repo)
            //CreateCommunityMap();
            //      * Map is of the cell ID (int)

            int initialMapCode = 3;
            int mapCode;

            // write to community csv
            InitializeCsvCommunity();

            mapCode = initialMapCode;

            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                SiteVars.MapCode[site] = mapCode;

                foreach (ISpeciesCohorts species_cohort in SiteVars.Cohorts[site])
                {
                    foreach (ICohort cohort in species_cohort)
                    {
                        CommunityCsv.WriteLine("{0},{1},{2},{3},{4:0.0}", mapCode, species_cohort.Species.Name, cohort.Data.Age, cohort.Data.Biomass, cohort.Data.ANPP);
                    }
                }

                ++mapCode;
            }
            
            CommunityCsv.Close();

            // write to community log
            //InitializeLogCommunity();

            mapCode = initialMapCode;
            mapCode++;


            if (!mapCodeCreated)
                CreateCommunityMap();

            mapCodeCreated = true;
        }
        //---------------------------------------------------------------------

        //private void InitializeLogCommunity()
        //{
        //    string logFileName = string.Format("community-input-file-{0}.txt", ModelCore.CurrentTime);
        //    PlugIn.ModelCore.UI.WriteLine("   Opening community log file \"{0}\" ...", logFileName);

        //    try
        //    {
        //        CommunityLog = new StreamWriter(logFileName);
        //    }
        //    catch (Exception err)
        //    {
        //        string mesg = string.Format("{0}", err.Message);
        //        throw new System.ApplicationException(mesg);
        //    }

        //    CommunityLog.AutoFlush = true;

        //    //Mapcode 0-2 typically reserved for outside the universe, water, or other.
        //    CommunityLog.WriteLine("LandisData \"Initial Communities\"");  
        //    CommunityLog.WriteLine();
        //    CommunityLog.WriteLine("MapCode 0");
        //    CommunityLog.WriteLine();
        //    CommunityLog.WriteLine("MapCode 1");
        //    CommunityLog.WriteLine();
        //    CommunityLog.WriteLine("MapCode 2");
        //    CommunityLog.WriteLine();
        //}

        private void InitializeCsvCommunity()
        {
            var csvFileName = string.Format("community-input-file-{0}.csv", ModelCore.CurrentTime);
            PlugIn.ModelCore.UI.WriteLine("   Opening community csv file \"{0}\" ...", csvFileName);

            try
            {
                CommunityCsv = new StreamWriter(csvFileName);  
            }
            catch (Exception err)
            {
                string mesg = string.Format("{0}", err.Message);
                throw new System.ApplicationException(mesg);
            }

            CommunityCsv.AutoFlush = true;

            CommunityCsv.WriteLine("MapCode,SpeciesName,CohortAge,CohortBiomass,CohortANPP");
        }

        //---------------------------------------------------------------------

        //private void LogCommunity()
        //{
        //}
        
                //---------------------------------------------------------------------


        private void CreateCommunityMap()
        {
            string path = MapNames.ReplaceTemplateVars(outputMapName, PlugIn.ModelCore.CurrentTime);
            PlugIn.ModelCore.UI.WriteLine("   Writing community biomass map to {0} ...", path);

            using (IOutputRaster<IntPixel> outputRaster = modelCore.CreateRaster<IntPixel>(path, modelCore.Landscape.Dimensions))
            {
                IntPixel pixel = outputRaster.BufferPixel;
                foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                {
                    if (site.IsActive)
                        pixel.MapCode.Value = SiteVars.MapCode[site];
                    else
                        pixel.MapCode.Value = 0;

                    outputRaster.WriteBufferPixel();
                }
            }
            

        }

    }
}

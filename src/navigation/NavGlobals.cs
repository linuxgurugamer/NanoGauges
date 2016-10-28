﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nereid
{
   namespace NanoGauges
   {
      public static class NavGlobals
      {
         public static readonly Runway RUNWAY_090_SPACECENTER = new Runway("RWY 090", Coords.Create(-74.7130, -0.0486), 65.75f, 90.0f);
         public static readonly Runway RUNWAY_270_SPACECENTER = new Runway("RWY 270", Coords.Create(-74.5039, -0.0501), 65.75f, 270.0f);
         public static readonly Runway RUNWAY_090_OLDAIRFIELD = new Runway("RWY 090", Coords.Create(-71.9663, -1.5182), 65.75f, 90.0f,2.5f);
         public static readonly Runway RUNWAY_270_OLDAIRFIELD = new Runway("RWY 270", Coords.Create(-71.8900, -1.5571), 65.75f, 270.0f,2.5f);

         public static readonly Airfield AIRFIELD_SPACECENTER = new Airfield("Space Center", RUNWAY_090_SPACECENTER, RUNWAY_270_SPACECENTER);
         public static readonly Airfield AIRFIELD_OLDAIRFIELD = new Airfield("Old Airfield", RUNWAY_090_OLDAIRFIELD, RUNWAY_270_OLDAIRFIELD);

         public static readonly Airfield[] Airfields = new Airfield[] 
         {
            AIRFIELD_SPACECENTER,
            AIRFIELD_OLDAIRFIELD
         };

         private const int INDEX_NO_AIRFIELD = -1;
         private static int indexAirfield; 
         public static Airfield destinationAirfield { get; private set; }
         public static Runway landingRunway { get; private set; }
         // runway has ILS
         public static bool ILS { get; private set; }
         // vessel is inside cone of ILS beam
         public static bool InBeam { get; private set; }

         public static double distanceToRunway { get; private set; }
         public static double distanceToAirfield { get; private set; }
         public static double bearingToRunway { get; private set; }
         public static double bearingToAirfield { get; private set; }
         public static double verticalGlideslopeDeviation { get; private set; }
         public static double horizontalGlideslopeDeviation { get; private set; }


         static NavGlobals()
         {
            destinationAirfield = null;
            indexAirfield = INDEX_NO_AIRFIELD;
         }
         

         public static void ResetNavigation()
         {
            Log.Info("reset of nav point");
            indexAirfield = -1;
            destinationAirfield = null;
            landingRunway = null;
            ILS = false;
            distanceToRunway = double.MaxValue;
            distanceToAirfield = double.MaxValue;
            verticalGlideslopeDeviation = double.MaxValue;
            horizontalGlideslopeDeviation = double.MaxValue;
            bearingToAirfield = 0.0;
            bearingToRunway = 0.0;
         }

         public static void Update()
         {
            Vessel vessel = FlightGlobals.ActiveVessel;
            if (vessel == null) return;

            if(destinationAirfield == null)  return;

            distanceToAirfield = NavUtils.DistanceToAirfield(vessel, destinationAirfield);
            bearingToAirfield = NavUtils.InitialBearingToAirfield(vessel, destinationAirfield);

            // do this not to often and not in an airfield
            if(landingRunway==null || distanceToAirfield> 2000 || !InBeam)
            {
               landingRunway = destinationAirfield.GetLandingRunwayForBearing(bearingToAirfield);
               ILS = landingRunway.HasILS;
            }
            
            if(landingRunway!=null)
            {
               // Bearing to runway
               bearingToRunway = NavUtils.InitialBearingToRunway(vessel, landingRunway);
               if (bearingToRunway < 0)
               {
                  bearingToRunway = 360 - bearingToRunway;
               }
               //
               // distance to runway
               distanceToRunway = NavUtils.DistanceToRunway(vessel, landingRunway);
               //
               // glide slope
               horizontalGlideslopeDeviation = NavUtils.HorizontalGlideSlopeDeviation(bearingToRunway, landingRunway);
               verticalGlideslopeDeviation = NavUtils.VerticalGlideSlopeDeviation(vessel, landingRunway);
               //
               // check if we are inside the ILS cone
               bool insideHorizontalBeam = NavUtils.InsideCone(bearingToRunway, landingRunway.To, landingRunway.ILSCone);
               bool insideVerticalBeam = NavUtils.InsideCone(verticalGlideslopeDeviation, 0.0, landingRunway.ILSCone);
               InBeam = insideHorizontalBeam && insideVerticalBeam && distanceToRunway <= landingRunway.ILSRange;

            }
            else
            {
               bearingToRunway = 0.0;
               distanceToRunway = double.MaxValue;
               InBeam = false;
               horizontalGlideslopeDeviation = double.MaxValue;
               verticalGlideslopeDeviation = double.MaxValue;
            }


         }

         public static void SetDestinationAirfield(Airfield airfield)
         {
            if(airfield!=null)
            {
               Log.Info("set navigation airfield to "+airfield);
               destinationAirfield = airfield;
               Update();
            }
            else
            {
               Log.Info("set navigation airfield cleared");
               ResetNavigation();
            }
         }

         public static void SelectNextAirfield()
         {
            indexAirfield++;
            if (indexAirfield >= Airfields.Length) indexAirfield = INDEX_NO_AIRFIELD;
            if (indexAirfield == INDEX_NO_AIRFIELD)
            {
               SetDestinationAirfield(null);
            }
            else
            {
               SetDestinationAirfield(Airfields[indexAirfield]);
            }
         }
      }
   }

}

﻿using System;
using UnityEngine;


namespace Nereid
{
   namespace NanoGauges
   {

      public abstract class AbstractCompassGauge : HorizontalGauge
      {
         private static readonly Texture2D SCALE00 = Utils.GetTexture("Nereid/NanoGauges/Resource/COMPASS00-scale");
         private static readonly Texture2D SCALE0B = Utils.GetTexture("Nereid/NanoGauges/Resource/COMPASS0B-scale");

         private const int X_FLAG_RELATIVE = 14;
         private readonly Flag relativeFlag;

         private float scaleOffset = 0;

         public AbstractCompassGauge(int id, Texture2D skin)
            : base(id, skin, SCALE00,SCALE0B)
         {
            relativeFlag = new RFlag(this);
         }

         protected abstract float GetDegrees();

         protected override void DrawFlags()
         {
            base.DrawFlags();
            relativeFlag.Draw(X_FLAG_RELATIVE, 0);
         }

         protected void Relative()
         {
            SecondaryScale();
            relativeFlag.Down();
         }

         protected void Absolut()
         {
            PrimaryScale();
            relativeFlag.Up();
         }


         protected override float GetScaleOffset()
         {
            float offset = ( GetDegrees() % 360 )  / (float)SCALE_WIDTH;
            if (!IsOn() || !IsInLimits()) return this.scaleOffset;
            this.scaleOffset = offset;
            return offset;
         }
      }
   }
}

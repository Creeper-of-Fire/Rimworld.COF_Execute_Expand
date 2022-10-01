using UnityEngine;
using Verse;

namespace COF_Torture.Things
{
    public class Building_IronMaiden : Building_TortureBed
    {
        public Graphic graphic; //必定绘制
        public Graphic graphic_top;
        public Graphic graphic_top_using;
        public Graphic graphic_blood;
        public Graphic graphic_blood_top;
        public Graphic graphic_blood_top_using; 

        public override void Draw()
        {
            base.Draw();
            IntVec3 position = this.Position;
            Rot4 north = Rot4.North;
            Vector3 shiftedWithAltitude;
            setGraphic();
            //shiftedWithAltitude = position.ToVector3ShiftedWithAltitude(AltitudeLayer.Building);
            //graphic.Draw(shiftedWithAltitude, north, (Thing)this);
            if (isUsing)
            {
                //关上的盖子
                shiftedWithAltitude = position.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteOverheadLow);
                graphic_top_using.Draw(shiftedWithAltitude, north, (Thing)this);
            }
            else
            {
                //打开的盖子
                shiftedWithAltitude = position.ToVector3ShiftedWithAltitude(AltitudeLayer.Building);
                graphic_top.Draw(shiftedWithAltitude, north, (Thing)this);
            }

            if (isUsed)
            {
                //底部的血液
                shiftedWithAltitude = position.ToVector3ShiftedWithAltitude(AltitudeLayer.BuildingOnTop);
                graphic_blood.Draw(shiftedWithAltitude, north, (Thing)this);
                if (isUsing)
                {
                    //关上的盖子上的血液
                    shiftedWithAltitude = position.ToVector3ShiftedWithAltitude(AltitudeLayer.MoteOverhead);
                    graphic_blood_top_using.Draw(shiftedWithAltitude, north, (Thing)this);
                }
                else
                {
                    //打开的盖子上的血液
                    //shiftedWithAltitude = position.ToVector3ShiftedWithAltitude(AltitudeLayer.BuildingOnTop);
                    graphic_blood_top.Draw(shiftedWithAltitude, north, (Thing)this);
                }
            }
        }

        public void setGraphic()
        {
            string texPath = this.Graphic.path;
            var dS = this.Graphic.drawSize;
            var gph = this.Graphic.GetCopy(dS, null);
            if (this.graphic == null)
            {
                gph.path = texPath;
                this.graphic = gph.GetCopy(dS, null);
            }
            if (this.graphic_top == null)
            {
                gph.path = texPath + "_top";
                this.graphic_top = gph.GetCopy(dS, null);
            }

            if (this.graphic_top_using == null)
            {
                gph.path = texPath + "_top_using";
                this.graphic_top_using = gph.GetCopy(dS, null);
            }

            if (this.graphic_blood == null)
            {
                gph.path = texPath + "_blood";
                this.graphic_blood = gph.GetCopy(dS, null);
            }

            if (this.graphic_blood_top == null)
            {
                gph.path = texPath + "_blood_top";
                this.graphic_blood_top = gph.GetCopy(dS, null);
            }

            if (this.graphic_blood_top_using == null)
            {
                gph.path = texPath + "_blood_top_using";
                this.graphic_blood_top_using = gph.GetCopy(dS, null);
            }
        }
    }
}
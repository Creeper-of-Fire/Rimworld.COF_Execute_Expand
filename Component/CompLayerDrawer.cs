using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace COF_Torture.Component
{
    public class CompDataGraphicLayer
    {
        public GraphicData graphicData;
        public AltitudeLayer altitudeLayer;
        public float ShowSeverity;
    }
    public class CompPropertiesLayerExtension : CompProperties
    {
        public List<CompDataGraphicLayer> graphicLayers;

        public CompPropertiesLayerExtension() => this.compClass = typeof (CompLayerExtension);
    }
    public class CompLayerExtension : ThingComp
    {
        private CompPropertiesLayerExtension Props => (CompPropertiesLayerExtension) this.props;

        public override void PostDraw()
        {
            base.PostDraw();
            if (this.Props.graphicLayers == null || this.Props.graphicLayers.Count <= 0)
                return;
            foreach (CompDataGraphicLayer graphicLayer in this.Props.graphicLayers)
            {
                Vector3 loc = GenThing.TrueCenter(this.parent.Position, this.parent.Rotation, this.parent.def.size, graphicLayer.altitudeLayer.AltitudeFor());
                graphicLayer.graphicData.Graphic.Draw(loc, this.parent.Rotation, (Thing) this.parent);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace COF_Torture.Utility
{
    public static class RJWUtility
    {
        public static bool IsMissingForPawn(this BodyPartRecord self, Pawn pawn)
        {
            if (pawn == null)
                throw new ArgumentNullException(nameof(pawn));
            if (self == null)
                throw new ArgumentNullException(nameof(self));
            return pawn.health.hediffSet.hediffs
                .Where(hediff => hediff.Part == self).Any(hediff => hediff is Hediff_MissingPart);
        }

        /// <summary>
        /// 操你妈的什么乱七八糟的傻逼政治正确，有鸡巴的就是男的，就这么简单
        /// </summary>
        /// <param name="pawn">角色</param>
        public static bool IsMale(this Pawn pawn) =>
            has_penis_infertile(pawn, pawn.health.hediffSet.hediffs) ||
            has_penis_fertile(pawn, pawn.health.hediffSet.hediffs);

        /// <summary>
        /// 操你妈的什么乱七八糟的傻逼政治正确，有屄的就是女的，就这么简单
        /// </summary>
        /// <param name="pawn">角色</param>
        public static bool IsFemale(this Pawn pawn) =>
            has_vagina(pawn, pawn.health.hediffSet.hediffs);

        public static bool has_penis_infertile(Pawn pawn, List<Hediff> parts = null)
        {
            if (parts == null)
                parts = pawn.health.hediffSet.hediffs;
            return !parts.NullOrEmpty() && parts.Any(is_penis_infertile);
        }

        public static bool has_penis_fertile(Pawn pawn, List<Hediff> parts = null)
        {
            if (parts == null)
                parts = pawn.health.hediffSet.hediffs;
            return !parts.NullOrEmpty() && parts.Any(is_penis_fertile);
        }

        public static bool has_vagina(Pawn pawn, List<Hediff> parts = null)
        {
            if (parts == null)
                parts = pawn.health.hediffSet.hediffs;
            return !parts.NullOrEmpty() && parts.Any(is_vagina);
        }


        private static bool is_penis_infertile(Hediff hed) =>
            hed.def.defName.ToLower().Contains("pegdick") || hed.def.defName.ToLower().Contains("tentacle") &&
            !hed.def.defName.ToLower().Contains("penis");

        public static bool is_penis_fertile(Hediff hed) =>
            hed.def.defName.ToLower().Contains("penis") || hed.def.defName.ToLower().Contains("ovipositorm");

        public static bool is_vagina(Hediff hed) =>
            hed.def.defName.ToLower().Contains("vagina") || hed.def.defName.ToLower().Contains("ovipositorf");

        
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HYDtmn.Abstraction
{
    public static class MaterialComponentCssList
    {
        public static IEnumerable<string> GetDivWrap(int phoneSpan,int tabletSpan,int desktopSpan)
        {
            //mdc-layout-grid__cell--span-4-phone mdc-layout-grid__cell--span-8-tablet mdc-layout-grid__cell--span-12-desktop
            yield return $"mdc-layout-grid__cell--span-{phoneSpan}-phone";
            yield return $"mdc-layout-grid__cell--span-{tabletSpan}-tablet";
            yield return $"mdc-layout-grid__cell--span-{desktopSpan}-desktop";

        }

        public static IEnumerable<string> GetDivWrap(int consume)
        {
            switch(consume)
            {
                case 1:
                    return GetDivWrap(4, 2, 3);
                case 2:
                    return GetDivWrap(4, 8, 6);
                case 3:
                    return GetDivWrap(4, 6, 9);
                case 4:
                default:
                    return GetDivWrap(4, 8, 12);
            }
            
        }

        public static IEnumerable<string> GetDivWrapFull()
        {
            return GetDivWrap(4, 8, 12);
        }

        public static IEnumerable<string> GetDivWrap3Quart()
        {
            return GetDivWrap(4, 6, 9);

        }

        public static IEnumerable<string> GetDivWrap1Quart()
        {
            return GetDivWrap(4, 2, 3);
        }
        public static IEnumerable<string> GetDivWrapHalf()
        {
            return GetDivWrap(4, 8, 6);
        }

    }
}

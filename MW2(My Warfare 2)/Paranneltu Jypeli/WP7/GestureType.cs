#if WINDOWS_PHONE
using XnaGestureType = Microsoft.Xna.Framework.Input.Touch.GestureType;
#endif

namespace Jypeli.WP7
{
    /// <summary>
    /// Kosketuseleen tyyppi.
    /// </summary>
    public enum GestureType
    {
        /// <summary>
        /// Tökkäys.
        /// </summary>
        Tap
#if WINDOWS_PHONE
 = XnaGestureType.Tap
#endif
,

        /// <summary>
        /// Kaksoistökkäys, eli kaksi tökkäystä peräkkäin.
        /// </summary>
        DoubleTap
#if WINDOWS_PHONE
 = XnaGestureType.DoubleTap
#endif
,

        /// <summary>
        /// Raahaaminen vaakasuunnassa.
        /// </summary>
        HorizontalDrag
#if WINDOWS_PHONE
 = XnaGestureType.HorizontalDrag
#endif
,
        /// <summary>
        /// Raahaaminen pystysuunnassa.
        /// </summary>
        VerticalDrag
#if WINDOWS_PHONE
 = XnaGestureType.VerticalDrag
#endif
,

        /// <summary>
        /// Raahaamisen loppuminen. Tämä ei anna nopeutta,
        /// ainoastaan ilmoittaa että ei raahata enää.
        /// </summary>
        DragComplete
#if WINDOWS_PHONE
 = XnaGestureType.DragComplete
#endif
,

        /// <summary>
        /// Nopea pyyhkäisy.
        /// </summary>
        Flick
#if WINDOWS_PHONE
 = XnaGestureType.Flick
#endif
,

        /// <summary>
        /// Nipistys.
        /// </summary>
        Pinch
#if WINDOWS_PHONE
 = XnaGestureType.Pinch
#endif
,
    }
}

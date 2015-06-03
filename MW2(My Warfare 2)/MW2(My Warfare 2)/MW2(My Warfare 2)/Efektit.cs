using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;

public class Efektit
{
    public List<GameObject> Tehosteet { get; private set; }
    public Queue<PhysicsObject> Sirpaleet { get; private set; }

    public Efektit()
    {
        Tehosteet = new List<GameObject>();
        Sirpaleet = new Queue<PhysicsObject>();
    }

    public void LisaaTehosteObjekti(GameObject tehoste)
    {
        Tehosteet.Add(tehoste);
        if (Tehosteet.Count > Vakiot.TEHOSTEOBJEKTIEN_MAX_MAARA)
        {
            GameObject o = Tehosteet[0];
            Tehosteet.RemoveAt(0);
            o.Destroy();
        }
    }

    /// <summary>
    /// Muutetaan fyysinen objekti tehosteobjektiksi.
    /// </summary>
    /// <param name="muutettava"></param>
    public void MuutaTehosteObjektiksi(PhysicsObject muutettava)
    {
        if (!muutettava.IsAddedToGame) return;

        GameObject sirpale = new GameObject(muutettava.Width, muutettava.Height);
        sirpale.Image = muutettava.Image;
        sirpale.Position = muutettava.Position;
        sirpale.Angle = muutettava.Angle;
        MW2_My_Warfare_2_.Peli.Add(sirpale);
        MW2_My_Warfare_2_.Peli.Efektit.LisaaTehosteObjekti(sirpale);
        muutettava.Destroy();
    }

    /// <summary>
    /// Lisätään sirpale tietorakenteeseen ja muutetaan vanhoja GameObjecteiksi.
    /// </summary>
    /// <param name="sirpale"></param>
    public void LisaaSirpale(PhysicsObject sirpale)
    {
        Sirpaleet.Enqueue(sirpale);
        PoistaVanhaSirpale();
    }

    public void LisaaAjastettuSirpale(PhysicsObject sirpale, TimeSpan elinaika)
    {
        sirpale.MaximumLifetime = elinaika;
        sirpale.LifeTimeEnded += delegate { MuutaTehosteObjektiksi(sirpale); };

        PoistaVanhaSirpale();
    }

    /// <summary>
    /// Tyhjennetään tietorakenteet, mutta ei tuhota olioita.
    /// </summary>
    public void Tyhjenna()
    {
        Tehosteet.Clear();
        Sirpaleet.Clear();
    }

    public void PoistaEfekti(GameObject efekti)
    {
        Tehosteet.Remove(efekti);
    }

    private void PoistaVanhaSirpale()
    {
        if (Sirpaleet.Count > Vakiot.SIRPALEIDEN_MAX_MAARA)
        {
            MuutaTehosteObjektiksi(Sirpaleet.Dequeue());
        }
    }
}